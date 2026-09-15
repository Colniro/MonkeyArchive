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
        // Base stats and Tier 5 upgrade prices below are Medium-difficulty
        // values pulled from current datamined BTD6 game files (not the wiki,
        // which was unreachable), so they reflect the live game rather than
        // a possibly outdated reference image.
        new Tower
        {
            Name = "Dart Monkey", Category = "Primary", Image = "dart_monkey.png",
            Description = "Throws a single dart at nearby Bloons. Short range and low pierce but cheap.",
            Cost = 200, Damage = 1, Pierce = 2, Range = 32,
            UpgradePaths =
            [
                new TowerUpgradePath { PathLabel = "Top Path", Name = "Ultra-Juggernaut", Description = "Hurls a massive spiked Juggernaut that splits into two more on impact.", Price = 15000 },
                new TowerUpgradePath { PathLabel = "Middle Path", Name = "Plasma Monkey Fan Club", Description = "Turns all nearby Dart Monkeys into Plasma Monkeys with boosted stats.", Price = 45000 },
                new TowerUpgradePath { PathLabel = "Bottom Path", Name = "Crossbow Master", Description = "Fires a devastating bolt with huge damage, pierce, and range.", Price = 21500 },
            ]
        },
        new Tower
        {
            Name = "Boomerang Monkey", Category = "Primary", Image = "boomerang_monkey.png",
            Description = "Throws a boomerang that hits Bloons on the way out and the way back.",
            Cost = 315, Damage = 1, Pierce = 4, Range = 43,
            UpgradePaths =
            [
                new TowerUpgradePath { PathLabel = "Top Path", Name = "Glaive Lord", Description = "Summons orbiting glaives and rains down glaives across the whole screen.", Price = 32500 },
                new TowerUpgradePath { PathLabel = "Middle Path", Name = "Perma Charge", Description = "A permanent supercharged boomerang with huge pierce and damage.", Price = 35000 },
                new TowerUpgradePath { PathLabel = "Bottom Path", Name = "MOAB Domination", Description = "Massively boosts damage against MOAB-class Bloons.", Price = 50000 },
            ]
        },
        new Tower
        {
            Name = "Bomb Shooter", Category = "Primary", Image = "canon_monkey.png",
            Description = "Lobs an explosive shell that pops a cluster of Bloons in an area.",
            Cost = 375, Damage = 1, Pierce = 22, Range = 40,
            UpgradePaths =
            [
                new TowerUpgradePath { PathLabel = "Top Path", Name = "Bloon Crush", Description = "Crushes nearby Bloons in a huge shockwave, stunning MOAB-class Bloons.", Price = 55000 },
                new TowerUpgradePath { PathLabel = "Middle Path", Name = "MOAB Eliminator", Description = "Fires missiles that deal heavy bonus damage to MOAB-class Bloons.", Price = 26000 },
                new TowerUpgradePath { PathLabel = "Bottom Path", Name = "Bomb Blitz", Description = "Rapidly fires a barrage of explosive shells.", Price = 30000 },
            ]
        },
        new Tower
        {
            Name = "Tack Shooter", Category = "Primary", Image = "tac_shooter_monkey.png",
            Description = "Fires 8 tacks in every direction at once, great for close-range pops.",
            Cost = 260, Damage = 1, Pierce = 1, Range = 23,
            UpgradePaths =
            [
                new TowerUpgradePath { PathLabel = "Top Path", Name = "Inferno Ring", Description = "Surrounds the tower in a ring of fire that continuously burns Bloons.", Price = 45500 },
                new TowerUpgradePath { PathLabel = "Middle Path", Name = "Super Maelstrom", Description = "Launches a devastating vortex of blades that pulls in and shreds Bloons.", Price = 15000 },
                new TowerUpgradePath { PathLabel = "Bottom Path", Name = "The Tack Zone", Description = "Fires a huge number of tacks in every direction at once.", Price = 20000 },
            ]
        },
        new Tower
        {
            Name = "Ice Monkey", Category = "Primary", Image = "ice_monkey.png",
            Description = "Freezes nearby Bloons solid, stopping them and popping Frozen and Lead.",
            Cost = 400, Damage = 1, Pierce = 40, Range = 20,
            UpgradePaths =
            [
                new TowerUpgradePath { PathLabel = "Top Path", Name = "Super Brittle", Description = "Bloons hit by ice take massively increased damage from all sources.", Price = 28000 },
                new TowerUpgradePath { PathLabel = "Middle Path", Name = "Absolute Zero", Description = "Freezes every Bloon on screen and briefly stuns MOAB-class Bloons.", Price = 21000 },
                new TowerUpgradePath { PathLabel = "Bottom Path", Name = "Icicle Impale", Description = "Fires sharp icicles that pierce through many Bloons.", Price = 30000 },
            ]
        },
        new Tower
        {
            Name = "Glue Gunner", Category = "Primary", Image = "glue_gunner_monkey.png",
            Description = "Coats a Bloon in glue, slowing it down and weakening it over time.",
            Cost = 225, Damage = 1, Pierce = 1, Range = 46,
            UpgradePaths =
            [
                new TowerUpgradePath { PathLabel = "Top Path", Name = "The Bloon Solver", Description = "Glue does heavy damage over time and dissolves Bloons completely.", Price = 22500 },
                new TowerUpgradePath { PathLabel = "Middle Path", Name = "Glue Storm", Description = "Covers the entire screen in glue, slowing every Bloon.", Price = 16000 },
                new TowerUpgradePath { PathLabel = "Bottom Path", Name = "Super Glue", Description = "Thicker glue that deals extra damage over a longer duration.", Price = 24000 },
            ]
        },

        new Tower
        {
            Name = "Sniper Monkey", Category = "Military", Image = "sniper_monkey.png",
            Description = "Snipes a single Bloon anywhere on screen with a precise, powerful shot.",
            Cost = 350, Damage = 2, Pierce = 1, Range = -1,
            UpgradePaths =
            [
                new TowerUpgradePath { PathLabel = "Top Path", Name = "Cripple MOAB", Description = "Shots against MOAB-class Bloons massively slow and weaken them.", Price = 32000 },
                new TowerUpgradePath { PathLabel = "Middle Path", Name = "Elite Sniper", Description = "Faster firing and bonus damage against Ceramic Bloons.", Price = 12000 },
                new TowerUpgradePath { PathLabel = "Bottom Path", Name = "Elite Defender", Description = "Automatically destroys any Bloon that gets too close to the exit.", Price = 14900 },
            ]
        },
        new Tower
        {
            Name = "Monkey Ace", Category = "Military", Image = "monkey_ace_monkey.png",
            Description = "Flies over the track dropping darts, with adjustable patrol routes.",
            Cost = 800, Damage = 1, Pierce = 5, Range = 22,
            UpgradePaths =
            [
                new TowerUpgradePath { PathLabel = "Top Path", Name = "Sky Shredder", Description = "Fires a relentless spray of darts while flying over the track.", Price = 42500 },
                new TowerUpgradePath { PathLabel = "Middle Path", Name = "Tsar Bomba", Description = "Drops a single massive bomb that obliterates everything in a huge blast.", Price = 26000 },
                new TowerUpgradePath { PathLabel = "Bottom Path", Name = "Flying Fortress", Description = "A heavily armed plane that fires missiles and razor darts constantly.", Price = 90000 },
            ]
        },
        new Tower
        {
            Name = "Heli Pilot", Category = "Military", Image = "heli_pilot_monkey.png",
            Description = "A mobile chopper that can be flown anywhere on the map to intercept Bloons.",
            Cost = 1500, Damage = 1, Pierce = 3, Range = 22,
            UpgradePaths =
            [
                new TowerUpgradePath { PathLabel = "Top Path", Name = "Apache Prime", Description = "A powerful gunship with missiles that deal heavy damage to MOAB-class Bloons.", Price = 45000 },
                new TowerUpgradePath { PathLabel = "Middle Path", Name = "Special Poperations", Description = "Fires a special weapon with excellent all-round popping power.", Price = 30000 },
                new TowerUpgradePath { PathLabel = "Bottom Path", Name = "Comanche Commander", Description = "A faster, more heavily armed chopper with improved missiles.", Price = 35000 },
            ]
        },
        new Tower
        {
            Name = "Mortar Monkey", Category = "Military", Image = "mortar_monkey.png",
            Description = "Lobs a shell to a targeted spot on the map, popping Bloons in a blast radius.",
            Cost = 600, Damage = 2, Pierce = 25, Range = 30,
            UpgradePaths =
            [
                new TowerUpgradePath { PathLabel = "Top Path", Name = "The Biggest One", Description = "Fires a massive shell that devastates a huge area on impact.", Price = 36000 },
                new TowerUpgradePath { PathLabel = "Middle Path", Name = "Pop and Awe", Description = "Shells burn Bloons and deal heavy bonus damage to MOAB-class Bloons.", Price = 38000 },
                new TowerUpgradePath { PathLabel = "Bottom Path", Name = "Blooncineration", Description = "Shells ignite the ground, continuously burning Bloons that pass through.", Price = 40000 },
            ]
        },
        new Tower
        {
            Name = "Dartling Gunner", Category = "Military", Image = "dartlin_gunner_monkey.png",
            Description = "A rapid-fire turret that can be aimed anywhere on screen.",
            Cost = 850, Damage = 1, Pierce = 1, Range = -1,
            UpgradePaths =
            [
                new TowerUpgradePath { PathLabel = "Top Path", Name = "Ray of Doom", Description = "Fires a continuous, devastating laser beam that melts through Bloons.", Price = 75000 },
                new TowerUpgradePath { PathLabel = "Middle Path", Name = "M.A.D", Description = "Fires a barrage of missiles that deal heavy damage to MOAB-class Bloons.", Price = 65000 },
                new TowerUpgradePath { PathLabel = "Bottom Path", Name = "Bloon Exclusion Zone", Description = "An energy field that pushes back and damages nearby Bloons.", Price = 58000 },
            ]
        },

        new Tower
        {
            Name = "Wizard Monkey", Category = "Magic", Image = "wizard_monkey.png",
            Description = "Hurls bolts of magic at Bloons, popping Lead without help.",
            Cost = 250, Damage = 1, Pierce = 3, Range = 40,
            UpgradePaths =
            [
                new TowerUpgradePath { PathLabel = "Top Path", Name = "Archmage", Description = "Summons powerful magic hands that repeatedly slam nearby Bloons.", Price = 32000 },
                new TowerUpgradePath { PathLabel = "Middle Path", Name = "Wizard Lord Phoenix", Description = "Commands a phoenix that burns Bloons and revives fallen wizards.", Price = 50000 },
                new TowerUpgradePath { PathLabel = "Bottom Path", Name = "Soulbind", Description = "Curses Bloons so that popping one damages others nearby.", Price = 26500 },
            ]
        },
        new Tower
        {
            Name = "Super Monkey", Category = "Magic", Image = "super_monkey_monkey.png",
            Description = "Fires a rapid stream of darts with great range, damage, and speed.",
            Cost = 2500, Damage = 1, Pierce = 1, Range = 50,
            UpgradePaths =
            [
                new TowerUpgradePath { PathLabel = "Top Path", Name = "True Sun God", Description = "A godlike being that fires devastating sun blades and boosts nearby Super Monkeys.", Price = 500000 },
                new TowerUpgradePath { PathLabel = "Middle Path", Name = "The Anti-Bloon", Description = "A being of pure power that destroys everything in its path.", Price = 70000 },
                new TowerUpgradePath { PathLabel = "Bottom Path", Name = "Legend of the Night", Description = "A cloaked hero that fires piercing energy waves across the screen.", Price = 165650 },
            ]
        },
        new Tower
        {
            Name = "Ninja Monkey", Category = "Magic", Image = "ninja_monkey.png",
            Description = "Throws fast, silent shurikens and can spot Camo Bloons.",
            Cost = 400, Damage = 1, Pierce = 2, Range = 40,
            UpgradePaths =
            [
                new TowerUpgradePath { PathLabel = "Top Path", Name = "Grandmaster Ninja", Description = "Master of throwing stars, striking with blinding speed and precision.", Price = 35000 },
                new TowerUpgradePath { PathLabel = "Middle Path", Name = "Grand Saboteur", Description = "Plants bombs on Bloons that detonate and disable MOAB-class Bloons.", Price = 22000 },
                new TowerUpgradePath { PathLabel = "Bottom Path", Name = "Master Bomber", Description = "Throws explosive shurikens that deal heavy area damage.", Price = 40000 },
            ]
        },
        new Tower
        {
            Name = "Alchemist", Category = "Magic", Image = "alchemist_monkey.png",
            Description = "Brews potions that buff nearby towers or corrode Bloons with acid.",
            Cost = 550, Damage = 1, Pierce = 1, Range = 45,
            UpgradePaths =
            [
                new TowerUpgradePath { PathLabel = "Top Path", Name = "Permanent Brew", Description = "A permanent, powerful buff that boosts all nearby towers' damage and speed.", Price = 48000 },
                new TowerUpgradePath { PathLabel = "Middle Path", Name = "Total Transformation", Description = "Instantly turns a Bloon into a much weaker one.", Price = 45000 },
                new TowerUpgradePath { PathLabel = "Bottom Path", Name = "Bloon Master Alchemist", Description = "Acid flasks corrode Bloons and deal heavy damage over time.", Price = 40000 },
            ]
        },
        new Tower
        {
            Name = "Druid", Category = "Magic", Image = "druid_monkey.png",
            Description = "Channels the power of nature to strike Bloons with thorns and lightning.",
            Cost = 400, Damage = 1, Pierce = 1, Range = 35,
            UpgradePaths =
            [
                new TowerUpgradePath { PathLabel = "Top Path", Name = "Superstorm", Description = "Summons a raging storm that strikes Bloons with lightning and rain.", Price = 60000 },
                new TowerUpgradePath { PathLabel = "Middle Path", Name = "Spirit of the Forest", Description = "A powerful nature spirit that buffs nearby towers and damages Bloons.", Price = 35000 },
                new TowerUpgradePath { PathLabel = "Bottom Path", Name = "Avatar of Wrath", Description = "A fiery avatar that hurls flaming boulders at Bloons.", Price = 45000 },
            ]
        },
        new Tower
        {
            Name = "Mermonkey", Category = "Magic", Image = "mermaid_monkey.png",
            Description = "An aquatic support monkey that buffs nearby towers, strongest near water.",
            Cost = 300, Damage = 2, Pierce = 2, Range = 28,
            UpgradePaths =
            [
                new TowerUpgradePath { PathLabel = "Top Path", Name = "Lord of the Abyss", Description = "Summons sea creatures and buffs nearby towers with the power of the deep.", Price = 23000 },
                new TowerUpgradePath { PathLabel = "Middle Path", Name = "Popseidon", Description = "A powerful trident strike that deals heavy damage across the water.", Price = 52000 },
                new TowerUpgradePath { PathLabel = "Bottom Path", Name = "The Final Harmonic", Description = "A resonant song that boosts and empowers all nearby towers.", Price = 25000 },
            ]
        },

        new Tower
        {
            Name = "Banana Farm", Category = "Support", Image = "banana_farm_monkey.png",
            Description = "Grows bananas over time that can be collected for extra income.",
            Cost = 1250, Damage = 0, Pierce = 0, Range = 40,
            UpgradePaths =
            [
                new TowerUpgradePath { PathLabel = "Top Path", Name = "Banana Central", Description = "A massive banana processing facility that generates huge amounts of cash.", Price = 115000 },
                new TowerUpgradePath { PathLabel = "Middle Path", Name = "Monkey-Nomics", Description = "Generates a steady, powerful stream of passive income.", Price = 100000 },
                new TowerUpgradePath { PathLabel = "Bottom Path", Name = "Monkey Wall Street", Description = "Invests bananas for a chance at a huge cash payout.", Price = 70000 },
            ]
        },
        new Tower
        {
            Name = "Spike Factory", Category = "Support", Image = "spike_factory_monkey.png",
            Description = "Produces piles of spikes on the track that pop passing Bloons.",
            Cost = 1000, Damage = 1, Pierce = 5, Range = 34,
            UpgradePaths =
            [
                new TowerUpgradePath { PathLabel = "Top Path", Name = "Super Mines", Description = "Deploys powerful mines that deal massive damage to MOAB-class Bloons.", Price = 125000 },
                new TowerUpgradePath { PathLabel = "Middle Path", Name = "Carpet of Spikes", Description = "Covers the entire track in a thick carpet of spikes.", Price = 41000 },
                new TowerUpgradePath { PathLabel = "Bottom Path", Name = "Perma-Spike", Description = "Produces spikes that never wear out and pop more Bloons.", Price = 30000 },
            ]
        },
        new Tower
        {
            Name = "Monkey Village", Category = "Support", Image = "village.png",
            Description = "Boosts the range, pierce, or attack speed of nearby towers.",
            Cost = 1200, Damage = 0, Pierce = 0, Range = 40,
            UpgradePaths =
            [
                new TowerUpgradePath { PathLabel = "Top Path", Name = "Primary Expertise", Description = "Massively boosts the power of all Primary monkeys in range.", Price = 25000 },
                new TowerUpgradePath { PathLabel = "Middle Path", Name = "Homeland Defense", Description = "Automatically attacks Bloons that get too close to the exit.", Price = 40000 },
                new TowerUpgradePath { PathLabel = "Bottom Path", Name = "Monkeyopolis", Description = "A thriving monkey city that boosts range and grants extra income.", Price = 5000 },
            ]
        },
        new Tower
        {
            Name = "Engineer Monkey", Category = "Support", Image = "engineer_monkey.png",
            Description = "Builds sentry guns and gives nearby towers extra pierce.",
            Cost = 350, Damage = 1, Pierce = 3, Range = 40,
            UpgradePaths =
            [
                new TowerUpgradePath { PathLabel = "Top Path", Name = "Sentry Paragon", Description = "Builds an incredibly powerful sentry that fires devastating projectiles.", Price = 32000 },
                new TowerUpgradePath { PathLabel = "Middle Path", Name = "Ultraboost", Description = "Massively boosts the pierce and attack speed of all nearby towers.", Price = 72000 },
                new TowerUpgradePath { PathLabel = "Bottom Path", Name = "XXXL Trap", Description = "Builds a giant trap that deals massive damage when triggered.", Price = 45000 },
            ]
        },
        new Tower
        {
            Name = "Beast Handler", Category = "Support", Image = "beast_handler_monkey.png",
            Description = "Commands loyal animal companions to fight alongside your towers.",
            Cost = 250, Damage = 1, Pierce = 1, Range = 20,
            UpgradePaths =
            [
                new TowerUpgradePath { PathLabel = "Top Path", Name = "Megalodon", Description = "Summons a giant shark that devours Bloons near water.", Price = 45000 },
                new TowerUpgradePath { PathLabel = "Middle Path", Name = "Giganotosaurus", Description = "Commands a massive dinosaur that tramples and crushes Bloons.", Price = 60000 },
                new TowerUpgradePath { PathLabel = "Bottom Path", Name = "Pouakai", Description = "Summons a giant bird of prey that swoops down on Bloons.", Price = 30000 },
            ]
        },
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

    // /Towers/Details/dart-monkey - id is the tower's slug (see Tower.Slug),
    // not its display name, so the URL stays readable without encoding.
    public IActionResult Details(string id)
    {
        var index = AllTowers.FindIndex(t => t.Slug == id);
        if (index == -1)
        {
            return NotFound();
        }

        // Previous/next arrows cycle through AllTowers in list order and wrap
        // around at both ends, so every tower (including the first and last)
        // always has somewhere to go.
        var previous = AllTowers[(index - 1 + AllTowers.Count) % AllTowers.Count];
        var next = AllTowers[(index + 1) % AllTowers.Count];
        ViewData["PreviousSlug"] = previous.Slug;
        ViewData["PreviousName"] = previous.Name;
        ViewData["NextSlug"] = next.Slug;
        ViewData["NextName"] = next.Name;

        return View(AllTowers[index]);
    }
}
