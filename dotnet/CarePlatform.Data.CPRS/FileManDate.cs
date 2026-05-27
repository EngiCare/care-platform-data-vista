// Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace CarePlatform.Data.CPRS
{
    /// <summary>
    /// FileMan date conversion helpers. FileMan dates are stored as a numeric
    /// "YYYMMDD[.HHMMSS]" where YYY = (calendar year - 1700). For example
    /// 2026-04-21 14:30:00 = 3260421.143.
    ///
    /// Mirrors the conversion the CPRS desktop performs in <c>StrToFMDateTime</c>
    /// /<c>FMDateTimeToDateTime</c> (XLib FMDateTime.pas). Used by reports,
    /// imaging, procedures, consults, and any other endpoint that forwards an
    /// alpha/omega range to a VistA RPC.
    /// </summary>
    public static class FileManDate
    {
        // Matches a value that already looks like a FileMan date (YYYMMDD[.frac]).
        // 7 leading digits is canonical; allow 5–7 to handle pre-2000 truncation
        // (RPC layer is forgiving but this lets callers pass through values like
        // "3260421" without re-parsing.)
        private static readonly Regex _fmShape = new Regex(
            @"^\d{5,7}(\.\d+)?$", RegexOptions.Compiled);

        // T-relative shorthand used throughout CPRS forms: "T", "T+5", "T-30",
        // optionally with a time suffix ("T@1230", "T-1@0800"). Time portion
        // is normalized to HHMMSS.
        private static readonly Regex _tRelative = new Regex(
            @"^T(?<sign>[+\-])?(?<days>\d+)?(@(?<time>\d{1,6}))?$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>
        /// Convert a user-supplied date string to a FileMan date string.
        /// Accepts: empty (→ ""), already-FM, "T" / "T±N" relative, ISO
        /// "yyyy-MM-dd" / "yyyy-MM-ddTHH:mm[:ss]", or any other value
        /// <see cref="DateTime.TryParse(string, out DateTime)"/> can resolve.
        /// Returns "" when the input cannot be interpreted (caller's RPC will
        /// then receive an empty parameter, matching CPRS behavior).
        /// </summary>
        public static string ToFM(string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return "";
            var s = input.Trim();

            if (_fmShape.IsMatch(s)) return s;          // pass-through

            var t = _tRelative.Match(s);
            if (t.Success)
            {
                int sign = t.Groups["sign"].Value == "-" ? -1 : 1;
                int days = int.TryParse(t.Groups["days"].Value, out var d) ? d : 0;
                var dt = DateTime.Today.AddDays(sign * days);
                if (t.Groups["time"].Success && int.TryParse(t.Groups["time"].Value, out var hms))
                {
                    var hh = (hms / 10000) % 24;
                    var mm = (hms / 100) % 100;
                    var ss = hms % 100;
                    dt = dt.Date.AddHours(hh).AddMinutes(mm).AddSeconds(ss);
                }
                return Format(dt);
            }

            if (DateTime.TryParse(s, CultureInfo.InvariantCulture,
                                  DateTimeStyles.AssumeLocal, out var parsed))
                return Format(parsed);

            // Last-resort: ISO with 'T' separator that .NET sometimes refuses
            // when the timezone is missing (rare, but seen with "yyyy-MM-ddTHH:mm").
            if (DateTime.TryParse(s, out var fallback)) return Format(fallback);

            return "";
        }

        /// <summary>Format a CLR DateTime as a FileMan date string.</summary>
        public static string Format(DateTime dt)
        {
            var yyy = dt.Year - 1700;
            var datePart = $"{yyy:D3}{dt.Month:D2}{dt.Day:D2}";
            if (dt.TimeOfDay == TimeSpan.Zero) return datePart;
            var timePart = $"{dt.Hour:D2}{dt.Minute:D2}{dt.Second:D2}".TrimEnd('0');
            if (timePart.Length == 0) return datePart;
            return datePart + "." + timePart;
        }

        /// <summary>
        /// Convenience wrapper that converts both ends of a date range and
        /// swaps them when alpha &gt; omega (mirrors the swap in
        /// rReports.pas <c>LoadReportText</c> line 314).
        /// </summary>
        public static (string alpha, string omega) ToFMRange(string? alpha, string? omega)
        {
            var a = ToFM(alpha);
            var o = ToFM(omega);
            if (!string.IsNullOrEmpty(a) && !string.IsNullOrEmpty(o)
                && double.TryParse(a, NumberStyles.Float, CultureInfo.InvariantCulture, out var ad)
                && double.TryParse(o, NumberStyles.Float, CultureInfo.InvariantCulture, out var od)
                && ad > od)
            {
                return (o, a);
            }
            return (a, o);
        }
    }
}
