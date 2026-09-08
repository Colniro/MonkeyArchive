namespace MonkeyArchive.Models;

// Carries the already-filtered tower list plus the current search/category
// state back to the view, so the search box and the active filter pill can
// be re-rendered exactly as the user left them after a page reload.
public class TowersOverviewViewModel
{
    public required List<Tower> Towers { get; set; }
    public required string Search { get; set; }
    public required string SelectedCategory { get; set; }
}
