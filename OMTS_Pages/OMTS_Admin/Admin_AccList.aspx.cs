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
    public partial class AccList : System.Web.UI.Page, ILocalizable
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

        private bool IsUserLoggedIn()
        {
            // Check if the user is logged in (ADD IN ALL PAGES THAT NEED ACCESS)
            // Check if the session contains the UserID or FirebaseUID
            return Session["UserID"] != null || Session["FirebaseUID"] != null;
        }
        protected async void Page_Load(object sender, EventArgs e)
        {


            // Check if the user is logged in (ADD IN ALL PAGES THAT NEED ACCESS)
            if (!IsUserLoggedIn())
            {
                // Redirect to the login page if the user is not logged in
                Response.Redirect("~/OMTS_Pages/OMTS_AM/AM_LoginGoogle.aspx", false);
                Context.ApplicationInstance.CompleteRequest(); // Prevents further processing
                return;
            }

            
                // Set the page culture on load.
            if (!IsPostBack)
            {
                LoadResources();
                Page.DataBind();
                BindAccountList();

                // Read the userId from the query string
                string userId = Request.QueryString["userId"];
                FirebaseHelper firebaseHelper = new FirebaseHelper();
                User user = await firebaseHelper.GetUserById(userId);

                if (!string.IsNullOrEmpty(userId))
                {
                    // Store the userId in a hidden field or a server-side variable
                    lblEditUserId.Text = userId; 
                    lblEditUsername.Text = user.Username; 
                    lblEditHP.Text = user.PhoneNumber;
                    lblEditEmail.Text = user.Email;

                }

                // Check if the popup query string parameter exists
                string popup = Request.QueryString["popup"];
                if (popup == "1")
                {
                    // Register a startup script to open the pop-up
                    string script = "$('.overlay').fadeIn(); $('.popup').fadeIn();";
                    ClientScript.RegisterStartupScript(this.GetType(), "OpenPopup", script, true);
                }
            }

        }

        private async void BindAccountList()
        {
            FirebaseHelper firebaseHelper = new FirebaseHelper();
            var users = await firebaseHelper.GetUsers();

            rptAccounts.DataSource = users;
            rptAccounts.DataBind();
        }

        protected async void btnResetPassword_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            string userId = btn.CommandArgument;

            FirebaseHelper firebaseHelper = new FirebaseHelper();
            var user = await firebaseHelper.GetUserById(userId);

            if (user != null)
            {
                await firebaseHelper.SendPasswordResetEmailAsync(user.Email);
                ShowSuccessMessage("Password reset email sent.");
            }
            else
            {
                ShowErrorMessage("User not found.");
            }
        }

        protected async void btnDisableAccount_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            string userId = btn.CommandArgument;

            FirebaseHelper firebaseHelper = new FirebaseHelper();
            var user = await firebaseHelper.GetUserById(userId);

            if (user != null)
            {
                await firebaseHelper.DisableUserAccount(userId, user.FirebaseUid);
                ShowSuccessMessage("Account disabled successfully.");
                BindAccountList(); // Refresh the list
            }
            else
            {
                ShowErrorMessage("User not found.");
            }
        }

        protected async void btnEnableAccount_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            string userId = btn.CommandArgument;

            FirebaseHelper firebaseHelper = new FirebaseHelper();
            var user = await firebaseHelper.GetUserById(userId);

            if (user != null)
            {
                await firebaseHelper.EnableUserAccount(userId, user.FirebaseUid);
                ShowSuccessMessage("Account enabled successfully.");
                BindAccountList(); // Refresh the list
            }
            else
            {
                ShowErrorMessage("User not found.");
            }
        }

        //private string _userID;
        //protected async void btnEditRole_Click(object sender, EventArgs e)
        //{



        //    _userID = Request.QueryString["BadgeId"];
        //    if (!string.IsNullOrEmpty(_userID))
        //    {
        //        lblEditUserId.Text = _userID;
        //    }
        //}

        protected async void btnSaveRole_Click(object sender, EventArgs e)
        {
            string userId = Request.QueryString["userId"];
            string newRole = ddlEditRole.SelectedValue.ToString();

            FirebaseHelper firebaseHelper = new FirebaseHelper();
            var user = await firebaseHelper.GetUserById(userId);

            if (user != null)
            {
                user.Role = newRole;
                await firebaseHelper.UpdateUserPersonalInfo(user); // Use the corrected method
                ShowSuccessMessage("Role updated successfully.");
                BindAccountList(); // Refresh the list
            }
            else
            {
                ShowErrorMessage("User not found.");
            }

            // Close the pop-up
            string script = "$('.overlay').fadeOut(); $('.popup').fadeOut();";
            ClientScript.RegisterStartupScript(this.GetType(), "ClosePopup", script, true);
        }

        protected void rptAccounts_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            // Check if the item is a data item (not a header or footer)
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                // Get the data item bound to the current row
                var user = e.Item.DataItem as User; // Replace "User" with your data model type

                // Find the buttons in the current row
                Button btnDisableAccount = e.Item.FindControl("btnDisableAccount") as Button;
                Button btnEnableAccount = e.Item.FindControl("btnEnableAccount") as Button;
                Button btnResetPassword = e.Item.FindControl("btnResetPassword") as Button;

                // Set visibility based on the user's status
                if (user != null)
                {
                    btnDisableAccount.Visible = user.AccountStatus == "enabled"; // Show Disable button if status is "enabled"
                    btnEnableAccount.Visible = user.AccountStatus == "disabled"; // Show Enable button if status is "disabled"
                    btnResetPassword.Visible = !string.IsNullOrWhiteSpace(user.Email);
                }
            }
        }
        private void ShowSuccessMessage(string message)
        {
            string script = $"alert('{message}');";
            ClientScript.RegisterStartupScript(this.GetType(), "Popup", script, true);
        }

        private void ShowErrorMessage(string message)
        {
            string script = $"alert('{message}');";
            ClientScript.RegisterStartupScript(this.GetType(), "Popup", script, true);
        }

        private void LoadResources()
        {

            // General Settings heading.
            litUserAccounts.Text = GetGlobalResourceObject("Resources", "Heading_ListOfUserAccounts")?.ToString();

            // Buttons and labels.
            btnAccMgmt.Text = GetGlobalResourceObject("Resources", "SideBtnAM")?.ToString();
            btnCustom.Text = GetGlobalResourceObject("Resources", "SideBtnWC")?.ToString();
            btnCertManager.Text = GetGlobalResourceObject("Resources", "SideBtnTM")?.ToString();
        }
    }
}