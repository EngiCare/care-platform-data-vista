using CarePlatform.Data.VistA;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarePlatform.Data.CPRS
{
    /// <summary>
    /// Cover sheet data — allergies, postings, demographics, problem/allergy detail.
    /// Migrated from cprs/rCover.pas — ORQQAL, ORQQPP, ORWPT PTINQ, WVRPCOR, TIU GET RECORD TEXT RPCs.
    /// </summary>
    public class CoverSheetController : BaseController
    {
        /// <summary>
        /// Per-session cache of allowed RPC names (MainRPC + DetailRPC) from ORWCV1 COVERSHEET LIST.
        /// Prevents arbitrary RPC execution through the generic paneldata/paneldetail endpoints.
        /// </summary>
        private static readonly ConcurrentDictionary<string, HashSet<string>> _allowedRpcs = new();

        public CoverSheetController() : base() { }

        /// <summary>
        /// List all allergies for a patient (cover sheet display).
        /// RPC: ORQQAL LIST
        /// Returns: allergy list strings
        /// </summary>
        [HttpGet, Route("api/coversheet/allergies")]
        public async Task<ActionResult<List<string>>> Allergies([FromQuery] string dfn)
        {
            var vq = new VistaQuery("ORQQAL LIST");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// List all allergy/adverse reaction report (full text).
        /// RPC: ORQQAL LIST REPORT
        /// </summary>
        [HttpGet, Route("api/coversheet/allergyreport")]
        public async Task<ActionResult<List<string>>> AllergyReport([FromQuery] string dfn)
        {
            var vq = new VistaQuery("ORQQAL LIST REPORT");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get detailed allergy information.
        /// RPC: ORQQAL DETAIL
        /// </summary>
        [HttpGet, Route("api/coversheet/allergydetail")]
        public async Task<ActionResult<List<string>>> AllergyDetail(
            [FromQuery] string dfn,
            [FromQuery] int ien)
        {
            var vq = new VistaQuery("ORQQAL DETAIL");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            vq.addParameter(VistaQuery.LITERAL, ien.ToString());
            vq.addParameter(VistaQuery.LITERAL, "");
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// List clinical warnings/postings (CWAD) for a patient.
        /// RPC: ORQQPP LIST
        /// Returns: posting list strings
        /// </summary>
        [HttpGet, Route("api/coversheet/postings")]
        public async Task<ActionResult<List<string>>> Postings([FromQuery] string dfn)
        {
            var vq = new VistaQuery("ORQQPP LIST");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get posting detail (crisis notes, warnings, etc.).
        /// RPC: WVRPCOR POSTREP
        /// Pass the flag character (e.g., 'C' for crisis, 'W' for warning, 'A' for allergy, 'D' for directive).
        /// </summary>
        [HttpGet, Route("api/coversheet/postingdetail")]
        public async Task<ActionResult<List<string>>> PostingDetail(
            [FromQuery] string dfn,
            [FromQuery] string flag)
        {
            var vq = new VistaQuery("WVRPCOR POSTREP");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            vq.addParameter(VistaQuery.LITERAL, flag);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get TIU record text (for posting details that are TIU documents).
        /// RPC: TIU GET RECORD TEXT
        /// </summary>
        [HttpGet, Route("api/coversheet/recordtext")]
        public async Task<ActionResult<List<string>>> RecordText([FromQuery] string ien)
        {
            var vq = new VistaQuery("TIU GET RECORD TEXT");
            vq.addParameter(VistaQuery.LITERAL, ien);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Load patient demographics/inquiry.
        /// RPC: ORWPT PTINQ
        /// Returns: demographic text lines
        /// </summary>
        [HttpGet, Route("api/coversheet/demographics")]
        public async Task<ActionResult<List<string>>> Demographics([FromQuery] string dfn)
        {
            var vq = new VistaQuery("ORWPT PTINQ");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get problem detail for a specific problem (cover sheet drill-down).
        /// RPC: ORQQPL DETAIL
        /// </summary>
        [HttpGet, Route("api/coversheet/problemdetail")]
        public async Task<ActionResult<List<string>>> ProblemDetail(
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
        /// Get patient immunization history for cover sheet display.
        /// RPC: PXVIMM ADMIN HX
        /// Returns: IEN^Name^AdminDate^Series^Reaction^Facility per line.
        /// </summary>
        [HttpGet, Route("api/coversheet/immunizations")]
        public async Task<ActionResult<List<string>>> Immunizations([FromQuery] string dfn)
        {
            var vq = new VistaQuery("PXVIMM ADMIN HX");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            return await this.Session.tQuery(vq);
        }

        // ── Dynamic cover sheet configuration ────────────────────────

        /// <summary>
        /// Get cover sheet panel configuration from VistA.
        /// RPC: ORWCV1 COVERSHEET LIST (no parameters)
        /// Returns one caret-delimited config string per panel (pieces 1-19).
        /// Maps to Pascal OnInitCoverSheet in oCoverSheet.pas.
        /// </summary>
        [HttpGet, Route("api/coversheet/config")]
        public async Task<ActionResult<List<string>>> CoverSheetConfig()
        {
            var vq = new VistaQuery("ORWCV1 COVERSHEET LIST");
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Generic cover sheet panel data loader.
        /// Calls the specified MainRPC (as configured by ORWCV1 COVERSHEET LIST) with DFN and optional Param1.
        /// Maps to Pascal OnRefreshDisplay in mCoverSheetDisplayPanel_CPRS.pas.
        /// Security: Only RPCs present in the session's cover sheet config are allowed.
        /// </summary>
        [HttpGet, Route("api/coversheet/paneldata")]
        public async Task<ActionResult<List<string>>> PanelData(
            [FromQuery] string rpc, [FromQuery] string dfn, [FromQuery] string param1 = "")
        {
            if (string.IsNullOrWhiteSpace(rpc))
                return BadRequest("rpc parameter is required.");

            var allowed = await GetAllowedRpcsAsync();
            if (!allowed.Contains(rpc))
                return BadRequest($"RPC '{rpc}' is not in the cover sheet configuration.");

            var vq = new VistaQuery(rpc);
            vq.addParameter(VistaQuery.LITERAL, dfn);
            if (!string.IsNullOrEmpty(param1))
                vq.addParameter(VistaQuery.LITERAL, param1);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Generic cover sheet detail loader.
        /// Calls the specified DetailRPC (as configured by ORWCV1 COVERSHEET LIST) with DFN + IEN.
        /// Maps to Pascal OnGetDetail / DisplayItemDetail in mCoverSheetDisplayPanel_CPRS.pas.
        /// Security: Only RPCs present in the session's cover sheet config are allowed.
        /// </summary>
        [HttpGet, Route("api/coversheet/paneldetail")]
        public async Task<ActionResult<List<string>>> PanelDetail(
            [FromQuery] string rpc, [FromQuery] string dfn, [FromQuery] string ien)
        {
            if (string.IsNullOrWhiteSpace(rpc))
                return BadRequest("rpc parameter is required.");

            var allowed = await GetAllowedRpcsAsync();
            if (!allowed.Contains(rpc))
                return BadRequest($"RPC '{rpc}' is not in the cover sheet configuration.");

            var vq = new VistaQuery(rpc);
            vq.addParameter(VistaQuery.LITERAL, dfn);
            vq.addParameter(VistaQuery.LITERAL, ien);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Lazily builds and caches the set of allowed RPC names for this session
        /// by fetching ORWCV1 COVERSHEET LIST and extracting piece 6 (MainRPC) and piece 16 (DetailRPC).
        /// </summary>
        private async Task<HashSet<string>> GetAllowedRpcsAsync()
        {
            var sessionId = this.Session.GetHashCode().ToString();
            if (_allowedRpcs.TryGetValue(sessionId, out var cached))
                return cached;

            var vq = new VistaQuery("ORWCV1 COVERSHEET LIST");
            var configLines = await this.Session.tQuery(vq);

            var rpcs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var line in configLines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var pieces = line.Split('^');
                if (pieces.Length > 5 && !string.IsNullOrWhiteSpace(pieces[5]))
                    rpcs.Add(pieces[5]);   // Piece 6 = MainRPC (0-indexed: [5])
                if (pieces.Length > 15 && !string.IsNullOrWhiteSpace(pieces[15]))
                    rpcs.Add(pieces[15]);  // Piece 16 = DetailRPC (0-indexed: [15])
            }

            _allowedRpcs.TryAdd(sessionId, rpcs);
            return rpcs;
        }
    }
}
