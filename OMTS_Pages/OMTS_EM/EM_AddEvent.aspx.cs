using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;

namespace fyp
{
    public partial class EM_AddEvent : Page
    {
        private readonly FirebaseHelper _firebase = new FirebaseHelper();

        private bool IsUserLoggedIn()
        {
            // Check if the user is logged in (ADD IN ALL PAGES THAT NEED ACCESS)
            // Check if the session contains the UserID or FirebaseUID
            return Session["UserID"] != null || Session["FirebaseUID"] != null;
        }
        protected void Page_Load(object sender, EventArgs e)
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
                        Page.DataBind();   // only bind on first load

                // Get the current user's role from the session
                string userRole = Session["UserRole"]?.ToString();
                 // Set visibility for each button based on the user's role
                btnMyEvents.Visible = userRole == "Member" || userRole == "Admin";
                btnJoinEvent.Visible = userRole == "Member" || userRole == "Admin";
                btnCompletedEvent.Visible = userRole == "Member" || userRole == "Admin";
                btnEventManagement.Visible = userRole == "Staff" || userRole == "Admin";
                btnEventFeedbacks.Visible = userRole == "Staff" || userRole == "Admin";


            }
        }
        // --- Helpers ---------------------------------------------------------

        private void ShowMsg(string msg, bool ok = false)
        {
            lblMessage.Visible = true;
            lblMessage.Text = msg;
            lblMessage.ForeColor = ok ? Color.Green : Color.Red;
        }

        private bool TryGetDateAndTime(out DateTime startDate, out DateTime? endDate, out TimeSpan time)
        {
            startDate = default;
            endDate = null;
            time = default;

            // WebForms TextMode="Date" -> yyyy-MM-dd ; TextMode="Time" -> HH:mm
            var dateFmt = "yyyy-MM-dd";
            var timeFmt = "hh\\:mm";          // TimeSpan.ParseExact format
            var timeFmt24 = "HH\\:mm";

            if (!DateTime.TryParseExact(txtEventStartDate.Text.Trim(), dateFmt, CultureInfo.InvariantCulture,
                                        DateTimeStyles.None, out var dStart))
            {
                ShowMsg("Invalid Start Date. Please use the date picker.");
                return false;
            }

            if (!TimeSpan.TryParseExact(txtEventTime.Text.Trim(), new[] { timeFmt24, timeFmt },
                                        CultureInfo.InvariantCulture, out var t))
            {
                ShowMsg("Invalid Event Time. Please use the time picker (HH:mm).");
                return false;
            }

            DateTime? dEnd = null;
            var endRaw = txtEventEndDate.Text.Trim();
            if (!string.IsNullOrEmpty(endRaw))
            {
                if (!DateTime.TryParseExact(endRaw, dateFmt, CultureInfo.InvariantCulture,
                                            DateTimeStyles.None, out var dE))
                {
                    ShowMsg("Invalid End Date. Please use the date picker.");
                    return false;
                }
                dEnd = dE;
            }

            startDate = dStart;
            endDate = dEnd;
            time = t;
            return true;
        }

        // --- CLICK HANDLER (drop-in replacement) ----------------------------

        protected void btnSaveEvent_Click(object sender, EventArgs e)
        {
            // Use WebForms async pattern to avoid async void pitfalls
            RegisterAsyncTask(new PageAsyncTask(SaveEventAsync));
        }

        private async Task SaveEventAsync()
        {
            try
            {
                // Basic required inputs
                var title = txtEventTitle.Text.Trim();
                var desc = txtEventDescription.Text.Trim();
                var loc = txtEventLocation.Text.Trim();
                var regStartRaw = txtRegistrationStartDate.Text.Trim();
                var organizerName = Session["Username"]?.ToString() ?? "System";

                if (string.IsNullOrEmpty(title) ||
                    string.IsNullOrEmpty(desc) ||
                    string.IsNullOrEmpty(loc))
                {
                    ShowMsg("Please fill in Event Title, Description, and Location.");
                    return;
                }

                if (!TryGetDateAndTime(out var startDate, out var endDate, out var t))
                    return;

                var startDateTime = new DateTime(startDate.Year, startDate.Month, startDate.Day, t.Hours, t.Minutes, 0);

                // Business rules
                if (startDateTime <= DateTime.Now)
                {
                    ShowMsg("Event start must be in the future.");
                    return;
                }

                DateTime? endDateTime = null;
                if (endDate.HasValue)
                {
                    endDateTime = new DateTime(endDate.Value.Year, endDate.Value.Month, endDate.Value.Day, t.Hours, t.Minutes, 0);
                    if (endDateTime <= startDateTime)
                    {
                        ShowMsg("End date must be after the start date.");
                        return;
                    }
                }

                DateTime? regStartDate = null;
                if (!string.IsNullOrWhiteSpace(regStartRaw))
                {
                    if (!DateTime.TryParseExact(regStartRaw.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture,
                                                DateTimeStyles.None, out var r))
                    {
                        ShowMsg("Invalid Registration Start Date.");
                        return;
                    }
                    if (r > startDate)
                    {
                        ShowMsg("Registration Start Date must be on/before the Event Start Date.");
                        return;
                    }
                    regStartDate = r;
                }

                if (!int.TryParse(ddlMaxParticipants.SelectedValue, out var maxPax) || maxPax <= 0)
                {
                    ShowMsg("Please select a valid Maximum Participants value.");
                    return;
                }

                // Build payload
                var ev = new Event
                {
                    EventTitle = title,
                    EventDescription = desc,
                    EventStartDate = startDate.ToString("yyyy-MM-dd"),
                    EventEndDate = endDate?.ToString("yyyy-MM-dd") ?? string.Empty,
                    EventDate = startDate.ToString("yyyy-MM-dd"),   // legacy/back-compat
                    EventTime = txtEventTime.Text.Trim(),           // keep raw "HH:mm"
                    EventLocation = loc,
                    OrganizerName = organizerName,
                    MaxParticipants = maxPax,
                    EventPictureUrl = null,
                    EventMaterials = new Dictionary<string, string>(),
                    RegistrationStartDate = regStartDate?.ToString("yyyy-MM-dd") ?? string.Empty,
                    IsDeleted = false,
                    EventStatus = "Upcoming",
                };

                // Persist
                var ok = await _firebase.AddEvent(ev);
                if (!ok)
                {
                    ShowMsg("Failed to add event. (Firebase returned false)");
                    return;
                }

                // Success -> redirect
                // (Use false + CompleteRequest to avoid ThreadAbortException)
                Response.Redirect("EM_EventManagement.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
            }
            catch (Exception ex)
            {
                // Surface the real reason
                ShowMsg("Save failed: " + ex.Message);
                System.Diagnostics.Trace.TraceError("EM_AddEvent Save failed: " + ex);
            }
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            Response.Redirect("EM_EventManagement.aspx");
        }
    }
}