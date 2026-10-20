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
    // null heisst die Reichweite ist unendlich, zum Beispiel bei Sniper Monkey oder Dartling Gunner.
    // Das ist keine echte Zahl auf dem Spielfeld. So steht es auch in der DB Spalte Range.
    public int? Range { get; set; }
    public List<TowerUpgradePath> UpgradePaths { get; set; } = [];

    // Id für die URL der Detailseite, zum Beispiel /Towers/Details/dart-monkey.
    // Kommt direkt aus der Slug Spalte in der DB, wird nicht mehr aus dem Namen berechnet.
    public required string Slug { get; set; }
}
