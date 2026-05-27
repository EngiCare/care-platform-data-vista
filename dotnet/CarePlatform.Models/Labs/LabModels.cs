using CarePlatform.Models.Common;

namespace CarePlatform.Models.Labs;

/// <summary>
/// Lab test item from long lists (ORWLRR ATOMICS, ALLTESTS, etc.)
/// </summary>
public class LabTest
{
    public string Ien { get; set; } = "";
    public string Name { get; set; } = "";
    public string Specimen { get; set; } = "";
}

/// <summary>
/// Lab result from cumulative/interim results.
/// </summary>
public class LabResult
{
    public string TestName { get; set; } = "";
    public string Value { get; set; } = "";
    public string Units { get; set; } = "";
    public string ReferenceRange { get; set; } = "";
    public string Flag { get; set; } = "";        // H, L, H*, L*, HH, LL
    public DateTime? CollectionDate { get; set; }
    public string Specimen { get; set; } = "";
    public string Comment { get; set; } = "";
    public string Status { get; set; } = "";
    public string Site { get; set; } = "";

    public bool IsAbnormal => !string.IsNullOrEmpty(Flag);
    public bool IsCritical => Flag is "H*" or "L*" or "HH" or "LL";
}

/// <summary>
/// Lab test group defined by user.
/// </summary>
public class LabTestGroup
{
    public string Ien { get; set; } = "";
    public string Name { get; set; } = "";
    public List<LabTest> Tests { get; set; } = [];
}

/// <summary>
/// Lab report category for the tree navigation.
/// </summary>
public class LabReportCategory
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string ReportType { get; set; } = "";  // C=cumulative, I=interim, M=micro, W=worksheet
    public List<LabReportCategory> Children { get; set; } = [];
}

/// <summary>
/// Lab date range filter.
/// </summary>
public class LabDateFilter
{
    public string Preset { get; set; } = "6m";    // today, 1w, 1m, 6m, 1y, 2y, all, custom
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}

/// <summary>
/// Reference range for a lab test (for graph overlay lines).
/// </summary>
public class LabRefRange
{
    public string TestName { get; set; } = "";
    public double? Low { get; set; }
    public double? High { get; set; }
    public string Units { get; set; } = "";
}

/// <summary>
/// Microbiology culture result.
/// </summary>
public class LabMicroResult
{
    public string Ien { get; set; } = "";
    public DateTime? CollectionDate { get; set; }
    public string Specimen { get; set; } = "";
    public string Accession { get; set; } = "";
    public string Provider { get; set; } = "";
    public string Site { get; set; } = "";
    public List<LabMicroOrganism> Organisms { get; set; } = [];
    public string Comment { get; set; } = "";
}

/// <summary>
/// Organism identified within a micro culture with antibiotic sensitivities.
/// </summary>
public class LabMicroOrganism
{
    public string Name { get; set; } = "";
    public string Quantity { get; set; } = "";
    public List<LabMicroSensitivity> Sensitivities { get; set; } = [];
}

/// <summary>
/// Antibiotic sensitivity result for an organism.
/// </summary>
public class LabMicroSensitivity
{
    public string Antibiotic { get; set; } = "";
    public string Interpretation { get; set; } = ""; // S=Susceptible, R=Resistant, I=Intermediate
    public string MIC { get; set; } = "";             // Minimum inhibitory concentration
}

/// <summary>
/// Vitals grid data from GMV ORQQVI1 GRID.
/// </summary>
public class VitalsGridData
{
    public string Dfn { get; set; } = "";
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public List<VitalsGridRow> Rows { get; set; } = [];
}

/// <summary>
/// A row in the vitals grid display.
/// </summary>
public class VitalsGridRow
{
    public DateTime? DateTime { get; set; }
    public string Temperature { get; set; } = "";
    public string Pulse { get; set; } = "";
    public string Respiration { get; set; } = "";
    public string BloodPressure { get; set; } = "";
    public string Height { get; set; } = "";
    public string Weight { get; set; } = "";
    public string Pain { get; set; } = "";
    public string PulseOximetry { get; set; } = "";
    public string BMI { get; set; } = "";
}

/// <summary>
/// Request body for interim selected tests.
/// Maps to: InterimSelect in rLabs.pas — ORWLRR INTERIMS.
/// </summary>
public class LabSelectedTestsRequest
{
    public string Date1 { get; set; } = "";
    public string Date2 { get; set; } = "";
    public List<string> Tests { get; set; } = [];
}

/// <summary>
/// Request body for VistA device printing.
/// Maps to: PrintLabsToDevice in rLabs.pas — ORWRP PRINT LAB REPORTS.
/// </summary>
public class LabPrintRequest
{
    public string Device { get; set; } = "";
    public string Report { get; set; } = "";
    public int DaysBack { get; set; } = 365;
    public List<string> Tests { get; set; } = [];
}
