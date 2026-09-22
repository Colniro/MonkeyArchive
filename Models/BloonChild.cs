namespace MonkeyArchive.Models;

// Ein Eintrag in der PopsInto Liste eines Bloons, zum Beispiel Ceramic wird zu 3x Rainbow.
public class BloonChild
{
    // Muss gleich heissen wie der Name eines anderen Bloons.
    // So kann die Detailseite das passende Bild finden und neben der Anzahl zeigen.
    public required string Name { get; set; }
    public required int Count { get; set; }
}
