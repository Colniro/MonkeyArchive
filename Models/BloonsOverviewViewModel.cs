namespace MonkeyArchive.Models;

// Carries the already-filtered bloon list plus the current search/category
// state back to the view, so the search box and the active filter pill can
// be re-rendered exactly as the user left them after a page reload.
public class BloonsOverviewViewModel
{
    public required List<Bloon> Bloons { get; set; }
    public required string Search { get; set; }
    public required string SelectedCategory { get; set; }
}
