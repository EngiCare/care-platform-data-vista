using CarePlatform.Models.Common;

namespace CarePlatform.Models.Problems;

/// <summary>
/// Full problem record from ORQQPL PROBLEM LIST and ORQQPL EDIT LOAD.
/// </summary>
public class Problem
{
    public string Ifn { get; set; } = "";
    public string Dfn { get; set; } = "";
    public string Description { get; set; } = "";
    public string IcdCode { get; set; } = "";
    public string SnomedCode { get; set; } = "";
    public string Status { get; set; } = "";       // A=Active, I=Inactive
    public string Priority { get; set; } = "";      // A=Acute, C=Chronic
    public DateTime? OnsetDate { get; set; }
    public DateTime? DateRecorded { get; set; }
    public DateTime? DateResolved { get; set; }
    public DateTime? DateModified { get; set; }
    public string ResponsibleProvider { get; set; } = "";
    public long ResponsibleProviderIen { get; set; }
    public string RecordingProvider { get; set; } = "";
    public string Location { get; set; } = "";
    public string Service { get; set; } = "";
    public string Condition { get; set; } = "";     // T=Transcribed, P=Permanent, H=Hidden
    public bool InactiveIcdFlag { get; set; }        // # = inactive ICD code
    public bool ServiceConnected { get; set; }
    public bool AgentOrangeExposure { get; set; }
    public bool IonizingRadiationExposure { get; set; }
    public bool PersianGulfExposure { get; set; }
    public bool HeadNeckCancer { get; set; }
    public bool MilitarySexualTrauma { get; set; }
    public bool CombatVeteran { get; set; }
    public bool ShipboardHazard { get; set; }
    public bool CampLejeune { get; set; }
    public List<ProblemComment> Comments { get; set; } = [];
    public string Narrative { get; set; } = "";

    public static Problem Parse(string vistaLine)
    {
        // Real VistA ORQQPL PROBLEM LIST format (confirmed from CPRS Delphi fProbs.pas LoadProblems
        // and rProbs.pas, 1-based Piece indexing → 0-based C# array):
        //   p[0]=IFN              (Piece 1)
        //   p[1]=Status A/I       (Piece 2)
        //   p[2]=Description      (Piece 3)
        //   p[3]=ICD code         (Piece 4)
        //   p[4]=Onset date       (Piece 5)   ← onset
        //   p[5]=Last updated     (Piece 6)   ← date modified
        //   p[6]=Service connected (Piece 7)
        //   p[8]=Condition T/P/H  (Piece 9)
        //   p[9]=Location IEN;Name (Piece 10)
        //   p[10]=Loc type         (Piece 11)
        //   p[11]=Provider IEN;Name (Piece 12)
        //   p[12]=Service IEN;Name  (Piece 13)
        //   p[13]=Priority A/C     (Piece 14)
        //   p[14]=Has comments 0/1 (Piece 15)
        //   p[16]=SC conditions    (Piece 17)
        //   p[17]='#' inactive ICD flag (Piece 18)
        //   p[18]=Code text        (Piece 19)
        //   p[19]=Code system ICD/10D (Piece 20)
        var p = VistaStringParser.Split(vistaLine);
        return new Problem
        {
            Ifn = p.Length > 0 ? p[0] : "",
            Status = p.Length > 1 ? p[1] : "",
            Description = p.Length > 2 ? p[2] : "",
            IcdCode = p.Length > 3 ? p[3] : "",
            OnsetDate = p.Length > 4 ? VistaStringParser.ParseFmDateTime(p[4]) : null,
            DateModified = p.Length > 5 ? VistaStringParser.ParseFmDateTime(p[5]) : null,
            ServiceConnected = p.Length > 6 && p[6] != "0" && !string.IsNullOrEmpty(p[6]),
            Condition = p.Length > 8 ? p[8] : "",       // T=Transcribed, P=Permanent, H=Hidden
            Location = p.Length > 9 ? ExtractName(p[9]) : "",
            ResponsibleProvider = p.Length > 11 ? ExtractName(p[11]) : "",
            Service = p.Length > 12 ? ExtractName(p[12]) : "",
            Priority = p.Length > 13 ? p[13] : "",      // A=Acute, C=Chronic
            InactiveIcdFlag = p.Length > 17 && p[17] == "#",
            IcdCodeSystem = p.Length > 19 ? p[19] : ""
        };
    }

    /// <summary>
    /// Extracts the display name from VistA IEN;Name format (e.g., "13947;ANESTHESIA BILLING NC" → "ANESTHESIA BILLING NC").
    /// </summary>
    private static string ExtractName(string ienNamePair)
    {
        if (string.IsNullOrEmpty(ienNamePair)) return "";
        var idx = ienNamePair.IndexOf(';');
        return idx >= 0 && idx < ienNamePair.Length - 1 ? ienNamePair[(idx + 1)..] : ienNamePair;
    }

    public static List<Problem> ParseList(List<string> vistaLines)
    {
        return vistaLines
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .Select(Parse)
            .Where(p => !string.IsNullOrEmpty(p.Ifn))
            .ToList();
    }

    // ── From uProbs.pas TProbRec — SNOMED/ICD coordination ──
    public string SnomedConceptVuid { get; set; } = "";
    public string SnomedDesignationVuid { get; set; } = "";
    public string IcdCodeDate { get; set; } = "";
    public string IcdCodeSystem { get; set; } = "";      // ICD or 10D
    public bool NtrtRequested { get; set; }
    public string NtrtComment { get; set; } = "";
    public List<CoordinateExpression> CoordinateExpressions { get; set; } = [];
}

/// <summary>
/// SNOMED/ICD coordinate expression from uProbs.pas TCoordExpr.
/// </summary>
public class CoordinateExpression
{
    public string ConceptVuid { get; set; } = "";
    public string DesignationVuid { get; set; } = "";
    public string Narrative { get; set; } = "";
}

/// <summary>
/// Problem comment entry.
/// </summary>
public class ProblemComment
{
    public string Text { get; set; } = "";
    public string Author { get; set; } = "";
    public DateTime? DateTime { get; set; }
}

/// <summary>
/// Problem filter/view settings.
/// </summary>
public class ProblemFilter
{
    public string Status { get; set; } = "A";    // A=Active, I=Inactive, B=Both, R=Removed
    public string FmDate { get; set; } = "";
}

/// <summary>
/// Problem lexicon search result from ORQQPL4 LEX.
/// </summary>
public class ProblemLexiconResult
{
    public string Ien { get; set; } = "";
    public string Text { get; set; } = "";
    public string IcdCode { get; set; } = "";
    public string SnomedCt { get; set; } = "";

    public static ProblemLexiconResult Parse(string vistaLine)
    {
        var p = VistaStringParser.Split(vistaLine);
        return new ProblemLexiconResult
        {
            Ien = p.Length > 0 ? p[0] : "",
            Text = p.Length > 1 ? p[1] : "",
            IcdCode = p.Length > 2 ? p[2] : "",
            SnomedCt = p.Length > 3 ? p[3] : ""
        };
    }

    public static List<ProblemLexiconResult> ParseList(List<string> vistaLines)
    {
        return vistaLines
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .Select(Parse)
            .ToList();
    }
}
