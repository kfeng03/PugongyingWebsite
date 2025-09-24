using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Drawing;

namespace fyp
{
    public partial class EM_EventFeedbacks : System.Web.UI.Page
    {
        private FirebaseHelper firebaseHelper = new FirebaseHelper();

        private bool IsUserLoggedIn()
        {
            return Session["UserID"] != null || Session["FirebaseUID"] != null;
        }

        protected async void Page_Load(object sender, EventArgs e)
        {
            // Check if user is logged in
            if (!IsUserLoggedIn())
            {
                Response.Redirect("~/OMTS_Pages/OMTS_AM/AM_LoginGoogle.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            // Check user role - only staff and admin can access
            string userRole = Session["UserRole"]?.ToString();
            if (userRole != "Staff" && userRole != "Admin")
            {
                Response.Redirect("EM_MyEvents.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            if (!IsPostBack)
            {

                // Set visibility for each button based on the user's role
                btnMyEvents.Visible = userRole == "Member" || userRole == "Admin";
                btnJoinEvent.Visible = userRole == "Member" || userRole == "Admin";
                btnCompletedEvent.Visible = userRole == "Member" || userRole == "Admin";
                btnEventManagement.Visible = userRole == "Staff" || userRole == "Admin";
                btnEventFeedbacks.Visible = userRole == "Staff" || userRole == "Admin";

                // Load events dropdown
                await LoadEventsDropdown();
            }
        }

        private async Task LoadEventsDropdown()
        {
            try
            {
                // Get all events
                var events = await firebaseHelper.GetEvents();

                // Filter only completed events
                var completedEvents = events
                    .Where(e => e.IsEventInPast())
                    .OrderByDescending(e => DateTime.Parse(e.EventStartDate ?? e.EventDate))
                    .ToList();

                // Populate dropdown
                ddlEventFilter.Items.Clear();
                ddlEventFilter.Items.Add(new ListItem("Select an Event", ""));

                foreach (var evt in completedEvents)
                {
                    ddlEventFilter.Items.Add(new ListItem(
                        $"{evt.EventTitle} ({evt.EventStartDate ?? evt.EventDate})",
                        evt.EventID
                    ));
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = $"Error loading events: {ex.Message}";
                lblMessage.ForeColor = Color.Red;
                lblMessage.Visible = true;
            }
        }

        protected async void ddlEventFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                string selectedEventId = ddlEventFilter.SelectedValue;

                if (string.IsNullOrEmpty(selectedEventId))
                {
                    rptEventFeedbacks.DataSource = null;
                    rptEventFeedbacks.DataBind();
                    lblNoFeedbacks.Visible = true;
                    return;
                }

                // Get feedbacks for the selected event
                var feedbacks = await firebaseHelper.GetEventFeedbacksByEventID(selectedEventId);

                if (feedbacks == null || !feedbacks.Any())
                {
                    rptEventFeedbacks.DataSource = null;
                    rptEventFeedbacks.DataBind();
                    lblNoFeedbacks.Visible = true;
                }
                else
                {
                    rptEventFeedbacks.DataSource = feedbacks;
                    rptEventFeedbacks.DataBind();
                    lblNoFeedbacks.Visible = false;
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = $"Error loading feedbacks: {ex.Message}";
                lblMessage.ForeColor = Color.Red;
                lblMessage.Visible = true;
            }
        }
    }
}