// Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
using CarePlatform.Data.VistA;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarePlatform.Data.CPRS
{
    /// <summary>
    /// Encounter / PCE — visit types, diagnoses, procedures, immunizations,
    /// skin tests, patient education, health factors, exams, lexicon, modifiers,
    /// service-connected eligibility, GAF scores, mental health screens,
    /// PCE data load/save for notes.
    /// Migrated from cprs/Encounter/rPCE.pas — ORWPCE *, ORWPCE4 *, ORWLEX *,
    /// ORQQPX *, TIU GET DOCUMENT PARAMETERS, TIU GET DEFAULT PROVIDER,
    /// TIU IS USER A PROVIDER? RPCs.
    /// </summary>
    public class EncounterController : BaseController
    {
        public EncounterController() : base() { }

        #region Encounter List

        /// <summary>
        /// List encounters for a patient (alias for pce4note with defaults).
        /// RPC: ORWPCE PCE4NOTE
        /// </summary>
        [HttpGet, Route("api/encounter/list")]
        public async Task<ActionResult<List<string>>> EncounterList([FromQuery] string dfn = "")
        {
            var vq = new VistaQuery("ORWPCE PCE4NOTE");
            vq.addParameter(VistaQuery.LITERAL, "0");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            vq.addParameter(VistaQuery.LITERAL, "");
            return await this.Session.tQuery(vq);
        }

        #endregion

        #region Encounter Form & Visit Types

        /// <summary>
        /// Load the encounter form data for the given location and date
        /// (visit types, procedures, diagnoses, immunisations, skin tests, etc.).
        /// RPC: ORWPCE VISIT
        /// </summary>
        [HttpGet, Route("api/encounter/visit")]
        public async Task<ActionResult<List<string>>> Visit(
            [FromQuery] string dfn,
            [FromQuery] int location,
            [FromQuery] string visitDate,
            [FromQuery] string serviceCategory = "")
        {
            var vq = new VistaQuery("ORWPCE VISIT");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            vq.addParameter(VistaQuery.LITERAL, location.ToString());
            vq.addParameter(VistaQuery.LITERAL, visitDate);
            if (!string.IsNullOrEmpty(serviceCategory))
                vq.addParameter(VistaQuery.LITERAL, serviceCategory);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Determine whether to auto-select visit type for a location.
        /// RPC: ORWPCE AUTO VISIT TYPE SELECT
        /// </summary>
        [HttpGet, Route("api/encounter/autovisittypeselect")]
        public async Task<ActionResult<string>> AutoVisitTypeSelect([FromQuery] int location)
        {
            var vq = new VistaQuery("ORWPCE AUTO VISIT TYPE SELECT");
            vq.addParameter(VistaQuery.LITERAL, location.ToString());
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Get service for the encounter.
        /// RPC: ORWPCE GETSVC
        /// </summary>
        [HttpGet, Route("api/encounter/getsvc")]
        public async Task<ActionResult<string>> GetSvc(
            [FromQuery] int location,
            [FromQuery] int provider)
        {
            var vq = new VistaQuery("ORWPCE GETSVC");
            vq.addParameter(VistaQuery.LITERAL, location.ToString());
            vq.addParameter(VistaQuery.LITERAL, provider.ToString());
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Check if the location is a clinic.
        /// RPC: ORWPCE ISCLINIC
        /// </summary>
        [HttpGet, Route("api/encounter/isclinic")]
        public async Task<ActionResult<string>> IsClinic([FromQuery] int location)
        {
            var vq = new VistaQuery("ORWPCE ISCLINIC");
            vq.addParameter(VistaQuery.LITERAL, location.ToString());
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Check if a location is a non-count clinic.
        /// RPC: ORWPCE1 NONCOUNT
        /// </summary>
        [HttpGet, Route("api/encounter/noncount")]
        public async Task<ActionResult<string>> NonCountClinic([FromQuery] int location)
        {
            var vq = new VistaQuery("ORWPCE1 NONCOUNT");
            vq.addParameter(VistaQuery.LITERAL, location.ToString());
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Check if an encounter already exists (has visit).
        /// RPC: ORWPCE HASVISIT
        /// </summary>
        [HttpGet, Route("api/encounter/hasvisit")]
        public async Task<ActionResult<string>> HasVisit(
            [FromQuery] int noteIen,
            [FromQuery] string dfn,
            [FromQuery] int location,
            [FromQuery] string visitDate)
        {
            var vq = new VistaQuery("ORWPCE HASVISIT");
            vq.addParameter(VistaQuery.LITERAL, noteIen.ToString());
            vq.addParameter(VistaQuery.LITERAL, dfn);
            vq.addParameter(VistaQuery.LITERAL, location.ToString());
            vq.addParameter(VistaQuery.LITERAL, visitDate);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Get the visit IEN for a note.
        /// RPC: ORWPCE GET VISIT
        /// </summary>
        [HttpGet, Route("api/encounter/getvisit")]
        public async Task<ActionResult<List<string>>> GetVisit(
            [FromQuery] int noteIen,
            [FromQuery] string dfn = "",
            [FromQuery] string visitStr = "")
        {
            var vq = new VistaQuery("ORWPCE GET VISIT");
            vq.addParameter(VistaQuery.LITERAL, noteIen.ToString());
            if (!string.IsNullOrEmpty(dfn))
            {
                vq.addParameter(VistaQuery.LITERAL, dfn);
                vq.addParameter(VistaQuery.LITERAL, visitStr);
            }
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Determine if a note is for a cancelled or no-show appointment.
        /// RPC: ORWPCE CXNOSHOW
        /// </summary>
        [HttpGet, Route("api/encounter/cxnoshow")]
        public async Task<ActionResult<string>> CancelOrNoShow([FromQuery] int noteIen)
        {
            var vq = new VistaQuery("ORWPCE CXNOSHOW");
            vq.addParameter(VistaQuery.LITERAL, noteIen.ToString());
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Check whether encounter entry can happen anytime.
        /// RPC: ORWPCE ANYTIME
        /// </summary>
        [HttpGet, Route("api/encounter/anytime")]
        public async Task<ActionResult<string>> Anytime()
        {
            var vq = new VistaQuery("ORWPCE ANYTIME");
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Check whether a location always requires checkout.
        /// RPC: ORWPCE ALWAYS CHECKOUT
        /// </summary>
        [HttpGet, Route("api/encounter/alwayscheckout")]
        public async Task<ActionResult<string>> AlwaysCheckout([FromQuery] int location)
        {
            var vq = new VistaQuery("ORWPCE ALWAYS CHECKOUT");
            vq.addParameter(VistaQuery.LITERAL, location.ToString());
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Ask PCE setting for user at a location.
        /// RPC: ORWPCE ASKPCE
        /// </summary>
        [HttpGet, Route("api/encounter/askpce")]
        public async Task<ActionResult<string>> AskPCE(
            [FromQuery] long userDuz,
            [FromQuery] int location)
        {
            var vq = new VistaQuery("ORWPCE ASKPCE");
            vq.addParameter(VistaQuery.LITERAL, userDuz.ToString());
            vq.addParameter(VistaQuery.LITERAL, location.ToString());
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Force PCE entry check for user at a location.
        /// RPC: ORWPCE FORCE
        /// </summary>
        [HttpGet, Route("api/encounter/force")]
        public async Task<ActionResult<string>> ForcePCE(
            [FromQuery] long userDuz,
            [FromQuery] int location)
        {
            var vq = new VistaQuery("ORWPCE FORCE");
            vq.addParameter(VistaQuery.LITERAL, userDuz.ToString());
            vq.addParameter(VistaQuery.LITERAL, location.ToString());
            return await this.Session.sQuery(vq);
        }

        #endregion

        #region Diagnoses

        /// <summary>
        /// Get diagnosis text.
        /// RPC: ORWPCE GET DX TEXT
        /// </summary>
        [HttpGet, Route("api/encounter/dxtext")]
        public async Task<ActionResult<string>> GetDxText([FromQuery] string dxIen)
        {
            var vq = new VistaQuery("ORWPCE GET DX TEXT");
            vq.addParameter(VistaQuery.LITERAL, dxIen);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Check ICD code version for a date.
        /// RPC: ORWPCE ICDVER
        /// </summary>
        [HttpGet, Route("api/encounter/icdver")]
        public async Task<ActionResult<string>> IcdVersion([FromQuery] string date)
        {
            var vq = new VistaQuery("ORWPCE ICDVER");
            vq.addParameter(VistaQuery.LITERAL, date);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Check if an ICD code is active for a given date.
        /// RPC: ORWPCE ACTIVE CODE
        /// </summary>
        [HttpGet, Route("api/encounter/activecode")]
        public async Task<ActionResult<string>> ActiveCode(
            [FromQuery] string code,
            [FromQuery] string date)
        {
            var vq = new VistaQuery("ORWPCE ACTIVE CODE");
            vq.addParameter(VistaQuery.LITERAL, code);
            vq.addParameter(VistaQuery.LITERAL, date);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Get active problems for a patient as potential diagnoses.
        /// RPC: ORWPCE ACTPROB
        /// </summary>
        [HttpGet, Route("api/encounter/activeproblems")]
        public async Task<ActionResult<List<string>>> ActiveProblems([FromQuery] string dfn)
        {
            var vq = new VistaQuery("ORWPCE ACTPROB");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Diagnosis lookup: retrieve diagnosis data for encounter.
        /// RPC: ORWPCE DIAG
        /// </summary>
        [HttpGet, Route("api/encounter/diag")]
        public async Task<ActionResult<List<string>>> Diag(
            [FromQuery] int location,
            [FromQuery] string visitDate)
        {
            var vq = new VistaQuery("ORWPCE DIAG");
            vq.addParameter(VistaQuery.LITERAL, location.ToString());
            vq.addParameter(VistaQuery.LITERAL, visitDate);
            return await this.Session.tQuery(vq);
        }

        #endregion

        #region Lexicon

        /// <summary>
        /// Lexicon code lookup.
        /// RPC: ORWPCE LEXCODE
        /// </summary>
        [HttpGet, Route("api/encounter/lexcode")]
        public async Task<ActionResult<string>> LexCode(
            [FromQuery] string code,
            [FromQuery] int codeSystem,
            [FromQuery] string date = "")
        {
            var vq = new VistaQuery("ORWPCE LEXCODE");
            vq.addParameter(VistaQuery.LITERAL, code);
            vq.addParameter(VistaQuery.LITERAL, codeSystem.ToString());
            if (!string.IsNullOrEmpty(date))
                vq.addParameter(VistaQuery.LITERAL, date);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Lexicon search with ICD-10 support.
        /// RPC: ORWPCE LEX
        /// </summary>
        [HttpGet, Route("api/encounter/lex")]
        public async Task<ActionResult<List<string>>> Lex(
            [FromQuery] string x,
            [FromQuery] string app = "",
            [FromQuery] string date = "")
        {
            var vq = new VistaQuery("ORWPCE LEX");
            vq.addParameter(VistaQuery.LITERAL, x);
            vq.addParameter(VistaQuery.LITERAL, app);
            if (!string.IsNullOrEmpty(date))
                vq.addParameter(VistaQuery.LITERAL, date);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get the frequency of a lexicon term.
        /// RPC: ORWLEX GETFREQ
        /// </summary>
        [HttpGet, Route("api/encounter/lexfreq")]
        public async Task<ActionResult<string>> LexFreq([FromQuery] string code)
        {
            var vq = new VistaQuery("ORWLEX GETFREQ");
            vq.addParameter(VistaQuery.LITERAL, code);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Get ICD-10 diagnoses from lexicon.
        /// RPC: ORWLEX GETI10DX
        /// </summary>
        [HttpGet, Route("api/encounter/i10dx")]
        public async Task<ActionResult<List<string>>> GetI10Dx([FromQuery] string code)
        {
            var vq = new VistaQuery("ORWLEX GETI10DX");
            vq.addParameter(VistaQuery.LITERAL, code);
            return await this.Session.tQuery(vq);
        }

        #endregion

        #region Procedures

        /// <summary>
        /// Load procedure items for the encounter form.
        /// RPC: ORWPCE PROC
        /// </summary>
        [HttpGet, Route("api/encounter/proc")]
        public async Task<ActionResult<List<string>>> Proc(
            [FromQuery] int location,
            [FromQuery] string visitDate)
        {
            var vq = new VistaQuery("ORWPCE PROC");
            vq.addParameter(VistaQuery.LITERAL, location.ToString());
            vq.addParameter(VistaQuery.LITERAL, visitDate);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get CPT modifiers for a code.
        /// RPC: ORWPCE CPTMODS
        /// </summary>
        [HttpGet, Route("api/encounter/cptmods")]
        public async Task<ActionResult<List<string>>> CptModifiers([FromQuery] string cptCode)
        {
            var vq = new VistaQuery("ORWPCE CPTMODS");
            vq.addParameter(VistaQuery.LITERAL, cptCode);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get modifier list.
        /// RPC: ORWPCE GETMOD
        /// </summary>
        [HttpGet, Route("api/encounter/getmod")]
        public async Task<ActionResult<List<string>>> GetModifiers()
        {
            var vq = new VistaQuery("ORWPCE GETMOD");
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Check whether data has CPT codes.
        /// RPC: ORWPCE HASCPT
        /// </summary>
        [HttpPost, Route("api/encounter/hascpt")]
        public async Task<ActionResult<List<string>>> HasCpt([FromBody] List<string> items)
        {
            var vq = new VistaQuery("ORWPCE HASCPT");
            var dhl = new DictionaryHashList();
            for (int i = 0; i < items.Count; i++)
                dhl.Add((i + 1).ToString(), items[i]);
            vq.addParameter(VistaQuery.LIST, dhl);
            return await this.Session.tQuery(vq);
        }

        #endregion

        #region Immunizations

        /// <summary>
        /// Load immunization items for the encounter form.
        /// RPC: ORWPCE IMM
        /// </summary>
        [HttpGet, Route("api/encounter/imm")]
        public async Task<ActionResult<List<string>>> Imm(
            [FromQuery] int location,
            [FromQuery] string visitDate)
        {
            var vq = new VistaQuery("ORWPCE IMM");
            vq.addParameter(VistaQuery.LITERAL, location.ToString());
            vq.addParameter(VistaQuery.LITERAL, visitDate);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get immunization types.
        /// RPC: ORWPCE GET IMMUNIZATION TYPE
        /// </summary>
        [HttpGet, Route("api/encounter/immtypes")]
        public async Task<ActionResult<List<string>>> ImmunizationTypes([FromQuery] string visitDate)
        {
            var vq = new VistaQuery("ORWPCE GET IMMUNIZATION TYPE");
            vq.addParameter(VistaQuery.LITERAL, visitDate);
            return await this.Session.tQuery(vq);
        }

        #endregion

        #region Skin Tests

        /// <summary>
        /// Load skin-test items for the encounter form.
        /// RPC: ORWPCE SK
        /// </summary>
        [HttpGet, Route("api/encounter/sk")]
        public async Task<ActionResult<List<string>>> SkinTests(
            [FromQuery] int location,
            [FromQuery] string visitDate)
        {
            var vq = new VistaQuery("ORWPCE SK");
            vq.addParameter(VistaQuery.LITERAL, location.ToString());
            vq.addParameter(VistaQuery.LITERAL, visitDate);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get skin test types.
        /// RPC: ORWPCE GET SKIN TEST TYPE
        /// </summary>
        [HttpGet, Route("api/encounter/sktypes")]
        public async Task<ActionResult<List<string>>> SkinTestTypes([FromQuery] string visitDate)
        {
            var vq = new VistaQuery("ORWPCE GET SKIN TEST TYPE");
            vq.addParameter(VistaQuery.LITERAL, visitDate);
            return await this.Session.tQuery(vq);
        }

        #endregion

        #region Patient Education

        /// <summary>
        /// Load patient education items for the encounter form.
        /// RPC: ORWPCE PED
        /// </summary>
        [HttpGet, Route("api/encounter/ped")]
        public async Task<ActionResult<List<string>>> PatientEducation(
            [FromQuery] int location,
            [FromQuery] string visitDate)
        {
            var vq = new VistaQuery("ORWPCE PED");
            vq.addParameter(VistaQuery.LITERAL, location.ToString());
            vq.addParameter(VistaQuery.LITERAL, visitDate);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get education topics.
        /// RPC: ORWPCE GET EDUCATION TOPICS
        /// </summary>
        [HttpGet, Route("api/encounter/edtopics")]
        public async Task<ActionResult<List<string>>> EducationTopics()
        {
            var vq = new VistaQuery("ORWPCE GET EDUCATION TOPICS");
            return await this.Session.tQuery(vq);
        }

        #endregion

        #region Health Factors

        /// <summary>
        /// Load health factor items for the encounter form.
        /// RPC: ORWPCE HF
        /// </summary>
        [HttpGet, Route("api/encounter/hf")]
        public async Task<ActionResult<List<string>>> HealthFactors(
            [FromQuery] int location,
            [FromQuery] string visitDate)
        {
            var vq = new VistaQuery("ORWPCE HF");
            vq.addParameter(VistaQuery.LITERAL, location.ToString());
            vq.addParameter(VistaQuery.LITERAL, visitDate);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get health factor types.
        /// RPC: ORWPCE GET HEALTH FACTORS TY
        /// </summary>
        [HttpGet, Route("api/encounter/hftypes")]
        public async Task<ActionResult<List<string>>> HealthFactorTypes([FromQuery] int idx = 1)
        {
            var vq = new VistaQuery("ORWPCE GET HEALTH FACTORS TY");
            vq.addParameter(VistaQuery.LITERAL, idx.ToString());
            return await this.Session.tQuery(vq);
        }

        #endregion

        #region Exams

        /// <summary>
        /// Load exam items for the encounter form.
        /// RPC: ORWPCE XAM
        /// </summary>
        [HttpGet, Route("api/encounter/xam")]
        public async Task<ActionResult<List<string>>> Exams(
            [FromQuery] int location,
            [FromQuery] string visitDate)
        {
            var vq = new VistaQuery("ORWPCE XAM");
            vq.addParameter(VistaQuery.LITERAL, location.ToString());
            vq.addParameter(VistaQuery.LITERAL, visitDate);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get exam types.
        /// RPC: ORWPCE GET EXAM TYPE
        /// </summary>
        [HttpGet, Route("api/encounter/examtypes")]
        public async Task<ActionResult<List<string>>> ExamTypes()
        {
            var vq = new VistaQuery("ORWPCE GET EXAM TYPE");
            return await this.Session.tQuery(vq);
        }

        #endregion

        #region Set-of-Codes Lookups

        /// <summary>
        /// Get set-of-codes for a PCE field (levels, results, etc.).
        /// RPC: ORWPCE GET SET OF CODES
        /// </summary>
        [HttpGet, Route("api/encounter/setofcodes")]
        public async Task<ActionResult<List<string>>> GetSetOfCodes(
            [FromQuery] string fileNumber,
            [FromQuery] string fieldNumber,
            [FromQuery] string ien = "1")
        {
            var vq = new VistaQuery("ORWPCE GET SET OF CODES");
            vq.addParameter(VistaQuery.LITERAL, fileNumber);
            vq.addParameter(VistaQuery.LITERAL, fieldNumber);
            vq.addParameter(VistaQuery.LITERAL, ien);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get excluded items for a location and type.
        /// RPC: ORWPCE GET EXCLUDED
        /// </summary>
        [HttpGet, Route("api/encounter/excluded")]
        public async Task<ActionResult<List<string>>> GetExcluded(
            [FromQuery] int location,
            [FromQuery] int pceType)
        {
            var vq = new VistaQuery("ORWPCE GET EXCLUDED");
            vq.addParameter(VistaQuery.LITERAL, location.ToString());
            vq.addParameter(VistaQuery.LITERAL, pceType.ToString());
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get historical locations for PCE.
        /// RPC: ORQQPX GET HIST LOCATIONS
        /// </summary>
        [HttpGet, Route("api/encounter/histlocations")]
        public async Task<ActionResult<List<string>>> GetHistLocations()
        {
            var vq = new VistaQuery("ORQQPX GET HIST LOCATIONS");
            return await this.Session.tQuery(vq);
        }

        #endregion

        #region Service-Connected / Eligibility

        /// <summary>
        /// Get SC/AO/IR/EC/MST/HNC/CV/SHD/CL eligibility conditions for a patient.
        /// RPC: ORWPCE SCSEL
        /// </summary>
        [HttpGet, Route("api/encounter/scsel")]
        public async Task<ActionResult<string>> ScSel(
            [FromQuery] string dfn,
            [FromQuery] string encounterDate = "",
            [FromQuery] string location = "")
        {
            var vq = new VistaQuery("ORWPCE SCSEL");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            vq.addParameter(VistaQuery.LITERAL, encounterDate);
            vq.addParameter(VistaQuery.LITERAL, location);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Get rated SC disabilities for a patient.
        /// RPC: ORWPCE SCDIS
        /// </summary>
        [HttpGet, Route("api/encounter/scdis")]
        public async Task<ActionResult<List<string>>> ScDisabilities([FromQuery] string dfn)
        {
            var vq = new VistaQuery("ORWPCE SCDIS");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            return await this.Session.tQuery(vq);
        }

        #endregion

        #region PCE Data Load / Save / Delete

        /// <summary>
        /// Load PCE data associated with a note.
        /// RPC: ORWPCE PCE4NOTE
        /// </summary>
        [HttpGet, Route("api/encounter/pce4note")]
        public async Task<ActionResult<List<string>>> PCE4Note(
            [FromQuery] int noteIen,
            [FromQuery] string dfn = "",
            [FromQuery] string visitStr = "")
        {
            var vq = new VistaQuery("ORWPCE PCE4NOTE");
            if (noteIen < 1)
            {
                vq.addParameter(VistaQuery.LITERAL, noteIen.ToString());
                vq.addParameter(VistaQuery.LITERAL, dfn);
                vq.addParameter(VistaQuery.LITERAL, visitStr);
            }
            else
            {
                vq.addParameter(VistaQuery.LITERAL, noteIen.ToString());
            }
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Save PCE data for a note.
        /// RPC: ORWPCE SAVE
        /// </summary>
        [HttpPost, Route("api/encounter/save")]
        public async Task<ActionResult<List<string>>> Save(
            [FromBody] List<string> pceList,
            [FromQuery] int noteIen,
            [FromQuery] int location)
        {
            var vq = new VistaQuery("ORWPCE SAVE");
            var dhl = new DictionaryHashList();
            for (int i = 0; i < pceList.Count; i++)
                dhl.Add((i + 1).ToString(), pceList[i]);
            vq.addParameter(VistaQuery.LIST, dhl);
            vq.addParameter(VistaQuery.LITERAL, noteIen.ToString());
            vq.addParameter(VistaQuery.LITERAL, location.ToString());
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Delete a PCE encounter data element.
        /// RPC: ORWPCE DELETE
        /// </summary>
        [HttpPost, Route("api/encounter/delete")]
        public async Task<ActionResult<string>> Delete([FromBody] List<string> pceList)
        {
            var vq = new VistaQuery("ORWPCE DELETE");
            var dhl = new DictionaryHashList();
            for (int i = 0; i < pceList.Count; i++)
                dhl.Add((i + 1).ToString(), pceList[i]);
            vq.addParameter(VistaQuery.LIST, dhl);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Get current encounter info for a note.
        /// RPC: ORWPCE PCE4NOTE
        /// </summary>
        [HttpGet, Route("api/encounter/current")]
        public async Task<ActionResult<string>> CurrentEncounter(
            [FromQuery] string noteIen = "0",
            [FromQuery] string dfn = "",
            [FromQuery] string visitStr = "")
        {
            var vq = new VistaQuery("ORWPCE PCE4NOTE");
            vq.addParameter(VistaQuery.LITERAL, noteIen);
            // When noteIen < 1 (no note selected), Pascal sends DFN + VisitStr
            // so VistA can locate the encounter without a note reference.
            if (int.TryParse(noteIen, out var ien) && ien < 1)
            {
                vq.addParameter(VistaQuery.LITERAL, dfn);
                vq.addParameter(VistaQuery.LITERAL, visitStr);
            }
            return await this.Session.sQuery(vq);
        }

        #endregion

        #region Provider / User

        /// <summary>
        /// Verify a provider is active for a given date/time.
        /// RPC: ORWPCE ACTIVE PROV
        /// </summary>
        [HttpGet, Route("api/encounter/activeprov")]
        public async Task<ActionResult<string>> ActiveProvider(
            [FromQuery] string provider,
            [FromQuery] string dateTime)
        {
            var vq = new VistaQuery("ORWPCE ACTIVE PROV");
            vq.addParameter(VistaQuery.LITERAL, provider);
            vq.addParameter(VistaQuery.LITERAL, dateTime);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Get default provider for a note/location/user.
        /// RPC: TIU GET DEFAULT PROVIDER
        /// </summary>
        [HttpGet, Route("api/encounter/defaultprovider")]
        public async Task<ActionResult<string>> DefaultProvider(
            [FromQuery] int location,
            [FromQuery] long userDuz,
            [FromQuery] string date,
            [FromQuery] int noteIen)
        {
            var vq = new VistaQuery("TIU GET DEFAULT PROVIDER");
            vq.addParameter(VistaQuery.LITERAL, location.ToString());
            vq.addParameter(VistaQuery.LITERAL, userDuz.ToString());
            vq.addParameter(VistaQuery.LITERAL, date);
            vq.addParameter(VistaQuery.LITERAL, noteIen.ToString());
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Check if user is a provider.
        /// RPC: TIU IS USER A PROVIDER?
        /// </summary>
        [HttpGet, Route("api/encounter/isuseranprovider")]
        public async Task<ActionResult<string>> IsUserAProvider(
            [FromQuery] long userDuz,
            [FromQuery] string date)
        {
            var vq = new VistaQuery("TIU IS USER A PROVIDER?");
            vq.addParameter(VistaQuery.LITERAL, userDuz.ToString());
            vq.addParameter(VistaQuery.LITERAL, date);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Check if user is a USR provider.
        /// RPC: TIU IS USER A USR PROVIDER
        /// </summary>
        [HttpGet, Route("api/encounter/isusrprovider")]
        public async Task<ActionResult<string>> IsUserAUsrProvider(
            [FromQuery] long userDuz,
            [FromQuery] string date)
        {
            var vq = new VistaQuery("TIU IS USER A USR PROVIDER");
            vq.addParameter(VistaQuery.LITERAL, userDuz.ToString());
            vq.addParameter(VistaQuery.LITERAL, date);
            return await this.Session.sQuery(vq);
        }

        #endregion

        #region Document Parameters & Workload

        /// <summary>
        /// Get document parameters (exposure required, suppress DX/CPT, etc.).
        /// RPC: TIU GET DOCUMENT PARAMETERS
        /// </summary>
        [HttpGet, Route("api/encounter/docparams")]
        public async Task<ActionResult<string>> GetDocumentParameters(
            [FromQuery] int noteIen,
            [FromQuery] int titleIen = 0)
        {
            var vq = new VistaQuery("TIU GET DOCUMENT PARAMETERS");
            if (noteIen <= 0)
            {
                vq.addParameter(VistaQuery.LITERAL, "0");
                vq.addParameter(VistaQuery.LITERAL, titleIen.ToString());
            }
            else
            {
                vq.addParameter(VistaQuery.LITERAL, noteIen.ToString());
            }
            return await this.Session.sQuery(vq);
        }

        #endregion

        #region GAF (Global Assessment of Functioning)

        /// <summary>
        /// Check if GAF scoring is enabled.
        /// RPC: ORWPCE GAFOK
        /// </summary>
        [HttpGet, Route("api/encounter/gafok")]
        public async Task<ActionResult<string>> GafOk()
        {
            var vq = new VistaQuery("ORWPCE GAFOK");
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Check if a location is a mental health clinic.
        /// RPC: ORWPCE MHCLINIC
        /// </summary>
        [HttpGet, Route("api/encounter/mhclinic")]
        public async Task<ActionResult<string>> MhClinic([FromQuery] int location)
        {
            var vq = new VistaQuery("ORWPCE MHCLINIC");
            vq.addParameter(VistaQuery.LITERAL, location.ToString());
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Load recent GAF scores for a patient.
        /// RPC: ORWPCE LOADGAF
        /// </summary>
        [HttpGet, Route("api/encounter/loadgaf")]
        public async Task<ActionResult<List<string>>> LoadGaf(
            [FromQuery] string dfn,
            [FromQuery] int limit = 10)
        {
            var vq = new VistaQuery("ORWPCE LOADGAF");
            var dhl = new DictionaryHashList();
            dhl.Add("\"DFN\"", dfn);
            dhl.Add("\"LIMIT\"", limit.ToString());
            vq.addParameter(VistaQuery.LIST, dhl);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Save a GAF score.
        /// RPC: ORWPCE SAVEGAF
        /// </summary>
        [HttpPost, Route("api/encounter/savegaf")]
        public async Task<ActionResult<List<string>>> SaveGaf(
            [FromQuery] string dfn,
            [FromQuery] int score,
            [FromQuery] string gafDate,
            [FromQuery] long staff)
        {
            var vq = new VistaQuery("ORWPCE SAVEGAF");
            var dhl = new DictionaryHashList();
            dhl.Add("\"DFN\"", dfn);
            dhl.Add("\"GAF\"", score.ToString());
            dhl.Add("\"DATE\"", gafDate);
            dhl.Add("\"STAFF\"", staff.ToString());
            vq.addParameter(VistaQuery.LIST, dhl);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get the URL for GAF information.
        /// RPC: ORWPCE GAFURL
        /// </summary>
        [HttpGet, Route("api/encounter/gafurl")]
        public async Task<ActionResult<string>> GafUrl()
        {
            var vq = new VistaQuery("ORWPCE GAFURL");
            return await this.Session.sQuery(vq);
        }

        #endregion

        #region Mental Health Tests

        /// <summary>
        /// Check if MH tests are enabled.
        /// RPC: ORWPCE MHTESTOK
        /// </summary>
        [HttpGet, Route("api/encounter/mhtestok")]
        public async Task<ActionResult<string>> MhTestOk()
        {
            var vq = new VistaQuery("ORWPCE MHTESTOK");
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Check if a user is authorized for a specific MH test.
        /// RPC: ORWPCE MH TEST AUTHORIZED
        /// </summary>
        [HttpGet, Route("api/encounter/mhtestauth")]
        public async Task<ActionResult<string>> MhTestAuthorized(
            [FromQuery] string test,
            [FromQuery] long userDuz)
        {
            var vq = new VistaQuery("ORWPCE MH TEST AUTHORIZED");
            vq.addParameter(VistaQuery.LITERAL, test);
            vq.addParameter(VistaQuery.LITERAL, userDuz.ToString());
            return await this.Session.sQuery(vq);
        }

        #endregion
    }
}
