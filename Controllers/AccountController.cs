using Microsoft.AspNetCore.Mvc;
using MonkeyArchive.Models;

namespace MonkeyArchive.Controllers;

// Login, Registrierung und die Favorites Liste, die aktuell immer leer ist.
// Accounts liegen einfach in einem Dictionary im Speicher, per Username als Key. Es gibt keine Datenbank.
// Die Session speichert nur den Username vom eingeloggten Nutzer als Text.
public class AccountController : Controller
{
    private static readonly Dictionary<string, User> Users = new(StringComparer.OrdinalIgnoreCase);

    private const string SessionKey = "Username";

    // Wird bei Register als Dropdown angeboten. ResetPassword zeigt einfach die Frage, die der Account schon hat.
    public static readonly string[] SecurityQuestions =
    [
        "What was your first pet's name?",
        "What city were you born in?",
        "What was the name of your first school?",
        "What's your mother's maiden name?",
        "What was your childhood nickname?"
    ];

    [HttpGet]
    public IActionResult Login(string? returnUrl)
    {
        if (HttpContext.Session.GetString(SessionKey) is not null)
        {
            return RedirectToAction("Index", "Home");
        }

        ViewData["ReturnUrl"] = returnUrl;
        ViewData["Message"] = TempData["Message"];
        return View();
    }

    [HttpPost]
    public IActionResult Login(string username, string password, string? returnUrl)
    {
        if (Users.TryGetValue(username, out var user) && user.Password == password)
        {
            HttpContext.Session.SetString(SessionKey, user.Username);
            return Url.IsLocalUrl(returnUrl) ? Redirect(returnUrl!) : RedirectToAction("Index", "Home");
        }

        ViewData["Error"] = "Wrong username or password.";
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

        ViewData["SecurityQuestions"] = SecurityQuestions;
        return View();
    }

    [HttpPost]
    public IActionResult Register(string username, string password, string confirmPassword, string securityQuestion, string securityAnswer)
    {
        ViewData["SecurityQuestions"] = SecurityQuestions;

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(securityAnswer))
        {
            ViewData["Error"] = "Please fill in all fields.";
            return View();
        }

        if (password != confirmPassword)
        {
            ViewData["Error"] = "Passwords don't match.";
            return View();
        }

        if (Users.ContainsKey(username))
        {
            ViewData["Error"] = "An account with this username already exists.";
            return View();
        }

        var user = new User { Username = username, Password = password, SecurityQuestion = securityQuestion, SecurityAnswer = securityAnswer };
        Users[username] = user;
        HttpContext.Session.SetString(SessionKey, user.Username);

        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    public IActionResult Logout()
    {
        HttpContext.Session.Remove(SessionKey);
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult ForgotPassword()
    {
        return View();
    }

    [HttpPost]
    public IActionResult ForgotPassword(string username)
    {
        if (!Users.ContainsKey(username))
        {
            ViewData["Error"] = "No account found with that username.";
            return View();
        }

        return RedirectToAction("ResetPassword", new { username });
    }

    [HttpGet]
    public IActionResult ResetPassword(string username)
    {
        if (!Users.TryGetValue(username, out var user))
        {
            return RedirectToAction("ForgotPassword");
        }

        ViewData["Username"] = username;
        ViewData["SecurityQuestion"] = user.SecurityQuestion;
        return View();
    }

    [HttpPost]
    public IActionResult ResetPassword(string username, string securityAnswer, string password, string confirmPassword)
    {
        if (!Users.TryGetValue(username, out var user))
        {
            return RedirectToAction("ForgotPassword");
        }

        ViewData["Username"] = username;
        ViewData["SecurityQuestion"] = user.SecurityQuestion;

        if (!string.Equals(user.SecurityAnswer.Trim(), securityAnswer.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            ViewData["Error"] = "That answer doesn't match.";
            return View();
        }

        if (string.IsNullOrWhiteSpace(password) || password != confirmPassword)
        {
            ViewData["Error"] = "Passwords don't match.";
            return View();
        }

        user.Password = password;
        TempData["Message"] = "Password reset. Log in with your new password.";
        return RedirectToAction("Login");
    }

    public IActionResult Favorites()
    {
        var username = HttpContext.Session.GetString(SessionKey);
        if (username is null || !Users.TryGetValue(username, out var user))
        {
            return RedirectToAction("Login", new { returnUrl = Url.Action("Favorites") });
        }

        return View(user.FavoriteTowerSlugs);
    }
}
