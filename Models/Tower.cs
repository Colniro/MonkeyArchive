namespace MonkeyArchive.Models;

// Ein BTD6 Tower. Wird auf der /Towers Übersicht und auf der eigenen Detailseite gezeigt.
public class Tower
{
    public required string Name { get; set; }

    // Ist "Primary", "Military", "Magic" oder "Support".
    // Muss zu den Filter Pills und den --Cat* Farben in site.css passen.
    public required string Category { get; set; }

    // Nur der Dateiname, kein Pfad. Der Pfad wird in der View mit wwwroot/images/monkeys/ zusammengebaut.
    public required string Image { get; set; }

    // Felder für die Detailseite. Die Basiswerte gelten für Medium Difficulty, Tier 0-0-0.
    public string Description { get; set; } = "";
    public int Cost { get; set; }
    public int Damage { get; set; }
    public int Pierce { get; set; }
    // -1 heisst die Reichweite ist unendlich, zum Beispiel bei Sniper Monkey oder Dartling Gunner.
    // Das ist keine echte Zahl auf dem Spielfeld.
    public int Range { get; set; }
    public List<TowerUpgradePath> UpgradePaths { get; set; } = [];

    // Id für die URL der Detailseite, zum Beispiel /Towers/Details/dart-monkey.
    public string Slug => Name.ToLowerInvariant().Replace(" ", "-");
}
