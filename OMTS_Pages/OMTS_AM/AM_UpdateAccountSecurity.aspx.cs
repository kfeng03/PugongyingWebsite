using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Firebase.Auth;
using Firebase.Database;

namespace fyp
{
    public partial class UpdateAccountSecurity : System.Web.UI.Page, ILocalizable
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

                // Fetch and display user details
                FetchAndDisplayUserDetails();

                // Configure security question dropdown
                ddlSecurityQuestion.Items.Clear();  // Clear existing items to avoid duplication
                ddlSecurityQuestion.Items.Add(new ListItem(GetGlobalResourceObject("Resources", "SecurityQuestion_1").ToString(), "1"));
                ddlSecurityQuestion.Items.Add(new ListItem(GetGlobalResourceObject("Resources", "SecurityQuestion_2").ToString(), "2"));
                ddlSecurityQuestion.Items.Add(new ListItem(GetGlobalResourceObject("Resources", "SecurityQuestion_3").ToString(), "3"));

                // Initially hide the password verification panel
                pnlPasswordVerification.Visible = false;
            }
        }

        private async void FetchAndDisplayUserDetails()
        {
            // Get the user's Designated User ID from the session
            string userId = Session["UserID"]?.ToString();

            if (!string.IsNullOrEmpty(userId))
            {
                // Fetch user details from Firebase using Designated User ID
                FirebaseHelper firebaseHelper = new FirebaseHelper();
                User user = await firebaseHelper.GetUserById(userId);

                if (user != null)
                {
                    // Populate fields with user data
                    txtEmail.Text = user.Email;
                    txtPhoneNumber.Text = user.PhoneNumber;

                    // Enable email field if the account is phone-only
                    if (string.IsNullOrEmpty(user.Email))
                    {
                        txtEmail.Enabled = true; // Allow user to input email
                        btnDeleteAccount.Visible= false;
                        btnResetPassword.Enabled = false;
                    }
                    else
                    {
                        txtEmail.Enabled = false; // Disable email field if already set
                    }

                    // Set the selected security question and answer
                    if (!string.IsNullOrEmpty(user.SecurityQuestion))
                    {
                        ddlSecurityQuestion.SelectedValue = user.SecurityQuestion;
                    }
                    txtAnswer.Text = user.SecurityAnswer;

                    // Store the email for future use
                    Session["UserEmail"] = user.Email;
                }
            }
        }


        protected async void btnSaveChanges_Click(object sender, EventArgs e)
        {
            // Validate inputs
            if (string.IsNullOrEmpty(txtPhoneNumber.Text) ||
                string.IsNullOrEmpty(ddlSecurityQuestion.SelectedValue) || string.IsNullOrEmpty(txtAnswer.Text))
            {
                ShowErrorMessage("All fields are required.");
                return;
            }

            // Get the user's Designated User ID from the session
            string userId = Session["UserID"]?.ToString();

            if (!string.IsNullOrEmpty(userId))
            {
                // Fetch the current user details
                FirebaseHelper firebaseHelper = new FirebaseHelper();
                User user = await firebaseHelper.GetUserById(userId);

                if (user != null)
                {
                    // Check if the user is adding an email for the first time
                    if (string.IsNullOrEmpty(user.Email) && !string.IsNullOrEmpty(txtEmail.Text))
                    {
                        try
                        {
                            // Get the Firebase UID from the session
                            string firebaseUid = Session["FirebaseUID"]?.ToString();

                            if (!string.IsNullOrEmpty(firebaseUid))
                            {
                                // Link the email to the Firebase account
                                string email = txtEmail.Text;
                                string password = txtPhoneNumber.Text; 

                                await firebaseHelper.LinkEmailToAccount(firebaseUid, email, password);

                                // Update the email in the Realtime Database
                                user.Email = email;
                                await firebaseHelper.UpdateUserSecurityInfo(user);

                                ShowSuccessMessage("Email linked successfully. Default password is your Phone Number (with country code).");
                            }
                        }
                        catch (Exception ex)
                        {
                            ShowErrorMessage($"Failed to link email: {ex.Message}");
                            return;
                        }
                    }

                    // Update other security details in the Realtime Database
                    user.PhoneNumber = txtPhoneNumber.Text;
                    user.SecurityQuestion = ddlSecurityQuestion.SelectedValue;
                    user.SecurityAnswer = txtAnswer.Text;

                    try
                    {
                        // Update non-sensitive information in the Realtime Database
                        await firebaseHelper.UpdateUserSecurityInfo(user);
                        ShowSuccessMessage("Changes saved successfully.");
                    }
                    catch (Exception ex)
                    {
                        ShowErrorMessage($"Failed to save changes: {ex.Message}");
                    }
                }
                else
                {
                    ShowErrorMessage("User details not found.");
                }
            }
            else
            {
                ShowErrorMessage("User not logged in.");
            }
        }
        protected void btnResetPassword_Click(object sender, EventArgs e)
        {
            // Show password verification panel before sending reset email
            pnlPasswordVerification.Visible = true;

            // Store which operation we're verifying for
            Session["VerificationType"] = "PasswordReset";

            // Hide the main security form temporarily
            pnlSecurityForm.Visible = false;
        }

        protected async void btnVerifyPassword_Click(object sender, EventArgs e)
        {
            string password = txtVerifyPassword.Text.Trim();
            string email = Session["UserEmail"] as string;

            if (string.IsNullOrEmpty(password))
            {
                ShowErrorMessage("Please enter your current password.");
                return;
            }

            if (string.IsNullOrEmpty(email))
            {
                ShowErrorMessage("Email information missing. Please reload the page.");
                return;
            }

            try
            {
                // Verify password with Firebase
                bool isPasswordValid = await VerifyPassword(email, password);

                if (!isPasswordValid)
                {
                    ShowErrorMessage("The password you entered is incorrect. Please try again.");
                    return;
                }

                // Password verified successfully
                string verificationType = Session["VerificationType"] as string;

                if (verificationType == "PasswordReset")
                {
                    await ProcessPasswordReset(email);
                }
                else if (verificationType == "DeleteAccount")
                {
                    await ProcessAccountDeletion();
                }

                // Clear password field and hide verification panel
                txtVerifyPassword.Text = string.Empty;
                pnlPasswordVerification.Visible = false;
                pnlSecurityForm.Visible = true;

                // Clear verification type
                Session.Remove("VerificationType");
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Verification failed: {ex.Message}");
            }
        }

        protected void btnCancelVerification_Click(object sender, EventArgs e)
        {
            // Cancel verification and go back to the main form
            txtVerifyPassword.Text = string.Empty;
            pnlPasswordVerification.Visible = false;
            pnlSecurityForm.Visible = true;
            Session.Remove("VerificationType");
        }

        private async Task<bool> VerifyPassword(string email, string password)
        {
            try
            {
                // Create the auth provider
                FirebaseAuthProvider authProvider = new FirebaseAuthProvider(new FirebaseConfig("AIzaSyAqpGAELltYzFuKiCjl4o71VszV6Px7J6w"));

                // Attempt to sign in with the provided credentials
                var authData = await authProvider.SignInWithEmailAndPasswordAsync(email, password);

                // If we get here, the password is valid
                return true;
            }
            catch (Exception)
            {
                // Authentication failed
                return false;
            }
        }

        private async Task ProcessPasswordReset(string email)
        {
            // Check if the button was clicked within the last minute
            if (Session["LastResetPasswordClick"] != null)
            {
                DateTime lastClickTime = (DateTime)Session["LastResetPasswordClick"];
                if (DateTime.Now.Subtract(lastClickTime).TotalMinutes < 1)
                {
                    ShowErrorMessage("Please wait 1 minute before requesting another password reset.");
                    return;
                }
            }

            if (string.IsNullOrEmpty(email))
            {
                ShowErrorMessage("Email is required to reset the password.");
                return;
            }

            try
            {
                // Use the FirebaseHelper instance to send the password reset email
                FirebaseHelper firebaseHelper = new FirebaseHelper();
                await firebaseHelper.SendPasswordResetEmailAsync(email);

                // Update the last click time in the session
                Session["LastResetPasswordClick"] = DateTime.Now;
                ShowSuccessMessage("Password reset email sent. Please check your inbox.");
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Failed to send password reset email: {ex.Message}");
            }
        }

        protected void btnDeleteAccount_Click(object sender, EventArgs e)
        {
            // Show password verification panel before deleting account
            pnlPasswordVerification.Visible = true;

            // Store which operation we're verifying for
            Session["VerificationType"] = "DeleteAccount";

            // Hide the main security form temporarily
            pnlSecurityForm.Visible = false;
        }

        private async Task ProcessAccountDeletion()
        {
            // Get the user's Designated User ID and Firebase UID from the session
            string userId = Session["UserID"]?.ToString();
            string firebaseUid = Session["FirebaseUID"]?.ToString();

            if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(firebaseUid))
            {
                try
                {
                    // Use the FirebaseHelper instance to disable the account
                    FirebaseHelper firebaseHelper = new FirebaseHelper();
                    await firebaseHelper.DisableUserAccount(userId, firebaseUid);

                    // Show success message with JavaScript alert and immediate redirect after closing
                    string alertAndRedirectScript = @"
                alert('Your account has been disabled successfully.');
                window.location.href = '~/OMTS_Pages/OMTS_AM/AM_LoginHP.aspx';";

                    // Execute the script
                    ClientScript.RegisterStartupScript(this.GetType(), "alertAndRedirect", alertAndRedirectScript, true);

                    // Clear the session
                    Session.Clear();
                }
                catch (Exception ex)
                {
                    ShowErrorMessage($"Failed to disable account: {ex.Message}");
                }
            }
            else
            {
                ShowErrorMessage("User not logged in.");
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

        private bool IsUserLoggedIn()
        {
            // Check if the user is logged in (ADD IN ALL PAGES THAT NEED ACCESS)
            // Check if the session contains the UserID or FirebaseUID
            return Session["UserID"] != null || Session["FirebaseUID"] != null;
        }

        private void LoadResources()
        {
            // Heading for the page
            litSecuritySettings.Text = GetGlobalResourceObject("Resources", "Heading_UpdateAccountSecurity").ToString();

            // Button text
            btnSaveChanges.Text = GetGlobalResourceObject("Resources", "Button_SaveChanges").ToString();
            btnDeleteAccount.Text = GetGlobalResourceObject("Resources", "Button_DeleteAccount").ToString();
            btnAccSecurity.Text = GetGlobalResourceObject("Resources", "SideBtnAS")?.ToString();
            btnPersonalInfo.Text = GetGlobalResourceObject("Resources", "SideBtnPI")?.ToString();
            btnResetPassword.Text = GetGlobalResourceObject("Resources", "Button_ResetPassword")?.ToString();
            btnVerifyPassword.Text = GetGlobalResourceObject("Resources", "Button_Verify")?.ToString();
            btnCancelVerification.Text = GetGlobalResourceObject("Resources", "Button_Cancel")?.ToString();

            // Labels
            lblVerifyPassword.Text = GetGlobalResourceObject("Resources", "Label_CurrentPassword")?.ToString() ?? "Current Password:";
            lblVerificationInstructions.Text = GetGlobalResourceObject("Resources", "Text_PasswordVerificationInstructions")?.ToString() ??
                "For security purposes, please enter your current password to continue.";
        }
    }
}