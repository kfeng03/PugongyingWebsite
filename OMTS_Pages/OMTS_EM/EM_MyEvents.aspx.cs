using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace fyp
{
    public partial class EM_MyEvents : System.Web.UI.Page
    {
        private FirebaseHelper firebaseHelper = new FirebaseHelper();
        private string currentUserID;
        private User currentUser;

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

                await LoadMyEvents();
            }
        }

        protected string GetStatusCssClass(string status)
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

        private async Task LoadMyEvents()
        {
            try
            {
                // Get the current user
                currentUser = await firebaseHelper.GetUserById(currentUserID);

                if (currentUser == null || currentUser.MemberData == null || currentUser.MemberData.EventID_List == null || !currentUser.MemberData.EventID_List.Any())
                {
                    rptMyEvents.DataSource = null;
                    rptMyEvents.DataBind();
                    lblNoEvents.Visible = true;
                    return;
                }

                // Get all events
                var allEvents = await firebaseHelper.GetActiveEvents();

                // Filter for events that the user has joined
                var myEvents = allEvents.Where(e => currentUser.MemberData.EventID_List.Contains(e.EventID)).ToList();

                // Update status for each event
                foreach (var ev in myEvents)
                {
                    ev.UpdateEventStatus();
                }

                // Further filter to get only upcoming and in-progress events
                var activeEvents = myEvents.Where(e => e.EventStatus == "Upcoming" || e.EventStatus == "In Progress").ToList();

                if (!activeEvents.Any())
                {
                    rptMyEvents.DataSource = null;
                    rptMyEvents.DataBind();
                    lblNoEvents.Visible = true;
                    return;
                }

                rptMyEvents.DataSource = activeEvents;
                rptMyEvents.DataBind();
                lblNoEvents.Visible = false;
            }
            catch (Exception ex)
            {
                lblMessage.Text = "Error loading events: " + ex.Message;
                lblMessage.ForeColor = System.Drawing.Color.Red;
                lblMessage.Visible = true;
            }
        }

        protected void btnViewDetails_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            string eventId = btn.CommandArgument;

            if (!string.IsNullOrEmpty(eventId))
            {
                // Store the current page as referrer
                Session["EventDetailsReferrer"] = Request.Url.AbsolutePath;
                Response.Redirect($"EM_EventDetails.aspx?id={eventId}");
            }
        }

        protected async void btnLeave_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            string eventId = btn.CommandArgument;

            if (string.IsNullOrEmpty(eventId))
            {
                lblMessage.Text = "Error: Event ID is missing.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                lblMessage.Visible = true;
                return;
            }

            try
            {
                // Get the current user
                currentUser = await firebaseHelper.GetUserById(currentUserID);

                if (currentUser == null)
                {
                    lblMessage.Text = "Error: User not found.";
                    lblMessage.ForeColor = System.Drawing.Color.Red;
                    lblMessage.Visible = true;
                    return;
                }

                // Get event details to check its status
                var eventDetails = await firebaseHelper.GetEventById(eventId);
                if (eventDetails != null)
                {
                    eventDetails.UpdateEventStatus();

                    // If event is completed, don't allow leaving
                    if (eventDetails.EventStatus == "Completed")
                    {
                        lblMessage.Text = "You cannot leave a completed event.";
                        lblMessage.ForeColor = System.Drawing.Color.Red;
                        lblMessage.Visible = true;
                        return;
                    }
                }

                // Check if the user has joined this event
                if (currentUser.MemberData?.EventID_List == null || !currentUser.MemberData.EventID_List.Contains(eventId))
                {
                    lblMessage.Text = "You haven't joined this event.";
                    lblMessage.ForeColor = System.Drawing.Color.Red;
                    lblMessage.Visible = true;
                    return;
                }

                // Remove the event from the user's list
                currentUser.MemberData.EventID_List.Remove(eventId);

                // Update the user in Firebase
                await firebaseHelper.UpdateUserEventList(currentUserID, currentUser.MemberData.EventID_List);

                lblMessage.Text = "You have successfully left the event.";
                lblMessage.ForeColor = System.Drawing.Color.Green;
                lblMessage.CssClass = "success-message";
                lblMessage.Visible = true;

                // Reload the events
                await LoadMyEvents();
            }
            catch (Exception ex)
            {
                lblMessage.Text = "Error leaving event: " + ex.Message;
                lblMessage.ForeColor = System.Drawing.Color.Red;
                lblMessage.CssClass = "error-message";
                lblMessage.Visible = true;
            }
        }
    }
}