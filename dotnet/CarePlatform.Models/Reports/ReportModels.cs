namespace CarePlatform.Models.Reports;

/// <summary>
/// Report definition from the report tree structure.
/// Field mapping matches TReportTreeObject in cprs/uReports.pas (MakeReportTreeObject).
/// VistA line format from ORWRP3 EXPAND COLUMNS:
///   p0=ID ^ p1=Name ^ p2=Qualifier ^ p3=HSTag ^ p4=Entry ^ p5=Routine ^
///   p6=Remote ^ p7=RptType ^ p8=Category ^ p9=RPCName ^ p10=IFN ^
///   p11=SortOrder ^ p12=MaxDaysBack ^ p13=Direct ^ p14=HDR ^ p15=FHIE ^ p16=FHIEONLY
/// </summary>
public class ReportDefinition
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Qualifier { get; set; } = "";
    public string HSTag { get; set; } = "";
    public string Remote { get; set; } = "";
    public string RptType { get; set; } = "";
    public string Category { get; set; } = "";
    public string RPCName { get; set; } = "";
    public string IFN { get; set; } = "";
    public string SortOrder { get; set; } = "";
    public string MaxDaysBack { get; set; } = "";
    public string Direct { get; set; } = "";
    public string HDR { get; set; } = "";
    public string FHIE { get; set; } = "";
    public string FHIEONLY { get; set; } = "";
    public bool SupportsDateRange { get; set; }
    public bool SupportsRemoteData { get; set; }
    /// <summary>
    /// Qualifier type code from file 101.24 piece 3 (looked up via ORWRP REPORT LISTS
    /// [REPORT LIST] section by IFN). Drives how the JS render dispatcher interprets
    /// the report payload (table vs text vs HTML; per-QT row shaping). Mirrors
    /// ReportQualifierType in cprs/rReports.pas line 380-388.
    /// 0=OTHER, 1=MOSTRECENT, 2=DATERANGE, 3=IMAGING, 4=NUTR, 5=HSCOMPONENT,
    /// 6=HSWPCOMPONENT, 19=PROCEDURES, 28=SURGERY (see cprs/fReports.pas QT_* consts).
    /// </summary>
    public int QualifierType { get; set; }
    public List<ReportDefinition> Children { get; set; } = [];

    /// <summary>
    /// Parse a single caret-delimited VistA report definition line.
    /// Matches MakeReportTreeObject in cprs/uReports.pas.
    /// </summary>
    public static ReportDefinition Parse(string line)
    {
        var p = line.Split('^');
        var remote = p.Length > 6 ? p[6] : "";
        var qualifier = p.Length > 2 ? p[2] : "";
        return new ReportDefinition
        {
            Id = p.Length > 0 ? p[0] : "",
            Name = p.Length > 1 ? p[1] : "",
            Qualifier = qualifier,
            HSTag = p.Length > 3 ? p[3] : "",
            Remote = remote,
            RptType = p.Length > 7 ? p[7] : "",
            Category = p.Length > 8 ? p[8] : "",
            RPCName = p.Length > 9 ? p[9] : "",
            IFN = p.Length > 10 ? p[10] : "",
            SortOrder = p.Length > 11 ? p[11] : "",
            MaxDaysBack = p.Length > 12 ? p[12] : "",
            Direct = p.Length > 13 ? p[13] : "",
            HDR = p.Length > 14 ? p[14] : "",
            FHIE = p.Length > 15 ? p[15] : "",
            FHIEONLY = p.Length > 16 ? p[16] : "",
            SupportsRemoteData = remote == "1" || remote == "2",
            SupportsDateRange = qualifier.Length > 0 && (qualifier[0] == 'T' || qualifier[0] == 'd')
        };
    }

    /// <summary>
    /// Parse lines from ORWRP3 EXPAND COLUMNS [REPORT LIST] section,
    /// building a hierarchical tree from [PARENT START]/[PARENT END] markers.
    /// Matches LoadTreeView in cprs/fReports.pas + ListReports in cprs/rReports.pas.
    /// </summary>
    public static List<ReportDefinition> ParseTreeFromExpandColumns(List<string> allLines)
    {
        // Extract only the [REPORT LIST] section (between marker and $$END)
        var lines = new List<string>();
        bool inSection = false;
        foreach (var line in allLines)
        {
            if (line == "[REPORT LIST]") { inSection = true; continue; }
            if (line == "$$END") { if (inSection) break; continue; }
            if (inSection && !string.IsNullOrWhiteSpace(line))
                lines.Add(line);
        }

        // Build tree using [PARENT START]/[PARENT END] markers
        // matching the logic in fReports.pas LoadTreeView
        var roots = new List<ReportDefinition>();
        var parentStack = new Stack<ReportDefinition>();

        foreach (var line in lines)
        {
            var piece0 = line.Split('^')[0].ToUpperInvariant();

            if (piece0 == "[PARENT END]")
            {
                if (parentStack.Count > 0)
                    parentStack.Pop();
                continue;
            }

            if (piece0 == "[PARENT START]")
            {
                // [PARENT START]^ID^Name^... — pieces 2+ are the definition
                var parts = line.Split('^');
                var defLine = string.Join("^", parts.Skip(1));
                var node = Parse(defLine);

                if (parentStack.Count > 0)
                    parentStack.Peek().Children.Add(node);
                else
                    roots.Add(node);

                parentStack.Push(node);
                continue;
            }

            // Regular report line
            var report = Parse(line);
            if (parentStack.Count > 0)
                parentStack.Peek().Children.Add(report);
            else
                roots.Add(report);
        }

        return roots;
    }

    public static List<ReportDefinition> ParseList(List<string> lines)
        => lines.Where(l => !string.IsNullOrWhiteSpace(l)
                         && !l.StartsWith("[") && l != "$$END")
                .Select(Parse).ToList();
}

/// <summary>
/// Report content result.
/// </summary>
public class ReportContent
{
    public string ReportId { get; set; } = "";
    public string Title { get; set; } = "";
    public string ContentType { get; set; } = "";  // text, html, table
    public List<string> TextLines { get; set; } = [];
    public string HtmlContent { get; set; } = "";
    public List<ReportTableRow> TableRows { get; set; } = [];
    public List<string> ColumnHeaders { get; set; } = [];
}

/// <summary>
/// A row in tabular report data.
/// </summary>
public class ReportTableRow
{
    public List<string> Columns { get; set; } = [];
}

/// <summary>
/// Report filter options.
/// </summary>
public class ReportFilter
{
    public string ReportId { get; set; } = "";
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int MaxOccurrences { get; set; }
    public string Qualifier { get; set; } = "";
    public List<string> RemoteSites { get; set; } = [];
}
