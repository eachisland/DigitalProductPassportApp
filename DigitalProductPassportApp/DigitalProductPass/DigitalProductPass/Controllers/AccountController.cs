using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using DigitalProductPass.Services;
using DigitalProductPass.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalProductPass.Controllers;

public class AccountController : Controller
{
    public const string StoreOwnerRole = "StoreOwner";

    private readonly IConfiguration _config;
    private readonly UiText _t;

    public AccountController(IConfiguration config, UiText t)
    {
        _config = config;
        _t = t;
    }

    [HttpGet, AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true) return RedirectToLocal(returnUrl);
        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel());
    }

    [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel vm, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        if (!ModelState.IsValid) return View(vm);

        // Credentials come from configuration ("StoreOwner" section) so they can be changed without code edits.
        var expectedUser = _config["StoreOwner:Username"] ?? "admin";
        var expectedPass = _config["StoreOwner:Password"] ?? "DNZ2026";

        var ok = FixedTimeEquals(vm.Username.Trim(), expectedUser) & FixedTimeEquals(vm.Password, expectedPass);
        if (!ok)
        {
            await Task.Delay(400); // slows down password guessing a little
            ModelState.AddModelError("", _t["login.invalid"]);
            vm.Password = "";
            return View(vm);
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, expectedUser),
            new(ClaimTypes.Role, StoreOwnerRole)
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties { IsPersistent = vm.RememberMe });

        return RedirectToLocal(returnUrl);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    [HttpGet, AllowAnonymous]
    public IActionResult Denied() => View();

    private IActionResult RedirectToLocal(string? returnUrl) =>
        !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)
            ? LocalRedirect(returnUrl)
            : RedirectToAction("Index", "Products");

    private static bool FixedTimeEquals(string a, string b) =>
        CryptographicOperations.FixedTimeEquals(
            SHA256.HashData(Encoding.UTF8.GetBytes(a)),
            SHA256.HashData(Encoding.UTF8.GetBytes(b)));
}
