using CarePlatform.Data.VistA;
using CarePlatform.Models.Patient;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarePlatform.Data.CPRS
{
    /// <summary>
    /// Patient selection, identification, demographics, and lookup.
    /// Migrated from cprs/rCore.pas — patient-specific RPCs (ORWPT *, DG *, ORVAA, ORWMHV, VAFCTFU).
    /// </summary>
    public class PatientController : BaseController
    {
        public PatientController() : base() { }

        #region Patient Selection & Demographics

        /// <summary>
        /// Select a patient — updates DISV, calls Pt Select actions, returns key fields.
        /// RPC: ORWPT SELECT
        /// Pieces: NAME^SEX^DOB^SSN^LOCIEN^LOCNAME^ROOMBED^CWAD^SENSITIVE^ADMITTIME^CONVERTED^SVCONN^SC%^ICN^Age^TreatSpec^SpecialtySvc
        /// </summary>
        [HttpGet, Route("api/patient/select")]
        public async Task<ActionResult<PatientDemographics>> Select([FromQuery] string dfn)
        {
            var vq = new VistaQuery("ORWPT SELECT");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            var result = await this.Session.sQuery(vq);
            return PatientDemographics.Parse(dfn, result);
        }

        /// <summary>
        /// Get primary care info for a patient.
        /// RPC: ORWPT1 PRCARE
        /// Pieces: PrimaryTeam^PrimaryProvider^Attending^Associate^MHTC^InProvider
        /// </summary>
        [HttpGet, Route("api/patient/primarycare")]
        public async Task<ActionResult<PatientPrimaryCare>> PrimaryCare([FromQuery] string dfn)
        {
            var vq = new VistaQuery("ORWPT1 PRCARE");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            var result = await this.Session.sQuery(vq);
            return PatientPrimaryCare.Parse(result);
        }

        /// <summary>
        /// Get patient identifiers displayed upon selection.
        /// RPC: ORWPT ID INFO
        /// Pieces: SSN^DOB^SEX^VET^SC%^WARD^RM-BED^NAME
        /// </summary>
        [HttpGet, Route("api/patient/idinfo")]
        public async Task<ActionResult<PatientIdInfo>> IdInfo([FromQuery] string dfn)
        {
            var vq = new VistaQuery("ORWPT ID INFO");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            var result = await this.Session.sQuery(vq);
            return PatientIdInfo.Parse(result);
        }

        /// <summary>
        /// Get date of death for a patient (returns FM date or empty).
        /// RPC: ORWPT DIEDON
        /// </summary>
        [HttpGet, Route("api/patient/dateofdeath")]
        public async Task<ActionResult<string>> DateOfDeath([FromQuery] string dfn)
        {
            var vq = new VistaQuery("ORWPT DIEDON");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Get current inpatient location for a patient.
        /// RPC: ORWPT INPLOC
        /// Pieces: LocationIEN^LocationName^WardService
        /// </summary>
        [HttpGet, Route("api/patient/inpatientlocation")]
        public async Task<ActionResult<string>> InpatientLocation([FromQuery] string dfn)
        {
            var vq = new VistaQuery("ORWPT INPLOC");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Check if patient record is restricted.
        /// RPC: ORWPT SELCHK
        /// Returns: piece 1 = '1' if restricted
        /// </summary>
        [HttpGet, Route("api/patient/restrictedcheck")]
        public async Task<ActionResult<string>> RestrictedCheck([FromQuery] string dfn)
        {
            var vq = new VistaQuery("ORWPT SELCHK");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Get encounter text (location name, abbreviation, room/bed, provider name).
        /// RPC: ORWPT ENCTITL
        /// Pieces: LocationName^LocationAbbr^RoomBed^ProviderName
        /// </summary>
        [HttpGet, Route("api/patient/encountertext")]
        public async Task<ActionResult<string>> EncounterText(
            [FromQuery] string dfn,
            [FromQuery] int location = 0,
            [FromQuery] long provider = 0)
        {
            var vq = new VistaQuery("ORWPT ENCTITL");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            vq.addParameter(VistaQuery.LITERAL, location.ToString());
            vq.addParameter(VistaQuery.LITERAL, provider.ToString());
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Check for legacy data for a patient.
        /// RPC: ORWPT LEGACY
        /// Returns: line 0 = '1' if legacy data exists, subsequent lines = message
        /// </summary>
        [HttpGet, Route("api/patient/legacy")]
        public async Task<ActionResult<List<string>>> Legacy([FromQuery] string dfn)
        {
            var vq = new VistaQuery("ORWPT LEGACY");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get COVID/other information panel data for a patient.
        /// RPC: ORWPT2 COVID
        /// </summary>
        [HttpGet, Route("api/patient/otherinformation")]
        public async Task<ActionResult<string>> OtherInformation([FromQuery] string dfn)
        {
            var vq = new VistaQuery("ORWPT2 COVID");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Get other information panel detail text.
        /// RPC: ORWOTHER DETAIL
        /// </summary>
        [HttpGet, Route("api/patient/otherinformationdetail")]
        public async Task<ActionResult<List<string>>> OtherInformationDetail(
            [FromQuery] string dfn,
            [FromQuery] string valueType)
        {
            var vq = new VistaQuery("ORWOTHER DETAIL");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            vq.addParameter(VistaQuery.LITERAL, valueType);
            return await this.Session.tQuery(vq);
        }

        #endregion

        #region Patient Lookup / Search

        /// <summary>
        /// Get the last-selected patient(s).
        /// RPC: ORWPT TOP
        /// </summary>
        [HttpGet, Route("api/patient/top")]
        public async Task<ActionResult<List<string>>> Top()
        {
            var vq = new VistaQuery("ORWPT TOP");
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get a subset of all patients (for long list box scrolling).
        /// RPC: ORWPT LIST ALL
        /// </summary>
        [HttpGet, Route("api/patient/listall")]
        public async Task<ActionResult<List<string>>> ListAll(
            [FromQuery] string startFrom,
            [FromQuery] int direction)
        {
            var vq = new VistaQuery("ORWPT LIST ALL");
            vq.addParameter(VistaQuery.LITERAL, startFrom);
            vq.addParameter(VistaQuery.LITERAL, direction.ToString());
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Search patients by last name initial + last 4 SSN digits (Last5).
        /// RPC: ORWPT LAST5
        /// Returns: DFN^PatientName^DOB^SSN per line
        /// </summary>
        [HttpGet, Route("api/patient/searchlast5")]
        public async Task<ActionResult<List<PatientSearchResult>>> SearchLast5([FromQuery] string last5)
        {
            var vq = new VistaQuery("ORWPT LAST5");
            vq.addParameter(VistaQuery.LITERAL, last5.ToUpper());
            var results = await this.Session.tQuery(vq);
            return PatientSearchResult.ParseList(results);
        }

        /// <summary>
        /// Search RPL patients by Last5.
        /// RPC: ORWPT LAST5 RPL
        /// </summary>
        [HttpGet, Route("api/patient/searchlast5rpl")]
        public async Task<ActionResult<List<string>>> SearchLast5Rpl([FromQuery] string last5)
        {
            var vq = new VistaQuery("ORWPT LAST5 RPL");
            vq.addParameter(VistaQuery.LITERAL, last5.ToUpper());
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Search patients by full SSN.
        /// RPC: ORWPT FULLSSN
        /// Returns: DFN^PatientName^DOB^SSN per line
        /// </summary>
        [HttpGet, Route("api/patient/searchfullssn")]
        public async Task<ActionResult<List<PatientSearchResult>>> SearchFullSsn([FromQuery] string fullSsn)
        {
            // Strip dashes from SSN
            string ssn = fullSsn.Replace("-", "").ToUpper();
            var vq = new VistaQuery("ORWPT FULLSSN");
            vq.addParameter(VistaQuery.LITERAL, ssn);
            var results = await this.Session.tQuery(vq);
            return PatientSearchResult.ParseList(results);
        }

        /// <summary>
        /// Search RPL patients by full SSN.
        /// RPC: ORWPT FULLSSN RPL
        /// </summary>
        [HttpGet, Route("api/patient/searchfullssnrpl")]
        public async Task<ActionResult<List<string>>> SearchFullSsnRpl([FromQuery] string fullSsn)
        {
            string ssn = fullSsn.Replace("-", "").ToUpper();
            var vq = new VistaQuery("ORWPT FULLSSN RPL");
            vq.addParameter(VistaQuery.LITERAL, ssn);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// List patients by ward.
        /// RPC: ORWPT BYWARD
        /// Returns: DFN^PatientName^RoomBed per line
        /// </summary>
        [HttpGet, Route("api/patient/byward")]
        public async Task<ActionResult<List<PatientSearchResult>>> ByWard([FromQuery] int wardIen)
        {
            var vq = new VistaQuery("ORWPT BYWARD");
            vq.addParameter(VistaQuery.LITERAL, wardIen.ToString());
            var results = await this.Session.tQuery(vq);
            return PatientSearchResult.ParseList(results);
        }

        #endregion

        #region Admissions & Appointments

        /// <summary>
        /// List all admissions for a patient.
        /// RPC: ORWPT ADMITLST
        /// Returns: MovementTime^LocIEN^LocName^Type per line
        /// </summary>
        [HttpGet, Route("api/patient/admissions")]
        public async Task<ActionResult<List<string>>> Admissions([FromQuery] string dfn)
        {
            var vq = new VistaQuery("ORWPT ADMITLST");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// List visits/appointments for a patient.
        /// RPC: ORWCV VST (replaces ORWPT APPTLST)
        /// </summary>
        [HttpGet, Route("api/patient/visits")]
        public async Task<ActionResult<List<string>>> Visits(
            [FromQuery] string dfn,
            [FromQuery] string from = "0",
            [FromQuery] string thru = "0",
            [FromQuery] int skipAdmits = 0)
        {
            var vq = new VistaQuery("ORWCV VST");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            vq.addParameter(VistaQuery.LITERAL, from);
            vq.addParameter(VistaQuery.LITERAL, thru);
            vq.addParameter(VistaQuery.LITERAL, skipAdmits.ToString());
            return await this.Session.tQuery(vq);
        }

        #endregion

        #region Patient List Defaults

        /// <summary>
        /// Get the default patient list source character (C=clinic, W=ward, etc.).
        /// RPC: ORWPT DFLTSRC
        /// </summary>
        [HttpGet, Route("api/patient/defaultlistsource")]
        public async Task<ActionResult<string>> DefaultListSource()
        {
            var vq = new VistaQuery("ORWPT DFLTSRC");
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Save the default patient list setting.
        /// RPC: ORWPT SAVDFLT
        /// </summary>
        [HttpPost, Route("api/patient/savedefault")]
        public async Task<ActionResult<string>> SaveDefault([FromQuery] string listDefault)
        {
            var vq = new VistaQuery("ORWPT SAVDFLT");
            vq.addParameter(VistaQuery.LITERAL, listDefault);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Get clinic date range options for patient lookup.
        /// RPC: ORWPT CLINRNG
        /// </summary>
        [HttpGet, Route("api/patient/clinicdateranges")]
        public async Task<ActionResult<List<string>>> ClinicDateRanges()
        {
            var vq = new VistaQuery("ORWPT CLINRNG");
            return await this.Session.tQuery(vq);
        }

        #endregion

        #region Sensitive Record Access (DG RPCs)

        /// <summary>
        /// Check sensitive record access for a patient.
        /// RPC: DG SENSITIVE RECORD ACCESS
        /// Returns: line 0 = access status code, subsequent lines = message text
        /// Access status: 0=no action, 1=display warning, 2=display & log, 3=no access, -1=error
        /// </summary>
        [HttpGet, Route("api/patient/sensitiverecordaccess")]
        public async Task<ActionResult<SensitiveRecordResult>> SensitiveRecordAccess([FromQuery] string dfn)
        {
            var vq = new VistaQuery("DG SENSITIVE RECORD ACCESS");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            var results = await this.Session.tQuery(vq);
            return SensitiveRecordResult.Parse(results);
        }

        /// <summary>
        /// Log that a sensitive record was accessed (sends bulletin).
        /// RPC: DG SENSITIVE RECORD BULLETIN
        /// Returns: '1' if successful
        /// </summary>
        [HttpPost, Route("api/patient/logsensitiveaccess")]
        public async Task<ActionResult<string>> LogSensitiveAccess([FromQuery] string dfn)
        {
            var vq = new VistaQuery("DG SENSITIVE RECORD BULLETIN");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Check if means test is required for a patient.
        /// RPC: DG CHK PAT/DIV MEANS TEST
        /// Returns: line 0 = '1' if required, subsequent lines = message
        /// </summary>
        [HttpGet, Route("api/patient/meanstest")]
        public async Task<ActionResult<List<string>>> MeansTestRequired([FromQuery] string dfn)
        {
            var vq = new VistaQuery("DG CHK PAT/DIV MEANS TEST");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Check for similar records (BS5 cross-reference).
        /// RPC: DG CHK BS5 XREF Y/N
        /// Returns: line 0 = '1' if similar records found, subsequent lines = details
        /// </summary>
        [HttpGet, Route("api/patient/similarrecords")]
        public async Task<ActionResult<List<string>>> SimilarRecords([FromQuery] string dfn)
        {
            var vq = new VistaQuery("DG CHK BS5 XREF Y/N");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Check for duplicate patient records via BS5 cross-reference.
        /// RPC: DG CHK BS5 XREF ARRAY
        /// Returns: line 0 = '1' if duplicates found, subsequent lines = Flag^DFN^Name^FMDate^SSN
        /// Maps to fPtSel.pas DupLastSSN — called after patient selection, before finalizing.
        /// </summary>
        [HttpGet, Route("api/patient/duplicaterecords")]
        public async Task<ActionResult<List<string>>> DuplicateRecords([FromQuery] string dfn)
        {
            var vq = new VistaQuery("DG CHK BS5 XREF ARRAY");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            return await this.Session.tQuery(vq);
        }

        #endregion

        #region Cross-Reference & External Data

        /// <summary>
        /// Convert an ICN to a DFN.
        /// RPC: VAFCTFU CONVERT ICN TO DFN
        /// </summary>
        [HttpGet, Route("api/patient/icntodfn")]
        public async Task<ActionResult<string>> IcnToDfn([FromQuery] string icn)
        {
            var vq = new VistaQuery("VAFCTFU CONVERT ICN TO DFN");
            vq.addParameter(VistaQuery.LITERAL, icn);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Get VAA (Veterans Affairs Applications) data for a patient.
        /// RPC: ORVAA VAA
        /// </summary>
        [HttpGet, Route("api/patient/vaadata")]
        public async Task<ActionResult<List<string>>> VaaData([FromQuery] string dfn)
        {
            var vq = new VistaQuery("ORVAA VAA");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get MHV (My HealtheVet) data for a patient.
        /// RPC: ORWMHV MHV
        /// </summary>
        [HttpGet, Route("api/patient/mhvdata")]
        public async Task<ActionResult<List<string>>> MhvData([FromQuery] string dfn)
        {
            var vq = new VistaQuery("ORWMHV MHV");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            return await this.Session.tQuery(vq);
        }

        #endregion

        #region Cover Sheet — Appointments & Flags

        /// <summary>
        /// List upcoming/recent appointments for a patient.
        /// Called by CPRS Web CoverSheet for the \"Appointments\" widget.
        /// RPC: ORWCV VST (same as api/patient/visits, exposed at the URL the CoverSheet calls)
        /// </summary>
        [HttpGet, Route("api/appointment/list")]
        public async Task<ActionResult<List<string>>> AppointmentList(
            [FromQuery] string dfn,
            [FromQuery] string from = "0",
            [FromQuery] string thru = "0")
        {
            var vq = new VistaQuery("ORWCV VST");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            vq.addParameter(VistaQuery.LITERAL, from);
            vq.addParameter(VistaQuery.LITERAL, thru);
            vq.addParameter(VistaQuery.LITERAL, "0");
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Check whether a patient has record flags (PRF).
        /// Called by CPRS Web CoverSheet for the patient-flags banner.
        /// RPC: ORPRF HASFLG
        /// </summary>
        [HttpGet, Route("api/patient/flags")]
        public async Task<ActionResult<List<string>>> Flags([FromQuery] string dfn)
        {
            var vq = new VistaQuery("ORPRF HASFLG");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            return await this.Session.tQuery(vq);
        }

        #endregion

        #region Expanded Demographics

        /// <summary>
        /// Get expanded patient demographics (NOK, emergency contact, insurance, etc.).
        /// RPC: ORWPT PTINQ
        /// </summary>
        [HttpGet, Route("api/patient/expandeddemographics")]
        public async Task<ActionResult<List<string>>> ExpandedDemographics([FromQuery] string dfn)
        {
            var vq = new VistaQuery("ORWPT PTINQ");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            return await this.Session.tQuery(vq);
        }

        #endregion
    }
}
