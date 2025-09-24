using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using static System.Net.Mime.MediaTypeNames;

namespace fyp
{
    public partial class MA_AwardBadges : System.Web.UI.Page, ILocalizable
    {
        protected FirebaseHelper firebaseHelper;

        protected void Page_PreInit(object sender, EventArgs e)
        {
            if (Session["CurrentLanguage"] != null)
            {
                string selectedLanguage = Session["CurrentLanguage"].ToString();
                Thread.CurrentThread.CurrentCulture = new CultureInfo(selectedLanguage);
                Thread.CurrentThread.CurrentUICulture = new CultureInfo(selectedLanguage);
            }
        }


        // The LocalizeContent method is defined as part of the ILocalizable interface.
        public void LocalizeContent()
        {
            LoadResources(); // This will reload the resources for the page
        }
        private bool IsUserLoggedIn()
        {
            // Check if the user is logged in (ADD IN ALL PAGES THAT NEED ACCESS)
            // Check if the session contains the UserID or FirebaseUID
            return Session["UserID"] != null || Session["FirebaseUID"] != null;
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            // Always initialize firebaseHelper
            firebaseHelper = new FirebaseHelper();

            if (!IsPostBack)
            {
                // Get the current user's role from the session
                string userRole = Session["UserRole"]?.ToString();
                // Set visibility for each button based on the user's role
                btnInformation.Visible = true; // All can access
                btnDashboard.Visible = userRole == "Member" || userRole == "Admin";
                btnApplicationForm.Visible = userRole == "Member" || userRole == "Admin";
                btnReviewApp.Visible = userRole == "Staff" || userRole == "Admin";
                btnAwardBadge.Visible = userRole == "Staff" || userRole == "Admin";
                btnMaterials.Visible = userRole == "Staff" || userRole == "Admin";
                LoadResources();
                Page.DataBind();
                BindUsers();
                BindBadges();

                // Initialize the selection state
                InitializeSelectionState();

                // Make sure summary panel is hidden initially
                pnlSummary.Visible = false;
            }

            // Update the note text with the current selection count
            UpdateAwardBadgeNote();
        }

        private async void BindUsers()
        {
            // Fetch all users from Firebase
            var users = await firebaseHelper.GetUsers();
            // Filter users by role (only "Member" role should appear)
            var filteredUsers = users
                .Where(u => u.Role == "Member") // Filter by role
                .Select(u => new
                {
                    userId = u.UserId,
                    username = u.Username,
                    phoneNumber = u.PhoneNumber,
                    email = u.Email,
                    level = u.MemberData != null ? u.MemberData.Level : 0 // Handle null MemberData
                }).ToList();
            listTable.DataSource = filteredUsers;
            listTable.DataBind();

            // Initialize the selection state after binding the grid
            InitializeSelectionState();

            // Update the note text
            UpdateAwardBadgeNote();
        }
        private async void BindBadges()
        {
            var badges = await firebaseHelper.GetBadges();
            ddlBadges.DataSource = badges;
            ddlBadges.DataTextField = "BadgeName";
            ddlBadges.DataValueField = "BadgeId";
            ddlBadges.DataBind();
        }

        private async Task AwardBadgeToUser(string userId, string badgeId)
        {
            try
            {
                // Ensure firebaseHelper is initialized
                if (firebaseHelper == null)
                {
                    firebaseHelper = new FirebaseHelper();
                }

                // Get current user data
                var user = await firebaseHelper.GetUserById(userId);
                if (user == null || user.MemberData == null)
                {
                    throw new Exception($"User {userId} not found or does not have member data.");
                }

                // Get badge data
                var badge = await firebaseHelper.GetBadgeById(badgeId);
                if (badge == null)
                {
                    throw new Exception($"Badge {badgeId} not found.");
                }

                // Initialize BadgeID_List if it's null
                if (user.MemberData.BadgeID_List == null)
                {
                    user.MemberData.BadgeID_List = new List<string>();
                }

                // Check if user already has this badge
                // Now we're checking if any of the BadgeID_List entries starts with the badge ID
                bool hasAlreadyBeenAwarded = false;
                string existingBadgeStatus = "";

                foreach (string badgeEntry in user.MemberData.BadgeID_List)
                {
                    // Split by colon to separate ID and status
                    string[] parts = badgeEntry.Split(':');
                    if (parts.Length > 0 && parts[0] == badgeId)
                    {
                        hasAlreadyBeenAwarded = true;
                        if (parts.Length > 1)
                        {
                            existingBadgeStatus = parts[1];
                        }
                        break;
                    }
                }

                // Only add the badge if the user doesn't already have it
                if (!hasAlreadyBeenAwarded)
                {
                    // Add badge to user's BadgeID_List with "new" status
                    string badgeWithStatus = $"{badgeId}:new";
                    user.MemberData.BadgeID_List.Add(badgeWithStatus);

                    // Create updates dictionary
                    var updates = new Dictionary<string, object>
            {
                { "MemberData/BadgeID_List", user.MemberData.BadgeID_List }
            };

                    // Update user in Firebase
                    await firebaseHelper.UpdateUserBadges(userId, updates);

                    // Note: Points are not added here automatically anymore
                    // Points will be added when the user claims the badge in another page
                }
                // If the badge was already awarded, we don't modify its status
                // This prevents changing a "claimed" badge back to "new"
            }
            catch (Exception ex)
            {
                // Log the error
                System.Diagnostics.Debug.WriteLine($"Error awarding badge: {ex.Message}");
                throw;
            }
        }


        protected async void btnConfirm_Click(object sender, EventArgs e)
        {
            try
            {
                // Ensure firebaseHelper is initialized
                if (firebaseHelper == null)
                {
                    firebaseHelper = new FirebaseHelper();
                }

                string selectedBadgeId = ddlBadges.SelectedValue;

                if (string.IsNullOrEmpty(selectedBadgeId))
                {
                    ScriptManager.RegisterStartupScript(this, GetType(), "showerror",
                        "alert('Please select a badge to award.');", true);
                    return;
                }

                // Get the badge info
                var badge = await firebaseHelper.GetBadgeById(selectedBadgeId);
                if (badge == null)
                {
                    ScriptManager.RegisterStartupScript(this, GetType(), "showerror",
                        "alert('Selected badge not found.');", true);
                    return;
                }

                // Clear previous summary lists
                lstSuccessful.Items.Clear();
                lstFailed.Items.Clear();

                // Set badge info in summary
                litBadgeInfo.Text = $"Badge: {badge.BadgeName} (ID: {badge.BadgeId})";

                bool anySelected = false;

                // Process each row
                for (int i = 0; i < listTable.Rows.Count; i++)
                {
                    GridViewRow row = listTable.Rows[i];
                    CheckBox chkSelect = (CheckBox)row.FindControl("chkSelect");

                    if (chkSelect != null && chkSelect.Checked)
                    {
                        anySelected = true;
                        string userId = row.Cells[1].Text; // User ID
                        string username = row.Cells[2].Text; // Username

                        // Get the user to check if they already have the badge
                        var user = await firebaseHelper.GetUserById(userId);
                        bool alreadyHasBadge = false;

                        if (user != null && user.MemberData != null && user.MemberData.BadgeID_List != null)
                        {
                            // Check if the user already has this badge
                            alreadyHasBadge = user.MemberData.HasBadge(selectedBadgeId);
                        }

                        if (!alreadyHasBadge)
                        {
                            // Award the badge
                            await AwardBadgeToUser(userId, selectedBadgeId);

                            // Add to successful list
                            lstSuccessful.Items.Add(new ListItem($"{username} ({userId})"));
                        }
                        else
                        {
                            // Add to failed list with reason
                            lstFailed.Items.Add(new ListItem($"{username} ({userId}) - Already has this badge"));
                        }
                    }
                }

                if (!anySelected)
                {
                    // No users were selected
                    ScriptManager.RegisterStartupScript(this, GetType(), "showerror",
                        "alert('Please select at least one member to award the badge.');", true);
                    return;
                }

                // Show the summary panel
                pnlSummary.Visible = true;


                // Update the summary header
                litSummaryHeader.Text = $"Badge Award Summary - {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}";
            }
            catch (Exception ex)
            {
                // Show error message
                ScriptManager.RegisterStartupScript(this, GetType(), "showerror",
                    $"alert('Error awarding badges: {ex.Message}');", true);
            }
        }

        protected void btnDone_Click(object sender, EventArgs e)
        {
            // Hide the summary panel
            pnlSummary.Visible = false;

            // Show the main controls
            listTable.Visible = true;
            selectAllCheckbox.Visible = true;
            ddlBadges.Visible = true;
            btnConfirm.Visible = true;
            lblAwardBadgeNote.Visible = true;

            // Reset all checkboxes
            selectAllCheckbox.Checked = false;
            for (int i = 0; i < listTable.Rows.Count; i++)
            {
                GridViewRow row = listTable.Rows[i];
                CheckBox chkSelect = (CheckBox)row.FindControl("chkSelect");
                if (chkSelect != null)
                {
                    chkSelect.Checked = false;
                }

                // Also update the ViewState
                if (i < SelectedRows.Count)
                {
                    SelectedRows[i] = false;
                }
            }

            // Update the note text
            UpdateAwardBadgeNote();
        }

        // Keep track of selected checkboxes in ViewState
        private List<bool> SelectedRows
        {
            get
            {
                if (ViewState["SelectedRows"] == null)
                {
                    ViewState["SelectedRows"] = new List<bool>();
                }
                return (List<bool>)ViewState["SelectedRows"];
            }
            set
            {
                ViewState["SelectedRows"] = value;
            }
        }



        private void InitializeSelectionState()
        {
            // Initialize the selection state for all rows
            SelectedRows = new List<bool>();
            for (int i = 0; i < listTable.Rows.Count; i++)
            {
                SelectedRows.Add(false);
            }
        }

        protected void selectAllCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            // Update all checkboxes with the select all state
            bool selectAll = selectAllCheckbox.Checked;

            // Update the ViewState
            for (int i = 0; i < listTable.Rows.Count; i++)
            {
                SelectedRows[i] = selectAll;
            }

            // Update the UI checkboxes
            for (int i = 0; i < listTable.Rows.Count; i++)
            {
                CheckBox cb = (CheckBox)listTable.Rows[i].FindControl("chkSelect");
                if (cb != null)
                {
                    cb.Checked = selectAll;
                }
            }

            // Update the note text
            UpdateAwardBadgeNote();
        }

        protected void chkSelect_CheckedChanged(object sender, EventArgs e)
        {
            // Get the checkbox and its row index
            CheckBox checkbox = (CheckBox)sender;
            GridViewRow row = (GridViewRow)checkbox.NamingContainer;
            int rowIndex = row.RowIndex;

            // Update the ViewState
            SelectedRows[rowIndex] = checkbox.Checked;

            // Check if all checkboxes are checked or unchecked
            bool allChecked = true;
            foreach (bool selected in SelectedRows)
            {
                if (!selected)
                {
                    allChecked = false;
                    break;
                }
            }

            // Update the select all checkbox
            selectAllCheckbox.Checked = allChecked;

            // Update the note text
            UpdateAwardBadgeNote();
        }

        protected void listTable_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                // Set the checkbox state based on ViewState
                if (SelectedRows.Count > e.Row.RowIndex)
                {
                    CheckBox cb = (CheckBox)e.Row.FindControl("chkSelect");
                    if (cb != null)
                    {
                        cb.Checked = SelectedRows[e.Row.RowIndex];
                    }
                }
            }
        }

        private void UpdateAwardBadgeNote()
        {
            // Count the number of selected checkboxes
            int selectedCount = 0;
            foreach (bool selected in SelectedRows)
            {
                if (selected)
                {
                    selectedCount++;
                }
            }

            // Get the resource string and replace the placeholder
            string noteText = GetGlobalResourceObject("Resources", "AwardBadge_Note")?.ToString() ?? "{0} member(s) will be awarded with";
            noteText = noteText.Replace("{0}", selectedCount.ToString());

            // Update the literal with the new text
            lblAwardBadgeNote.Text = noteText;
        }

        private void LoadResources()
        {


            // Set page labels and controls from resource files
            litAwardBadges.Text = GetGlobalResourceObject("Resources", "Heading_AwardBadges")?.ToString();


            // Button text
            //lnkSelectAll.Text = GetGlobalResourceObject("Resources", "Link_SelectAll")?.ToString();
            btnInformation.Text = GetGlobalResourceObject("Resources", "SideBtnInfo").ToString();
            btnDashboard.Text = GetGlobalResourceObject("Resources", "SideBtnDb")?.ToString();
            btnApplicationForm.Text = GetGlobalResourceObject("Resources", "SideBtnMAApp")?.ToString();
            btnReviewApp.Text = GetGlobalResourceObject("Resources", "SideBtnRA")?.ToString();
            btnAwardBadge.Text = GetGlobalResourceObject("Resources", "SideBtnAB")?.ToString();
            btnMaterials.Text = GetGlobalResourceObject("Resources", "SideBtnMAMat")?.ToString();
            btnConfirm.Text = GetGlobalResourceObject("Resources", "Button_Confirm")?.ToString();

        }

    }
}