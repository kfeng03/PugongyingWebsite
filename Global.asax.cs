using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace fyp
{
    public class Global : HttpApplication
    {
        private static bool _settingsPreloaded = false;
        private static readonly object _lockObject = new object();

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            // 允许所有主机头访问
            if (Context.Request.Url.Host.Contains("ngrok") ||
                Context.Request.Headers["X-Original-Host"] != null)
            {
                // 跳过主机头验证
            }

            // Ensure settings are loaded before processing request
            EnsureSettingsPreloaded();

            HttpContext context = HttpContext.Current;

            // Check if we have cached settings with a language preference
            if (Site1.cachedSettings != null && !string.IsNullOrEmpty(Site1.cachedSettings.SystemLanguage))
            {
                string language = Site1.cachedSettings.SystemLanguage;
                Thread.CurrentThread.CurrentCulture = new CultureInfo(language);
                Thread.CurrentThread.CurrentUICulture = new CultureInfo(language);
                // Store in session for future requests
                if (context.Session != null)
                    context.Session["CurrentLanguage"] = language;
            }
            else if (context.Session != null && context.Session["CurrentLanguage"] != null)
            {
                // Fallback to session if cached settings not available yet
                string language = context.Session["CurrentLanguage"].ToString();
                Thread.CurrentThread.CurrentCulture = new CultureInfo(language);
                Thread.CurrentThread.CurrentUICulture = new CultureInfo(language);
            }
            else
            {
                // Final fallback to default language
                Thread.CurrentThread.CurrentCulture = new CultureInfo("en");
                Thread.CurrentThread.CurrentUICulture = new CultureInfo("en");
            }
        }

        protected void Application_Start(object sender, EventArgs e)
        {
            // Initialize Firebase
            //string pathToServiceAccount = HttpContext.Current.Server.MapPath("~/App_Data/fyp-omts-firebase-adminsdk-fbsvc-ff309fe44b.json");
            string pathToServiceAccount = HttpContext.Current.Server.MapPath("~/App_Data/pgy-omts-firebase-adminsdk-fbsvc-3e8c3d5cc9.json");

            FirebaseApp.Create(new AppOptions()
            {
                Credential = GoogleCredential.FromFile(pathToServiceAccount),
            });

            // Preload site settings at application start
            PreloadSiteSettings().GetAwaiter().GetResult();
        }

        private void EnsureSettingsPreloaded()
        {
            if (!_settingsPreloaded)
            {
                lock (_lockObject)
                {
                    if (!_settingsPreloaded)
                    {
                        try
                        {
                            // Try to load settings synchronously with timeout
                            var task = PreloadSiteSettings();
                            if (task.Wait(TimeSpan.FromSeconds(3)))
                            {
                                _settingsPreloaded = true;
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error ensuring settings preloaded: {ex.Message}");
                        }
                    }
                }
            }
        }

        private static async Task PreloadSiteSettings()
        {
            try
            {
                // Create SiteSettingsHelper instance
                SiteSettingsHelper settingsHelper = new SiteSettingsHelper();

                // Load site settings first
                Site1.cachedSettings = await settingsHelper.GetSiteSettings();

                // Then get the custom CSS styles
                Site1.cachedCustomStyles = await settingsHelper.GetSiteStylesCss();

                // Mark as loaded
                Site1.customStylesLoaded = true;
                _settingsPreloaded = true;

                System.Diagnostics.Debug.WriteLine("Preloaded site settings at application start");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error preloading site settings: {ex.Message}");
                // Don't set the flags so we'll try again
            }
        }
    }
}