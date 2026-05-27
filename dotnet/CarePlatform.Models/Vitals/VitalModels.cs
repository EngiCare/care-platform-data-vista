// Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
namespace CarePlatform.Models.Vitals;

/// <summary>
/// Vital sign measurement.
/// </summary>
public class VitalSign
{
    public string Type { get; set; } = "";          // T, P, R, BP, Ht, Wt, Pain, POx, etc.
    public string TypeName { get; set; } = "";
    public string Value { get; set; } = "";
    public string Units { get; set; } = "";
    public DateTime? DateTime { get; set; }
    public string Qualifier { get; set; } = "";
    public string EnteredBy { get; set; } = "";
    public string Location { get; set; } = "";
}

/// <summary>
/// Vitals store request (one set of vitals at a point in time).
/// </summary>
public class VitalsStoreRequest
{
    public string Dfn { get; set; } = "";
    public string LocationIen { get; set; } = "";
    public string DateTime { get; set; } = "";
    public Dictionary<string, string> Values { get; set; } = new();  // Type → Value
}

/// <summary>
/// Request to mark a vital sign as entered-in-error.
/// </summary>
public class MarkErrorRequest
{
    public string VitalIen { get; set; } = "";
    public string Reason { get; set; } = "";
}
