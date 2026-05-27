using System.Collections.Generic;

namespace CarePlatform.Security.VA.STSv2.Models
{
    public class TestHeaderViewModel
    {
        public string BootstrapJwt { get; set; }
        public string UserName { get; set; }
        public IEnumerable<KeyValuePair<string, string>> Claims { get; set; }
        public string ClaimsString { get; set; }
        public IEnumerable<KeyValuePair<string, string>> Headers { get; set; }
        public string HeaderString { get; set; }
    }
}
