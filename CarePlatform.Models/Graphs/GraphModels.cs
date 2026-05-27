namespace CarePlatform.Models.Graphs;

/// <summary>
/// Graph data item from ORWGRPC FASTDATA / ORWGRPC FASTITEM.
/// </summary>
public class GraphItem
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Type { get; set; } = "";          // V=vitals, L=lab, M=med, etc.
    public string TypeName { get; set; } = "";
    public List<GraphDataPoint> DataPoints { get; set; } = [];
}

/// <summary>
/// A single data point for graphing.
/// </summary>
public class GraphDataPoint
{
    public DateTime? DateTime { get; set; }
    public double? Value { get; set; }
    public string TextValue { get; set; } = "";
    public string Flag { get; set; } = "";
    public double? HighRef { get; set; }
    public double? LowRef { get; set; }
}

/// <summary>
/// Graph view/profile saved by user from ORWGRPC GETVIEW.
/// </summary>
public class GraphView
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public List<string> ItemIds { get; set; } = [];
    public string DateRange { get; set; } = "";
}

/// <summary>
/// Graph settings from ORWGRPC GETPREF.
/// Maps to uGraphs.pas TGraphSetting.
/// </summary>
public class GraphSettings
{
    public string DateRange { get; set; } = "";
    public bool Show3D { get; set; }
    public bool ShowValues { get; set; }
    public bool ShowLegend { get; set; } = true;
    public bool DualView { get; set; }
    public string MaxGraphs { get; set; } = "";
    public string MinGraphHeight { get; set; } = "";
    public List<GraphView> Views { get; set; } = [];

    // ── From uGraphs.pas TGraphSetting ──
    public bool ClearBackground { get; set; }
    public bool ShowDates { get; set; } = true;
    public bool FixedDateRange { get; set; }
    public bool ShowGradient { get; set; }
    public bool ShowHints { get; set; } = true;
    public bool HorizontalZoom { get; set; } = true;
    public bool ShowLines { get; set; } = true;
    public bool ShowPoints { get; set; } = true;
    public bool SortByType { get; set; }
    public bool VerticalZoom { get; set; } = true;
    public bool StayOnTop { get; set; }
    public bool Turbo { get; set; }
    public bool MergeLabs { get; set; }
    public string DateRangeInpatient { get; set; } = "";
    public string DateRangeOutpatient { get; set; } = "";
    public string HighTime { get; set; } = "";
    public string LowTime { get; set; } = "";
    public int MaxSelect { get; set; }
    public int MaxSelectMin { get; set; }
    public int MaxSelectMax { get; set; }
    public string Sources { get; set; } = "";
    public int SortColumn { get; set; }
}

/// <summary>
/// Graph user activity state from uGraphs.pas TGraphActivity.
/// </summary>
public class GraphActivity
{
    public string CurrentSettingString { get; set; } = "";
    public string PublicSettingString { get; set; } = "";
    public string PersonalSettingString { get; set; } = "";
    public bool PublicEditor { get; set; }
    public string Status { get; set; } = "";
    public bool TurboOn { get; set; }
    public string Cache { get; set; } = "";
    public string OldDfn { get; set; } = "";
}
