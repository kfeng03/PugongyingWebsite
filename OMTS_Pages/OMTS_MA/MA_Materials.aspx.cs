// Changes to MA_Materials.aspx.cs to implement badge icon uploads

// First, let's modify the page to add file upload capability for badge icons
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.Script.Serialization;

namespace fyp
{
    public partial class MA_Material : System.Web.UI.Page, ILocalizable
    {
        protected FirebaseHelper firebaseHelper;
        protected CloudinaryHelper cloudinaryHelper;

        protected void Page_PreInit(object sender, EventArgs e)
        {
            if (Session["CurrentLanguage"] != null)
            {
                string selectedLanguage = Session["CurrentLanguage"].ToString();
                Thread.CurrentThread.CurrentCulture = new CultureInfo(selectedLanguage);
                Thread.CurrentThread.CurrentUICulture = new CultureInfo(selectedLanguage);
            }
        }

        public void LocalizeContent()
        {
            LoadResources();
        }

        private bool IsUserLoggedIn()
        {
            return Session["UserID"] != null || Session["FirebaseUID"] != null;
        }

        protected async void Page_Load(object sender, EventArgs e)
        {
            // Initialize Firebase helper
            firebaseHelper = new FirebaseHelper();

            // Initialize Cloudinary helper
            cloudinaryHelper = new CloudinaryHelper();

            // Check if the user is logged in
            //if (!IsUserLoggedIn())
            //{
            //    Response.Redirect("AM_LoginHP.aspx", false);
            //    Context.ApplicationInstance.CompleteRequest();
            //    return;
            //}

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

                // Load resources and bind UI
                LoadResources();
                Page.DataBind();

                // Hide all popups by default
                HideAllPopups();

                // Load active badges
                await LoadActiveBadges();

                // Load deleted badges (they'll be hidden but ready when needed)
                await LoadDeletedBadges();


            }
        }


        private void HideAllPopups()
        {
            // Hide all popups
            overlay.Style["display"] = "none";
            badgePopup.Style["display"] = "none";
            deletePopup.Style["display"] = "none";
            recycleBinPopup.Style["display"] = "none";
            permanentDeletePopup.Style["display"] = "none";

        }

        private void ShowPopup(HtmlGenericControl popup)
        {
            // Hide all popups first
            HideAllPopups();

            // Show the overlay and the specified popup
            overlay.Style["display"] = "block";
            popup.Style["display"] = "block";
        }

        private async Task LoadActiveBadges()
        {
            try
            {
                // Get active badges from Firebase
                var activeBadges = await firebaseHelper.GetActiveBadges();

                // Bind badges to repeater
                rptBadges.DataSource = activeBadges;
                rptBadges.DataBind();
            }
            catch (Exception ex)
            {
                lblMessage.Text = "Error loading badges: " + ex.Message;
                lblMessage.ForeColor = Color.Red;
            }
        }

        private async Task LoadDeletedBadges()
        {
            try
            {
                // Get deleted badges from Firebase
                var deletedBadges = await firebaseHelper.GetDeletedBadges();

                // Bind deleted badges to repeater
                rptDeletedBadges.DataSource = deletedBadges;
                rptDeletedBadges.DataBind();
            }
            catch (Exception ex)
            {
                lblMessage.Text = "Error loading deleted badges: " + ex.Message;
                lblMessage.ForeColor = Color.Red;
            }
        }

        #region Badge Management

        protected void btnCreateBadge_Click(object sender, EventArgs e)
        {
            // Clear form fields for a new badge
            txtBadgeName.Text = string.Empty;
            txtBadgeDescription.Text = string.Empty;
            txtPointsAwarded.Text = "0";

            // Reset the badge icon preview to default
            badgePreview.ImageUrl = "https://res.cloudinary.com/dkzzqgxce/image/upload/v1741607554/pngtree-luxury-red-golden-circle-vector-ribbon-badge-png-image_7093843_xn1rl8.png";
            hdnBadgeIconUrl.Value = string.Empty;

            // Set up form for badge creation
            litBadgePopupTitle.Text = GetGlobalResourceObject("Resources", "Heading_CreateBadge")?.ToString() ?? "Create New Badge";
            hdnBadgeAction.Value = "create";
            hdnBadgeId.Value = string.Empty;

            // Show the badge popup
            ShowPopup(badgePopup);
        }

        protected void BadgeCommand(object sender, CommandEventArgs e)
        {
            string commandName = e.CommandName;
            string badgeId = e.CommandArgument.ToString();

            if (commandName == "Edit")
            {
                // Handle edit badge command
                EditBadge(badgeId);
            }
            else if (commandName == "Delete")
            {
                // Handle delete badge command
                DeleteBadge(badgeId);
            }
        }

        protected void RecycleCommand(object sender, CommandEventArgs e)
        {
            string commandName = e.CommandName;
            string badgeId = e.CommandArgument.ToString();

            if (commandName == "Restore")
            {
                // Handle restore badge command
                RestoreBadge(badgeId);
            }
            else if (commandName == "PermanentDelete")
            {
                // Handle permanent delete badge command
                PermanentDeleteBadge(badgeId);
            }
        }

        private async void EditBadge(string badgeId)
        {
            try
            {
                // Get badge details from Firebase
                var badge = await firebaseHelper.GetBadgeById(badgeId);

                if (badge != null)
                {
                    // Populate form fields with badge details
                    txtBadgeName.Text = badge.BadgeName;
                    txtBadgeDescription.Text = badge.BadgeDesc;
                    txtPointsAwarded.Text = badge.BadgePoints.ToString();

                    // Set badge icon preview
                    if (!string.IsNullOrEmpty(badge.BadgeIconUrl))
                    {
                        badgePreview.ImageUrl = badge.BadgeIconUrl;
                        hdnBadgeIconUrl.Value = badge.BadgeIconUrl; // Store current icon URL
                    }
                    else
                    {
                        badgePreview.ImageUrl = "https://res.cloudinary.com/dkzzqgxce/image/upload/v1741607554/pngtree-luxury-red-golden-circle-vector-ribbon-badge-png-image_7093843_xn1rl8.png";
                        hdnBadgeIconUrl.Value = string.Empty;
                    }

                    // Set up form for badge editing
                    litBadgePopupTitle.Text = GetGlobalResourceObject("Resources", "Heading_EditBadge")?.ToString() ?? "Edit Badge";
                    hdnBadgeAction.Value = "edit";
                    hdnBadgeId.Value = badgeId;

                    // Show the badge popup
                    ShowPopup(badgePopup);
                }
                else
                {
                    lblMessage.Text = "Badge not found.";
                    lblMessage.ForeColor = Color.Red;
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = "Error loading badge details: " + ex.Message;
                lblMessage.ForeColor = Color.Red;
            }
        }
        private async void DeleteBadge(string badgeId)
        {
            try
            {
                // Get badge details from Firebase for display in confirmation
                var badge = await firebaseHelper.GetBadgeById(badgeId);

                if (badge != null)
                {
                    // Set badge name in the confirmation message
                    litDeleteBadgeName.Text = badge.BadgeName;

                    // Store badge ID for deletion
                    hdnDeleteBadgeId.Value = badgeId;

                    // Show the delete confirmation popup
                    ShowPopup(deletePopup);
                }
                else
                {
                    lblMessage.Text = "Badge not found.";
                    lblMessage.ForeColor = Color.Red;
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = "Error loading badge details: " + ex.Message;
                lblMessage.ForeColor = Color.Red;
            }
        }

        private async void PermanentDeleteBadge(string badgeId)
        {
            try
            {
                // Get badge details from Firebase for display in confirmation
                var badge = await firebaseHelper.GetBadgeById(badgeId);

                if (badge != null)
                {
                    // Set badge name in the confirmation message
                    litPermanentDeleteBadgeName.Text = badge.BadgeName;

                    // Store badge ID for deletion
                    hdnPermanentDeleteBadgeId.Value = badgeId;

                    // Show the permanent delete confirmation popup
                    ShowPopup(permanentDeletePopup);
                }
                else
                {
                    lblMessage.Text = "Badge not found.";
                    lblMessage.ForeColor = Color.Red;
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = "Error loading badge details: " + ex.Message;
                lblMessage.ForeColor = Color.Red;
            }
        }

        private async void RestoreBadge(string badgeId)
        {
            try
            {
                // Restore the badge in Firebase
                await firebaseHelper.RestoreBadge(badgeId);

                // Reload both active and deleted badges
                await LoadActiveBadges();
                await LoadDeletedBadges();

                // Show success message
                lblMessage.Text = "Badge restored successfully.";
                lblMessage.ForeColor = Color.Green;

                // Keep the recycle bin popup open
                ShowPopup(recycleBinPopup);
            }
            catch (Exception ex)
            {
                lblMessage.Text = "Error restoring badge: " + ex.Message;
                lblMessage.ForeColor = Color.Red;
            }
        }

        protected async void btnSaveBadge_Click(object sender, EventArgs e)
        {
            try
            {
                // Get user inputs
                string badgeName = txtBadgeName.Text.Trim();
                string badgeDescription = txtBadgeDescription.Text.Trim();
                int badgePoints = 0;

                // Parse the points - should be validated by the RangeValidator
                if (!int.TryParse(txtPointsAwarded.Text, out badgePoints))
                {
                    badgePoints = 0;
                }

                // Get action and badge ID from hidden fields
                string action = hdnBadgeAction.Value;
                string badgeId = hdnBadgeId.Value;

                // Handle badge icon upload
                string badgeIconUrl = hdnBadgeIconUrl.Value; // Keep existing URL by default

                if (fuBadge.HasFile)
                {
                    try
                    {
                        // Upload to Cloudinary using existing CloudinaryHelper
                        badgeIconUrl = cloudinaryHelper.UploadFile(fuBadge.PostedFile, "badge_icons");

                        // Log success for debugging
                        System.Diagnostics.Debug.WriteLine($"Badge icon uploaded successfully: {badgeIconUrl}");
                    }
                    catch (Exception uploadEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"Badge icon upload error: {uploadEx.Message}");
                        lblBadgeMessage.Text = "Error uploading badge icon: " + uploadEx.Message;
                        lblBadgeMessage.ForeColor = Color.Red;
                        return;
                    }
                }
                else
                {
                    badgeIconUrl = "https://res.cloudinary.com/dkzzqgxce/image/upload/v1741607554/pngtree-luxury-red-golden-circle-vector-ribbon-badge-png-image_7093843_xn1rl8.png";
                }

                // Check if resetBadgeIcon is set to true
                if (hdnResetBadgeIcon.Value == "true")
                {
                    // Set default badge icon URL
                    badgeIconUrl = "https://res.cloudinary.com/dkzzqgxce/image/upload/v1741607554/pngtree-luxury-red-golden-circle-vector-ribbon-badge-png-image_7093843_xn1rl8.png";
                    hdnResetBadgeIcon.Value = "false"; // Reset the flag
                }

                if (action == "create")
                {
                    // Generate next badge ID
                    string newBadgeId = await firebaseHelper.GenerateNextBadgeID();

                    // Create new badge
                    var badge = new Badge
                    {
                        BadgeId = newBadgeId,
                        BadgeName = badgeName,
                        BadgeDesc = badgeDescription,
                        BadgePoints = badgePoints,
                        BadgeIconUrl = badgeIconUrl,
                        IsDeleted = false
                    };

                    // Add badge to Firebase
                    await firebaseHelper.AddBadge(badge);

                    // Show success message
                    lblMessage.Text = "Badge created successfully.";
                    lblMessage.ForeColor = Color.Green;
                }
                else if (action == "edit")
                {
                    // Get existing badge
                    var existingBadge = await firebaseHelper.GetBadgeById(badgeId);

                    if (existingBadge == null)
                    {
                        lblBadgeMessage.Text = "Badge not found.";
                        return;
                    }

                    // Update badge properties
                    existingBadge.BadgeName = badgeName;
                    existingBadge.BadgeDesc = badgeDescription;
                    existingBadge.BadgePoints = badgePoints;

                    // Only update icon URL if it changed
                    if (badgeIconUrl != existingBadge.BadgeIconUrl)
                    {
                        existingBadge.BadgeIconUrl = badgeIconUrl;
                    }

                    // Update badge in Firebase
                    await firebaseHelper.UpdateBadge(existingBadge);

                    // Show success message
                    lblMessage.Text = "Badge updated successfully.";
                    lblMessage.ForeColor = Color.Green;
                }

                // Hide the badge popup
                HideAllPopups();

                // Reload active badges
                await LoadActiveBadges();
            }
            catch (Exception ex)
            {
                lblBadgeMessage.Text = "Error: " + ex.Message;
                lblBadgeMessage.ForeColor = Color.Red;
            }
        }
        protected void btnRemoveBadgeIcon_Click(object sender, EventArgs e)
        {
            // Set default badge icon in the UI
            badgePreview.ImageUrl = "https://res.cloudinary.com/dkzzqgxce/image/upload/v1741607554/pngtree-luxury-red-golden-circle-vector-ribbon-badge-png-image_7093843_xn1rl8.png";

            // Set flag to reset badge icon on save
            hdnResetBadgeIcon.Value = "true";

            // Show message
            lblBadgeMessage.Text = "Badge icon will be reset when you save changes.";
            lblBadgeMessage.ForeColor = Color.Blue;
        }
        protected void btnCancelBadge_Click(object sender, EventArgs e)
        {
            // Hide the badge popup
            HideAllPopups();
        }

        protected async void btnConfirmDelete_Click(object sender, EventArgs e)
        {
            try
            {
                // Get badge ID from hidden field
                string badgeId = hdnDeleteBadgeId.Value;

                if (!string.IsNullOrEmpty(badgeId))
                {
                    // Soft delete the badge
                    await firebaseHelper.SoftDeleteBadge(badgeId);

                    // Reload both active and deleted badges
                    await LoadActiveBadges();
                    await LoadDeletedBadges();

                    // Display success message
                    lblMessage.Text = "Badge moved to recycle bin.";
                    lblMessage.ForeColor = Color.Green;

                    // Hide the delete confirmation popup
                    HideAllPopups();
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = "Error deleting badge: " + ex.Message;
                lblMessage.ForeColor = Color.Red;
            }
        }

        protected void btnCancelDelete_Click(object sender, EventArgs e)
        {
            // Hide the delete confirmation popup
            HideAllPopups();
        }

        protected async void btnConfirmPermanentDelete_Click(object sender, EventArgs e)
        {
            try
            {
                // Get badge ID from hidden field
                string badgeId = hdnPermanentDeleteBadgeId.Value;

                if (!string.IsNullOrEmpty(badgeId))
                {
                    // Permanently delete the badge
                    await firebaseHelper.DeleteBadge(badgeId);

                    // Reload deleted badges
                    await LoadDeletedBadges();

                    // Display success message
                    lblMessage.Text = "Badge permanently deleted.";
                    lblMessage.ForeColor = Color.Green;

                    // Hide the permanent delete confirmation popup and show recycle bin
                    ShowPopup(recycleBinPopup);
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = "Error permanently deleting badge: " + ex.Message;
                lblMessage.ForeColor = Color.Red;
            }
        }

        protected void btnCancelPermanentDelete_Click(object sender, EventArgs e)
        {
            // Hide the permanent delete confirmation popup and show recycle bin
            ShowPopup(recycleBinPopup);
        }

        protected void btnShowRecycleBin_Click(object sender, EventArgs e)
        {
            // Show the recycle bin popup
            ShowPopup(recycleBinPopup);
        }

        protected void btnCloseRecycleBin_Click(object sender, EventArgs e)
        {
            // Hide the recycle bin popup
            HideAllPopups();
        }

        protected void btnIncrement_Click(object sender, EventArgs e)
        {
            // Increment points by 5
            int currentPoints;
            if (int.TryParse(txtPointsAwarded.Text, out currentPoints))
            {
                txtPointsAwarded.Text = (currentPoints + 5).ToString();
            }
            else
            {
                txtPointsAwarded.Text = "5";
            }
        }

        protected void btnDecrement_Click(object sender, EventArgs e)
        {
            // Decrement points by 5, but not below 0
            int currentPoints;
            if (int.TryParse(txtPointsAwarded.Text, out currentPoints))
            {
                txtPointsAwarded.Text = Math.Max(0, currentPoints - 5).ToString();
            }
            else
            {
                txtPointsAwarded.Text = "0";
            }
        }

        #endregion

        #region Media Management
        protected void rblMediaType_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Hide all panels first
            pnlVideoLink.Visible = false;
            pnlPdfLink.Visible = false;
            pnlImageUpload.Visible = false;

            // Show the appropriate panel based on selection
            switch (rblMediaType.SelectedValue)
            {
                case "video":
                    pnlVideoLink.Visible = true;
                    break;
                case "pdf":
                    pnlPdfLink.Visible = true;
                    break;
                case "image":
                    pnlImageUpload.Visible = true;
                    break;
            }
        }

        protected async void btnSubmitMedia_Click(object sender, EventArgs e)
        {
            try
            {
                bool success = false;
                string mediaType = rblMediaType.SelectedValue;
                string mediaUrl = string.Empty;
                string mediaTitle = txtMediaTitle.Text.Trim();

                if (string.IsNullOrEmpty(mediaTitle))
                {
                    ShowErrorMessage("Please enter a title for the introduction media.");
                    return;
                }

                // Process based on media type
                switch (mediaType)
                {
                    case "video":
                        // Validate video link
                        mediaUrl = txtVideoLink.Text.Trim();
                        if (string.IsNullOrEmpty(mediaUrl))
                        {
                            ShowErrorMessage("Please enter a video link.");
                            return;
                        }

                        // Basic validation for video links
                        if (!IsValidUrl(mediaUrl))
                        {
                            ShowErrorMessage("Please enter a valid URL.");
                            return;
                        }
                        success = true;
                        break;

                    case "pdf":
                        // Validate PDF link
                        mediaUrl = txtPdfLink.Text.Trim();
                        if (string.IsNullOrEmpty(mediaUrl))
                        {
                            ShowErrorMessage("Please enter a PDF link.");
                            return;
                        }

                        // Basic validation for PDF links
                        if (!IsValidUrl(mediaUrl))
                        {
                            ShowErrorMessage("Please enter a valid URL.");
                            return;
                        }
                        success = true;
                        break;

                    case "image":
                        // Upload image file
                        if (!fuInfoImage.HasFile)
                        {
                            ShowErrorMessage("Please select an image file to upload.");
                            return;
                        }

                        try
                        {
                            // Validate file type
                            string fileExtension = System.IO.Path.GetExtension(fuInfoImage.FileName).ToLower();
                            string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };

                            if (!allowedExtensions.Contains(fileExtension))
                            {
                                ShowErrorMessage("Only image files (JPG, JPEG, PNG, GIF) are allowed.");
                                return;
                            }

                            // Upload to Cloudinary
                            CloudinaryHelper cloudinaryHelper = new CloudinaryHelper();
                            mediaUrl = cloudinaryHelper.UploadFile(fuInfoImage.PostedFile, "info_graphics");
                            success = true;
                        }
                        catch (Exception ex)
                        {
                            ShowErrorMessage($"Failed to upload image: {ex.Message}");
                            return;
                        }
                        break;
                }

                if (success)
                {
                    // Save to Firebase
                    await firebaseHelper.UpdateIntroductionMedia("intro_media", new IntroductionMedia
                    {
                        Title = mediaTitle,
                        MediaType = mediaType,
                        MediaUrl = mediaUrl,
                        UpdatedAt = DateTime.UtcNow.ToString("o")
                    });

                    ShowSuccessMessage("Introduction media has been successfully updated.");

                    // Clear form
                    txtMediaTitle.Text = string.Empty;
                    txtVideoLink.Text = string.Empty;
                    txtPdfLink.Text = string.Empty;
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error updating introduction media: {ex.Message}");
            }
        }

        private bool IsValidUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out Uri uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }
        #endregion


        private void LoadResources()
        {
            // Set page labels and controls from resource files
            litHeading1.Text = GetGlobalResourceObject("Resources", "Heading_IntroMedia")?.ToString();
            litHeading2.Text = GetGlobalResourceObject("Resources", "Heading_ManageBadge")?.ToString();
            litHeading3.Text = GetGlobalResourceObject("Resources", "Heading_TestimonialTemp")?.ToString();
            litTestimonialTempSubtext.Text = GetGlobalResourceObject("Resources", "Note_TestimonialTemp")?.ToString();
            litBadgePopupTitle.Text = GetGlobalResourceObject("Resources", "Heading_CreateBadge")?.ToString();
            litIntroMedia.Text = GetGlobalResourceObject("Resources", "Note_IntroMedia")?.ToString();


            // Button text
            btnInformation.Text = GetGlobalResourceObject("Resources", "SideBtnInfo")?.ToString();
            btnDashboard.Text = GetGlobalResourceObject("Resources", "SideBtnDb")?.ToString();
            btnApplicationForm.Text = GetGlobalResourceObject("Resources", "SideBtnMAApp")?.ToString();
            btnReviewApp.Text = GetGlobalResourceObject("Resources", "SideBtnRA")?.ToString();
            btnAwardBadge.Text = GetGlobalResourceObject("Resources", "SideBtnAB")?.ToString();
            btnMaterials.Text = GetGlobalResourceObject("Resources", "SideBtnMAMat")?.ToString();
            lnkAnalyzer.Text = GetGlobalResourceObject("Resources", "Link_TestimonialTemp")?.ToString();
            //btnUploadAnalyze.Text = GetGlobalResourceObject("Resources", "Button_UploadAnalyze")?.ToString();
            btnCreateBadge.Text = GetGlobalResourceObject("Resources", "Button_CreateBadge")?.ToString();
            btnSaveBadge.Text = GetGlobalResourceObject("Resources", "Button_Save")?.ToString() ?? "Save";
            btnConfirmDelete.Text = GetGlobalResourceObject("Resources", "Button_Delete")?.ToString() ?? "Delete";
            btnConfirmPermanentDelete.Text = GetGlobalResourceObject("Resources", "Button_DeletePermanently")?.ToString() ?? "Delete Permanently";
            //btnSubmit.Text = GetGlobalResourceObject("Resources", "Button_Submit")?.ToString();
            btnRemoveBadgeIcon.Text = GetGlobalResourceObject("Resources", "Button_RemoveImage")?.ToString() ?? "Remove Image";
        }

        // Helper methods
        private void ShowSuccessMessage(string message)
        {
            string script = $"alert('{message}');";
            ClientScript.RegisterStartupScript(this.GetType(), "SuccessPopup", script, true);
        }

        private void ShowErrorMessage(string message)
        {
            string script = $"alert('{message}');";
            ClientScript.RegisterStartupScript(this.GetType(), "ErrorPopup", script, true);
        }
    }
}