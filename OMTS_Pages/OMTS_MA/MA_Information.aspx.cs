using System;
using System.Collections.Generic;
using System.Drawing;
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
    public partial class MA_Information : System.Web.UI.Page, ILocalizable
    {
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

        protected async void Page_Load(object sender, EventArgs e)
        {


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

                // Load active badges
                await LoadActiveBadges();
                await LoadIntroductionMedia();
            }

        }

        private async Task LoadActiveBadges()
        {
            var firebaseHelper = new FirebaseHelper();

            // Get active badges from Firebase
            var activeBadges = await firebaseHelper.GetActiveBadges();

            // Bind badges to repeater
            rptBadges.DataSource = activeBadges;
            rptBadges.DataBind();
        }


        private async Task LoadIntroductionMedia()
        {
            try
            {
                var firebaseHelper = new FirebaseHelper();

                // Get the introduction media
                var introMedia = await firebaseHelper.GetIntroductionMedia("intro_media");

                if (introMedia != null)
                {
                    // Set the title
                    litMediaTitle.Text = introMedia.Title;

                    // Hide all media containers first
                    divVideoContainer.Visible = false;
                    divPdfContainer.Visible = false;
                    divImageContainer.Visible = false;

                    // Show the appropriate container based on media type
                    switch (introMedia.MediaType)
                    {
                        case "video":
                            divVideoContainer.Visible = true;

                            // Check if it's a YouTube link
                            if (introMedia.MediaUrl.Contains("youtube.com") || introMedia.MediaUrl.Contains("youtu.be"))
                            {
                                // Extract the YouTube video ID
                                string videoId = ExtractYouTubeVideoId(introMedia.MediaUrl);
                                if (!string.IsNullOrEmpty(videoId))
                                {
                                    // Set up the iframe
                                    litYouTubeEmbed.Text = $"<iframe width=\"560\" height=\"315\" src=\"https://www.youtube.com/embed/{videoId}\" " +
                                        $"frameborder=\"0\" allow=\"accelerometer; autoplay; clipboard-write; encrypted-media; " +
                                        $"gyroscope; picture-in-picture\" allowfullscreen></iframe>";
                                }
                                else
                                {
                                    // Fallback to direct link
                                    hlVideoLink.NavigateUrl = introMedia.MediaUrl;
                                    hlVideoLink.Visible = true;
                                    litYouTubeEmbed.Text = string.Empty;
                                }
                            }
                            // Check if it's a Google Drive video
                            else if (introMedia.MediaUrl.Contains("drive.google.com"))
                            {
                                // Set up a direct link since embedding might be complicated
                                hlVideoLink.NavigateUrl = introMedia.MediaUrl;
                                hlVideoLink.Visible = true;
                                litYouTubeEmbed.Text = string.Empty;
                            }
                            else
                            {
                                // Any other video link
                                hlVideoLink.NavigateUrl = introMedia.MediaUrl;
                                hlVideoLink.Visible = true;
                                litYouTubeEmbed.Text = string.Empty;
                            }
                            break;

                        case "pdf":
                            divPdfContainer.Visible = true;


                            // Try to embed if it's a Google Drive PDF
                            if (introMedia.MediaUrl.Contains("drive.google.com"))
                            {
                                string pdfId = ExtractGoogleDriveId(introMedia.MediaUrl);
                                if (!string.IsNullOrEmpty(pdfId))
                                {
                                    litPdfEmbed.Text = $"<iframe src=\"https://drive.google.com/file/d/{pdfId}/preview\" " +
                                        $"width=\"100%\" height=\"600px\" allow=\"autoplay\"></iframe>";
                                }
                            }
                            break;

                        case "image":
                            divImageContainer.Visible = true;
                            imgInfographic.ImageUrl = introMedia.MediaUrl;
                            break;
                    }
                }
                else
                {
                    // No media found, show a default message or image
                    pnlNoMedia.Visible = true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading introduction media: {ex.Message}");
                pnlNoMedia.Visible = true;
            }
        }

        private string ExtractYouTubeVideoId(string url)
        {
            try
            {
                // Handle full youtube.com URLs
                if (url.Contains("youtube.com/watch"))
                {
                    Uri uri = new Uri(url);
                    var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
                    return query["v"] ?? string.Empty;
                }

                // Handle youtu.be short URLs
                if (url.Contains("youtu.be/"))
                {
                    int index = url.IndexOf("youtu.be/");
                    string id = url.Substring(index + 9);
                    int endIndex = id.IndexOf('?');
                    if (endIndex > 0)
                    {
                        id = id.Substring(0, endIndex);
                    }
                    return id;
                }

                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        private string ExtractGoogleDriveId(string url)
        {
            try
            {
                // Example: https://drive.google.com/file/d/1JH6tFmB9oZCJjGkZ-JF8w-OLXhq0Uxk_/view?usp=sharing
                if (url.Contains("/file/d/"))
                {
                    int startIndex = url.IndexOf("/file/d/") + 8;
                    string id = url.Substring(startIndex);
                    int endIndex = id.IndexOf('/');
                    if (endIndex > 0)
                    {
                        id = id.Substring(0, endIndex);
                    }
                    return id;
                }

                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        private void LoadResources()
        {


            // Set page labels and controls from resource files
            litBadgesGallery.Text = GetGlobalResourceObject("Resources", "Heading_BadgesGallery")?.ToString();
            litAchievementInfo.Text = GetGlobalResourceObject("Resources", "Heading_MemberAchievement")?.ToString();
            


            // Button text
            btnInformation.Text = GetGlobalResourceObject("Resources", "SideBtnInfo").ToString();
            btnDashboard.Text = GetGlobalResourceObject("Resources", "SideBtnDb")?.ToString();
            btnApplicationForm.Text = GetGlobalResourceObject("Resources", "SideBtnMAApp")?.ToString();
            btnReviewApp.Text = GetGlobalResourceObject("Resources", "SideBtnRA")?.ToString();
            btnAwardBadge.Text = GetGlobalResourceObject("Resources", "SideBtnAB")?.ToString();
            btnMaterials.Text = GetGlobalResourceObject("Resources", "SideBtnMAMat")?.ToString();
          

        }


    }
}