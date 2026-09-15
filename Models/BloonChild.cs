namespace MonkeyArchive.Models;

// One entry in a Bloon's PopsInto list, e.g. Ceramic -> 3x Rainbow.
public class BloonChild
{
    // Matches another Bloon's Name, so the detail page can look up its
    // image to render an icon alongside the count.
    public required string Name { get; set; }
    public required int Count { get; set; }
}
