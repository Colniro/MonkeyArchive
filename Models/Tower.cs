namespace MonkeyArchive.Models;

// One BTD6 tower shown on the /Towers overview page.
public class Tower
{
    public required string Name { get; set; }

    // One of "Primary", "Military", "Magic", "Support" - matches the
    // filter pill values and the --Cat* colour variables in site.css.
    public required string Category { get; set; }

    // File name only (no path); resolved against wwwroot/images/monkeys/ in the view.
    public required string Image { get; set; }
}
