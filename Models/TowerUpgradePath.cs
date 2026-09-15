namespace MonkeyArchive.Models;

// One maxed (Tier 5) upgrade path shown on a tower's detail page. Every
// BTD6 tower has exactly three paths (top/middle/bottom); this represents
// one of them at its final tier.
public class TowerUpgradePath
{
    public required string PathLabel { get; set; } // "Top Path", "Middle Path", "Bottom Path"
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required int Price { get; set; }
}
