using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace fyp
{
    public partial class UpdatePersonalInfo : System.Web.UI.Page, ILocalizable
    {

        // The LocalizeContent method is defined as part of the ILocalizable interface.
        public void LocalizeContent()
        {
            LoadResources(); // This will reload the resources for the page
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

            // Set the page culture on load.
            if (!IsPostBack)
            {
                PopulateInterestTags();
                LoadResources();
                Page.DataBind();

               
                // Fetch and display user details
                FetchAndDisplayUserDetails();
            }
            // Set initial visibility
            rptInterests.Visible = true;
            pnlTagContainer.Visible = false;
            btnToggleEdit.Visible = true;
            // New logic to hide Remove Photo button if current image is default
            string defaultProfilePicUrl = "https://res.cloudinary.com/dkzzqgxce/image/upload/v1741189568/profile_placeholder_wbnidc.png";
            btnRemoveProfilePic.Visible = profilePreview.ImageUrl != defaultProfilePicUrl;
        }

        private void PopulateInterestTags()
        {
            // Clear the existing container
            pnlTagContainer.Controls.Clear();

            // Create a container div
            Panel tagContainer = new Panel();
            tagContainer.CssClass = "tag-container";
            tagContainer.ID = "tagContainer";

            // Add each category as a tag
            foreach (string category in CategoryUtility.GetAvailableCategories())
            {
                Panel tag = new Panel();
                tag.CssClass = "tag";
                tag.Attributes["data-tag"] = category;
                tag.Controls.Add(new LiteralControl(category));

                tagContainer.Controls.Add(tag);
            }

            pnlTagContainer.Controls.Add(tagContainer);
        }

        protected void btnToggleEdit_Click(object sender, EventArgs e)
        {
            // Toggle visibility
            rptInterests.Visible = !rptInterests.Visible;
            pnlTagContainer.Visible = !pnlTagContainer.Visible;
            btnToggleEdit.Visible = !btnToggleEdit.Visible;
        }
        private bool IsUserLoggedIn() 
        {
            // Check if the user is logged in (ADD IN ALL PAGES THAT NEED ACCESS)
            // Check if the session contains the UserID or FirebaseUID
            return Session["UserID"] != null || Session["FirebaseUID"] != null;
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

        private async void FetchAndDisplayUserDetails()
        {
            // Get Designated User ID from session
            string userId = Session["UserID"]?.ToString();

            if (!string.IsNullOrEmpty(userId))
            {
                // Fetch user details from Firebase using Designated User ID
                FirebaseHelper firebaseHelper = new FirebaseHelper();
                User user = await firebaseHelper.GetUserById(userId);

                if (user != null)
                {
                    // Populate fields with user data
                    txtUsername.Text = user.Username;
                    litRole.Text = user.Role;
                    //progress bar

                    int currentPoints = user.MemberData.Points;
                    int requiredPoints = await GetMaxPointsPerLevel();
                    int currentLvl = user.MemberData.Level;
                    int progressPercentage = (int)(((double)currentPoints / requiredPoints) * 100);

                    // Set progress width dynamically
                    progressBar.Style["width"] = progressPercentage + "%";

                    // Update text label
                    lblPoints.Text = $"{currentPoints}/{requiredPoints} Points";
                    lblCurrLvl.Text = currentLvl.ToString();
                    lblNextLvl.Text = (currentLvl+1).ToString();

                    // Validate and set profile picture
                    if (!string.IsNullOrEmpty(user.ProfilePictureUrl))
                    {
                        try
                        {

                            System.Diagnostics.Debug.WriteLine($"Picture URL: "+user.ProfilePictureUrl);

                            if (user != null && !string.IsNullOrEmpty(user.ProfilePictureUrl))
                            {
                                // Set the image URL directly
                                profilePreview.ImageUrl = user.ProfilePictureUrl;
                            }
                            else
                            {
                                // Fallback to default image
                                profilePreview.ImageUrl = "https://res.cloudinary.com/dkzzqgxce/image/upload/v1741189568/profile_placeholder_wbnidc.png";
                            }
                        }
                        catch (Exception urlEx)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error processing profile picture URL: {urlEx.Message}");
                            profilePreview.ImageUrl = "https://res.cloudinary.com/dkzzqgxce/image/upload/v1741189568/profile_placeholder_wbnidc.png";
                        }

                        // Set Remove Photo button visibility
                        string defaultProfilePicUrl = "https://res.cloudinary.com/dkzzqgxce/image/upload/v1741189568/profile_placeholder_wbnidc.png";
                        btnRemoveProfilePic.Visible = profilePreview.ImageUrl != defaultProfilePicUrl;
                    }
                    else
                    {
                        // Use default image if no URL is provided
                        profilePreview.ImageUrl = "https://res.cloudinary.com/dkzzqgxce/image/upload/v1741189568/profile_placeholder_wbnidc.png";
                    }

                    // Load interests (if available)
                    if (user.MemberData?.Interest_List != null && user.MemberData.Interest_List.Any())
                    {
                        rptInterests.DataSource = user.MemberData.Interest_List.Select(i => new { InterestName = i }).ToList();
                        rptInterests.DataBind();
                    }
                }
                else
                {
                    // Handle case where user details are not found
                    litRole.Text = "User details not found.";
                }
            }
            else
            {
                // Handle case where Designated User ID is not in session
                litRole.Text = "User not logged in.";
            }
        }

        protected void profilePreview_Error(object sender, EventArgs e)
        {
            // Log the error
            System.Diagnostics.Debug.WriteLine("Error loading profile picture");

            // Ensure fallback to default image
            profilePreview.ImageUrl = "https://res.cloudinary.com/dkzzqgxce/image/upload/v1741189568/profile_placeholder_wbnidc.png";
        }


        private void LoadResources()
        {


            // Set page labels and controls from resource files
            litHeading.Text = GetGlobalResourceObject("Resources", "Heading_PersonalInfo")?.ToString();
            lblUsername.Text = GetGlobalResourceObject("Resources", "Label_Username")?.ToString();
            lblProfilePic.Text = GetGlobalResourceObject("Resources", "Label_ProfilePicture")?.ToString();
            lblRole.Text = GetGlobalResourceObject("Resources", "Label_AARole")?.ToString();
            lblCurrentLevel.Text = GetGlobalResourceObject("Resources", "Label_CurrentLevel")?.ToString();
            lnkAchievementDashboard.Text = GetGlobalResourceObject("Resources", "Hyperlink_AchievementDashboardText")?.ToString();
            lblAreaOfInterest.Text = GetGlobalResourceObject("Resources", "Label_AreaOfInterest")?.ToString();
            litInterestNote.Text = GetGlobalResourceObject("Resources", "InterestNote")?.ToString();


            // Button text
            btnSaveChanges.Text = GetGlobalResourceObject("Resources", "Button_SaveChanges").ToString();
            btnAccSecurity.Text = GetGlobalResourceObject("Resources", "SideBtnAS")?.ToString();
            btnPersonalInfo.Text = GetGlobalResourceObject("Resources", "SideBtnPI")?.ToString();
            btnEditUsername.Text = GetGlobalResourceObject("Resources", "Button_Edit")?.ToString();
            //btnEditInt.Text = GetGlobalResourceObject("Resources", "Button_Edit")?.ToString();
        }


        protected async void btnSaveChanges_Click(object sender, EventArgs e)
        {
            // Get the updated username and interests from hidden fields
            string newUsername = hdnUpdatedUsername.Value;
            string newInterests = hdnSelectedInterests.Value;

            // Get the user's Designated User ID from the session
            string userId = Session["UserID"]?.ToString();

            if (string.IsNullOrEmpty(userId))
            {
                ShowErrorMessage("User not logged in.");
                return;
            }

            try
            {
                // Fetch the current user details
                FirebaseHelper firebaseHelper = new FirebaseHelper();
                User user = await firebaseHelper.GetUserById(userId);

                if (user == null)
                {
                    ShowErrorMessage("User details not found.");
                    return;
                }

                // Default profile picture URL
                string defaultProfilePicUrl = "https://res.cloudinary.com/dkzzqgxce/image/upload/v1741189568/profile_placeholder_wbnidc.png";

                // Profile Picture Handling
                string profilePictureUrl = null;

                // Check if profile picture should be reset or a new file is uploaded
                if (hdnResetProfilePic.Value == "true")
                {
                    // Set to default profile picture
                    profilePictureUrl = defaultProfilePicUrl;
                    user.ProfilePictureUrl = profilePictureUrl;

                    // Reset the hidden field
                    hdnResetProfilePic.Value = "false";
                }
                else if (fuProfilePic.HasFile)
                {
                    try
                    {
                        // Validate and upload the file
                        profilePictureUrl = await ValidateAndUploadProfilePicture(fuProfilePic.PostedFile, userId);
                        user.ProfilePictureUrl = profilePictureUrl;
                    }
                    catch (Exception uploadEx)
                    {
                        ShowErrorMessage($"Failed to upload profile picture: {uploadEx.Message}");
                        return;
                    }
                }

                // Update the username (only if changed)
                if (!string.IsNullOrEmpty(newUsername))
                {
                    user.Username = newUsername;
                }

                // Update the area of interest (if provided)
                if (!string.IsNullOrEmpty(newInterests))
                {
                    if (user.MemberData == null)
                    {
                        user.MemberData = new MemberData();
                    }
                    user.MemberData.Interest_List = newInterests.Split(',').ToList();
                }

                // Save the updated user object to Firebase
                await firebaseHelper.UpdateUserPersonalInfo(user);

                // Update profile picture in Realtime Database if a new URL is set
                if (!string.IsNullOrEmpty(profilePictureUrl))
                {
                    await firebaseHelper.UpdateUserProfilePicture(userId, profilePictureUrl);
                }

                // Disable the TextBox after saving
                txtUsername.Enabled = false;
                btnEditUsername.Visible = true;

                // Clear the hidden fields
                hdnUpdatedUsername.Value = string.Empty;
                hdnSelectedInterests.Value = string.Empty;

                // Show success message
                ShowSuccessMessage("Changes saved successfully.");

                // Reload the user details
                FetchAndDisplayUserDetails();
            }
            catch (Exception ex)
            {
                // Show error message
                ShowErrorMessage($"Failed to save changes: {ex.Message}");
            }
        }
        // New method to validate and upload profile picture
        private async Task<string> ValidateAndUploadProfilePicture(HttpPostedFile uploadedFile, string userId)
        {
            // Validate file type
            string fileExtension = Path.GetExtension(uploadedFile.FileName).ToLower();
            string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };
            if (!allowedExtensions.Contains(fileExtension))
            {
                throw new Exception("Invalid file type. Please upload an image (JPG, JPEG, PNG, GIF).");
            }

            // Validate file size
            if (uploadedFile.ContentLength > 5 * 1024 * 1024)
            {
                throw new Exception("File size should not exceed 5MB.");
            }

            // Upload the file to Cloudinary
            CloudinaryHelper cloudinaryHelper = new CloudinaryHelper();
            string profilePictureUrl = cloudinaryHelper.UploadFile(uploadedFile, "profile_pictures");

            if (string.IsNullOrEmpty(profilePictureUrl))
            {
                throw new Exception("Failed to upload profile picture.");
            }

            // Validate the Cloudinary URL
            if (!IsValidCloudinaryUrl(profilePictureUrl))
            {
                throw new Exception("Invalid profile picture URL.");
            }

            return profilePictureUrl;
        }

        protected void btnRemoveProfilePic_Click(object sender, EventArgs e)
        {
            // Set the default profile picture URL in the page's UI
            string defaultProfilePicUrl = "https://res.cloudinary.com/dkzzqgxce/image/upload/v1741189568/profile_placeholder_wbnidc.png";
            profilePreview.ImageUrl = defaultProfilePicUrl;

            // Update the hidden field to indicate profile picture should be reset
            hdnResetProfilePic.Value = "true";

            // Show a message that the profile picture will be updated on save
            ShowSuccessMessage("Profile picture will be reset when you save changes.");
        }
        protected void btnEditUsername_Click(object sender, EventArgs e)
        {
            // Enable the TextBox for editing
            txtUsername.Enabled = true;
            btnEditUsername.Visible = false; // Hide the Edit button
        }

        protected void txtUsername_TextChanged(object sender, EventArgs e)
        {
            hdnUpdatedUsername.Value = txtUsername.Text;
        }

        protected void btnConfirmInterests_Click(object sender, EventArgs e)
        {
            // Get the selected interests from the hidden field
            string selectedInterests = hdnSelectedInterests.Value;

            if (!string.IsNullOrEmpty(selectedInterests))
            {
                // Split the selected interests and bind them to the Repeater
                var interestsList = selectedInterests.Split(',').Select(i => new { InterestName = i }).ToList();
                rptInterests.DataSource = interestsList;
                rptInterests.DataBind();
            }

            // Close the popup after updating
            ScriptManager.RegisterStartupScript(this, GetType(), "closePopup", "closePopup();", true);
        }


        // Add a method to validate Cloudinary URLs
        private bool IsValidCloudinaryUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
                return false;

            try
            {
                // Check if the URL is from Cloudinary
                return url.Contains("res.cloudinary.com") &&
                       (url.EndsWith(".jpg") || url.EndsWith(".png") || url.EndsWith(".jpeg") || url.EndsWith(".gif"));
            }
            catch
            {
                return false;
            }
        }
        // Add these helper methods if they don't exist
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