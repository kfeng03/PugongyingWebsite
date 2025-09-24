using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace fyp
{
    public partial class EM_EditEvent : Page
    {
        private FirebaseHelper firebaseHelper = new FirebaseHelper();
        private string eventId;
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

                // Get event ID from query string
                eventId = Request.QueryString["eventId"];

                if (string.IsNullOrEmpty(eventId))
                {
                    lblMessage.Text = "Event ID is required!";
                    lblMessage.ForeColor = Color.Red;
                    return;
                }

                hdnEventID.Value = eventId;

                // Load event details
                await LoadEventDetails(eventId);
            }
        }

        // In the LoadEventDetails method:
        private async Task LoadEventDetails(string eventId)
        {
            try
            {
                // Get event from Firebase
                var eventData = await firebaseHelper.GetEventById(eventId);

                if (eventData == null)
                {
                    lblMessage.Text = "Event not found!";
                    lblMessage.ForeColor = Color.Red;
                    return;
                }

                // Populate form fields
                txtEventTitle.Text = eventData.EventTitle;
                txtEventDescription.Text = eventData.EventDescription;

                // For start date, use EventStartDate if available, otherwise use EventDate
                txtEventStartDate.Text = !string.IsNullOrEmpty(eventData.EventStartDate) ?
                                        eventData.EventStartDate : eventData.EventDate;

                // Set end date if available
                txtEventEndDate.Text = eventData.EventEndDate;

                // Set registration start date if available
                txtRegistrationStartDate.Text = eventData.RegistrationStartDate;

                txtEventTime.Text = eventData.EventTime;
                txtEventLocation.Text = eventData.EventLocation;

                // Set dropdown for max participants
                if (eventData.MaxParticipants > 0)
                {
                    ListItem item = ddlMaxParticipants.Items.FindByValue(eventData.MaxParticipants.ToString());
                    if (item != null)
                        item.Selected = true;
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = "Error loading event details: " + ex.Message;
                lblMessage.ForeColor = Color.Red;
            }
        }

        // In the btnUpdateEvent_Click method:
        protected async void btnUpdateEvent_Click(object sender, EventArgs e)
        {
            try
            {
                // Get event ID from hidden field
                string eventId = hdnEventID.Value;

                if (string.IsNullOrEmpty(eventId))
                {
                    lblMessage.Text = "Event ID is missing!";
                    lblMessage.ForeColor = Color.Red;
                    return;
                }

                // Get existing event data
                var existingEvent = await firebaseHelper.GetEventById(eventId);

                if (existingEvent == null)
                {
                    lblMessage.Text = "Event not found!";
                    lblMessage.ForeColor = Color.Red;
                    return;
                }

                // Get user inputs
                string eventTitle = txtEventTitle.Text.Trim();
                string eventDescription = txtEventDescription.Text.Trim();
                string eventStartDate = txtEventStartDate.Text.Trim();
                string eventEndDate = txtEventEndDate.Text.Trim();
                string eventTime = txtEventTime.Text.Trim();
                string eventLocation = txtEventLocation.Text.Trim();
                string registrationStartDate = txtRegistrationStartDate.Text.Trim();
                int maxParticipants = int.Parse(ddlMaxParticipants.SelectedValue);

                // Validate required fields
                if (string.IsNullOrEmpty(eventTitle) || string.IsNullOrEmpty(eventDescription) ||
                    string.IsNullOrEmpty(eventStartDate) ||
                    string.IsNullOrEmpty(eventTime) || string.IsNullOrEmpty(eventLocation))
                {
                    lblMessage.Text = "All required fields (Event Title, Description, Start Date, Time, Location) must be filled!";
                    lblMessage.ForeColor = Color.Red;
                    return;
                }

                // Validate event date and time
                try
                {
                    DateTime startDate = DateTime.Parse(eventStartDate);
                    TimeSpan time = TimeSpan.Parse(eventTime);
                    DateTime eventStartDateTime = new DateTime(startDate.Year, startDate.Month, startDate.Day, time.Hours, time.Minutes, 0);

                    // Validate end date if provided
                    if (!string.IsNullOrEmpty(eventEndDate))
                    {
                        DateTime endDate = DateTime.Parse(eventEndDate);
                        DateTime eventEndDateTime = new DateTime(endDate.Year, endDate.Month, endDate.Day, time.Hours, time.Minutes, 0);

                        // End date must be after start date
                        if (eventEndDateTime < eventStartDateTime)
                        {
                            lblMessage.Text = "Event end date must be after the start date!";
                            lblMessage.ForeColor = Color.Red;
                            return;
                        }
                    }

                    // Validate registration date if provided
                    if (!string.IsNullOrEmpty(registrationStartDate))
                    {
                        DateTime regStartDate = DateTime.Parse(registrationStartDate);

                        // Registration date must be before the event start date
                        if (regStartDate > startDate)
                        {
                            lblMessage.Text = "Registration start date must be before the event start date!";
                            lblMessage.ForeColor = Color.Red;
                            return;
                        }
                    }
                }
                catch (Exception ex)
                {
                    lblMessage.Text = "Invalid date or time format. Please check and try again.";
                    lblMessage.ForeColor = Color.Red;
                    return;
                }

                // Preserve existing values for the fields we're not changing
                // Note: We're keeping the existing values for organizerName, eventPictureUrl, and eventMaterials

                // Update event properties
                existingEvent.EventTitle = eventTitle;
                existingEvent.EventDescription = eventDescription;
                existingEvent.EventStartDate = eventStartDate;
                existingEvent.EventEndDate = eventEndDate;
                existingEvent.EventDate = eventStartDate; // For backward compatibility
                existingEvent.EventTime = eventTime;
                existingEvent.EventLocation = eventLocation;
                existingEvent.RegistrationStartDate = registrationStartDate;
                existingEvent.MaxParticipants = maxParticipants;

                // Update event status based on the new date and time
                existingEvent.UpdateEventStatus();

                // Update event in Firebase
                await firebaseHelper.UpdateEvent(existingEvent);

                // Redirect after successful update
                Response.Redirect("EM_EventManagement.aspx", false);
                Context.ApplicationInstance.CompleteRequest(); // Prevents further processing
            }
            catch (Exception ex)
            {
                lblMessage.Text = "Error updating event: " + ex.Message;
                lblMessage.ForeColor = Color.Red;
            }
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            Response.Redirect("EM_EventManagement.aspx");
        }
    }
}