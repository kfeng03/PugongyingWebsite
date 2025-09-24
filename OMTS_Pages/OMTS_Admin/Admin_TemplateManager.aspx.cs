using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace fyp
{
    public partial class Admin_TemplateManager : System.Web.UI.Page, ILocalizable
    {
        protected void Page_PreInit(object sender, EventArgs e)
        {
            if (Session["CurrentLanguage"] != null)
            {
                string selectedLanguage = Session["CurrentLanguage"].ToString();
                Thread.CurrentThread.CurrentCulture = new CultureInfo(selectedLanguage);
                Thread.CurrentThread.CurrentUICulture = new CultureInfo(selectedLanguage);
            }
        }

        // The LocalizeContent method is defined as part of the ILocalizable interface.
        public void LocalizeContent()
        {
            LoadResources(); // This will reload the resources for the page
        }
    
    
        private FirebaseHelper firebaseHelper;
        private CloudinaryHelper cloudinaryHelper;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Initialize helpers
            firebaseHelper = new FirebaseHelper();
            cloudinaryHelper = new CloudinaryHelper();

            // Check user authorization
            if (!IsUserAuthorized())
            {
                Response.Redirect("~/OMTS_Pages/OMTS_AM/AM_LoginHP.aspx", false);
                Context.ApplicationInstance.CompleteRequest(); // Prevents further processing
                return;
            }

            if (!IsPostBack)
            {
                LoadTemplates();
            }
        }

        private bool IsUserAuthorized()
        {
            // Check if user is logged in and has appropriate role
            string userRole = Session["UserRole"]?.ToString();
            return userRole == "Admin" || userRole == "Staff";
        }

        protected void ddlTemplateType_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Reload templates with the selected filter
            LoadTemplates();
        }

        private async void LoadTemplates()
        {
            try
            {
                // Get all templates
                var templates = await firebaseHelper.GetAllTemplates();

                // Debug logging
                System.Diagnostics.Debug.WriteLine($"Total Templates Retrieved: {templates.Count}");
                foreach (var template in templates)
                {
                    System.Diagnostics.Debug.WriteLine($"Template ID: {template.TemplateId}");
                    System.Diagnostics.Debug.WriteLine($"Template URL: {template.TemplateUrl}");
                    System.Diagnostics.Debug.WriteLine($"Template Name: {template.TemplateName}");
                    System.Diagnostics.Debug.WriteLine($"Template Type: {template.TemplateType}");
                    System.Diagnostics.Debug.WriteLine($"Is Testimonial Template: {template.IsTestimonialTemplate}");
                    System.Diagnostics.Debug.WriteLine($"Is Deleted: {template.IsDeleted}");
                    System.Diagnostics.Debug.WriteLine("---");
                }

                // Filter templates based on selected type
                string selectedType = ddlTemplateType.SelectedValue;

                var filteredTemplates = templates.Where(t =>
                    !t.IsDeleted &&
                    (selectedType == "All" || t.TemplateType == selectedType)
                ).ToList();

                System.Diagnostics.Debug.WriteLine($"Filtered Templates Count: {filteredTemplates.Count}");

                // Show templates in repeater
                rptTemplates.DataSource = filteredTemplates;
                rptTemplates.DataBind();

                // Show/hide no templates message
                pnlNoTemplates.Visible = filteredTemplates.Count == 0;
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error loading templates: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Template loading error: {ex}");
            }
        }

        protected void rptTemplates_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            string templateId = e.CommandArgument.ToString();

            switch (e.CommandName)
            {
                case "SelectTestimonial":
                    SelectTestimonialTemplate(templateId);
                    break;
                case "EditTemplate":
                    EditTemplate(templateId);
                    break;
                case "DeleteTemplate":
                    DeleteTemplate(templateId);
                    break;
            }
        }

        private async void SelectTestimonialTemplate(string templateId)
        {
            try
            {
                // Set the selected template as the testimonial template
                await firebaseHelper.SetTestimonialTemplate(templateId);

                // Reload templates to reflect changes
                LoadTemplates();

                ShowSuccessMessage("Testimonial template updated successfully.");
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error selecting testimonial template: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Template selection error: {ex}");
            }
        }

        // In EditTemplate method of Admin_TemplateManager.aspx.cs:
        private async void EditTemplate(string templateId)
        {
            try
            {
                // Retrieve the full template details
                var template = await firebaseHelper.GetTemplateById(templateId);

                if (template == null)
                {
                    ShowErrorMessage("Template not found.");
                    return;
                }

                // Store template details in session for editing
                Session["EditingTemplateId"] = templateId;
                Session["EditingTemplateUrl"] = template.TemplateUrl;
                Session["EditingIsPortrait"] = template.ImageWidth < template.ImageHeight;
                Session["EditingFields"] = template.Fields; // Make sure fields are stored

                // Redirect to template editor page
                Response.Redirect("~/OMTS_Pages/OMTS_Admin/Admin_CreateTemplate.aspx?mode=edit", false);
                Context.ApplicationInstance.CompleteRequest(); // Prevents further processing
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error editing template: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Edit Template Error: {ex}");
            }
        }
        private async void DeleteTemplate(string templateId)
        {
            try
            {
                // Soft delete the template
                await firebaseHelper.SoftDeleteTemplate(templateId);

                // Reload templates
                LoadTemplates();

                ShowSuccessMessage("Template deleted successfully.");
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error deleting template: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Template deletion error: {ex}");
            }
        }

        private void ShowSuccessMessage(string message)
        {
            lblMessage.Text = message;
            pnlMessage.CssClass = "status-message alert-success";
            pnlMessage.Visible = true;
        }

        private void ShowErrorMessage(string message)
        {
            lblMessage.Text = message;
            pnlMessage.CssClass = "status-message alert-danger";
            pnlMessage.Visible = true;
        }
        private void LoadResources()
        {

            // General Settings heading.
            litHeading.Text = GetGlobalResourceObject("Resources", "SideBtnTM")?.ToString();
            litTxt.Text= GetGlobalResourceObject("Resources", "Label_NoTemplate").ToString();
            // Buttons and labels.
            btnAccMgmt.Text = GetGlobalResourceObject("Resources", "SideBtnAM")?.ToString();
            btnCustom.Text = GetGlobalResourceObject("Resources", "SideBtnWC")?.ToString();
            btnCertManager.Text = GetGlobalResourceObject("Resources", "SideBtnTM")?.ToString();
        }
    }
}