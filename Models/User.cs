namespace MonkeyArchive.Models;

// A registered account, stored in-memory in AccountController's dictionary
// (keyed by e-mail) rather than a database.
public class User
{
    public required string Email { get; set; }
    public required string Password { get; set; }

    // Favoriting a tower isn't wired up on any page yet, so this stays
    // empty for now - it exists so the Favorites page has somewhere to
    // read from once that feature is built.
    public List<string> FavoriteTowerSlugs { get; set; } = [];
}
