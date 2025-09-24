using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Threading.Tasks;

namespace fyp
{
    public partial class Site1 : System.Web.UI.MasterPage
    {
        // These should be static to be shared across all instances
        public static bool customStylesLoaded = false;
        public static string cachedCustomStyles = null;
        public static SiteSettings cachedSettings = null;
        // Add a timestamp to track when we last loaded styles
        private static DateTime lastStyleLoadTime = DateTime.MinValue;
        // Define a refresh interval (e.g., 5 minutes)
        private static readonly TimeSpan StyleRefreshInterval = TimeSpan.FromMinutes(5);

        protected void Page_Init(object sender, EventArgs e)
        {
            // Set culture as early as possible
            SetCultureFromSettings();

            // Load settings synchronously if not loaded yet
            EnsureSettingsLoaded();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            // Set the culture again to make sure it's applied
            SetCultureFromSettings();

            // Set the page culture on load.
            if (!IsPostBack)
            {
                LoadResources();
                Page.DataBind();
                // Check if the user is an Admin and show/hide the Admin Settings link
                linkAdminSettings.Visible = IsUserAdmin();
            }

            // Check if we need to refresh the styles (first load or cache expired)
            bool needStyleRefresh = !customStylesLoaded ||
                                   (DateTime.Now - lastStyleLoadTime) > StyleRefreshInterval;

            if (needStyleRefresh)
            {
                // Start the background task to load styles - will be used for FUTURE requests
                // This approach avoids blocking or async issues
                Task.Run(() => LoadStylesInBackground());
            }
        }

        protected void Page_PreRender(object sender, EventArgs e)
        {
            // Apply styles and settings during PreRender to ensure page structure is ready
            ApplySettingsAndStyles();
        }

        // Ensure settings are loaded synchronously if needed
        private void EnsureSettingsLoaded()
        {
            if (!customStylesLoaded || cachedSettings == null)
            {
                try
                {
                    // Load settings synchronously for the first request
                    var settingsHelper = new SiteSettingsHelper();
                    var task = Task.Run(async () =>
                    {
                        if (cachedSettings == null)
                        {
                            cachedSettings = await settingsHelper.GetSiteSettings();
                        }
                        if (string.IsNullOrEmpty(cachedCustomStyles))
                        {
                            cachedCustomStyles = await settingsHelper.GetSiteStylesCss();
                        }
                    });

                    // Wait for completion with timeout
                    if (task.Wait(TimeSpan.FromSeconds(5)))
                    {
                        customStylesLoaded = true;
                        lastStyleLoadTime = DateTime.Now;
                        System.Diagnostics.Debug.WriteLine("Settings loaded synchronously during page init");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error loading settings synchronously: {ex.Message}");
                }
            }
        }

        // Set culture from cached settings
        private void SetCultureFromSettings()
        {
            try
            {
                if (cachedSettings != null && !string.IsNullOrEmpty(cachedSettings.SystemLanguage))
                {
                    // Set the current thread culture
                    Thread.CurrentThread.CurrentCulture = new CultureInfo(cachedSettings.SystemLanguage);
                    Thread.CurrentThread.CurrentUICulture = new CultureInfo(cachedSettings.SystemLanguage);

                    // Set the session value if not already set
                    if (Session["CurrentLanguage"] == null)
                    {
                        Session["CurrentLanguage"] = cachedSettings.SystemLanguage;
                    }
                }
                else if (Session["CurrentLanguage"] != null)
                {
                    // Fallback to session
                    string language = Session["CurrentLanguage"].ToString();
                    Thread.CurrentThread.CurrentCulture = new CultureInfo(language);
                    Thread.CurrentThread.CurrentUICulture = new CultureInfo(language);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error setting culture: {ex.Message}");
                // Fallback to default
                Thread.CurrentThread.CurrentCulture = new CultureInfo("en");
                Thread.CurrentThread.CurrentUICulture = new CultureInfo("en");
            }
        }

        // Apply cached styles and settings (called during PreRender)
        private void ApplySettingsAndStyles()
        {
            try
            {
                // Apply cached styles if available
                if (!string.IsNullOrEmpty(cachedCustomStyles))
                {
                    // Create a new style element
                    HtmlGenericControl style = new HtmlGenericControl("style");
                    style.Attributes.Add("type", "text/css");
                    style.InnerHtml = cachedCustomStyles;

                    // Add it to the header
                    Page.Header.Controls.Add(style);
                }

                // Apply cached settings if available
                if (cachedSettings != null)
                {
                    // Set organization name and logo in the header if they exist
                    if (!string.IsNullOrEmpty(cachedSettings.OrganizationName))
                    {
                        litOrgName.Text = cachedSettings.OrganizationName;
                    }
                    else
                    {
                        litOrgName.Text = "Your Organization"; // Default
                    }

                    if (!string.IsNullOrEmpty(cachedSettings.LogoUrl))
                    {
                        imgLogo.ImageUrl = cachedSettings.LogoUrl;
                        imgLogo.Visible = true;
                    }
                    else
                    {
                        imgLogo.Visible = false;
                    }
                }
                else
                {
                    // Default values if no cached settings
                    litOrgName.Text = "Your Organization";
                    imgLogo.Visible = false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error applying cached styles: {ex.Message}");
                // Use defaults if there's an error
                litOrgName.Text = "Your Organization";
                imgLogo.Visible = false;
            }
        }

        // Load styles in the background - won't block the page
        private async Task LoadStylesInBackground()
        {
            try
            {
                if (!customStylesLoaded) // Double-check to avoid concurrent loads
                {
                    customStylesLoaded = true; // Mark as loaded

                    // Create SiteSettingsHelper instance
                    SiteSettingsHelper settingsHelper = new SiteSettingsHelper();

                    // Get the custom CSS styles
                    cachedCustomStyles = await settingsHelper.GetSiteStylesCss();

                    // Load site settings
                    cachedSettings = await settingsHelper.GetSiteSettings();

                    // Update the timestamp
                    lastStyleLoadTime = DateTime.Now;

                    System.Diagnostics.Debug.WriteLine("Background style loading complete");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in background style loading: {ex.Message}");
                // Reset the flag so we can try again next time
                customStylesLoaded = false;
            }
        }

        // Public method to force refresh styles (can be called from admin page)
        public static void ForceStyleRefresh()
        {
            // Reset the flags to force a refresh on next request
            customStylesLoaded = false;
            lastStyleLoadTime = DateTime.MinValue;
        }

        // Method to check if the current user is an Admin
        private bool IsUserAdmin()
        {
            // Replace this with your actual logic to check the user's role
            // For example, you might check a session variable or call a method from your FirebaseHelper
            if (Session["UserRole"] != null && Session["UserRole"].ToString() == "Admin")
            {
                return true;
            }
            return false;
        }

        void SetCulture(string language)
        {
            CultureInfo cultureInfo = new CultureInfo(language);
            Thread.CurrentThread.CurrentCulture = cultureInfo;
            Thread.CurrentThread.CurrentUICulture = cultureInfo;
        }

        public void LoadResources()
        {
            try
            {
                // Navbar
                linkAdminSettings.Text = GetGlobalResourceObject("Resources", "Nav_AdminSettings")?.ToString() ?? "Admin Settings";
                linkCourses.Text = GetGlobalResourceObject("Resources", "Nav_Courses")?.ToString() ?? "Courses";
                linkEvents.Text = GetGlobalResourceObject("Resources", "Nav_Events")?.ToString() ?? "Events";
                linkAchievements.Text = GetGlobalResourceObject("Resources", "Nav_Achievements")?.ToString() ?? "Achievements";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading resources: {ex.Message}");
                // Set fallback values
                linkAdminSettings.Text = "Admin Settings";
                linkCourses.Text = "Courses";
                linkEvents.Text = "Events";
                linkAchievements.Text = "Achievements";
            }
        }

        protected void imgLogo_Click(object sender, ImageClickEventArgs e)
        {
            // Clear all session data
            Session.Clear(); // Or use Session.Abandon() to terminate the session
            Response.Cookies.Clear();

           
            // Redirect to home page
            Response.Redirect("../../Portfolio_Pages/Homepage.aspx");
        }
    }
}