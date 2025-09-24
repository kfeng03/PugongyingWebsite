using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace fyp
{
    public partial class EM_JoinEvent : System.Web.UI.Page
    {
        private FirebaseHelper firebaseHelper = new FirebaseHelper();
        private string currentUserID;
        private User currentUser;
        private List<Event> availableEvents;

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

                // Load the current user
                currentUser = await firebaseHelper.GetUserById(currentUserID);

                await LoadAvailableEvents(txtSearch.Text.Trim()); // Refresh the list while preserving search results

            }
        }

        // Helper method to get status CSS class
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

        private async Task<int> GetEventParticipantCountAsync(string eventId)
        {
            // Use the helper method to get the actual count
            return await firebaseHelper.GetEventParticipantCount(eventId);
        }

        private async Task LoadAvailableEvents(string searchText = "")
        {
            try
            {
                // Get all active events
                var allEvents = await firebaseHelper.GetActiveEvents();

                // Update the status for each event
                foreach (var ev in allEvents)
                {
                    ev.UpdateEventStatus();
                }

                // Apply search filter if provided
                if (!string.IsNullOrEmpty(searchText))
                {
                    searchText = searchText.ToLower(); // Case-insensitive search
                    allEvents = allEvents.Where(e =>
                        e.EventTitle.ToLower().Contains(searchText) ||
                        e.EventDescription.ToLower().Contains(searchText) ||
                        e.EventLocation.ToLower().Contains(searchText) ||
                        (e.OrganizerName != null && e.OrganizerName.ToLower().Contains(searchText))
                    ).ToList();
                }

                // Pre-fetch all participant counts in one batch
                Dictionary<string, int> participantCounts = new Dictionary<string, int>();
                foreach (var ev in allEvents)
                {
                    // Fetch all participant counts in parallel
                    int count = await firebaseHelper.GetEventParticipantCount(ev.EventID);
                    participantCounts[ev.EventID] = count;
                }

                // Store the participant counts in ViewState for use in ItemDataBound
                ViewState["ParticipantCounts"] = participantCounts;

                // Store events for later use
                availableEvents = allEvents;

                if (!allEvents.Any())
                {
                    rptAvailableEvents.DataSource = null;
                    rptAvailableEvents.DataBind();
                    lblNoEvents.Visible = true;
                    return;
                }

                rptAvailableEvents.DataSource = allEvents;
                rptAvailableEvents.DataBind();
                lblNoEvents.Visible = false;
            }
            catch (Exception ex)
            {
                lblMessage.Text = "Error loading events: " + ex.Message;
                lblMessage.ForeColor = System.Drawing.Color.Red;
                lblMessage.Visible = true;
            }
        }

        protected void rptAvailableEvents_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                var eventItem = (Event)e.Item.DataItem;
                var btnJoin = (Button)e.Item.FindControl("btnJoin");
                var lblJoined = (Label)e.Item.FindControl("lblJoined");
                var lblStatus = (Label)e.Item.FindControl("lblStatus");
                var lblParticipants = (Label)e.Item.FindControl("lblParticipants");

                bool hasJoined = currentUser?.MemberData?.EventID_List?.Contains(eventItem.EventID) ?? false;

                // Update event status
                eventItem.UpdateEventStatus();

                // Get the pre-fetched participant count
                Dictionary<string, int> participantCounts = ViewState["ParticipantCounts"] as Dictionary<string, int>;
                int participantCount = 0;

                if (participantCounts != null && participantCounts.ContainsKey(eventItem.EventID))
                {
                    participantCount = participantCounts[eventItem.EventID];
                }

                // Update the participants display
                if (lblParticipants != null)
                {
                    lblParticipants.Text = $"Participants: {participantCount}/{eventItem.MaxParticipants}";
                }

                // Check if registration is open
                bool isRegistrationOpen = eventItem.IsRegistrationOpen();
                bool isFull = participantCount >= eventItem.MaxParticipants;

                // User can only join upcoming events with open registration
                bool canJoin = eventItem.EventStatus == "Upcoming" && isRegistrationOpen && !isFull;

                // Set appropriate visibility
                btnJoin.Visible = !hasJoined && canJoin;
                lblJoined.Visible = hasJoined;

                // Show appropriate message when user can't join
                if (!hasJoined)
                {
                    if (isFull && !btnJoin.Visible)
                    {
                        lblStatus.Text = "Event Full";
                        lblStatus.CssClass = "badge badge-warning";
                        lblStatus.Visible = true;
                    }
                    else if (!isRegistrationOpen && eventItem.EventStatus == "Upcoming" && !btnJoin.Visible)
                    {
                        // Check if registration hasn't started yet or if it's closed
                        DateTime registrationStart = eventItem.GetRegistrationStartDateTime();
                        if (registrationStart != DateTime.MinValue && registrationStart > DateTime.Now)
                        {
                            lblStatus.Text = "Registration Opens: " + registrationStart.ToString("MMM dd");
                            lblStatus.CssClass = "badge badge-purple";
                        }
                        else
                        {
                            lblStatus.Text = "Registration Closed";
                            lblStatus.CssClass = "badge badge-secondary";
                        }
                        lblStatus.Visible = true;
                    }
                    else if (eventItem.EventStatus == "In Progress" && !btnJoin.Visible)
                    {
                        lblStatus.Text = "Event In Progress";
                        lblStatus.CssClass = "badge badge-info";
                        lblStatus.Visible = true;
                    }
                    else if (eventItem.EventStatus == "Completed" && !btnJoin.Visible)
                    {
                        lblStatus.Text = "Event Completed";
                        lblStatus.CssClass = "badge badge-danger";
                        lblStatus.Visible = true;
                    }
                }
            }
        }
        protected void btnSearch_Click(object sender, EventArgs e)
        {
            string searchText = txtSearch.Text.Trim();
            LoadAvailableEvents(searchText).GetAwaiter().GetResult();

            // Update UI based on search results
            if (rptAvailableEvents.Items.Count == 0)
            {
                lblNoEvents.Text = "No events found matching your search criteria.";
                lblNoEvents.Visible = true;
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

        protected async void btnJoin_Click(object sender, EventArgs e)
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

                // Get event details and verify it's still open for registration
                var eventDetails = await firebaseHelper.GetEventById(eventId);
                if (eventDetails == null)
                {
                    lblMessage.Text = "Error: Event not found.";
                    lblMessage.ForeColor = System.Drawing.Color.Red;
                    lblMessage.Visible = true;
                    return;
                }

                eventDetails.UpdateEventStatus();

                // Check if event is still upcoming and registration is open
                if (eventDetails.EventStatus != "Upcoming" || !eventDetails.IsRegistrationOpen())
                {
                    lblMessage.Text = "You cannot join this event. Registration is closed or event has already started.";
                    lblMessage.ForeColor = System.Drawing.Color.Red;
                    lblMessage.Visible = true;
                    await LoadAvailableEvents(txtSearch.Text.Trim()); // Refresh the list to update status
                    return;
                }

                // Check if the event is full
                int participantCount = await GetEventParticipantCountAsync(eventId);
                if (participantCount >= eventDetails.MaxParticipants)
                {
                    lblMessage.Text = "This event is now full. You cannot join.";
                    lblMessage.ForeColor = System.Drawing.Color.Red;
                    lblMessage.Visible = true;
                    await LoadAvailableEvents(txtSearch.Text.Trim()); // Refresh the list to update status
                    return;
                }

                // Initialize EventID_List if it doesn't exist
                if (currentUser.MemberData == null)
                {
                    currentUser.MemberData = new MemberData
                    {
                        EventID_List = new List<string>()
                    };
                }
                else if (currentUser.MemberData.EventID_List == null)
                {
                    currentUser.MemberData.EventID_List = new List<string>();
                }

                // Check if the user has already joined this event
                if (currentUser.MemberData.EventID_List.Contains(eventId))
                {
                    lblMessage.Text = "You have already joined this event.";
                    lblMessage.ForeColor = System.Drawing.Color.Blue;
                    lblMessage.Visible = true;
                    return;
                }

                // Add the event to the user's list
                currentUser.MemberData.EventID_List.Add(eventId);

                // Update the user in Firebase
                await firebaseHelper.UpdateUserEventList(currentUserID, currentUser.MemberData.EventID_List);

                lblMessage.Text = "You have successfully joined the event!";
                lblMessage.ForeColor = System.Drawing.Color.Green;
                lblMessage.Visible = true;

                // Reload the events to update participant counts
                await LoadAvailableEvents(txtSearch.Text.Trim());
            }
            catch (Exception ex)
            {
                lblMessage.Text = "Error joining event: " + ex.Message;
                lblMessage.ForeColor = System.Drawing.Color.Red;
                lblMessage.Visible = true;
            }
        }
    }
}