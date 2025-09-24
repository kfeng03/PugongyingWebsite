using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace fyp
{
    public partial class EM_RecycleBin : System.Web.UI.Page
    {
        private FirebaseHelper firebaseHelper = new FirebaseHelper();

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
                btnMyEvents.Visible = userRole == "Member";
                btnJoinEvent.Visible = userRole == "Member";
                btnCompletedEvent.Visible = userRole == "Member";
                btnEventManagement.Visible = userRole == "Staff" || userRole == "Admin";
                btnEventFeedbacks.Visible = userRole == "Staff" || userRole == "Admin";
                await LoadDeletedEvents();
            }
        }

        private async Task LoadDeletedEvents()
        {
            try
            {
                var deletedEvents = await firebaseHelper.GetDeletedEvents();

                if (deletedEvents == null || !deletedEvents.Any())
                {
                    rptDeletedEvents.DataSource = null;
                    rptDeletedEvents.DataBind();
                    lblNoDeletedEvents.Visible = true;
                    return;
                }

                lblNoDeletedEvents.Visible = false;
                rptDeletedEvents.DataSource = deletedEvents;
                rptDeletedEvents.DataBind();
            }
            catch (Exception ex)
            {
                lblMessage.Text = "Error loading deleted events: " + ex.Message;
                lblMessage.ForeColor = System.Drawing.Color.Red;
            }
        }

        protected async void btnRestore_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            string eventId = btn.CommandArgument;

            if (string.IsNullOrEmpty(eventId))
            {
                lblMessage.Text = "Error: Event ID is empty";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                return;
            }

            try
            {
                // Restore the event (set IsDeleted to false)
                await firebaseHelper.RestoreEvent(eventId);

                // Refresh the list
                await LoadDeletedEvents();

                lblMessage.Text = "Event restored successfully.";
                lblMessage.ForeColor = System.Drawing.Color.Green;
            }
            catch (Exception ex)
            {
                lblMessage.Text = "Error restoring event: " + ex.Message;
                lblMessage.ForeColor = System.Drawing.Color.Red;
            }
        }

        protected async void btnDelete_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            string eventId = btn.CommandArgument;

            if (string.IsNullOrEmpty(eventId))
            {
                lblMessage.Text = "Error: Event ID is empty";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                return;
            }

            try
            {
                // Log the event ID for debugging
                System.Diagnostics.Debug.WriteLine("Attempting to delete event: " + eventId);

                // Permanently delete the event
                await firebaseHelper.DeleteEvent(eventId);

                // Refresh the list
                await LoadDeletedEvents();

                lblMessage.Text = "Event permanently deleted.";
                lblMessage.ForeColor = System.Drawing.Color.Green;
            }
            catch (Exception ex)
            {
                lblMessage.Text = "Error deleting event: " + ex.Message;
                System.Diagnostics.Debug.WriteLine("Exception details: " + ex.ToString());
                lblMessage.ForeColor = System.Drawing.Color.Red;
            }
        }

        protected void btnBack_Click(object sender, EventArgs e)
        {
            Response.Redirect("EM_EventManagement.aspx");
        }
    }
}