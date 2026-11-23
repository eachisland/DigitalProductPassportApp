using System.Data.Common;
using System.Net;
using System.Text.RegularExpressions;
using DigitalProductPass.Controllers;
using DigitalProductPass.Data;
using DigitalProductPass.Models;
using DigitalProductPass.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DigitalProductPass.Tests;

public class PassportLookupTests
{
    private const string ModernId = "c3df1989-6dcc-4730-84be-104bc9d3652f";
    private const string NoBatchId = "9bc89f25-741e-4d0b-8a81-819ac1d35734";
    private const string LegacyId = "072790c3-0962-4789-a681-7126d531f8b7";
    private const string ModernSerial = "TSHORG-WHI-M-7K3M9";
    private const string NoBatchSerial = "TSHORG-WHI-L-W9DP7";
    private const string LegacySerial = "DPP-7K3M-9QXA";
    private const string Gtin = "05901234123457";
    private const string Batch = "Lot-05";
    private const string BatchedPath = $"/01/{Gtin}/10/{Batch}/21/{ModernSerial}";
    private const string UnbatchedPath = $"/01/{Gtin}/21/{NoBatchSerial}";
    private const string LegacyPath = $"/p/{LegacyId}";
    private const string WrongGtinPath = $"/01/09506000134352/10/{Batch}/21/{ModernSerial}";
    private const string WrongBatchPath = $"/01/{Gtin}/10/Lot-06/21/{ModernSerial}";
    private const string CodeNotFound = "We couldn't find a product with that code.";

    private readonly TestApplication _app = new();
    private HttpClient _client = null!;

    [OneTimeSetUp]
    public async Task SeedGarments()
    {
        using var scope = _app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.EnsureCreatedAsync();

        var brand = new Brand { Name = "Test Textile" };
        db.Products.AddRange(
            new Product
            {
                Brand = brand,
                Name = "Organic Cotton T-Shirt",
                Sku = "TSH-ORG-001",
                Gtin = Gtin,
                Items =
                {
                    new ProductItem { PublicId = Guid.Parse(ModernId), SerialCode = ModernSerial, BatchNumber = Batch },
                    new ProductItem { PublicId = Guid.Parse(NoBatchId), SerialCode = NoBatchSerial }
                }
            },
            new Product
            {
                Brand = brand,
                Name = "Legacy Shirt",
                Sku = "LEGACY-SHIRT",
                Items = { new ProductItem { PublicId = Guid.Parse(LegacyId), SerialCode = LegacySerial } }
            });
        await db.SaveChangesAsync();
    }

    [SetUp]
    public void OpenClient()
    {
        _client = _app.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("https://localhost")
        });
    }

    [TearDown]
    public void CloseClient() => _client.Dispose();

    [OneTimeTearDown]
    public async Task CloseApplication() => await _app.DisposeAsync();

    [TestCase("  tshorg-whi-m-7k3m9  ", ModernId, ModernSerial)]
    [TestCase("tshorg\u2011whi\u2013l\u2212w9dp7", NoBatchId, NoBatchSerial)]
    [TestCase("dpp-7k3m-9qxa", LegacyId, LegacySerial)]
    [TestCase($"https://labels.example{BatchedPath}", ModernId, ModernSerial)]
    public async Task ManualFormOpensTheMatchingPassport(string input, string publicId, string expectedSerial)
    {
        var page = await _client.GetStringAsync("/scan");
        var form = Regex.Match(page, """<form\b[^>]*\bmethod="post"[^>]*\baction="([^"]+)"[^>]*>""");
        var token = Regex.Match(page, @"name=""__RequestVerificationToken""[^>]*value=""([^""]+)""");
        Assert.That(form.Groups[1].Value, Is.EqualTo("/scan"));
        Assert.That(token.Success, Is.True);

        using var fields = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["code"] = input,
            ["__RequestVerificationToken"] = token.Groups[1].Value
        });
        using var response = await _client.PostAsync(form.Groups[1].Value, fields);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Redirect));
        Assert.That(response.Headers.Location?.OriginalString, Is.EqualTo($"/p/{publicId}"));

        var passport = await _client.GetStringAsync(response.Headers.Location!);
        Assert.That(passport, Does.Contain(expectedSerial));
    }

    [TestCase(ModernId, BatchedPath)]
    [TestCase(NoBatchId, UnbatchedPath)]
    [TestCase(LegacyId, LegacyPath)]
    public async Task IssuedQrImageAndItsLinkOpenTheMatchingPassport(string publicId, string path)
    {
        var payload = $"https://localhost{path}";
        using var image = await _client.GetAsync($"/qr/{publicId}.png");
        Assert.That(image.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(image.Content.Headers.ContentType?.MediaType, Is.EqualTo("image/png"));
        var bytes = await image.Content.ReadAsByteArrayAsync();
        Assert.That(bytes, Is.EqualTo(new QrCodeService().Png(payload)));

        using var scan = await Resolve(payload);
        Assert.That(scan.StatusCode, Is.EqualTo(HttpStatusCode.Redirect));
        Assert.That(scan.Headers.Location?.OriginalString, Is.EqualTo($"/p/{publicId}"));

        using var direct = await _client.GetAsync(path);
        Assert.That(direct.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [TestCase(ModernId, ModernId)]
    [TestCase(ModernSerial, ModernId)]
    [TestCase($"{LegacyPath}/?ref=label#qr", LegacyId)]
    [TestCase($"https://labels.example/shop{BatchedPath}#label", ModernId)]
    [TestCase($"/01/{Gtin}/10/Lot%2D05/21/tshorg%2Dwhi-m-7k3m9", ModernId)]
    public async Task ScannerAcceptsSupportedIdentifierFormats(string input, string publicId)
    {
        using var response = await Resolve(input);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Redirect));
        Assert.That(response.Headers.Location?.OriginalString, Is.EqualTo($"/p/{publicId}"));
    }

    [TestCase(" ", "Enter or scan a code.")]
    [TestCase("TSH-ORG-001", CodeNotFound)]
    [TestCase("TSHORG-WHI-M", CodeNotFound)]
    [TestCase("/p/00000000-0000-0000-0000-000000000001", CodeNotFound)]
    [TestCase($"https://labels.example/orders/{ModernId}", CodeNotFound)]
    [TestCase($"https://labels.example/product?next={BatchedPath}", CodeNotFound)]
    [TestCase($"{BatchedPath}/extra", CodeNotFound)]
    [TestCase(WrongGtinPath, CodeNotFound)]
    [TestCase(WrongBatchPath, CodeNotFound)]
    public async Task ScannerReturnsToTheFormWithAnError(string input, string expectedMessage)
    {
        using var response = await Resolve(input);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Redirect));
        Assert.That(response.Headers.Location?.OriginalString, Is.EqualTo("/scan"));
        var page = await _client.GetStringAsync(response.Headers.Location!);
        Assert.That(WebUtility.HtmlDecode(page), Does.Contain(expectedMessage));
    }

    [TestCase(WrongGtinPath)]
    [TestCase(WrongBatchPath)]
    public async Task OpeningMismatchedGs1LinksDirectlyReturnsNotFound(string path)
    {
        using var response = await _client.GetAsync(path);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    private Task<HttpResponseMessage> Resolve(string input) =>
        _client.GetAsync($"/scan/resolve?code={Uri.EscapeDataString(input)}");

    private sealed class TestApplication : WebApplicationFactory<ScanController>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("PublicBaseUrl", "https://localhost");
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IDbContextOptionsConfiguration<ApplicationDbContext>>();
                services.AddSingleton<DbConnection>(_ =>
                {
                    var connection = new SqliteConnection("Data Source=:memory:");
                    connection.Open();
                    return connection;
                });
                services.AddDbContext<ApplicationDbContext>((provider, options) =>
                    options.UseSqlite(provider.GetRequiredService<DbConnection>()));
            });
        }
    }
}
