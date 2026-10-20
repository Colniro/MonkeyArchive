using Microsoft.AspNetCore.Mvc;
using MonkeyArchive.Models;
using MySqlConnector;

namespace MonkeyArchive.Controllers;

// Login, Registrierung und die Favorites Liste.
// Accounts liegen jetzt in der User Tabelle in der Datenbank, nicht mehr im Speicher.
// Die Session speichert nur den Username vom eingeloggten Nutzer als Text.
public class AccountController : Controller
{
    private readonly string _connString;

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

    public AccountController(IConfiguration config)
    {
        _connString = config.GetConnectionString("Default")!;
    }

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
        using var conn = new MySqlConnection(_connString);
        conn.Open();

        using var cmd = new MySqlCommand("SELECT Password FROM User WHERE Username = @username", conn);
        cmd.Parameters.AddWithValue("@username", username);
        var storedPassword = cmd.ExecuteScalar() as string;

        if (storedPassword is not null && storedPassword == password)
        {
            HttpContext.Session.SetString(SessionKey, username);
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

        using var conn = new MySqlConnection(_connString);
        conn.Open();

        using (var checkCmd = new MySqlCommand("SELECT COUNT(*) FROM User WHERE Username = @username", conn))
        {
            checkCmd.Parameters.AddWithValue("@username", username);
            if (Convert.ToInt32(checkCmd.ExecuteScalar()) > 0)
            {
                ViewData["Error"] = "An account with this username already exists.";
                return View();
            }
        }

        using (var insertCmd = new MySqlCommand(
            "INSERT INTO User (Username, Password, SecurityQuestion, Answer) VALUES (@username, @password, @question, @answer)", conn))
        {
            insertCmd.Parameters.AddWithValue("@username", username);
            insertCmd.Parameters.AddWithValue("@password", password);
            insertCmd.Parameters.AddWithValue("@question", securityQuestion);
            insertCmd.Parameters.AddWithValue("@answer", securityAnswer);
            insertCmd.ExecuteNonQuery();
        }

        HttpContext.Session.SetString(SessionKey, username);
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
        using var conn = new MySqlConnection(_connString);
        conn.Open();

        using var cmd = new MySqlCommand("SELECT COUNT(*) FROM User WHERE Username = @username", conn);
        cmd.Parameters.AddWithValue("@username", username);

        if (Convert.ToInt32(cmd.ExecuteScalar()) == 0)
        {
            ViewData["Error"] = "No account found with that username.";
            return View();
        }

        return RedirectToAction("ResetPassword", new { username });
    }

    [HttpGet]
    public IActionResult ResetPassword(string username)
    {
        using var conn = new MySqlConnection(_connString);
        conn.Open();

        using var cmd = new MySqlCommand("SELECT SecurityQuestion FROM User WHERE Username = @username", conn);
        cmd.Parameters.AddWithValue("@username", username);
        var question = cmd.ExecuteScalar() as string;

        if (question is null)
        {
            return RedirectToAction("ForgotPassword");
        }

        ViewData["Username"] = username;
        ViewData["SecurityQuestion"] = question;
        return View();
    }

    [HttpPost]
    public IActionResult ResetPassword(string username, string securityAnswer, string password, string confirmPassword)
    {
        using var conn = new MySqlConnection(_connString);
        conn.Open();

        string? question = null;
        string? storedAnswer = null;

        using (var cmd = new MySqlCommand("SELECT SecurityQuestion, Answer FROM User WHERE Username = @username", conn))
        {
            cmd.Parameters.AddWithValue("@username", username);
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                question = reader.GetString(0);
                storedAnswer = reader.GetString(1);
            }
        }

        if (question is null)
        {
            return RedirectToAction("ForgotPassword");
        }

        ViewData["Username"] = username;
        ViewData["SecurityQuestion"] = question;

        if (!string.Equals(storedAnswer!.Trim(), securityAnswer.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            ViewData["Error"] = "That answer doesn't match.";
            return View();
        }

        if (string.IsNullOrWhiteSpace(password) || password != confirmPassword)
        {
            ViewData["Error"] = "Passwords don't match.";
            return View();
        }

        using (var updateCmd = new MySqlCommand("UPDATE User SET Password = @password WHERE Username = @username", conn))
        {
            updateCmd.Parameters.AddWithValue("@password", password);
            updateCmd.Parameters.AddWithValue("@username", username);
            updateCmd.ExecuteNonQuery();
        }

        TempData["Message"] = "Password reset. Log in with your new password.";
        return RedirectToAction("Login");
    }

    public IActionResult Favorites()
    {
        var username = HttpContext.Session.GetString(SessionKey);
        if (username is null)
        {
            return RedirectToAction("Login", new { returnUrl = Url.Action("Favorites") });
        }

        using var conn = new MySqlConnection(_connString);
        conn.Open();

        using (var checkCmd = new MySqlCommand("SELECT COUNT(*) FROM User WHERE Username = @username", conn))
        {
            checkCmd.Parameters.AddWithValue("@username", username);
            if (Convert.ToInt32(checkCmd.ExecuteScalar()) == 0)
            {
                return RedirectToAction("Login", new { returnUrl = Url.Action("Favorites") });
            }
        }

        // Nur die Felder, die die Kartenansicht braucht (Name/Image/Category fürs Hexagon).
        // Beschreibung, Preis usw. werden hier nicht gebraucht, bleiben einfach auf ihrem Default.
        var favorites = new List<Tower>();
        using (var cmd = new MySqlCommand(
            @"SELECT t.Slug, t.Name, t.ImageFileName, tc.Name AS CategoryName
              FROM FavouriteTowers f
              JOIN Tower t ON f.TowerID = t.ID
              JOIN TowerCategory tc ON t.CategoryId = tc.ID
              JOIN User u ON f.UserID = u.ID
              WHERE u.Username = @username
              ORDER BY t.ID", conn))
        {
            cmd.Parameters.AddWithValue("@username", username);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                favorites.Add(new Tower
                {
                    Slug = reader.GetString(0),
                    Name = reader.GetString(1),
                    Image = reader.GetString(2),
                    Category = reader.GetString(3)
                });
            }
        }

        return View(favorites);
    }
}
