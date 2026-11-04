using System.Security.Cryptography;
using System.Text;

namespace MonkeyArchive.Helpers;

// Macht aus Passwörtern und Sicherheitsantworten einen Hash, damit in der
// Datenbank kein Klartext mehr steht. Benutzt PBKDF2 mit SHA256, das ist schon
// in .NET eingebaut, es braucht also kein extra NuGet Paket.
//
// Gespeichert wird alles in einem einzigen Text:
//   PBKDF2$<Durchläufe>$<Salt>$<Hash>
// Der Salt ist pro Eintrag zufällig. So haben zwei gleiche Passwörter trotzdem
// einen anderen Hash. Weil die Anzahl Durchläufe mitgespeichert wird, kann man
// sie später erhöhen, ohne dass alte Hashes ungültig werden.
public static class PasswordHelper
{
    private const string Prefix = "PBKDF2";

    // Empfehlung von OWASP für PBKDF2 mit SHA256.
    private const int Iterations = 600_000;
    private const int SaltSize = 16;
    private const int HashSize = 32;

    public static string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, HashSize);
        return $"{Prefix}${Iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }

    public static bool IsHashed(string stored) => stored.StartsWith(Prefix + "$", StringComparison.Ordinal);

    // Prüft ein eingegebenes Passwort gegen den Wert aus der Datenbank.
    // Accounts von vor der Umstellung haben noch Klartext in der DB. Die werden
    // hier auch noch angenommen, needsRehash sagt dem Aufrufer dann, dass er den
    // Wert jetzt als Hash neu speichern soll.
    public static bool Verify(string password, string stored, out bool needsRehash)
    {
        needsRehash = false;

        if (!IsHashed(stored))
        {
            var matches = CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(password), Encoding.UTF8.GetBytes(stored));
            needsRehash = matches;
            return matches;
        }

        var parts = stored.Split('$');
        if (parts.Length != 4 || !int.TryParse(parts[1], out var iterations))
        {
            return false;
        }

        byte[] salt;
        byte[] expected;
        try
        {
            salt = Convert.FromBase64String(parts[2]);
            expected = Convert.FromBase64String(parts[3]);
        }
        catch (FormatException)
        {
            return false;
        }

        var actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, expected.Length);

        // FixedTimeEquals braucht immer gleich lang, egal wo der erste Unterschied
        // ist. So kann man aus der Antwortzeit nichts über den Hash ablesen.
        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }

    // Bei der Sicherheitsantwort sollen Gross und Kleinschreibung und Leerzeichen
    // am Rand egal sein ("Banana " ist gleich wie "banana"). Darum wird die
    // Antwort vor dem Hashen und vor dem Prüfen gleich gemacht.
    public static string HashAnswer(string answer) => Hash(NormalizeAnswer(answer));

    public static bool VerifyAnswer(string answer, string stored)
    {
        if (!IsHashed(stored))
        {
            return NormalizeAnswer(answer) == NormalizeAnswer(stored);
        }

        return Verify(NormalizeAnswer(answer), stored, out _);
    }

    private static string NormalizeAnswer(string answer) => answer.Trim().ToLowerInvariant();
}
