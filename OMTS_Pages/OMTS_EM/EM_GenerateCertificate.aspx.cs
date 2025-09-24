using System;
using System.Threading.Tasks;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace fyp
{
    public partial class EM_GenerateCertificate : System.Web.UI.Page
    {
        private FirebaseHelper firebaseHelper = new FirebaseHelper();
        private string currentUserID;
        private User currentUser;
        private Event currentEvent;
        private string eventId;

        private bool IsUserLoggedIn()
        {
            return Session["UserID"] != null || Session["FirebaseUID"] != null;
        }

        protected async void Page_Load(object sender, EventArgs e)
        {
            if (!IsUserLoggedIn())
            {
                Response.Redirect("~/OMTS_Pages/OMTS_AM/AM_LoginGoogle.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            // Get the current user's ID
            currentUserID = Session["UserID"]?.ToString();

            // Get the event ID from the query string
            eventId = Request.QueryString["id"];
            if (string.IsNullOrEmpty(eventId))
            {
                Response.Redirect("EM_CompletedEvent.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            if (!IsPostBack)
            {
                // Get the current user's role from the session
                string userRole = Session["UserRole"]?.ToString();

                // Set visibility for each button based on the user's role
                btnMyEvents.Visible = userRole == "Member" || userRole == "Admin";
                btnJoinEvent.Visible = userRole == "Member" || userRole == "Admin";
                btnCompletedEvent.Visible = userRole == "Member" || userRole == "Admin";
                btnEventManagement.Visible = userRole == "Staff" || userRole == "Admin";

                await LoadCertificateData();
            }
        }

        private async Task LoadCertificateData()
        {
            try
            {
                // Get the event details
                currentEvent = await firebaseHelper.GetEventById(eventId);
                if (currentEvent == null)
                {
                    // Event not found, redirect to completed events
                    Response.Redirect("EM_CompletedEvent.aspx", false);
                    Context.ApplicationInstance.CompleteRequest();
                    return;
                }

                // Get the current user
                currentUser = await firebaseHelper.GetUserById(currentUserID);
                if (currentUser == null)
                {
                    // User not found, redirect to login
                    Response.Redirect("AM_LoginEmail.aspx", false);
                    Context.ApplicationInstance.CompleteRequest();
                    return;
                }

                // Check if this event is in the user's completed events
                // Check if this event is in the user's completed events
                bool hasCompletedEvent = false;
                if (currentUser.MemberData?.EventID_List != null)
                {
                    hasCompletedEvent = currentUser.MemberData.EventID_List.Contains(eventId);
                }

                // Update event status to ensure we have the latest status
                currentEvent.UpdateEventStatus();

                if (!hasCompletedEvent || currentEvent.EventStatus != "Completed")
                {
                    // User hasn't completed this event or event is not completed
                    Response.Redirect("EM_CompletedEvent.aspx", false);
                    Context.ApplicationInstance.CompleteRequest();
                    return;
                }

                // Populate certificate data
                litParticipantName.Text = currentUser.Username;
                litEventName.Text = currentEvent.EventTitle;

                // Format event details
                string eventDetails = string.Format(
                    "held on {0} at {1}",
                    DateTime.Parse(currentEvent.EventDate).ToString("MMMM d, yyyy"),
                    currentEvent.EventLocation
                );
                litEventDetails.Text = eventDetails;

                // Set organizer name
                litOrganizerName.Text = currentEvent.OrganizerName;

                // Set organization name (from web.config or as a default)
                litOrganizationName.Text = GetGlobalResourceObject("Resources", "Label_OrgName")?.ToString() ?? "Online Management Training System";

                // Set issue date
                litIssueDate.Text = DateTime.Now.ToString("MMMM d, yyyy");
            }
            catch (Exception ex)
            {
                // Log error and redirect
                System.Diagnostics.Debug.WriteLine("Error loading certificate data: " + ex.Message);
                Response.Redirect("EM_CompletedEvent.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
            }
        }

        protected void btnBack_Click(object sender, EventArgs e)
        {
            Response.Redirect("EM_CompletedEvent.aspx");
        }
    }
}