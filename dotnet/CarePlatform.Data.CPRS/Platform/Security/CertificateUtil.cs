// Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using CarePlatform.Data.CPRS;
using Microsoft.Extensions.Logging;

namespace CarePlatform.Data.CPRS.Security
{
    public class CertificateUtil
    {
        /// <summary>
        /// Load a certificate from a file path. Uses AppContext.BaseDirectory instead of HttpRuntime.AppDomainAppPath.
        /// </summary>
        public static X509Certificate2 GetCertificate(string certificateName, string certificatePassword, string basePath = "")
        {
            string signingCertificatePath = Path.Combine(
                !string.IsNullOrEmpty(basePath) ? basePath : AppContext.BaseDirectory,
                certificateName);
            return X509CertificateLoader.LoadPkcs12FromFile(signingCertificatePath, certificatePassword,
                X509KeyStorageFlags.PersistKeySet);
        }

        /// <summary>
        /// Look up a certificate from the certificate store by subject name, with optional intended purpose and issuer filtering.
        /// On Linux, the LocalMachine cert store with StoreName.My is not supported;
        /// a self-signed certificate is returned as a fallback for Docker/CI environments.
        /// </summary>
        // Cache the Linux self-signed certificate so every call returns the SAME key pair.
        // Without this, JWT signing and validation use different keys → IDX10503.
        private static X509Certificate2? _cachedLinuxCert;
        private static readonly object _linuxCertLock = new();

        public static X509Certificate2 GetCertificate(StoreName name, StoreLocation location, string subjectName,
            string intendedPurpose = "", string issuerName = "")
        {
            var logger = ECConfiguration.LoggerFactory?.CreateLogger(typeof(CertificateUtil));

            if (!OperatingSystem.IsWindows())
            {
                if (_cachedLinuxCert == null)
                {
                    lock (_linuxCertLock)
                    {
                        if (_cachedLinuxCert == null)
                        {
                            logger?.LogInformation("Running on Linux — creating cached self-signed certificate for {SubjectName}", subjectName);
                            using var rsa = RSA.Create(2048);
                            var req = new CertificateRequest(subjectName, rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                            _cachedLinuxCert = req.CreateSelfSigned(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddYears(1));
                        }
                    }
                }
                return _cachedLinuxCert;
            }

            X509Store store = new X509Store(name, location);
            X509Certificate2Collection certificates = null;
            store.Open(OpenFlags.ReadOnly);

            logger?.LogDebug("Looking up certificate for issuer={Issuer} subject={Subject} purpose={Purpose}", issuerName, subjectName, intendedPurpose);

            try
            {
                X509Certificate2 result = null;
                certificates = store.Certificates;

                for (int i = 0; i < certificates.Count; i++)
                {
                    X509Certificate2 cert = certificates[i];

                    logger?.LogTrace("Issuer {Issuer} - Subject Name:{Subject}", cert.IssuerName.Name, cert.SubjectName.Name);
                    logger?.LogTrace("Comparing to: {Subject} - {Match}", cert.SubjectName.Name, cert.SubjectName.Name == subjectName);

                    if (cert.SubjectName.Name.ToString() == subjectName &&
                        (string.IsNullOrEmpty(issuerName) || cert.IssuerName.Name.ToString() == issuerName))
                    {
                        if (!string.IsNullOrEmpty(intendedPurpose))
                        {
                            foreach (var ext in cert.Extensions)
                            {
                                var eku = ext as X509EnhancedKeyUsageExtension;
                                if (eku != null)
                                {
                                    foreach (var oid in eku.EnhancedKeyUsages)
                                    {
                                        logger?.LogTrace("Intended purpose: {Purpose}", oid.FriendlyName);

                                        if (oid.FriendlyName == intendedPurpose)
                                        {
                                            if (result != null)
                                            {
                                                var e = new ApplicationException(string.Format("There are multiple certificate for subject Name {0} with intended purpose {1}", subjectName, intendedPurpose));
                                                logger?.LogError(e, "Multiple certificates found for subject {Subject} purpose {Purpose}", subjectName, intendedPurpose);
                                                throw e;
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
                                var e = new ApplicationException(string.Format("There are multiple certificate for subject Name {0} - Issuer Name {1}", subjectName, issuerName));
                                logger?.LogError(e, "Multiple certificates found for subject {Subject} issuer {Issuer}", subjectName, issuerName);
                                throw e;
                            }
                            result = new X509Certificate2(cert);
                        }
                    }
                }

                if (result == null)
                {
                    var e = new ApplicationException(string.Format("No certificate was found for issuer Name {0} subject Name {1} and intended purpose {2}", issuerName, subjectName, intendedPurpose));
                    logger?.LogError(e, "No certificate found for issuer {Issuer} subject {Subject} purpose {Purpose}", issuerName, subjectName, intendedPurpose);
                    throw e;
                }

                logger?.LogDebug("Cert Chosen: Issuer {Issuer} - Subject Name:{Subject}", result.IssuerName.Name, result.SubjectName.Name);
                return result;
            }
            finally
            {
                if (certificates != null)
                {
                    for (int i = 0; i < certificates.Count; i++)
                    {
                        X509Certificate2 cert = certificates[i];
                        cert.Reset();
                    }
                }
                store.Close();
            }
        }

        /// <summary>
        /// Get all certificates matching a subject name from the store.
        /// </summary>
        public static List<X509Certificate2> GetCertificates(StoreName name, StoreLocation location, string subjectName)
        {
            X509Store store = new X509Store(name, location);
            store.Open(OpenFlags.ReadOnly);

            try
            {
                var results = new List<X509Certificate2>();
                var certificates = store.Certificates;

                for (int i = 0; i < certificates.Count; i++)
                {
                    X509Certificate2 cert = certificates[i];
                    if (cert.SubjectName.Name != null &&
                        cert.SubjectName.Name.Equals(subjectName, StringComparison.OrdinalIgnoreCase))
                    {
                        results.Add(new X509Certificate2(cert));
                    }
                }
                return results;
            }
            finally
            {
                store.Close();
            }
        }
    }
}
