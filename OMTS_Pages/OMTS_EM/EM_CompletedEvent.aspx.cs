using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace fyp
{
    public partial class EM_CompletedEvent : System.Web.UI.Page
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

                try
                {
                    // Load the current user
                    currentUser = await firebaseHelper.GetUserById(currentUserID);
                    await LoadCompletedEvents();
                }
                catch (Exception ex)
                {
                    lblMessage.Text = "Error loading completed events: " + ex.Message;
                    lblMessage.ForeColor = System.Drawing.Color.Red;
                    lblNoEvents.Visible = true;
                }
            }
        }

        private async Task LoadCompletedEvents()
        {
            try
            {
                // Check if user exists and has joined any events
                if (currentUser == null ||
                    currentUser.MemberData?.EventID_List == null ||
                    !currentUser.MemberData.EventID_List.Any())
                {
                    rptCompletedEvents.DataSource = null;
                    rptCompletedEvents.DataBind();
                    lblNoEvents.Visible = true;
                    return;
                }

                // Get all events
                var allEvents = await firebaseHelper.GetEvents();

                // Filter for completed events that the user has joined
                var completedEvents = allEvents
                    .Where(e => currentUser.MemberData.EventID_List.Contains(e.EventID) &&
                           e.IsEventInPast())
                    .ToList();

                if (!completedEvents.Any())
                {
                    rptCompletedEvents.DataSource = null;
                    rptCompletedEvents.DataBind();
                    lblNoEvents.Visible = true;
                    return;
                }

                rptCompletedEvents.DataSource = completedEvents;
                rptCompletedEvents.DataBind();
                lblNoEvents.Visible = false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading completed events: {ex.Message}");
                throw; // Re-throw to be handled by the caller
            }
        }

        protected void btnViewDetails_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            string eventId = btn.CommandArgument;

            if (!string.IsNullOrEmpty(eventId))
            {
                // Store referrer for back button
                Session["EventDetailsReferrer"] = Request.Url.AbsolutePath;
                Response.Redirect($"EM_EventDetails.aspx?id={eventId}");
            }
        }

        protected void btnGiveFeedback_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            string eventId = btn.CommandArgument;

            // Store the event ID in ViewState
            ViewState["CurrentEventId"] = eventId;

            // Show the feedback modal
            pnlFeedbackModal.Visible = true;
        }

        protected async void btnSubmitFeedback_Click(object sender, EventArgs e)
        {
            try
            {
                // Get the event ID from ViewState
                string eventId = ViewState["CurrentEventId"]?.ToString();

                if (string.IsNullOrEmpty(eventId))
                {
                    lblMessage.Text = "Error: Event ID is missing.";
                    lblMessage.ForeColor = System.Drawing.Color.Red;
                    return;
                }

                // Get the feedback text
                string feedbackText = txtFeedback.Text.Trim();

                if (string.IsNullOrEmpty(feedbackText))
                {
                    lblMessage.Text = "Please provide your feedback.";
                    lblMessage.ForeColor = System.Drawing.Color.Red;
                    return;
                }

                // Create feedback object
                var eventFeedback = new EventFeedback
                {
                    EventID = eventId,
                    UserID = currentUserID,
                    FeedbackText = feedbackText,
                    SubmittedDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
                };

                // Save feedback using FirebaseHelper
                await firebaseHelper.SaveEventFeedback(eventFeedback);

                // Hide the modal
                pnlFeedbackModal.Visible = false;

                // Refresh the completed events list
                await LoadCompletedEvents();

                // Reset feedback text and current event ID
                txtFeedback.Text = string.Empty;
                ViewState["CurrentEventId"] = null;

                // Show success message
                lblMessage.Text = "Feedback submitted successfully!";
                lblMessage.ForeColor = System.Drawing.Color.Green;
            }
            catch (Exception ex)
            {
                lblMessage.Text = $"Error submitting feedback: {ex.Message}";
                lblMessage.ForeColor = System.Drawing.Color.Red;
            }
        }

        protected void btnCancelFeedback_Click(object sender, EventArgs e)
        {
            // Hide the feedback modal
            pnlFeedbackModal.Visible = false;
            txtFeedback.Text = string.Empty;
            ViewState["CurrentEventId"] = null;
        }

        protected async void btnViewCertificate_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            string eventId = btn.CommandArgument;

            if (string.IsNullOrEmpty(eventId))
            {
                return;
            }

            try
            {
                // Get event and user details
                var eventData = await firebaseHelper.GetEventById(eventId);
                var user = await firebaseHelper.GetUserById(currentUserID);

                if (eventData == null || user == null)
                {
                    lblMessage.Text = "Error: Could not load event or user details.";
                    lblMessage.ForeColor = System.Drawing.Color.Red;
                    return;
                }

                // Populate certificate details
                litUserName.Text = user.Username;
                litEventName.Text = eventData.EventTitle;

                // Use the event date from the event data
                string eventDate = !string.IsNullOrEmpty(eventData.EventStartDate) ?
                                  eventData.EventStartDate : eventData.EventDate;
                litEventDate.Text = DateTime.Parse(eventDate).ToString("MMMM dd, yyyy");

                // Show the certificate panel
                pnlCertificate.Visible = true;
                ScriptManager.RegisterStartupScript(this, GetType(), "ShowCertificate",
                    "document.querySelector('.certificate-container').style.display = 'flex';", true);
            }
            catch (Exception ex)
            {
                lblMessage.Text = "Error displaying certificate: " + ex.Message;
                lblMessage.ForeColor = System.Drawing.Color.Red;
            }
        }
    }
}