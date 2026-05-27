// Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
// Inlined from CarePlatform.Data.VA NuGet package.
// DictionaryHashList is an ordered list of key-value pairs used to build
// VistA RPC parameters.  It lives in CarePlatform.Data.VistA so all existing
// controller code resolves it without any using changes.

using System;
using System.Collections;
using System.Collections.Generic;

namespace CarePlatform.Data.VistA
{
    /// <summary>
    /// An ordered dictionary that preserves insertion order and supports
    /// index-based access via <see cref="DictionaryEntry"/>.
    /// </summary>
    public class DictionaryHashList
    {
        private readonly List<DictionaryEntry> _entries = new();

        public int Count => _entries.Count;

        public void Add(string key, string value)
        {
            _entries.Add(new DictionaryEntry(key, value));
        }

        /// <summary>
        /// Access an entry by its ordinal position.
        /// </summary>
        public DictionaryEntry this[int index] => _entries[index];

        /// <summary>
        /// Try to find a value by key.  Returns null when not found.
        /// </summary>
        public string this[string key]
        {
            get
            {
                for (int i = 0; i < _entries.Count; i++)
                {
                    if (string.Equals((string)_entries[i].Key, key, StringComparison.Ordinal))
                        return (string)_entries[i].Value;
                }
                return null;
            }
        }

        public void Clear() => _entries.Clear();
    }

    /// <summary>
    /// Converts VistA FileMan timestamps (e.g. "3260310.143500") to standard
    /// date-time strings.  FileMan dates are days since 12/31/1700 with a
    /// YYYMMDD.HHMMSS format where YYY = year - 1700.
    /// </summary>
    public static class VistaTimestamp
    {
        public static string toUtcString(string vistaTimestamp)
        {
            if (string.IsNullOrWhiteSpace(vistaTimestamp))
                return "";

            try
            {
                // Split on the decimal point
                string datePart = vistaTimestamp;
                string timePart = "000000";

                int dot = vistaTimestamp.IndexOf('.');
                if (dot >= 0)
                {
                    datePart = vistaTimestamp.Substring(0, dot);
                    timePart = vistaTimestamp.Substring(dot + 1).PadRight(6, '0');
                }

                // datePart is YYYMMDD where YYY = year - 1700
                int year = int.Parse(datePart.Substring(0, datePart.Length - 4)) + 1700;
                int month = int.Parse(datePart.Substring(datePart.Length - 4, 2));
                int day = int.Parse(datePart.Substring(datePart.Length - 2, 2));

                int hour = int.Parse(timePart.Substring(0, 2));
                int minute = int.Parse(timePart.Substring(2, 2));
                int second = int.Parse(timePart.Substring(4, 2));

                var dt = new DateTime(year, month, day, hour, minute, second, DateTimeKind.Utc);
                return dt.ToString("yyyy-MM-ddTHH:mm:ssZ");
            }
            catch
            {
                return vistaTimestamp; // Return raw value on parse failure
            }
        }
    }
}
