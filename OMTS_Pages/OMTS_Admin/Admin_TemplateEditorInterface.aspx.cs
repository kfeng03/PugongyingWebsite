using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace fyp.OMTS_Pages.OMTS_Admin
{
    public partial class Admin_TemplateEditorInterface : System.Web.UI.Page
    {
        protected FirebaseHelper firebaseHelper;

        protected void Page_Load(object sender, EventArgs e)
        {
            firebaseHelper = new FirebaseHelper();

            if (!IsPostBack)
            {
                // Check if we're editing an existing template
                string templateId = Request.QueryString["templateId"];
                if (!string.IsNullOrEmpty(templateId))
                {
                    LoadExistingTemplate(templateId);
                }
            }
            else
            {
                // Handle postback - check if template data was submitted
                if (!string.IsNullOrEmpty(hdnTemplateData.Value))
                {
                    SaveTemplate();
                }
            }
        }

        private async void LoadExistingTemplate(string templateId)
        {
            try
            {
                var template = await firebaseHelper.GetJsonTemplateById(templateId);
                if (template != null)
                {
                    hdnTemplateData.Value = template.TemplateData;
                    hdnTemplateName.Value = template.TemplateName;
                    hdnTemplateDescription.Value = template.TemplateDescription;
                    hdnTemplateType.Value = template.TemplateType;
                    hdnTemplateImageUrl.Value = template.TemplateImageUrl;
                    hdnAction.Value = "edit";

                    // Register JavaScript to load the template data
                    string script = $"loadTemplateFromServer('{template.TemplateData}');";
                    ScriptManager.RegisterStartupScript(this, GetType(), "LoadTemplate", script, true);
                }
            }
            catch (Exception ex)
            {
                // Handle error
                System.Diagnostics.Debug.WriteLine($"Error loading template: {ex.Message}");
            }
        }

        private async void SaveTemplate()
        {
            try
            {
                // Parse the template data JSON
                var templateDataJson = hdnTemplateData.Value;
                var templateDataObj = JObject.Parse(templateDataJson);
                var elements = templateDataObj["elements"] as JArray;

                // Create elements structure instead of fields
                var elementsDict = new Dictionary<string, object>();
                if (elements != null)
                {
                    foreach (var el in elements)
                    {
                        var elementId = (string)el["id"];
                        elementsDict[elementId] = new Dictionary<string, object>
                {
                    { "id", el["id"] },
                    { "type", el["type"] },
                    { "style", el["style"] },
                    { "text", el["text"] },
                    { "dataField", el["dataField"] },
                    { "defaultValue", el["defaultValue"] },
                    { "imageUrl", el["imageUrl"] },
                    { "locked", el["locked"] }
                };
                    }
                }

                // Create meta information
                var meta = templateDataObj["meta"];

                // Prepare metadata with camelCase naming
                var metadata = new Dictionary<string, object>
                {
                    { "templateName", hdnTemplateName.Value },
                    { "templateDescription", hdnTemplateDescription.Value },
                    { "templateType", hdnTemplateType.Value },
                    { "templateImageUrl", hdnTemplateImageUrl.Value },
                    { "createdBy", Session["UserId"]?.ToString() ?? "Admin" },
                    { "createdAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
                    { "lastModified", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
                    { "isDeleted", false },
                    { "isActive", true },
                    { "elements", elementsDict },
                    //{ "meta", meta }
                };

                if (hdnAction.Value == "edit")
                {
                    string templateId = Request.QueryString["templateId"];
                    if (!string.IsNullOrEmpty(templateId))
                    {
                        await firebaseHelper.UpdateStructuredTemplate(templateId, metadata);
                    }
                }
                else
                {
                    await firebaseHelper.AddStructuredTemplate(metadata);
                }

                string script = @"
            alert('Template saved successfully!');
            if (window.parent && window.parent.closeTemplateEditor) {
                window.parent.closeTemplateEditor();
            }
        ";
                ScriptManager.RegisterStartupScript(this, GetType(), "SaveSuccess", script, true);
            }
            catch (Exception ex)
            {
                string script = $"alert('Error saving template: {ex.Message}');";
                ScriptManager.RegisterStartupScript(this, GetType(), "SaveError", script, true);
            }
        }
    }
}