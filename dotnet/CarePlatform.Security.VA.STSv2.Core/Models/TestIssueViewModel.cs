using System.Collections.Generic;

namespace CarePlatform.Security.VA.STSv2.Models
{
    public class TestIssueViewModel
    {
        public string BootstrapJwt { get; set; }
        public string UserName { get; set; }
        public IEnumerable<KeyValuePair<string, string>> RequestHeaders { get; set; }
        public string RequestHeaderString { get; set; }
        public IEnumerable<KeyValuePair<string, string>> ResponseHeaders { get; set; }
        public string ResponseHeaderString { get; set; }
        public string CertificateName { get; set; }
        public string Endpoint { get; set; }
        public string RequestString { get; set; }
        public string ResponseString { get; set; }
        public string Token { get; set; }

        // OBO exchange diagnostics
        public bool OboSuccess { get; set; }
        public string OboAccessToken { get; set; }
        public string OboRequestedScope { get; set; }
        public string OboTokenEndpoint { get; set; }
        public int OboExpiresIn { get; set; }
        public string OboError { get; set; }
    }
}
