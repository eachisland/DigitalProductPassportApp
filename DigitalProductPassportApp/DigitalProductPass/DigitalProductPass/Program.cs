using DigitalProductPass.Data;
using DigitalProductPass.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sql => sql.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(5), errorNumbersToAdd: null)));

// Store owner login: cookie authentication, one role ("StoreOwner").
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/Denied";
        options.Cookie.Name = "dpp.auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });
builder.Services.AddAuthorization();

builder.Services.AddSingleton<QrCodeService>();
builder.Services.AddScoped<PassportLinkService>();
builder.Services.AddScoped<UiText>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Development only: create/upgrade the database and add demo data.
// In Production run `dotnet ef database update` as a separate deployment step.
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    if (db.Database.GetMigrations().Any())
        db.Database.Migrate();
    else
        db.Database.EnsureCreated(); // while no migration exists yet
    DbSeeder.Seed(db);
}

app.UseHttpsRedirection();

// Numbers in forms (<input type="number">) always use "." as decimal separator. Pin the request culture so a
// server running with a tr-TR / de-DE culture doesn't misread "0.35" as 35. The UI language is handled by UiText.
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("en-US"),
    SupportedCultures = new[] { new System.Globalization.CultureInfo("en-US") },
    SupportedUICultures = new[] { new System.Globalization.CultureInfo("en-US") },
    RequestCultureProviders = new List<IRequestCultureProvider>()
});

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
