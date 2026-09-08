using Microsoft.AspNetCore.Mvc;
using MonkeyArchive.Models;

namespace MonkeyArchive.Controllers;

// Serves the "All Towers" overview page (/Towers). The tower list is fixed
// game data rather than something users create, so it lives here as a plain
// in-memory collection instead of coming from a database.
public class TowersController : Controller
{
    private static readonly List<Tower> AllTowers =
    [
        new Tower { Name = "Dart Monkey", Category = "Primary", Image = "dart_monkey.png" },
        new Tower { Name = "Boomerang Monkey", Category = "Primary", Image = "boomerang_monkey.png" },
        new Tower { Name = "Bomb Shooter", Category = "Primary", Image = "canon_monkey.png" },
        new Tower { Name = "Tack Shooter", Category = "Primary", Image = "tac_shooter_monkey.png" },
        new Tower { Name = "Ice Monkey", Category = "Primary", Image = "ice_monkey.png" },
        new Tower { Name = "Glue Gunner", Category = "Primary", Image = "glue_gunner_monkey.png" },

        new Tower { Name = "Sniper Monkey", Category = "Military", Image = "sniper_monkey.png" },
        new Tower { Name = "Monkey Ace", Category = "Military", Image = "monkey_ace_monkey.png" },
        new Tower { Name = "Heli Pilot", Category = "Military", Image = "heli_pilot_monkey.png" },
        new Tower { Name = "Mortar Monkey", Category = "Military", Image = "mortar_monkey.png" },
        new Tower { Name = "Dartling Gunner", Category = "Military", Image = "dartlin_gunner_monkey.png" },

        new Tower { Name = "Wizard Monkey", Category = "Magic", Image = "wizard_monkey.png" },
        new Tower { Name = "Super Monkey", Category = "Magic", Image = "super_monkey_monkey.png" },
        new Tower { Name = "Ninja Monkey", Category = "Magic", Image = "ninja_monkey.png" },
        new Tower { Name = "Alchemist", Category = "Magic", Image = "alchemist_monkey.png" },
        new Tower { Name = "Druid", Category = "Magic", Image = "druid_monkey.png" },
        new Tower { Name = "Mermonkey", Category = "Magic", Image = "mermaid_monkey.png" },

        new Tower { Name = "Banana Farm", Category = "Support", Image = "banana_monkey.png" },
        new Tower { Name = "Spike Factory", Category = "Support", Image = "spice_factory_monkey.png" },
        new Tower { Name = "Monkey Village", Category = "Support", Image = "village.png" },
        new Tower { Name = "Engineer Monkey", Category = "Support", Image = "engineer_monkey.png" },
        new Tower { Name = "Beast Handler", Category = "Support", Image = "beast_handler_monkey.png" },
    ];

    // Search and category filtering both happen here, driven purely by the
    // query string (?search=...&category=...). The page needs no JavaScript:
    // the search form and the filter pills are plain GET links/forms that
    // reload this action with different parameters.
    public IActionResult Index(string? search, string? category)
    {
        var towers = AllTowers.AsEnumerable();

        var selectedCategory = string.IsNullOrWhiteSpace(category) ? "All" : category;
        if (selectedCategory != "All")
        {
            towers = towers.Where(t => t.Category == selectedCategory);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            towers = towers.Where(t => t.Name.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        var model = new TowersOverviewViewModel
        {
            Towers = towers.ToList(),
            Search = search ?? string.Empty,
            SelectedCategory = selectedCategory
        };

        return View(model);
    }
}
