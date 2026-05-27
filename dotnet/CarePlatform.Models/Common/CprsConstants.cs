namespace CarePlatform.Models.Common;

/// <summary>
/// Centralized CPRS constants migrated from Delphi uConst.pas.
/// These constants define the domain-specific codes, identifiers,
/// and enumerations used throughout the VistA/CPRS integration.
/// </summary>
public static class CprsConstants
{
    #region Chart Tab Indexes

    public const int CT_NOPAGE = -1;
    public const int CT_UNKNOWN = 0;
    public const int CT_COVER = 1;
    public const int CT_PROBLEMS = 2;
    public const int CT_MEDS = 3;
    public const int CT_ORDERS = 4;
    public const int CT_HP = 5;
    public const int CT_NOTES = 6;
    public const int CT_CONSULTS = 7;
    public const int CT_DCSUMM = 8;
    public const int CT_LABS = 9;
    public const int CT_REPORTS = 10;
    public const int CT_SURGERY = 11;

    #endregion

    #region Changes Object Item Types

    public const int CH_DOC = 10;   // TIU documents (progress notes)
    public const int CH_SUM = 12;   // Discharge Summaries
    public const int CH_CON = 15;   // Consults
    public const int CH_SUR = 18;   // Surgery reports
    public const int CH_ORD = 20;   // Orders
    public const int CH_PCE = 30;   // Encounter Form (PCE) items

    #endregion

    #region Changes Object Signature Requirements

    public const int CH_SIGN_NA = 0;    // Signature not applicable (checkbox greyed)
    public const int CH_SIGN_YES = 1;   // Obtain signature (checkbox checked)
    public const int CH_SIGN_NO = 2;    // Don't obtain signature (checkbox unchecked)

    #endregion

    #region Sign & Release Orders

    public const string SS_ONCHART = "0";
    public const string SS_ESIGNED = "1";
    public const string SS_UNSIGNED = "2";
    public const string SS_NOTREQD = "3";
    public const string SS_DIGSIG = "7";

    public const string RS_HOLD = "0";
    public const string RS_RELEASE = "1";

    public const string NO_PROVIDER = "E";
    public const string NO_VERBAL = "V";
    public const string NO_PHONE = "P";
    public const string NO_POLICY = "I";
    public const string NO_WRITTEN = "W";

    #endregion

    #region Actions on Orders

    public const int ORDER_NEW = 0;
    public const int ORDER_DC = 1;
    public const int ORDER_RENEW = 2;
    public const int ORDER_HOLD = 3;
    public const int ORDER_EDIT = 4;
    public const int ORDER_COPY = 5;
    public const int ORDER_QUICK = 9;
    public const int ORDER_ACT = 10;
    public const int ORDER_SIGN = 11;
    public const int ORDER_CPLXRN = 12;

    #endregion

    #region Order Action Codes

    public const string OA_COPY = "RW";
    public const string OA_CHANGE = "XX";
    public const string OA_RENEW = "RN";
    public const string OA_HOLD = "HD";
    public const string OA_DC = "DC";
    public const string OA_UNHOLD = "RL";
    public const string OA_FLAG = "FL";
    public const string OA_UNFLAG = "UF";
    public const string OA_COMPLETE = "CP";
    public const string OA_ALERT = "AL";
    public const string OA_REFILL = "RF";
    public const string OA_VERIFY = "VR";
    public const string OA_CHART = "CR";
    public const string OA_RELEASE = "RS";
    public const string OA_SIGN = "ES";
    public const string OA_ONCHART = "OC";
    public const string OA_COMMENT = "CM";
    public const string OA_TRANSFER = "XFR";
    public const string OA_CHGEVT = "EV";
    public const string OA_EDREL = "MN";

    #endregion

    #region Ordering Dialog Form IDs

    public const int OD_ACTIVITY = 100;
    public const int OD_ALLERGY = 105;
    public const int OD_CONSULT = 110;
    public const int OD_PROCEDURE = 112;
    public const int OD_DIET_TXT = 115;
    public const int OD_DIET = 117;
    public const int OD_LAB = 120;
    public const int OD_BB = 125;
    public const int OD_MEDINPT = 130;
    public const int OD_MEDS = 135;
    public const int OD_MEDOUTPT = 140;
    public const int OD_MEDNONVA = 145;
    public const int OD_NURSING = 150;
    public const int OD_MISC = 151;
    public const int OD_GENERIC = 152;
    public const int OD_IMAGING = 160;
    public const int OD_VITALS = 171;
    public const int OD_RTC = 175;
    public const int OD_MEDIV = 180;
    public const int OD_TEXTONLY = 999;
    public const int OD_CLINICMED = 1444;
    public const int OD_CLINICINF = 1555;
    public const int OD_AUTOACK = 9999;

    public const int OM_NAV = 1001;
    public const int OM_QUICK = 1002;
    public const int OM_TABBED = 1003;
    public const int OM_TREE = 1004;
    public const int OM_ALLERGY = 1105;
    public const int OM_HTML = 1200;

    #endregion

    #region Ordering Role

    public const int OR_NOKEY = 0;
    public const int OR_CLERK = 1;
    public const int OR_NURSE = 2;
    public const int OR_PHYSICIAN = 3;
    public const int OR_STUDENT = 4;
    public const int OR_BADKEYS = 5;

    #endregion

    #region Quick Orders

    public const int QL_DIALOG = 0;
    public const int QL_AUTO = 1;
    public const int QL_VERIFY = 2;
    public const int QL_REJECT = 8;
    public const int QL_CANCEL = 9;
    public const int MAX_KEYVARS = 10;

    #endregion

    #region Order Signature Statuses

    public const int OSS_UNSIGNED = 2;
    public const int OSS_NOT_REQUIRE = 3;

    #endregion

    #region Pharmacy Types

    public const string PST_UNIT_DOSE = "U";
    public const string PST_IV_FLUIDS = "F";
    public const string PST_OUTPATIENT = "O";

    #endregion

    #region Medication Status Groups

    public const int MED_ACTIVE = 0;    // Active status (active, hold, on call)
    public const int MED_PENDING = 1;   // Pending status (non-verified)
    public const int MED_NONACTIVE = 2; // Non-active status (expired, dc'd)

    #endregion

    #region Medication Actions

    public const int MED_NONE = 0;
    public const int MED_NEW = 1;
    public const int MED_DC = 2;
    public const int MED_HOLD = 3;
    public const int MED_RENEW = 4;
    public const int MED_REFILL = 5;

    #endregion

    #region Date/Time Validation

    public const string DT_FUTURE = "F";
    public const string DT_PAST = "P";
    public const string DT_MMDDREQ = "E";
    public const string DT_TIMEOPT = "T";
    public const string DT_TIMEREQ = "R";

    #endregion

    #region Change Context Types

    public const int CC_CLICK = 0;
    public const int CC_INIT_PATIENT = 1;
    public const int CC_NOTIFICATION = 2;
    public const int CC_REFRESH = 3;
    public const int CC_RESUME = 4;

    #endregion

    #region Notification Types

    public const int NF_LAB_RESULTS = 3;
    public const int NF_FLAGGED_ORDERS = 6;
    public const int NF_ORDER_REQUIRES_ELEC_SIGNATURE = 12;
    public const int NF_ABNORMAL_LAB_RESULTS = 14;
    public const int NF_IMAGING_RESULTS = 22;
    public const int NF_CONSULT_REQUEST_RESOLUTION = 23;
    public const int NF_ABNORMAL_IMAGING_RESULTS = 25;
    public const int NF_IMAGING_REQUEST_CANCEL_HELD = 26;
    public const int NF_NEW_SERVICE_CONSULT_REQUEST = 27;
    public const int NF_CONSULT_REQUEST_CANCEL_HOLD = 30;
    public const int NF_SITE_FLAGGED_RESULTS = 32;
    public const int NF_ORDERER_FLAGGED_RESULTS = 33;
    public const int NF_ORDER_REQUIRES_COSIGNATURE = 37;
    public const int NF_LAB_ORDER_CANCELED = 42;
    public const int NF_STAT_RESULTS = 44;
    public const int NF_DNR_EXPIRING = 45;
    public const int NF_MEDICATIONS_EXPIRING_INPT = 47;
    public const int NF_UNVERIFIED_MEDICATION_ORDER = 48;
    public const int NF_NEW_ORDER = 50;
    public const int NF_IMAGING_RESULTS_AMENDED = 53;
    public const int NF_CRITICAL_LAB_RESULTS = 57;
    public const int NF_UNVERIFIED_ORDER = 59;
    public const int NF_FLAGGED_OI_RESULTS = 60;
    public const int NF_FLAGGED_OI_ORDER = 61;
    public const int NF_DC_ORDER = 62;
    public const int NF_CONSULT_REQUEST_UPDATED = 63;
    public const int NF_FLAGGED_OI_EXP_INPT = 64;
    public const int NF_FLAGGED_OI_EXP_OUTPT = 65;
    public const int NF_CONSULT_PROC_INTERPRETATION = 66;
    public const int NF_IMAGING_REQUEST_CHANGED = 67;
    public const int NF_LAB_THRESHOLD_EXCEEDED = 68;
    public const int NF_MAMMOGRAM_RESULTS = 69;
    public const int NF_PAP_SMEAR_RESULTS = 70;
    public const int NF_ANATOMIC_PATHOLOGY_RESULTS = 71;
    public const int NF_MEDICATIONS_EXPIRING_OUTPT = 72;
    public const int NF_RX_RENEWAL_REQUEST = 73;
    public const int NF_DEA_AUTO_DC_CS_MED_ORDER = 74;
    public const int NF_DEA_CERT_REVOKED = 75;
    public const int NF_LAPSED_ORDER = 78;
    public const int NF_HIRISK_ORDER = 79;
    public const int NF_RTC_CANCEL_ORDERS = 91;
    public const int NF_DCSUMM_UNSIGNED_NOTE = 901;
    public const int NF_CONSULT_UNSIGNED_NOTE = 902;
    public const int NF_NOTES_UNSIGNED_NOTE = 903;
    public const int NF_SURGERY_UNSIGNED_NOTE = 904;

    #endregion

    #region Notify Application Events

    public const string NAE_OPEN = "BEG";
    public const string NAE_CLOSE = "END";
    public const string NAE_NEWPT = "XPT";
    public const string NAE_REPORT = "RPT";
    public const string NAE_ORDER = "ORD";

    #endregion

    #region TIU Delete Document Reasons

    public const string DR_PRIVACY = "P";
    public const string DR_ADMIN = "A";
    public const string DR_NOTREQ = "";
    public const string DR_CANCEL = "CANCEL";

    #endregion

    #region TIU Document Types

    public const int TYP_PROGRESS_NOTE = 3;
    public const int TYP_ADDENDUM = 81;
    public const int TYP_DC_SUMM = 244;

    #endregion

    #region TIU National Document Class Names

    public const string DCL_CONSULTS = "CONSULTS";
    public const string DCL_CLINPROC = "CLINICAL PROCEDURES";
    public const string DCL_SURG_OR = "SURGICAL REPORTS";
    public const string DCL_SURG_NON_OR = "PROCEDURE REPORT (NON-O.R.)";

    #endregion

    #region TIU/Notes View Contexts

    public const int NC_RECENT = 0;
    public const int NC_ALL = 1;
    public const int NC_UNSIGNED = 2;
    public const int NC_UNCOSIGNED = 3;
    public const int NC_BY_AUTHOR = 4;
    public const int NC_BY_DATE = 5;
    public const int NC_CUSTOM = 6;
    public const int NC_SEARCHTEXT = 7;

    #endregion

    #region Surgery View Contexts

    public const int SR_RECENT = 0;
    public const int SR_ALL = -1;
    public const int SR_BY_DATE = -5;
    public const int SR_CUSTOM = -6;

    #endregion

    #region Consult Context Types

    public const int CC_CONSULT_ALL = 1;
    public const int CC_BY_STATUS = 2;
    public const int CC_BY_SERVICE = 4;
    public const int CC_BY_DATE = 5;
    public const int CC_CONSULT_CUSTOM = 6;

    #endregion

    #region Package Identifiers

    public const string PKG_CONSULTS = "GMR(123,";
    public const string PKG_SURGERY = "SRF(";
    public const string PKG_PRF = "PRF";

    #endregion

    #region New Person Filters

    public const int NPF_ALL = 0;
    public const int NPF_PROVIDER = 1;
    public const int NPF_SUPPRESS = 9;

    #endregion

    #region Location Types

    public const int LOC_ALL = 0;
    public const int LOC_OUTP = 1;
    public const int LOC_INP = 2;

    #endregion

    #region File Numbers

    public const int FN_HOSPITAL_LOCATION = 44;
    public const int FN_NEW_PERSON = 200;

    #endregion

    #region Sensitive Patient Access

    public const int DGSR_FAIL = -1;
    public const int DGSR_NONE = 0;
    public const int DGSR_SHOW = 1;
    public const int DGSR_ASK = 2;
    public const int DGSR_DENY = 3;

    #endregion

    #region Word Processing & Entry Width

    public const string TX_WPTYPE = "^WP^";
    public const int MAX_ENTRY_WIDTH = 80;
    public const int MAX_PROGRESSNOTE_WIDTH = 80;
    public const int MAX_CONSULT_WIDTH = 74;

    #endregion

    #region Non-VA Medication

    public const string NONVAMEDGROUP = "Non-VA Meds";
    public const string NONVAMEDTXT = "Non-VA";

    #endregion

    #region Miscellaneous

    public const string DISCONTINUED_ORDER = "2";

    #endregion
}

#region Enumerations

/// <summary>
/// Chart tab identifiers matching CPRS tab pages.
/// </summary>
public enum ChartTab
{
    NoPage = CprsConstants.CT_NOPAGE,
    Unknown = CprsConstants.CT_UNKNOWN,
    CoverSheet = CprsConstants.CT_COVER,
    Problems = CprsConstants.CT_PROBLEMS,
    Medications = CprsConstants.CT_MEDS,
    Orders = CprsConstants.CT_ORDERS,
    HistoryPhysical = CprsConstants.CT_HP,
    Notes = CprsConstants.CT_NOTES,
    Consults = CprsConstants.CT_CONSULTS,
    DischargeSummaries = CprsConstants.CT_DCSUMM,
    Labs = CprsConstants.CT_LABS,
    Reports = CprsConstants.CT_REPORTS,
    Surgery = CprsConstants.CT_SURGERY
}

/// <summary>
/// Change item types for the pending changes tracking system.
/// </summary>
public enum ChangeItemType
{
    Document = CprsConstants.CH_DOC,
    DischargeSummary = CprsConstants.CH_SUM,
    Consult = CprsConstants.CH_CON,
    Surgery = CprsConstants.CH_SUR,
    Order = CprsConstants.CH_ORD,
    Encounter = CprsConstants.CH_PCE
}

/// <summary>
/// Signature requirement state for change items.
/// </summary>
public enum SignState
{
    NotApplicable = CprsConstants.CH_SIGN_NA,
    Yes = CprsConstants.CH_SIGN_YES,
    No = CprsConstants.CH_SIGN_NO
}

/// <summary>
/// Ordering role/privilege level for the current user.
/// </summary>
public enum OrderingRole
{
    NoKey = CprsConstants.OR_NOKEY,
    Clerk = CprsConstants.OR_CLERK,
    Nurse = CprsConstants.OR_NURSE,
    Physician = CprsConstants.OR_PHYSICIAN,
    Student = CprsConstants.OR_STUDENT,
    BadKeys = CprsConstants.OR_BADKEYS
}

/// <summary>
/// Medication status groups for filtering.
/// </summary>
public enum MedicationStatusGroup
{
    Active = CprsConstants.MED_ACTIVE,
    Pending = CprsConstants.MED_PENDING,
    NonActive = CprsConstants.MED_NONACTIVE
}

/// <summary>
/// Actions that can be performed on medication orders.
/// </summary>
public enum MedicationAction
{
    None = CprsConstants.MED_NONE,
    New = CprsConstants.MED_NEW,
    Discontinue = CprsConstants.MED_DC,
    Hold = CprsConstants.MED_HOLD,
    Renew = CprsConstants.MED_RENEW,
    Refill = CprsConstants.MED_REFILL
}

/// <summary>
/// Notes/TIU view context for filtering document lists.
/// </summary>
public enum NoteViewContext
{
    Recent = CprsConstants.NC_RECENT,
    All = CprsConstants.NC_ALL,
    Unsigned = CprsConstants.NC_UNSIGNED,
    Uncosigned = CprsConstants.NC_UNCOSIGNED,
    ByAuthor = CprsConstants.NC_BY_AUTHOR,
    ByDate = CprsConstants.NC_BY_DATE,
    Custom = CprsConstants.NC_CUSTOM,
    SearchText = CprsConstants.NC_SEARCHTEXT
}

/// <summary>
/// Location type filter for location lookups.
/// </summary>
public enum LocationType
{
    All = CprsConstants.LOC_ALL,
    Outpatient = CprsConstants.LOC_OUTP,
    Inpatient = CprsConstants.LOC_INP
}

/// <summary>
/// Sensitive patient access level returned by DG SENSITIVE RECORD ACCESS.
/// </summary>
public enum SensitiveAccessLevel
{
    Fail = CprsConstants.DGSR_FAIL,
    None = CprsConstants.DGSR_NONE,
    Show = CprsConstants.DGSR_SHOW,
    Ask = CprsConstants.DGSR_ASK,
    Deny = CprsConstants.DGSR_DENY
}

/// <summary>
/// Order dialog form type identifiers.
/// </summary>
public enum OrderDialogType
{
    Activity = CprsConstants.OD_ACTIVITY,
    Allergy = CprsConstants.OD_ALLERGY,
    Consult = CprsConstants.OD_CONSULT,
    Procedure = CprsConstants.OD_PROCEDURE,
    DietText = CprsConstants.OD_DIET_TXT,
    Diet = CprsConstants.OD_DIET,
    Lab = CprsConstants.OD_LAB,
    BloodBank = CprsConstants.OD_BB,
    MedInpatient = CprsConstants.OD_MEDINPT,
    Meds = CprsConstants.OD_MEDS,
    MedOutpatient = CprsConstants.OD_MEDOUTPT,
    MedNonVA = CprsConstants.OD_MEDNONVA,
    Nursing = CprsConstants.OD_NURSING,
    Misc = CprsConstants.OD_MISC,
    Generic = CprsConstants.OD_GENERIC,
    Imaging = CprsConstants.OD_IMAGING,
    Vitals = CprsConstants.OD_VITALS,
    ReturnToClinic = CprsConstants.OD_RTC,
    MedIV = CprsConstants.OD_MEDIV,
    TextOnly = CprsConstants.OD_TEXTONLY,
    ClinicMed = CprsConstants.OD_CLINICMED,
    ClinicInfusion = CprsConstants.OD_CLINICINF,
    AutoAcknowledge = CprsConstants.OD_AUTOACK
}

/// <summary>
/// Quick order acceptance levels.
/// </summary>
public enum QuickOrderLevel
{
    Dialog = CprsConstants.QL_DIALOG,
    Auto = CprsConstants.QL_AUTO,
    Verify = CprsConstants.QL_VERIFY,
    Reject = CprsConstants.QL_REJECT,
    Cancel = CprsConstants.QL_CANCEL
}

#endregion
