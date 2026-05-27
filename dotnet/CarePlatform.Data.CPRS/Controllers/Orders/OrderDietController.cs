// Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
using CarePlatform.Data.VistA;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarePlatform.Data.CPRS
{
    /// <summary>
    /// Diet/nutrition order dialogs — diets, tube feeding, late trays, isolation, meals.
    /// Migrated from cprs/Orders/rODDiet.pas — ORWDFH RPCs.
    /// </summary>
    public class OrderDietController : BaseController
    {
        public OrderDietController() : base() { }

        [HttpGet, Route("api/orderdiet/attributes")]
        public async Task<ActionResult<string>> DietAttributes([FromQuery] int orderableItem)
        {
            var vq = new VistaQuery("ORWDFH ATTR");
            vq.addParameter(VistaQuery.LITERAL, orderableItem.ToString());
            return await this.Session.sQuery(vq);
        }

        [HttpGet, Route("api/orderdiet/params")]
        public async Task<ActionResult<List<string>>> LoadDietParams(
            [FromQuery] string dfn, [FromQuery] string location)
        {
            var vq = new VistaQuery("ORWDFH PARAM");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            vq.addParameter(VistaQuery.LITERAL, location);
            return await this.Session.tQuery(vq);
        }

        [HttpGet, Route("api/orderdiet/currentdiettext")]
        public async Task<ActionResult<List<string>>> CurrentDietText([FromQuery] string dfn)
        {
            var vq = new VistaQuery("ORWDFH TXT");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            return await this.Session.tQuery(vq);
        }

        [HttpGet, Route("api/orderdiet/tubefeeding")]
        public async Task<ActionResult<List<string>>> TubeFeedingProducts()
        {
            var vq = new VistaQuery("ORWDFH TFPROD");
            return await this.Session.tQuery(vq);
        }

        [HttpGet, Route("api/orderdiet/expandedqty")]
        public async Task<ActionResult<string>> ExpandedQuantity(
            [FromQuery] int product, [FromQuery] int strength, [FromQuery] string qty)
        {
            var vq = new VistaQuery("ORWDFH QTY2CC");
            vq.addParameter(VistaQuery.LITERAL, product.ToString());
            vq.addParameter(VistaQuery.LITERAL, strength.ToString());
            vq.addParameter(VistaQuery.LITERAL, qty);
            return await this.Session.sQuery(vq);
        }

        [HttpGet, Route("api/orderdiet/diets")]
        public async Task<ActionResult<List<string>>> SubsetOfDiets(
            [FromQuery] string startFrom, [FromQuery] int direction)
        {
            var vq = new VistaQuery("ORWDFH DIETS");
            vq.addParameter(VistaQuery.LITERAL, startFrom);
            vq.addParameter(VistaQuery.LITERAL, direction.ToString());
            return await this.Session.tQuery(vq);
        }

        [HttpGet, Route("api/orderdiet/opdiets")]
        public async Task<ActionResult<List<string>>> OutpatientDiets()
        {
            var vq = new VistaQuery("ORWDFH OPDIETS");
            return await this.Session.tQuery(vq);
        }

        [HttpPost, Route("api/orderdiet/latetray")]
        public async Task<ActionResult<List<string>>> OrderLateTray(
            [FromQuery] string dfn, [FromQuery] long provider,
            [FromQuery] long location, [FromQuery] string meal,
            [FromQuery] string mealTime, [FromQuery] bool bagged = false)
        {
            var vq = new VistaQuery("ORWDFH ADDLATE");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            vq.addParameter(VistaQuery.LITERAL, provider.ToString());
            vq.addParameter(VistaQuery.LITERAL, location.ToString());
            vq.addParameter(VistaQuery.LITERAL, meal);
            vq.addParameter(VistaQuery.LITERAL, mealTime);
            vq.addParameter(VistaQuery.LITERAL, bagged ? "1" : "0");
            return await this.Session.tQuery(vq);
        }

        [HttpGet, Route("api/orderdiet/isolationid")]
        public async Task<ActionResult<string>> IsolationID()
        {
            var vq = new VistaQuery("ORWDFH ISOIEN");
            return await this.Session.sQuery(vq);
        }

        [HttpGet, Route("api/orderdiet/currentisolation")]
        public async Task<ActionResult<string>> CurrentIsolation([FromQuery] string dfn)
        {
            var vq = new VistaQuery("ORWDFH CURISO");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            return await this.Session.sQuery(vq);
        }

        [HttpGet, Route("api/orderdiet/isolations")]
        public async Task<ActionResult<List<string>>> LoadIsolations()
        {
            var vq = new VistaQuery("ORWDFH ISOLIST");
            return await this.Session.tQuery(vq);
        }

        [HttpGet, Route("api/orderdiet/dialogtype")]
        public async Task<ActionResult<string>> DietDialogType([FromQuery] int groupIen)
        {
            var vq = new VistaQuery("ORWDFH FINDTYP");
            vq.addParameter(VistaQuery.LITERAL, groupIen.ToString());
            return await this.Session.sQuery(vq);
        }

        [HttpGet, Route("api/orderdiet/currentmeals")]
        public async Task<ActionResult<List<string>>> CurrentRecurringMeals(
            [FromQuery] string dfn, [FromQuery] string mealType = "")
        {
            var vq = new VistaQuery("ORWDFH CURRENT MEALS");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            vq.addParameter(VistaQuery.LITERAL, mealType);
            return await this.Session.tQuery(vq);
        }

        [HttpGet, Route("api/orderdiet/nfslocready")]
        public async Task<ActionResult<string>> OutpatientLocationConfigured([FromQuery] string location)
        {
            var vq = new VistaQuery("ORWDFH NFSLOC READY");
            vq.addParameter(VistaQuery.LITERAL, location);
            return await this.Session.sQuery(vq);
        }
    }
}
