// Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
using CarePlatform.Models.Common;

namespace CarePlatform.Models.DCSumm;

/// <summary>
/// Discharge Summary record (TIU class 244).
/// Maps to: uDCSumm.pas — TEditDCSummRec, TDCSummRec, TDCSummTitles, TDCSummPrefs.
/// </summary>
public class DischargeSummary
{
    public string Ien { get; set; } = "";
    public string Title { get; set; } = "";
    public DateTime? ReferenceDate { get; set; }
    public DateTime? AdmitDate { get; set; }
    public DateTime? DischargeDate { get; set; }
    public string AuthorName { get; set; } = "";
    public long AuthorDuz { get; set; }
    public string CosignerName { get; set; } = "";
    public long CosignerDuz { get; set; }
    public string AttendingName { get; set; } = "";
    public long AttendingDuz { get; set; }
    public string Status { get; set; } = "";
    public string Urgency { get; set; } = "";
    public string LocationName { get; set; } = "";
    public string Subject { get; set; } = "";
    public string ParentIen { get; set; } = "";
    public bool HasAddenda { get; set; }
    public List<string> TextLines { get; set; } = [];

    /// <summary>
    /// Parse from a TiuDocument (which correctly parses TIU DOCUMENTS BY CONTEXT).
    /// </summary>
    public static DischargeSummary FromTiu(TiuDocument tiu) => new()
    {
        Ien = tiu.Ien,
        Title = tiu.Title,
        ReferenceDate = tiu.ReferenceDate,
        AuthorName = tiu.AuthorName,
        AuthorDuz = tiu.AuthorDuz,
        LocationName = tiu.LocationName,
        Status = tiu.Status,
        Subject = tiu.Subject,
        DischargeDate = tiu.DischargeDate,
        ParentIen = tiu.ParentDocument,
        HasAddenda = tiu.HasChildrenFlag.Contains('+')
    };

    /// <summary>
    /// Parse a single caret-delimited VistA line from TIU DOCUMENTS BY CONTEXT.
    /// Uses TiuDocument for correct 15-piece field mapping per cprs/uDocTree.pas.
    /// </summary>
    public static DischargeSummary Parse(string vistaLine) => FromTiu(TiuDocument.Parse(vistaLine));

    public static List<DischargeSummary> ParseList(List<string> vistaLines)
    {
        return vistaLines
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .Select(Parse)
            .Where(s => !string.IsNullOrEmpty(s.Ien))
            .ToList();
    }
}

/// <summary>
/// DC Summary properties for the properties dialog.
/// Maps to: fDCSummProps.pas — TEditDCSummRec fields.
/// </summary>
public class DCSummProperties
{
    public string Ien { get; set; } = "";
    public string TitleIen { get; set; } = "";
    public string TitleName { get; set; } = "";
    public string AuthorDuz { get; set; } = "";
    public string AuthorName { get; set; } = "";
    public string CosignerDuz { get; set; } = "";
    public string CosignerName { get; set; } = "";
    public string AttendingDuz { get; set; } = "";
    public string AttendingName { get; set; } = "";
    public string LocationIen { get; set; } = "";
    public string LocationName { get; set; } = "";
    public string AdmitDate { get; set; } = "";
    public string DischargeDate { get; set; } = "";
    public string RefDate { get; set; } = "";
    public string Urgency { get; set; } = "R";
    public string DocClass { get; set; } = "DC";
    public string Status { get; set; } = "";
    public bool RequireCosign { get; set; }
    public string VisitStr { get; set; } = "";
}

/// <summary>
/// Admission record for the admission picker in fDCSummProps.pas.
/// Maps to: uDCSumm.pas — TAdmitRec.
/// </summary>
public class AdmissionRecord
{
    public string AdmitDateTime { get; set; } = "";
    public string LocationIen { get; set; } = "";
    public string LocationName { get; set; } = "";
    public string VisitStr { get; set; } = "";
    public string DischargeDateTime { get; set; } = "";
    public string Type { get; set; } = "";
    public int SummaryStatus { get; set; }
    public string SummaryStatusText { get; set; } = "";
    public string DisplayDate { get; set; } = "";
}
