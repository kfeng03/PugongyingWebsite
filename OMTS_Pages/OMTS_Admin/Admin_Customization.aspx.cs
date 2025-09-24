using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace fyp
{
    public partial class WebForm1 : Page, ILocalizable
    {
        private SiteSettingsHelper siteSettingsHelper;
        private CloudinaryHelper cloudinaryHelper;

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

        protected void Page_Load(object sender, EventArgs e)
        {
            // Initialize helpers
            siteSettingsHelper = new SiteSettingsHelper();
            cloudinaryHelper = new CloudinaryHelper();

            // Check if the user is authenticated and has admin role
            if (!IsUserAdmin())
            {
                Response.Redirect("~/OMTS_Pages/OMTS_AM/AM_LoginHP.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            // Set the page culture on load.
            if (!IsPostBack)
            {
                LoadResources();
                Page.DataBind();
                LoadSiteSettings();
            }

            // Check if we need to redirect back to customization
            if (!IsPostBack && Request.QueryString["returnToCustomization"] == "true")
            {
                Response.Redirect("~/OMTS_Pages/OMTS_Admin/Admin_Customization.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
            }
        }

        private bool IsUserAdmin()
        {
            // Check if user is logged in and has admin role
            string userRole = Session["UserRole"]?.ToString();
            return userRole == "Admin";
        }

        private async void LoadSiteSettings()
        {
            try
            {
                // Load site settings from Firebase
                SiteSettings settings = await siteSettingsHelper.GetSiteSettings();

                // Set form values from settings
                if (settings != null)
                {
                    // Set organization name
                    txtOrgName.Text = settings.OrganizationName;

                    // Set logo preview
                    if (!string.IsNullOrEmpty(settings.LogoUrl))
                    {
                        logoPreview.ImageUrl = settings.LogoUrl;
                    }

                    // Set theme selection
                    if (!string.IsNullOrEmpty(settings.Theme))
                    {
                        hdnSelectedTheme.Value = settings.Theme;
                        ScriptManager.RegisterStartupScript(this, GetType(), "selectTheme",
                            $"document.getElementById('{settings.Theme}').classList.add('theme-selected');", true);
                    }

                    // Set font selection
                    //if (!string.IsNullOrEmpty(settings.Font))
                    //{
                    //    ddlFont.SelectedValue = settings.Font;
                    //}


                    // Set language dropdown
                    if (!string.IsNullOrEmpty(settings.SystemLanguage))
                    {
                        ddlLanguage.SelectedValue = settings.SystemLanguage;
                    }
                    else
                    {
                        // Default to English
                        ddlLanguage.SelectedValue = "en";
                    }
                    // Set max points per level
                    txtMaxPointsPerLevel.Text = settings.MaxPointsPerLevel.ToString();
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"Error loading site settings: {ex.Message}", MessageType.Error);
            }
        }

        protected async void btnSaveGeneralSettings_Click(object sender, EventArgs e)
        {
            try
            {
                // Get current settings or create new if none exist
                SiteSettings settings = await siteSettingsHelper.GetSiteSettings() ?? new SiteSettings();

                // Save the old theme to check if it changed
                string oldTheme = settings.Theme;

                // Update organization name
                settings.OrganizationName = txtOrgName.Text.Trim();

                // Upload logo if a new file is selected
                if (fuLogo.HasFile)
                {
                    // Upload with resizing
                    string logoUrl = cloudinaryHelper.UploadFile(
                        fuLogo.PostedFile,
                        "organization_logos");

                    settings.LogoUrl = logoUrl;
                }

                // Update theme
                bool themeChanged = false;
                if (!string.IsNullOrEmpty(hdnSelectedTheme.Value) && oldTheme != hdnSelectedTheme.Value)
                {
                    settings.Theme = hdnSelectedTheme.Value;
                    themeChanged = true;
                }

                // Update font, colors, languages, etc.
                //settings.Font = ddlFont.SelectedValue;

                // Update system language
                settings.SystemLanguage = ddlLanguage.SelectedValue;
                // Update max points per level
                if (int.TryParse(txtMaxPointsPerLevel.Text, out int maxPoints) && maxPoints > 0)
                {
                    settings.MaxPointsPerLevel = maxPoints;
                }
                else
                {
                    // Use default value if invalid input
                    settings.MaxPointsPerLevel = 100;
                    txtMaxPointsPerLevel.Text = "100";
                }
                // Save all settings
                await siteSettingsHelper.SaveSiteSettings(settings);
                // Update the session language
                Session["CurrentLanguage"] = settings.SystemLanguage;

                // Update the thread culture
                Thread.CurrentThread.CurrentCulture = new CultureInfo(settings.SystemLanguage);
                Thread.CurrentThread.CurrentUICulture = new CultureInfo(settings.SystemLanguage);

                // Force refresh the styles for future requests
                Site1.ForceStyleRefresh();

                // Also update the static cache with the new settings
                Site1.cachedSettings = settings;
                Site1.cachedCustomStyles = await siteSettingsHelper.GetSiteStylesCss();
                Site1.customStylesLoaded = true;

                // Show success message
                ShowMessage("Settings saved successfully.", MessageType.Success);

                Response.Redirect("~/OMTS_Pages/OMTS_Admin/Admin_AccList.aspx?returnToCustomization=true", false);
                Context.ApplicationInstance.CompleteRequest();
            }
            catch (Exception ex)
            {
                ShowMessage($"Error saving settings: {ex.Message}", MessageType.Error);
            }
        }
        protected void ddlFont_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Automatically show the save indicator when font changes
            ScriptManager.RegisterStartupScript(this, GetType(), "showFontSaveIndicator",
                "document.getElementById('fontSaveIndicator').style.display = 'inline';", true);
        }

        // New method for language dropdown selection change
        protected void ddlLanguage_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Show the save indicator
            ScriptManager.RegisterStartupScript(this, GetType(), "showLanguageSaveIndicator",
                "document.getElementById('languageSaveIndicator').style.display = 'inline';", true);
        }


        private void LoadResources()
        {
            // General Settings heading.
            litGeneralSettings.Text = GetGlobalResourceObject("Resources", "Heading_GeneralSettings")?.ToString();

            // Content Customization heading.
            litContentCus.Text = GetGlobalResourceObject("Resources", "Heading_ContentCus")?.ToString();

            // Hyperlinks text.
            lnkCourse.Text = GetGlobalResourceObject("Resources", "Hyperlink_Courses")?.ToString();
            lnkEvents.Text = GetGlobalResourceObject("Resources", "Hyperlink_Events")?.ToString();
            lnkAchievement.Text = GetGlobalResourceObject("Resources", "Hyperlink_Achievements")?.ToString();

            // Buttons and labels.
            btnAccMgmt.Text = GetGlobalResourceObject("Resources", "SideBtnAM")?.ToString();
            btnCustom.Text = GetGlobalResourceObject("Resources", "SideBtnWC")?.ToString();
            btnCertManager.Text = GetGlobalResourceObject("Resources", "SideBtnTM")?.ToString();
            //btnSetDefaultLang.Text = GetGlobalResourceObject("Resources", "Button_SetDefault")?.ToString();
            btnSaveGeneralSettings.Text = GetGlobalResourceObject("Resources", "Button_SaveChanges")?.ToString() ?? "Save Changes";
        }

        private enum MessageType
        {
            Success,
            Error,
            Warning,
            Info
        }

        private void ShowMessage(string message, MessageType type)
        {
            lblMessage.Text = message;
            pnlMessage.Visible = true;

            switch (type)
            {
                case MessageType.Success:
                    pnlMessage.CssClass = "message message-success";
                    break;
                case MessageType.Error:
                    pnlMessage.CssClass = "message message-error";
                    break;
                case MessageType.Warning:
                    pnlMessage.CssClass = "message message-warning";
                    break;
                case MessageType.Info:
                    pnlMessage.CssClass = "message message-info";
                    break;
            }
        }
    }
}