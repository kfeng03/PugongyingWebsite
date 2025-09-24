using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace fyp
{
    public partial class verifyOTP : System.Web.UI.Page, ILocalizable
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

            // Heading
            litOtpHeading.Text = GetGlobalResourceObject("Resources", "Heading_Proceed")?.ToString();
            litOtpSubtext.Text = string.Format(GetGlobalResourceObject("Resources", "Subtext_Otp_Verification")?.ToString(), "012***45");

            // Hyperlinks text
            lnkResendOtp.Text = GetGlobalResourceObject("Resources", "Link_Otp_ResendCode")?.ToString();

            // Buttons and labels
            litPinNumber.Text = GetGlobalResourceObject("Resources", "Label_Otp_PinNumber")?.ToString();
            btnVerify.Text = GetGlobalResourceObject("Resources", "Button_Otp_Verify")?.ToString();
        }
    }
}
