// Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
using CarePlatform.Models.Common;

namespace CarePlatform.Models.Medications;

/// <summary>
/// Medication record parsed from ORWPS ACTIVE blocks.
/// Blocks start with ~OrderId and contain line items.
/// </summary>
public class Medication
{
    public string OrderId { get; set; } = "";
    public string DrugName { get; set; } = "";
    public string Sig { get; set; } = "";
    public string Status { get; set; } = "";
    public string Type { get; set; } = "";           // IP, OP, NV (Inpatient, Outpatient, Non-VA)
    public DateTime? StartDate { get; set; }
    public DateTime? StopDate { get; set; }
    public DateTime? LastFillDate { get; set; }
    public int Refills { get; set; }
    public int RefillsRemaining { get; set; }
    public int DaysSupply { get; set; }
    public int Quantity { get; set; }
    public string PharmacyText { get; set; } = "";
    public string Provider { get; set; } = "";
    public string Clinic { get; set; } = "";
    public string Schedule { get; set; } = "";
    public string Route { get; set; } = "";
    public bool IsNonVA { get; set; }
    public bool IsUnitDose { get; set; }
    public bool IsIV { get; set; }
    public List<string> DetailLines { get; set; } = [];
}

/// <summary>
/// Medication view configuration from the first line of ORWPS ACTIVE.
/// </summary>
public class MedicationViewConfig
{
    public string View { get; set; } = "";
    public string DateRange { get; set; } = "";
    public string DateRangeInpatient { get; set; } = "";
    public string DateRangeOutpatient { get; set; } = "";
}

/// <summary>
/// Complete medications tab data.
/// </summary>
public class MedicationsTabData
{
    public MedicationViewConfig ViewConfig { get; set; } = new();
    public List<Medication> OutpatientMeds { get; set; } = [];
    public List<Medication> InpatientMeds { get; set; } = [];
    public List<Medication> NonVAMeds { get; set; } = [];

    /// <summary>
    /// Convenience property: all medications combined.
    /// </summary>
    public List<Medication> Medications { get; set; } = [];
}
