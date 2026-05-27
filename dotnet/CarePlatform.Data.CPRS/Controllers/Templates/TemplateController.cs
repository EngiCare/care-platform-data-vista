// Copyright (c) 2026 Engineered Care, Inc. Licensed under the MIT License.
using CarePlatform.Data.VistA;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarePlatform.Data.CPRS
{
    /// <summary>
    /// Templates — template trees, boilerplate, text expansion, create/modify/delete,
    /// access levels, defaults, locking, linked data, reminder dialogs,
    /// template fields (list, load, save, lock, import/export).
    /// Migrated from cprs/Templates/rTemplates.pas — TIU TEMPLATE *, TIU FIELD *,
    /// TIU GET LIST OF OBJECTS, TIU GET BOILERPLATE, TIU LONG LIST BOILERPLATED,
    /// TIU REMINDER DIALOGS, TIU REM DLG OK AS TEMPLATE RPCs.
    /// </summary>
    public class TemplateController : BaseController
    {
        public TemplateController() : base() { }

        #region Template Tree

        /// <summary>
        /// Get template roots for the current user.
        /// RPC: TIU TEMPLATE GETROOTS
        /// </summary>
        [HttpGet, Route("api/template/roots")]
        public async Task<ActionResult<List<string>>> GetRoots([FromQuery] long userDuz)
        {
            var vq = new VistaQuery("TIU TEMPLATE GETROOTS");
            vq.addParameter(VistaQuery.LITERAL, userDuz.ToString());
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get children of a template node.
        /// RPC: TIU TEMPLATE GETITEMS
        /// </summary>
        [HttpGet, Route("api/template/items")]
        public async Task<ActionResult<List<string>>> GetItems([FromQuery] string templateId)
        {
            var vq = new VistaQuery("TIU TEMPLATE GETITEMS");
            vq.addParameter(VistaQuery.LITERAL, templateId);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Set children for a template node.
        /// RPC: TIU TEMPLATE SET ITEMS
        /// </summary>
        [HttpPost, Route("api/template/setitems")]
        public async Task<ActionResult<List<string>>> SetItems(
            [FromQuery] string templateId,
            [FromBody] List<string> children)
        {
            var vq = new VistaQuery("TIU TEMPLATE SET ITEMS");
            vq.addParameter(VistaQuery.LITERAL, templateId);
            var dhl = new DictionaryHashList();
            for (int i = 0; i < children.Count; i++)
                dhl.Add((i + 1).ToString(), children[i]);
            vq.addParameter(VistaQuery.LIST, dhl);
            return await this.Session.tQuery(vq);
        }

        #endregion

        #region Boilerplate & Text

        /// <summary>
        /// Get boilerplate text for a template.
        /// RPC: TIU TEMPLATE GETBOIL
        /// </summary>
        [HttpGet, Route("api/template/boilerplate")]
        public async Task<ActionResult<List<string>>> GetBoilerplate([FromQuery] string templateId)
        {
            var vq = new VistaQuery("TIU TEMPLATE GETBOIL");
            vq.addParameter(VistaQuery.LITERAL, templateId);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Expand template text (resolve objects, fields, etc.).
        /// RPC: TIU TEMPLATE GETTEXT
        /// </summary>
        [HttpPost, Route("api/template/gettext")]
        public async Task<ActionResult<List<string>>> GetText(
            [FromQuery] string dfn,
            [FromQuery] string visitStr,
            [FromBody] List<string> boilerplate)
        {
            var vq = new VistaQuery("TIU TEMPLATE GETTEXT");
            vq.addParameter(VistaQuery.LITERAL, dfn);
            vq.addParameter(VistaQuery.LITERAL, visitStr);
            var dhl = new DictionaryHashList();
            for (int i = 0; i < boilerplate.Count; i++)
                dhl.Add((i + 1) + ",0", boilerplate[i]);
            vq.addParameter(VistaQuery.LIST, dhl);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Check boilerplate for required fields / objects.
        /// RPC: TIU TEMPLATE CHECK BOILERPLATE
        /// </summary>
        [HttpPost, Route("api/template/checkboilerplate")]
        public async Task<ActionResult<List<string>>> CheckBoilerplate([FromBody] List<string> boilerplate)
        {
            var vq = new VistaQuery("TIU TEMPLATE CHECK BOILERPLATE");
            var dhl = new DictionaryHashList();
            for (int i = 0; i < boilerplate.Count; i++)
                dhl.Add("2," + (i + 1) + ",0", boilerplate[i]);
            vq.addParameter(VistaQuery.LIST, dhl);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get boilerplate for a TIU title.
        /// RPC: TIU GET BOILERPLATE
        /// </summary>
        [HttpGet, Route("api/template/titleboilerplate")]
        public async Task<ActionResult<List<string>>> GetTitleBoilerplate([FromQuery] string titleIen)
        {
            var vq = new VistaQuery("TIU GET BOILERPLATE");
            vq.addParameter(VistaQuery.LITERAL, titleIen);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Long list of boilerplated (template-linked) TIU titles.
        /// RPC: TIU LONG LIST BOILERPLATED
        /// </summary>
        [HttpGet, Route("api/template/boilerplatedtitles")]
        public async Task<ActionResult<List<string>>> BoilerplatedTitles(
            [FromQuery] string startFrom,
            [FromQuery] int direction)
        {
            var vq = new VistaQuery("TIU LONG LIST BOILERPLATED");
            vq.addParameter(VistaQuery.LITERAL, startFrom);
            vq.addParameter(VistaQuery.LITERAL, direction.ToString());
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Long list of all TIU titles for template linking.
        /// RPC: TIU TEMPLATE ALL TITLES
        /// </summary>
        [HttpGet, Route("api/template/alltitles")]
        public async Task<ActionResult<List<string>>> AllTitles(
            [FromQuery] string startFrom,
            [FromQuery] int direction)
        {
            var vq = new VistaQuery("TIU TEMPLATE ALL TITLES");
            vq.addParameter(VistaQuery.LITERAL, startFrom);
            vq.addParameter(VistaQuery.LITERAL, direction.ToString());
            return await this.Session.tQuery(vq);
        }

        #endregion

        #region Template CRUD

        /// <summary>
        /// Create or modify a template.
        /// RPC: TIU TEMPLATE CREATE/MODIFY
        /// </summary>
        [HttpPost, Route("api/template/createmodify")]
        public async Task<ActionResult<List<string>>> CreateModify(
            [FromQuery] string templateId,
            [FromBody] List<string> fields)
        {
            var vq = new VistaQuery("TIU TEMPLATE CREATE/MODIFY");
            vq.addParameter(VistaQuery.LITERAL, templateId);
            var dhl = new DictionaryHashList();
            for (int i = 0; i < fields.Count; i++)
            {
                var eqPos = fields[i].IndexOf('=');
                if (eqPos > 0)
                {
                    var key = fields[i].Substring(0, eqPos);
                    var val = fields[i].Substring(eqPos + 1);
                    dhl.Add(key, val);
                }
            }
            vq.addParameter(VistaQuery.LIST, dhl);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Delete templates.
        /// RPC: TIU TEMPLATE DELETE
        /// </summary>
        [HttpPost, Route("api/template/delete")]
        public async Task<ActionResult<List<string>>> Delete([FromBody] List<string> templateIds)
        {
            var vq = new VistaQuery("TIU TEMPLATE DELETE");
            var dhl = new DictionaryHashList();
            for (int i = 0; i < templateIds.Count; i++)
                dhl.Add((i + 1).ToString(), templateIds[i]);
            vq.addParameter(VistaQuery.LIST, dhl);
            return await this.Session.tQuery(vq);
        }

        #endregion

        #region Access & Editor

        /// <summary>
        /// Check if a user is a template editor for a specific template.
        /// RPC: TIU TEMPLATE ISEDITOR
        /// </summary>
        [HttpGet, Route("api/template/iseditor")]
        public async Task<ActionResult<string>> IsEditor(
            [FromQuery] string templateId,
            [FromQuery] long userDuz)
        {
            var vq = new VistaQuery("TIU TEMPLATE ISEDITOR");
            vq.addParameter(VistaQuery.LITERAL, templateId);
            vq.addParameter(VistaQuery.LITERAL, userDuz.ToString());
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Get access level for a template (user + location context).
        /// RPC: TIU TEMPLATE ACCESS LEVEL
        /// </summary>
        [HttpGet, Route("api/template/accesslevel")]
        public async Task<ActionResult<string>> AccessLevel(
            [FromQuery] string templateId,
            [FromQuery] long userDuz,
            [FromQuery] int location)
        {
            var vq = new VistaQuery("TIU TEMPLATE ACCESS LEVEL");
            vq.addParameter(VistaQuery.LITERAL, templateId);
            vq.addParameter(VistaQuery.LITERAL, userDuz.ToString());
            vq.addParameter(VistaQuery.LITERAL, location.ToString());
            return await this.Session.sQuery(vq);
        }

        #endregion

        #region Defaults & Description

        /// <summary>
        /// Get user template defaults.
        /// RPC: TIU TEMPLATE GET DEFAULTS
        /// </summary>
        [HttpGet, Route("api/template/defaults")]
        public async Task<ActionResult<string>> GetDefaults()
        {
            var vq = new VistaQuery("TIU TEMPLATE GET DEFAULTS");
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Save user template defaults.
        /// RPC: TIU TEMPLATE SET DEFAULTS
        /// </summary>
        [HttpPost, Route("api/template/setdefaults")]
        public async Task<ActionResult<string>> SetDefaults([FromQuery] string defaults)
        {
            var vq = new VistaQuery("TIU TEMPLATE SET DEFAULTS");
            vq.addParameter(VistaQuery.LITERAL, defaults);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Get description for a template.
        /// RPC: TIU TEMPLATE GET DESCRIPTION
        /// </summary>
        [HttpGet, Route("api/template/description")]
        public async Task<ActionResult<List<string>>> GetDescription([FromQuery] string templateIen)
        {
            var vq = new VistaQuery("TIU TEMPLATE GET DESCRIPTION");
            vq.addParameter(VistaQuery.LITERAL, templateIen);
            return await this.Session.tQuery(vq);
        }

        #endregion

        #region Locking

        /// <summary>
        /// Lock a template for editing.
        /// RPC: TIU TEMPLATE LOCK
        /// </summary>
        [HttpPost, Route("api/template/lock")]
        public async Task<ActionResult<string>> Lock([FromQuery] string templateId)
        {
            var vq = new VistaQuery("TIU TEMPLATE LOCK");
            vq.addParameter(VistaQuery.LITERAL, templateId);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Unlock a template.
        /// RPC: TIU TEMPLATE UNLOCK
        /// </summary>
        [HttpPost, Route("api/template/unlock")]
        public async Task<ActionResult<string>> Unlock([FromQuery] string templateId)
        {
            var vq = new VistaQuery("TIU TEMPLATE UNLOCK");
            vq.addParameter(VistaQuery.LITERAL, templateId);
            return await this.Session.sQuery(vq);
        }

        #endregion

        #region Linked Data & Objects

        /// <summary>
        /// Get linked template data.
        /// RPC: TIU TEMPLATE GETLINK
        /// </summary>
        [HttpGet, Route("api/template/getlink")]
        public async Task<ActionResult<string>> GetLink([FromQuery] string link)
        {
            var vq = new VistaQuery("TIU TEMPLATE GETLINK");
            vq.addParameter(VistaQuery.LITERAL, link);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Get list of TIU objects.
        /// RPC: TIU GET LIST OF OBJECTS
        /// </summary>
        [HttpGet, Route("api/template/objects")]
        public async Task<ActionResult<List<string>>> GetObjects()
        {
            var vq = new VistaQuery("TIU GET LIST OF OBJECTS");
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Get personal objects allowed for templates.
        /// RPC: TIU TEMPLATE PERSONAL OBJECTS
        /// </summary>
        [HttpGet, Route("api/template/personalobjects")]
        [Route("api/template/personal")]
        public async Task<ActionResult<List<string>>> PersonalObjects()
        {
            var vq = new VistaQuery("TIU TEMPLATE PERSONAL OBJECTS");
            return await this.Session.tQuery(vq);
        }

        #endregion

        #region Reminder Dialogs

        /// <summary>
        /// Get allowed reminder dialogs for templates.
        /// RPC: TIU REMINDER DIALOGS
        /// </summary>
        [HttpGet, Route("api/template/reminderdialogs")]
        public async Task<ActionResult<List<string>>> ReminderDialogs()
        {
            var vq = new VistaQuery("TIU REMINDER DIALOGS");
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Check if a reminder dialog is allowed as a template.
        /// RPC: TIU REM DLG OK AS TEMPLATE
        /// </summary>
        [HttpGet, Route("api/template/remdlgok")]
        public async Task<ActionResult<string>> RemDlgOkAsTemplate([FromQuery] string remDlgIen)
        {
            var vq = new VistaQuery("TIU REM DLG OK AS TEMPLATE");
            vq.addParameter(VistaQuery.LITERAL, remDlgIen);
            return await this.Session.sQuery(vq);
        }

        #endregion

        #region Template Fields

        /// <summary>
        /// Long list of template fields.
        /// RPC: TIU FIELD LIST
        /// </summary>
        [HttpGet, Route("api/template/fieldlist")]
        public async Task<ActionResult<List<string>>> FieldList(
            [FromQuery] string startFrom,
            [FromQuery] int direction)
        {
            var vq = new VistaQuery("TIU FIELD LIST");
            vq.addParameter(VistaQuery.LITERAL, startFrom);
            vq.addParameter(VistaQuery.LITERAL, direction.ToString());
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Load a template field by name.
        /// RPC: TIU FIELD LOAD
        /// </summary>
        [HttpGet, Route("api/template/fieldload")]
        public async Task<ActionResult<List<string>>> FieldLoad([FromQuery] string fieldName)
        {
            var vq = new VistaQuery("TIU FIELD LOAD");
            vq.addParameter(VistaQuery.LITERAL, fieldName);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Load a template field by IEN.
        /// RPC: TIU FIELD LOAD BY IEN
        /// </summary>
        [HttpGet, Route("api/template/fieldloadbyien")]
        public async Task<ActionResult<List<string>>> FieldLoadByIen([FromQuery] string ien)
        {
            var vq = new VistaQuery("TIU FIELD LOAD BY IEN");
            vq.addParameter(VistaQuery.LITERAL, ien);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Check if user can edit template fields.
        /// RPC: TIU FIELD CAN EDIT
        /// </summary>
        [HttpGet, Route("api/template/fieldcanedit")]
        public async Task<ActionResult<string>> FieldCanEdit()
        {
            var vq = new VistaQuery("TIU FIELD CAN EDIT");
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Save a template field.
        /// RPC: TIU FIELD SAVE
        /// </summary>
        [HttpPost, Route("api/template/fieldsave")]
        public async Task<ActionResult<List<string>>> FieldSave(
            [FromQuery] string fieldId,
            [FromBody] List<string> fields)
        {
            var vq = new VistaQuery("TIU FIELD SAVE");
            vq.addParameter(VistaQuery.LITERAL, fieldId);
            var dhl = new DictionaryHashList();
            for (int i = 0; i < fields.Count; i++)
            {
                var eqPos = fields[i].IndexOf('=');
                if (eqPos > 0)
                {
                    var key = fields[i].Substring(0, eqPos);
                    var val = fields[i].Substring(eqPos + 1);
                    dhl.Add(key, val);
                }
            }
            vq.addParameter(VistaQuery.LIST, dhl);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Lock a template field.
        /// RPC: TIU FIELD LOCK
        /// </summary>
        [HttpPost, Route("api/template/fieldlock")]
        public async Task<ActionResult<string>> FieldLock([FromQuery] string fieldId)
        {
            var vq = new VistaQuery("TIU FIELD LOCK");
            vq.addParameter(VistaQuery.LITERAL, fieldId);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Unlock a template field.
        /// RPC: TIU FIELD UNLOCK
        /// </summary>
        [HttpPost, Route("api/template/fieldunlock")]
        public async Task<ActionResult<string>> FieldUnlock([FromQuery] string fieldId)
        {
            var vq = new VistaQuery("TIU FIELD UNLOCK");
            vq.addParameter(VistaQuery.LITERAL, fieldId);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Delete a template field.
        /// RPC: TIU FIELD DELETE
        /// </summary>
        [HttpPost, Route("api/template/fielddelete")]
        public async Task<ActionResult<string>> FieldDelete([FromQuery] string fieldId)
        {
            var vq = new VistaQuery("TIU FIELD DELETE");
            vq.addParameter(VistaQuery.LITERAL, fieldId);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Export template fields.
        /// RPC: TIU FIELD EXPORT
        /// </summary>
        [HttpPost, Route("api/template/fieldexport")]
        public async Task<ActionResult<List<string>>> FieldExport([FromBody] List<string> fieldList)
        {
            var vq = new VistaQuery("TIU FIELD EXPORT");
            var dhl = new DictionaryHashList();
            for (int i = 0; i < fieldList.Count; i++)
                dhl.Add((i + 1).ToString(), fieldList[i]);
            vq.addParameter(VistaQuery.LIST, dhl);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Import template fields.
        /// RPC: TIU FIELD IMPORT
        /// </summary>
        [HttpPost, Route("api/template/fieldimport")]
        public async Task<ActionResult<List<string>>> FieldImport([FromBody] List<string> fieldList)
        {
            var vq = new VistaQuery("TIU FIELD IMPORT");
            var dhl = new DictionaryHashList();
            for (int i = 0; i < fieldList.Count; i++)
                dhl.Add((i + 1).ToString(), fieldList[i]);
            vq.addParameter(VistaQuery.LIST, dhl);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Check if a field name is unique.
        /// RPC: TIU FIELD NAME IS UNIQUE
        /// </summary>
        [HttpGet, Route("api/template/fieldnameunique")]
        public async Task<ActionResult<string>> FieldNameIsUnique(
            [FromQuery] string fieldName,
            [FromQuery] string ien)
        {
            var vq = new VistaQuery("TIU FIELD NAME IS UNIQUE");
            vq.addParameter(VistaQuery.LITERAL, fieldName);
            vq.addParameter(VistaQuery.LITERAL, ien);
            return await this.Session.sQuery(vq);
        }

        /// <summary>
        /// Check template fields for errors.
        /// RPC: TIU FIELD CHECK
        /// </summary>
        [HttpGet, Route("api/template/fieldcheck")]
        public async Task<ActionResult<List<string>>> FieldCheck()
        {
            var vq = new VistaQuery("TIU FIELD CHECK");
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Convert text to LM (letter-merge) format.
        /// RPC: TIU FIELD DOLMTEXT
        /// </summary>
        [HttpPost, Route("api/template/dolmtext")]
        public async Task<ActionResult<List<string>>> DoLmText([FromBody] List<string> text)
        {
            var vq = new VistaQuery("TIU FIELD DOLMTEXT");
            var dhl = new DictionaryHashList();
            for (int i = 0; i < text.Count; i++)
                dhl.Add((i + 1) + ",0", text[i]);
            vq.addParameter(VistaQuery.LIST, dhl);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Build template fields from XML.
        /// RPC: TIU FIELD LIST ADD
        /// </summary>
        [HttpPost, Route("api/template/fieldlistadd")]
        public async Task<ActionResult<List<string>>> FieldListAdd([FromBody] List<string> xmlData)
        {
            var vq = new VistaQuery("TIU FIELD LIST ADD");
            var dhl = new DictionaryHashList();
            for (int i = 0; i < xmlData.Count; i++)
                dhl.Add((i + 1).ToString(), xmlData[i]);
            vq.addParameter(VistaQuery.LIST, dhl);
            return await this.Session.tQuery(vq);
        }

        /// <summary>
        /// Import loaded fields.
        /// RPC: TIU FIELD LIST IMPORT
        /// </summary>
        [HttpPost, Route("api/template/fieldlistimport")]
        public async Task<ActionResult<List<string>>> FieldListImport()
        {
            var vq = new VistaQuery("TIU FIELD LIST IMPORT");
            return await this.Session.tQuery(vq);
        }

        #endregion
    }
}
