using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using MonkeyArchive.Models;
using MySqlConnector;

namespace MonkeyArchive.Controllers;

// Zeigt die "Alle Bloons" Übersicht auf /Bloons und die einzelnen Detailseiten.
// Die Daten kommen bei jedem Request frisch aus der MySQL Datenbank (siehe LoadAllBloons),
// es steht nichts mehr fest im Code.
public class BloonsController : Controller
{
    private readonly string _connString;

    public BloonsController(IConfiguration config)
    {
        _connString = config.GetConnectionString("Default")!;
    }

    // Lädt alle Bloons mit Kategorie Name, Immunitäten und Pops Into Liste aus der DB.
    // Drei Abfragen statt einer pro Bloon, damit es nicht für jeden Bloon extra zur DB geht.
    private List<Bloon> LoadAllBloons()
    {
        var bloons = new List<Bloon>();
        var bloonsById = new Dictionary<int, Bloon>();

        using var conn = new MySqlConnection(_connString);
        conn.Open();

        using (var cmd = new MySqlCommand(
            @"SELECT b.ID, b.Slug, b.Name, bc.Name AS CategoryName, b.Description, b.ImageFileName, b.ColorHex,
                     b.ImageScale, b.Rbe, b.Speed, b.IsCamo, b.IsLead, b.Layer, b.PopsIntoNote
              FROM Bloon b
              JOIN BloonCategory bc ON b.CategoryId = bc.ID
              ORDER BY b.ID", conn))
        using (var reader = cmd.ExecuteReader())
        {
            var idCol = reader.GetOrdinal("ID");
            var slugCol = reader.GetOrdinal("Slug");
            var nameCol = reader.GetOrdinal("Name");
            var categoryCol = reader.GetOrdinal("CategoryName");
            var descCol = reader.GetOrdinal("Description");
            var imageCol = reader.GetOrdinal("ImageFileName");
            var colorCol = reader.GetOrdinal("ColorHex");
            var scaleCol = reader.GetOrdinal("ImageScale");
            var rbeCol = reader.GetOrdinal("Rbe");
            var speedCol = reader.GetOrdinal("Speed");
            var camoCol = reader.GetOrdinal("IsCamo");
            var leadCol = reader.GetOrdinal("IsLead");
            var layerCol = reader.GetOrdinal("Layer");
            var noteCol = reader.GetOrdinal("PopsIntoNote");

            while (reader.Read())
            {
                var bloon = new Bloon
                {
                    Slug = reader.GetString(slugCol),
                    Name = reader.GetString(nameCol),
                    Category = reader.GetString(categoryCol),
                    Description = reader.GetString(descCol),
                    Image = reader.GetString(imageCol),
                    Color = reader.GetString(colorCol),
                    ImageScale = (double)reader.GetDecimal(scaleCol),
                    Rbe = reader.GetInt32(rbeCol),
                    // Speed ist in der DB eine DECIMAL Spalte. "0.##" schneidet unnötige Nullen ab,
                    // z.B. wird aus 25.00 einfach "25" und aus 87.50 "87.5".
                    Speed = reader.GetDecimal(speedCol).ToString("0.##", CultureInfo.InvariantCulture),
                    IsCamo = reader.GetBoolean(camoCol),
                    IsLead = reader.GetBoolean(leadCol),
                    Layer = reader.GetInt32(layerCol),
                    PopsIntoNote = reader.IsDBNull(noteCol) ? null : reader.GetString(noteCol)
                };

                bloons.Add(bloon);
                bloonsById[reader.GetInt32(idCol)] = bloon;
            }
        }

        // Immunity.Name ist in der DB nur "Explosions"/"Cold"/"Energy", die View zeigt die Tags
        // aber als "Immune to Explosions" usw., darum wird das hier davorgesetzt.
        using (var cmd = new MySqlCommand(
            @"SELECT bi.BloonId, i.Name AS ImmunityName
              FROM BloonImmunities bi
              JOIN Immunity i ON bi.ImmunityId = i.ID
              ORDER BY bi.BloonId", conn))
        using (var reader = cmd.ExecuteReader())
        {
            var bloonIdCol = reader.GetOrdinal("BloonId");
            var immunityCol = reader.GetOrdinal("ImmunityName");

            while (reader.Read())
            {
                if (!bloonsById.TryGetValue(reader.GetInt32(bloonIdCol), out var bloon))
                {
                    continue;
                }

                bloon.Immunities.Add("Immune to " + reader.GetString(immunityCol));
            }
        }

        using (var cmd = new MySqlCommand(
            @"SELECT p.BloonId, p.Count, child.Name AS ChildName
              FROM BloonPopsInto p
              JOIN Bloon child ON p.ChildBloonId = child.ID
              ORDER BY p.BloonId, p.ID", conn))
        using (var reader = cmd.ExecuteReader())
        {
            var bloonIdCol = reader.GetOrdinal("BloonId");
            var countCol = reader.GetOrdinal("Count");
            var childNameCol = reader.GetOrdinal("ChildName");

            while (reader.Read())
            {
                if (!bloonsById.TryGetValue(reader.GetInt32(bloonIdCol), out var bloon))
                {
                    continue;
                }

                bloon.PopsInto.Add(new BloonChild
                {
                    Name = reader.GetString(childNameCol),
                    Count = reader.GetInt32(countCol)
                });
            }
        }

        return bloons;
    }

    // Gleich wie TowersController.Index. Suche und Kategorie kommen aus dem Query String, kein JavaScript nötig.
    public IActionResult Index(string? search, string? category)
    {
        var bloons = LoadAllBloons().AsEnumerable();

        var selectedCategory = string.IsNullOrWhiteSpace(category) ? "All" : category;
        if (selectedCategory != "All")
        {
            bloons = bloons.Where(b => b.Category == selectedCategory);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            bloons = bloons.Where(b => b.Name.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        var model = new BloonsOverviewViewModel
        {
            Bloons = bloons.ToList(),
            Search = search ?? string.Empty,
            SelectedCategory = selectedCategory
        };

        return View(model);
    }

    // Zum Beispiel /Bloons/Details/red. Die id ist der Slug vom Bloon, nicht der Name.
    // So bleibt die URL lesbar und muss nicht encodiert werden.
    public IActionResult Details(string id)
    {
        var allBloons = LoadAllBloons();
        var index = allBloons.FindIndex(b => b.Slug == id);
        if (index == -1)
        {
            return NotFound();
        }

        // Die Pfeile gehen durch die Liste in ihrer Reihenfolge, gleich wie bei TowersController.Details.
        // Am Anfang und Ende springt es wieder um.
        var previous = allBloons[(index - 1 + allBloons.Count) % allBloons.Count];
        var next = allBloons[(index + 1) % allBloons.Count];
        ViewData["PreviousSlug"] = previous.Slug;
        ViewData["PreviousName"] = previous.Name;
        ViewData["NextSlug"] = next.Slug;
        ViewData["NextName"] = next.Name;

        // Die "Pops Into" Karte braucht Bild und Scale von jedem Kind Bloon.
        // Ein BloonChild speichert aber nur den Namen. Darum wird das Bild hier gesucht und nicht in der View.
        ViewData["BloonImages"] = allBloons.ToDictionary(b => b.Name, b => b.Image);
        ViewData["BloonScales"] = allBloons.ToDictionary(b => b.Name, b => b.ImageScale);

        return View(allBloons[index]);
    }
}
