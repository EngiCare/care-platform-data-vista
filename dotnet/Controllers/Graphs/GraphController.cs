using CarePlatform.Data.VistA;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarePlatform.Data.CPRS
{
    /// <summary>
    /// Graphs — lab-test groups, fast data, items, types, classes, taxonomy,
    /// date ranges, user preferences, views, sizing, test specifications.
    /// Migrated from cprs/rGraphs.pas — ORWGRPC *, ORWLRR TG/ATESTS/ATG RPCs.
    /// </summary>
    public class GraphController : BaseController
    {
        public GraphController() : base() { }

        #region Lab Test Groups

        /// <summary>
        /// Get lab test groups.
        /// RPC: ORWLRR TG
        /// </summary>
        [HttpGet, Route("api/graph/testgroups")]
        public async Task<ActionResult<List<string>>> TestGroups(
            [FromQuery] string startFrom,
            [FromQuery] int direction)
        {
            var vq = new VistaQuery("ORWLRR TG");
            vq.addParameter(VistaQuery.LITERAL, startFrom);
            vq.addParameter(VistaQuery.LITERAL, direction.ToString());
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get all tests for a test group.
        /// RPC: ORWLRR ATESTS
        /// </summary>
        [HttpGet, Route("api/graph/atests")]
        public async Task<ActionResult<List<string>>> ATests(
            [FromQuery] string startFrom,
            [FromQuery] int direction)
        {
            var vq = new VistaQuery("ORWLRR ATESTS");
            vq.addParameter(VistaQuery.LITERAL, startFrom);
            vq.addParameter(VistaQuery.LITERAL, direction.ToString());
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get all test groups (atomic test groups).
        /// RPC: ORWLRR ATG
        /// </summary>
        [HttpGet, Route("api/graph/atg")]
        public async Task<ActionResult<List<string>>> Atg(
            [FromQuery] string startFrom,
            [FromQuery] int direction)
        {
            var vq = new VistaQuery("ORWLRR ATG");
            vq.addParameter(VistaQuery.LITERAL, startFrom);
            vq.addParameter(VistaQuery.LITERAL, direction.ToString());
            return await this.Session.tQuery(vq);
        }

        #endregion

        #region Public / Report Settings

        /// <summary>
        /// Get public graph settings.
        /// RPC: ORWGRPC PUBLIC
        /// </summary>
        [HttpGet, Route("api/graph/public")]
        public async Task<ActionResult<List<string>>> PublicSettings()
        {
            var vq = new VistaQuery("ORWGRPC PUBLIC");
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get graph report parameters for a patient.
        /// RPC: ORWGRPC RPTPARAM
        /// </summary>
        [HttpGet, Route("api/graph/rptparam")]
        public async Task<ActionResult<string>> RptParam([FromQuery] string dfn)
        {
            var vq = new VistaQuery("ORWGRPC RPTPARAM");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            return await this.Session.sQuery(vq);
        }

        #endregion

        #region Graph Data

        /// <summary>
        /// Get graph data for a selected item.
        /// RPC: ORWGRPC DATA
        /// </summary>
        [HttpGet, Route("api/graph/data")]
        public async Task<ActionResult<List<string>>> Data(
            [FromQuery] string item = "",
            [FromQuery] string range = "")
        {
            var vq = new VistaQuery("ORWGRPC DATA");
            vq.addParameter(VistaQuery.LITERAL, item);
            vq.addParameter(VistaQuery.LITERAL, range);
            return await this.Session.tQuery(vq);
        }

        #endregion

        #region Detail Data

        /// <summary>
        /// Get details for a graph data point by date range (rpcDetailDay).
        /// RPC: ORWGRPC DETAILS
        /// Params: dfn, date1, date2, typeItem, complete
        /// </summary>
        [HttpGet, Route("api/graph/details")]
        public async Task<ActionResult<List<string>>> Details(
            [FromQuery] string dfn,
            [FromQuery] string date1,
            [FromQuery] string date2,
            [FromQuery] string typeItem,
            [FromQuery] string complete = "")
        {
            var vq = new VistaQuery("ORWGRPC DETAILS");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            vq.addParameter(VistaQuery.LITERAL, date1);
            vq.addParameter(VistaQuery.LITERAL, date2);
            vq.addParameter(VistaQuery.LITERAL, typeItem);
            vq.addParameter(VistaQuery.LITERAL, complete);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get detail for selected items (rpcDetailSelected).
        /// RPC: ORWGRPC DETAIL
        /// Params: dfn, date1, date2, tests (list), complete
        /// </summary>
        [HttpPost, Route("api/graph/detail")]
        public async Task<ActionResult<List<string>>> Detail(
            [FromQuery] string dfn,
            [FromQuery] string date1,
            [FromQuery] string date2,
            [FromQuery] string complete,
            [FromBody] List<string> tests)
        {
            var vq = new VistaQuery("ORWGRPC DETAIL");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            vq.addParameter(VistaQuery.LITERAL, date1);
            vq.addParameter(VistaQuery.LITERAL, date2);
            var dhl = new DictionaryHashList();
            for (int i = 0; i < tests.Count; i++)
                dhl.Add((i + 1).ToString(), tests[i]);
            vq.addParameter(VistaQuery.LIST, dhl);
            vq.addParameter(VistaQuery.LITERAL, complete);
            return await this.Session.tQuery(vq);
        }

        #endregion

        #region Fast Data

        /// <summary>
        /// Get fast data (summary/header) for graphing.
        /// RPC: ORWGRPC FASTDATA
        /// </summary>
        [HttpGet, Route("api/graph/fastdata")]
        public async Task<ActionResult<List<string>>> FastData([FromQuery] string dfn)
        {
            var vq = new VistaQuery("ORWGRPC FASTDATA");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get a fast item for graphing.
        /// RPC: ORWGRPC FASTITEM
        /// </summary>
        [HttpGet, Route("api/graph/fastitem")]
        public async Task<ActionResult<List<string>>> FastItem(
            [FromQuery] string dfn,
            [FromQuery] string item)
        {
            var vq = new VistaQuery("ORWGRPC FASTITEM");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            vq.addParameter(VistaQuery.LITERAL, item);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get fast lab data for graphing.
        /// RPC: ORWGRPC FASTLABS
        /// </summary>
        [HttpGet, Route("api/graph/fastlabs")]
        public async Task<ActionResult<List<string>>> FastLabs([FromQuery] string dfn)
        {
            var vq = new VistaQuery("ORWGRPC FASTLABS");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get fast task data for graphing.
        /// RPC: ORWGRPC FASTTASK
        /// Params: dfn, oldDFN (prior patient for cache management)
        /// </summary>
        [HttpGet, Route("api/graph/fasttask")]
        public async Task<ActionResult<List<string>>> FastTask(
            [FromQuery] string dfn,
            [FromQuery] string oldDfn = "")
        {
            var vq = new VistaQuery("ORWGRPC FASTTASK");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            vq.addParameter(VistaQuery.LITERAL, oldDfn);
            return await this.Session.tQuery(vq);
        }

        #endregion

        #region Items & Types

        /// <summary>
        /// Get all items available for graphing.
        /// RPC: ORWGRPC ALLITEMS
        /// Params: dfn
        /// </summary>
        [HttpGet, Route("api/graph/allitems")]
        public async Task<ActionResult<List<string>>> AllItems([FromQuery] string dfn)
        {
            var vq = new VistaQuery("ORWGRPC ALLITEMS");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get graph types.
        /// RPC: ORWGRPC TYPES
        /// Params: dfn, subtypes (boolean flag)
        /// </summary>
        [HttpGet, Route("api/graph/types")]
        public async Task<ActionResult<List<string>>> Types(
            [FromQuery] string dfn,
            [FromQuery] bool subtypes = false)
        {
            var vq = new VistaQuery("ORWGRPC TYPES");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            vq.addParameter(VistaQuery.LITERAL, subtypes ? "1" : "0");
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get items for a given type.
        /// RPC: ORWGRPC ITEMS
        /// </summary>
        [HttpGet, Route("api/graph/items")]
        public async Task<ActionResult<List<string>>> Items(
            [FromQuery] string dfn,
            [FromQuery] string type)
        {
            var vq = new VistaQuery("ORWGRPC ITEMS");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            vq.addParameter(VistaQuery.LITERAL, type);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get item data.
        /// RPC: ORWGRPC ITEMDATA
        /// Params: itemdata, timestamp, dfn
        /// </summary>
        [HttpGet, Route("api/graph/itemdata")]
        public async Task<ActionResult<List<string>>> ItemData(
            [FromQuery] string itemdata,
            [FromQuery] string timestamp,
            [FromQuery] string dfn)
        {
            var vq = new VistaQuery("ORWGRPC ITEMDATA");
            vq.addParameter(VistaQuery.LITERAL, itemdata);
            vq.addParameter(VistaQuery.LITERAL, timestamp);
            vq.addParameter(VistaQuery.LITERAL, dfn);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get data class for an item type.
        /// RPC: ORWGRPC CLASS
        /// Params: itemtype
        /// </summary>
        [HttpGet, Route("api/graph/class")]
        public async Task<ActionResult<List<string>>> DataClass([FromQuery] string itemtype)
        {
            var vq = new VistaQuery("ORWGRPC CLASS");
            vq.addParameter(VistaQuery.LITERAL, itemtype);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get taxonomy data for graphing.
        /// RPC: ORWGRPC TAX
        /// Params: getAll (boolean), taxonomies (list)
        /// </summary>
        [HttpPost, Route("api/graph/tax")]
        public async Task<ActionResult<List<string>>> Taxonomy(
            [FromQuery] bool getAll,
            [FromBody] List<string> taxonomies)
        {
            var vq = new VistaQuery("ORWGRPC TAX");
            vq.addParameter(VistaQuery.LITERAL, getAll ? "1" : "0");
            var dhl = new DictionaryHashList();
            for (int i = 0; i < taxonomies.Count; i++)
                dhl.Add((i + 1).ToString(), taxonomies[i]);
            vq.addParameter(VistaQuery.LIST, dhl);
            return await this.Session.tQuery(vq);
        }

        #endregion

        #region Dates

        /// <summary>
        /// Get date ranges for graphing.
        /// RPC: ORWGRPC GETDATES
        /// Params: reportId
        /// </summary>
        [HttpGet, Route("api/graph/getdates")]
        public async Task<ActionResult<string>> GetDates([FromQuery] string reportId = "")
        {
            var vq = new VistaQuery("ORWGRPC GETDATES");
            vq.addParameter(VistaQuery.LITERAL, reportId);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Get date-item graph data.
        /// RPC: ORWGRPC DATEITEM
        /// </summary>
        [HttpGet, Route("api/graph/dateitem")]
        public async Task<ActionResult<List<string>>> DateItem(
            [FromQuery] string dfn,
            [FromQuery] string date1,
            [FromQuery] string date2,
            [FromQuery] string item)
        {
            var vq = new VistaQuery("ORWGRPC DATEITEM");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            vq.addParameter(VistaQuery.LITERAL, date1);
            vq.addParameter(VistaQuery.LITERAL, date2);
            vq.addParameter(VistaQuery.LITERAL, item);
            return await this.Session.tQuery(vq);
        }

        #endregion

        #region User Preferences

        /// <summary>
        /// Get user preferences for graphing.
        /// RPC: ORWGRPC GETPREF
        /// </summary>
        [HttpGet, Route("api/graph/getpref")]
        public async Task<ActionResult<string>> GetPref()
        {
            var vq = new VistaQuery("ORWGRPC GETPREF");
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Save user preferences for graphing.
        /// RPC: ORWGRPC SETPREF
        /// Params: paramSetting, permission
        /// </summary>
        [HttpPost, Route("api/graph/setpref")]
        public async Task<ActionResult<string>> SetPref(
            [FromQuery] string settings,
            [FromQuery] string permission = "")
        {
            var vq = new VistaQuery("ORWGRPC SETPREF");
            vq.addParameter(VistaQuery.LITERAL, settings);
            vq.addParameter(VistaQuery.LITERAL, permission);
            return await this.Session.sQuery(vq);
        }

        #endregion

        #region Sizing

        /// <summary>
        /// Get graph sizing preferences.
        /// RPC: ORWGRPC GETSIZE
        /// </summary>
        [HttpGet, Route("api/graph/getsize")]
        public async Task<ActionResult<string>> GetSize()
        {
            var vq = new VistaQuery("ORWGRPC GETSIZE");
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Save graph sizing preferences.
        /// RPC: ORWGRPC SETSIZE
        /// Params: sizeSettings (list)
        /// </summary>
        [HttpPost, Route("api/graph/setsize")]
        public async Task<ActionResult<string>> SetSize([FromBody] List<string> sizeSettings)
        {
            var vq = new VistaQuery("ORWGRPC SETSIZE");
            var dhl = new DictionaryHashList();
            for (int i = 0; i < sizeSettings.Count; i++)
                dhl.Add((i + 1).ToString(), sizeSettings[i]);
            vq.addParameter(VistaQuery.LIST, dhl);
            return await this.Session.sQuery(vq);
        }

        #endregion

        #region Views

        /// <summary>
        /// Get saved graph profiles/views.
        /// RPC: ORWGRPC GETVIEWS
        /// Params: profiles, permission, ext, userx
        /// </summary>
        [HttpGet, Route("api/graph/getviews")]
        public async Task<ActionResult<List<string>>> GetViews(
            [FromQuery] string profiles = "",
            [FromQuery] string permission = "",
            [FromQuery] string ext = "",
            [FromQuery] string userx = "")
        {
            var vq = new VistaQuery("ORWGRPC GETVIEWS");
            vq.addParameter(VistaQuery.LITERAL, profiles);
            vq.addParameter(VistaQuery.LITERAL, permission);
            vq.addParameter(VistaQuery.LITERAL, ext);
            vq.addParameter(VistaQuery.LITERAL, userx);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Save a graph profile/view.
        /// RPC: ORWGRPC SETVIEWS
        /// Params: paramName, permission, paramValues (list)
        /// </summary>
        [HttpPost, Route("api/graph/setviews")]
        public async Task<ActionResult<string>> SetViews(
            [FromQuery] string paramName,
            [FromQuery] string permission,
            [FromBody] List<string> paramValues)
        {
            var vq = new VistaQuery("ORWGRPC SETVIEWS");
            vq.addParameter(VistaQuery.LITERAL, paramName);
            vq.addParameter(VistaQuery.LITERAL, permission);
            var dhl = new DictionaryHashList();
            for (int i = 0; i < paramValues.Count; i++)
                dhl.Add((i + 1).ToString(), paramValues[i]);
            vq.addParameter(VistaQuery.LIST, dhl);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Delete a saved graph profile/view.
        /// RPC: ORWGRPC DELVIEWS
        /// Params: paramName, permission
        /// </summary>
        [HttpPost, Route("api/graph/delviews")]
        public async Task<ActionResult<string>> DelViews(
            [FromQuery] string viewName,
            [FromQuery] string permission = "")
        {
            var vq = new VistaQuery("ORWGRPC DELVIEWS");
            vq.addParameter(VistaQuery.LITERAL, viewName);
            vq.addParameter(VistaQuery.LITERAL, permission);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Get all views (including public views).
        /// RPC: ORWGRPC ALLVIEWS
        /// Params: vtype, user
        /// </summary>
        [HttpGet, Route("api/graph/allviews")]
        public async Task<ActionResult<List<string>>> AllViews(
            [FromQuery] string vtype = "",
            [FromQuery] string user = "")
        {
            var vq = new VistaQuery("ORWGRPC ALLVIEWS");
            vq.addParameter(VistaQuery.LITERAL, vtype);
            vq.addParameter(VistaQuery.LITERAL, user);
            return await this.Session.tQuery(vq);
        }

        #endregion

        #region Test Specification & Lookup

        /// <summary>
        /// Get test specification data for graphing.
        /// RPC: ORWGRPC TESTSPEC
        /// </summary>
        [HttpGet, Route("api/graph/testspec")]
        public async Task<ActionResult<List<string>>> TestSpec([FromQuery] string item)
        {
            var vq = new VistaQuery("ORWGRPC TESTSPEC");
            vq.addParameter(VistaQuery.LITERAL, item);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get testing data for graph items.
        /// RPC: ORWGRPC TESTING
        /// </summary>
        [HttpGet, Route("api/graph/testing")]
        public async Task<ActionResult<List<string>>> Testing(
            [FromQuery] string dfn,
            [FromQuery] string item)
        {
            var vq = new VistaQuery("ORWGRPC TESTING");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            vq.addParameter(VistaQuery.LITERAL, item);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Lookup graph items by name.
        /// RPC: ORWGRPC LOOKUP
        /// </summary>
        [HttpGet, Route("api/graph/lookup")]
        public async Task<ActionResult<List<string>>> Lookup(
            [FromQuery] string file,
            [FromQuery] string startFrom,
            [FromQuery] int direction)
        {
            var vq = new VistaQuery("ORWGRPC LOOKUP");
            vq.addParameter(VistaQuery.LITERAL, file);
            vq.addParameter(VistaQuery.LITERAL, startFrom);
            vq.addParameter(VistaQuery.LITERAL, direction.ToString());
            return await this.Session.tQuery(vq);
        }

        #endregion
    }
}
