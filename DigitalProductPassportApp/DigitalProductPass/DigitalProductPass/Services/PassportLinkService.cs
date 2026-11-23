using DigitalProductPass.Models;

namespace DigitalProductPass.Services;

/// <summary>
/// Builds the public URLs that are encoded in the QR code.
/// If "PublicBaseUrl" is set (e.g. an ngrok address for phone tests) it is used, otherwise the current request origin.
/// </summary>
public class PassportLinkService
{
    private readonly IConfiguration _config;
    private readonly IHttpContextAccessor _accessor;

    public PassportLinkService(IConfiguration config, IHttpContextAccessor accessor)
    {
        _config = config;
        _accessor = accessor;
    }

    public string BaseUrl()
    {
        var configured = _config["PublicBaseUrl"];
        if (!string.IsNullOrWhiteSpace(configured)) return configured.TrimEnd('/');
        var req = _accessor.HttpContext?.Request;
        return req is null ? "" : $"{req.Scheme}://{req.Host}";
    }

    /// <summary>Plain resolver link: {base}/p/{guid}.</summary>
    public string Resolver(Guid publicId) => $"{BaseUrl()}/p/{publicId}";

    /// <summary>
    /// GS1 Digital Link (GTIN + batch + serial): {base}/01/{gtin14}[/10/{batch}]/21/{serial}.
    /// Returns null when the product has no GTIN. item.Product must be loaded.
    /// </summary>
    public string? Gs1DigitalLink(ProductItem item)
    {
        var gtin = item.Product?.Gtin;
        if (string.IsNullOrWhiteSpace(gtin)) return null;

        var link = $"{BaseUrl()}/01/{gtin}";
        if (!string.IsNullOrWhiteSpace(item.BatchNumber))
            link += $"/10/{Uri.EscapeDataString(item.BatchNumber)}";
        return link + $"/21/{Uri.EscapeDataString(item.SerialCode)}";
    }

    /// <summary>What the QR code encodes: the GS1 Digital Link when possible, else the resolver link.</summary>
    public string ShareLink(ProductItem item) => Gs1DigitalLink(item) ?? Resolver(item.PublicId);
}
