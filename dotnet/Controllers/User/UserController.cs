using CarePlatform.Data.VistA;
using CarePlatform.Models.User;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarePlatform.Data.CPRS
{
    /// <summary>
    /// User information, security keys, permissions, and person/location lookups.
    /// Migrated from cprs/rCore.pas — ORWU * RPCs.
    /// </summary>
    public class UserController : BaseController
    {
        public UserController() : base() { }

        #region User Info & Permissions

        /// <summary>
        /// Get comprehensive user information record.
        /// RPC: ORWU USERINFO
        /// Returns: DUZ^NAME^USRCLS^CANSIGN^ISPROVIDER^ORDERROLE^NOORDER^DTIME^CNTDN^VERORD^
        ///          NOTIFYAPPS^MSGHANG^DOMAIN^SERVICE^AUTOSAVE^INITTAB^LASTTAB^WEBACCESS^
        ///          ALLOWHOLD^ISRPL^RPLLIST^CORTABS^RPTTAB^STATION#^GECStatus^Production^
        ///          EnableActOneStep^JobNumber
        /// </summary>
        [HttpGet, Route("api/user/info")]
        public async Task<ActionResult<UserInfo>> Info()
        {
            var vq = new VistaQuery("ORWU USERINFO");
            var result = await this.Session.sQuery(vq);
            return UserInfo.Parse(result);
        }

        /// <summary>
        /// Get a user parameter value.
        /// RPC: ORWU PARAM
        /// </summary>
        [HttpGet, Route("api/user/param")]
        public async Task<ActionResult<string>> Param([FromQuery] string paramName)
        {
            var vq = new VistaQuery("ORWU PARAM");
            vq.addParameter(VistaQuery.LITERAL, paramName);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Get system/user parameters as JSON.
        /// RPC: ORWU SYSPARAM
        /// </summary>
        [HttpGet, Route("api/user/sysparam")]
        public async Task<ActionResult<string>> SysParam([FromQuery] long duz)
        {
            var vq = new VistaQuery("ORWU SYSPARAM");
            vq.addParameter(VistaQuery.LITERAL, duz.ToString());
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Check if current user has a security key.
        /// RPC: ORWU HASKEY
        /// Returns: '1' if user has the key
        /// </summary>
        [HttpGet, Route("api/user/haskey")]
        public async Task<ActionResult<string>> HasKey([FromQuery] string keyName)
        {
            var vq = new VistaQuery("ORWU HASKEY");
            vq.addParameter(VistaQuery.LITERAL, keyName);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Check if a specific person has a security key.
        /// RPC: ORWU NPHASKEY
        /// Returns: '1' if person has the key
        /// </summary>
        [HttpGet, Route("api/user/personhaskey")]
        public async Task<ActionResult<string>> PersonHasKey(
            [FromQuery] long personIen,
            [FromQuery] string keyName)
        {
            var vq = new VistaQuery("ORWU NPHASKEY");
            vq.addParameter(VistaQuery.LITERAL, personIen.ToString());
            vq.addParameter(VistaQuery.LITERAL, keyName);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Check if current user has access to a menu option.
        /// RPC: ORWU HAS OPTION ACCESS
        /// Returns: '1' if user has access
        /// </summary>
        [HttpGet, Route("api/user/hasoptionaccess")]
        public async Task<ActionResult<string>> HasOptionAccess([FromQuery] string optionName)
        {
            var vq = new VistaQuery("ORWU HAS OPTION ACCESS");
            vq.addParameter(VistaQuery.LITERAL, optionName);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Validate an electronic signature code.
        /// RPC: ORWU VALIDSIG
        /// Returns: '1' if valid
        /// </summary>
        [HttpPost, Route("api/user/validatesignature")]
        public async Task<ActionResult<string>> ValidateSignature([FromQuery] string esCode)
        {
            var vq = new VistaQuery("ORWU VALIDSIG");
            vq.addEncryptedParameter(VistaQuery.LITERAL, esCode);
            return await this.Session.sQuery(vq);
        }

        #endregion

        #region General Lookups

        /// <summary>
        /// Get the external name of an IEN within a file.
        /// RPC: ORWU EXTNAME
        /// </summary>
        [HttpGet, Route("api/user/externalname")]
        public async Task<ActionResult<string>> ExternalName(
            [FromQuery] long ien,
            [FromQuery] double fileNumber)
        {
            var vq = new VistaQuery("ORWU EXTNAME");
            vq.addParameter(VistaQuery.LITERAL, ien.ToString());
            vq.addParameter(VistaQuery.LITERAL, fileNumber.ToString());
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Get the global reference for a file ID.
        /// RPC: ORWU GBLREF
        /// </summary>
        [HttpGet, Route("api/user/globalref")]
        public async Task<ActionResult<string>> GlobalRef([FromQuery] string fileId)
        {
            var vq = new VistaQuery("ORWU GBLREF");
            vq.addParameter(VistaQuery.LITERAL, fileId);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Get a subset of a generic global reference (long list box scrolling).
        /// RPC: ORWU GENERIC
        /// </summary>
        [HttpGet, Route("api/user/generic")]
        public async Task<ActionResult<List<string>>> Generic(
            [FromQuery] string startFrom,
            [FromQuery] int direction,
            [FromQuery] string globalRef)
        {
            var vq = new VistaQuery("ORWU GENERIC");
            vq.addParameter(VistaQuery.LITERAL, startFrom);
            vq.addParameter(VistaQuery.LITERAL, direction.ToString());
            vq.addParameter(VistaQuery.LITERAL, globalRef);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Convert/validate a date/time string using %DT.
        /// RPC: ORWU DT
        /// Accepts: date strings, T, T-1, NOW, etc.
        /// Returns: FM date/time
        /// </summary>
        [HttpGet, Route("api/user/fmdate")]
        public async Task<ActionResult<string>> FmDate([FromQuery] string dateString)
        {
            var vq = new VistaQuery("ORWU DT");
            vq.addParameter(VistaQuery.LITERAL, dateString);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Validate a date/time string with %DT flags.
        /// RPC: ORWU VALDT
        /// </summary>
        [HttpGet, Route("api/user/validatedate")]
        public async Task<ActionResult<string>> ValidateDate(
            [FromQuery] string dateString,
            [FromQuery] string flags = "")
        {
            var vq = new VistaQuery("ORWU VALDT");
            vq.addParameter(VistaQuery.LITERAL, dateString);
            vq.addParameter(VistaQuery.LITERAL, flags);
            return await this.Session.sQuery(vq);
        }

        #endregion

        #region Person & Location Subsets (Long List Box)

        /// <summary>
        /// Get a subset of persons (all users).
        /// RPC: ORWU NEWPERS
        /// </summary>
        [HttpGet, Route("api/user/persons")]
        public async Task<ActionResult<List<string>>> Persons(
            [FromQuery] string startFrom,
            [FromQuery] int direction)
        {
            var vq = new VistaQuery("ORWU NEWPERS");
            vq.addParameter(VistaQuery.LITERAL, startFrom);
            vq.addParameter(VistaQuery.LITERAL, direction.ToString());
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get a subset of providers.
        /// RPC: ORWU NEWPERS (with 'PROVIDER' class filter)
        /// </summary>
        [HttpGet, Route("api/user/providers")]
        public async Task<ActionResult<List<string>>> Providers(
            [FromQuery] string startFrom,
            [FromQuery] int direction,
            [FromQuery] string dateTime = "")
        {
            var vq = new VistaQuery("ORWU NEWPERS");
            vq.addParameter(VistaQuery.LITERAL, startFrom);
            vq.addParameter(VistaQuery.LITERAL, direction.ToString());
            vq.addParameter(VistaQuery.LITERAL, "PROVIDER");
            if (!string.IsNullOrEmpty(dateTime))
                vq.addParameter(VistaQuery.LITERAL, dateTime);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get a subset of users with a class (for a given date/time).
        /// RPC: ORWU NEWPERS (with empty class + dateTime filter)
        /// </summary>
        [HttpGet, Route("api/user/userswithclass")]
        public async Task<ActionResult<List<string>>> UsersWithClass(
            [FromQuery] string startFrom,
            [FromQuery] int direction,
            [FromQuery] string dateTime)
        {
            var vq = new VistaQuery("ORWU NEWPERS");
            vq.addParameter(VistaQuery.LITERAL, startFrom);
            vq.addParameter(VistaQuery.LITERAL, direction.ToString());
            vq.addParameter(VistaQuery.LITERAL, "");
            vq.addParameter(VistaQuery.LITERAL, dateTime);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get a subset of all active and inactive persons.
        /// RPC: ORWU NEWPERS (with includeInactive=true)
        /// </summary>
        [HttpGet, Route("api/user/allpersons")]
        public async Task<ActionResult<List<string>>> AllPersons(
            [FromQuery] string startFrom,
            [FromQuery] int direction)
        {
            var vq = new VistaQuery("ORWU NEWPERS");
            vq.addParameter(VistaQuery.LITERAL, startFrom);
            vq.addParameter(VistaQuery.LITERAL, direction.ToString());
            vq.addParameter(VistaQuery.LITERAL, "");
            vq.addParameter(VistaQuery.LITERAL, "");
            vq.addParameter(VistaQuery.LITERAL, "");
            vq.addParameter(VistaQuery.LITERAL, "1"); // TRUE = include inactive
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get a subset of cosigners for a given document context.
        /// RPC: ORWU2 COSIGNER
        /// </summary>
        [HttpGet, Route("api/user/cosigners")]
        public async Task<ActionResult<List<string>>> Cosigners(
            [FromQuery] string startFrom,
            [FromQuery] int direction,
            [FromQuery] string fmDate,
            [FromQuery] int docType = 0,
            [FromQuery] int title = 0)
        {
            var vq = new VistaQuery("ORWU2 COSIGNER");
            vq.addParameter(VistaQuery.LITERAL, startFrom);
            vq.addParameter(VistaQuery.LITERAL, direction.ToString());
            vq.addParameter(VistaQuery.LITERAL, fmDate);
            vq.addParameter(VistaQuery.LITERAL, docType.ToString());
            vq.addParameter(VistaQuery.LITERAL, title.ToString());
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get a subset of devices.
        /// RPC: ORWU DEVICE
        /// </summary>
        [HttpGet, Route("api/user/devices")]
        public async Task<ActionResult<List<string>>> Devices(
            [FromQuery] string startFrom,
            [FromQuery] int direction)
        {
            var vq = new VistaQuery("ORWU DEVICE");
            vq.addParameter(VistaQuery.LITERAL, startFrom);
            vq.addParameter(VistaQuery.LITERAL, direction.ToString());
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get a subset of hospital locations.
        /// RPC: ORWU HOSPLOC
        /// </summary>
        [HttpGet, Route("api/user/hospitallocations")]
        public async Task<ActionResult<List<string>>> HospitalLocations(
            [FromQuery] string startFrom,
            [FromQuery] int direction)
        {
            var vq = new VistaQuery("ORWU HOSPLOC");
            vq.addParameter(VistaQuery.LITERAL, startFrom);
            vq.addParameter(VistaQuery.LITERAL, direction.ToString());
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get a subset of clinic locations.
        /// RPC: ORWU CLINLOC
        /// </summary>
        [HttpGet, Route("api/user/cliniclocations")]
        public async Task<ActionResult<List<string>>> ClinicLocations(
            [FromQuery] string startFrom,
            [FromQuery] int direction)
        {
            var vq = new VistaQuery("ORWU CLINLOC");
            vq.addParameter(VistaQuery.LITERAL, startFrom);
            vq.addParameter(VistaQuery.LITERAL, direction.ToString());
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get a subset of inpatient locations.
        /// RPC: ORWU INPLOC
        /// </summary>
        [HttpGet, Route("api/user/inpatientlocations")]
        public async Task<ActionResult<List<string>>> InpatientLocations(
            [FromQuery] string startFrom,
            [FromQuery] int direction)
        {
            var vq = new VistaQuery("ORWU INPLOC");
            vq.addParameter(VistaQuery.LITERAL, startFrom);
            vq.addParameter(VistaQuery.LITERAL, direction.ToString());
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get a subset of new locations (clinics, wards, and other types).
        /// RPC: ORWU1 NEWLOC
        /// </summary>
        [HttpGet, Route("api/user/newlocations")]
        public async Task<ActionResult<List<string>>> NewLocations(
            [FromQuery] string startFrom,
            [FromQuery] int direction)
        {
            var vq = new VistaQuery("ORWU1 NEWLOC");
            vq.addParameter(VistaQuery.LITERAL, startFrom);
            vq.addParameter(VistaQuery.LITERAL, direction.ToString());
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Convert a provider IEN to a displayable name.
        /// RPC: ORWU1 NAMECVT
        /// </summary>
        [HttpGet, Route("api/user/nameconvert")]
        public async Task<ActionResult<string>> NameConvert([FromQuery] long providerIen)
        {
            var vq = new VistaQuery("ORWU1 NAMECVT");
            vq.addParameter(VistaQuery.LITERAL, providerIen.ToString());
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Get the default printer for a user at a location.
        /// RPC: ORWRP GET DEFAULT PRINTER
        /// </summary>
        [HttpGet, Route("api/user/defaultprinter")]
        public async Task<ActionResult<string>> DefaultPrinter(
            [FromQuery] long duz,
            [FromQuery] int location)
        {
            var vq = new VistaQuery("ORWRP GET DEFAULT PRINTER");
            vq.addParameter(VistaQuery.LITERAL, duz.ToString());
            vq.addParameter(VistaQuery.LITERAL, location.ToString());
            return await this.Session.sQuery(vq);
        }

        #endregion

        #region Simplified Aliases (used by Web.CPRS)

        /// <summary>
        /// Search users (simplified alias for ORWU NEWPERS).
        /// </summary>
        [HttpGet, Route("api/user/search")]
        public async Task<ActionResult<List<string>>> SearchUsers(
            [FromQuery] string search = "")
        {
            var vq = new VistaQuery("ORWU NEWPERS");
            vq.addParameter(VistaQuery.LITERAL, search);
            vq.addParameter(VistaQuery.LITERAL, "1");
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// List clinics (simplified alias for ORQPT CLINICS).
        /// </summary>
        [HttpGet, Route("api/location/clinics")]
        public async Task<ActionResult<List<string>>> Clinics()
        {
            var vq = new VistaQuery("ORQPT CLINICS");
            return await this.Session.tQuery(vq);
        }

        #endregion

        #region Division Selection

        /// <summary>
        /// Get the list of divisions the current user is authorized for.
        /// Returns cached data from login — XUS DIVISION GET was called during
        /// the signon phase before the OR CPRS GUI CHART context switch.
        /// </summary>
        [HttpGet, Route("api/user/divisions")]
        public ActionResult<List<string>> Divisions()
        {
            return this.Session.CachedDivisionLines ?? new List<string> { "0" };
        }

        /// <summary>
        /// Set the current user's division and complete login by switching to the
        /// OR CPRS GUI CHART context. This is Phase 2 of the two-phase login —
        /// XUS DIVISION SET and XWB CREATE CONTEXT both run in the XUS SIGNON context
        /// before the context switch occurs.
        /// Returns: '1' if successful
        /// </summary>
        [HttpPost, Route("api/user/division/set")]
        public async Task<ActionResult<string>> SetDivision([FromQuery] string stationNumber)
        {
            await this.Session.SelectDivisionAndSetContext(stationNumber);
            return "1";
        }

        #endregion
    }
}
