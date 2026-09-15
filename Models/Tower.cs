namespace MonkeyArchive.Models;

// One BTD6 tower shown on the /Towers overview page and its own detail page.
public class Tower
{
    public required string Name { get; set; }

    // One of "Primary", "Military", "Magic", "Support" - matches the
    // filter pill values and the --Cat* colour variables in site.css.
    public required string Category { get; set; }

    // File name only (no path); resolved against wwwroot/images/monkeys/ in the view.
    public required string Image { get; set; }

    // Detail-page fields. Base stats are Medium-difficulty, tier 0-0-0.
    public string Description { get; set; } = "";
    public int Cost { get; set; }
    public int Damage { get; set; }
    public int Pierce { get; set; }
    // -1 means the tower's range is unlimited/global (e.g. Sniper Monkey,
    // Dartling Gunner) rather than a real tile-based number.
    public int Range { get; set; }
    public List<TowerUpgradePath> UpgradePaths { get; set; } = [];

    // URL-friendly id used for the detail page route (/Towers/Details/dart-monkey).
    public string Slug => Name.ToLowerInvariant().Replace(" ", "-");
}
