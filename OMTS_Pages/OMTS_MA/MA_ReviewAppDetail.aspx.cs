using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace fyp
{
    public partial class MA_ReviewAppDetail : System.Web.UI.Page, ILocalizable
    {
        protected FirebaseHelper firebaseHelper;
        private string applicationId;
        private AchievementApplication currentApplication;

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
            // Check if the user is logged in
            return Session["UserID"] != null || Session["FirebaseUID"] != null;
        }

        protected async void Page_Load(object sender, EventArgs e)
        {
            // Initialize Firebase helper
            firebaseHelper = new FirebaseHelper();

            // Get application ID from query string
            applicationId = Request.QueryString["id"];
            if (string.IsNullOrEmpty(applicationId))
            {
                // No ID provided, redirect back to list
                Response.Redirect("MA_ReviewApp.aspx",false);
                Context.ApplicationInstance.CompleteRequest(); // Prevents further processing
                return;
            }

            if (!IsPostBack)
            {
                // Get the current user's role from the session
                string userRole = Session["UserRole"]?.ToString();

                // Set visibility for each button based on the user's role
                btnInformation.Visible = true; // All can access
                btnDashboard.Visible = userRole == "Member" || userRole == "Admin";
                btnApplicationForm.Visible = userRole == "Member" || userRole == "Admin";
                btnReviewApp.Visible = userRole == "Staff" || userRole == "Admin";
                btnAwardBadge.Visible = userRole == "Staff" || userRole == "Admin";
                btnMaterials.Visible = userRole == "Staff" || userRole == "Admin";

                LoadResources();
                Page.DataBind();

                // Load application details
                await LoadApplicationDetails();
            }
        }
        

        private async Task LoadApplicationDetails()
        {
            try
            {
                // Get application by ID
                currentApplication = await firebaseHelper.GetAchievementApplicationById(applicationId);
                if (currentApplication == null)
                {
                    // Application not found
                    ShowError("Application not found.");
                    return;
                }

                // Set application details in form
                litApplicationId.Text = currentApplication.ApplicationId;
                lblStatus.Text = currentApplication.Status ?? "Pending";
                SetStatusStyle(currentApplication.Status);

                litRoleValue.Text = currentApplication.ParticipatedRole;
                litOutcomeText.Text = currentApplication.LearningOutcome;

                // Show existing comments if available
                if (!string.IsNullOrEmpty(currentApplication.Comment))
                {
                    trExistingComments.Visible = true;
                    litExistingComments.Text = currentApplication.Comment;
                }
                if (currentApplication.SupportingDocUrl != null)
                {
                    divNoAttch.Visible = false;
                    divPdfContainer.Visible = true;
                    // Try to embed if it's a Google Drive PDF
                    if (currentApplication.SupportingDocUrl.Contains("drive.google.com"))
                    {
                        string pdfId = ExtractGoogleDriveId(currentApplication.SupportingDocUrl);
                        if (!string.IsNullOrEmpty(pdfId))
                        {
                            litPdfEmbed.Text = $"<iframe src=\"https://drive.google.com/file/d/{pdfId}/preview\" " +
                                $"width=\"100%\" height=\"300px\" allow=\"autoplay\"></iframe>";
                        }
                    }
                    else
                    {
                        litPdfEmbed.Text = "Not a Google Drive Link";
                    }
                }
                else
                {
                    divNoAttch.Visible = true;
                    divPdfContainer.Visible = false;
                }

                // Get applicant details
                var applicant = await firebaseHelper.GetUserById(currentApplication.ApplicantRefId);
                litApplNameValue.Text = applicant != null
                    ? $"{applicant.Username} ({applicant.UserId})"
                    : $"Unknown ({currentApplication.ApplicantRefId})";

                // Get event details
                if (!string.IsNullOrEmpty(currentApplication.EventId))
                {
                    try
                    {
                        var eventInfo = await firebaseHelper.GetEventById(currentApplication.EventId);
                        if (eventInfo != null)
                        {
                            litEventNameValue.Text = $"{eventInfo.EventTitle}";
                        }
                        else
                        {
                            litEventNameValue.Text = $"Unknown Event ({currentApplication.EventId})";
                        }
                    }
                    catch
                    {
                        litEventNameValue.Text = $"Unknown Event ({currentApplication.EventId})";
                    }
                }
                else
                {
                    litEventNameValue.Text = "No event specified";
                }

                // Set form state based on application status
                SetFormState();
            }
            catch (Exception ex)
            {
                // Show error
                ShowError($"Error loading application details: {ex.Message}");
            }
        }

        private string ExtractGoogleDriveId(string url)
        {
            try
            {
                // Example: https://drive.google.com/file/d/1JH6tFmB9oZCJjGkZ-JF8w-OLXhq0Uxk_/view?usp=sharing
                if (url.Contains("/file/d/"))
                {
                    int startIndex = url.IndexOf("/file/d/") + 8;
                    string id = url.Substring(startIndex);
                    int endIndex = id.IndexOf('/');
                    if (endIndex > 0)
                    {
                        id = id.Substring(0, endIndex);
                    }
                    return id;
                }

                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        private void SetFormState()
        {
            string userRole = Session["UserRole"]?.ToString();

            if (userRole == "Staff" || userRole == "Admin"){
            // Set visibility of panels based on application status
            bool isPending = currentApplication.Status == null || currentApplication.Status == "Pending";

            // Only show action buttons for pending applications
            pnlActions.Visible = isPending;
            }
            else
            {
                pnlActions.Visible = false;
            }
        }

        private void SetStatusStyle(string status)
        {
            switch (status?.ToLower())
            {
                case "approved":
                    lblStatus.CssClass = "status-approved";
                    break;
                case "rejected":
                    lblStatus.CssClass = "status-rejected";
                    break;
                case "pending":
                default:
                    lblStatus.CssClass = "status-pending";
                    break;
            }
        }

        protected async void btnProceed_Click(object sender, EventArgs e)
        {
            try
            {
                // Get the current application if needed
                if (currentApplication == null)
                {
                    currentApplication = await firebaseHelper.GetAchievementApplicationById(applicationId);
                    if (currentApplication == null)
                    {
                        ShowError("Application not found.");
                        return;
                    }
                }

                string action = hdnAction.Value.ToLower();
                string comments = txtComment.Text.Trim();
                int pointsAwarded = 0;

                // Parse points, default to 0 if parsing fails
                int.TryParse(txtPointsAwarded.Text, out pointsAwarded);

                // Update application status based on action
                if (action == "approve")
                {
                    currentApplication.Status = "Approved";

                    // Award points to the user
                    if (pointsAwarded > 0)
                    {
                        await AwardPointsToUser(currentApplication.ApplicantRefId, pointsAwarded);
                    }
                }
                else if (action == "reject")
                {
                    currentApplication.Status = "Rejected";
                    // No points for rejected applications
                    pointsAwarded = 0;
                }
                else
                {
                    ShowError("Invalid action specified.");
                    return;
                }

                // Update application comment
                currentApplication.Comment = comments;

                // Save the updated application
                await firebaseHelper.UpdateAchievementApplication(applicationId, currentApplication);

                // Update the UI
                lblStatus.Text = currentApplication.Status;
                SetStatusStyle(currentApplication.Status);

                if (!string.IsNullOrEmpty(comments))
                {
                    trExistingComments.Visible = true;
                    litExistingComments.Text = comments;
                }

                // Update the form state
                SetFormState();

                // Show success message
                string message = action == "approve"
                    ? $"Application has been approved successfully. {pointsAwarded} points awarded to the applicant."
                    : "Application has been rejected successfully.";

                ScriptManager.RegisterStartupScript(this, GetType(), "showsuccess",
                    $"alert('{message}'); window.location.href = 'MA_ReviewApp.aspx';", true);
            }
            catch (Exception ex)
            {
                ShowError($"Error processing application: {ex.Message}");
            }
        }

        private async Task AwardPointsToUser(string userId, int points)
        {
            try
            {
                // Get the user
                var user = await firebaseHelper.GetUserById(userId);
                if (user == null || user.MemberData == null)
                {
                    throw new Exception($"User {userId} not found or does not have member data.");
                }

                // Add points to user
                user.MemberData.Points += points;


                // Create updates dictionary
                var updates = new Dictionary<string, object>
                {
                    { "MemberData/Points", user.MemberData.Points },
                };

                // Update user in Firebase
                await firebaseHelper.UpdateUserBadges(userId, updates);
            }
            catch (Exception ex)
            {
                // Log the error
                System.Diagnostics.Debug.WriteLine($"Error awarding points: {ex.Message}");
                throw;
            }
        }

        private void ShowError(string message)
        {
            ScriptManager.RegisterStartupScript(this, GetType(), "showerror",
                $"alert('{message}');", true);
        }

        private void LoadResources()
        {
            // Set page labels and controls from resource files
            litHeading.Text = GetGlobalResourceObject("Resources", "Heading_ReviewApp")?.ToString() ?? "Review Application";

            // Button text
            btnInformation.Text = GetGlobalResourceObject("Resources", "SideBtnInfo")?.ToString();
            btnDashboard.Text = GetGlobalResourceObject("Resources", "SideBtnDb")?.ToString();
            btnApplicationForm.Text = GetGlobalResourceObject("Resources", "SideBtnMAApp")?.ToString();
            btnReviewApp.Text = GetGlobalResourceObject("Resources", "SideBtnRA")?.ToString();
            btnAwardBadge.Text = GetGlobalResourceObject("Resources", "SideBtnAB")?.ToString();
            btnMaterials.Text = GetGlobalResourceObject("Resources", "SideBtnMAMat")?.ToString();

            btnApprove.Text = GetGlobalResourceObject("Resources", "Button_Approve")?.ToString() ?? "Approve";
            btnReject.Text = GetGlobalResourceObject("Resources", "Button_Reject")?.ToString() ?? "Reject";
            btnProceed.Text = GetGlobalResourceObject("Resources", "Button_Proceed")?.ToString() ?? "Proceed";
            litProceed.Text = GetGlobalResourceObject("Resources", "Heading_Proceed")?.ToString() ?? "Proceed with Review";
            litDetails.Text = GetGlobalResourceObject("Resources", "Note_ProceedTip")?.ToString() ?? "Please provide comments for this review.";
        }
    }
}