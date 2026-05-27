// Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
namespace CarePlatform.Models.Billing;

/// <summary>
/// Billing-Aware diagnosis record — maps to UBACore.pas / fBALocalDiagnoses.pas.
/// Used for associating diagnoses with orders for billing compliance.
/// </summary>
public class BillingDiagnosis
{
    public string Code { get; set; } = "";
    public string Narrative { get; set; } = "";
    public bool IsPrimary { get; set; }
    public string Source { get; set; } = "";        // PL=Problem List, PD=Personal Dx, EF=Encounter Form
    public bool IsInactive { get; set; }
    public string OrderId { get; set; } = "";
}

/// <summary>
/// Treatment factor flags for billing — maps to UBAConst.pas.
/// </summary>
public class TreatmentFactors
{
    public string ServiceConnected { get; set; } = "";     // SC or NSC
    public bool AgentOrange { get; set; }                   // AO
    public bool IonizingRadiation { get; set; }             // IR
    public bool EnvironmentalContaminant { get; set; }      // EC
    public bool HeadNeckCancer { get; set; }                // HNC
    public bool MilitarySexualTrauma { get; set; }          // MST
    public bool CombatVeteran { get; set; }                 // CV
    public bool ShipboardHazardDefense { get; set; }        // SHD
    public bool CampLejeune { get; set; }                   // CL
}

/// <summary>
/// Billing-aware order signing context.
/// Tracks diagnoses and treatment factors for unsigned orders.
/// </summary>
public class BillingOrderContext
{
    public string OrderId { get; set; } = "";
    public bool IsBillable { get; set; }
    public List<BillingDiagnosis> Diagnoses { get; set; } = [];
    public TreatmentFactors TreatmentFactors { get; set; } = new();
}

/// <summary>
/// Personal diagnosis list item from fBALocalDiagnoses.pas.
/// </summary>
public class PersonalDiagnosis
{
    public string Code { get; set; } = "";
    public string Narrative { get; set; } = "";
    public bool IsInactive { get; set; }
}
