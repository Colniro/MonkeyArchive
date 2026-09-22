namespace MonkeyArchive.Models;

// Ein maximierter Pfad (Tier 5) auf der Detailseite eines Towers.
// Jeder BTD6 Tower hat genau drei Pfade (oben, mitte, unten). Das hier ist einer davon auf der letzten Stufe.
public class TowerUpgradePath
{
    public required string PathLabel { get; set; } // "Top Path", "Middle Path", "Bottom Path"
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required int Price { get; set; }
}
