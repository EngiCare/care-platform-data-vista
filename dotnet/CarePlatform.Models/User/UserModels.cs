// Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
using CarePlatform.Models.Common;

namespace CarePlatform.Models.User;

/// <summary>
/// User info from ORWU USERINFO.
/// Piece: DUZ^NAME^USRCLS^CANSIGN^ISPROVIDER^ORDERROLE^NOORDER^DTIME^CNTDN^VERORD^
///        NOTIFYAPPS^MSGHANG^DOMAIN^SERVICE^AUTOSAVE^INITTAB^LASTTAB^WEBACCESS^
///        ALLOWHOLD^ISRPL^RPLLIST^CORTABS^RPTTAB^STATION#^GECStatus^Production^
///        EnableActOneStep^JobNumber
/// </summary>
public class UserInfo
{
    public long Duz { get; set; }
    public string Name { get; set; } = "";
    public string UserClass { get; set; } = "";
    public bool CanSign { get; set; }
    public bool IsProvider { get; set; }
    public string OrderRole { get; set; } = "";
    public bool NoOrderEntry { get; set; }
    public int DTime { get; set; }
    public int CountDown { get; set; }
    public bool VerifyOrders { get; set; }
    public int NotifyApps { get; set; }
    public int MessageHang { get; set; }
    public string Domain { get; set; } = "";
    public string Service { get; set; } = "";
    public int AutoSaveInterval { get; set; }
    public int InitialTab { get; set; }
    public int LastTab { get; set; }
    public bool WebAccess { get; set; }
    public bool AllowHold { get; set; }
    public bool IsRpl { get; set; }
    public string RplList { get; set; } = "";
    public string CorTabs { get; set; } = "";
    public string RptTab { get; set; } = "";
    public string StationNumber { get; set; } = "";
    public string GecStatus { get; set; } = "";
    public bool IsProduction { get; set; }
    public bool EnableActionOneStep { get; set; }

    public static UserInfo Parse(string vistaString)
    {
        var p = VistaStringParser.Split(vistaString);
        return new UserInfo
        {
            Duz = p.Length > 0 ? VistaStringParser.ParseLong(p[0]) : 0,
            Name = p.Length > 1 ? p[1] : "",
            UserClass = p.Length > 2 ? p[2] : "",
            CanSign = p.Length > 3 && VistaStringParser.ParseBool(p[3]),
            IsProvider = p.Length > 4 && VistaStringParser.ParseBool(p[4]),
            OrderRole = p.Length > 5 ? p[5] : "",
            NoOrderEntry = p.Length > 6 && VistaStringParser.ParseBool(p[6]),
            DTime = p.Length > 7 ? VistaStringParser.ParseInt(p[7]) : 300,
            CountDown = p.Length > 8 ? VistaStringParser.ParseInt(p[8]) : 10,
            VerifyOrders = p.Length > 9 && VistaStringParser.ParseBool(p[9]),
            NotifyApps = p.Length > 10 ? VistaStringParser.ParseInt(p[10]) : 0,
            MessageHang = p.Length > 11 ? VistaStringParser.ParseInt(p[11]) : 0,
            Domain = p.Length > 12 ? p[12] : "",
            Service = p.Length > 13 ? p[13] : "",
            AutoSaveInterval = p.Length > 14 ? VistaStringParser.ParseInt(p[14]) : 180,
            InitialTab = p.Length > 15 ? VistaStringParser.ParseInt(p[15]) : 1,
            LastTab = p.Length > 16 ? VistaStringParser.ParseInt(p[16]) : 0,
            WebAccess = p.Length > 17 && VistaStringParser.ParseBool(p[17]),
            AllowHold = p.Length > 18 && VistaStringParser.ParseBool(p[18]),
            IsRpl = p.Length > 19 && VistaStringParser.ParseBool(p[19]),
            RplList = p.Length > 20 ? p[20] : "",
            CorTabs = p.Length > 21 ? p[21] : "",
            RptTab = p.Length > 22 ? p[22] : "",
            StationNumber = p.Length > 23 ? p[23] : "",
            GecStatus = p.Length > 24 ? p[24] : "",
            IsProduction = p.Length > 25 && VistaStringParser.ParseBool(p[25]),
            EnableActionOneStep = p.Length > 26 && VistaStringParser.ParseBool(p[26])
        };
    }
}
