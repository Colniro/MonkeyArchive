using Microsoft.AspNetCore.Mvc;
using MonkeyArchive.Models;
using MySqlConnector;

namespace MonkeyArchive.Controllers;

// Zeigt die "Alle Towers" Übersicht auf /Towers und die einzelnen Detailseiten.
// Die Daten kommen bei jedem Request frisch aus der MySQL Datenbank (siehe LoadAllTowers),
// es steht nichts mehr fest im Code.
public class TowersController : Controller
{
    private readonly string _connString;

    public TowersController(IConfiguration config)
    {
        _connString = config.GetConnectionString("Default")!;
    }

    // Lädt alle Towers mit Kategorie Name und allen Upgrade Pfaden aus der DB.
    // Zwei Abfragen statt einer pro Tower, damit es nicht für jeden Tower extra zur DB geht.
    private List<Tower> LoadAllTowers()
    {
        var towers = new List<Tower>();
        var towersById = new Dictionary<int, Tower>();

        using var conn = new MySqlConnection(_connString);
        conn.Open();

        using (var cmd = new MySqlCommand(
            @"SELECT t.ID, t.Slug, t.Name, tc.Name AS CategoryName, t.Description, t.ImageFileName, t.Cost, t.Damage, t.Pierce, t.Range
              FROM Tower t
              JOIN TowerCategory tc ON t.CategoryId = tc.ID
              ORDER BY t.ID", conn))
        using (var reader = cmd.ExecuteReader())
        {
            var idCol = reader.GetOrdinal("ID");
            var slugCol = reader.GetOrdinal("Slug");
            var nameCol = reader.GetOrdinal("Name");
            var categoryCol = reader.GetOrdinal("CategoryName");
            var descCol = reader.GetOrdinal("Description");
            var imageCol = reader.GetOrdinal("ImageFileName");
            var costCol = reader.GetOrdinal("Cost");
            var damageCol = reader.GetOrdinal("Damage");
            var pierceCol = reader.GetOrdinal("Pierce");
            var rangeCol = reader.GetOrdinal("Range");

            while (reader.Read())
            {
                var tower = new Tower
                {
                    Slug = reader.GetString(slugCol),
                    Name = reader.GetString(nameCol),
                    Category = reader.GetString(categoryCol),
                    Description = reader.GetString(descCol),
                    Image = reader.GetString(imageCol),
                    Cost = reader.GetInt32(costCol),
                    Damage = reader.GetInt32(damageCol),
                    Pierce = reader.GetInt32(pierceCol),
                    Range = reader.IsDBNull(rangeCol) ? null : reader.GetInt32(rangeCol)
                };

                towers.Add(tower);
                towersById[reader.GetInt32(idCol)] = tower;
            }
        }

        // PathLabel.Name ist in der DB nur "Top"/"Middle"/"Bottom", die View will aber
        // "Top Path" usw. (siehe Towers/Details.cshtml), darum wird " Path" hier angehängt.
        using (var cmd = new MySqlCommand(
            @"SELECT up.TowerId, pl.Name AS PathLabelName, up.Name, up.Description, up.Price
              FROM TowerUpgradePaths up
              JOIN PathLabel pl ON up.PathLabelId = pl.ID
              ORDER BY up.TowerId, up.ID", conn))
        using (var reader = cmd.ExecuteReader())
        {
            var towerIdCol = reader.GetOrdinal("TowerId");
            var pathLabelCol = reader.GetOrdinal("PathLabelName");
            var nameCol = reader.GetOrdinal("Name");
            var descCol = reader.GetOrdinal("Description");
            var priceCol = reader.GetOrdinal("Price");

            while (reader.Read())
            {
                if (!towersById.TryGetValue(reader.GetInt32(towerIdCol), out var tower))
                {
                    continue;
                }

                tower.UpgradePaths.Add(new TowerUpgradePath
                {
                    PathLabel = reader.GetString(pathLabelCol) + " Path",
                    Name = reader.GetString(nameCol),
                    Description = reader.GetString(descCol),
                    Price = reader.GetInt32(priceCol)
                });
            }
        }

        return towers;
    }

    // Suche und Kategorie Filter laufen beide über die Query String Parameter (?search=...&category=...).
    // Die Seite braucht dafür kein JavaScript. Suchfeld und Filter Pills sind normale GET Links und Formulare.
    // Die laden diese Action einfach mit anderen Parametern neu.
    public IActionResult Index(string? search, string? category)
    {
        var towers = LoadAllTowers().AsEnumerable();

        var selectedCategory = string.IsNullOrWhiteSpace(category) ? "All" : category;
        if (selectedCategory != "All")
        {
            towers = towers.Where(t => t.Category == selectedCategory);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            towers = towers.Where(t => t.Name.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        var model = new TowersOverviewViewModel
        {
            Towers = towers.ToList(),
            Search = search ?? string.Empty,
            SelectedCategory = selectedCategory
        };

        return View(model);
    }

    // Zum Beispiel /Towers/Details/dart-monkey. Die id ist der Slug vom Tower, nicht der Name.
    // So bleibt die URL lesbar und muss nicht encodiert werden.
    public IActionResult Details(string id)
    {
        var allTowers = LoadAllTowers();
        var index = allTowers.FindIndex(t => t.Slug == id);
        if (index == -1)
        {
            return NotFound();
        }

        // Die Pfeile gehen durch die Liste in ihrer Reihenfolge. Am Anfang und Ende springt es wieder um.
        // So hat jeder Tower immer einen nächsten und vorherigen.
        var previous = allTowers[(index - 1 + allTowers.Count) % allTowers.Count];
        var next = allTowers[(index + 1) % allTowers.Count];
        ViewData["PreviousSlug"] = previous.Slug;
        ViewData["PreviousName"] = previous.Name;
        ViewData["NextSlug"] = next.Slug;
        ViewData["NextName"] = next.Name;

        var username = HttpContext.Session.GetString("Username");
        if (username is not null)
        {
            using var conn = new MySqlConnection(_connString);
            conn.Open();

            using var cmd = new MySqlCommand(
                @"SELECT COUNT(*) FROM FavouriteTowers f
                  JOIN User u ON f.UserID = u.ID
                  JOIN Tower t ON f.TowerID = t.ID
                  WHERE u.Username = @username AND t.Slug = @slug", conn);
            cmd.Parameters.AddWithValue("@username", username);
            cmd.Parameters.AddWithValue("@slug", id);
            ViewData["IsFavorited"] = Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }

        return View(allTowers[index]);
    }

    // Schaltet den Favoriten Status für einen Tower um. Braucht Login, sonst geht's zu Login
    // mit returnUrl zurück auf die Seite, von der aus geklickt wurde.
    [HttpPost]
    public IActionResult ToggleFavorite(string id, string? returnUrl)
    {
        var username = HttpContext.Session.GetString("Username");
        if (username is null)
        {
            return RedirectToAction("Login", "Account", new { returnUrl = returnUrl ?? Url.Action("Details", new { id }) });
        }

        using var conn = new MySqlConnection(_connString);
        conn.Open();

        using (var checkCmd = new MySqlCommand(
            @"SELECT COUNT(*) FROM FavouriteTowers f
              JOIN User u ON f.UserID = u.ID
              JOIN Tower t ON f.TowerID = t.ID
              WHERE u.Username = @username AND t.Slug = @slug", conn))
        {
            checkCmd.Parameters.AddWithValue("@username", username);
            checkCmd.Parameters.AddWithValue("@slug", id);
            var isFavorited = Convert.ToInt32(checkCmd.ExecuteScalar()) > 0;

            if (isFavorited)
            {
                using var deleteCmd = new MySqlCommand(
                    @"DELETE f FROM FavouriteTowers f
                      JOIN User u ON f.UserID = u.ID
                      JOIN Tower t ON f.TowerID = t.ID
                      WHERE u.Username = @username AND t.Slug = @slug", conn);
                deleteCmd.Parameters.AddWithValue("@username", username);
                deleteCmd.Parameters.AddWithValue("@slug", id);
                deleteCmd.ExecuteNonQuery();
            }
            else
            {
                using var insertCmd = new MySqlCommand(
                    @"INSERT INTO FavouriteTowers (UserID, TowerID)
                      SELECT u.ID, t.ID FROM User u, Tower t
                      WHERE u.Username = @username AND t.Slug = @slug", conn);
                insertCmd.Parameters.AddWithValue("@username", username);
                insertCmd.Parameters.AddWithValue("@slug", id);
                insertCmd.ExecuteNonQuery();
            }
        }

        return string.IsNullOrEmpty(returnUrl) || !Url.IsLocalUrl(returnUrl)
            ? RedirectToAction("Details", new { id })
            : Redirect(returnUrl);
    }
}
