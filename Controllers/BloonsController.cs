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
        //
        // Detail-page fields (RBE/Speed/IsLead/Layer/Immunities) are pulled
        // from the same datamined BTD6 game files used for TowersController
        // (maxHealth, speed, layerNumber, and the bloonProperties bitflag -
        // bit0 = Lead, bit1 = immune to explosions, bit2 = immune to cold,
        // bit3 = immune to energy, confirmed against Lead=1, DDT=3 and
        // Zebra=6). IsCamo is hardcoded per-bloon instead:
        // the JSON's own isCamo flag is unreliable (it reports false even
        // for DDT), so this uses the well-known fact that DDT is the only
        // one of these 17 base types that's inherently Camo - every other
        // colour only becomes Camo as a per-round modifier, not as part of
        // its identity. PopsInto/PopsIntoNote describe the reveal-on-pop
        // chain, which isn't stored in that data source at all (there's no
        // child-bloon mapping anywhere in it) - these instead reflect
        // BTD6's well-established, unchanged-since-launch degrade mechanic
        // (Blue -> Red, MOAB -> 4x Ceramic, etc.), with Ceramic and DDT
        // called out as random reveals rather than a fixed result, since
        // that's genuinely how the game picks their child bloon.
        new Bloon
        {
            Name = "Red", Category = "Normal", Image = "red_bloon.png", Color = "#DC3E3E", ImageScale = 1.98,
            Description = "The weakest bloon - a single hit pops it for good.",
            Rbe = 1, Speed = "25", IsCamo = false, IsLead = false, Layer = 1,
            PopsInto = []
        },
        new Bloon
        {
            Name = "Blue", Category = "Normal", Image = "blue_bloon.png", Color = "#2F80ED", ImageScale = 1.84,
            Description = "A little tougher than Red and slightly faster.",
            Rbe = 1, Speed = "35", IsCamo = false, IsLead = false, Layer = 2,
            PopsInto = [new BloonChild { Name = "Red", Count = 1 }]
        },
        new Bloon
        {
            Name = "Green", Category = "Normal", Image = "green_bloon.png", Color = "#4F9A3C", ImageScale = 1.73,
            Description = "Faster still, popping into a Blue underneath.",
            Rbe = 1, Speed = "45", IsCamo = false, IsLead = false, Layer = 3,
            PopsInto = [new BloonChild { Name = "Blue", Count = 1 }]
        },
        new Bloon
        {
            Name = "Yellow", Category = "Normal", Image = "yellow_bloon.png", Color = "#F2C94C", ImageScale = 0.89,
            Description = "Noticeably quick, outrunning most early towers' fire rate.",
            Rbe = 1, Speed = "80", IsCamo = false, IsLead = false, Layer = 4,
            PopsInto = [new BloonChild { Name = "Green", Count = 1 }]
        },
        new Bloon
        {
            Name = "Pink", Category = "Normal", Image = "pink_bloon.png", Color = "#F2789F", ImageScale = 0.87,
            Description = "The fastest of the basic colours, testing your reaction time.",
            Rbe = 1, Speed = "87.5", IsCamo = false, IsLead = false, Layer = 5,
            PopsInto = [new BloonChild { Name = "Yellow", Count = 1 }]
        },
        // Black/White/Lead/Zebra: the balloon art itself is near-black,
        // near-white, mid-grey and black+white respectively, so matching
        // the background to the same tone makes it disappear. These use a
        // deliberately *different* lightness from the balloon so the
        // silhouette still reads.
        new Bloon
        {
            Name = "Black", Category = "Normal", Image = "black_bloon.png", Color = "#5A5A5A", ImageScale = 2.72,
            Description = "Immune to explosive damage, and splits into two Pinks.",
            Rbe = 1, Speed = "45", IsCamo = false, IsLead = false, Layer = 6,
            Immunities = ["Immune to Explosions"],
            PopsInto = [new BloonChild { Name = "Pink", Count = 2 }]
        },
        new Bloon
        {
            Name = "White", Category = "Normal", Image = "white_bloon.png", Color = "#7C828A", ImageScale = 2.69,
            Description = "Immune to freezing effects, and also splits into two Pinks.",
            Rbe = 1, Speed = "50", IsCamo = false, IsLead = false, Layer = 6,
            Immunities = ["Immune to Cold"],
            PopsInto = [new BloonChild { Name = "Pink", Count = 2 }]
        },
        new Bloon
        {
            Name = "Purple", Category = "Normal", Image = "purple_bloon.png", Color = "#9B51E0", ImageScale = 0.89,
            Description = "Immune to energy and magic damage, popping into a single Pink.",
            Rbe = 1, Speed = "75", IsCamo = false, IsLead = false, Layer = 6,
            Immunities = ["Immune to Energy"],
            PopsInto = [new BloonChild { Name = "Pink", Count = 1 }]
        },
        new Bloon
        {
            Name = "Lead", Category = "Normal", Image = "lead_bloon.png", Color = "#2E3236", ImageScale = 0.88,
            Description = "Immune to sharp damage until popped by explosives or magic.",
            Rbe = 1, Speed = "25", IsCamo = false, IsLead = true, Layer = 7,
            PopsInto = [new BloonChild { Name = "Black", Count = 1 }, new BloonChild { Name = "White", Count = 1 }]
        },
        new Bloon
        {
            Name = "Zebra", Category = "Normal", Image = "zebra_bloon.png", Color = "#808080", ImageScale = 1.74,
            Description = "Combines Black's and White's immunities in one tricky package.",
            Rbe = 1, Speed = "45", IsCamo = false, IsLead = false, Layer = 7,
            Immunities = ["Immune to Explosions", "Immune to Cold"],
            PopsInto = [new BloonChild { Name = "Black", Count = 1 }, new BloonChild { Name = "White", Count = 1 }]
        },
        new Bloon
        {
            Name = "Rainbow", Category = "Normal", Image = "rainbow_bloon.png", Color = "linear-gradient(135deg, #E5484D, #F2994E, #F2C94C, #27AE60, #2F80ED, #9B51E0)", ImageScale = 1.57,
            Description = "A colourful heavyweight that reveals two Zebras when popped.",
            Rbe = 1, Speed = "55", IsCamo = false, IsLead = false, Layer = 8,
            PopsInto = [new BloonChild { Name = "Zebra", Count = 2 }]
        },
        new Bloon
        {
            Name = "Ceramic", Category = "Normal", Image = "ceramic_bloon.png", Color = "#8B4A2B", ImageScale = 0.97,
            Description = "A tough shell hiding a random weaker bloon underneath.",
            Rbe = 10, Speed = "62.5", IsCamo = false, IsLead = false, Layer = 9,
            PopsIntoNote = "Pops into one random weaker bloon - Rainbow, Zebra, Black, White, or Lead. The exact result isn't fixed."
        },

        // MOAB-class colours were originally guessed without looking at the
        // actual art - corrected against what the images really show:
        // MOAB is blue, BFB is red, ZOMG is a dark bomb with neon-green
        // accents, DDT is dark camo, BAD is purple. Each background is
        // still picked to sit apart in lightness from its (mostly dark)
        // bomb body so the shape stays visible, and apart in hue from the
        // similarly-coloured Normal bloon above so the two don't read as
        // the same tier.
        new Bloon
        {
            Name = "MOAB", Category = "MOAB-Class", Image = "moab_bloon.png", Color = "#1A4D8F",
            Description = "The first MOAB-class blimp, unleashing four Ceramics when destroyed.",
            Rbe = 200, Speed = "25", IsCamo = false, IsLead = false, Layer = 10,
            PopsInto = [new BloonChild { Name = "Ceramic", Count = 4 }]
        },
        new Bloon
        {
            Name = "BFB", Category = "MOAB-Class", Image = "bfb_bloon.png", Color = "#A8241F",
            Description = "A colossal blimp that splits into four MOABs.",
            Rbe = 700, Speed = "6.25", IsCamo = false, IsLead = false, Layer = 11,
            PopsInto = [new BloonChild { Name = "MOAB", Count = 4 }]
        },
        new Bloon
        {
            Name = "ZOMG", Category = "MOAB-Class", Image = "zomg_bloon.png", Color = "#4A6B23",
            Description = "One of the toughest blimps in the game, revealing four BFBs.",
            Rbe = 4000, Speed = "4.5", IsCamo = false, IsLead = false, Layer = 12,
            PopsInto = [new BloonChild { Name = "BFB", Count = 4 }]
        },
        new Bloon
        {
            Name = "DDT", Category = "MOAB-Class", Image = "ddt_bloon.png", Color = "#4A5560",
            Description = "A stealthy, lead, and fast blimp that's tough to pin down.",
            Rbe = 400, Speed = "66", IsCamo = true, IsLead = true, Layer = 12,
            Immunities = ["Immune to Explosions"],
            PopsIntoNote = "Pops into one random weaker bloon layer, similar to Ceramic - the exact result isn't fixed."
        },
        new Bloon
        {
            Name = "BAD", Category = "MOAB-Class", Image = "bad_bloon.png", Color = "#6B1F7A",
            Description = "The final blimp tier - immensely tough and worth defending against.",
            Rbe = 20000, Speed = "4.5", IsCamo = false, IsLead = false, Layer = 13,
            PopsInto = [new BloonChild { Name = "ZOMG", Count = 4 }]
        },
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

    // /Bloons/Details/red - id is the bloon's slug (see Bloon.Slug), not its
    // display name, so the URL stays readable without encoding.
    public IActionResult Details(string id)
    {
        var index = AllBloons.FindIndex(b => b.Slug == id);
        if (index == -1)
        {
            return NotFound();
        }

        // Previous/next arrows cycle through AllBloons in list order and
        // wrap around at both ends, same as TowersController.Details.
        var previous = AllBloons[(index - 1 + AllBloons.Count) % AllBloons.Count];
        var next = AllBloons[(index + 1) % AllBloons.Count];
        ViewData["PreviousSlug"] = previous.Slug;
        ViewData["PreviousName"] = previous.Name;
        ViewData["NextSlug"] = next.Slug;
        ViewData["NextName"] = next.Name;

        // The "Pops Into" card needs each child's image/scale to render its
        // icon; child bloons only store a Name, so resolve those here
        // instead of doing the lookup in the view.
        ViewData["BloonImages"] = AllBloons.ToDictionary(b => b.Name, b => b.Image);
        ViewData["BloonScales"] = AllBloons.ToDictionary(b => b.Name, b => b.ImageScale);

        return View(AllBloons[index]);
    }
}
