using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Newtonsoft.Json;

namespace fyp
{
    public partial class CertificateAnalyzer : System.Web.UI.Page
    {
        protected FirebaseHelper firebaseHelper;

        protected void Page_Load(object sender, EventArgs e)
        {
            firebaseHelper = new FirebaseHelper();

            if (!IsPostBack)
            {
                // Load existing templates for management
                LoadTemplates();
            }
        }

        protected async void btnOpenPopup_Click(object sender, EventArgs e)
        {
            // Validate template name
            if (string.IsNullOrEmpty(txtTemplateName.Text.Trim()))
            {
                ShowMessage("Please enter a template name.", MessageType.Error);
                return;
            }

            // Pass Template info via QueryString
            var url = $"Admin_TemplateEditorInterface.aspx" +
                      $"?name={HttpUtility.UrlEncode(txtTemplateName.Text)}" +
                      $"&orientation={rblOrientation.SelectedValue}" +
                      $"&type={rblTemplateType.SelectedValue}";

            // Register JS to open the iframe with that url
            ClientScript.RegisterStartupScript(this.GetType(), "popup",
                $"showPopup('{url}');", true);
        }

        private async void LoadTemplates()
        {
            try
            {
                var templates = await firebaseHelper.GetActiveJsonTemplates();
                // You can bind templates to a grid or list here if needed
                // For now, we'll just store them in session for reference
                Session["Templates"] = templates;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading templates: {ex.Message}");
            }
        }

        private void ShowMessage(string message, MessageType type)
        {
            messagePanel.Visible = true;
            lblMessage.Text = message;
            messagePanel.CssClass = $"message message-{type.ToString().ToLower()}";
        }

        public enum MessageType
        {
            Success,
            Error,
            Warning,
            Info
        }

        // JavaScript function to close the template editor popup
        protected void Page_PreRender(object sender, EventArgs e)
        {
            string script = @"
                function closeTemplateEditor() {
                    hidePopup();
                    // Refresh the page to show updated templates
                    window.location.reload();
                }
            ";
            ClientScript.RegisterStartupScript(this.GetType(), "CloseTemplateEditor", script, true);
        }
    }
}