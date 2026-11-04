namespace MonkeyArchive.Models;

// Eine Zeile aus der User Tabelle, so wie sie die Admin Seite zeigt.
// Passwort und Sicherheitsantwort fehlen hier absichtlich. Sie stehen nur als
// Hash in der DB und lassen sich darum nicht anzeigen, nur neu setzen.
public class AdminUser
{
    public int Id { get; set; }
    public required string Username { get; set; }
    public required string SecurityQuestion { get; set; }

    // Wie viele Towers dieser Account favorisiert hat. Kommt aus einem
    // LEFT JOIN auf FavouriteTowers, ist also 0 wenn er keine hat.
    public int FavoriteCount { get; set; }

    // Der Account, der die Admin Seite überhaupt öffnen darf. Die Liste
    // markiert ihn und blockiert das Löschen, damit man sich nicht selbst
    // aussperrt.
    public bool IsAdmin { get; set; }
}
