namespace MonkeyArchive.Models;

// Gibt die gefilterte Bloon Liste an die View weiter. Trägt auch Suchtext und Kategorie mit.
// So bleiben Suchfeld und Filter Pill nach einem Reload gleich wie vorher.
public class BloonsOverviewViewModel
{
    public required List<Bloon> Bloons { get; set; }
    public required string Search { get; set; }
    public required string SelectedCategory { get; set; }
}
