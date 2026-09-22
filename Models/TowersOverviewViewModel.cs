namespace MonkeyArchive.Models;

// Gibt die gefilterte Tower Liste an die View weiter. Trägt auch Suchtext und Kategorie mit.
// So bleiben Suchfeld und Filter Pill nach einem Reload gleich wie vorher.
public class TowersOverviewViewModel
{
    public required List<Tower> Towers { get; set; }
    public required string Search { get; set; }
    public required string SelectedCategory { get; set; }
}
