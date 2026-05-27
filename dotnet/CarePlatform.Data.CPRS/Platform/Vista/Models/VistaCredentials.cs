using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarePlatform.Data.VistA
{
    public class VistaCredentials
    {
        public VistaCredentials() { }

        public string AccountName{get;set;}

        public string AccountPassword{get;set;}

        public string SSOToken { get; set; }

        
        public DataSource AuthenticationSource{get;set;}

        public string FederatedUid{get;set;}
        public string LocalUid{get;set;}

        public string SubjectName{get;set;}
        public string SubjectPhone{get;set;}
        public string AuthenticationToken{get;set;}

        public string SecurityPhrase {get;set;}


    }
}
