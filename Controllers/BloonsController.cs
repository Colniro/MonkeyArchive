using Microsoft.AspNetCore.Mvc;
using MonkeyArchive.Models;

namespace MonkeyArchive.Controllers;

// Zeigt die "Alle Bloons" Übersicht auf /Bloons.
// Gleich aufgebaut wie TowersController. Feste Spieldaten als Liste im Speicher, gefiltert nach Kategorie und Suche.
public class BloonsController : Controller
{
    private static readonly List<Bloon> AllBloons =
    [
        // ImageScale gleicht den unterschiedlichen transparenten Rand der Bilder aus.
        // Zum Beispiel füllt Yellow fast das ganze Bild, Black nur etwa 30 Prozent.
        // Ohne diesen Wert wären die Ballons bei gleicher Boxgrösse unterschiedlich gross.
        // Gemessen wurde das über die Alpha Kanal Box vom Bild. MOAB Bilder sind schon einheitlich, die bleiben bei 1.0.
        //
        // Die Detail Felder (RBE, Speed, IsLead, Layer, Immunities) kommen aus den gleichen echten BTD6 Spieldaten wie bei TowersController.
        // Das sind maxHealth, speed, layerNumber und das bloonProperties Bitflag.
        // Bit 0 ist Lead, Bit 1 ist immun gegen Explosionen, Bit 2 ist immun gegen Kälte, Bit 3 ist immun gegen Energie.
        // Geprüft wurde das an Lead=1, DDT=3 und Zebra=6.
        // IsCamo ist dagegen für jeden Bloon von Hand eingetragen.
        // Das isCamo Feld im JSON stimmt nämlich nicht, es steht dort auch bei DDT auf false.
        // Bekannt ist aber, dass DDT von diesen 17 der einzige ist der von Natur aus Camo ist.
        // Bei allen anderen Farben ist Camo nur eine Eigenschaft für einzelne Runden, keine feste Eigenschaft.
        // PopsInto und PopsIntoNote zeigen was beim Platzen rauskommt.
        // Das steht in den Spieldaten gar nicht drin, es gibt dort keine Zuordnung von Bloon zu Kind Bloon.
        // Darum steht hier stattdessen die bekannte BTD6 Mechanik, die sich seit Release nicht geändert hat.
        // Zum Beispiel wird Blue zu Red, MOAB wird zu 4x Ceramic.
        // Bei Ceramic und DDT ist es im echten Spiel zufällig, darum steht dort ein Hinweistext statt einem festen Ergebnis.
        new Bloon
        {
            Name = "Red", Category = "Normal", Image = "red_bloon.png", Color = "#DC3E3E", ImageScale = 1.98,
            Description = "The weakest bloon - a single hit pops it for good.",
            Rbe = 1, Speed = "25", IsCamo = false, IsLead = false, Layer = 1,
            PopsInto = []
        },
        new Bloon
        {
            Name = "Blue", Category = "Normal", Image = "blue_bloon.png", Color = "#2F80ED", ImageScale = 1.84,
            Description = "A little tougher than Red and slightly faster.",
            Rbe = 1, Speed = "35", IsCamo = false, IsLead = false, Layer = 2,
            PopsInto = [new BloonChild { Name = "Red", Count = 1 }]
        },
        new Bloon
        {
            Name = "Green", Category = "Normal", Image = "green_bloon.png", Color = "#4F9A3C", ImageScale = 1.73,
            Description = "Faster still, popping into a Blue underneath.",
            Rbe = 1, Speed = "45", IsCamo = false, IsLead = false, Layer = 3,
            PopsInto = [new BloonChild { Name = "Blue", Count = 1 }]
        },
        new Bloon
        {
            Name = "Yellow", Category = "Normal", Image = "yellow_bloon.png", Color = "#F2C94C", ImageScale = 0.89,
            Description = "Noticeably quick, outrunning most early towers' fire rate.",
            Rbe = 1, Speed = "80", IsCamo = false, IsLead = false, Layer = 4,
            PopsInto = [new BloonChild { Name = "Green", Count = 1 }]
        },
        new Bloon
        {
            Name = "Pink", Category = "Normal", Image = "pink_bloon.png", Color = "#F2789F", ImageScale = 0.87,
            Description = "The fastest of the basic colours, testing your reaction time.",
            Rbe = 1, Speed = "87.5", IsCamo = false, IsLead = false, Layer = 5,
            PopsInto = [new BloonChild { Name = "Yellow", Count = 1 }]
        },
        // Black, White, Lead und Zebra sind im Bild selbst schon fast schwarz, fast weiss, grau oder schwarz weiss.
        // Ein Hintergrund in der gleichen Farbe würde den Ballon unsichtbar machen.
        // Darum ist der Hintergrund bewusst anders hell, damit man die Form noch sieht.
        new Bloon
        {
            Name = "Black", Category = "Normal", Image = "black_bloon.png", Color = "#5A5A5A", ImageScale = 2.72,
            Description = "Immune to explosive damage, and splits into two Pinks.",
            Rbe = 1, Speed = "45", IsCamo = false, IsLead = false, Layer = 6,
            Immunities = ["Immune to Explosions"],
            PopsInto = [new BloonChild { Name = "Pink", Count = 2 }]
        },
        new Bloon
        {
            Name = "White", Category = "Normal", Image = "white_bloon.png", Color = "#7C828A", ImageScale = 2.69,
            Description = "Immune to freezing effects, and also splits into two Pinks.",
            Rbe = 1, Speed = "50", IsCamo = false, IsLead = false, Layer = 6,
            Immunities = ["Immune to Cold"],
            PopsInto = [new BloonChild { Name = "Pink", Count = 2 }]
        },
        new Bloon
        {
            Name = "Purple", Category = "Normal", Image = "purple_bloon.png", Color = "#9B51E0", ImageScale = 0.89,
            Description = "Immune to energy and magic damage, popping into a single Pink.",
            Rbe = 1, Speed = "75", IsCamo = false, IsLead = false, Layer = 6,
            Immunities = ["Immune to Energy"],
            PopsInto = [new BloonChild { Name = "Pink", Count = 1 }]
        },
        new Bloon
        {
            Name = "Lead", Category = "Normal", Image = "lead_bloon.png", Color = "#2E3236", ImageScale = 0.88,
            Description = "Immune to sharp damage until popped by explosives or magic.",
            Rbe = 1, Speed = "25", IsCamo = false, IsLead = true, Layer = 7,
            PopsInto = [new BloonChild { Name = "Black", Count = 1 }, new BloonChild { Name = "White", Count = 1 }]
        },
        new Bloon
        {
            Name = "Zebra", Category = "Normal", Image = "zebra_bloon.png", Color = "#808080", ImageScale = 1.74,
            Description = "Combines Black's and White's immunities in one tricky package.",
            Rbe = 1, Speed = "45", IsCamo = false, IsLead = false, Layer = 7,
            Immunities = ["Immune to Explosions", "Immune to Cold"],
            PopsInto = [new BloonChild { Name = "Black", Count = 1 }, new BloonChild { Name = "White", Count = 1 }]
        },
        new Bloon
        {
            Name = "Rainbow", Category = "Normal", Image = "rainbow_bloon.png", Color = "linear-gradient(135deg, #E5484D, #F2994E, #F2C94C, #27AE60, #2F80ED, #9B51E0)", ImageScale = 1.57,
            Description = "A colourful heavyweight that reveals two Zebras when popped.",
            Rbe = 1, Speed = "55", IsCamo = false, IsLead = false, Layer = 8,
            PopsInto = [new BloonChild { Name = "Zebra", Count = 2 }]
        },
        new Bloon
        {
            Name = "Ceramic", Category = "Normal", Image = "ceramic_bloon.png", Color = "#8B4A2B", ImageScale = 0.97,
            Description = "A tough shell hiding a random weaker bloon underneath.",
            Rbe = 10, Speed = "62.5", IsCamo = false, IsLead = false, Layer = 9,
            PopsIntoNote = "Pops into one random weaker bloon - Rainbow, Zebra, Black, White, or Lead. The exact result isn't fixed."
        },

        // Die MOAB Farben waren zuerst geraten, ohne die Bilder genau anzuschauen. Das wurde korrigiert.
        // MOAB ist blau, BFB ist rot, ZOMG ist eine dunkle Bombe mit neongrünen Akzenten, DDT ist dunkel und camo, BAD ist lila.
        // Jeder Hintergrund ist extra so gewählt, dass er heller ist als der meist dunkle Bombenkörper.
        // So bleibt die Form gut sichtbar. Ausserdem ist der Farbton anders als beim ähnlich gefärbten Normal Bloon darüber,
        // damit man die beiden Stufen nicht verwechselt.
        new Bloon
        {
            Name = "MOAB", Category = "MOAB-Class", Image = "moab_bloon.png", Color = "#1A4D8F",
            Description = "The first MOAB-class blimp, unleashing four Ceramics when destroyed.",
            Rbe = 200, Speed = "25", IsCamo = false, IsLead = false, Layer = 10,
            PopsInto = [new BloonChild { Name = "Ceramic", Count = 4 }]
        },
        new Bloon
        {
            Name = "BFB", Category = "MOAB-Class", Image = "bfb_bloon.png", Color = "#A8241F",
            Description = "A colossal blimp that splits into four MOABs.",
            Rbe = 700, Speed = "6.25", IsCamo = false, IsLead = false, Layer = 11,
            PopsInto = [new BloonChild { Name = "MOAB", Count = 4 }]
        },
        new Bloon
        {
            Name = "ZOMG", Category = "MOAB-Class", Image = "zomg_bloon.png", Color = "#4A6B23",
            Description = "One of the toughest blimps in the game, revealing four BFBs.",
            Rbe = 4000, Speed = "4.5", IsCamo = false, IsLead = false, Layer = 12,
            PopsInto = [new BloonChild { Name = "BFB", Count = 4 }]
        },
        new Bloon
        {
            Name = "DDT", Category = "MOAB-Class", Image = "ddt_bloon.png", Color = "#4A5560",
            Description = "A stealthy, lead, and fast blimp that's tough to pin down.",
            Rbe = 400, Speed = "66", IsCamo = true, IsLead = true, Layer = 12,
            Immunities = ["Immune to Explosions"],
            PopsIntoNote = "Pops into one random weaker bloon layer, similar to Ceramic - the exact result isn't fixed."
        },
        new Bloon
        {
            Name = "BAD", Category = "MOAB-Class", Image = "bad_bloon.png", Color = "#6B1F7A",
            Description = "The final blimp tier - immensely tough and worth defending against.",
            Rbe = 20000, Speed = "4.5", IsCamo = false, IsLead = false, Layer = 13,
            PopsInto = [new BloonChild { Name = "ZOMG", Count = 4 }]
        },
    ];

    // Gleich wie TowersController.Index. Suche und Kategorie kommen aus dem Query String, kein JavaScript nötig.
    public IActionResult Index(string? search, string? category)
    {
        var bloons = AllBloons.AsEnumerable();

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

    // Zum Beispiel /Bloons/Details/red. Die id ist der Slug vom Bloon (siehe Bloon.Slug), nicht der Name.
    // So bleibt die URL lesbar und muss nicht encodiert werden.
    public IActionResult Details(string id)
    {
        var index = AllBloons.FindIndex(b => b.Slug == id);
        if (index == -1)
        {
            return NotFound();
        }

        // Die Pfeile gehen durch die AllBloons Liste in ihrer Reihenfolge, gleich wie bei TowersController.Details.
        // Am Anfang und Ende springt es wieder um.
        var previous = AllBloons[(index - 1 + AllBloons.Count) % AllBloons.Count];
        var next = AllBloons[(index + 1) % AllBloons.Count];
        ViewData["PreviousSlug"] = previous.Slug;
        ViewData["PreviousName"] = previous.Name;
        ViewData["NextSlug"] = next.Slug;
        ViewData["NextName"] = next.Name;

        // Die "Pops Into" Karte braucht Bild und Scale von jedem Kind Bloon.
        // Ein BloonChild speichert aber nur den Namen. Darum wird das Bild hier im Controller gesucht und nicht in der View.
        ViewData["BloonImages"] = AllBloons.ToDictionary(b => b.Name, b => b.Image);
        ViewData["BloonScales"] = AllBloons.ToDictionary(b => b.Name, b => b.ImageScale);

        return View(AllBloons[index]);
    }
}
