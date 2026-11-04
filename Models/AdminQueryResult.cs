namespace MonkeyArchive.Models;

// Ergebnis der SQL Konsole auf /Admin/Query.
// Die Konsole liest nur (SELECT/SHOW/DESCRIBE/EXPLAIN), darum braucht es hier
// keine Info über geänderte Zeilen, nur Spaltennamen und die Zeilen als Text.
public class AdminQueryResult
{
    public string Sql { get; set; } = "";
    public List<string> Columns { get; set; } = [];
    public List<List<string>> Rows { get; set; } = [];

    // Wird gesetzt, wenn mehr Zeilen zurückkamen als RowLimit erlaubt. Die View
    // zeigt dann einen Hinweis, dass die Ausgabe abgeschnitten ist.
    public bool Truncated { get; set; }
    public string? Error { get; set; }
    public bool HasRun { get; set; }
}
