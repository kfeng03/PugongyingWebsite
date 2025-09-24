using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace fyp
{
    public partial class MA_AchievementDashboard : System.Web.UI.Page, ILocalizable
    {
        protected FirebaseHelper firebaseHelper;
        protected CloudinaryHelper cloudinaryHelper;

        private User currentUser;
        private List<DashboardBadge> allBadges;

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
            // Check if the user is logged in
            return Session["UserID"] != null || Session["FirebaseUID"] != null;
        }

        protected async void Page_Load(object sender, EventArgs e)
        {
            // Initialize Firebase helper
            firebaseHelper = new FirebaseHelper();

            if (!IsUserLoggedIn())
            {
                // Redirect to login if not logged in
                Response.Redirect("~/OMTS_Pages/OMTS_AM/AM_LoginGoogle.aspx", false);
                Context.ApplicationInstance.CompleteRequest(); // Prevents further processing
                return;
            }

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

                await LoadUserData();
                await LoadUserApplications();
                await LoadBadges();

                // Check for badge ID in query string
                string badgeId = Request.QueryString["badge"];
                if (!string.IsNullOrEmpty(badgeId))
                {
                    await ShowBadgePopup(badgeId);
                }

                // Check for history view in query string
                string view = Request.QueryString["view"];
                if (view == "history")
                {
                    // Show history popup via JavaScript
                    ScriptManager.RegisterStartupScript(this, GetType(), "showHistoryPopup",
                        "$('.overlay').fadeIn(); $('.popup2').fadeIn();", true);
                }
            }
        }

        private async Task ShowBadgePopup(string badgeId)
        {
            try
            {
                // Find the badge in the list
                var badge = allBadges.FirstOrDefault(b => b.BadgeId == badgeId);
                if (badge == null)
                {
                    ShowError("Badge not found.");
                    return;
                }

                // Set badge details in the popup
                litBadgeNameValue.Text = badge.BadgeName;
                imgBadge.ImageUrl = badge.BadgeIconUrl;
                litBadgeDescValue.Text = badge.BadgeDesc;
                litPointValue.Text = badge.BadgePoints.ToString() + " points";

                // Set badge ID in hidden field
                hdnSelectedBadgeId.Value = badge.BadgeId;

                // Determine unlock status
                bool isUnlockable = badge.Status == "new";

                // Set visibility of unlock button and note
                btnUnlock.Visible = isUnlockable;
                lblUnlockNote.Visible = isUnlockable;

                // Register script to show the badge popup
                ScriptManager.RegisterStartupScript(this, GetType(), "showBadgePopup",
                    $"$(document).ready(function() {{ " +
                    $"  $('.overlay').fadeIn(); " +
                    $"  $('.popup1').fadeIn(); " +
                    $"}});", true);
            }
            catch (Exception ex)
            {
                ShowError($"Error showing badge details: {ex.Message}");
            }
        }

        private async Task LoadUserData()
        {
            try
            {
                string userId = Session["UserID"]?.ToString();
                if (string.IsNullOrEmpty(userId))
                {
                    // Try to get user ID from Firebase UID if available
                    string firebaseUid = Session["FirebaseUID"]?.ToString();
                    if (!string.IsNullOrEmpty(firebaseUid))
                    {
                        var userInfo = await firebaseHelper.GetUserByFirebaseUid(firebaseUid);
                        userId = userInfo.UserId;
                    }
                }

                if (string.IsNullOrEmpty(userId))
                {
                    // Still no user ID, redirect to login
                    Response.Redirect("~/OMTS_Pages/OMTS_AM/AM_LoginHP.aspx", false);
                    Context.ApplicationInstance.CompleteRequest(); // Prevents further processing
                    return;
                }

                // Get user data

                currentUser = await firebaseHelper.GetUserById(userId);
                if (currentUser == null)
                {
                    ShowError("User data not found.");
                    return;
                }

                // Set user profile data
                lblUsername.Text = currentUser.Username;

                // Format join date
                if (!string.IsNullOrEmpty(currentUser.JoinDate))
                {
                    try
                    {
                        DateTime joinDate = DateTime.Parse(currentUser.JoinDate);
                        lblJoinDate.Text = joinDate.ToString("MMMM d, yyyy");
                    }
                    catch
                    {
                        lblJoinDate.Text = currentUser.JoinDate;
                    }
                }

                // Set profile picture if available
                if (!string.IsNullOrEmpty(currentUser.ProfilePictureUrl))
                {
                    imgProfile.ImageUrl = currentUser.ProfilePictureUrl;
                }

                // Set member stats
                if (currentUser.MemberData != null)
                {
                    //under application --> filter user
                    var allApplications = await firebaseHelper.GetAchievementApplications();
                    var userApplications = allApplications
                        .Where(a => a.ApplicantRefId == currentUser.UserId &&
                                    a.Status == "Approved")
                        .ToList();
                    int achieveCount = userApplications.Count;

                    //under user tree
                    int courseCount = currentUser.MemberData.CourseID_List?.Count ?? 0;

                    lblAchievements.Text = achieveCount.ToString();
                    lblCoursesCompleted.Text = courseCount.ToString();

                    int currentPoints = currentUser.MemberData.Points;
                    int requiredPoints = await GetMaxPointsPerLevel();
                    int currentLvl = currentUser.MemberData.Level;
                    int progressPercentage = (int)(((double)currentPoints / requiredPoints) * 100);

                    // Set progress width dynamically
                    progressBar.Style["width"] = progressPercentage + "%";

                    // Update text label
                    lblPoints.Text = $"{currentPoints}/{requiredPoints} Points";
                    lblCurrLvl.Text = currentLvl.ToString();
                    lblNextLvl.Text = (currentLvl + 1).ToString();

                    // Show level up button if points are enough for next level
                    btnLevelUp.Visible = currentPoints >= requiredPoints;
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error loading user data: {ex.Message}");
            }
        }
        private async Task<int> GetMaxPointsPerLevel()
        {
            try
            {
                SiteSettingsHelper settingsHelper = new SiteSettingsHelper();
                var settings = await settingsHelper.GetSiteSettings();

                if (settings != null && settings.MaxPointsPerLevel > 0)
                {
                    return settings.MaxPointsPerLevel;
                }

                return 100; // Default if settings not found
            }
            catch (Exception)
            {
                return 100; // Default if error occurs
            }
        }
        private async Task LoadUserApplications()
        {
            try
            {
                if (currentUser == null || string.IsNullOrEmpty(currentUser.UserId))
                {
                    return;
                }

                // Get all applications submitted by the current user
                var allApplications = await firebaseHelper.GetAchievementApplications();
                var userApplications = allApplications.Where(a => a.ApplicantRefId == currentUser.UserId).ToList();

                // Prepare data for grid view
                var displayData = new List<object>();

                foreach (var app in userApplications)
                {
                    // Get event details
                    string eventName = "Unknown";
                    if (!string.IsNullOrEmpty(app.EventId))
                    {
                        try
                        {
                            var eventInfo = await firebaseHelper.GetEventById(app.EventId);
                            if (eventInfo != null)
                            {
                                eventName = eventInfo.EventTitle;
                            }
                        }
                        catch
                        {
                            // If there's an error getting the event, use the default "Unknown"
                        }
                    }

                    // Add to display data
                    displayData.Add(new
                    {
                        applicationId = app.ApplicationId,
                        eventName = eventName,
                        participatedRole = app.ParticipatedRole,
                        status = app.Status ?? "Pending"
                    });
                }

                // Bind to GridView
                gvApplications.DataSource = displayData;
                gvApplications.DataBind();
            }
            catch (Exception ex)
            {
                ShowError($"Error loading applications: {ex.Message}");
            }
        }

        private async Task LoadBadges()
        {
            try
            {
                // Get all badges
                var badges = await firebaseHelper.GetBadges();

                // Filter out deleted badges
                badges = badges.Where(b => !b.IsDeleted).ToList();

                // Create the badge list for the dashboard
                allBadges = new List<DashboardBadge>();

                foreach (var badge in badges)
                {
                    string badgeStatus = "locked"; // Default status

                    // Check if user has this badge
                    if (currentUser?.MemberData?.BadgeID_List != null)
                    {
                        foreach (string badgeEntry in currentUser.MemberData.BadgeID_List)
                        {
                            // Split by colon to separate ID and status
                            string[] parts = badgeEntry.Split(':');
                            if (parts.Length > 0 && parts[0] == badge.BadgeId)
                            {
                                if (parts.Length > 1)
                                {
                                    badgeStatus = parts[1]; // Get status (new or claimed)
                                }
                                else
                                {
                                    badgeStatus = "claimed"; // Default for old format
                                }
                                break;
                            }
                        }
                    }

                    // Add to dashboard badges list
                    allBadges.Add(new DashboardBadge
                    {
                        BadgeId = badge.BadgeId,
                        BadgeName = badge.BadgeName,
                        BadgeDesc = badge.BadgeDesc,
                        BadgeIconUrl = badge.BadgeIconUrl,
                        BadgePoints = badge.BadgePoints,
                        Status = badgeStatus
                    });
                }

                // Bind badges to repeater
                rptBadges.DataSource = allBadges;
                rptBadges.DataBind();
            }
            catch (Exception ex)
            {
                ShowError($"Error loading badges: {ex.Message}");
            }
        }

        protected void rptBadges_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                // Get the badge from the data item
                var badge = (DashboardBadge)e.Item.DataItem;

                // Find the badge image control
                System.Web.UI.WebControls.Image imgBadge = (System.Web.UI.WebControls.Image)e.Item.FindControl("imgBadge");

                if (imgBadge != null)
                {
                    // Add onclick attribute to navigate with query string
                    imgBadge.Attributes.Add("onclick", $"window.location.href='MA_AchievementDashboard.aspx?badge={badge.BadgeId}'; return false;");
                    imgBadge.Style.Add("cursor", "pointer");
                }
            }
        }

        protected string GetBadgeCssClass(string status)
        {
            switch (status.ToLower())
            {
                case "new":
                    return "badge-new";
                case "claimed":
                    return "badge-earned";
                case "locked":
                default:
                    return "badge-locked";
            }
        }

        protected string GetStatusCssClass(string status)
        {
            switch (status?.ToLower())
            {
                case "approved":
                    return "status-approved";
                case "rejected":
                    return "status-rejected";
                case "pending":
                default:
                    return "status-pending";
            }
        }

        protected void gvApplications_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "ViewDetails")
            {
                string applicationId = e.CommandArgument.ToString();

                // Redirect to application details page
                Response.Redirect($"MA_ReviewAppDetail.aspx?id={applicationId}");
            }
        }
        protected async void btnUnlock_Click(object sender, EventArgs e)
        {
            try
            {
                // Step 1: Get the current user
                string userId = Session["UserID"]?.ToString();
                if (string.IsNullOrEmpty(userId))
                {
                    ShowError("User not logged in.");
                    return;
                }

                // Step 2: Get the current user's data
                var user = await firebaseHelper.GetUserById(userId);
                if (user == null || user.MemberData == null)
                {
                    ShowError("User data not found.");
                    return;
                }

                // Step 3: Get the selected badge ID
                string badgeId = hdnSelectedBadgeId.Value;
                if (string.IsNullOrEmpty(badgeId))
                {
                    ShowError("No badge selected.");
                    return;
                }

                // Step 4: Get the badge data
                var badge = await firebaseHelper.GetBadgeById(badgeId);
                if (badge == null)
                {
                    ShowError("Selected badge not found.");
                    return;
                }

                // Step 5: Ensure BadgeID_List is initialized
                if (user.MemberData.BadgeID_List == null)
                {
                    user.MemberData.BadgeID_List = new List<string>();
                }

                // Step 6: Check if the user already has the badge
                bool hasBadge = false;
                string existingBadgeStatus = "";

                for (int i = 0; i < user.MemberData.BadgeID_List.Count; i++)
                {
                    string badgeEntry = user.MemberData.BadgeID_List[i];
                    string[] parts = badgeEntry.Split(':');
                    if (parts.Length > 0 && parts[0] == badgeId)
                    {
                        hasBadge = true;
                        if (parts.Length > 1)
                        {
                            existingBadgeStatus = parts[1];
                        }
                        break;
                    }
                }

                // Step 7: If the badge exists and is in "new" status, update it to "claimed"
                if (hasBadge && existingBadgeStatus == "new")
                {
                    // Update the badge status to "claimed"
                    for (int i = 0; i < user.MemberData.BadgeID_List.Count; i++)
                    {
                        string badgeEntry = user.MemberData.BadgeID_List[i];
                        string[] parts = badgeEntry.Split(':');
                        if (parts.Length > 0 && parts[0] == badgeId)
                        {
                            user.MemberData.BadgeID_List[i] = $"{badgeId}:claimed";
                            break;
                        }
                    }

                    // Step 8: Add points to the user
                    user.MemberData.Points += badge.BadgePoints;



                    // Step 10: Prepare updates for Firebase
                    var updates = new Dictionary<string, object>
                    {
                        { "MemberData/BadgeID_List", user.MemberData.BadgeID_List },
                        { "MemberData/Points", user.MemberData.Points },
                        { "MemberData/Level", user.MemberData.Level }
                    };

                    // Step 11: Update Firebase
                    await firebaseHelper.UpdateUserBadges(userId, updates);

                    // Step 12: Show success message and reload the page
                    ScriptManager.RegisterStartupScript(this, GetType(), "badgeClaimed",
                        $"alert('Badge claimed successfully! You earned {badge.BadgePoints} points.'); " +
                        "window.location.href = 'MA_AchievementDashboard.aspx';", true);
                }
                else
                {
                    // If the badge is not in "new" status, show an error
                    ShowError("This badge cannot be claimed or has already been claimed.");
                }
            }
            catch (Exception ex)
            {
                // Log the full exception
                System.Diagnostics.Debug.WriteLine($"Error in btnUnlock_Click: {ex}");
                ShowError($"Error claiming badge: {ex.Message}");
            }
        }

        protected async void btnLevelUp_Click(object sender, EventArgs e)
        {
            try
            {
                // Step 1: Get the current user
                string userId = Session["UserID"]?.ToString();
                if (string.IsNullOrEmpty(userId))
                {
                    ShowError("User not logged in.");
                    return;
                }

                // Step 2: Get the current user's data
                var user = await firebaseHelper.GetUserById(userId);
                if (user == null || user.MemberData == null)
                {
                    ShowError("User data not found.");
                    return;
                }

                // Step 3: Check if the user has enough points to level up
                int pointsRequiredForNextLevel = await GetMaxPointsPerLevel();
                if (user.MemberData.Points < pointsRequiredForNextLevel)
                {
                    ShowError("Not enough points to level up.");
                    return;
                }

                // Step 4: Increment the user's level
                user.MemberData.Level += 1;

                // Step 5: Subtract the points required for the current level
                user.MemberData.Points -= pointsRequiredForNextLevel;

                // Step 6: Prepare updates for Firebase
                var updates = new Dictionary<string, object>
                {
                    { "MemberData/Level", user.MemberData.Level },
                    { "MemberData/Points", user.MemberData.Points }
                };

                // Step 7: Update Firebase
                await firebaseHelper.UpdateUserBadges(userId, updates);

                // Step 8: Show success message and reload the page
                ScriptManager.RegisterStartupScript(this, GetType(), "levelUpSuccess",
                    $"alert('Congratulations! You are now Level {user.MemberData.Level}.'); " +
                    "window.location.href = 'MA_AchievementDashboard.aspx';", true);
            }
            catch (Exception ex)
            {
                // Log the full exception
                System.Diagnostics.Debug.WriteLine($"Error in btnLevelUp_Click: {ex}");
                ShowError($"Error leveling up: {ex.Message}");
            }
        }


        private void LoadResources()
        {
            // Set page labels and controls from resource files
            litMyDashboard.Text = GetGlobalResourceObject("Resources", "Heading_MyDashboard")?.ToString();
            litBadgeInfo.Text = GetGlobalResourceObject("Resources", "Heading_BadgeInfo")?.ToString();
            litRecentApplication.Text = GetGlobalResourceObject("Resources", "Heading_ListOfRecentApplications")?.ToString();

            // Button text
            btnInformation.Text = GetGlobalResourceObject("Resources", "SideBtnInfo")?.ToString();
            btnDashboard.Text = GetGlobalResourceObject("Resources", "SideBtnDb")?.ToString();
            btnApplicationForm.Text = GetGlobalResourceObject("Resources", "SideBtnMAApp")?.ToString();
            btnReviewApp.Text = GetGlobalResourceObject("Resources", "SideBtnRA")?.ToString();
            btnAwardBadge.Text = GetGlobalResourceObject("Resources", "SideBtnAB")?.ToString();
            btnMaterials.Text = GetGlobalResourceObject("Resources", "SideBtnMAMat")?.ToString();

            btnLevelUp.Text = GetGlobalResourceObject("Resources", "Button_LevelUp")?.ToString();
            btnExport.Text = GetGlobalResourceObject("Resources", "Button_ExportTestimonial")?.ToString();
            btnUnlock.Text = GetGlobalResourceObject("Resources", "Button_Unlock")?.ToString();
            lnkHistory.Text = GetGlobalResourceObject("Resources", "Link_AchievementHistory")?.ToString();
        }

        protected async void btnExport_Click(object sender, EventArgs e)
        {
            try
            {
                string userId = Session["UserID"]?.ToString();
                currentUser = await firebaseHelper.GetUserById(userId);

                // Check if user is logged in
                if (currentUser == null)
                {
                    ShowError("Please log in to export your testimonial.");
                    return;
                }

                // Check user level requirement (Level 5+)
                int userLevel = currentUser.MemberData?.Level ?? 0;
                if (userLevel < 5)
                {
                    ShowError("You must be at least Level 5 to export a testimonial. Your current level is " + userLevel + ".");
                    return;
                }

                // Check badge count requirement (3+ badges)
                int claimedBadgeCount = 0;
                if (currentUser.MemberData?.BadgeID_List != null)
                {
                    foreach (string badgeEntry in currentUser.MemberData.BadgeID_List)
                    {
                        string[] parts = badgeEntry.Split(':');
                        if (parts.Length > 1 && parts[1] == "claimed")
                        {
                            claimedBadgeCount++;
                        }
                    }
                }

                if (claimedBadgeCount < 3)
                {
                    ShowError($"You must have at least 3 badges to export a testimonial. You currently have {claimedBadgeCount} badge(s).");
                    return;
                }

                // Show a loading message
                lblMessage.Text = "Generating your testimonial...";
                lblMessage.Visible = true;

                // Step 1: Get all available testimonial templates from Firebase
                var allTemplates = await firebaseHelper.GetAllTemplates();

                // Filter to only active testimonial templates
                var testimonialTemplates = allTemplates
                    .Where(t => !t.IsDeleted && t.IsActive && t.TemplateType == "Testimonial")
                    .ToList();

                if (testimonialTemplates.Count == 0)
                {
                    ShowError("No testimonial templates found. Please contact an administrator.");
                    return;
                }

                // First try to get the default testimonial template
                var selectedTemplate = testimonialTemplates.FirstOrDefault(t => t.IsTestimonialTemplate);

                // If no default template is set, use the first available
                if (selectedTemplate == null)
                {
                    selectedTemplate = testimonialTemplates.First();
                }

                // Log the selected template information
                System.Diagnostics.Debug.WriteLine($"Selected template: {selectedTemplate.TemplateId} - {selectedTemplate.TemplateName}");

                // Step 2: Get the template URL and field coordinates
                string templateUrl = selectedTemplate.TemplateUrl;
                var fieldCoordinates = selectedTemplate.Fields;

                if (string.IsNullOrEmpty(templateUrl))
                {
                    ShowError("Template image URL is missing.");
                    return;
                }

                if (fieldCoordinates == null || fieldCoordinates.Count == 0)
                {
                    ShowError("Template field coordinates are missing.");
                    return;
                }

                // Step 3: Download the template image
                string templateImagePath = await DownloadTemplateImage(templateUrl);
                if (string.IsNullOrEmpty(templateImagePath))
                {
                    ShowError("Failed to download template image.");
                    return;
                }

                // Step 4: Gather user data for the testimonial
                string recipientName = currentUser.Username;
                string currentDate = DateTime.Now.ToString("MMMM d, yyyy");
                int coursesCompleted = currentUser.MemberData?.CourseID_List?.Count ?? 0;

                // Get achievements count
                var allApplications = await firebaseHelper.GetAchievementApplications();
                int achievementsCount = allApplications
                    .Where(a => a.ApplicantRefId == currentUser.UserId && a.Status == "Approved")
                    .Count();

                // Get badges data - we need to download badge images
                var userBadges = currentUser.MemberData?.BadgeID_List ?? new List<string>();
                var badgesList = new List<DashboardBadge>();

                foreach (string badgeEntry in userBadges)
                {
                    string[] parts = badgeEntry.Split(':');
                    if (parts.Length > 1 && parts[1] == "claimed")
                    {
                        try
                        {
                            var badgeDetails = await firebaseHelper.GetBadgeById(parts[0]);
                            if (badgeDetails != null)
                            {
                                badgesList.Add(new DashboardBadge
                                {
                                    BadgeId = badgeDetails.BadgeId,
                                    BadgeName = badgeDetails.BadgeName,
                                    BadgeDesc = badgeDetails.BadgeDesc,
                                    BadgeIconUrl = badgeDetails.BadgeIconUrl,
                                    BadgePoints = badgeDetails.BadgePoints,
                                    Status = "claimed"
                                });
                            }
                        }
                        catch (Exception badgeEx)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error loading badge {parts[0]}: {badgeEx.Message}");
                            // Continue with other badges
                        }
                    }
                }

                // Step 5: Generate the testimonial image
                string generatedCertificatePath = await GenerateTestimonial(
                    templateImagePath,
                    fieldCoordinates,
                    recipientName,
                    currentDate,
                    coursesCompleted,
                    achievementsCount,
                    badgesList
                );

                if (string.IsNullOrEmpty(generatedCertificatePath))
                {
                    ShowError("Failed to generate testimonial image.");
                    return;
                }

                // Ensure CloudinaryHelper is initialized
                if (cloudinaryHelper == null)
                {
                    cloudinaryHelper = new CloudinaryHelper();
                }
                // Step 6: Upload the generated image to Cloudinary
                string certificateUrl = cloudinaryHelper.UploadFileFromPath(
                    generatedCertificatePath,
                    "user_testimonials"
                );

                if (string.IsNullOrEmpty(certificateUrl))
                {
                    ShowError("Failed to upload generated testimonial.");
                    return;
                }
                try
                {
                    // Step 7: Store the testimonial URL in Firebase
                    // Create a proper certificate object to store
                    var certificateData = new Dictionary<string, object>
    {
        { "url", certificateUrl },
        { "templateId", selectedTemplate.TemplateId },
        { "generatedAt", DateTime.UtcNow.ToString("o") }
    };

                    // Generate a unique certificate ID
                    string certificateId = $"cert_{Guid.NewGuid():N}";

                    // Store in Firebase using the structured data object
                    await firebaseHelper.StoreGeneratedCertificate(
                        currentUser.UserId,
                        certificateId,
                        certificateData
                    );

                    System.Diagnostics.Debug.WriteLine($"Certificate stored successfully with ID: {certificateId}");
                }
                catch (Exception ex)
                {
                    // Don't fail the whole process if Firebase storage fails
                    // The user can still download the certificate
                    System.Diagnostics.Debug.WriteLine($"Error storing certificate in Firebase: {ex.Message}");
                }
                // Clean up temporary files
                CleanupTempFiles(templateImagePath, generatedCertificatePath);


                // Use JavaScript to open the download in a new tab
                ScriptManager.RegisterStartupScript(this, GetType(), "openDownloadInNewTab",
                    $"alert('Testimonial generated successfully!'); " +
                    $"window.open('{certificateUrl}', '_blank');",
                    true);

            }
            catch (Exception ex)
            {
                // Log the error
                System.Diagnostics.Debug.WriteLine($"Error in btnExport_Click: {ex}");
                ShowError($"Error generating testimonial: {ex.Message}");
            }
        }

        /// <summary>
        /// Generates a testimonial image using the provided template and user data
        /// </summary>
        private async Task<string> GenerateTestimonial(
            string templateImagePath,
            Dictionary<string, FieldData> fieldCoordinates,
            string recipientName,
            string currentDate,
            int coursesCompleted,
            int achievementsCount,
            List<DashboardBadge> earnedBadges)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Beginning testimonial generation...");

                // Ensure the Temp directory exists
                string tempDirectory = Server.MapPath("~/Temp");
                if (!System.IO.Directory.Exists(tempDirectory))
                {
                    System.IO.Directory.CreateDirectory(tempDirectory);
                }

                // Load the template image
                using (var bitmap = new System.Drawing.Bitmap(templateImagePath))
                {
                    System.Diagnostics.Debug.WriteLine($"Template image loaded: {bitmap.Width}x{bitmap.Height}");

                    using (var graphics = System.Drawing.Graphics.FromImage(bitmap))
                    {
                        // Configure graphics for high quality text rendering
                        graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                        graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                        graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

                        // Define fonts for different field types
                        var titleFont = new System.Drawing.Font("Arial", 30, System.Drawing.FontStyle.Bold);
                        var nameFont = new System.Drawing.Font("Arial", 24, System.Drawing.FontStyle.Bold);
                        var regularFont = new System.Drawing.Font("Arial", 18);
                        var smallFont = new System.Drawing.Font("Arial", 14);
                        var textBrush = System.Drawing.Brushes.Black;

                        // Log the field keys to help with debugging
                        System.Diagnostics.Debug.WriteLine("Field keys in template:");
                        foreach (var key in fieldCoordinates.Keys)
                        {
                            System.Diagnostics.Debug.WriteLine($"- {key}");
                        }

                        // Process each field by exact name match
                        foreach (var field in fieldCoordinates)
                        {
                            string fieldName = field.Key;
                            var bounds = field.Value.Bounds;

                            System.Diagnostics.Debug.WriteLine($"Processing field: {fieldName}, Bounds: {bounds}");

                            // Skip invalid bounds
                            if (bounds.Width <= 0 || bounds.Height <= 0)
                            {
                                System.Diagnostics.Debug.WriteLine($"Skipping field {fieldName} due to invalid bounds");
                                continue;
                            }

                            // Create StringFormat for text alignment
                            var format = new System.Drawing.StringFormat
                            {
                                Alignment = System.Drawing.StringAlignment.Center,
                                LineAlignment = System.Drawing.StringAlignment.Center
                            };

                            // Match based on exact field name (case-insensitive)
                            if (fieldName.Equals("Participant Name", StringComparison.OrdinalIgnoreCase))
                            {
                                var font = FitFontToRectangle(graphics, recipientName, nameFont, bounds.Size);
                                graphics.DrawString(recipientName, font, textBrush, bounds, format);
                                System.Diagnostics.Debug.WriteLine($"Drew recipient name: {recipientName}");
                            }
                            else if (fieldName.Equals("Achievements received", StringComparison.OrdinalIgnoreCase) ||
                                     fieldName.Equals("Achievements", StringComparison.OrdinalIgnoreCase))
                            {
                                string achieveText = achievementsCount.ToString();
                                var font = FitFontToRectangle(graphics, achieveText, regularFont, bounds.Size);
                                graphics.DrawString(achieveText, font, textBrush, bounds, format);
                                System.Diagnostics.Debug.WriteLine($"Drew achievements: {achieveText}");
                            }
                            else if (fieldName.Equals("Courses completed", StringComparison.OrdinalIgnoreCase) ||
                                     fieldName.Equals("Courses", StringComparison.OrdinalIgnoreCase))
                            {
                                string coursesText = coursesCompleted.ToString();
                                var font = FitFontToRectangle(graphics, coursesText, regularFont, bounds.Size);
                                graphics.DrawString(coursesText, font, textBrush, bounds, format);
                                System.Diagnostics.Debug.WriteLine($"Drew courses: {coursesText}");
                            }
                            else if (fieldName.Equals("Badges earned", StringComparison.OrdinalIgnoreCase) ||
                                     fieldName.Equals("Badges", StringComparison.OrdinalIgnoreCase))
                            {
                                await DrawBadges(graphics, bounds, earnedBadges);
                                System.Diagnostics.Debug.WriteLine($"Drew badges area");
                            }
                            else if (fieldName.Equals("Comments", StringComparison.OrdinalIgnoreCase))
                            {
                                string commentsText = "This certificate recognizes the member's active participation and valuable contributions to our community.";
                                var font = FitFontToRectangle(graphics, commentsText, smallFont, bounds.Size);
                                DrawMultilineText(graphics, commentsText, font, textBrush, bounds);
                                System.Diagnostics.Debug.WriteLine($"Drew comments");
                            }
                            else if (fieldName.Equals("Date", StringComparison.OrdinalIgnoreCase) ||
                                     fieldName.Equals("Awarded on", StringComparison.OrdinalIgnoreCase))
                            {
                                var font = FitFontToRectangle(graphics, currentDate, smallFont, bounds.Size);
                                graphics.DrawString(currentDate, font, textBrush, bounds, format);
                                System.Diagnostics.Debug.WriteLine($"Drew date: {currentDate}");
                            }
                            else if (fieldName.Equals("Director Signature", StringComparison.OrdinalIgnoreCase) ||
                                     fieldName.Contains("Signature"))
                            {
                                string signatureText = "Director";
                                var font = FitFontToRectangle(graphics, signatureText, smallFont, bounds.Size);
                                graphics.DrawString(signatureText, font, textBrush, bounds, format);
                                System.Diagnostics.Debug.WriteLine($"Drew signature");
                            }
                            else
                            {
                                // For any unrecognized field, leave it blank or apply a fallback
                                System.Diagnostics.Debug.WriteLine($"Unrecognized field: {fieldName} - leaving blank");
                            }
                        }
                    }

                    // Save the generated testimonial to a temporary file
                    string outputFileName = $"testimonial_{Guid.NewGuid():N}.png";
                    string outputPath = System.IO.Path.Combine(tempDirectory, outputFileName);
                    bitmap.Save(outputPath, System.Drawing.Imaging.ImageFormat.Png);

                    System.Diagnostics.Debug.WriteLine($"Testimonial saved to: {outputPath}");
                    return outputPath;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error generating testimonial: {ex}");
                return null;
            }
        }

        /// <summary>
        /// Draws multi-line text within the specified bounds
        /// </summary>
        private void DrawMultilineText(
            System.Drawing.Graphics graphics,
            string text,
            System.Drawing.Font font,
            System.Drawing.Brush brush,
            System.Drawing.Rectangle bounds)
        {
            try
            {
                if (string.IsNullOrEmpty(text)) return;

                // This StringFormat object is used to wrap text
                var format = new System.Drawing.StringFormat
                {
                    Alignment = System.Drawing.StringAlignment.Center,
                    LineAlignment = System.Drawing.StringAlignment.Center,
                    Trimming = System.Drawing.StringTrimming.Word,
                    FormatFlags = System.Drawing.StringFormatFlags.LineLimit
                };

                // Measure the text to check if it fits
                System.Drawing.SizeF textSize = graphics.MeasureString(text, font, bounds.Width, format);

                // If text is too large, try to reduce font size
                if (textSize.Height > bounds.Height)
                {
                    float fontSize = font.Size;

                    // Keep reducing font size until it fits or reaches minimum size
                    while (fontSize > 8 && textSize.Height > bounds.Height)
                    {
                        fontSize -= 0.5f;
                        using (var smallerFont = new System.Drawing.Font(font.FontFamily, fontSize, font.Style))
                        {
                            textSize = graphics.MeasureString(text, smallerFont, bounds.Width, format);
                            if (textSize.Height <= bounds.Height)
                            {
                                // Found a font size that fits, use it to draw the text
                                graphics.DrawString(text, smallerFont, brush, bounds, format);
                                return;
                            }
                        }
                    }

                    // If we couldn't fit all text, truncate it and add ellipsis
                    if (textSize.Height > bounds.Height)
                    {
                        // Find a suitable truncation point
                        int approxCharactersPerLine = (int)(bounds.Width / (font.Size * 0.5));
                        int totalLines = bounds.Height / (int)(font.Size * 1.2);
                        int charactersToKeep = approxCharactersPerLine * totalLines;

                        // Don't truncate if the text is shorter than our estimate
                        if (text.Length > charactersToKeep)
                        {
                            // Truncate and add ellipsis
                            text = text.Substring(0, Math.Max(0, charactersToKeep - 3)) + "...";
                        }
                    }
                }

                // Draw the text
                graphics.DrawString(text, font, brush, bounds, format);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in DrawMultilineText: {ex.Message}");
            }
        }

        /// <summary>
        /// Draws badge icons in a given area
        /// </summary>
        private async Task DrawBadges(
            System.Drawing.Graphics graphics,
            System.Drawing.Rectangle bounds,
            List<DashboardBadge> earnedBadges)
        {
            try
            {
                if (earnedBadges == null || earnedBadges.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("No badges to draw");
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"Drawing {earnedBadges.Count} badges in area: {bounds}");

                // Calculate badge size and spacing based on the available area
                int maxBadgesPerRow = Math.Min(6, earnedBadges.Count);  // Maximum 6 badges per row

                // Calculate badge size based on available space
                int rows = (int)Math.Ceiling((double)earnedBadges.Count / maxBadgesPerRow);
                int badgeSize = Math.Min(bounds.Width / maxBadgesPerRow, bounds.Height / (rows + 1)) - 10; // Leave space for label

                // Ensure badge size is reasonable
                badgeSize = Math.Max(badgeSize, 40);  // Minimum size of 40px
                badgeSize = Math.Min(badgeSize, 80);  // Maximum size of 80px

                int horizontalSpacing = (bounds.Width - (badgeSize * Math.Min(maxBadgesPerRow, earnedBadges.Count))) /
                                       (Math.Min(maxBadgesPerRow, earnedBadges.Count) + 1);

                int verticalSpacing = 10;

                // Calculate starting position
                int startY = bounds.Y + 5;

                // Ensure the Temp directory exists
                string tempDirectory = Server.MapPath("~/Temp");
                if (!System.IO.Directory.Exists(tempDirectory))
                {
                    System.IO.Directory.CreateDirectory(tempDirectory);
                }

                // Track successfully drawn badges
                int badgesDrawn = 0;

                for (int row = 0; row < rows; row++)
                {
                    // Calculate how many badges to draw in this row
                    int badgesInThisRow = Math.Min(maxBadgesPerRow, earnedBadges.Count - (row * maxBadgesPerRow));

                    // Calculate horizontal starting position for this row (to center the badges)
                    int rowWidth = badgesInThisRow * badgeSize + (badgesInThisRow - 1) * horizontalSpacing;
                    int startX = bounds.X + (bounds.Width - rowWidth) / 2;

                    // Draw badges in this row
                    for (int col = 0; col < badgesInThisRow; col++)
                    {
                        int badgeIndex = row * maxBadgesPerRow + col;
                        if (badgeIndex >= earnedBadges.Count) break;

                        var badge = earnedBadges[badgeIndex];
                        if (string.IsNullOrEmpty(badge.BadgeIconUrl)) continue;

                        try
                        {
                            // Calculate position for this badge
                            int x = startX + col * (badgeSize + horizontalSpacing);
                            int y = startY + row * (badgeSize + verticalSpacing);

                            // Download and draw the badge image
                            string badgeFileName = $"badge_{Guid.NewGuid():N}.png";
                            string badgeFilePath = System.IO.Path.Combine(tempDirectory, badgeFileName);

                            using (var client = new System.Net.WebClient())
                            {
                                System.Diagnostics.Debug.WriteLine($"Downloading badge from: {badge.BadgeIconUrl}");

                                try
                                {
                                    // Try to download the badge image
                                    await client.DownloadFileTaskAsync(new Uri(badge.BadgeIconUrl), badgeFilePath);

                                    if (System.IO.File.Exists(badgeFilePath))
                                    {
                                        // Draw badge with rounded corners
                                        using (var badgeImage = System.Drawing.Image.FromFile(badgeFilePath))
                                        {
                                            // Create a destination rectangle
                                            var destRect = new System.Drawing.Rectangle(x, y, badgeSize, badgeSize);

                                            // Draw the badge
                                            graphics.DrawImage(badgeImage, destRect);

                                            // Increment counter
                                            badgesDrawn++;

                                            System.Diagnostics.Debug.WriteLine($"Drew badge at position: {destRect}");
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    System.Diagnostics.Debug.WriteLine($"Failed to download badge: {ex.Message}");
                                }
                                finally
                                {
                                    // Clean up regardless of success
                                    try
                                    {
                                        if (System.IO.File.Exists(badgeFilePath))
                                        {
                                            System.IO.File.Delete(badgeFilePath);
                                        }
                                    }
                                    catch
                                    {
                                        // Ignore cleanup errors
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error drawing badge {badgeIndex}: {ex.Message}");
                            // Continue with next badge
                        }
                    }
                }

                //// If we managed to draw badges, add a label
                //if (badgesDrawn > 0)
                //{
                //    // Draw "Earned Badges" text under the badges
                //    var font = new System.Drawing.Font("Arial", 12, System.Drawing.FontStyle.Bold);
                //    var brush = System.Drawing.Brushes.Black;
                //    var format = new System.Drawing.StringFormat
                //    {
                //        Alignment = System.Drawing.StringAlignment.Center,
                //        LineAlignment = System.Drawing.StringAlignment.Center
                //    };

                //    // Calculate label position (below all badges)
                //    int labelY = startY + (rows * (badgeSize + verticalSpacing));
                //    var labelBounds = new System.Drawing.Rectangle(
                //        bounds.X, labelY, bounds.Width, 24);  // Fixed height for label

                //    // Only draw if there's space
                //    if (labelY + 24 <= bounds.Y + bounds.Height)
                //    {
                //        graphics.DrawString("Earned Badges", font, brush, labelBounds, format);
                //        System.Diagnostics.Debug.WriteLine($"Drew 'Earned Badges' label at: {labelBounds}");
                //    }
                //}
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in DrawBadges: {ex.Message}");
            }
        }
        /// <summary>
        /// Downloads a template image from the provided URL and returns the local path
        /// </summary>
        private async Task<string> DownloadTemplateImage(string templateUrl)
        {
            try
            {
                // Create a unique filename
                string fileName = $"template_{Guid.NewGuid():N}.png";
                string tempDirectory = Server.MapPath("~/Temp");

                // Ensure the Temp directory exists
                if (!System.IO.Directory.Exists(tempDirectory))
                {
                    System.IO.Directory.CreateDirectory(tempDirectory);
                }

                string filePath = System.IO.Path.Combine(tempDirectory, fileName);

                // Download the image
                using (var client = new System.Net.WebClient())
                {
                    System.Diagnostics.Debug.WriteLine($"Downloading template from: {templateUrl}");
                    await client.DownloadFileTaskAsync(new Uri(templateUrl), filePath);
                    System.Diagnostics.Debug.WriteLine($"Template downloaded to: {filePath}");

                    // Verify the file exists and has content
                    if (System.IO.File.Exists(filePath) && new System.IO.FileInfo(filePath).Length > 0)
                    {
                        return filePath;
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("Downloaded file is empty or does not exist");
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error downloading template image: {ex}");
                return null;
            }
        }


        /// <summary>
        /// Cleans up temporary files created during testimonial generation
        /// </summary>
        private void CleanupTempFiles(string templateImagePath, string generatedCertificatePath)
        {
            try
            {
                // Delete the template image
                if (!string.IsNullOrEmpty(templateImagePath) && System.IO.File.Exists(templateImagePath))
                {
                    System.IO.File.Delete(templateImagePath);
                    System.Diagnostics.Debug.WriteLine($"Deleted template image: {templateImagePath}");
                }

                // Delete the generated certificate
                if (!string.IsNullOrEmpty(generatedCertificatePath) && System.IO.File.Exists(generatedCertificatePath))
                {
                    System.IO.File.Delete(generatedCertificatePath);
                    System.Diagnostics.Debug.WriteLine($"Deleted generated certificate: {generatedCertificatePath}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error cleaning up temp files: {ex.Message}");
                // Continue even if cleanup fails
            }
        }

        /// <summary>
        /// Calculates a font size that fits the text within the given rectangle
        /// </summary>
        private System.Drawing.Font FitFontToRectangle(
            System.Drawing.Graphics graphics,
            string text,
            System.Drawing.Font font,
            System.Drawing.Size rectangleSize)
        {
            // Start with the current font size
            float fontSize = font.Size;
            System.Drawing.Font newFont = font;
            System.Drawing.SizeF textSize;

            // Reduce font size until it fits
            while (fontSize > 8) // Don't go smaller than 8pt
            {
                // Create a new font with the current size
                newFont = new System.Drawing.Font(font.FontFamily, fontSize, font.Style);

                // Measure the text with this font
                textSize = graphics.MeasureString(text, newFont);

                // If text fits, we're done
                if (textSize.Width <= rectangleSize.Width && textSize.Height <= rectangleSize.Height)
                {
                    break;
                }

                // Otherwise, reduce font size and try again
                fontSize -= 1;
            }

            return newFont;
        }

        /// <summary>
        /// Shows an error message to the user
        /// </summary>
        private void ShowError(string message)
        {
            lblMessage.Text = message;
            lblMessage.ForeColor = System.Drawing.Color.Red;
            lblMessage.Visible = true;

            System.Diagnostics.Debug.WriteLine($"ERROR: {message}");
        }

    }
    // Helper class for dashboard badges
    public class DashboardBadge
    {
        public string BadgeId { get; set; }
        public string BadgeName { get; set; }
        public string BadgeDesc { get; set; }
        public string BadgeIconUrl { get; set; }
        public int BadgePoints { get; set; }
        public string Status { get; set; } // locked, new, or claimed
    }
}