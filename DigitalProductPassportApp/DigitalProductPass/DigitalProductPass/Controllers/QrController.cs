using System.Text;
using DigitalProductPass.Data;
using DigitalProductPass.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DigitalProductPass.Controllers;

public class QrController : Controller
{
    private readonly QrCodeService _qr;
    private readonly PassportLinkService _links;
    private readonly ApplicationDbContext _db;

    public QrController(QrCodeService qr, PassportLinkService links, ApplicationDbContext db)
    {
        _qr = qr;
        _links = links;
        _db = db;
    }

    [HttpGet("qr/{publicId:guid}.{format:regex(^(png|svg)$)}")]
    public async Task<IActionResult> Get(Guid publicId, string format)
    {
        var item = await _db.ProductItems
            .AsNoTracking()
            .Include(i => i.Product)
            .FirstOrDefaultAsync(i => i.PublicId == publicId);
        if (item is null) return NotFound();

        // GS1 Digital Link when the product has a GTIN, otherwise the plain /p/{guid} link.
        var url = _links.ShareLink(item);
        Response.Headers.CacheControl = "public,max-age=3600";

        return format == "svg"
            ? File(Encoding.UTF8.GetBytes(_qr.Svg(url)), "image/svg+xml")
            : File(_qr.Png(url), "image/png");
    }
}
