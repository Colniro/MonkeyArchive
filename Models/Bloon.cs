namespace MonkeyArchive.Models;

// One BTD6 bloon shown on the /Bloons overview page and its own detail page.
public class Bloon
{
    public required string Name { get; set; }

    // "Normal" or "MOAB-Class" - matches the filter pill values.
    public required string Category { get; set; }

    // File name only (no path); resolved against wwwroot/images/bloons/ in the view.
    public required string Image { get; set; }

    // Bloons don't share four category colours like towers do - each one
    // is its own colour, so it travels on the model instead of being
    // computed from Category in the view.
    public required string Color { get; set; }

    // The source PNGs crop their balloon art to wildly different amounts of
    // transparent padding (some fill ~95% of the canvas, others ~30%), so
    // at a shared display size they'd look inconsistently sized. This
    // scales the rendered image to even that out - see BloonsController
    // for how the values were measured.
    public double ImageScale { get; set; } = 1.0;

    // Detail-page fields.
    public string Description { get; set; } = "";
    public int Rbe { get; set; }
    public string Speed { get; set; } = "";
    public bool IsCamo { get; set; }
    public bool IsLead { get; set; }

    // This bloon's position in BTD6's fixed layer order (Red=1 ... BAD=13),
    // straight from the datamined layerNumber field - several bloons share
    // a number (Black/White/Purple=6, Lead/Zebra=7, DDT/ZOMG=12) because
    // that's genuinely how the game orders them, not a display bug.
    public int Layer { get; set; }

    // Damage immunities beyond Lead (which gets its own tag) - e.g. Black
    // is immune to explosions. Empty for bloons with none.
    public List<string> Immunities { get; set; } = [];

    // Empty for the weakest layer (Red) - popping it just removes the bloon.
    public List<BloonChild> PopsInto { get; set; } = [];

    // Set instead of PopsInto for bloons whose reveal isn't a fixed chain
    // (Ceramic and DDT pop into one random weaker layer in the real game,
    // not one specific result) - shown as plain text instead of the icon row.
    public string? PopsIntoNote { get; set; }

    // URL-friendly id used for the detail page route (/Bloons/Details/red).
    public string Slug => Name.ToLowerInvariant().Replace(" ", "-");

    // Detail-page hero background, derived from this bloon's own accent
    // colour rather than a shared category palette (see Color above).
    // Rainbow's Color is already a multi-stop CSS gradient string, not a
    // single hex value, so it's used as-is instead of being blended.
    public string HeroBackground => Color.StartsWith("linear-gradient", StringComparison.Ordinal)
        ? Color
        : $"linear-gradient(120deg, {Blend(Color, 255, 0.22)}, {Blend(Color, 0, 0.4)})";

    private static string Blend(string hex, int target, double amount)
    {
        hex = hex.TrimStart('#');
        var r = Convert.ToInt32(hex[..2], 16);
        var g = Convert.ToInt32(hex[2..4], 16);
        var b = Convert.ToInt32(hex[4..6], 16);
        r += (int)((target - r) * amount);
        g += (int)((target - g) * amount);
        b += (int)((target - b) * amount);
        return $"#{r:X2}{g:X2}{b:X2}";
    }
}
