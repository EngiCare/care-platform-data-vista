// Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
// Inlined from CarePlatform.Data NuGet package.
// These minimal types replace the external dependency while keeping the same
// namespace (CarePlatform.Data) so existing code resolves them automatically.

using System;
using System.Collections.Generic;

namespace CarePlatform.Data
{
    /// <summary>
    /// Represents a permission (menu option, security key, etc.) that can be
    /// assigned to a VistA user session.
    /// </summary>
    public abstract class AbstractPermission
    {
        public string PermissionId { get; set; }
        public string Name { get; set; }
        public string RecordNumber { get; set; }
        public bool IsPrimary { get; set; }

        public abstract PermissionType Type { get; }

        protected AbstractPermission() { }

        protected AbstractPermission(string name)
        {
            Name = name;
        }

        protected AbstractPermission(string id, string name)
        {
            PermissionId = id;
            Name = name;
        }

        protected AbstractPermission(string id, string name, string recordNumber)
        {
            PermissionId = id;
            Name = name;
            RecordNumber = recordNumber;
        }
    }

    /// <summary>
    /// The kind of permission.
    /// </summary>
    public enum PermissionType
    {
        MenuOption,
        SecurityKey,
        DelegatedOption,
        Other
    }

    /// <summary>
    /// Describes a data source (site connection endpoint).
    /// Only the subset of properties actually referenced by local code is kept.
    /// </summary>
    public class DataSource
    {
        public string Provider { get; set; }
        public int Port { get; set; }
        public string Protocol { get; set; }
        public SiteId SiteId { get; set; }
    }

    /// <summary>
    /// Minimal user class that holds the fields populated during VistA login.
    /// </summary>
    public class User
    {
        public string Uid { get; set; }
        public PersonName Name { get; set; }
        public SocSecNum SSN { get; set; }
        public SiteId LogonSiteId { get; set; }
        public string Title { get; set; }
        public Service Service { get; set; }
        public string Greeting { get; set; }

        /// <summary>
        /// Cached raw response lines from XUS DIVISION GET, fetched during login
        /// while still in XUS SIGNON context (before OR CPRS GUI CHART context switch).
        /// </summary>
        public List<string> DivisionLines { get; set; }
    }

    /// <summary>
    /// A VistA person name parsed from "LAST,FIRST MI" format.
    /// </summary>
    public class PersonName
    {
        public string Lastname { get; set; }
        public string Firstname { get; set; }
        public string FullName { get; set; }

        public PersonName() { }

        public PersonName(string fullName)
        {
            FullName = fullName ?? "";
            if (string.IsNullOrEmpty(fullName)) return;

            var parts = fullName.Split(',');
            Lastname = parts[0].Trim();
            Firstname = parts.Length > 1 ? parts[1].Trim().Split(' ')[0] : "";
        }

        public override string ToString() => FullName ?? $"{Lastname},{Firstname}";
    }

    /// <summary>
    /// Social Security Number wrapper.
    /// </summary>
    public class SocSecNum
    {
        public string Value { get; set; }

        public SocSecNum() { }
        public SocSecNum(string value) { Value = value; }

        public override string ToString() => Value ?? "";
    }

    /// <summary>
    /// Site identifier.
    /// </summary>
    public class SiteId
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public SiteId() { }
        public SiteId(string id) { Id = id; }
        public SiteId(string id, string name) { Id = id; Name = name; }

        public override string ToString() => Id ?? "";
    }

    /// <summary>
    /// A VistA service (e.g. division or department).
    /// </summary>
    public class Service
    {
        public string Name { get; set; }
        public string Id { get; set; }

        public override string ToString() => Name ?? "";
    }
}
