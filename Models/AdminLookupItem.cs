namespace MonkeyArchive.Models;

// Ein Eintrag aus einer Lookup Tabelle (TowerCategory, BloonCategory) für die
// Dropdowns in den Admin Formularen.
public class AdminLookupItem
{
    public int Id { get; set; }
    public required string Name { get; set; }
}
