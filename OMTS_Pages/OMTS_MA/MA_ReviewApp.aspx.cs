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
    public partial class MA_ReviewApp : System.Web.UI.Page, ILocalizable
    {
        protected FirebaseHelper firebaseHelper;

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

        protected void Page_Load(object sender, EventArgs e)
        {
            // Initialize Firebase helper
            firebaseHelper = new FirebaseHelper();

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

                // Load applications
                LoadApplications();
            }
        }

        private async void LoadApplications()
        {
            try
            {
                // Get all applications
                var applications = await firebaseHelper.GetAchievementApplications();

                // Apply status filter if selected
                string statusFilter = ddlStatusFilter.SelectedValue;
                if (statusFilter != "All")
                {
                    applications = applications.Where(a => a.Status == statusFilter).ToList();
                }

                // Create a list to store the display data
                var displayData = new List<ApplicationDisplayData>();

                foreach (var app in applications)
                {
                    // Get applicant details
                    var applicant = await firebaseHelper.GetUserById(app.ApplicantRefId);
                    string applicantName = applicant != null ? applicant.Username : "Unknown";

                    // Get event details
                    string eventName = "Unknown";
                    if (!string.IsNullOrEmpty(app.EventId))
                    {
                        try
                        {
                            var eventInfo = await GetEventById(app.EventId);
                            eventName = eventInfo?.EventTitle ?? "Unknown";
                        }
                        catch
                        {
                            // If there's an error getting the event, use the default "Unknown"
                            eventName = "Unknown";
                        }
                    }

                    // Add to display data
                    displayData.Add(new ApplicationDisplayData
                    {
                        ApplicationId = app.ApplicationId,
                        ApplicantName = applicantName,
                        EventName = eventName,
                        ParticipatedRole = app.ParticipatedRole,
                        LearningOutcome = app.LearningOutcome,
                        SupportingDocUrl = app.SupportingDocUrl,
                        Status = app.Status ?? "Pending",
                        Comment = app.Comment
                    });
                }

                // Bind data to grid
                gvApplications.DataSource = displayData;
                gvApplications.DataBind();
            }
            catch (Exception ex)
            {
                // Log the error
                System.Diagnostics.Debug.WriteLine($"Error loading applications: {ex.Message}");

                // Show error message
                ScriptManager.RegisterStartupScript(this, GetType(), "showerror",
                    $"alert('Error loading applications: {ex.Message}');", true);
            }
        }

        // Helper method to get event by ID
        private async Task<Event> GetEventById(string eventId)
        {
            // Use the Firebase helper to get the event
            return await firebaseHelper.GetEventById(eventId);
        }

        // Handle GridView row command events
        protected void gvApplications_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "ReviewApplication" || e.CommandName == "ViewDetails")
            {
                string applicationId = e.CommandArgument.ToString();

                // Redirect to review detail page with application ID
                Response.Redirect($"MA_ReviewAppDetail.aspx?id={applicationId}");
            }
        }

        // Format the status with appropriate CSS class
        protected string GetStatusCssClass(string status)
        {
            switch (status?.ToLower())
            {
                case "approved":
                    return "status-approved";
                case "rejected":
                    return "status-rejected";
                case "pending":
                default:
                    return "status-pending";
            }
        }

        // Handle status filter change
        protected void ddlStatusFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadApplications();
        }

        // Handle row data bound event to customize row appearance
        protected void gvApplications_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                // Add any custom formatting for rows here if needed
            }
        }

        private void LoadResources()
        {
            // Set page labels and controls from resource files
            litRecentApplication.Text = GetGlobalResourceObject("Resources", "Heading_ListOfRecentApplications")?.ToString();

            // Button text
            btnInformation.Text = GetGlobalResourceObject("Resources", "SideBtnInfo")?.ToString();
            btnDashboard.Text = GetGlobalResourceObject("Resources", "SideBtnDb")?.ToString();
            btnApplicationForm.Text = GetGlobalResourceObject("Resources", "SideBtnMAApp")?.ToString();
            btnReviewApp.Text = GetGlobalResourceObject("Resources", "SideBtnRA")?.ToString();
            btnAwardBadge.Text = GetGlobalResourceObject("Resources", "SideBtnAB")?.ToString();
            btnMaterials.Text = GetGlobalResourceObject("Resources", "SideBtnMAMat")?.ToString();
        }
    }

    // Class to hold display data for the grid
    public class ApplicationDisplayData
    {
        public string ApplicationId { get; set; }
        public string ApplicantName { get; set; }
        public string EventName { get; set; }
        public string ParticipatedRole { get; set; }
        public string LearningOutcome { get; set; }
        public string SupportingDocUrl { get; set; }
        public string Status { get; set; }
        public string Comment { get; set; }
    }
}