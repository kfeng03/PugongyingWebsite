using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace fyp
{
    public partial class EM_EventDetails : System.Web.UI.Page
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
                ShowError("Event ID not provided.");
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

                await LoadEventDetails();
            }
        }

        private async Task<int> GetEventParticipantCountAsync(string eventId)
        {
            // Use the new helper method to get the actual count
            return await firebaseHelper.GetEventParticipantCount(eventId);
        }

        private async Task LoadEventDetails()
        {
            try
            {
                // Get the event details
                currentEvent = await firebaseHelper.GetEventById(eventId);
                if (currentEvent == null)
                {
                    ShowError("Event not found.");
                    return;
                }

                // Get the current user
                currentUser = await firebaseHelper.GetUserById(currentUserID);
                if (currentUser == null)
                {
                    ShowError("User not found.");
                    return;
                }

                // Update event status
                currentEvent.UpdateEventStatus();

                // Populate the event details
                litEventTitle.Text = currentEvent.EventTitle;
                litOrganizerName.Text = currentEvent.OrganizerName;

                // Show start date (use EventDate for backward compatibility)
                string startDate = !string.IsNullOrEmpty(currentEvent.EventStartDate) ?
                                  currentEvent.EventStartDate : currentEvent.EventDate;
                litEventStartDate.Text = startDate;
                litEventTime.Text = currentEvent.EventTime;

                // Show end date if available
                if (!string.IsNullOrEmpty(currentEvent.EventEndDate))
                {
                    litEventEndDate.Text = currentEvent.EventEndDate;
                    divEndDate.Visible = true;
                }
                else
                {
                    divEndDate.Visible = false;
                }

                // Show registration date if available
                if (!string.IsNullOrEmpty(currentEvent.RegistrationStartDate))
                {
                    litRegistrationDate.Text = currentEvent.RegistrationStartDate;
                    divRegistration.Visible = true;
                }
                else
                {
                    divRegistration.Visible = false;
                }

                litEventLocation.Text = currentEvent.EventLocation;
                litEventDescription.Text = currentEvent.EventDescription;

                // Set event status
                litEventStatus.Text = currentEvent.EventStatus;

                // Set status color
                switch (currentEvent.EventStatus)
                {
                    case "Upcoming":
                        divEventStatus.Attributes["class"] = "event-status status-upcoming";
                        break;
                    case "In Progress":
                        divEventStatus.Attributes["class"] = "event-status status-in-progress";
                        break;
                    case "Completed":
                        divEventStatus.Attributes["class"] = "event-status status-completed";
                        break;
                }

                // Display event image if available
                if (!string.IsNullOrEmpty(currentEvent.EventPictureUrl))
                {
                    imgEventPicture.ImageUrl = currentEvent.EventPictureUrl;
                    imgEventPicture.Visible = true;
                }

                // Display participants count - Use await with the async method
                int participantCount = await GetEventParticipantCountAsync(currentEvent.EventID);
                litParticipants.Text = $"Participants: {participantCount}/{currentEvent.MaxParticipants}";

                // Display event materials
                if (currentEvent.EventMaterials != null && currentEvent.EventMaterials.Count > 0)
                {
                    rptEventMaterials.DataSource = currentEvent.EventMaterials.Keys;
                    rptEventMaterials.DataBind();
                    pnlNoMaterials.Visible = false;
                }
                else
                {
                    pnlNoMaterials.Visible = true;
                }

                // Determine if the user has joined this event
                bool hasJoined = currentUser.MemberData?.EventID_List?.Contains(currentEvent.EventID) ?? false;

                // Show appropriate buttons based on event status, registration status, and user participation
                if (currentEvent.EventStatus == "Upcoming")
                {
                    if (hasJoined)
                    {
                        btnLeave.Visible = true;
                        btnJoin.Visible = false;
                    }
                    else
                    {
                        // Check if registration is open
                        bool isRegistrationOpen = currentEvent.IsRegistrationOpen();

                        if (isRegistrationOpen)
                        {
                            btnJoin.Visible = true;
                            btnLeave.Visible = false;

                            // Check if the event is full - Use await with the async method
                            int currentParticipantCount = await GetEventParticipantCountAsync(currentEvent.EventID);
                            if (currentParticipantCount >= currentEvent.MaxParticipants)
                            {
                                btnJoin.Enabled = false;
                                btnJoin.Text = "Event Full";
                            }
                        }
                        else
                        {
                            btnJoin.Visible = true;
                            btnLeave.Visible = false;
                            btnJoin.Enabled = false;

                            // Check if registration hasn't started yet or if it's closed
                            DateTime registrationStart = currentEvent.GetRegistrationStartDateTime();
                            if (registrationStart != DateTime.MinValue && registrationStart > DateTime.Now)
                            {
                                btnJoin.Text = "Registration Opens: " + registrationStart.ToString("MMM dd");
                            }
                            else
                            {
                                btnJoin.Text = "Registration Closed";
                            }
                        }
                    }
                }
                else if (currentEvent.EventStatus == "In Progress")
                {
                    // For in-progress events, allow leaving but not joining
                    if (hasJoined)
                    {
                        btnLeave.Visible = true;
                        btnJoin.Visible = false;
                    }
                    else
                    {
                        btnJoin.Visible = true;
                        btnLeave.Visible = false;
                        btnJoin.Enabled = false;
                        btnJoin.Text = "Event In Progress";
                    }
                }
                else // Completed
                {
                    btnJoin.Visible = false;
                    btnLeave.Visible = false;
                }
            }
            catch (Exception ex)
            {
                ShowError("Error loading event details: " + ex.Message);
            }
        }

        protected void rptEventMaterials_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                string materialName = e.Item.DataItem.ToString();
                HyperLink hlDownload = (HyperLink)e.Item.FindControl("hlDownload");

                if (hlDownload != null)
                {
                    hlDownload.NavigateUrl = GetMaterialUrl(materialName);
                }
            }
        }

        protected string GetMaterialUrl(string materialName)
        {
            if (currentEvent?.EventMaterials != null && currentEvent.EventMaterials.ContainsKey(materialName))
            {
                return currentEvent.EventMaterials[materialName];
            }
            return "#";
        }

        protected void btnBack_Click(object sender, EventArgs e)
        {
            // Check if we have a stored referrer
            if (Session["EventDetailsReferrer"] != null)
            {
                string referrerPage = Session["EventDetailsReferrer"].ToString();
                Response.Redirect(referrerPage);
                return;
            }

            // Fallback: Check HTTP referrer
            if (Request.UrlReferrer != null)
            {
                string referrerPath = Request.UrlReferrer.AbsolutePath;

                // Determine the page to return to based on the referrer path
                if (referrerPath.Contains("EM_JoinEvent.aspx"))
                {
                    Response.Redirect("EM_JoinEvent.aspx");
                    return;
                }
                else if (referrerPath.Contains("EM_MyEvents.aspx"))
                {
                    Response.Redirect("EM_MyEvents.aspx");
                    return;
                }
                else if (referrerPath.Contains("EM_CompletedEvent.aspx"))
                {
                    Response.Redirect("EM_CompletedEvent.aspx");
                    return;
                }
            }

            // Default fallback - determine where to send the user based on current user's role
            string userRole = Session["UserRole"]?.ToString();
            if (userRole == "Staff" || userRole == "Admin")
            {
                Response.Redirect("EM_EventManagement.aspx");
            }
            else
            {
                Response.Redirect("EM_MyEvents.aspx");
            }
        }

        protected async void btnJoin_Click(object sender, EventArgs e)
        {
            try
            {
                // Get current event status to double-check
                currentEvent = await firebaseHelper.GetEventById(eventId);
                if (currentEvent == null)
                {
                    ShowError("Event not found.");
                    return;
                }

                currentEvent.UpdateEventStatus();

                // Verify event is still upcoming and registration is open
                if (currentEvent.EventStatus != "Upcoming" || !currentEvent.IsRegistrationOpen())
                {
                    ShowError("You cannot join this event. Registration is closed or the event has already started.");
                    await LoadEventDetails(); // Refresh to update UI
                    return;
                }

                // Check if the event is full
                int participantCount = await GetEventParticipantCountAsync(eventId);
                if (participantCount >= currentEvent.MaxParticipants)
                {
                    ShowError("This event is now full. You cannot join.");
                    await LoadEventDetails(); // Refresh to update UI
                    return;
                }

                // Check if the user has already joined
                if (currentUser.MemberData?.EventID_List?.Contains(eventId) ?? false)
                {
                    ShowSuccess("You have already joined this event.");
                    return;
                }

                // Initialize EventID_List if needed
                if (currentUser.MemberData == null)
                {
                    currentUser.MemberData = new MemberData { EventID_List = new List<string>() };
                }
                else if (currentUser.MemberData.EventID_List == null)
                {
                    currentUser.MemberData.EventID_List = new List<string>();
                }

                // Add event to user's list
                currentUser.MemberData.EventID_List.Add(eventId);

                // Update in Firebase
                await firebaseHelper.UpdateUserEventList(currentUserID, currentUser.MemberData.EventID_List);

                ShowSuccess("You have successfully joined the event!");

                // Reload the page to update the UI
                await LoadEventDetails();
            }
            catch (Exception ex)
            {
                ShowError("Error joining event: " + ex.Message);
            }
        }

        protected async void btnLeave_Click(object sender, EventArgs e)
        {
            try
            {
                // Check if the user has joined this event
                if (!(currentUser.MemberData?.EventID_List?.Contains(eventId) ?? false))
                {
                    ShowError("You haven't joined this event.");
                    return;
                }

                // Get current event status to check if leaving is allowed
                currentEvent = await firebaseHelper.GetEventById(eventId);
                if (currentEvent == null)
                {
                    ShowError("Event not found.");
                    return;
                }

                currentEvent.UpdateEventStatus();

                // If event is completed, don't allow leaving
                if (currentEvent.EventStatus == "Completed")
                {
                    ShowError("You cannot leave a completed event.");
                    return;
                }

                // Remove the event from the user's list
                currentUser.MemberData.EventID_List.Remove(eventId);

                // Update in Firebase
                await firebaseHelper.UpdateUserEventList(currentUserID, currentUser.MemberData.EventID_List);

                ShowSuccess("You have left the event.");

                // Reload the page to update the UI and participant count
                await LoadEventDetails();
            }
            catch (Exception ex)
            {
                ShowError("Error leaving event: " + ex.Message);
            }
        }

        private void ShowError(string message)
        {
            lblMessage.Text = message;
            lblMessage.ForeColor = System.Drawing.Color.Red;
        }

        private void ShowSuccess(string message)
        {
            lblMessage.Text = message;
            lblMessage.ForeColor = System.Drawing.Color.Green;
        }
    }
}