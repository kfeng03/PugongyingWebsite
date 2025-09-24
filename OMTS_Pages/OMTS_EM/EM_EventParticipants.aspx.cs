using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace fyp
{
    public partial class EM_EventParticipants : System.Web.UI.Page
    {
        private FirebaseHelper firebaseHelper = new FirebaseHelper();
        private string eventId;
        private Event currentEvent;

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

            // Check user role - only staff and admin can access this page
            string userRole = Session["UserRole"]?.ToString();
            if (userRole != "Staff" && userRole != "Admin")
            {
                Response.Redirect("EM_MyEvents.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            // Get the event ID from the query string
            eventId = Request.QueryString["id"];
            if (string.IsNullOrEmpty(eventId))
            {
                Response.Redirect("EM_EventManagement.aspx", false);
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

                await LoadEventAndParticipants();
            }
        }

        private async Task LoadEventAndParticipants()
        {
            try
            {
                // Get event details
                currentEvent = await firebaseHelper.GetEventById(eventId);
                if (currentEvent == null)
                {
                    lblMessage.Text = "Event not found.";
                    return;
                }

                // Set event information
                litEventTitle.Text = currentEvent.EventTitle;

                // Display event info with date and time
                string startDate = !string.IsNullOrEmpty(currentEvent.EventStartDate) ?
                                  currentEvent.EventStartDate : currentEvent.EventDate;
                string eventInfo = $"Date: {startDate}";

                if (!string.IsNullOrEmpty(currentEvent.EventEndDate))
                    eventInfo += $" - {currentEvent.EventEndDate}";

                eventInfo += $" | Time: {currentEvent.EventTime}";
                eventInfo += $" | Location: {currentEvent.EventLocation}";
                eventInfo += $" | Organizer: {currentEvent.OrganizerName}";

                litEventInfo.Text = eventInfo;

                // Load participants
                await LoadParticipants();
            }
            catch (Exception ex)
            {
                lblMessage.Text = "Error loading event details: " + ex.Message;
            }
        }

        private async Task LoadParticipants()
        {
            try
            {
                // Get all users from Firebase
                var allUsers = await firebaseHelper.GetUsers();
                if (allUsers == null || !allUsers.Any())
                {
                    return;
                }

                // Filter users who have this event in their EventID_List
                var participants = allUsers
                    .Where(u => u.MemberData?.EventID_List != null &&
                                u.MemberData.EventID_List.Contains(eventId))
                    .Select(u => new ParticipantInfo
                    {
                        UserId = u.UserId,
                        Username = u.Username,
                        Email = u.Email,
                        PhoneNumber = u.PhoneNumber,
                        JoinDate = u.JoinDate
                    })
                    .ToList();

                // Bind participants to the GridView
                gvParticipants.DataSource = participants;
                gvParticipants.DataBind();
            }
            catch (Exception ex)
            {
                lblMessage.Text = "Error loading participants: " + ex.Message;
            }
        }

        protected void btnBack_Click(object sender, EventArgs e)
        {
            Response.Redirect("EM_EventManagement.aspx");
        }

        protected void btnExport_Click(object sender, EventArgs e)
        {
            try
            {
                // Create CSV content
                StringBuilder csv = new StringBuilder();

                // Add headers
                csv.AppendLine("User ID,Username,Email,Phone Number,Join Date");

                // Add data from GridView
                foreach (GridViewRow row in gvParticipants.Rows)
                {
                    string userId = row.Cells[0].Text;
                    string username = row.Cells[1].Text;
                    string email = row.Cells[2].Text;
                    string phoneNumber = row.Cells[3].Text;
                    string joinDate = row.Cells[4].Text;

                    // Properly escape values with quotes if they contain commas
                    username = EscapeCsvValue(username);
                    email = EscapeCsvValue(email);
                    phoneNumber = EscapeCsvValue(phoneNumber);
                    joinDate = EscapeCsvValue(joinDate);

                    csv.AppendLine($"{userId},{username},{email},{phoneNumber},{joinDate}");
                }

                // Set response headers for file download
                Response.Clear();
                Response.Buffer = true;
                Response.AddHeader("content-disposition",
                    $"attachment;filename=Participants_{currentEvent.EventTitle.Replace(" ", "_")}_{DateTime.Now.ToString("yyyyMMdd")}.csv");
                Response.Charset = "";
                Response.ContentType = "application/text";
                Response.Output.Write(csv.ToString());
                Response.Flush();
                Response.End();
            }
            catch (Exception ex)
            {
                lblMessage.Text = "Error exporting participants: " + ex.Message;
            }
        }

        private string EscapeCsvValue(string value)
        {
            // If value contains comma, quotes, or newline, wrap in quotes and escape inner quotes
            if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
            {
                return "\"" + value.Replace("\"", "\"\"") + "\"";
            }
            return value;
        }
    }

    public class ParticipantInfo
    {
        public string UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string JoinDate { get; set; }
    }
}