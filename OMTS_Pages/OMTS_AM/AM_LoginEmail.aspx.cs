using FirebaseAdmin.Auth;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.WebControls;
using Newtonsoft.Json.Linq;
using System.Data.SqlClient;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Text;


namespace fyp
{
    public partial class LoginEmail : System.Web.UI.Page, ILocalizable
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
        protected void Page_Load(object sender, EventArgs e)
        {

            // Set the page culture on load.
            if (!IsPostBack)
            {
                LoadResources();
                Page.DataBind();
            }

        }


        private void LoadResources()
        {


            // heading.
            litLoginHeading.Text = GetGlobalResourceObject("Resources", "Login2_Heading")?.ToString();
            litLoginSubtext.Text = GetGlobalResourceObject("Resources", "Login2_Info")?.ToString();


            // Hyperlinks text.
            lnkPWreset.Text = GetGlobalResourceObject("Resources", "Link_ForgotPassword")?.ToString();
            //lnkLoginHP.Text = GetGlobalResourceObject("Resources", "Link_LoginHP")?.ToString();
            //lnkCreate.Text = GetGlobalResourceObject("Resources", "Link_CreateAcc")?.ToString();

            // Buttons and labels.
            litEmail.Text = GetGlobalResourceObject("Resources", "Label_Email")?.ToString();
            litPw.Text = GetGlobalResourceObject("Resources", "Label_Pw")?.ToString();
            btnLogin.Text = GetGlobalResourceObject("Resources", "Button_Login")?.ToString();
        }

        protected async void btnLogin_Click(object sender, EventArgs e)
        {
            string email = txtEmail.Text;
            string password = txtPw.Text;
            bool loginResult = await LoginWithEmailPassword(email, password);

            if (loginResult)
            {
                // Fetch Firebase UID from session
                string firebaseUid = Session["FirebaseUID"]?.ToString();

                if (!string.IsNullOrEmpty(firebaseUid))
                {
                    // Use FirebaseHelper to fetch the user's role and designated user ID
                    FirebaseHelper firebaseHelper = new FirebaseHelper();
                    var (role, userId, username) = await firebaseHelper.GetUserByFirebaseUid(firebaseUid);

                    if (role != null && userId != null)
                    {
                        // Store additional user details in session
                        Session["UserID"] = userId; // Designated user ID (e.g., U001, U002)
                        Session["UserRole"] = role; // User role
                        Session["User"] = username; // User role
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("User details not found in Firebase Realtime Database.");
                    }
                }

                // Redirect after storing all session data

                Response.Redirect("AM_UpdatePersonalInfo.aspx", false);
                Context.ApplicationInstance.CompleteRequest(); // Prevents further processing
            }
            else
            {
                string script = "alert('Login failed. Please check credentials.');";
                ClientScript.RegisterStartupScript(this.GetType(), "Popup", script, true);
            }
        }

        private async Task<bool> LoginWithEmailPassword(string email, string password)
        {
            using (HttpClient client = new HttpClient())
            {
                string firebaseAuthUrl = "https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key=AIzaSyD7OIF-dnrsre_YaDVuWjw527whdmaSoi0";

                var requestData = new
                {
                    email = email,
                    password = password,
                    returnSecureToken = true
                };

                string jsonData = JsonConvert.SerializeObject(requestData);
                HttpContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(firebaseAuthUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    JObject data = JObject.Parse(json);
                    string fbID = data["localId"].ToString();
                    string idToken = data["idToken"].ToString();

                    // Store in session
                    Session["FirebaseUID"] = fbID;
                    return true;
                }
                else
                {
                    string errorJson = await response.Content.ReadAsStringAsync();
                    // Log the error or handle it as needed
                    System.Diagnostics.Debug.WriteLine("Login failed: " + errorJson);
                    return false;
                }
            }
        }
    }
}