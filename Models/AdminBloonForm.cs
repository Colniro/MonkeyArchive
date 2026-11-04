namespace MonkeyArchive.Models;

// Formular für /Admin/EditBloon. Gleiche Idee wie AdminTowerForm: alle Spalten
// der Bloon Tabelle, inklusive der Felder die nur die Darstellung steuern
// (ColorHex, ImageScale).
public class AdminBloonForm
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public string Slug { get; set; } = "";
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string ImageFileName { get; set; } = "";

    // Normalerweise ein Hex Wert wie #DC3E3E. Rainbow hat einen ganzen CSS
    // Farbverlauf drin, darum ist das ein freies Textfeld und kein Color Picker.
    public string ColorHex { get; set; } = "";
    public decimal ImageScale { get; set; } = 1.0m;
    public int Rbe { get; set; }
    public decimal Speed { get; set; }
    public bool IsCamo { get; set; }
    public bool IsLead { get; set; }
    public int Layer { get; set; }
    public string? GameVersion { get; set; }
    public string? PopsIntoNote { get; set; }

    public List<AdminLookupItem> Categories { get; set; } = [];
}
