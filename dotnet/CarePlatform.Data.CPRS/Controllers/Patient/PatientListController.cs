// Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
using CarePlatform.Data.VistA;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarePlatform.Data.CPRS
{
    /// <summary>
    /// Patient list sources and list queries — specialties, teams, wards, clinics, providers.
    /// Migrated from cprs/rCore.pas — ORQPT * RPCs.
    /// </summary>
    public class PatientListController : BaseController
    {
        public PatientListController() : base() { }

        #region List Sources

        /// <summary>
        /// Get the default patient list source (name and type).
        /// RPC: ORQPT DEFAULT LIST SOURCE
        /// Returns: Ptr^SourceName^SourceType
        /// </summary>
        [HttpGet, Route("api/patientlist/defaultsource")]
        public async Task<ActionResult<string>> DefaultSource()
        {
            var vq = new VistaQuery("ORQPT DEFAULT LIST SOURCE");
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Get the default clinic date range for patient list.
        /// RPC: ORQPT DEFAULT CLINIC DATE RANG
        /// </summary>
        [HttpGet, Route("api/patientlist/defaultclinicdaterange")]
        public async Task<ActionResult<string>> DefaultClinicDateRange()
        {
            var vq = new VistaQuery("ORQPT DEFAULT CLINIC DATE RANG");
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Get the default patient list sort order.
        /// RPC: ORQPT DEFAULT LIST SORT
        /// Returns: sort character (e.g. 'A' for alpha)
        /// </summary>
        [HttpGet, Route("api/patientlist/defaultsort")]
        public async Task<ActionResult<string>> DefaultSort()
        {
            var vq = new VistaQuery("ORQPT DEFAULT LIST SORT");
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// List all treating specialties.
        /// RPC: ORQPT SPECIALTIES
        /// Returns: IEN^SpecialtyName per line
        /// </summary>
        [HttpGet, Route("api/patientlist/specialties")]
        public async Task<ActionResult<List<string>>> Specialties()
        {
            var vq = new VistaQuery("ORQPT SPECIALTIES");
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// List all patient care teams.
        /// RPC: ORQPT TEAMS
        /// Returns: IEN^TeamName per line
        /// </summary>
        [HttpGet, Route("api/patientlist/teams")]
        public async Task<ActionResult<List<string>>> Teams()
        {
            var vq = new VistaQuery("ORQPT TEAMS");
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// List all PCMM teams and providers.
        /// RPC: ORQPT PTEAMPR
        /// Returns: IEN^TeamName per line
        /// </summary>
        [HttpGet, Route("api/patientlist/pcmmteams")]
        public async Task<ActionResult<List<string>>> PcmmTeams()
        {
            var vq = new VistaQuery("ORQPT PTEAMPR");
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// List all active inpatient wards.
        /// RPC: ORQPT WARDS
        /// Returns: IEN^WardName per line
        /// </summary>
        [HttpGet, Route("api/patientlist/wards")]
        [HttpGet, Route("api/patient/wards")]
        public async Task<ActionResult<List<string>>> Wards()
        {
            var vq = new VistaQuery("ORQPT WARDS");
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// List all active clinics.
        /// RPC: ORQPT CLINICS
        /// Returns: IEN^ClinicName per line
        /// </summary>
        [HttpGet, Route("api/patientlist/clinics")]
        [HttpGet, Route("api/patient/clinics")]
        public async Task<ActionResult<List<string>>> Clinics()
        {
            var vq = new VistaQuery("ORQPT CLINICS");
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get the user's default patient list.
        /// RPC: ORQPT DEFAULT PATIENT LIST
        /// Returns: DFN^PatientName per line
        /// </summary>
        [HttpGet, Route("api/patientlist/defaultlist")]
        public async Task<ActionResult<List<string>>> DefaultList()
        {
            var vq = new VistaQuery("ORQPT DEFAULT PATIENT LIST");
            return await this.Session.tQuery(vq);
        }

        #endregion

        #region Patient Lists by Source

        /// <summary>
        /// List patients by provider.
        /// RPC: ORQPT PROVIDER PATIENTS
        /// Returns: DFN^PatientName per line
        /// </summary>
        [HttpGet, Route("api/patientlist/byprovider")]
        public async Task<ActionResult<List<string>>> ByProvider([FromQuery] long providerIen)
        {
            var vq = new VistaQuery("ORQPT PROVIDER PATIENTS");
            vq.addParameter(VistaQuery.LITERAL, providerIen.ToString());
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// List patients by team.
        /// RPC: ORQPT TEAM PATIENTS
        /// Returns: DFN^PatientName per line
        /// </summary>
        [HttpGet, Route("api/patientlist/byteam")]
        public async Task<ActionResult<List<string>>> ByTeam([FromQuery] int teamIen)
        {
            var vq = new VistaQuery("ORQPT TEAM PATIENTS");
            vq.addParameter(VistaQuery.LITERAL, teamIen.ToString());
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// List patients by PCMM team.
        /// RPC: ORQPT PTEAM PATIENTS
        /// Returns: DFN^PatientName per line
        /// </summary>
        [HttpGet, Route("api/patientlist/bypcmmteam")]
        public async Task<ActionResult<List<string>>> ByPcmmTeam([FromQuery] int teamIen)
        {
            var vq = new VistaQuery("ORQPT PTEAM PATIENTS");
            vq.addParameter(VistaQuery.LITERAL, teamIen.ToString());
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// List patients by treating specialty.
        /// RPC: ORQPT SPECIALTY PATIENTS
        /// Returns: DFN^PatientName per line
        /// </summary>
        [HttpGet, Route("api/patientlist/byspecialty")]
        public async Task<ActionResult<List<string>>> BySpecialty([FromQuery] int specialtyIen)
        {
            var vq = new VistaQuery("ORQPT SPECIALTY PATIENTS");
            vq.addParameter(VistaQuery.LITERAL, specialtyIen.ToString());
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// List patients by clinic with date range.
        /// RPC: ORQPT CLINIC PATIENTS
        /// Returns: DFN^PatientName^Location^ApptTime per line
        /// </summary>
        [HttpGet, Route("api/patientlist/byclinic")]
        public async Task<ActionResult<List<string>>> ByClinic(
            [FromQuery] int clinicIen,
            [FromQuery] string firstDate = "",
            [FromQuery] string lastDate = "")
        {
            var vq = new VistaQuery("ORQPT CLINIC PATIENTS");
            vq.addParameter(VistaQuery.LITERAL, clinicIen.ToString());
            vq.addParameter(VistaQuery.LITERAL, firstDate);
            vq.addParameter(VistaQuery.LITERAL, lastDate);
            return await this.Session.tQuery(vq);
        }

        #endregion

        #region Restricted Patient List (RPL)

        /// <summary>
        /// Create a Restricted Patient List based on team list info.
        /// RPC: ORQPT MAKE RPL
        /// Returns: RPL job number
        /// </summary>
        [HttpPost, Route("api/patientlist/makerpl")]
        public async Task<ActionResult<string>> MakeRpl([FromQuery] string rplList)
        {
            var vq = new VistaQuery("ORQPT MAKE RPL");
            vq.addParameter(VistaQuery.LITERAL, rplList);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Read patients from RPL (for long list box scrolling).
        /// RPC: ORQPT READ RPL
        /// </summary>
        [HttpGet, Route("api/patientlist/readrpl")]
        public async Task<ActionResult<List<string>>> ReadRpl(
            [FromQuery] string rplJobNumber,
            [FromQuery] string startFrom,
            [FromQuery] int direction)
        {
            var vq = new VistaQuery("ORQPT READ RPL");
            vq.addParameter(VistaQuery.LITERAL, rplJobNumber);
            vq.addParameter(VistaQuery.LITERAL, startFrom);
            vq.addParameter(VistaQuery.LITERAL, direction.ToString());
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Delete/kill a Restricted Patient List.
        /// RPC: ORQPT KILL RPL
        /// </summary>
        [HttpPost, Route("api/patientlist/killrpl")]
        public async Task<ActionResult<string>> KillRpl([FromQuery] string rplJobNumber)
        {
            var vq = new VistaQuery("ORQPT KILL RPL");
            vq.addParameter(VistaQuery.LITERAL, rplJobNumber);
            return await this.Session.sQuery(vq);
        }

        #endregion
    }
}
