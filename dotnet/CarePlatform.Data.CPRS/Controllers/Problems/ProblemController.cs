using CarePlatform.Data.VistA;
using CarePlatform.Models.Problems;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarePlatform.Data.CPRS
{
    /// <summary>
    /// Problem list management — CRUD, lexicon search, filters, categories.
    /// Migrated from cprs/rProbs.pas — ORQQPL * RPCs.
    /// </summary>
    public class ProblemController : BaseController
    {
        public ProblemController() : base() { }

        #region Problem List & Detail

        /// <summary>
        /// Get the problem list for a patient.
        /// RPC: ORQQPL PROBLEM LIST
        /// </summary>
        [HttpGet, Route("api/problem/list")]
        public async Task<ActionResult<List<Models.Problems.Problem>>> List(
            [FromQuery] string dfn,
            [FromQuery] string status,
            [FromQuery] string fmDate = "")
        {
            var vq = new VistaQuery("ORQQPL PROBLEM LIST");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            vq.addParameter(VistaQuery.LITERAL, status);
            vq.addParameter(VistaQuery.LITERAL, fmDate);
            var results = await this.Session.tQuery(vq);
            return Models.Problems.Problem.ParseList(results);
        }

        /// <summary>
        /// Get detailed problem info.
        /// RPC: ORQQPL DETAIL (via CoverSheetController for cover sheet; here for full detail view)
        /// </summary>
        [HttpGet, Route("api/problem/detail")]
        public async Task<ActionResult<List<string>>> Detail(
            [FromQuery] string dfn,
            [FromQuery] int ien,
            [FromQuery] string dateTime = "")
        {
            var vq = new VistaQuery("ORQQPL DETAIL");
            vq.addParameter(VistaQuery.LITERAL, dfn + "^" + dateTime);
            vq.addParameter(VistaQuery.LITERAL, ien.ToString());
            vq.addParameter(VistaQuery.LITERAL, "");
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get problem comments.
        /// RPC: ORQQPL PROB COMMENTS
        /// </summary>
        [HttpGet, Route("api/problem/comments")]
        public async Task<ActionResult<List<string>>> Comments([FromQuery] string problemIfn)
        {
            var vq = new VistaQuery("ORQQPL PROB COMMENTS");
            vq.addParameter(VistaQuery.LITERAL, problemIfn);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get audit history for a problem.
        /// RPC: ORQQPL AUDIT HIST
        /// </summary>
        [HttpGet, Route("api/problem/audithistory")]
        public async Task<ActionResult<List<string>>> AuditHistory([FromQuery] string problemIfn)
        {
            var vq = new VistaQuery("ORQQPL AUDIT HIST");
            vq.addParameter(VistaQuery.LITERAL, problemIfn);
            return await this.Session.tQuery(vq);
        }

        #endregion

        #region Problem CRUD

        /// <summary>
        /// Load a problem for editing.
        /// RPC: ORQQPL EDIT LOAD
        /// </summary>
        [HttpGet, Route("api/problem/editload")]
        public async Task<ActionResult<List<string>>> EditLoad([FromQuery] string problemIfn)
        {
            var vq = new VistaQuery("ORQQPL EDIT LOAD");
            vq.addParameter(VistaQuery.LITERAL, problemIfn);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Save an edited problem.
        /// RPC: ORQQPL EDIT SAVE
        /// </summary>
        [HttpPost, Route("api/problem/editsave")]
        public async Task<ActionResult<List<string>>> EditSave(
            [FromQuery] string problemIfn,
            [FromQuery] long providerId,
            [FromQuery] string ptVamc,
            [FromQuery] string primUser,
            [FromBody] List<string> problemFile,
            [FromQuery] string searchString = "")
        {
            var vq = new VistaQuery("ORQQPL EDIT SAVE");
            vq.addParameter(VistaQuery.LITERAL, problemIfn);
            vq.addParameter(VistaQuery.LITERAL, providerId.ToString());
            vq.addParameter(VistaQuery.LITERAL, ptVamc);
            vq.addParameter(VistaQuery.LITERAL, primUser);
            var dhl = new DictionaryHashList();
            for (int i = 0; i < problemFile.Count; i++)
                dhl.Add(i.ToString(), problemFile[i]);
            vq.addParameter(VistaQuery.LIST, dhl);
            vq.addParameter(VistaQuery.LITERAL, searchString);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Add and save a new problem.
        /// RPC: ORQQPL ADD SAVE
        /// </summary>
        [HttpPost, Route("api/problem/addsave")]
        public async Task<ActionResult<List<string>>> AddSave(
            [FromQuery] string patientInfo,
            [FromQuery] long providerId,
            [FromQuery] string ptVamc,
            [FromBody] List<string> problemFile,
            [FromQuery] string searchString = "")
        {
            var vq = new VistaQuery("ORQQPL ADD SAVE");
            vq.addParameter(VistaQuery.LITERAL, patientInfo);
            vq.addParameter(VistaQuery.LITERAL, providerId.ToString());
            vq.addParameter(VistaQuery.LITERAL, ptVamc);
            var dhl = new DictionaryHashList();
            for (int i = 0; i < problemFile.Count; i++)
                dhl.Add(i.ToString(), problemFile[i]);
            vq.addParameter(VistaQuery.LIST, dhl);
            vq.addParameter(VistaQuery.LITERAL, searchString);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Delete a problem.
        /// RPC: ORQQPL DELETE
        /// </summary>
        [HttpPost, Route("api/problem/delete")]
        public async Task<ActionResult<List<string>>> Delete(
            [FromQuery] string problemIfn,
            [FromQuery] long providerId,
            [FromQuery] string ptVamc,
            [FromQuery] string comment = "")
        {
            var vq = new VistaQuery("ORQQPL DELETE");
            vq.addParameter(VistaQuery.LITERAL, problemIfn);
            vq.addParameter(VistaQuery.LITERAL, providerId.ToString());
            vq.addParameter(VistaQuery.LITERAL, ptVamc);
            vq.addParameter(VistaQuery.LITERAL, comment);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Update a problem.
        /// RPC: ORQQPL UPDATE
        /// </summary>
        [HttpPost, Route("api/problem/update")]
        public async Task<ActionResult<List<string>>> Update([FromBody] List<string> altProbFile)
        {
            var vq = new VistaQuery("ORQQPL UPDATE");
            var dhl = new DictionaryHashList();
            for (int i = 0; i < altProbFile.Count; i++)
                dhl.Add(i.ToString(), altProbFile[i]);
            vq.addParameter(VistaQuery.LIST, dhl);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Verify a problem.
        /// RPC: ORQQPL VERIFY
        /// </summary>
        [HttpPost, Route("api/problem/verify")]
        public async Task<ActionResult<List<string>>> Verify([FromQuery] string problemIfn)
        {
            var vq = new VistaQuery("ORQQPL VERIFY");
            vq.addParameter(VistaQuery.LITERAL, problemIfn);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Replace a problem entry.
        /// RPC: ORQQPL REPLACE
        /// </summary>
        [HttpPost, Route("api/problem/replace")]
        public async Task<ActionResult<List<string>>> Replace([FromQuery] string problemIfn)
        {
            var vq = new VistaQuery("ORQQPL REPLACE");
            vq.addParameter(VistaQuery.LITERAL, problemIfn);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Check for a duplicate problem.
        /// RPC: ORQQPL CHECK DUP
        /// </summary>
        [HttpGet, Route("api/problem/checkduplicate")]
        public async Task<ActionResult<string>> CheckDuplicate(
            [FromQuery] string dfn,
            [FromQuery] string termIen,
            [FromQuery] string termText)
        {
            var vq = new VistaQuery("ORQQPL CHECK DUP");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            vq.addParameter(VistaQuery.LITERAL, termIen);
            vq.addParameter(VistaQuery.LITERAL, termText);
            return await this.Session.sQuery(vq);
        }

        #endregion

        #region Initialization & Preferences

        /// <summary>
        /// Initialize patient context for the problem list.
        /// RPC: ORQQPL INIT PT
        /// </summary>
        [HttpGet, Route("api/problem/initpatient")]
        public async Task<ActionResult<List<string>>> InitPatient([FromQuery] string dfn)
        {
            var vq = new VistaQuery("ORQQPL INIT PT");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Initialize user context for the problem list.
        /// RPC: ORQQPL INIT USER
        /// </summary>
        [HttpGet, Route("api/problem/inituser")]
        public async Task<ActionResult<List<string>>> InitUser([FromQuery] long providerId)
        {
            var vq = new VistaQuery("ORQQPL INIT USER");
            vq.addParameter(VistaQuery.LITERAL, providerId.ToString());
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Save problem list view preferences.
        /// RPC: ORQQPL SAVEVIEW
        /// </summary>
        [HttpPost, Route("api/problem/saveviewpreferences")]
        public async Task<ActionResult<string>> SaveViewPreferences([FromQuery] string viewPref)
        {
            var vq = new VistaQuery("ORQQPL SAVEVIEW");
            vq.addParameter(VistaQuery.LITERAL, viewPref);
            return await this.Session.sQuery(vq);
        }

        #endregion

        #region Lexicon & Search

        /// <summary>
        /// Lexicon search for problems.
        /// RPC: ORQQPL4 LEX
        /// </summary>
        [HttpGet, Route("api/problem/lexiconsearch")]
        public async Task<ActionResult<List<ProblemLexiconResult>>> LexiconSearch(
            [FromQuery] string searchFor,
            [FromQuery] string view,
            [FromQuery] string fmDate = "",
            [FromQuery] bool extend = true)
        {
            var vq = new VistaQuery("ORQQPL4 LEX");
            vq.addParameter(VistaQuery.LITERAL, searchFor);
            vq.addParameter(VistaQuery.LITERAL, view);
            vq.addParameter(VistaQuery.LITERAL, fmDate);
            vq.addParameter(VistaQuery.LITERAL, extend ? "1" : "0");
            var results = await this.Session.tQuery(vq);
            return ProblemLexiconResult.ParseList(results);
        }

        /// <summary>
        /// Send NTRT (New Term Rapid Turnaround) bulletin for an unmatched problem.
        /// RPC: ORQQPL PROBLEM NTRT BULLETIN
        /// </summary>
        [HttpPost, Route("api/problem/ntrtbulletin")]
        public async Task<ActionResult<string>> NtrtBulletin(
            [FromQuery] string term,
            [FromQuery] string providerId,
            [FromQuery] string patientId,
            [FromQuery] string comment)
        {
            var vq = new VistaQuery("ORQQPL PROBLEM NTRT BULLETIN");
            vq.addParameter(VistaQuery.LITERAL, term);
            vq.addParameter(VistaQuery.LITERAL, providerId);
            vq.addParameter(VistaQuery.LITERAL, patientId);
            vq.addParameter(VistaQuery.LITERAL, comment);
            return await this.Session.sQuery(vq);
        }

        #endregion

        #region Filters & Providers

        /// <summary>
        /// Get patient's team providers for the problem list.
        /// RPC: ORQPT PATIENT TEAM PROVIDERS
        /// </summary>
        [HttpGet, Route("api/problem/patientproviders")]
        public async Task<ActionResult<List<string>>> PatientProviders([FromQuery] string dfn)
        {
            var vq = new VistaQuery("ORQPT PATIENT TEAM PROVIDERS");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get provider list (for problem assignment).
        /// RPC: ORQQPL PROVIDER LIST
        /// </summary>
        [HttpGet, Route("api/problem/providerlist")]
        public async Task<ActionResult<List<string>>> ProviderList(
            [FromQuery] string flag,
            [FromQuery] int number,
            [FromQuery] string from,
            [FromQuery] string part)
        {
            var vq = new VistaQuery("ORQQPL PROVIDER LIST");
            vq.addParameter(VistaQuery.LITERAL, flag);
            vq.addParameter(VistaQuery.LITERAL, number.ToString());
            vq.addParameter(VistaQuery.LITERAL, from);
            vq.addParameter(VistaQuery.LITERAL, part);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get provider filter list for problems.
        /// RPC: ORQQPL PROV FILTER LIST
        /// </summary>
        [HttpPost, Route("api/problem/providerfilterlist")]
        public async Task<ActionResult<List<string>>> ProviderFilterList([FromBody] List<string> provList)
        {
            var vq = new VistaQuery("ORQQPL PROV FILTER LIST");
            var dhl = new DictionaryHashList();
            for (int i = 0; i < provList.Count; i++)
                dhl.Add(i.ToString(), provList[i]);
            vq.addParameter(VistaQuery.LIST, dhl);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get clinic filter list for problems.
        /// RPC: ORQQPL CLIN FILTER LIST
        /// </summary>
        [HttpPost, Route("api/problem/clinicfilterlist")]
        public async Task<ActionResult<List<string>>> ClinicFilterList([FromBody] List<string> locList)
        {
            var vq = new VistaQuery("ORQQPL CLIN FILTER LIST");
            var dhl = new DictionaryHashList();
            for (int i = 0; i < locList.Count; i++)
                dhl.Add(i.ToString(), locList[i]);
            vq.addParameter(VistaQuery.LIST, dhl);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get service filter list for problems.
        /// RPC: ORQQPL SERV FILTER LIST
        /// </summary>
        [HttpPost, Route("api/problem/servicefilterlist")]
        public async Task<ActionResult<List<string>>> ServiceFilterList([FromBody] List<string> locList)
        {
            var vq = new VistaQuery("ORQQPL SERV FILTER LIST");
            var dhl = new DictionaryHashList();
            for (int i = 0; i < locList.Count; i++)
                dhl.Add(i.ToString(), locList[i]);
            vq.addParameter(VistaQuery.LIST, dhl);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Search clinics.
        /// RPC: ORQQPL CLIN SRCH
        /// </summary>
        [HttpGet, Route("api/problem/clinicsearch")]
        public async Task<ActionResult<List<string>>> ClinicSearch([FromQuery] string searchArg = "")
        {
            var vq = new VistaQuery("ORQQPL CLIN SRCH");
            vq.addParameter(VistaQuery.LITERAL, searchArg);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Search services.
        /// RPC: ORQQPL SRVC SRCH
        /// </summary>
        [HttpGet, Route("api/problem/servicesearch")]
        public async Task<ActionResult<List<string>>> ServiceSearch(
            [FromQuery] string startFrom,
            [FromQuery] int direction,
            [FromQuery] bool all = false)
        {
            var vq = new VistaQuery("ORQQPL SRVC SRCH");
            vq.addParameter(VistaQuery.LITERAL, startFrom);
            vq.addParameter(VistaQuery.LITERAL, direction.ToString());
            vq.addParameter(VistaQuery.LITERAL, all ? "1" : "0");
            return await this.Session.tQuery(vq);
        }

        #endregion

        #region User Problem Categories

        /// <summary>
        /// Get user problem categories.
        /// RPC: ORQQPL USER PROB CATS
        /// </summary>
        [HttpGet, Route("api/problem/usercategories")]
        public async Task<ActionResult<List<string>>> UserCategories(
            [FromQuery] long provider,
            [FromQuery] int location)
        {
            var vq = new VistaQuery("ORQQPL USER PROB CATS");
            vq.addParameter(VistaQuery.LITERAL, provider.ToString());
            vq.addParameter(VistaQuery.LITERAL, location.ToString());
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get user problem list for a category.
        /// RPC: ORQQPL USER PROB LIST
        /// </summary>
        [HttpGet, Route("api/problem/userproblemlist")]
        public async Task<ActionResult<List<string>>> UserProblemList([FromQuery] string categoryIen)
        {
            var vq = new VistaQuery("ORQQPL USER PROB LIST");
            vq.addParameter(VistaQuery.LITERAL, categoryIen);
            return await this.Session.tQuery(vq);
        }

        #endregion
    }
}
