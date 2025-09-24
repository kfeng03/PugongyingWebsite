using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace fyp
{
    public partial class EM_EventManagement : Page
    {
        private FirebaseHelper firebaseHelper = new FirebaseHelper(); // Initialize FirebaseHelper

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

            if (!IsPostBack)
            {
                // Get the current user's role from the session
                string userRole = Session["UserRole"]?.ToString();
                // Set visibility for each button based on the user's role
                btnMyEvents.Visible = userRole == "Member" || userRole == "Admin"; 
                btnJoinEvent.Visible = userRole == "Member" || userRole == "Admin"; 
                btnCompletedEvent.Visible = userRole == "Member" || userRole == "Admin";
                btnEventManagement.Visible = userRole == "Staff" || userRole == "Admin";
                btnEventFeedbacks.Visible = userRole == "Staff" || userRole == "Admin";
                await LoadActiveEvents();
            }
        }

        protected string GetStatusClass(string status)
        {
            switch (status.ToLower())
            {
                case "upcoming":
                    return "status-upcoming";
                case "in progress":
                    return "status-in-progress";
                case "completed":
                    return "status-completed";
                default:
                    return "";
            }
        }

        private async Task LoadActiveEvents()
        {
            var events = await firebaseHelper.GetActiveEvents();
            if (events == null || !events.Any())
            {
                lblMessage.Text = "No events available.";
                lblMessage.Visible = true;
                rptEvents.DataSource = null;
                rptEvents.DataBind();
                return;
            }

            lblMessage.Text = ""; // Clear message if events are available
            lblMessage.Visible = false;
            rptEvents.DataSource = events;
            rptEvents.DataBind();
        }

        protected void btnEdit_Click(object sender, EventArgs e)
        {
            LinkButton btn = (LinkButton)sender;
            string eventId = btn.CommandArgument;
            // Redirect to Edit Event Page with the Event ID as a query parameter
            Response.Redirect($"EM_EditEvent.aspx?eventId={eventId}");
        }

        protected void btnParticipants_Click(object sender, EventArgs e)
        {
            LinkButton btn = (LinkButton)sender;
            string eventId = btn.CommandArgument;
            // Redirect to Participants Page with the Event ID as a query parameter
            Response.Redirect($"EM_EventParticipants.aspx?id={eventId}");
        }

        protected async void btnDelete_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            string eventId = btn.CommandArgument;

            // Prevent multiple execution
            if (string.IsNullOrEmpty(eventId)) return;

            // Soft delete the event
            await firebaseHelper.SoftDeleteEvent(eventId);

            // Refresh event list
            await LoadActiveEvents();
        }

        protected void btnRecycleBin_Click(object sender, EventArgs e)
        {
            Response.Redirect("EM_RecycleBin.aspx");
        }
    }
}