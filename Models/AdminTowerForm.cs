namespace MonkeyArchive.Models;

// Formular für /Admin/EditTower. Hat bewusst alle Spalten der Tower Tabelle
// drin und nicht nur die, die auf der öffentlichen Detailseite auftauchen.
// Das Model Tower reicht hier nicht: dort fehlen ID, CategoryId und GameVersion,
// und Category ist dort der Name statt der Fremdschlüssel.
public class AdminTowerForm
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public string Slug { get; set; } = "";
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string ImageFileName { get; set; } = "";
    public int Cost { get; set; }
    public int Damage { get; set; }
    public int Pierce { get; set; }

    // null heisst unendliche Reichweite, zum Beispiel Sniper Monkey. Das Feld
    // bleibt im Formular dann einfach leer.
    public int? Range { get; set; }
    public string? GameVersion { get; set; }

    // Für das Kategorie Dropdown. Kommt aus der TowerCategory Tabelle, ist also
    // nicht fest im Code und wächst mit, wenn eine Kategorie dazukommt.
    public List<AdminLookupItem> Categories { get; set; } = [];

    // Nur für die Liste auf /Admin/Towers gefüllt. Beide Zahlen hängen per
    // Fremdschlüssel an diesem Tower und verschwinden beim Löschen mit. Darum
    // stehen sie in der Rückfrage vor dem Löschen, damit man vorher sieht,
    // was alles weggeht.
    public int FavoriteCount { get; set; }
    public int UpgradePathCount { get; set; }
}
