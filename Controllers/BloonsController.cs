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
        new Bloon { Name = "Red", Category = "Normal", Image = "red_bloon.png", Color = "#DC3E3E" },
        new Bloon { Name = "Blue", Category = "Normal", Image = "blue_bloon.png", Color = "#2F80ED" },
        new Bloon { Name = "Green", Category = "Normal", Image = "green_bloon.png", Color = "#27AE60" },
        new Bloon { Name = "Yellow", Category = "Normal", Image = "yellow_bloon.png", Color = "#F2C94C" },
        new Bloon { Name = "Pink", Category = "Normal", Image = "pint_bloon.png", Color = "#F2789F" },
        new Bloon { Name = "Black", Category = "Normal", Image = "black_bloon.png", Color = "#2C2C2C" },
        new Bloon { Name = "White", Category = "Normal", Image = "white_bloon.png", Color = "#D9D9D9" },
        new Bloon { Name = "Purple", Category = "Normal", Image = "purple_bloon.png", Color = "#9B51E0" },
        new Bloon { Name = "Lead", Category = "Normal", Image = "lead_bloon.png", Color = "#6D7278" },
        new Bloon { Name = "Zebra", Category = "Normal", Image = "zebra_bloon.png", Color = "#1A1A1A" },
        new Bloon { Name = "Rainbow", Category = "Normal", Image = "raibow_bloon.png", Color = "linear-gradient(135deg, #E5484D, #F2994E, #F2C94C, #27AE60, #2F80ED, #9B51E0)" },
        new Bloon { Name = "Ceramic", Category = "Normal", Image = "ceramic_bloon.png", Color = "#8B4A2B" },

        new Bloon { Name = "MOAB", Category = "MOAB-Class", Image = "moab_bloon.png", Color = "#A9AFB8" },
        new Bloon { Name = "BFB", Category = "MOAB-Class", Image = "bfb_bloon.png", Color = "#C77DC9" },
        new Bloon { Name = "ZOMG", Category = "MOAB-Class", Image = "zomg_bloon.png", Color = "#7A1E1E" },
        new Bloon { Name = "DDT", Category = "MOAB-Class", Image = "ddt_bloon.png", Color = "#3D2B56" },
        new Bloon { Name = "BAD", Category = "MOAB-Class", Image = "bad_bloon.png", Color = "#1F1F2E" },
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
