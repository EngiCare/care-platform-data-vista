// Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
namespace CarePlatform.Models.Notifications;

/// <summary>
/// VistA alert / notification record from ORWORB FASTUSER or XUS GET ALERTS.
/// Maps to: rMisc.pas — LoadNotifications.
/// </summary>
public class Notification
{
    public string AlertId { get; set; } = "";       // XQAID
    public string PatientDfn { get; set; } = "";
    public string PatientName { get; set; } = "";
    public string Message { get; set; } = "";
    public string InfoLink { get; set; } = "";      // route to navigate on follow-up
    public string Urgency { get; set; } = "";       // H=High, M=Moderate, L=Low
    public DateTime? AlertDate { get; set; }
    public bool IsNew { get; set; }
    public string ForwardedBy { get; set; } = "";
    public string Comments { get; set; } = "";
}

/// <summary>
/// Response from processing/following up on an alert.
/// </summary>
public class NotificationActionResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = "";
    public string NavigateTo { get; set; } = "";
}
