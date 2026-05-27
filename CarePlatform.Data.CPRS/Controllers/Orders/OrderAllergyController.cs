using CarePlatform.Data.VistA;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarePlatform.Data.CPRS
{
    /// <summary>
    /// Allergy order entry — match, symptoms, load/save, bulletins, site params.
    /// Migrated from cprs/Orders/rODAllergy.pas — ORWDAL32 RPCs.
    /// </summary>
    public class OrderAllergyController : BaseController
    {
        public OrderAllergyController() : base() { }

        [HttpGet, Route("api/orderallergy/defaults")]
        public async Task<ActionResult<List<string>>> ODForAllergies()
        {
            var vq = new VistaQuery("ORWDAL32 DEF");
            return await this.Session.tQuery(vq);
        }

        [HttpGet, Route("api/orderallergy/match")]
        public async Task<ActionResult<List<string>>> SearchForAllergies([FromQuery] string search)
        {
            var vq = new VistaQuery("ORWDAL32 ALLERGY MATCH");
            vq.addParameter(VistaQuery.LITERAL, search);
            return await this.Session.tQuery(vq);
        }

        [HttpGet, Route("api/orderallergy/symptoms")]
        public async Task<ActionResult<List<string>>> SubsetOfSymptoms(
            [FromQuery] string startFrom, [FromQuery] int direction)
        {
            var vq = new VistaQuery("ORWDAL32 SYMPTOMS");
            vq.addParameter(VistaQuery.LITERAL, startFrom);
            vq.addParameter(VistaQuery.LITERAL, direction.ToString());
            return await this.Session.tQuery(vq);
        }

        [HttpGet, Route("api/orderallergy/loadforedit")]
        public async Task<ActionResult<List<string>>> LoadAllergyForEdit([FromQuery] int allergyIen)
        {
            var vq = new VistaQuery("ORWDAL32 LOAD FOR EDIT");
            vq.addParameter(VistaQuery.LITERAL, allergyIen.ToString());
            return await this.Session.tQuery(vq);
        }

        [HttpPost, Route("api/orderallergy/save")]
        public async Task<ActionResult<string>> SaveAllergy(
            [FromQuery] int ien, [FromQuery] string dfn,
            [FromBody] List<string> allergyData)
        {
            var vq = new VistaQuery("ORWDAL32 SAVE ALLERGY");
            vq.addParameter(VistaQuery.LITERAL, ien.ToString());
            vq.addParameter(VistaQuery.LITERAL, dfn);
            var dhl = new DictionaryHashList();
            for (int i = 0; i < allergyData.Count; i++)
                dhl.Add(i.ToString(), allergyData[i]);
            vq.addParameter(VistaQuery.LIST, dhl);
            return await this.Session.sQuery(vq);
        }

        [HttpPost, Route("api/orderallergy/sendbulletin")]
        public async Task<ActionResult<string>> SendARTBulletin(
            [FromQuery] long userDuz, [FromQuery] string dfn,
            [FromQuery] string freeTextEntry, [FromBody] List<string> comments)
        {
            var vq = new VistaQuery("ORWDAL32 SEND BULLETIN");
            vq.addParameter(VistaQuery.LITERAL, userDuz.ToString());
            vq.addParameter(VistaQuery.LITERAL, dfn);
            vq.addParameter(VistaQuery.LITERAL, freeTextEntry);
            var dhl = new DictionaryHashList();
            for (int i = 0; i < comments.Count; i++)
                dhl.Add(i.ToString(), comments[i]);
            vq.addParameter(VistaQuery.LIST, dhl);
            return await this.Session.sQuery(vq);
        }

        [HttpGet, Route("api/orderallergy/siteparams")]
        public async Task<ActionResult<string>> GetSiteParams()
        {
            var vq = new VistaQuery("ORWDAL32 SITE PARAMS");
            return await this.Session.sQuery(vq);
        }

        [HttpGet, Route("api/orderallergy/clinicaluser")]
        public async Task<ActionResult<string>> IsARTClinicalUser()
        {
            var vq = new VistaQuery("ORWDAL32 CLINUSER");
            return await this.Session.sQuery(vq);
        }
    }
}
