using CarePlatform.Models.Common;

namespace CarePlatform.Models.Common;

/// <summary>
/// Shared TIU document record parsed from TIU DOCUMENTS BY CONTEXT RPC.
/// Used by Notes (class=3), DCSumm (class=244), and Consults TIU sub-documents.
///
/// RPC returns 15 caret-delimited pieces (from cprs/uDocTree.pas):
///   1  - Document IEN
///   2  - Document Title
///   3  - FM date of document
///   4  - Patient Name
///   5  - DUZ;Author name  (format: "DUZ;Initials;Display Name")
///   6  - Location
///   7  - Status
///   8  - ADM/VIS: date;FMDate
///   9  - Discharge Date;FMDate
///   10 - Package variable pointer
///   11 - Number of images
///   12 - Subject
///   13 - Has children  (+, >, <, %, combinations)
///   14 - Parent document (IEN or context number for top-level)
///   15 - Order children of ID Note by title rather than date
/// </summary>
public class TiuDocument
{
    public string Ien { get; set; } = "";
    public string Title { get; set; } = "";
    public string FmDate { get; set; } = "";
    public DateTime? ReferenceDate { get; set; }
    public string PatientName { get; set; } = "";

    /// <summary>Raw DUZ;Initials;DisplayName from piece 5.</summary>
    public string AuthorRaw { get; set; } = "";
    /// <summary>Author display name extracted from piece 5 (third semicolon-delimited part).</summary>
    public string AuthorName { get; set; } = "";
    /// <summary>Author DUZ extracted from piece 5 (first semicolon-delimited part).</summary>
    public long AuthorDuz { get; set; }

    public string LocationName { get; set; } = "";
    public string Status { get; set; } = "";

    /// <summary>ADM/VIS: date;FMDate string from piece 8.</summary>
    public string VisitString { get; set; } = "";
    /// <summary>Discharge Date;FMDate string from piece 9.</summary>
    public string DischargeDateString { get; set; } = "";
    public DateTime? DischargeDate { get; set; }

    /// <summary>Package variable pointer (e.g., "GMR(123,456" for consults) from piece 10.</summary>
    public string PackageRef { get; set; } = "";
    public int ImageCount { get; set; }
    public string Subject { get; set; } = "";

    /// <summary>
    /// Has children flag from piece 13.
    /// Values: +  = has children, > = is ID child, &lt; = is ID parent,
    ///         % = group/category node, combinations like +>, +&lt;, *prefix.
    /// Empty = leaf document with no children.
    /// </summary>
    public string HasChildrenFlag { get; set; } = "";
    public bool HasChildren => !string.IsNullOrEmpty(HasChildrenFlag);

    /// <summary>Parent document IEN from piece 14. For top-level notes, equals the context number (e.g., "1").</summary>
    public string ParentDocument { get; set; } = "";

    /// <summary>If true, order children of this ID Note by title rather than date (piece 15).</summary>
    public bool OrderByTitle { get; set; }

    /// <summary>
    /// Parse a single caret-delimited line from TIU DOCUMENTS BY CONTEXT RPC.
    /// Field indices match cprs/uDocTree.pas MakeNoteTreeObject (pieces 1-15, 0-based array 0-14).
    /// </summary>
    public static TiuDocument Parse(string vistaLine)
    {
        var p = VistaStringParser.Split(vistaLine);
        var authorRaw = p.Length > 4 ? p[4] : "";   // piece 5: "DUZ;Initials;DisplayName"
        var authorParts = authorRaw.Split(';');
        var hasChildrenRaw = p.Length > 12 ? p[12] : "";
        // Strip leading '*' from hasChildren per Pascal: if Copy(DocHasChildren, 1, 1) = '*' then ...
        if (hasChildrenRaw.StartsWith('*'))
            hasChildrenRaw = hasChildrenRaw[1..];

        var dischargeDateStr = p.Length > 8 ? p[8] : "";  // piece 9: "Dis: date;FMDate"
        // Extract FM date portion from discharge date string (after semicolon)
        var dischargeFmDate = dischargeDateStr.Contains(';')
            ? dischargeDateStr.Split(';')[1]
            : dischargeDateStr;

        return new TiuDocument
        {
            Ien = p.Length > 0 ? p[0] : "",
            Title = p.Length > 1 ? p[1] : "",
            FmDate = p.Length > 2 ? p[2] : "",
            ReferenceDate = p.Length > 2 ? VistaStringParser.ParseFmDateTime(p[2]) : null,
            PatientName = p.Length > 3 ? p[3] : "",
            AuthorRaw = authorRaw,
            AuthorDuz = authorParts.Length > 0 ? long.TryParse(authorParts[0], out var duz) ? duz : 0 : 0,
            AuthorName = authorParts.Length > 2 ? authorParts[2] : (authorParts.Length > 1 ? authorParts[1] : authorRaw),
            LocationName = p.Length > 5 ? p[5] : "",
            Status = p.Length > 6 ? p[6] : "",
            VisitString = p.Length > 7 ? p[7] : "",
            DischargeDateString = dischargeDateStr,
            DischargeDate = VistaStringParser.ParseFmDateTime(dischargeFmDate),
            PackageRef = p.Length > 9 ? p[9] : "",
            ImageCount = p.Length > 10 ? (int.TryParse(p[10], out var ic) ? ic : 0) : 0,
            Subject = p.Length > 11 ? p[11] : "",
            HasChildrenFlag = hasChildrenRaw,
            ParentDocument = p.Length > 13 ? p[13] : "",
            OrderByTitle = p.Length > 14 && p[14] == "1"
        };
    }

    /// <summary>
    /// Parse a list of caret-delimited lines from TIU DOCUMENTS BY CONTEXT RPC.
    /// Skips blank lines and lines with no IEN.
    /// </summary>
    public static List<TiuDocument> ParseList(List<string> vistaLines)
    {
        return vistaLines
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .Select(Parse)
            .Where(d => !string.IsNullOrEmpty(d.Ien))
            .ToList();
    }
}
