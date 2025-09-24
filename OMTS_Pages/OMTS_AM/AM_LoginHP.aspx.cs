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
    public partial class AM_LoginHP : System.Web.UI.Page, ILocalizable
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
            litLoginHeading.Text = GetGlobalResourceObject("Resources", "Login1_Heading")?.ToString() ?? "Login with Phone Number";
            litLoginSubtext.Text = GetGlobalResourceObject("Resources", "Login1_Info")?.ToString() ?? "Please enter your phone number to receive an OTP for login.";
//            btnLogin.Text = GetGlobalResourceObject("Resources", "Button_Login")?.ToString() ?? "Login";
            lnkLoginEmail.Text = GetGlobalResourceObject("Resources", "Link_LoginEmail")?.ToString() ?? "Login with Email instead";
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

        // This method will be called by AJAX when login is successful
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

                return JsonConvert.SerializeObject(new { success = true, role = role });
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new { success = false, error = ex.Message });
            }
        }
    }
}