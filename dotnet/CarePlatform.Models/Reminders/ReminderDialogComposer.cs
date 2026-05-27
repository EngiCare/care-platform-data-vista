using System.Text;

namespace CarePlatform.Models.Reminders;

/// <summary>
/// Walks a parsed <see cref="ReminderDialogDefinition"/> + user
/// <see cref="ReminderDialogResponse.Responses"/> dictionary and produces:
/// <list type="bullet">
/// <item>the boilerplate text that should be appended to the note body, and</item>
/// <item>the list of PCE finding writes that should be persisted via the
/// canonical <c>PXRMRPCG GENFUPD</c> RPC.</item>
/// </list>
/// Mirrors cprs/uReminders.pas <c>TRemDlgElement</c> composition rules so the
/// web client and the desktop CPRS client emit equivalent note text and PCE
/// writes for the same user inputs.
/// </summary>
public static class ReminderDialogComposer
{
    public static ReminderDialogProcessResult Compose(
        ReminderDialogDefinition? def,
        IDictionary<string, string>? responses)
    {
        var result = new ReminderDialogProcessResult();
        if (def == null || responses == null || responses.Count == 0)
            return result;

        var text = new StringBuilder();
        ProcessElements(def.Elements, responses, text, result.PceWritten);
        result.GeneratedText = text.ToString().TrimEnd('\r', '\n');
        return result;
    }

    /// <summary>
    /// Recursively process a list of reminder-dialog elements. Mirrors the
    /// desktop CPRS uReminders.pas walk that calls <c>EnableChildren</c> on a
    /// parent Check element: child elements are only composed when the parent
    /// Check is selected. Per-choice branching for Radio mirrors
    /// <c>SetChecked</c> + <c>ChildrenRequired = crOne</c>.
    /// </summary>
    private static void ProcessElements(
        List<ReminderDialogElementDefinition> elements,
        IDictionary<string, string> responses,
        StringBuilder text,
        List<PceFindingWrite> pceWritten)
    {
        foreach (var el in elements.OrderBy(e => e.Sequence))
        {
            var hasValue = responses.TryGetValue(el.Ien, out var value) && !string.IsNullOrEmpty(value);
            var parentChecked = false;

            // Any element that isn't Radio/Static/Header is rendered as a
            // parent checkbox with its TRemPrompt children nested underneath.
            var isCheckboxRendered = el.PromptType != "Radio" &&
                                     el.PromptType != "Static" &&
                                     !el.IsHeader;
            var hasPrompts = el.Prompts != null && el.Prompts.Count > 0;

            if (hasValue && isCheckboxRendered && hasPrompts)
            {
                if (IsTruthy(value))
                {
                    parentChecked = true;
                    EmitParentCheck(el, text, pceWritten);
                    AppendPromptResponses(el, responses, text);
                }
            }
            else if (hasValue)
            {
                switch (el.PromptType)
                {
                    case "Radio":
                    case "Combo":
                    {
                        var choice = el.Choices.FirstOrDefault(c =>
                            string.Equals(c.Code, value, StringComparison.OrdinalIgnoreCase));
                        if (choice == null) break;
                        if (!string.IsNullOrEmpty(choice.Boilerplate))
                            text.AppendLine(choice.Boilerplate);
                        else if (!string.IsNullOrEmpty(choice.Label))
                            text.AppendLine(choice.Label);
                        var ft = string.IsNullOrEmpty(choice.FindingType) ? el.FindingType : choice.FindingType;
                        var fi = string.IsNullOrEmpty(choice.FindingItem) ? choice.Code : choice.FindingItem;
                        AddPceFinding(ft, fi, choice.Label, pceWritten);
                        if (choice.Children != null && choice.Children.Count > 0)
                            ProcessElements(choice.Children, responses, text, pceWritten);
                        break;
                    }
                    case "Check":
                    {
                        if (!IsTruthy(value)) break;
                        parentChecked = true;
                        EmitParentCheck(el, text, pceWritten);
                        AppendPromptResponses(el, responses, text);
                        break;
                    }
                    case "FreeText":
                    case "Date":
                    case "Numeric":
                        text.AppendLine($"{el.Caption}: {value}");
                        break;
                    case "Static":
                        if (!string.IsNullOrEmpty(el.Boilerplate))
                            text.AppendLine(el.Boilerplate);
                        break;
                }
            }

            // Recurse into normal nested children. For checkbox-rendered
            // parents only include children when the parent is selected
            // (mirrors EnableChildren in cprs/uReminders.pas).
            if (el.Children != null && el.Children.Count > 0)
            {
                if (!isCheckboxRendered || parentChecked)
                    ProcessElements(el.Children, responses, text, pceWritten);
            }
        }
    }

    private static void EmitParentCheck(
        ReminderDialogElementDefinition el,
        StringBuilder text,
        List<PceFindingWrite> pceWritten)
    {
        if (!string.IsNullOrEmpty(el.Boilerplate))
            text.AppendLine(el.Boilerplate);
        else if (!string.IsNullOrEmpty(el.Caption))
            text.AppendLine(el.Caption);
        AddPceFinding(el.FindingType, el.FindingItem, el.Caption, pceWritten);
    }

    private static void AppendPromptResponses(
        ReminderDialogElementDefinition el,
        IDictionary<string, string> responses,
        StringBuilder text)
    {
        if (el?.Prompts == null || el.Prompts.Count == 0) return;
        foreach (var p in el.Prompts.OrderBy(pp => pp.Sequence))
        {
            var key = $"{el.Ien}::{p.Code}";
            if (!responses.TryGetValue(key, out var v) || string.IsNullOrWhiteSpace(v))
                continue;
            var caption = string.IsNullOrWhiteSpace(p.Caption) ? p.Code : p.Caption;
            text.AppendLine($"{caption}: {v}");
        }
    }

    private static void AddPceFinding(string findingType, string findingItem, string label, List<PceFindingWrite> pceWritten)
    {
        if (string.IsNullOrWhiteSpace(findingType) || string.IsNullOrWhiteSpace(findingItem))
            return;
        pceWritten.Add(new PceFindingWrite
        {
            Type = findingType,
            Item = findingItem,
            Label = label ?? ""
        });
    }

    private static bool IsTruthy(string value) =>
        string.Equals(value, "1", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
}
