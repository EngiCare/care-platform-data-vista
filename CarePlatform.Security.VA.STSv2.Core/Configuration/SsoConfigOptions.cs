namespace CarePlatform.Security.VA.STSv2.Configuration
{
    /// <summary>
    /// Settings driving the SOAP STS request and the client-cert lookup.
    /// Mirrors the legacy <c>SSOConfig:*</c> appSettings from Web.config.
    /// </summary>
    public class SsoConfigOptions
    {
        public string CertificateName { get; set; }
        public string IssuerName { get; set; }
        public string SsoEndpoint { get; set; }
        public string StsIssuerAddress { get; set; }
        public string AppId { get; set; }
        public string AppUrl { get; set; }
    }
}
