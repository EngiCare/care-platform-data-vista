namespace CarePlatform.Security.VA.STSv2.Configuration
{
    /// <summary>
    /// STS scope used for the On-Behalf-Of token exchange (e.g. <c>api://sqa.sts.va.gov/token</c>).
    /// Bound from the <c>Sts</c> section of configuration.
    /// </summary>
    public class StsOptions
    {
        public string Scope { get; set; }
    }
}
