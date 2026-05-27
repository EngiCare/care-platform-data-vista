// Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
namespace CarePlatform.Models.Reminders;

/// <summary>
/// Clinical reminder item.
/// </summary>
public class Reminder
{
    public string Ien { get; set; } = "";
    public string Name { get; set; } = "";
    public string Status { get; set; } = "";       // DUE, APPLICABLE, N/A, DONE, etc.
    public DateTime? DueDate { get; set; }
    public DateTime? LastOccurrence { get; set; }
    public string Priority { get; set; } = "";
    public string Frequency { get; set; } = "";
}

/// <summary>
/// Reminder dialog definition for evaluation.
/// </summary>
public class ReminderDialog
{
    public string Ien { get; set; } = "";
    public string Name { get; set; } = "";
    public List<ReminderDialogElement> Elements { get; set; } = [];
}

/// <summary>
/// A single element in a reminder dialog.
/// Maps to uReminders.pas TRemDlgElement.
/// </summary>
public class ReminderDialogElement
{
    public string Id { get; set; } = "";
    public string DlgId { get; set; } = "";
    public string TaxId { get; set; } = "";
    public string Type { get; set; } = "";         // checkbox, combo, text, date, MH, etc.
    public string Label { get; set; } = "";
    public string Default { get; set; } = "";
    public bool Required { get; set; }
    public bool IsChecked { get; set; }
    public bool HasComment { get; set; }
    public bool ChildrenShareChecked { get; set; }
    public bool HasSharedPrompts { get; set; }
    public List<string> Choices { get; set; } = [];
    public List<ReminderDialogElement> Children { get; set; } = [];
    public List<ReminderData> Data { get; set; } = [];
    public List<ReminderPrompt> Prompts { get; set; } = [];
}

/// <summary>
/// PCE data item from a reminder element.
/// Maps to uReminders.pas TRemData.
/// </summary>
public class ReminderData
{
    public string Id { get; set; } = "";
    public string DataType { get; set; } = "";     // DX=diagnosis, PRC=procedure, IMM=immunization,
                                                    // SK=skin test, PED=patient ed, HF=health factor,
                                                    // EX=exam, VIT=vitals, ORD=order, MH=mental health,
                                                    // WH=women's health
    public string PceRoot { get; set; } = "";
    public string InternalValue { get; set; } = "";
    public string ExternalValue { get; set; } = "";
    public string Narrative { get; set; } = "";
    public string Category { get; set; } = "";
    public bool AddToProgressNote { get; set; }
    public List<string> ActiveDates { get; set; } = [];
    public List<ReminderDataChoice> Choices { get; set; } = [];
    public int ChoicesMin { get; set; }
    public int ChoicesMax { get; set; }
}

/// <summary>
/// A selectable choice within a reminder data item.
/// </summary>
public class ReminderDataChoice
{
    public string Id { get; set; } = "";
    public string Text { get; set; } = "";
    public bool IsChecked { get; set; }
    public List<string> ActiveDates { get; set; } = [];
}

/// <summary>
/// Prompt definition within a reminder element.
/// Maps to uReminders.pas TRemPrompt.
/// </summary>
public class ReminderPrompt
{
    public string Id { get; set; } = "";
    public string PromptType { get; set; } = "";   // FreeText, Date, Number, Combo, etc.
    public string Value { get; set; } = "";
    public string Caption { get; set; } = "";
    public bool Required { get; set; }
    public bool IsShared { get; set; }
    public string OverrideType { get; set; } = "";
    public string MiscText { get; set; } = "";
    public List<string> Choices { get; set; } = [];
}
