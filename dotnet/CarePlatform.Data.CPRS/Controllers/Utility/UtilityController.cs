// Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
using CarePlatform.Data.VistA;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarePlatform.Data.CPRS
{
    /// <summary>
    /// Miscellaneous utility endpoints — primary-care detail, tool menu, symbol
    /// table, version checks, patch checks, RPC availability, font/size
    /// persistence, share node, DLL version checks, COM object hooks, and
    /// real-time-consult additional info.
    /// Migrated from cprs/rMisc.pas, cprs/rEventHooks.pas, cprs/Orders/rODRTC.pas
    /// — ORWPT1, ORWU, ORWUX, XWB, ORWCH, ORWPT, ORUTL4, ORWCOM, ORWDSD1 RPCs.
    /// </summary>
    public class UtilityController : BaseController
    {
        public UtilityController() : base() { }

        #region Primary Care

        /// <summary>
        /// Get primary-care detail for a patient.
        /// RPC: ORWPT1 PCDETAIL
        /// </summary>
        [HttpGet, Route("api/utility/primarycaredetail")]
        public async Task<ActionResult<List<string>>> DetailPrimaryCare([FromQuery] string dfn)
        {
            var vq = new VistaQuery("ORWPT1 PCDETAIL");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            return await this.Session.tQuery(vq);
        }

        #endregion

        #region Tool Menu

        /// <summary>
        /// Retrieve the tool menu items configured for the current user.
        /// RPC: ORWU TOOLMENU
        /// </summary>
        [HttpGet, Route("api/utility/toolmenu")]
        public async Task<ActionResult<List<string>>> GetToolMenu()
        {
            var vq = new VistaQuery("ORWU TOOLMENU");
            return await this.Session.tQuery(vq);
        }

        #endregion

        #region Symbol Table

        /// <summary>
        /// List the current M symbol table entries.
        /// RPC: ORWUX SYMTAB
        /// </summary>
        [HttpGet, Route("api/utility/symboltable")]
        public async Task<ActionResult<List<string>>> ListSymbolTable()
        {
            var vq = new VistaQuery("ORWUX SYMTAB");
            return await this.Session.tQuery(vq);
        }

        #endregion

        #region Server Variables

        /// <summary>
        /// Get a server-side M variable value by reference.
        /// RPC: XWB GET VARIABLE VALUE
        /// </summary>
        [HttpGet, Route("api/utility/getvar")]
        public async Task<ActionResult<string>> GetVariableValue([FromQuery] string reference)
        {
            var vq = new VistaQuery("XWB GET VARIABLE VALUE");
            vq.addParameter(VistaQuery.REFERENCE, reference);
            return await this.Session.sQuery(vq);
        }

        #endregion

        #region Patch / Version / Availability

        /// <summary>
        /// Check whether a specific patch is installed on the server.
        /// Returns "1" if present.
        /// RPC: ORWU PATCH
        /// </summary>
        [HttpGet, Route("api/utility/haspatch")]
        public async Task<ActionResult<string>> ServerHasPatch([FromQuery] string patchId)
        {
            var vq = new VistaQuery("ORWU PATCH");
            vq.addParameter(VistaQuery.LITERAL, patchId);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Check whether the site spans the international date line.
        /// Returns "1" if true.
        /// RPC: ORWU OVERDL
        /// </summary>
        [HttpGet, Route("api/utility/spansintldateline")]
        public async Task<ActionResult<string>> SiteSpansIntlDateLine()
        {
            var vq = new VistaQuery("ORWU OVERDL");
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Check whether one or more RPCs are available on the server.
        /// Supply items as "RPCName^Version" strings.
        /// RPC: XWB ARE RPCS AVAILABLE
        /// </summary>
        [HttpPost, Route("api/utility/rpcsavailable")]
        public async Task<ActionResult<List<string>>> AreRpcsAvailable([FromBody] List<string> rpcs)
        {
            var vq = new VistaQuery("XWB ARE RPCS AVAILABLE");
            vq.addParameter(VistaQuery.LITERAL, "L");
            var dhl = new DictionaryHashList();
            for (int i = 0; i < rpcs.Count; i++)
                dhl.Add(i.ToString(), rpcs[i]);
            vq.addParameter(VistaQuery.LIST, dhl);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get the server version for an option, given the client version.
        /// RPC: ORWU VERSRV
        /// </summary>
        [HttpGet, Route("api/utility/serverversion")]
        public async Task<ActionResult<string>> ServerVersion(
            [FromQuery] string option,
            [FromQuery] string verClient)
        {
            var vq = new VistaQuery("ORWU VERSRV");
            vq.addParameter(VistaQuery.LITERAL, option);
            vq.addParameter(VistaQuery.LITERAL, verClient);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Get the version of a VistA package by namespace.
        /// RPC: ORWU VERSION
        /// </summary>
        [HttpGet, Route("api/utility/packageversion")]
        public async Task<ActionResult<string>> PackageVersion([FromQuery] string ns)
        {
            var vq = new VistaQuery("ORWU VERSION");
            vq.addParameter(VistaQuery.LITERAL, ns);
            return await this.Session.sQuery(vq);
        }

        #endregion

        #region Font & Layout Persistence

        /// <summary>
        /// Load the user's saved font size.
        /// RPC: ORWCH LDFONT
        /// </summary>
        [HttpGet, Route("api/utility/fontsize")]
        public async Task<ActionResult<string>> LoadFontSize()
        {
            var vq = new VistaQuery("ORWCH LDFONT");
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Save the user's font size.
        /// RPC: ORWCH SAVFONT
        /// </summary>
        [HttpPost, Route("api/utility/fontsize")]
        public async Task<ActionResult<string>> SaveFontSize([FromQuery] int fontSize)
        {
            var vq = new VistaQuery("ORWCH SAVFONT");
            vq.addParameter(VistaQuery.LITERAL, fontSize.ToString());
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Load all saved window bounds, widths, and column settings.
        /// RPC: ORWCH LOADALL
        /// </summary>
        [HttpGet, Route("api/utility/sizesall")]
        public async Task<ActionResult<List<string>>> LoadAllSizes()
        {
            var vq = new VistaQuery("ORWCH LOADALL");
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Save all window bounds, widths, and column settings.
        /// RPC: ORWCH SAVEALL
        /// </summary>
        [HttpPost, Route("api/utility/sizesall")]
        public async Task<ActionResult<string>> SaveAllSizes([FromBody] List<string> items)
        {
            var vq = new VistaQuery("ORWCH SAVEALL");
            var dhl = new DictionaryHashList();
            for (int i = 0; i < items.Count; i++)
                dhl.Add(i.ToString(), items[i]);
            vq.addParameter(VistaQuery.LIST, dhl);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Load a single named size/position value.
        /// RPC: ORWCH LOADSIZ
        /// </summary>
        [HttpGet, Route("api/utility/size")]
        public async Task<ActionResult<string>> LoadSize([FromQuery] string name)
        {
            var vq = new VistaQuery("ORWCH LOADSIZ");
            vq.addParameter(VistaQuery.LITERAL, name);
            return await this.Session.sQuery(vq);
        }

        #endregion

        #region Share Node

        /// <summary>
        /// Set the share node so other apps can see the currently selected patient.
        /// RPC: ORWPT SHARE
        /// </summary>
        [HttpPost, Route("api/utility/sharenode")]
        public async Task<ActionResult<string>> SetShareNode(
            [FromQuery] string ip,
            [FromQuery] string handle,
            [FromQuery] string dfn)
        {
            var vq = new VistaQuery("ORWPT SHARE");
            vq.addParameter(VistaQuery.LITERAL, ip);
            vq.addParameter(VistaQuery.LITERAL, handle);
            vq.addParameter(VistaQuery.LITERAL, dfn);
            return await this.Session.sQuery(vq);
        }

        #endregion

        #region DLL Version Check

        /// <summary>
        /// Check the server-registered version of a DLL.
        /// Returns "-1^message" on mismatch or the OK status.
        /// RPC: ORUTL4 DLL
        /// </summary>
        [HttpGet, Route("api/utility/dllversioncheck")]
        public async Task<ActionResult<string>> DllVersionCheck(
            [FromQuery] string dllName,
            [FromQuery] string dllVersion)
        {
            var vq = new VistaQuery("ORUTL4 DLL");
            vq.addParameter(VistaQuery.LITERAL, dllName);
            vq.addParameter(VistaQuery.LITERAL, dllVersion);
            return await this.Session.sQuery(vq);
        }

        #endregion

        #region COM Object Hooks (rEventHooks)

        /// <summary>
        /// Get COM GUIDs triggered on patient change.
        /// RPC: ORWCOM PTOBJ
        /// </summary>
        [HttpGet, Route("api/utility/com/patientchange")]
        public async Task<ActionResult<string>> GetPatientChangeGUIDs()
        {
            var vq = new VistaQuery("ORWCOM PTOBJ");
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Get COM GUIDs triggered on order accept for a display group.
        /// RPC: ORWCOM ORDEROBJ
        /// </summary>
        [HttpGet, Route("api/utility/com/orderaccept")]
        public async Task<ActionResult<string>> GetOrderAcceptGUIDs([FromQuery] int displayGroup)
        {
            var vq = new VistaQuery("ORWCOM ORDEROBJ");
            vq.addParameter(VistaQuery.LITERAL, displayGroup.ToString());
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Get all active COM objects.
        /// RPC: ORWCOM GETOBJS
        /// </summary>
        [HttpGet, Route("api/utility/com/objects")]
        public async Task<ActionResult<List<string>>> GetAllActiveCOMObjects()
        {
            var vq = new VistaQuery("ORWCOM GETOBJS");
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get details for a specific COM object by IEN.
        /// RPC: ORWCOM DETAILS
        /// </summary>
        [HttpGet, Route("api/utility/com/details")]
        public async Task<ActionResult<string>> GetCOMObjectDetails([FromQuery] int ien)
        {
            var vq = new VistaQuery("ORWCOM DETAILS");
            vq.addParameter(VistaQuery.LITERAL, ien.ToString());
            return await this.Session.sQuery(vq);
        }

        #endregion

        #region Real-Time Consult Info (rODRTC)

        /// <summary>
        /// Get additional information for a real-time consult order dialog.
        /// RPC: ORWDSD1 GETINFO
        /// </summary>
        [HttpGet, Route("api/utility/rtcinfo")]
        public async Task<ActionResult<List<string>>> GetAdditionalInfo(
            [FromQuery] string locIen,
            [FromQuery] string what)
        {
            var vq = new VistaQuery("ORWDSD1 GETINFO");
            vq.addParameter(VistaQuery.LITERAL, locIen);
            vq.addParameter(VistaQuery.LITERAL, what);
            return await this.Session.tQuery(vq);
        }

        #endregion
    }
}
