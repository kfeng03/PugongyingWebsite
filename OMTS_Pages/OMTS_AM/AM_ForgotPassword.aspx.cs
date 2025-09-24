using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Threading.Tasks;
using System.Drawing;
using System.Globalization;
using System.Threading;

namespace fyp
{
    public partial class AM_ForgotPassword : System.Web.UI.Page, ILocalizable
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
    
        private FirebaseHelper firebaseHelper;
        private string userEmail = string.Empty;
        private User currentUser = null;

        protected void Page_Load(object sender, EventArgs e)
        {
            firebaseHelper = new FirebaseHelper();

            if (!IsPostBack)
            {
                LoadResources();

                // Get email from query string if coming from another page
                string email = Request.QueryString["email"];
                if (!string.IsNullOrEmpty(email))
                {
                    txtEmail.Text = email;
                }
            }
        }
        private void LoadResources()
        {


            // heading.
            litResetHeading.Text = GetGlobalResourceObject("Resources", "Reset_Password_Heading")?.ToString();
            litResetSubtext.Text = GetGlobalResourceObject("Resources", "Reset_Password_Info")?.ToString();


            // Hyperlinks text.
            lnkBackToLogin.Text = GetGlobalResourceObject("Resources", "Link_BackToLogin")?.ToString();
            btnSendResetEmail.Text = GetGlobalResourceObject("Resources", "Button_SendResetEmail")?.ToString();

            // Buttons and labels.
            litVerificationSuccess.Text = GetGlobalResourceObject("Resources", "Security_Answer_Correct")?.ToString();
            btnVerifyAnswer.Text = GetGlobalResourceObject("Resources", "Button_Verify")?.ToString();
            btnContinue.Text = GetGlobalResourceObject("Resources", "Button_Continue")?.ToString();
        }
        protected async void btnContinue_Click(object sender, EventArgs e)
        {
            try
            {
                userEmail = txtEmail.Text.Trim();

                if (string.IsNullOrEmpty(userEmail))
                {
                    ShowErrorMessage("Please enter your email address.");
                    return;
                }

                // Validate email format
                if (!IsValidEmail(userEmail))
                {
                    ShowErrorMessage("Please enter a valid email address.");
                    return;
                }

                // Check if user exists with this email
                var allUsers = await firebaseHelper.GetUsers();
                currentUser = allUsers.FirstOrDefault(u => u.Email == userEmail);

                if (currentUser == null)
                {
                    ShowErrorMessage("No account found with this email address.");
                    return;
                }

                // Log the security question and answer for debugging
                System.Diagnostics.Debug.WriteLine($"SecurityQuestion: {currentUser.SecurityQuestion}");
                System.Diagnostics.Debug.WriteLine($"SecurityAnswer: {currentUser.SecurityAnswer}");

                // Check if security question is set
                if (string.IsNullOrEmpty(currentUser.SecurityQuestion?.Trim()) || string.IsNullOrEmpty(currentUser.SecurityAnswer?.Trim()))
                {
                    ShowErrorMessage("Security question is not set up for this account. Please contact an administrator for assistance.");
                    return;
                }


                // Map the security question ID to the actual question text
                string securityQuestionText = GetSecurityQuestionText(currentUser.SecurityQuestion);

                // Show security question
                lblSecurityQuestion.Text = securityQuestionText;

                // Update UI to show security question step
                pnlEmail.Visible = false;
                pnlSecurityQuestion.Visible = true;
                stepEmail.Attributes["class"] = "step";
                stepSecurity.Attributes["class"] = "step active";

                // Store the user ID in session
                Session["ResetPasswordUserId"] = currentUser.UserId;
                Session["ResetPasswordEmail"] = userEmail;
            }
            catch (Exception ex)
            {
                ShowErrorMessage("Error: " + ex.Message);
            }
        }

        protected async void btnVerifyAnswer_Click(object sender, EventArgs e)
        {
            try
            {
                // Check if security answer is provided
                if (string.IsNullOrEmpty(txtSecurityAnswer.Text.Trim()))
                {
                    ShowErrorMessage("Please enter your security answer.");
                    return;
                }

                string userId = Session["ResetPasswordUserId"] as string;

                if (string.IsNullOrEmpty(userId))
                {
                    ShowErrorMessage("Session expired. Please start over.");
                    ResetToStep1();
                    return;
                }

                // Get user from Firebase
                currentUser = await firebaseHelper.GetUserById(userId);

                if (currentUser == null)
                {
                    ShowErrorMessage("User not found. Please start over.");
                    ResetToStep1();
                    return;
                }

                // Verify security answer
                string providedAnswer = txtSecurityAnswer.Text.Trim();
                string storedAnswer = currentUser.SecurityAnswer;

                if (string.IsNullOrEmpty(providedAnswer) || !providedAnswer.Equals(storedAnswer, StringComparison.OrdinalIgnoreCase))
                {
                    ShowErrorMessage("Incorrect security answer. Please try again.");
                    return;
                }


                // Answer is correct, move to reset step
                lblMessage.Visible = false;
                pnlSecurityQuestion.Visible = false;
                pnlResetPassword.Visible = true;
                stepSecurity.Attributes["class"] = "step";
                stepReset.Attributes["class"] = "step active";
            }
            catch (Exception ex)
            {
                ShowErrorMessage("Error: " + ex.Message);
            }
        }

        protected async void btnSendResetEmail_Click(object sender, EventArgs e)
        {
            try
            {
                string email = Session["ResetPasswordEmail"] as string;

                if (string.IsNullOrEmpty(email))
                {
                    ShowErrorMessage("Session expired. Please start over.");
                    ResetToStep1();
                    return;
                }

                // Send password reset email using Firebase
                await firebaseHelper.SendPasswordResetEmailAsync(email);

                // Show success message
                lblMessage.Text = "Password reset email has been sent. Please check your email for further instructions.";
                lblMessage.ForeColor = Color.Green;
                lblMessage.Visible = true;

                // Disable the button to prevent multiple clicks
                btnSendResetEmail.Enabled = false;
                btnSendResetEmail.Text = "Email Sent";
            }
            catch (Exception ex)
            {
                ShowErrorMessage("Error sending reset email: " + ex.Message);
            }
        }

        private void ShowErrorMessage(string message)
        {
            lblMessage.Text = message;
            lblMessage.ForeColor = Color.Red;
            lblMessage.Visible = true;
        }

        private void ResetToStep1()
        {
            // Clear session data
            Session.Remove("ResetPasswordUserId");
            Session.Remove("ResetPasswordEmail");

            // Reset UI
            pnlEmail.Visible = true;
            pnlSecurityQuestion.Visible = false;
            pnlResetPassword.Visible = false;

            stepEmail.Attributes["class"] = "step active";
            stepSecurity.Attributes["class"] = "step";
            stepReset.Attributes["class"] = "step";

            txtEmail.Text = string.Empty;
            txtSecurityAnswer.Text = string.Empty;
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        // Helper method to map security question IDs to their text
        private string GetSecurityQuestionText(string questionId)
        {
            switch (questionId)
            {
                case "1":
                    return "What is your mother's maiden name?";
                case "2":
                    return "What was the name of your first pet?";
                case "3":
                    return "What city were you born in?";
                default:
                    return "Security Question"; // Fallback text
            }
        }
    }
}