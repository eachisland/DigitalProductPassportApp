using System.Text;
using DigitalProductPass.Services;
using Microsoft.AspNetCore.Mvc;

namespace DigitalProductPass.Controllers;

public class QrController : Controller
{
    private readonly QrCodeService _qr;
    private readonly PassportLinkService _links;

    public QrController(QrCodeService qr, PassportLinkService links)
    {
        _qr = qr;
        _links = links;
    }

    [HttpGet("qr/{publicId:guid}.{format:regex(^(png|svg)$)}")]
    public IActionResult Get(Guid publicId, string format)
    {
        var url = _links.For(publicId);
        Response.Headers.CacheControl = "public,max-age=86400";

        return format == "svg"
            ? File(Encoding.UTF8.GetBytes(_qr.Svg(url)), "image/svg+xml")
            : File(_qr.Png(url), "image/png");
    }
}
