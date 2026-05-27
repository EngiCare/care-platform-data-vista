using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using CarePlatform.Data.CPRS;
//using System;
//using System.Collections.Generic;
//using System.Collections.Specialized;
//using System.Text;
//using gov.va.medora.mdo.exceptions;
//using gov.va.medora.utils;
//using gov.va.medora.mdo.dao.sql.UserValidation;
//using gov.va.medora.mdo.src.mdo;
//using gov.va.medora.mdo.conf;
//using CarePlatform.Data;
//using CarePlatform.Data.Exceptions;

namespace CarePlatform.Data.VistA
{
    static class StringExtensions
    {

        public static IEnumerable<String> SplitInParts(this String s, Int32 partLength)
        {
            if (s == null)
                throw new ArgumentNullException("s");
            if (partLength <= 0)
                throw new ArgumentException("Part length has to be positive.", "partLength");

            for (var i = 0; i < s.Length; i += partLength)
                yield return s.Substring(i, Math.Min(partLength, s.Length - i));
        }

    }


    public class VistaAccount
    {
        private readonly ILogger _logger = ECConfiguration.LoggerFactory?.CreateLogger<VistaAccount>();

        public string AccountId { get; set; }

        public VistaConnection Cxn { get; set; }
        public Dictionary<string, AbstractPermission> Permissions { get; set; }

        public bool IsAuthenticated { get; set; }
        public bool IsAuthorized {get; private set;}

        public string AuthenticationMethod {get;set;}

        /// <summary>
        /// Stores the context permission between Phase 1 (authorize) and Phase 2 (selectDivisionAndSetContext).
        /// </summary>
        private AbstractPermission _pendingPermission;



        public AbstractPermission PrimaryPermission
        {
            get
            {
                if (Permissions == null || Permissions.Count == 0)
                {
                    return null;
                }

                if (Permissions.Where(p => p.Value.IsPrimary).Count() > 0)
                {
                    return Permissions.Where(p => p.Value.IsPrimary).FirstOrDefault().Value;
                }

                return null;
            }
        }


        public VistaAccount(VistaConnection cxn) 
        {
            this.Cxn = cxn;
            this.Permissions = new Dictionary<string, AbstractPermission>();
        }

        //public override string authenticate(AbstractCredentials credentials)
        //{
        //    return authenticate(credentials, null);
        //}

        public async Task<string> authenticate(VistaCredentials credentials, DataSource validationDataSource = null)
        {
            if (Cxn == null || !Cxn.IsConnected)
                throw new ApplicationException("VistaConnection is not connected.");
            
            if (credentials == null)
                throw new UnauthorizedAccessException("Credentials must be specified.");
            

            return await login(credentials);
        }

        public async Task<string> authenticateSSO(VistaCredentials credentials, DataSource validationDataSource = null)
        {
            if (Cxn == null || !Cxn.IsConnected)
                throw new ApplicationException("VistaConnection is not connected.");
            
            if (credentials == null)
                throw new UnauthorizedAccessException("Credentials must be specified.");
            

            return await loginWithSSOToken(credentials);
        }

        internal async Task<string> login(VistaCredentials credentials)
        {
            if (String.IsNullOrEmpty(credentials.AccountName))
                throw new UnauthorizedAccessException("Missing Access Code");
            
            if (String.IsNullOrEmpty(credentials.AccountPassword))
                throw new UnauthorizedAccessException("Missing Verify Code");
            
            VistaQuery vq = new VistaQuery("XUS SIGNON SETUP");
            string rtn = await Cxn.query(vq);
            if (rtn == null)
                throw new ApplicationException("Unable to setup authentication");
            

            vq = new VistaQuery("XUS AV CODE");
                vq.addEncryptedParameter(VistaQuery.LITERAL, credentials.AccountName + ';' + credentials.AccountPassword);
            rtn = await Cxn.query(vq);


            string[] flds = rtn.Split(new string[]{ StringUtils.CRLF } , StringSplitOptions.None);
            if (flds[0] == "0")
                throw new UnauthorizedAccessException(flds[3]);
            
            AccountId = flds[0];

            // Set the connection's UID
            Cxn.Uid = AccountId;

            // Save the credentials
            credentials.LocalUid = AccountId;
            //credentials.AuthenticationSource = Cxn.DataSource;
            //credentials.AuthenticationToken = Cxn.DataSource.SiteId.Id + '_' + AccountId;

            IsAuthenticated = true;

            // Set the greeting if there is one
            if (flds.Length > 7)
            {
                return flds[7];
            }
            return "OK";
        }

        internal async Task<string> loginWithSSOToken(VistaCredentials credentials)
        {
            if (String.IsNullOrEmpty(credentials.SSOToken))
                throw new ApplicationException("Missing SSO Token");
           
           

            VistaQuery vq = new VistaQuery("XUS SIGNON SETUP");
            string rtn = await Cxn.query(vq);
            if (rtn == null)
                throw new ApplicationException("Unable to setup authentication");
            

            // 
            // 12/8/2017 - this is the old code - we're updating for SSO capability...
            //vq = new VistaQuery("XUS AV CODE");
            //vq.addEncryptedParameter(VistaQuery.LITERAL, credentials.AccountName + ';' + credentials.AccountPassword);
            //rtn = (string)Cxn.query(vq);
            // end of SSO update 12/8/2017

            vq = new VistaQuery("XUS ESSO VALIDATE");

            DictionaryHashList list = new DictionaryHashList();
            //list.Add("0", "^XTMP(\"SML\")");

            int i = 0;
            foreach( var s in credentials.SSOToken.SplitInParts(200))
                list.Add(i++.ToString(), s);

            vq.addParameter(VistaQuery.GLOBAL, list);
            rtn = await Cxn.query(vq);

            //Console.WriteLine("[Debug] " + "Text returned from SSO query: " + rtn);

            string[] flds = rtn.Split(new string[] { StringUtils.CRLF }, StringSplitOptions.None);

            if (flds[0] == "0")
            {
                throw new UnauthorizedAccessException(flds[3]);
            }
            AccountId = flds[0];

            // Set the connection's UID
            Cxn.Uid = AccountId;

            // Save the credentials
            credentials.LocalUid = AccountId;
            //credentials.AuthenticationSource = Cxn.DataSource;
            //credentials.AuthenticationToken = Cxn.DataSource.SiteId.Id + '_' + AccountId;

            IsAuthenticated = true;

            // Set the greeting if there is one
            if (flds.Length > 7)
            {
                return flds[7];
            }
            return "OK";
        }


        /// <summary>
        /// Phase 1 of the two-phase login: authenticate, fetch divisions, build User.
        /// Does NOT set the OR CPRS GUI CHART context — that happens in Phase 2
        /// (selectDivisionAndSetContext) after the user picks a division.
        ///
        /// This matches the CPRS Delphi login sequence (SelDiv.pas) where division
        /// selection occurs entirely within the XUS SIGNON context.
        /// </summary>
        public async Task<User> authorize(VistaCredentials credentials, AbstractPermission permission)
        {
            if (permission == null)
            {
                throw new ArgumentNullException("permission");
            }
            checkAuthorizeReadiness();
            checkPermissionString(permission.Name);

            // Fetch divisions while still in XUS SIGNON context.
            // XUS DIVISION GET/SET are XUS* RPCs, allowed in signon context,
            // but NOT registered to OR CPRS GUI CHART.
            List<string> divisionLines = await fetchDivisions();

            // Store the permission for Phase 2 (selectDivisionAndSetContext)
            _pendingPermission = permission;

            User User = toUser(credentials);
            User.DivisionLines = divisionLines;
            // XUS GET USER INFO is in XWBSEC's always-allowed list — safe to call now
            await setUserInfoAtLogin(User);
            return User;
        }

        /// <summary>
        /// Phase 2: set the selected division and switch to the application context.
        /// Must be called after authorize() and before any OR CPRS GUI CHART RPCs.
        /// Both XUS DIVISION SET (XUS* prefix) and XWB CREATE CONTEXT (always-allowed)
        /// work in the XUS SIGNON context.
        /// </summary>
        public async Task selectDivisionAndSetContext(string stationNumber)
        {
            if (_pendingPermission == null)
                throw new InvalidOperationException("authorize() must be called before selectDivisionAndSetContext()");

            if (!String.IsNullOrEmpty(stationNumber))
            {
                VistaQuery vq = new VistaQuery("XUS DIVISION SET");
                vq.addParameter(VistaQuery.LITERAL, stationNumber);
                await Cxn.query(vq);
            }

            await setContext(_pendingPermission);
            _pendingPermission = null;
        }

        /// <summary>
        /// Fetch the user's division list (XUS DIVISION GET).
        /// Must be called while still in the XUS SIGNON context.
        /// </summary>
        internal async Task<List<string>> fetchDivisions()
        {
            VistaQuery vq = new VistaQuery("XUS DIVISION GET");
            string response = await Cxn.query(vq);
            return response.Split(new string[] { "\r\n" }, StringSplitOptions.None).ToList();
        }

        public async Task setUserInfoAtLogin(User user)
        {
            VistaQuery vq = new VistaQuery("XUS GET USER INFO");
            string response = await Cxn.query(vq);
            toUserFromLogin(response, user);
        }

        internal void toUserFromLogin(string response, User user)
        {
            if (response == "")
            {
                return;
            }
            string[] flds = response.Split( new string[]{"\r\n"}, StringSplitOptions.None);
            user.Uid = flds[0];
            user.Name = new PersonName(flds[1]);
            string[] subflds = flds[3].Split( new string[]{"^"}, StringSplitOptions.None);
            //user.LogonSiteId = new SiteId(subflds[2], subflds[1]);
            user.Title = flds[4];
            if (flds[5] != "")
            {
                user.Service = new Service();
                user.Service.Name = flds[5];
            }
        }

        internal async Task doTheAuthorize(VistaCredentials credentials, AbstractPermission permission)
        {
            if (permission != null && !String.IsNullOrEmpty(permission.Name))
            {
                await setContext(permission);
            }

        }

        internal User toUser(VistaCredentials credentials)
        {
            User u = new User();
            u.Uid = Cxn.Uid;
            u.Name = new PersonName(credentials.SubjectName);
            u.SSN = new SocSecNum(credentials.FederatedUid);
            u.LogonSiteId = new SiteId(Cxn.SiteId);
            _logger?.LogDebug("User LogonSiteId: {SiteId}", u.LogonSiteId.Id);
            return u;
        }

        internal void checkAuthorizeReadiness()
        {
            if (Cxn == null || !Cxn.IsConnected)
            {
                throw new ApplicationException("Must have connection");
            }
            if (!this.IsAuthenticated)
            {
                throw new UnauthorizedAccessException("Account must be authenticated");
            }
        }

        internal void checkPermissionString(string permission)
        {
            if (String.IsNullOrEmpty(permission))
            {
                throw new UnauthorizedAccessException("Must have a context");
            }
        }

        public async Task setContext(AbstractPermission permission)
        {
            if (permission == null || string.IsNullOrEmpty(permission.Name))
            {
                throw new ArgumentNullException("permission");
            }


            VistaQuery request = buildSetContextRequest(permission.Name);
            string response = "";
            try
            {
                response = await Cxn.query(request);
            }
            catch (ApplicationException e)
            {
                response = e.Message;
            }
            if (response != "1")
            {
                throw getException(response);
            }
            if (!hasPermission(Cxn.Account.Permissions, permission))
            {
                Cxn.Account.Permissions.Add(permission.Name, permission);
            }
            this.IsAuthorized = this.IsAuthorized || permission.IsPrimary;
        }

        internal VistaQuery buildSetContextRequest(string context)
        {
            VistaQuery vq = new VistaQuery("XWB CREATE CONTEXT");
            if (Cxn.GetType().Name != "MockConnection")
            {
                vq.addEncryptedParameter(VistaQuery.LITERAL, context);
            }
            else
            {
                vq.addParameter(VistaQuery.LITERAL, context);
            }
            return vq;
        }

        internal Exception getException(string result)
        {
            if (result.IndexOf("The context") != -1 &&
                result.IndexOf("does not exist on server") != -1)
            {
                return new ApplicationException(result);
            }
            if (result.IndexOf("User") != -1 &&
                result.IndexOf("does not have access to option") != -1)
            {
                return new UnauthorizedAccessException(result);
            }
            if (result.StartsWith("Option locked"))
            {
                return new ApplicationException(result);
            }
            return new Exception(result);
        }


        // This is how the visitor gets the requested context - typically 
        // OR CPRS GUI CHART. The visitor comes back from VistA with CAPRI
        // context only.
        internal void addContextInVista(string duz, AbstractPermission requestedContext)
        {
            
            if (hasPermission(this.Cxn.Account.Permissions, requestedContext))
            {
                _logger?.LogDebug("Checking for permission but Cxn.Account.Permissions is false");
                return;
            }

            try
            {
                setContext(requestedContext);
            }
            catch (Exception)
            {
                throw;
            }
        }

        internal bool hasPermission(Dictionary<string, AbstractPermission> permissions, AbstractPermission permission)
        {
            bool found = false;

            if (permissions.ContainsKey(permission.Name))
            {
                return true;
            }

            foreach (AbstractPermission perm in permissions.Values)
            {
                if (String.Equals(perm.Name, permission.Name, StringComparison.CurrentCultureIgnoreCase))
                {
                    found = true;
                    break;
                }
            }

            return found;
        }


        public async Task<User> authenticateAndAuthorize(VistaCredentials credentials, AbstractPermission permission, DataSource validationDataSource = null)
        {
            string msg = await authenticate(credentials, validationDataSource);
            User u = await authorize(credentials, permission);
            u.Greeting = msg;
            return u;
        }

        public async Task<User> authenticateAndAuthorizeSSO(VistaCredentials credentials, AbstractPermission permission, DataSource validationDataSource = null)
        {
            string msg = await authenticateSSO(credentials, validationDataSource);
            User u = await authorize(credentials, permission);
            u.Greeting = msg;
            return u;
        }

        //public void addDdrContext(AbstractCredentials credentials, AbstractConnection currentCxn)
        //{
        //    if (credentials.AuthenticationMethod == VistaConstants.LOGIN_CREDENTIALS)
        //    {
        //        throw new UnauthorizedAccessException("Wrong credential type");
        //    }
        //    VistaConnection cxn = new VistaConnection(currentCxn.DataSource);
        //    cxn.connect();
        //    cxn.Account.authenticate(credentials);
        //    cxn.disconnect();
        //}

        //public string getAuthenticationTokenFromVista()
        //{
        //    VistaQuery vq = new VistaQuery("XUS SET VISITOR");
        //    return (string)Cxn.query(vq);
        //}

        //internal void setVisitorContext(AbstractPermission requestedContext, string DUZ)
        //{
        //    try
        //    {
        //        setContext(requestedContext);
        //        return;
        //    }
        //    catch (UnauthorizedAccessException uae)
        //    {
        //        addContextInVista(DUZ, requestedContext);
        //        setContext(requestedContext);
        //    }
        //    catch (Exception e)
        //    {
        //        throw;
        //    }
        //}


    }


}
