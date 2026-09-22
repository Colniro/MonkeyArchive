namespace MonkeyArchive.Models;

// Ein registrierter Account. Liegt im Speicher in einem Dictionary im AccountController, per Username.
// Es gibt keine echte Datenbank dafür.
public class User
{
    public required string Username { get; set; }
    public required string Password { get; set; }

    // Wird beim Passwort vergessen Ablauf gebraucht.
    // Es gibt keinen E-Mail Server in diesem Projekt, darum gibt es keinen Reset Link per Mail.
    // Wer die Frage richtig beantwortet, darf ein neues Passwort setzen.
    public required string SecurityQuestion { get; set; }
    public required string SecurityAnswer { get; set; }

    // Das Favorisieren von Towers ist noch auf keiner Seite eingebaut.
    // Darum ist die Liste im Moment immer leer.
    // Sie ist schon da, damit die Favorites Seite später etwas zum Anzeigen hat.
    public List<string> FavoriteTowerSlugs { get; set; } = [];
}
