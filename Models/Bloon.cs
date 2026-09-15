namespace MonkeyArchive.Models;

// One BTD6 bloon shown on the /Bloons overview page.
public class Bloon
{
    public required string Name { get; set; }

    // "Normal" or "MOAB-Class" - matches the filter pill values.
    public required string Category { get; set; }

    // File name only (no path); resolved against wwwroot/images/bloons/ in the view.
    public required string Image { get; set; }

    // Bloons don't share four category colours like towers do - each one
    // is its own colour, so it travels on the model instead of being
    // computed from Category in the view.
    public required string Color { get; set; }
}
