namespace MonkeyArchive.Models;

// Ein BTD6 Bloon. Wird auf der /Bloons Übersicht und auf der eigenen Detailseite gezeigt.
public class Bloon
{
    public required string Name { get; set; }

    // Ist "Normal" oder "MOAB-Class". Muss zu den Filter Pills passen.
    public required string Category { get; set; }

    // Nur der Dateiname, kein Pfad. Der Pfad wird in der View mit wwwroot/images/bloons/ zusammengebaut.
    public required string Image { get; set; }

    // Bloons haben nicht nur vier Kategorie Farben wie die Towers. Jeder Bloon hat seine eigene Farbe.
    // Darum steht die Farbe hier im Model und wird nicht aus der Category berechnet.
    public required string Color { get; set; }

    // Die Bilder haben unterschiedlich viel transparenten Rand um den Ballon.
    // Manche füllen fast das ganze Bild, andere nur einen kleinen Teil.
    // Darum würden sie bei gleicher Grösse unterschiedlich gross aussehen.
    // Dieser Wert skaliert das Bild damit es wieder passt. Siehe BloonsController für die genauen Werte.
    public double ImageScale { get; set; } = 1.0;

    // Felder für die Detailseite.
    public string Description { get; set; } = "";
    public int Rbe { get; set; }
    public string Speed { get; set; } = "";
    public bool IsCamo { get; set; }
    public bool IsLead { get; set; }

    // Die Position in der festen Layer Reihenfolge von BTD6. Red ist 1, BAD ist 13.
    // Der Wert kommt direkt aus den echten Spieldaten (layerNumber).
    // Manche Bloons teilen sich eine Nummer, zum Beispiel Black, White und Purple sind alle 6.
    // Das ist so im Spiel und kein Fehler.
    public int Layer { get; set; }

    // Immunitäten ausser Lead. Lead hat einen eigenen Tag.
    // Zum Beispiel ist Black immun gegen Explosionen. Ist leer wenn ein Bloon keine hat.
    public List<string> Immunities { get; set; } = [];

    // Ist leer beim schwächsten Layer (Red). Der verschwindet einfach beim Platzen.
    public List<BloonChild> PopsInto { get; set; } = [];

    // Wird statt PopsInto benutzt wenn das Ergebnis nicht fix ist.
    // Ceramic und DDT platzen im echten Spiel zu einem zufälligen schwächeren Bloon.
    // Hier steht dann ein Text statt der Bilderreihe.
    public string? PopsIntoNote { get; set; }

    // Id für die URL der Detailseite, zum Beispiel /Bloons/Details/red.
    public string Slug => Name.ToLowerInvariant().Replace(" ", "-");

    // Hintergrund für den Hero Bereich auf der Detailseite.
    // Wird aus der eigenen Farbe des Bloons berechnet, nicht aus einer Kategorie Palette wie bei Towers.
    // Rainbow hat schon einen fertigen CSS Farbverlauf als Color, keinen einzelnen Hex Wert.
    // Darum wird der einfach direkt verwendet statt berechnet.
    public string HeroBackground => Color.StartsWith("linear-gradient", StringComparison.Ordinal)
        ? Color
        : $"linear-gradient(120deg, {Blend(Color, 255, 0.22)}, {Blend(Color, 0, 0.4)})";

    private static string Blend(string hex, int target, double amount)
    {
        hex = hex.TrimStart('#');
        var r = Convert.ToInt32(hex[..2], 16);
        var g = Convert.ToInt32(hex[2..4], 16);
        var b = Convert.ToInt32(hex[4..6], 16);
        r += (int)((target - r) * amount);
        g += (int)((target - g) * amount);
        b += (int)((target - b) * amount);
        return $"#{r:X2}{g:X2}{b:X2}";
    }
}
