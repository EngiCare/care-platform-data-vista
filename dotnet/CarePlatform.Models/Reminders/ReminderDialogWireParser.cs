namespace CarePlatform.Models.Reminders;

/// <summary>
/// Parses the caret-delimited line list returned by canonical
/// <c>PXRM REMINDER DIALOG (TIU)</c> / <c>ORQQPXRM REMINDER DIALOG</c> into a
/// strongly-typed <see cref="ReminderDialogDefinition"/> the web client can
/// render directly.
/// </summary>
/// <remarks>
/// Row layout (cprs/uReminders.pas L2584-L2660):
/// <list type="bullet">
/// <item>Header (idx 0): <c>&lt;ien&gt;^&lt;wipe&gt;^&lt;name&gt;</c></item>
/// <item>Type 1 (element, 27 pieces): <c>1^FID^FDlgID^TypeCode^^Indent^FindingType^Hist^^^^^^^HideChildren^ChildrenIndent^ChildrenSharePrompts^ChildrenRequired^^^^^^^GetDataSkip^^</c></item>
/// <item>Type 2 (caption): <c>2^FID^FDlgID^Caption</c></item>
/// <item>Type 3 (finding): <c>3^FID^^DataTypeCode^^^FindingCode^Narrative^Category^OrderSubtype^^^^...</c></item>
/// <item>Type 4 (prompt): <c>4^FID^^Code^^Default^Force^Caption^1^Required^^^^Validate^^^^^^</c></item>
/// </list>
/// Children are linked via dotted FDlgID path (parent <c>E5</c>, child <c>E5.E6</c>).
/// Radio elements are detected by ChildrenRequired ∈ {1, 3} — their dotted
/// children become <see cref="ReminderDialogElementDefinition.Choices"/>.
/// </remarks>
public static class ReminderDialogWireParser
{
    public static ReminderDialogDefinition Parse(IEnumerable<string> lines)
    {
        var def = new ReminderDialogDefinition();
        var byFid = new Dictionary<string, ReminderDialogElementDefinition>(StringComparer.OrdinalIgnoreCase);
        var dlgIdByFid = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var ordered = new List<ReminderDialogElementDefinition>();
        var explicitTypeCodes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var promptCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        var first = true;
        foreach (var raw in lines)
        {
            if (raw is null) continue;
            var line = raw.TrimEnd('\r', '\n');
            if (line.Length == 0) continue;
            var p = line.Split('^');

            if (first)
            {
                // Header: <ien>^<wipe>^<name>
                def.Ien = Get(p, 0);
                def.Name = Get(p, 2);
                first = false;
                continue;
            }

            switch (Get(p, 0))
            {
                case "1":
                    {
                        var fid = Get(p, 1);
                        var dlgId = Get(p, 2);
                        if (string.IsNullOrEmpty(fid)) break;

                        var typeCode = Get(p, 3);
                        var findingType = Get(p, 6);
                        var historical = Get(p, 7) == "1";
                        var hideChildren = Get(p, 14) == "1";
                        var sharePrompts = Get(p, 16) == "1";
                        var childrenRequired = Get(p, 17);
                        // GetDataSkip is piece 25 (index 24): "1" ⇒ has prompts.
                        // We don't store it directly; presence is later inferred
                        // from accumulated type-4 rows.
                        // PromptType derived later once we know prompt codes.
                        var elem = new ReminderDialogElementDefinition
                        {
                            Ien = fid,
                            ElemTypeCode = typeCode,
                            Historical = historical,
                            HideWhenDisabled = hideChildren,
                            ChildrenSharePrompts = sharePrompts,
                            ChildrenRequired = childrenRequired,
                            FindingType = findingType,
                            Sequence = ordered.Count + 1,
                            // Provisional — refined after we see all prompts.
                            PromptType = DerivePromptType(typeCode, childrenRequired, promptCode: null)
                        };
                        explicitTypeCodes[fid] = typeCode;
                        byFid[fid] = elem;
                        dlgIdByFid[fid] = dlgId;
                        ordered.Add(elem);
                        break;
                    }
                case "2":
                    {
                        var fid = Get(p, 1);
                        if (byFid.TryGetValue(fid, out var elem))
                            elem.Caption = Get(p, 3);
                        break;
                    }
                case "3":
                    {
                        var fid = Get(p, 1);
                        if (!byFid.TryGetValue(fid, out var elem)) break;
                        elem.Findings.Add(new ReminderFindingDefinition
                        {
                            DataTypeCode = Get(p, 3),
                            FindingCode  = Get(p, 6),
                            Narrative    = Get(p, 7),
                            Category     = Get(p, 8),
                            OrderSubtype = Get(p, 9),
                            Code2        = Get(p, 12)
                        });
                        // Treat the type-3 row as the FindingItem source so the
                        // round-tripped definition can drive PCE writes.
                        if (string.IsNullOrEmpty(elem.FindingItem))
                            elem.FindingItem = Get(p, 6);
                        // Element-level FindingType (type-1 piece 6) is only
                        // populated by the desktop emitter for elements whose
                        // PCE category is *implicit* (HF taxonomy, radio
                        // groups). Concrete check elements that carry their
                        // category via the type-3 DataTypeCode (POV/CPT/IMM/
                        // SK/PED/HF/GFIND/EX/WHR/...) leave piece 6 empty —
                        // hoist it from the first type-3 row so the composer
                        // can drive PCE writes.
                        if (string.IsNullOrEmpty(elem.FindingType))
                            elem.FindingType = Get(p, 3);
                        break;
                    }
                case "4":
                    {
                        var fid = Get(p, 1);
                        if (!byFid.TryGetValue(fid, out var elem)) break;
                        var code = Get(p, 3);
                        elem.Prompts.Add(new ReminderPromptDefinition
                        {
                            Code = code,
                            Default = Get(p, 5),
                            Forced = Get(p, 6) == "F",
                            Caption = Get(p, 7),
                            Required = Get(p, 9) == "1",
                            Validate = Get(p, 14),
                            Sequence = elem.Prompts.Count + 1
                        });
                        promptCounts.TryGetValue(fid, out var n);
                        promptCounts[fid] = n + 1;
                        // Refine PromptType now that we know a prompt code.
                        elem.PromptType = DerivePromptType(
                            explicitTypeCodes.GetValueOrDefault(fid, ""),
                            elem.ChildrenRequired,
                            code);
                        break;
                    }
            }
        }

        // Header detection: a "D" element whose caption looks like "--- ... ---"
        // is rendered as a section heading by the desktop client.
        foreach (var elem in ordered)
        {
            if (elem.PromptType == "Static" &&
                elem.Caption.StartsWith("--- ", StringComparison.Ordinal) &&
                elem.Caption.EndsWith(" ---", StringComparison.Ordinal))
            {
                elem.IsHeader = true;
            }
            // Required hint — the emitter appends " *" to the caption when an
            // element with a non-Radio prompt is required. Strip and surface.
            if (elem.Caption.EndsWith(" *", StringComparison.Ordinal))
            {
                elem.Required = true;
                elem.Caption = elem.Caption[..^2];
            }
        }

        // Reconstruct hierarchy from dotted FDlgID paths. A child's FDlgID is
        // <parentDlgId>.<childFid>, so the parent's FID is the segment before
        // the last dot. Top-level elements (FDlgID == FID) are root.
        foreach (var (fid, elem) in byFid)
        {
            var dlgId = dlgIdByFid[fid];
            var lastDot = dlgId.LastIndexOf('.');
            if (lastDot < 0)
            {
                def.Elements.Add(elem);
                continue;
            }
            var parentDlgId = dlgId[..lastDot];
            var parentFidEnd = parentDlgId.LastIndexOf('.');
            var parentFid = parentFidEnd < 0 ? parentDlgId : parentDlgId[(parentFidEnd + 1)..];
            if (!byFid.TryGetValue(parentFid, out var parent))
            {
                def.Elements.Add(elem);
                continue;
            }

            // Radio detection: ChildrenRequired ∈ {1,3} ⇒ children are Choices.
            if (parent.ChildrenRequired is "1" or "3")
            {
                parent.PromptType = "Radio";
                parent.Required = parent.ChildrenRequired == "1";
                parent.Choices.Add(new ReminderDialogChoice
                {
                    Code = elem.Ien,
                    Label = elem.Caption,
                    FindingType = string.IsNullOrEmpty(elem.FindingType)
                        ? parent.FindingType
                        : elem.FindingType,
                    // Recover the choice's source FindingItem from the type-3
                    // row the emitter writes for each radio-child element so
                    // PCE writes target the real finding code (e.g. "HF001")
                    // rather than the synthetic FID ("E47").
                    FindingItem = elem.FindingItem,
                    HideWhenDisabled = elem.HideWhenDisabled
                });
                // The child element is now subsumed into the parent's Choices —
                // don't add it as a normal Children entry (it has no presence
                // in def.Elements either).
            }
            else
            {
                parent.Children.Add(elem);
            }
        }

        // Sort element collections by their original wire order.
        foreach (var elem in byFid.Values)
        {
            if (elem.Children.Count > 1)
                elem.Children.Sort((a, b) => a.Sequence.CompareTo(b.Sequence));
        }

        return def;
    }

    private static string Get(string[] p, int i) => i < p.Length ? p[i] : "";

    /// <summary>
    /// Reverse of the EmitElement type-derivation table. Best-effort — Numeric
    /// and FreeText collapse to FreeText (both emit COM); Group and Static
    /// collapse to Static (both emit "D" with no prompt).
    /// </summary>
    private static string DerivePromptType(string typeCode, string childrenRequired, string? promptCode)
    {
        if (childrenRequired is "1" or "3") return "Radio";
        switch (typeCode)
        {
            case "D":
                return "Static";
            case "H":
            case "T":
            case "":
                return "Check";
            case "C":
                return promptCode switch
                {
                    "DATE" or "DATE_TIME" or "VST_DATE" => "Date",
                    "VST_LOC" => "Combo",
                    "COM" => "FreeText",
                    null => "Check",   // C with no prompt yet — treat as check; refined when prompt seen
                    _ => "Check"        // C with a non-canonical prompt code (POV_PRIM, CPT_QTY, etc.)
                };
            default:
                return "Static";
        }
    }
}
