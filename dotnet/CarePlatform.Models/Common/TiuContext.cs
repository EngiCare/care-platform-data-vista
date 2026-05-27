// Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
namespace CarePlatform.Models.Common;

/// <summary>
/// TIU document view context — matches Pascal TTIUContext record from cprs/uTIU.pas.
/// Serializes to/from the semicolon-delimited string used by
/// ORWTIU GET TIU CONTEXT / ORWTIU SAVE TIU CONTEXT RPCs.
///
/// Format: BeginDate;EndDate;Status;Author;MaxDocs;ShowSubject;SortBy;ListAscending;TreeAscending;GroupBy;SearchField;KeyWord
/// </summary>
public class TiuContext
{
    // ── NC_ constants from cprs/uConst.pas ──
    /// <summary>Last N signed notes (default view).</summary>
    public const int NC_RECENT = 0;
    /// <summary>All signed notes.</summary>
    public const int NC_ALL = 1;
    /// <summary>Unsigned notes.</summary>
    public const int NC_UNSIGNED = 2;
    /// <summary>Uncosigned notes.</summary>
    public const int NC_UNCOSIGNED = 3;
    /// <summary>Signed notes by author.</summary>
    public const int NC_BY_AUTHOR = 4;
    /// <summary>Signed notes by date range.</summary>
    public const int NC_BY_DATE = 5;
    /// <summary>Custom view (user-configured).</summary>
    public const int NC_CUSTOM = 6;

    /// <summary>Default max documents when preference is 0 or unset.</summary>
    public const int DEFAULT_MAX_DOCS = 100;

    public string BeginDate { get; set; } = "";
    public string EndDate { get; set; } = "";
    public int Status { get; set; } = NC_ALL;
    public long Author { get; set; }
    public int MaxDocs { get; set; }
    public bool ShowSubject { get; set; }
    /// <summary>Sort by: R=Date, D=Title, S=Subject, A=Author, L=Location.</summary>
    public string SortBy { get; set; } = "";
    public bool ListAscending { get; set; }
    /// <summary>Group by: D=Visit Date, L=Location, T=Title, A=Author, ""=none.</summary>
    public string GroupBy { get; set; } = "";
    public bool TreeAscending { get; set; }
    /// <summary>Search field: T=Title, S=Subject, B=Both, ""=none.</summary>
    public string SearchField { get; set; } = "";
    public string Keyword { get; set; } = "";

    /// <summary>Effective max docs — returns DEFAULT_MAX_DOCS when MaxDocs is 0.</summary>
    public int EffectiveMaxDocs => MaxDocs > 0 ? MaxDocs : DEFAULT_MAX_DOCS;

    /// <summary>
    /// Parse the semicolon-delimited context string returned by ORWTIU GET TIU CONTEXT.
    /// Matches cprs/rTIU.pas GetCurrentTIUContext.
    /// </summary>
    public static TiuContext Parse(string contextString)
    {
        if (string.IsNullOrWhiteSpace(contextString))
            return new TiuContext();

        var pieces = contextString.Split(';');
        var ctx = new TiuContext
        {
            BeginDate = Piece(pieces, 0),
            EndDate = Piece(pieces, 1),
            Status = IntPiece(pieces, 2, NC_ALL),
            Author = LongPiece(pieces, 3),
            MaxDocs = IntPiece(pieces, 4, 0),
            ShowSubject = IntPiece(pieces, 5, 0) > 0,
            SortBy = Piece(pieces, 6),
            ListAscending = IntPiece(pieces, 7, 0) > 0,
            TreeAscending = IntPiece(pieces, 8, 0) > 0,
            GroupBy = Piece(pieces, 9),
            SearchField = Piece(pieces, 10),
            Keyword = Piece(pieces, 11)
        };

        // Validate status range (matches Pascal: if (StrToIntDef(Status,0) < 1) or (> 5) then Status := '1')
        if (ctx.Status < 1 || ctx.Status > 5)
            ctx.Status = NC_ALL;

        return ctx;
    }

    /// <summary>
    /// Serialize to the semicolon-delimited context string for ORWTIU SAVE TIU CONTEXT.
    /// Matches cprs/rTIU.pas SaveCurrentTIUContext.
    /// </summary>
    public string ToContextString()
    {
        return string.Join(";",
            BeginDate,
            EndDate,
            Status.ToString(),
            Author > 0 ? Author.ToString() : "",
            MaxDocs.ToString(),
            ShowSubject ? "1" : "0",
            SortBy,
            ListAscending ? "1" : "0",
            TreeAscending ? "1" : "0",
            GroupBy,
            SearchField,
            Keyword);
    }

    /// <summary>
    /// Create a default "Recent Notes" context with MaxDocs = DEFAULT_MAX_DOCS.
    /// Matches Pascal NC_RECENT initial load.
    /// </summary>
    public static TiuContext RecentDefault(int maxDocs = DEFAULT_MAX_DOCS) => new()
    {
        Status = NC_ALL,
        MaxDocs = maxDocs,
        SortBy = "R"
    };

    private static string Piece(string[] parts, int index) =>
        index < parts.Length ? parts[index] : "";

    private static int IntPiece(string[] parts, int index, int defaultValue) =>
        index < parts.Length && int.TryParse(parts[index], out var v) ? v : defaultValue;

    private static long LongPiece(string[] parts, int index) =>
        index < parts.Length && long.TryParse(parts[index], out var v) ? v : 0;
}
