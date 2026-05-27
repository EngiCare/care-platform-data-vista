// Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
using System;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Logging;

namespace CarePlatform.Security.VA.STSv2.Certificates
{
    /// <summary>
    /// Looks up an X509 certificate from a Windows certificate store. Ported from
    /// CarePlatform.Security.Certificates.CertificateUtil so this app does not pull
    /// the .NET Framework-only library.
    /// </summary>
    public class CertificateUtil
    {
        private readonly ILogger<CertificateUtil> _logger;

        public CertificateUtil(ILogger<CertificateUtil> logger)
        {
            _logger = logger;
        }

        public X509Certificate2 GetCertificate(
            StoreName name,
            StoreLocation location,
            string subjectName,
            string intendedPurpose = "",
            string issuerName = "")
        {
            using var store = new X509Store(name, location);
            store.Open(OpenFlags.ReadOnly);

            _logger.LogDebug(
                "Looking up certificate for issuer {Issuer} subject {Subject} intended purpose {Purpose}",
                issuerName, subjectName, intendedPurpose);

            X509Certificate2 result = null;

            foreach (var cert in store.Certificates)
            {
                _logger.LogDebug("Issuer {CertIssuer} - Subject {CertSubject}",
                    cert.IssuerName.Name, cert.SubjectName.Name);

                if (cert.SubjectName.Name != subjectName)
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(issuerName) && cert.IssuerName.Name != issuerName)
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(intendedPurpose))
                {
                    foreach (var ext in cert.Extensions)
                    {
                        if (ext is X509EnhancedKeyUsageExtension eku)
                        {
                            foreach (var oid in eku.EnhancedKeyUsages)
                            {
                                _logger.LogDebug("intended purpose: {Purpose}", oid.FriendlyName);

                                if (oid.FriendlyName == intendedPurpose)
                                {
                                    if (result != null)
                                    {
                                        throw new InvalidOperationException(
                                            $"There are multiple certificates for subject Name {subjectName} with intended purpose {intendedPurpose}");
                                    }

                                    result = new X509Certificate2(cert);
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (result != null)
                    {
                        throw new InvalidOperationException(
                            $"There are multiple certificates for subject Name {subjectName} - Issuer Name {issuerName}");
                    }

                    result = new X509Certificate2(cert);
                }
            }

            if (result == null)
            {
                throw new InvalidOperationException(
                    $"No certificate was found for issuer Name {issuerName} subject Name {subjectName} and intended purpose {intendedPurpose}");
            }

            _logger.LogDebug("Cert chosen: Issuer {Issuer} - Subject {Subject}",
                result.IssuerName.Name, result.SubjectName.Name);

            return result;
        }
    }
}
