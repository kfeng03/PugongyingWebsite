using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Text;

namespace fyp
{
    public partial class AM_LoginGoogle : System.Web.UI.Page, ILocalizable
    {
        private FirebaseHelper firebaseHelper = new FirebaseHelper();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadResources();
                Page.DataBind();
            }
        }

        public void LocalizeContent()
        {
            LoadResources(); // This will reload the resources for the page
        }

        private void LoadResources()
        {
            // Set page labels and controls from resource files if needed
            litLoginHeading.Text = GetGlobalResourceObject("Resources", "Login_Google_Heading")?.ToString() ?? "Login with Google";
            litLoginSubtext.Text = GetGlobalResourceObject("Resources", "Login_Google_Info")?.ToString() ?? "Sign in quickly and securely with your Google account";

            //lnkLoginPhone.Text = GetGlobalResourceObject("Resources", "Link_LoginPhone")?.ToString() ?? "Login with Phone Number";
            lnkLoginEmail.Text = GetGlobalResourceObject("Resources", "Link_LoginEmail")?.ToString() ?? "Login with Email";
        }

        protected void Page_PreInit(object sender, EventArgs e)
        {
            if (Session["CurrentLanguage"] != null)
            {
                string selectedLanguage = Session["CurrentLanguage"].ToString();
                Thread.CurrentThread.CurrentCulture = new CultureInfo(selectedLanguage);
                Thread.CurrentThread.CurrentUICulture = new CultureInfo(selectedLanguage);
            }
        }

        // This method will be called by AJAX when Google login is successful
        [System.Web.Services.WebMethod]
        public static string CompleteLogin(string firebaseUid, string userId, string username, string role)
        {
            try
            {
                // Store user info in session
                HttpContext.Current.Session["FirebaseUID"] = firebaseUid;
                HttpContext.Current.Session["UserID"] = userId;
                HttpContext.Current.Session["UserRole"] = role;
                HttpContext.Current.Session["User"] = username;

                // Log successful login
                System.Diagnostics.Debug.WriteLine($"User logged in successfully: {username} (ID: {userId}, Role: {role})");

                return JsonConvert.SerializeObject(new
                {
                    success = true,
                    role = role,
                    userId = userId,
                    username = username
                });
            }
            catch (Exception ex)
            {
                // Log error
                System.Diagnostics.Debug.WriteLine($"Error in CompleteLogin: {ex.Message}");

                return JsonConvert.SerializeObject(new
                {
                    success = false,
                    error = ex.Message
                });
            }
        }

        // Optional: Method to handle logout (if needed)
        [System.Web.Services.WebMethod]
        public static string LogoutUser()
        {
            try
            {
                // Clear all session variables
                HttpContext.Current.Session.Clear();
                HttpContext.Current.Session.Abandon();

                return JsonConvert.SerializeObject(new { success = true });
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new
                {
                    success = false,
                    error = ex.Message
                });
            }
        }

        // Optional: Method to check current login status
        [System.Web.Services.WebMethod]
        public static string CheckLoginStatus()
        {
            try
            {
                string userId = HttpContext.Current.Session["UserID"]?.ToString();
                string username = HttpContext.Current.Session["User"]?.ToString();
                string role = HttpContext.Current.Session["UserRole"]?.ToString();
                string firebaseUid = HttpContext.Current.Session["FirebaseUID"]?.ToString();

                bool isLoggedIn = !string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(firebaseUid);

                return JsonConvert.SerializeObject(new
                {
                    success = true,
                    isLoggedIn = isLoggedIn,
                    userId = userId,
                    username = username,
                    role = role,
                    firebaseUid = firebaseUid
                });
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new
                {
                    success = false,
                    error = ex.Message
                });
            }
        }
    }
}