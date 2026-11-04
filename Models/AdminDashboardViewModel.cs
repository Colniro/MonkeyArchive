namespace MonkeyArchive.Models;

// Alles, was die Admin Startseite (/Admin) anzeigt.
// Wird in AdminController.Index() mit wenigen Abfragen gefüllt.
public class AdminDashboardViewModel
{
    // Infos über die Verbindung selbst, damit man sieht, auf welcher Datenbank
    // man gerade arbeitet. Praktisch wenn es eine Test und eine echte DB gibt.
    public required string ServerVersion { get; set; }
    public required string DatabaseName { get; set; }

    public int UserCount { get; set; }
    public int TowerCount { get; set; }
    public int BloonCount { get; set; }
    public int FavoriteCount { get; set; }

    // Zeilenzahl pro Tabelle. Exakt gezählt mit COUNT(*), nicht die geschätzten
    // Werte aus information_schema (die stimmen bei InnoDB nicht).
    public List<AdminTableInfo> Tables { get; set; } = [];

    // Die beliebtesten Towers über alle Accounts. Zeigt, was die Nutzer wirklich benutzen.
    public List<AdminFavoriteStat> TopFavorites { get; set; } = [];

    // Die zuletzt registrierten Accounts. Die User Tabelle hat keine Spalte mit
    // Datum, darum ist die Reihenfolge über die ID (AUTO_INCREMENT) bestimmt.
    public List<AdminUser> NewestUsers { get; set; } = [];
}

public class AdminTableInfo
{
    public required string Name { get; set; }
    public int Rows { get; set; }
}

public class AdminFavoriteStat
{
    public required string Name { get; set; }
    public required string Slug { get; set; }
    public required string Image { get; set; }
    public required string Category { get; set; }
    public int Count { get; set; }
}
