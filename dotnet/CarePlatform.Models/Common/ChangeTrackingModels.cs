namespace CarePlatform.Models.Common;

/// <summary>
/// Represents a single pending change (unsigned document, order, or PCE item)
/// that needs to be reviewed/signed before the user can exit or switch patients.
/// Maps to Delphi TChangeItem in uCore.pas.
/// </summary>
public class ChangeItem
{
    /// <summary>
    /// Category of this change item (CH_DOC, CH_SUM, CH_CON, CH_SUR, CH_ORD, CH_PCE).
    /// Use <see cref="CprsConstants"/> values or <see cref="ChangeItemType"/> enum.
    /// </summary>
    public int ItemType { get; set; }

    /// <summary>Unique identifier for the item (IEN or composite key).</summary>
    public string Id { get; set; } = "";

    /// <summary>Display text shown to the user.</summary>
    public string Text { get; set; } = "";

    /// <summary>Group name for categorizing items (e.g. "Other Unsigned" for orders).</summary>
    public string GroupName { get; set; } = "";

    /// <summary>
    /// Signature state: CH_SIGN_NA (0), CH_SIGN_YES (1), CH_SIGN_NO (2).
    /// </summary>
    public int SignState { get; set; } = CprsConstants.CH_SIGN_NA;

    /// <summary>Parent document ID (for addenda linked to parent notes).</summary>
    public string ParentId { get; set; } = "";

    /// <summary>DUZ of the user who created this item.</summary>
    public long User { get; set; }

    /// <summary>Order display group identifier.</summary>
    public string OrderDisplayGroup { get; set; } = "";

    /// <summary>Whether this is a discontinue order.</summary>
    public bool IsDcOrder { get; set; }

    /// <summary>Whether this is a delayed order (event-delayed ordering).</summary>
    public bool IsDelayed { get; set; }
}

/// <summary>
/// Tracks all pending changes (unsigned/uncosigned documents, orders, PCE items)
/// for the current session. The client uses this to present the review/sign dialog
/// when switching patients or exiting.
/// Maps to Delphi TChanges in uCore.pas.
/// </summary>
public class PendingChanges
{
    /// <summary>Total count of pending change items across all lists.</summary>
    public int Count { get; set; }

    /// <summary>Unsigned TIU documents, discharge summaries, consult notes, surgery reports.</summary>
    public List<ChangeItem> Documents { get; set; } = [];

    /// <summary>Orders pending signature or review.</summary>
    public List<ChangeItem> Orders { get; set; } = [];

    /// <summary>Patient Care Encounter (PCE) items pending save.</summary>
    public List<ChangeItem> Pce { get; set; } = [];

    /// <summary>Order group names for grouping orders in the sign dialog.</summary>
    public List<string> OrderGroups { get; set; } = [];

    /// <summary>PCE group names for grouping encounter items.</summary>
    public List<string> PceGroups { get; set; } = [];

    /// <summary>Whether a cover sheet / problem list refresh is needed after signing.</summary>
    public bool RefreshCoverProblemList { get; set; }

    /// <summary>Whether the problem list specifically needs refresh after signing.</summary>
    public bool RefreshProblemList { get; set; }

    /// <summary>
    /// Whether any items can be signed (i.e., at least one item has SignState != CH_SIGN_NA).
    /// </summary>
    public bool CanSign =>
        Documents.Exists(d => d.SignState != CprsConstants.CH_SIGN_NA) ||
        Orders.Exists(o => o.SignState != CprsConstants.CH_SIGN_NA);

    /// <summary>
    /// Whether there are items requiring review before patient switch or exit.
    /// </summary>
    public bool RequiresReview =>
        Orders.Count > 0 ||
        Documents.Exists(d => d.SignState != CprsConstants.CH_SIGN_NA);
}

/// <summary>
/// Request to add a change item to the pending changes list.
/// </summary>
public class AddChangeRequest
{
    public int ItemType { get; set; }
    public string Id { get; set; } = "";
    public string Text { get; set; } = "";
    public string GroupName { get; set; } = "";
    public int SignState { get; set; }
    public string ParentId { get; set; } = "";
    public long User { get; set; }
    public string OrderDisplayGroup { get; set; } = "";
    public bool IsDcOrder { get; set; }
    public bool IsDelayed { get; set; }
}

/// <summary>
/// Request to update the signature state of a pending change item.
/// </summary>
public class UpdateSignStateRequest
{
    public int ItemType { get; set; }
    public string Id { get; set; } = "";
    public int NewSignState { get; set; }
}
