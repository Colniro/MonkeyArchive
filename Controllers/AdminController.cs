using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using MonkeyArchive.Helpers;
using MonkeyArchive.Models;
using MySqlConnector;

namespace MonkeyArchive.Controllers;

// Der Admin Bereich unter /Admin. Hier kann man alle Accounts sehen und
// bearbeiten, die Inhalte (Towers, Bloons) direkt in der Datenbank ändern und
// mit einer kleinen Nur Lese SQL Konsole in die Tabellen schauen.
//
// Zugang: nur der Account mit dem Username "admin". Es gibt in diesem Projekt
// absichtlich kein Rollen System mit eigener Spalte, weil die User Tabelle
// sonst eine Migration bräuchte. Der eine Name reicht und steht nur hier als
// Konstante, damit man ihn an einer Stelle ändern kann.
//
// AutoValidateAntiforgeryToken prüft bei jedem POST hier das versteckte Token,
// das Razor in jedes Formular legt. Ohne das könnte eine fremde Seite dem
// eingeloggten Admin ein Delete unterschieben (CSRF). Im restlichen Projekt ist
// das nicht nötig, hier hängen aber Löschaktionen dran.
[AutoValidateAntiforgeryToken]
public class AdminController : Controller
{
    private readonly string _connString;

    private const string SessionKey = "Username";

    // Der einzige Username, der auf /Admin kommt.
    public const string AdminUsername = "admin";

    // Obergrenze für die SQL Konsole. Verhindert, dass ein SELECT ohne WHERE
    // die ganze Seite mit Zeilen zumüllt.
    private const int QueryRowLimit = 200;

    // Was die SQL Konsole erlaubt. Alles, was Daten verändert (INSERT, UPDATE,
    // DELETE, DROP, ...), geht nur über die richtigen Formulare. Da gibt es
    // Prüfungen, in der Konsole gäbe es keine.
    private static readonly string[] AllowedQueryVerbs = ["SELECT", "SHOW", "DESCRIBE", "DESC", "EXPLAIN", "WITH"];

    public AdminController(IConfiguration config)
    {
        _connString = config.GetConnectionString("Default")!;
    }

    // Prüft, ob der eingeloggte Nutzer der Admin ist. Wird auch von
    // _Navigation.cshtml benutzt, damit der Menüpunkt nur dem Admin angezeigt
    // wird. Darum static und public.
    public static bool IsAdmin(HttpContext context)
    {
        var username = context.Session.GetString(SessionKey);
        return username is not null && string.Equals(username, AdminUsername, StringComparison.OrdinalIgnoreCase);
    }

    // Die Zugangskontrolle für den ganzen Controller an einer Stelle. Läuft vor
    // jeder Action, so kann keine Action vergessen werden.
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (HttpContext.Session.GetString(SessionKey) is null)
        {
            // Nicht eingeloggt: ganz normal zum Login und danach zurück hierher.
            context.Result = RedirectToAction("Login", "Account", new { returnUrl = Request.Path.ToString() });
            return;
        }

        if (!IsAdmin(HttpContext))
        {
            // Eingeloggt, aber kein Admin: absichtlich 404 statt "kein Zugriff".
            // So verrät die Seite nicht, dass es den Bereich überhaupt gibt.
            context.Result = NotFound();
            return;
        }

        base.OnActionExecuting(context);
    }

    // Dashboard ------------------------------------------------------------

    public IActionResult Index()
    {
        using var conn = new MySqlConnection(_connString);
        conn.Open();

        var model = new AdminDashboardViewModel
        {
            ServerVersion = conn.ServerVersion,
            DatabaseName = conn.Database
        };

        // Eine Abfrage für alle Zeilenzahlen statt einer pro Tabelle.
        // Die Namen sind fest, es sind genau die Tabellen aus CreateDB.sql.
        using (var cmd = new MySqlCommand(
            @"SELECT 'User' AS TableName, COUNT(*) AS RowCnt FROM User
              UNION ALL SELECT 'FavouriteTowers', COUNT(*) FROM FavouriteTowers
              UNION ALL SELECT 'Tower', COUNT(*) FROM Tower
              UNION ALL SELECT 'TowerCategory', COUNT(*) FROM TowerCategory
              UNION ALL SELECT 'TowerUpgradePaths', COUNT(*) FROM TowerUpgradePaths
              UNION ALL SELECT 'PathLabel', COUNT(*) FROM PathLabel
              UNION ALL SELECT 'Bloon', COUNT(*) FROM Bloon
              UNION ALL SELECT 'BloonCategory', COUNT(*) FROM BloonCategory
              UNION ALL SELECT 'BloonImmunities', COUNT(*) FROM BloonImmunities
              UNION ALL SELECT 'BloonPopsInto', COUNT(*) FROM BloonPopsInto
              UNION ALL SELECT 'Immunity', COUNT(*) FROM Immunity", conn))
        using (var reader = cmd.ExecuteReader())
        {
            while (reader.Read())
            {
                model.Tables.Add(new AdminTableInfo
                {
                    Name = reader.GetString(0),
                    Rows = reader.GetInt32(1)
                });
            }
        }

        // Die vier Zahlen für die grossen Kacheln kommen aus derselben Liste,
        // damit es keine zweite Runde zur DB braucht.
        model.UserCount = RowsOf(model, "User");
        model.FavoriteCount = RowsOf(model, "FavouriteTowers");
        model.TowerCount = RowsOf(model, "Tower");
        model.BloonCount = RowsOf(model, "Bloon");

        using (var cmd = new MySqlCommand(
            @"SELECT t.Name, t.Slug, t.ImageFileName, tc.Name AS CategoryName, COUNT(*) AS Cnt
              FROM FavouriteTowers f
              JOIN Tower t ON f.TowerID = t.ID
              JOIN TowerCategory tc ON t.CategoryId = tc.ID
              GROUP BY t.ID, t.Name, t.Slug, t.ImageFileName, tc.Name
              ORDER BY Cnt DESC, t.Name
              LIMIT 5", conn))
        using (var reader = cmd.ExecuteReader())
        {
            while (reader.Read())
            {
                model.TopFavorites.Add(new AdminFavoriteStat
                {
                    Name = reader.GetString(0),
                    Slug = reader.GetString(1),
                    Image = reader.GetString(2),
                    Category = reader.GetString(3),
                    Count = reader.GetInt32(4)
                });
            }
        }

        using (var cmd = new MySqlCommand(
            "SELECT ID, Username, SecurityQuestion FROM User ORDER BY ID DESC LIMIT 5", conn))
        using (var reader = cmd.ExecuteReader())
        {
            while (reader.Read())
            {
                var username = reader.GetString(1);
                model.NewestUsers.Add(new AdminUser
                {
                    Id = reader.GetInt32(0),
                    Username = username,
                    SecurityQuestion = reader.GetString(2),
                    IsAdmin = string.Equals(username, AdminUsername, StringComparison.OrdinalIgnoreCase)
                });
            }
        }

        ViewData["Message"] = TempData["Message"];
        return View(model);
    }

    private static int RowsOf(AdminDashboardViewModel model, string table) =>
        model.Tables.FirstOrDefault(t => t.Name == table)?.Rows ?? 0;

    // Accounts -------------------------------------------------------------

    // Liste aller Accounts. Das Suchfeld läuft wie bei Towers und Bloons über
    // den Query String, darum braucht es hier kein JavaScript.
    public IActionResult Users(string? search)
    {
        var users = new List<AdminUser>();

        using var conn = new MySqlConnection(_connString);
        conn.Open();

        var sql = @"SELECT u.ID, u.Username, u.SecurityQuestion, COUNT(f.TowerID) AS FavoriteCount
                    FROM User u
                    LEFT JOIN FavouriteTowers f ON f.UserID = u.ID";

        if (!string.IsNullOrWhiteSpace(search))
        {
            sql += " WHERE u.Username LIKE @search";
        }

        sql += @" GROUP BY u.ID, u.Username, u.SecurityQuestion
                  ORDER BY u.ID";

        using (var cmd = new MySqlCommand(sql, conn))
        {
            if (!string.IsNullOrWhiteSpace(search))
            {
                cmd.Parameters.AddWithValue("@search", $"%{search}%");
            }

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var username = reader.GetString(1);
                users.Add(new AdminUser
                {
                    Id = reader.GetInt32(0),
                    Username = username,
                    SecurityQuestion = reader.GetString(2),
                    FavoriteCount = reader.GetInt32(3),
                    IsAdmin = string.Equals(username, AdminUsername, StringComparison.OrdinalIgnoreCase)
                });
            }
        }

        ViewData["Search"] = search ?? "";
        ViewData["Message"] = TempData["Message"];
        ViewData["Error"] = TempData["Error"];
        return View(users);
    }

    [HttpGet]
    public IActionResult EditUser(int id)
    {
        var user = LoadUser(id);
        if (user is null)
        {
            return NotFound();
        }

        ViewData["SecurityQuestions"] = AccountController.SecurityQuestions;
        return View(user);
    }

    [HttpPost]
    public IActionResult EditUser(int id, string username, string password, string securityQuestion, string answer)
    {
        ViewData["SecurityQuestions"] = AccountController.SecurityQuestions;

        var user = LoadUser(id);
        if (user is null)
        {
            return NotFound();
        }

        var oldQuestion = user.SecurityQuestion;

        // Die eingegebenen Werte zurück ins Model, damit das Formular nach einem
        // Fehler nicht wieder die alten Werte aus der DB zeigt. Passwort und
        // Antwort gibt es dort nicht, die stehen nur als Hash in der DB.
        user.Username = username ?? "";
        user.SecurityQuestion = securityQuestion ?? "";

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(securityQuestion))
        {
            ViewData["Error"] = "Please fill in the username and the security question.";
            return View(user);
        }

        // Eine neue Frage ohne neue Antwort würde nicht mehr zur alten Antwort passen.
        if (securityQuestion != oldQuestion && string.IsNullOrWhiteSpace(answer))
        {
            ViewData["Error"] = "Please enter an answer for the new security question.";
            return View(user);
        }

        // Der Admin darf sich nicht selbst umbenennen. Sonst passt der Name in
        // der Session nicht mehr zu AdminUsername und der Bereich wäre für
        // niemanden mehr erreichbar.
        if (user.IsAdmin && !string.Equals(username, AdminUsername, StringComparison.OrdinalIgnoreCase))
        {
            ViewData["Error"] = $"The \"{AdminUsername}\" account can't be renamed, otherwise nobody could reach this page anymore.";
            user.Username = AdminUsername;
            return View(user);
        }

        using var conn = new MySqlConnection(_connString);
        conn.Open();

        // Username ist UNIQUE in der DB. Ohne diese Prüfung käme eine
        // MySqlException statt einer lesbaren Meldung.
        using (var checkCmd = new MySqlCommand("SELECT COUNT(*) FROM User WHERE Username = @username AND ID <> @id", conn))
        {
            checkCmd.Parameters.AddWithValue("@username", username);
            checkCmd.Parameters.AddWithValue("@id", id);
            if (Convert.ToInt32(checkCmd.ExecuteScalar()) > 0)
            {
                ViewData["Error"] = "Another account already uses this username.";
                return View(user);
            }
        }

        // Leeres Passwort oder leere Antwort heisst "nicht ändern". Dann kommt
        // NULL als Parameter und COALESCE behält den alten Wert aus der DB.
        using (var cmd = new MySqlCommand(
            @"UPDATE User
              SET Username = @username,
                  Password = COALESCE(@password, Password),
                  SecurityQuestion = @question,
                  Answer = COALESCE(@answer, Answer)
              WHERE ID = @id", conn))
        {
            cmd.Parameters.AddWithValue("@username", username);
            cmd.Parameters.AddWithValue("@password", string.IsNullOrWhiteSpace(password) ? DBNull.Value : PasswordHelper.Hash(password));
            cmd.Parameters.AddWithValue("@question", securityQuestion);
            cmd.Parameters.AddWithValue("@answer", string.IsNullOrWhiteSpace(answer) ? DBNull.Value : PasswordHelper.HashAnswer(answer));
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }

        TempData["Message"] = $"Account \"{username}\" saved.";
        return RedirectToAction(nameof(Users));
    }

    [HttpGet]
    public IActionResult CreateUser()
    {
        ViewData["SecurityQuestions"] = AccountController.SecurityQuestions;
        return View();
    }

    [HttpPost]
    public IActionResult CreateUser(string username, string password, string securityQuestion, string answer)
    {
        ViewData["SecurityQuestions"] = AccountController.SecurityQuestions;
        ViewData["Username"] = username;
        ViewData["SecurityQuestion"] = securityQuestion;
        ViewData["Answer"] = answer;

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password)
            || string.IsNullOrWhiteSpace(securityQuestion) || string.IsNullOrWhiteSpace(answer))
        {
            ViewData["Error"] = "Please fill in all fields.";
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

        using (var cmd = new MySqlCommand(
            "INSERT INTO User (Username, Password, SecurityQuestion, Answer) VALUES (@username, @password, @question, @answer)", conn))
        {
            cmd.Parameters.AddWithValue("@username", username);
            cmd.Parameters.AddWithValue("@password", PasswordHelper.Hash(password));
            cmd.Parameters.AddWithValue("@question", securityQuestion);
            cmd.Parameters.AddWithValue("@answer", PasswordHelper.HashAnswer(answer));
            cmd.ExecuteNonQuery();
        }

        TempData["Message"] = $"Account \"{username}\" created.";
        return RedirectToAction(nameof(Users));
    }

    // Löscht einen Account samt seinen Favoriten.
    [HttpPost]
    public IActionResult DeleteUser(int id)
    {
        var user = LoadUser(id);
        if (user is null)
        {
            return NotFound();
        }

        if (user.IsAdmin)
        {
            TempData["Error"] = $"The \"{AdminUsername}\" account can't be deleted.";
            return RedirectToAction(nameof(Users));
        }

        using var conn = new MySqlConnection(_connString);
        conn.Open();

        // FavouriteTowers zeigt per Foreign Key auf User. Die Zeilen müssen
        // zuerst weg, sonst lehnt MySQL das DELETE ab.
        using (var cmd = new MySqlCommand("DELETE FROM FavouriteTowers WHERE UserID = @id", conn))
        {
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }

        using (var cmd = new MySqlCommand("DELETE FROM User WHERE ID = @id", conn))
        {
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }

        TempData["Message"] = $"Account \"{user.Username}\" and its favorites were deleted.";
        return RedirectToAction(nameof(Users));
    }

    // Leert nur die Favoritenliste eines Accounts, der Account selbst bleibt.
    [HttpPost]
    public IActionResult ClearFavorites(int id)
    {
        var user = LoadUser(id);
        if (user is null)
        {
            return NotFound();
        }

        using var conn = new MySqlConnection(_connString);
        conn.Open();

        using var cmd = new MySqlCommand("DELETE FROM FavouriteTowers WHERE UserID = @id", conn);
        cmd.Parameters.AddWithValue("@id", id);
        var removed = cmd.ExecuteNonQuery();

        TempData["Message"] = $"Removed {removed} favorite(s) from \"{user.Username}\".";
        return RedirectToAction(nameof(Users));
    }

    private AdminUser? LoadUser(int id)
    {
        using var conn = new MySqlConnection(_connString);
        conn.Open();

        using var cmd = new MySqlCommand(
            @"SELECT u.ID, u.Username, u.SecurityQuestion, COUNT(f.TowerID)
              FROM User u
              LEFT JOIN FavouriteTowers f ON f.UserID = u.ID
              WHERE u.ID = @id
              GROUP BY u.ID, u.Username, u.SecurityQuestion", conn);
        cmd.Parameters.AddWithValue("@id", id);

        using var reader = cmd.ExecuteReader();
        if (!reader.Read())
        {
            return null;
        }

        var username = reader.GetString(1);
        return new AdminUser
        {
            Id = reader.GetInt32(0),
            Username = username,
            SecurityQuestion = reader.GetString(2),
            FavoriteCount = reader.GetInt32(3),
            IsAdmin = string.Equals(username, AdminUsername, StringComparison.OrdinalIgnoreCase)
        };
    }

    // Towers ---------------------------------------------------------------

    public IActionResult Towers(string? search)
    {
        var towers = new List<AdminTowerForm>();

        using var conn = new MySqlConnection(_connString);
        conn.Open();

        // Die zwei COUNTs sagen, was beim Löschen mit weggeht. Sie brauchen
        // DISTINCT: durch die zwei LEFT JOINs kommt jede Favoriten Zeile einmal
        // pro Upgrade Pfad vor und würde sonst mehrfach gezählt.
        var sql = @"SELECT t.ID, t.CategoryId, t.Slug, t.Name, t.ImageFileName, t.Cost, t.Damage, t.Pierce, t.`Range`, t.GameVersion, tc.Name AS CategoryName,
                           COUNT(DISTINCT f.UserID) AS FavoriteCount, COUNT(DISTINCT p.ID) AS UpgradeCount
                    FROM Tower t
                    JOIN TowerCategory tc ON t.CategoryId = tc.ID
                    LEFT JOIN FavouriteTowers f ON f.TowerID = t.ID
                    LEFT JOIN TowerUpgradePaths p ON p.TowerId = t.ID";

        if (!string.IsNullOrWhiteSpace(search))
        {
            sql += " WHERE t.Name LIKE @search OR t.Slug LIKE @search";
        }

        sql += @" GROUP BY t.ID, t.CategoryId, t.Slug, t.Name, t.ImageFileName, t.Cost, t.Damage, t.Pierce, t.`Range`, t.GameVersion, tc.Name
                  ORDER BY t.ID";

        using (var cmd = new MySqlCommand(sql, conn))
        {
            if (!string.IsNullOrWhiteSpace(search))
            {
                cmd.Parameters.AddWithValue("@search", $"%{search}%");
            }

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                towers.Add(new AdminTowerForm
                {
                    Id = reader.GetInt32(0),
                    CategoryId = reader.GetInt32(1),
                    Slug = reader.GetString(2),
                    Name = reader.GetString(3),
                    ImageFileName = reader.IsDBNull(4) ? "" : reader.GetString(4),
                    Cost = reader.GetInt32(5),
                    Damage = reader.GetInt32(6),
                    Pierce = reader.GetInt32(7),
                    Range = reader.IsDBNull(8) ? null : reader.GetInt32(8),
                    GameVersion = reader.IsDBNull(9) ? null : reader.GetString(9),
                    // Die Liste braucht nur den Kategorie Namen zum Anzeigen,
                    // kein ganzes Dropdown. Darum ein einzelner Eintrag.
                    Categories = [new AdminLookupItem { Id = reader.GetInt32(1), Name = reader.GetString(10) }],
                    FavoriteCount = reader.GetInt32(11),
                    UpgradePathCount = reader.GetInt32(12)
                });
            }
        }

        ViewData["Search"] = search ?? "";
        ViewData["Message"] = TempData["Message"];
        ViewData["Error"] = TempData["Error"];
        return View(towers);
    }

    [HttpGet]
    public IActionResult EditTower(int id)
    {
        using var conn = new MySqlConnection(_connString);
        conn.Open();

        AdminTowerForm form;

        using (var cmd = new MySqlCommand(
            @"SELECT ID, CategoryId, Slug, Name, Description, ImageFileName, Cost, Damage, Pierce, `Range`, GameVersion
              FROM Tower WHERE ID = @id", conn))
        {
            cmd.Parameters.AddWithValue("@id", id);

            using var reader = cmd.ExecuteReader();
            if (!reader.Read())
            {
                return NotFound();
            }

            form = new AdminTowerForm
            {
                Id = reader.GetInt32(0),
                CategoryId = reader.GetInt32(1),
                Slug = reader.GetString(2),
                Name = reader.GetString(3),
                Description = reader.IsDBNull(4) ? "" : reader.GetString(4),
                ImageFileName = reader.IsDBNull(5) ? "" : reader.GetString(5),
                Cost = reader.GetInt32(6),
                Damage = reader.GetInt32(7),
                Pierce = reader.GetInt32(8),
                Range = reader.IsDBNull(9) ? null : reader.GetInt32(9),
                GameVersion = reader.IsDBNull(10) ? null : reader.GetString(10)
            };
        }

        form.Categories = LoadLookup(conn, "TowerCategory");
        return View(form);
    }

    [HttpPost]
    public IActionResult EditTower(AdminTowerForm form)
    {
        using var conn = new MySqlConnection(_connString);
        conn.Open();

        form.Categories = LoadLookup(conn, "TowerCategory");

        if (string.IsNullOrWhiteSpace(form.Name) || string.IsNullOrWhiteSpace(form.Slug))
        {
            ViewData["Error"] = "Name and slug can't be empty.";
            return View(form);
        }

        // Slug steckt in der URL der Detailseite und ist in der DB UNIQUE.
        using (var checkCmd = new MySqlCommand("SELECT COUNT(*) FROM Tower WHERE Slug = @slug AND ID <> @id", conn))
        {
            checkCmd.Parameters.AddWithValue("@slug", form.Slug);
            checkCmd.Parameters.AddWithValue("@id", form.Id);
            if (Convert.ToInt32(checkCmd.ExecuteScalar()) > 0)
            {
                ViewData["Error"] = "Another tower already uses this slug.";
                return View(form);
            }
        }

        using (var cmd = new MySqlCommand(
            @"UPDATE Tower
              SET CategoryId = @categoryId, Slug = @slug, Name = @name, Description = @description,
                  ImageFileName = @image, Cost = @cost, Damage = @damage, Pierce = @pierce,
                  `Range` = @range, GameVersion = @version
              WHERE ID = @id", conn))
        {
            cmd.Parameters.AddWithValue("@categoryId", form.CategoryId);
            cmd.Parameters.AddWithValue("@slug", form.Slug);
            cmd.Parameters.AddWithValue("@name", form.Name);
            cmd.Parameters.AddWithValue("@description", form.Description ?? "");
            cmd.Parameters.AddWithValue("@image", form.ImageFileName ?? "");
            cmd.Parameters.AddWithValue("@cost", form.Cost);
            cmd.Parameters.AddWithValue("@damage", form.Damage);
            cmd.Parameters.AddWithValue("@pierce", form.Pierce);
            // Leeres Feld heisst unendliche Reichweite, das ist in der DB NULL.
            cmd.Parameters.AddWithValue("@range", form.Range.HasValue ? form.Range.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("@version", string.IsNullOrWhiteSpace(form.GameVersion) ? DBNull.Value : form.GameVersion);
            cmd.Parameters.AddWithValue("@id", form.Id);
            cmd.ExecuteNonQuery();
        }

        TempData["Message"] = $"Tower \"{form.Name}\" saved.";
        return RedirectToAction(nameof(Towers));
    }

    [HttpGet]
    public IActionResult CreateTower()
    {
        using var conn = new MySqlConnection(_connString);
        conn.Open();

        // Ein leeres Formular. Die Zahlenfelder stehen dadurch auf 0 und die
        // ID bleibt 0, die vergibt beim INSERT das AUTO_INCREMENT der Tabelle.
        return View(new AdminTowerForm { Categories = LoadLookup(conn, "TowerCategory") });
    }

    [HttpPost]
    public IActionResult CreateTower(AdminTowerForm form)
    {
        using var conn = new MySqlConnection(_connString);
        conn.Open();

        form.Categories = LoadLookup(conn, "TowerCategory");

        if (string.IsNullOrWhiteSpace(form.Name) || string.IsNullOrWhiteSpace(form.Slug))
        {
            ViewData["Error"] = "Name and slug can't be empty.";
            return View(form);
        }

        // Gleiche Prüfung wie beim Bearbeiten, nur ohne die eigene Zeile
        // auszunehmen: diesen Tower gibt es noch nicht.
        using (var checkCmd = new MySqlCommand("SELECT COUNT(*) FROM Tower WHERE Slug = @slug", conn))
        {
            checkCmd.Parameters.AddWithValue("@slug", form.Slug);
            if (Convert.ToInt32(checkCmd.ExecuteScalar()) > 0)
            {
                ViewData["Error"] = "Another tower already uses this slug.";
                return View(form);
            }
        }

        using (var cmd = new MySqlCommand(
            @"INSERT INTO Tower (CategoryId, Slug, Name, Description, ImageFileName, Cost, Damage, Pierce, `Range`, GameVersion)
              VALUES (@categoryId, @slug, @name, @description, @image, @cost, @damage, @pierce, @range, @version)", conn))
        {
            cmd.Parameters.AddWithValue("@categoryId", form.CategoryId);
            cmd.Parameters.AddWithValue("@slug", form.Slug);
            cmd.Parameters.AddWithValue("@name", form.Name);
            cmd.Parameters.AddWithValue("@description", form.Description ?? "");
            cmd.Parameters.AddWithValue("@image", form.ImageFileName ?? "");
            cmd.Parameters.AddWithValue("@cost", form.Cost);
            cmd.Parameters.AddWithValue("@damage", form.Damage);
            cmd.Parameters.AddWithValue("@pierce", form.Pierce);
            // Leeres Feld heisst unendliche Reichweite, das ist in der DB NULL.
            cmd.Parameters.AddWithValue("@range", form.Range.HasValue ? form.Range.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("@version", string.IsNullOrWhiteSpace(form.GameVersion) ? DBNull.Value : form.GameVersion);
            cmd.ExecuteNonQuery();
        }

        TempData["Message"] = $"Tower \"{form.Name}\" created.";
        return RedirectToAction(nameof(Towers));
    }

    // Löscht einen Tower samt allem, was per Fremdschlüssel an ihm hängt.
    [HttpPost]
    public IActionResult DeleteTower(int id)
    {
        using var conn = new MySqlConnection(_connString);
        conn.Open();

        string name;

        using (var cmd = new MySqlCommand("SELECT Name FROM Tower WHERE ID = @id", conn))
        {
            cmd.Parameters.AddWithValue("@id", id);
            if (cmd.ExecuteScalar() is not string found)
            {
                return NotFound();
            }

            name = found;
        }

        // Zwei Tabellen zeigen per Foreign Key auf Tower: die Favoriten der
        // Accounts und die Upgrade Pfade. Beide müssen zuerst weg, sonst lehnt
        // MySQL das DELETE ab. Gleiche Reihenfolge wie bei DeleteUser.
        using (var cmd = new MySqlCommand("DELETE FROM FavouriteTowers WHERE TowerID = @id", conn))
        {
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }

        using (var cmd = new MySqlCommand("DELETE FROM TowerUpgradePaths WHERE TowerId = @id", conn))
        {
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }

        using (var cmd = new MySqlCommand("DELETE FROM Tower WHERE ID = @id", conn))
        {
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }

        TempData["Message"] = $"Tower \"{name}\" was deleted, together with its upgrade paths and favorites.";
        return RedirectToAction(nameof(Towers));
    }

    // Bloons ---------------------------------------------------------------

    public IActionResult Bloons(string? search)
    {
        var bloons = new List<AdminBloonForm>();

        using var conn = new MySqlConnection(_connString);
        conn.Open();

        var sql = @"SELECT b.ID, b.CategoryId, b.Slug, b.Name, b.ImageFileName, b.ColorHex, b.Rbe, b.Speed,
                           b.IsCamo, b.IsLead, b.Layer, b.GameVersion, bc.Name AS CategoryName
                    FROM Bloon b
                    JOIN BloonCategory bc ON b.CategoryId = bc.ID";

        if (!string.IsNullOrWhiteSpace(search))
        {
            sql += " WHERE b.Name LIKE @search OR b.Slug LIKE @search";
        }

        sql += " ORDER BY b.ID";

        using (var cmd = new MySqlCommand(sql, conn))
        {
            if (!string.IsNullOrWhiteSpace(search))
            {
                cmd.Parameters.AddWithValue("@search", $"%{search}%");
            }

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                bloons.Add(new AdminBloonForm
                {
                    Id = reader.GetInt32(0),
                    CategoryId = reader.GetInt32(1),
                    Slug = reader.GetString(2),
                    Name = reader.GetString(3),
                    ImageFileName = reader.IsDBNull(4) ? "" : reader.GetString(4),
                    ColorHex = reader.IsDBNull(5) ? "" : reader.GetString(5),
                    Rbe = reader.GetInt32(6),
                    Speed = reader.GetDecimal(7),
                    IsCamo = reader.GetBoolean(8),
                    IsLead = reader.GetBoolean(9),
                    Layer = reader.GetInt32(10),
                    GameVersion = reader.IsDBNull(11) ? null : reader.GetString(11),
                    Categories = [new AdminLookupItem { Id = reader.GetInt32(1), Name = reader.GetString(12) }]
                });
            }
        }

        ViewData["Search"] = search ?? "";
        ViewData["Message"] = TempData["Message"];
        ViewData["Error"] = TempData["Error"];
        return View(bloons);
    }

    [HttpGet]
    public IActionResult EditBloon(int id)
    {
        using var conn = new MySqlConnection(_connString);
        conn.Open();

        AdminBloonForm form;

        using (var cmd = new MySqlCommand(
            @"SELECT ID, CategoryId, Slug, Name, Description, ImageFileName, ColorHex, ImageScale,
                     Rbe, Speed, IsCamo, IsLead, Layer, GameVersion, PopsIntoNote
              FROM Bloon WHERE ID = @id", conn))
        {
            cmd.Parameters.AddWithValue("@id", id);

            using var reader = cmd.ExecuteReader();
            if (!reader.Read())
            {
                return NotFound();
            }

            form = new AdminBloonForm
            {
                Id = reader.GetInt32(0),
                CategoryId = reader.GetInt32(1),
                Slug = reader.GetString(2),
                Name = reader.GetString(3),
                Description = reader.IsDBNull(4) ? "" : reader.GetString(4),
                ImageFileName = reader.IsDBNull(5) ? "" : reader.GetString(5),
                ColorHex = reader.IsDBNull(6) ? "" : reader.GetString(6),
                ImageScale = reader.GetDecimal(7),
                Rbe = reader.GetInt32(8),
                Speed = reader.GetDecimal(9),
                IsCamo = reader.GetBoolean(10),
                IsLead = reader.GetBoolean(11),
                Layer = reader.GetInt32(12),
                GameVersion = reader.IsDBNull(13) ? null : reader.GetString(13),
                PopsIntoNote = reader.IsDBNull(14) ? null : reader.GetString(14)
            };
        }

        form.Categories = LoadLookup(conn, "BloonCategory");
        return View(form);
    }

    [HttpPost]
    public IActionResult EditBloon(AdminBloonForm form)
    {
        using var conn = new MySqlConnection(_connString);
        conn.Open();

        form.Categories = LoadLookup(conn, "BloonCategory");

        if (string.IsNullOrWhiteSpace(form.Name) || string.IsNullOrWhiteSpace(form.Slug))
        {
            ViewData["Error"] = "Name and slug can't be empty.";
            return View(form);
        }

        using (var checkCmd = new MySqlCommand("SELECT COUNT(*) FROM Bloon WHERE Slug = @slug AND ID <> @id", conn))
        {
            checkCmd.Parameters.AddWithValue("@slug", form.Slug);
            checkCmd.Parameters.AddWithValue("@id", form.Id);
            if (Convert.ToInt32(checkCmd.ExecuteScalar()) > 0)
            {
                ViewData["Error"] = "Another bloon already uses this slug.";
                return View(form);
            }
        }

        using (var cmd = new MySqlCommand(
            @"UPDATE Bloon
              SET CategoryId = @categoryId, Slug = @slug, Name = @name, Description = @description,
                  ImageFileName = @image, ColorHex = @color, ImageScale = @scale, Rbe = @rbe,
                  Speed = @speed, IsCamo = @camo, IsLead = @lead, Layer = @layer,
                  GameVersion = @version, PopsIntoNote = @note
              WHERE ID = @id", conn))
        {
            cmd.Parameters.AddWithValue("@categoryId", form.CategoryId);
            cmd.Parameters.AddWithValue("@slug", form.Slug);
            cmd.Parameters.AddWithValue("@name", form.Name);
            cmd.Parameters.AddWithValue("@description", form.Description ?? "");
            cmd.Parameters.AddWithValue("@image", form.ImageFileName ?? "");
            cmd.Parameters.AddWithValue("@color", form.ColorHex ?? "");
            cmd.Parameters.AddWithValue("@scale", form.ImageScale);
            cmd.Parameters.AddWithValue("@rbe", form.Rbe);
            cmd.Parameters.AddWithValue("@speed", form.Speed);
            cmd.Parameters.AddWithValue("@camo", form.IsCamo);
            cmd.Parameters.AddWithValue("@lead", form.IsLead);
            cmd.Parameters.AddWithValue("@layer", form.Layer);
            cmd.Parameters.AddWithValue("@version", string.IsNullOrWhiteSpace(form.GameVersion) ? DBNull.Value : form.GameVersion);
            // Leeres Notiz Feld heisst, der Bloon platzt in feste Kinder statt
            // in einen zufälligen Bloon. Dann muss die Spalte NULL sein.
            cmd.Parameters.AddWithValue("@note", string.IsNullOrWhiteSpace(form.PopsIntoNote) ? DBNull.Value : form.PopsIntoNote);
            cmd.Parameters.AddWithValue("@id", form.Id);
            cmd.ExecuteNonQuery();
        }

        TempData["Message"] = $"Bloon \"{form.Name}\" saved.";
        return RedirectToAction(nameof(Bloons));
    }

    private static List<AdminLookupItem> LoadLookup(MySqlConnection conn, string table)
    {
        // Der Tabellenname kommt nur aus dem Code hier drin, nie aus einer
        // Eingabe. Tabellennamen kann man in MySQL ohnehin nicht als Parameter
        // übergeben, darum wird er direkt in den String gesetzt.
        var items = new List<AdminLookupItem>();

        using var cmd = new MySqlCommand($"SELECT ID, Name FROM `{table}` ORDER BY ID", conn);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            items.Add(new AdminLookupItem { Id = reader.GetInt32(0), Name = reader.GetString(1) });
        }

        return items;
    }

    // SQL Konsole ----------------------------------------------------------

    // Ohne sql ist das nur die leere Konsole. Mit sql im Query String wird die
    // Abfrage gleich ausgeführt. Das benutzen die "Browse" Links auf dem
    // Dashboard, damit man eine Tabelle mit einem Klick anschauen kann.
    // Nur Lesen ist erlaubt, ein GET verändert hier also nichts.
    [HttpGet]
    public IActionResult Query(string? sql)
    {
        return View(string.IsNullOrWhiteSpace(sql) ? new AdminQueryResult() : RunQuery(sql));
    }

    [HttpPost]
    [ActionName(nameof(Query))]
    public IActionResult QueryPost(string sql)
    {
        return View(nameof(Query), RunQuery(sql));
    }

    // Führt eine einzelne Leseabfrage aus und gibt das Ergebnis als Tabelle
    // zurück. Schreiben ist absichtlich gesperrt, siehe AllowedQueryVerbs.
    private AdminQueryResult RunQuery(string? sql)
    {
        var model = new AdminQueryResult { Sql = sql ?? "", HasRun = true };

        var trimmed = model.Sql.Trim().TrimEnd(';').Trim();

        if (trimmed.Length == 0)
        {
            model.Error = "Enter a query first.";
            return model;
        }

        // Mehrere Anweisungen auf einmal wären ein Weg, die Verb Prüfung zu
        // umgehen (zum Beispiel "SELECT 1; DELETE FROM User"). Darum geht nur eine.
        if (trimmed.Contains(';'))
        {
            model.Error = "Only one statement at a time (no semicolons).";
            return model;
        }

        var verb = trimmed.Split(' ', '\t', '\n', '\r', '(')[0].ToUpperInvariant();
        if (!AllowedQueryVerbs.Contains(verb))
        {
            model.Error = $"Only read-only queries are allowed here ({string.Join(", ", AllowedQueryVerbs)}). Use the forms in the other tabs to change data.";
            return model;
        }

        try
        {
            using var conn = new MySqlConnection(_connString);
            conn.Open();

            // Die Prüfung vom ersten Wort allein reicht nicht. MySQL 8 erlaubt zum
            // Beispiel "WITH x AS (SELECT 1) DELETE FROM User", das fängt mit WITH
            // an und löscht trotzdem. Darum läuft die Abfrage in einer Read Only
            // Transaktion. Dort lehnt MySQL jedes Schreiben selbst ab (Fehler 1792).
            // Am Ende wird die Transaktion nur zurückgerollt, es gibt nie ein Commit.
            using var transaction = conn.BeginTransaction(System.Data.IsolationLevel.RepeatableRead, isReadOnly: true);
            using var cmd = new MySqlCommand(trimmed, conn, transaction);
            using var reader = cmd.ExecuteReader();

            for (var i = 0; i < reader.FieldCount; i++)
            {
                model.Columns.Add(reader.GetName(i));
            }

            while (reader.Read())
            {
                if (model.Rows.Count >= QueryRowLimit)
                {
                    model.Truncated = true;
                    break;
                }

                var row = new List<string>(reader.FieldCount);
                for (var i = 0; i < reader.FieldCount; i++)
                {
                    row.Add(reader.IsDBNull(i) ? "NULL" : reader.GetValue(i).ToString() ?? "");
                }

                model.Rows.Add(row);
            }
        }
        catch (MySqlException ex)
        {
            // Die Meldung von MySQL direkt anzeigen. Sie ist für den Admin
            // genau das, was er braucht (falscher Spaltenname, Syntaxfehler, ...).
            model.Error = ex.Message;
        }

        return model;
    }
}
