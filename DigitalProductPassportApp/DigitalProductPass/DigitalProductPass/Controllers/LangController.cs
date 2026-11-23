using DigitalProductPass.Services;
using Microsoft.AspNetCore.Mvc;

namespace DigitalProductPass.Controllers;

public class LangController : Controller
{
    [HttpGet("lang/{culture}")]
    public IActionResult Set(string culture, string? returnUrl = null)
    {
        if (UiText.Supported.Contains(culture))
        {
            Response.Cookies.Append(UiText.CookieName, culture, new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                IsEssential = true,
                SameSite = SameSiteMode.Lax,
                HttpOnly = true
            });
        }
        return !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)
            ? LocalRedirect(returnUrl)
            : RedirectToAction("Index", "Home");
    }
}
