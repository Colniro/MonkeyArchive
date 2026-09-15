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

    // The source PNGs crop their balloon art to wildly different amounts of
    // transparent padding (some fill ~95% of the canvas, others ~30%), so
    // at a shared display size they'd look inconsistently sized. This
    // scales the rendered image to even that out - see BloonsController
    // for how the values were measured.
    public double ImageScale { get; set; } = 1.0;
}
