// Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
namespace CarePlatform.Models.Session;

/// <summary>
/// Authentication/connection request model.
/// </summary>
public class LoginRequest
{
    public string SiteId { get; set; } = "";
    public string AccessCode { get; set; } = "";
    public string VerifyCode { get; set; } = "";
    public string? SSOToken { get; set; }
    public string? HostName { get; set; }
    public int? Port { get; set; }
}

/// <summary>
/// Authentication response containing the JWT and session info.
/// </summary>
public class LoginResponse
{
    public bool Success { get; set; }
    public string? Token { get; set; }
    public string? ErrorMessage { get; set; }
    public string? UserName { get; set; }
    public long Duz { get; set; }
    public string? SiteId { get; set; }
    public string? StationNumber { get; set; }
}

/// <summary>
/// A division (facility) the user is authorized for.
/// Parsed from XUS DIVISION GET response lines: IEN^Name^StationNumber^IsDefault
/// </summary>
public class UserDivision
{
    public string Ien { get; set; } = "";
    public string Name { get; set; } = "";
    public string StationNumber { get; set; } = "";
    public bool IsDefault { get; set; }

    /// <summary>
    /// Parse a single XUS DIVISION GET result line (after the count line).
    /// Format: IEN^Name^StationNumber^IsDefault(1/0)
    /// </summary>
    public static UserDivision Parse(string line)
    {
        var pieces = line.Split('^');
        return new UserDivision
        {
            Ien = pieces.Length > 0 ? pieces[0] : "",
            Name = pieces.Length > 1 ? pieces[1] : "",
            StationNumber = pieces.Length > 2 ? pieces[2] : "",
            IsDefault = pieces.Length > 3 && pieces[3] == "1"
        };
    }

    /// <summary>
    /// Parse the full XUS DIVISION GET response (first line is count, remaining are divisions).
    /// </summary>
    public static List<UserDivision> ParseList(List<string> lines)
    {
        if (lines.Count <= 1) return new List<UserDivision>();
        return lines.Skip(1).Select(Parse).ToList();
    }
}

/// <summary>
/// Session status from api/session/timeremaining.
/// </summary>
public class SessionStatus
{
    public int SecondsRemaining { get; set; }
    public bool IsActive => SecondsRemaining > 0;
    public DateTime? CurrentTime { get; set; }
}

/// <summary>
/// A site available for connection, from the VhaSites XML configuration.
/// </summary>
public class SiteEntry
{
    public string SiteId { get; set; } = "";
    public string Name { get; set; } = "";
    public string VisnName { get; set; } = "";
}

/// <summary>
/// A VistA site parsed from an SSO/SSOi token's vistaid claims.
/// Format in token: siteId-duz (e.g., "500-12345").
/// </summary>
public class SsoSiteOption
{
    public string SiteId { get; set; } = "";
    public string Duz { get; set; } = "";
    public string Name { get; set; } = "";
}
