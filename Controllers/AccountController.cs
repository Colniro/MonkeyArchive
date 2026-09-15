using Microsoft.AspNetCore.Mvc;
using MonkeyArchive.Models;

namespace MonkeyArchive.Controllers;

// Login, registration and the (currently always-empty) Favorites list.
// Accounts are a plain in-memory dictionary keyed by e-mail rather than a
// database; the session just stores the logged-in user's e-mail as a string.
public class AccountController : Controller
{
    private static readonly Dictionary<string, User> Users = new(StringComparer.OrdinalIgnoreCase);

    private const string SessionKey = "UserEmail";

    [HttpGet]
    public IActionResult Login(string? returnUrl)
    {
        if (HttpContext.Session.GetString(SessionKey) is not null)
        {
            return RedirectToAction("Index", "Home");
        }

        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    public IActionResult Login(string email, string password, string? returnUrl)
    {
        if (Users.TryGetValue(email, out var user) && user.Password == password)
        {
            HttpContext.Session.SetString(SessionKey, user.Email);
            return Url.IsLocalUrl(returnUrl) ? Redirect(returnUrl!) : RedirectToAction("Index", "Home");
        }

        ViewData["Error"] = "Wrong e-mail or password.";
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpGet]
    public IActionResult Register()
    {
        if (HttpContext.Session.GetString(SessionKey) is not null)
        {
            return RedirectToAction("Index", "Home");
        }

        return View();
    }

    [HttpPost]
    public IActionResult Register(string email, string password, string confirmPassword)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            ViewData["Error"] = "Please fill in all fields.";
            return View();
        }

        if (password != confirmPassword)
        {
            ViewData["Error"] = "Passwords don't match.";
            return View();
        }

        if (Users.ContainsKey(email))
        {
            ViewData["Error"] = "An account with this e-mail already exists.";
            return View();
        }

        var user = new User { Email = email, Password = password };
        Users[email] = user;
        HttpContext.Session.SetString(SessionKey, user.Email);

        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    public IActionResult Logout()
    {
        HttpContext.Session.Remove(SessionKey);
        return RedirectToAction("Index", "Home");
    }

    public IActionResult Favorites()
    {
        var email = HttpContext.Session.GetString(SessionKey);
        if (email is null || !Users.TryGetValue(email, out var user))
        {
            return RedirectToAction("Login", new { returnUrl = Url.Action("Favorites") });
        }

        return View(user.FavoriteTowerSlugs);
    }
}
