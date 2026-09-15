using Microsoft.AspNetCore.Mvc;
using MonkeyArchive.Models;

namespace MonkeyArchive.Controllers;

// Serves the "All Bloons" overview page (/Bloons). Same shape as
// TowersController - fixed game data as an in-memory list, filtered
// server-side by category and search term.
public class BloonsController : Controller
{
    private static readonly List<Bloon> AllBloons =
    [
        // ImageScale compensates for how much transparent padding each
        // source PNG has around the actual balloon (measured via the
        // image's alpha-channel bounding box) - without it, e.g. Yellow
        // (balloon fills ~95% of its canvas) renders visibly larger than
        // Black (~30%) at the same box size. MOAB-class art is already
        // consistent, so those are left at the default 1.0.
        new Bloon { Name = "Red", Category = "Normal", Image = "red_bloon.png", Color = "#DC3E3E", ImageScale = 1.98 },
        new Bloon { Name = "Blue", Category = "Normal", Image = "blue_bloon.png", Color = "#2F80ED", ImageScale = 1.84 },
        new Bloon { Name = "Green", Category = "Normal", Image = "green_bloon.png", Color = "#4F9A3C", ImageScale = 1.73 },
        new Bloon { Name = "Yellow", Category = "Normal", Image = "yellow_bloon.png", Color = "#F2C94C", ImageScale = 0.89 },
        new Bloon { Name = "Pink", Category = "Normal", Image = "pink_bloon.png", Color = "#F2789F", ImageScale = 0.87 },
        // Black/White/Lead/Zebra: the balloon art itself is near-black,
        // near-white, mid-grey and black+white respectively, so matching
        // the background to the same tone makes it disappear. These use a
        // deliberately *different* lightness from the balloon so the
        // silhouette still reads.
        new Bloon { Name = "Black", Category = "Normal", Image = "black_bloon.png", Color = "#5A5A5A", ImageScale = 2.72 },
        new Bloon { Name = "White", Category = "Normal", Image = "white_bloon.png", Color = "#7C828A", ImageScale = 2.69 },
        new Bloon { Name = "Purple", Category = "Normal", Image = "purple_bloon.png", Color = "#9B51E0", ImageScale = 0.89 },
        new Bloon { Name = "Lead", Category = "Normal", Image = "lead_bloon.png", Color = "#2E3236", ImageScale = 0.88 },
        new Bloon { Name = "Zebra", Category = "Normal", Image = "zebra_bloon.png", Color = "#808080", ImageScale = 1.74 },
        new Bloon { Name = "Rainbow", Category = "Normal", Image = "rainbow_bloon.png", Color = "linear-gradient(135deg, #E5484D, #F2994E, #F2C94C, #27AE60, #2F80ED, #9B51E0)", ImageScale = 1.57 },
        new Bloon { Name = "Ceramic", Category = "Normal", Image = "ceramic_bloon.png", Color = "#8B4A2B", ImageScale = 0.97 },

        // MOAB-class colours were originally guessed without looking at the
        // actual art - corrected against what the images really show:
        // MOAB is blue, BFB is red, ZOMG is a dark bomb with neon-green
        // accents, DDT is dark camo, BAD is purple. Each background is
        // still picked to sit apart in lightness from its (mostly dark)
        // bomb body so the shape stays visible, and apart in hue from the
        // similarly-coloured Normal bloon above so the two don't read as
        // the same tier.
        new Bloon { Name = "MOAB", Category = "MOAB-Class", Image = "moab_bloon.png", Color = "#1A4D8F" },
        new Bloon { Name = "BFB", Category = "MOAB-Class", Image = "bfb_bloon.png", Color = "#A8241F" },
        new Bloon { Name = "ZOMG", Category = "MOAB-Class", Image = "zomg_bloon.png", Color = "#4A6B23" },
        new Bloon { Name = "DDT", Category = "MOAB-Class", Image = "ddt_bloon.png", Color = "#4A5560" },
        new Bloon { Name = "BAD", Category = "MOAB-Class", Image = "bad_bloon.png", Color = "#6B1F7A" },
    ];

    // Mirrors TowersController.Index: search and category filtering both
    // driven by the query string, no JavaScript required to render results.
    public IActionResult Index(string? search, string? category)
    {
        var bloons = AllBloons.AsEnumerable();

        var selectedCategory = string.IsNullOrWhiteSpace(category) ? "All" : category;
        if (selectedCategory != "All")
        {
            bloons = bloons.Where(b => b.Category == selectedCategory);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            bloons = bloons.Where(b => b.Name.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        var model = new BloonsOverviewViewModel
        {
            Bloons = bloons.ToList(),
            Search = search ?? string.Empty,
            SelectedCategory = selectedCategory
        };

        return View(model);
    }
}
