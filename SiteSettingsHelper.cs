using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace fyp
{
    public class SiteSettingsHelper
    {
        private readonly FirebaseHelper _firebaseHelper;

        public SiteSettingsHelper()
        {
            _firebaseHelper = new FirebaseHelper();
        }

        /// <summary>
        /// Gets the site settings from Firebase
        /// </summary>
        /// <returns>SiteSettings object or null if no settings exist</returns>
        public async Task<SiteSettings> GetSiteSettings()
        {
            try
            {
                // Attempt to fetch site settings from Firebase
                var settingsData = await _firebaseHelper.GetSiteSettingsData();

                if (settingsData != null)
                {
                    return settingsData;
                }

                // Return null if no settings exist
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting site settings: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Saves the site settings to Firebase
        /// </summary>
        /// <param name="settings">The settings to save</param>
        /// <returns>True if successful, false otherwise</returns>
        public async Task<bool> SaveSiteSettings(SiteSettings settings)
        {
            try
            {
                // Save the settings to Firebase
                await _firebaseHelper.SaveSiteSettingsData(settings);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving site settings: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Gets the CSS styles for the current site settings
        /// </summary>
        /// <returns>CSS string</returns>
        public async Task<string> GetSiteStylesCss()
        {
            try
            {
                // Get the site settings
                var settings = await GetSiteSettings();

                if (settings == null)
                {
                    // Return default styles if no settings exist
                    return GetDefaultCss();
                }

                // Build CSS based on site settings
                var css = new System.Text.StringBuilder();

                // Font family
                if (!string.IsNullOrEmpty(settings.Font))
                {
                    css.AppendLine($"body, .main-content {{ font-family: '{settings.Font}', sans-serif; }}");
                }

                // Theme - with improved dark mode handling
                switch (settings.Theme)
                {
                    case "theme1": // Light theme
                                   // Basic light theme
                        css.AppendLine("body { background-color: #f8f8f8; }");
                        css.AppendLine(".nav-bar { background-color: whitesmoke; }");
                        css.AppendLine(".main-content { background-color: white; }");
                        css.AppendLine(".form-buttons .btn, .btn { background-color: #444; color: white; }");
                        css.AppendLine(".tab { background-color: white; }");
                        break;

                    case "theme2": // Dark theme
                                   // Base dark theme styles
                        css.AppendLine("body { background-color: #222; color: #f1f1f1; }");
                        css.AppendLine(".nav-bar { background-color: #111;}");
                        css.AppendLine(".nav-options { color: white; }");
                        css.AppendLine(".lbl, .page-header, .edit-role-icon{color:white !important;}");
                        css.AppendLine(".subtext{color:whitesmoke !important;}");
                        css.AppendLine(".nav-options a { color: white !important; }"); // This ensures all links are white
                        css.AppendLine(".nav-options a:hover { color: #ccc !important; }"); // Optional hover effect
                        css.AppendLine(".nav-options a.active { color: white !important; }"); 
                        css.AppendLine(".nav-options a.active::after {background: white; }");
                        css.AppendLine(".main-content { background-color: #333 ; color: #f1f1f1; }");
                        css.AppendLine(".sidebar { background-color: #222 !important; ; }");
                        css.AppendLine(".sidebar-btn { background-color: #222 !important; ; color: white !important; ; }");
                        css.AppendLine(".form-buttons .btn, .btn { background-color: white !important; ; color: #555 !important; }");
                        css.AppendLine(".btn btn-danger{ background-color: darkred !important; color: white !important; }");
                        css.AppendLine(".tab, .popup, .popup1, .popup2, .summary-panel, .badgePopup , .pnlPasswordVerification, .pnlSecurityForm,.login-container,.profile-card,.achievement-panel { background-color: #444; color: white; }");
                        css.AppendLine(".sidebar-btn:hover {background-color: #EAEDEE !important; ;color:black !important; }");
                        css.AppendLine(".sidebar-btn.active{border-left: 5px solid white !important; ; color: white !important; ;}");
                        // Input styles
                        css.AppendLine("input{ background-color: #444 !important; color: white !important; border-color: #555 !important; }");
                        css.AppendLine(".form-control, .text-input, .text-input:disabled{ background-color: #444 !important; color: white !important; border-color: #555; }");
                        css.AppendLine(".form-table tr td { color: #f1f1f1; }");

                        // Table styles
                        css.AppendLine("th { background-color: #444; color: white; }");
                        css.AppendLine("tr:hover  {background:none !important; }");


                        // Message and notification styles
                        css.AppendLine(".message { background-color: #444; color: white; border-color: #555; }");
                        css.AppendLine(".message-success { background-color: #1b5e20; }");
                        css.AppendLine(".message-error { background-color: #b71c1c; }");
                        css.AppendLine(".message-warning { background-color: #e65100; }");

                        // Links
                        css.AppendLine("a { color: #80cbc4; }");
                        css.AppendLine("a:visited { color: #9fa8da; }");

                        // Additional dark mode adjustments for specific components
                        css.AppendLine(".profile-pic-container { background-color: #444; }");
                        css.AppendLine(".theme-preview { border-color: #555; }");
                        css.AppendLine(".alternate-link { color: #80cbc4; }");
                        break;

                    default:
                        // Default light theme
                        css.AppendLine("body { background-color: #f8f8f8; }");
                        css.AppendLine(".nav-bar { background-color: whitesmoke; }");
                        css.AppendLine(".main-content { background-color: white; }");
                        css.AppendLine(".form-buttons .btn, .btn { background-color: #444; color: white; }");
                        css.AppendLine(".tab { background-color: white; }");
                        break;
                }


                // Add logo size control
                css.AppendLine(".nav-bar .logo img { max-height: 60px; max-width: 200px; object-fit: contain; vertical-align: middle; }");
                css.AppendLine(".nav-bar .logo { display: flex; align-items: center; gap: 10px; }");

                return css.ToString();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error generating CSS: {ex.Message}");
                return GetDefaultCss();
            }
        }
        /// <summary>
        /// Gets the default CSS styles
        /// </summary>
        /// <returns>Default CSS string</returns>
        private string GetDefaultCss()
        {
            var css = new System.Text.StringBuilder();
            css.AppendLine("body { font-family: 'Arial', sans-serif; background-color: #f8f8f8; }");
            css.AppendLine(".nav-bar { background-color: white; }");
            css.AppendLine(".main-content { background-color: white; }");
            css.AppendLine(".btn { background-color: #444; }");
            return css.ToString();
        }
    }
}