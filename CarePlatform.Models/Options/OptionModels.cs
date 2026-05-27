namespace CarePlatform.Models.Options;

/// <summary>
/// User option/preference settings.
/// </summary>
public class UserOptions
{
    public PatientSelectionOptions PatientSelection { get; set; } = new();
    public NotificationOptions Notifications { get; set; } = new();
    public OrderOptions Orders { get; set; } = new();
    public NoteOptions Notes { get; set; } = new();
    public ReportOptions Reports { get; set; } = new();
    public OtherOptions Other { get; set; } = new();
}

public class PatientSelectionOptions
{
    public string DefaultListSource { get; set; } = "";
    public string ClinicStartDays { get; set; } = "";
    public string ClinicStopDays { get; set; } = "";
    public string DefaultProvider { get; set; } = "";
}

public class NotificationOptions
{
    public bool EnableNotifications { get; set; } = true;
    public string SortBy { get; set; } = "";
    public string Surrogate { get; set; } = "";
    public DateTime? SurrogateStart { get; set; }
    public DateTime? SurrogateEnd { get; set; }
}

public class OrderOptions
{
    public bool VerifyOrders { get; set; }
    public string OrderChecksLevel { get; set; } = "";
    public bool ShowExpiredMeds { get; set; }
    public int MedOrderDays { get; set; }
}

public class NoteOptions
{
    public string DefaultTitle { get; set; } = "";
    public long DefaultCosigner { get; set; }
    public string CosignerName { get; set; } = "";
    public int AutoSaveInterval { get; set; } = 180;
    public string DefaultView { get; set; } = "";
}

public class ReportOptions
{
    public int MaxOccurrences { get; set; }
    public string DefaultReport { get; set; } = "";
}

public class OtherOptions
{
    public bool EnableCCOW { get; set; }
    public string StartupPage { get; set; } = "";
    public bool UseLastTab { get; set; }
}
