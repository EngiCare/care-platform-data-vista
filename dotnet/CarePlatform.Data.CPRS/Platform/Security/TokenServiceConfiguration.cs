// Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
using System;
using Microsoft.Extensions.Configuration;

namespace CarePlatform.Data.CPRS.Security
{
    public static class TokenServiceConfiguration
    {
        // Certificate settings - uses either cert store or file-based certificate
        public static string SigningCertificateName = "";
        public static string SigningCertificateFileName = "";
        public static string SigningCertificatePassword = "";
        public static string IssuerName = "";
        public static string STSBaseAddress = "";
        public static string STSIssueAddress = "";
        public static string Realm = "";

        // Constants for federation metadata (kept for compatibility)
        public const string FederationMetadataAddress = "FederationMetadata/2007-06/FederationMetadata.xml";
        public const string WSFedStsIssue = "Issue/";

        public static void LoadConfiguration(IConfiguration configuration)
        {
            TokenServiceConfiguration.SigningCertificateName = configuration["TokenServiceConfig:CertificateName"] ?? "";
            TokenServiceConfiguration.SigningCertificateFileName = configuration["TokenServiceConfig:CertificateFileName"] ?? "";
            TokenServiceConfiguration.SigningCertificatePassword = configuration["TokenServiceConfig:CertificatePassword"] ?? "";
            TokenServiceConfiguration.IssuerName = configuration["TokenServiceConfig:IssuerName"] ?? "";
            TokenServiceConfiguration.STSBaseAddress = configuration["TokenServiceConfig:STSBaseAddress"] ?? "";
            TokenServiceConfiguration.STSIssueAddress = STSBaseAddress + WSFedStsIssue;
            TokenServiceConfiguration.Realm = configuration["TokenServiceConfig:Audience"] ?? "";
        }
    }
}
