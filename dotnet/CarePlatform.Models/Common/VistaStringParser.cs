namespace CarePlatform.Models.Common;

/// <summary>
/// Utility methods for parsing VistA caret-delimited (^) and pipe-delimited (|) strings.
/// Replicates the Delphi Piece/Pieces pattern from CPRS.
/// </summary>
public static class VistaStringParser
{
    /// <summary>
    /// Extract a single piece from a delimited string (1-based index, matching VistA Piece() function).
    /// </summary>
    public static string Piece(string source, char delimiter, int pieceNum)
    {
        if (string.IsNullOrEmpty(source) || pieceNum < 1)
            return string.Empty;

        var parts = source.Split(delimiter);
        return pieceNum <= parts.Length ? parts[pieceNum - 1] : string.Empty;
    }

    /// <summary>
    /// Extract a range of pieces from a delimited string (1-based, inclusive).
    /// </summary>
    public static string Pieces(string source, char delimiter, int first, int last)
    {
        if (string.IsNullOrEmpty(source) || first < 1)
            return string.Empty;

        var parts = source.Split(delimiter);
        var start = Math.Min(first - 1, parts.Length);
        var end = Math.Min(last, parts.Length);
        if (start >= end)
            return string.Empty;

        return string.Join(delimiter.ToString(), parts[start..end]);
    }

    /// <summary>
    /// Extract all pieces into an array.
    /// </summary>
    public static string[] Split(string source, char delimiter = '^')
    {
        return string.IsNullOrEmpty(source) ? [] : source.Split(delimiter);
    }

    /// <summary>
    /// Parse a VistA FM date/time string to DateTime.
    /// FM format: YYYMMDD.HHMMSS where YYY = year - 1700.
    /// </summary>
    public static DateTime? ParseFmDateTime(string? fmDate)
    {
        if (string.IsNullOrWhiteSpace(fmDate))
            return null;

        try
        {
            // Remove any trailing text after space
            var clean = fmDate.Trim().Split(' ')[0];
            var dotParts = clean.Split('.');
            var datePart = dotParts[0];

            if (datePart.Length < 7)
                return null;

            var year = 1700 + int.Parse(datePart[..3]);
            var month = int.Parse(datePart[3..5]);
            var day = int.Parse(datePart[5..7]);

            var hour = 0;
            var minute = 0;
            var second = 0;

            if (dotParts.Length > 1 && dotParts[1].Length >= 2)
            {
                var timePart = dotParts[1].PadRight(6, '0');
                hour = int.Parse(timePart[..2]);
                minute = int.Parse(timePart[2..4]);
                second = int.Parse(timePart[4..6]);
            }

            return new DateTime(year, month, day, hour, minute, second);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Convert DateTime to VistA FM date/time format.
    /// </summary>
    public static string ToFmDateTime(DateTime dt)
    {
        var year = dt.Year - 1700;
        var date = $"{year:D3}{dt.Month:D2}{dt.Day:D2}";
        if (dt is { Hour: 0, Minute: 0, Second: 0 })
            return date;
        return $"{date}.{dt.Hour:D2}{dt.Minute:D2}{dt.Second:D2}";
    }

    /// <summary>
    /// Parse a boolean-like VistA value ('1', 'Y', 'YES', 'TRUE' → true).
    /// </summary>
    public static bool ParseBool(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;
        var v = value.Trim().ToUpperInvariant();
        return v is "1" or "Y" or "YES" or "TRUE";
    }

    /// <summary>
    /// Safely parse an integer from a VistA string piece.
    /// </summary>
    public static int ParseInt(string? value, int defaultValue = 0)
    {
        return int.TryParse(value?.Trim(), out var result) ? result : defaultValue;
    }

    /// <summary>
    /// Safely parse a long from a VistA string piece.
    /// </summary>
    public static long ParseLong(string? value, long defaultValue = 0)
    {
        return long.TryParse(value?.Trim(), out var result) ? result : defaultValue;
    }

    /// <summary>
    /// Safely parse a double/float from a VistA string piece.
    /// </summary>
    public static double ParseDouble(string? value, double defaultValue = 0.0)
    {
        return double.TryParse(value?.Trim(), out var result) ? result : defaultValue;
    }
}
