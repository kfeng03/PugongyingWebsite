using Firebase.Database;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace fyp
{
    public partial class LoginStatusLabel : System.Web.UI.UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {


            if (!IsPostBack)
            {
                if (Session["CurrentLanguage"] != null)
                {
                    string selectedLanguage = Session["CurrentLanguage"].ToString();

                    // Apply stored language settings
                    Thread.CurrentThread.CurrentCulture = new CultureInfo(selectedLanguage);
                    Thread.CurrentThread.CurrentUICulture = new CultureInfo(selectedLanguage);

                    // Persist the selected culture (e.g., in session)
                    Session["CurrentLanguage"] = selectedLanguage;

                    // Ensure the page reloads its resources.
                    var page = Page as ILocalizable;
                    page?.LocalizeContent();


                    // Optionally, you can trigger an event or method in the master page to refresh content
                    var masterPage = (Site1)Page.Master;
                    masterPage?.LoadResources(); // Assuming LoadResources method is public and reloads resources
                }

                // Check if the user is logged in by checking session data
                if (Session["FirebaseUID"] != null)
                {
                    // User is logged in
                    //btnAdmin.Visible = false;
                    btnLogout.Visible = true;
                    btnLogin.Visible = false;
                    btnProfile.Visible = true;
                    lblInfo.Text = "Welcome Back, ";
                    
                    // Retrieve Firebase UID and UserID from session
                    string firebaseUid = Session["FirebaseUID"]?.ToString();
                    string username = Session["User"]?.ToString();

                    if (!string.IsNullOrEmpty(firebaseUid) && !string.IsNullOrEmpty(username))
                    {

                        lblInfo.Text += username;
                    }
                    else
                    {
                        // Handle case where Firebase UID or UserID is missing
                        lblInfo.Text = "User session data is incomplete.";
                    }
                }
                else
                {
                    // User is not logged in (guest)
                    //btnAdmin.Visible = false;
                    btnLogout.Visible = false;
                    btnLogin.Visible = true;
                    btnProfile.Visible = false;
                    lblInfo.Text = "Login to enjoy all features.";

                }
            }
        }

        protected void btnLogout_Click(object sender, EventArgs e)
        {
            // Clear all session data
            Session.Clear(); // Or use Session.Abandon() to terminate the session
            Response.Cookies.Clear();

            // Redirect to the Google login page
            Response.Redirect("~/OMTS_Pages/OMTS_AM/AM_LoginGoogle.aspx");
        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            // Redirect to the Google login page
            Response.Redirect("~/OMTS_Pages/OMTS_AM/AM_LoginGoogle.aspx");
        }


        protected void btnProfile_Click(object sender, EventArgs e)
        {
            // Redirect to the login page
            Response.Redirect("~/OMTS_Pages/OMTS_AM/AM_UpdatePersonalInfo.aspx");
        }


    }
}