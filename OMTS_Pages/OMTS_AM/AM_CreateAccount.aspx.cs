using FirebaseAdmin.Auth;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace fyp
{
    public partial class AM_CreateAccount : System.Web.UI.Page, ILocalizable
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
            litRegisterHeading.Text = GetGlobalResourceObject("Resources", "Link_CreateAcc")?.ToString();
            litRegisterSubtext.Text = GetGlobalResourceObject("Resources", "Register_Info")?.ToString();


            // Hyperlinks text.
            lnkLogin.Text = GetGlobalResourceObject("Resources", "Link_Login")?.ToString();


            // Buttons and labels.
           
            btnRegister.Text = GetGlobalResourceObject("Resources", "Button_Register")?.ToString();
        }

    }
}