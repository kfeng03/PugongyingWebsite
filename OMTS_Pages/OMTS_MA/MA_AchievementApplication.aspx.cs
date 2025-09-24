using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using static System.Net.Mime.MediaTypeNames;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System.Threading.Tasks;
using static fyp.MA_AchievementDashboard;

namespace fyp
{
    public partial class MA_AchievementApplication : System.Web.UI.Page, ILocalizable
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
                btnInformation.Visible = true; // All can access
                btnDashboard.Visible = userRole == "Member" || userRole == "Admin";
                btnApplicationForm.Visible = userRole == "Member" || userRole == "Admin";
                btnReviewApp.Visible = userRole == "Staff" || userRole == "Admin";
                btnAwardBadge.Visible = userRole == "Staff" || userRole == "Admin";
                btnMaterials.Visible = userRole == "Staff" || userRole == "Admin";


                LoadResources();
                Page.DataBind();

                await BindEvents();
                BindRoles();
            }

        }
        private async Task BindEvents()
        {
            FirebaseHelper firebaseHelper = new FirebaseHelper();
            var events = await firebaseHelper.GetEvents();

            // Clear existing items
            ddlEvent.Items.Clear();

            // Add a default item
            ddlEvent.Items.Add(new ListItem("Select Event", ""));

            // Bind the events to the dropdown
            foreach (var ev in events)
            {
                ddlEvent.Items.Add(new ListItem(ev.EventTitle, ev.EventID));
            }
        }

        private void BindRoles()
        {
            // Clear existing items
            ddlRole.Items.Clear();

            // Add a default item
            ddlRole.Items.Add(new ListItem("Select Role", ""));

            // Add predefined roles
            ddlRole.Items.Add(new ListItem("Participant", "Participant"));
            ddlRole.Items.Add(new ListItem("Facilitator", "Facilitator"));
            ddlRole.Items.Add(new ListItem("Organising Committee", "Organising Committee"));
            ddlRole.Items.Add(new ListItem("Event Secretary", "Event Secretary"));
            ddlRole.Items.Add(new ListItem("Event President", "Event President"));
        }

        private bool IsUserLoggedIn()
        {
            // Check if the user is logged in (ADD IN ALL PAGES THAT NEED ACCESS)
            // Check if the session contains the UserID or FirebaseUID
            return Session["UserID"] != null || Session["FirebaseUID"] != null;
        }
        


         private void LoadResources()
        {
            // Set page labels and controls from resource files
            litHeading.Text = GetGlobalResourceObject("Resources", "Heading_AchievementApplication")?.ToString();
            litEvent.Text = GetGlobalResourceObject("Resources", "EventNote")?.ToString();
            litAttchNote.Text = GetGlobalResourceObject("Resources", "ApplAttachmentNote")?.ToString();


            // Button text
            btnInformation.Text = GetGlobalResourceObject("Resources", "SideBtnInfo").ToString();
            btnDashboard.Text = GetGlobalResourceObject("Resources", "SideBtnDb")?.ToString();
            btnApplicationForm.Text = GetGlobalResourceObject("Resources", "SideBtnMAApp")?.ToString();
            btnReviewApp.Text = GetGlobalResourceObject("Resources", "SideBtnRA")?.ToString();
            btnAwardBadge.Text = GetGlobalResourceObject("Resources", "SideBtnAB")?.ToString();
            btnMaterials.Text = GetGlobalResourceObject("Resources", "SideBtnMAMat")?.ToString();
            btnSubmit.Text = GetGlobalResourceObject("Resources", "Button_Submit")?.ToString();
        }


        protected async void btnSubmit_Click(object sender, EventArgs e)
        {
            
            try
            {
                var firebaseHelper = new FirebaseHelper();

                // Get user inputs
                string applicantRefId = Session["UserId"].ToString();
                string eventid = ddlEvent.SelectedValue;
                string participatedRole = ddlRole.SelectedValue;
                string learningOutcome = txtLearningOutcome.Text;
                string comment = "no comment yet";
                string fileUrl = txtAttachment.Text;
                string status = "Pending";
                //string fileName = fuAttach.PostedFile.FileName;
                //int fileSize = fuAttach.PostedFile.ContentLength;
                //ShowSuccessMessage($"File '{fileName}' ({fileSize} bytes) uploaded successfully.");
                // Handle attachment Upload (if a new file is uploaded)
                // Convert the file to a byte array
                //byte[] fileBytes;
                //using (var stream = new MemoryStream())
                //{
                //    fuAttach.PostedFile.InputStream.CopyTo(stream);
                //    fileBytes = stream.ToArray();
                //}

                //// Upload the file to Google Drive
                //string fileUrl = await UploadToGoogleDrive(fileName, fileBytes);
                string applicationId = lblApplication.Text;

                // Save the URL to Firebase Database
                var application = new AchievementApplication
                {
                    ApplicationId = applicationId,
                    ApplicantRefId = applicantRefId,
                    EventId = eventid,
                    ParticipatedRole = participatedRole,
                    LearningOutcome = learningOutcome,
                    Comment = comment,
                    SupportingDocUrl = fileUrl,
                    Status = status
                };

                
                await firebaseHelper.SaveAchievementApplication(application);

                // Show success message
                ShowSuccessMessage("File uploaded and application submitted successfully.", "MA_AchievementDashboard.aspx");

            }
            catch (Exception ex)
            {
                // Show error message
                ShowErrorMessage("Error adding new entry");
            }
            
        }


        //private async Task<string> UploadToGoogleDrive(string fileName, byte[] fileBytes)
        //{
        //    UserCredential credential;

        //    using (var stream = new FileStream("~/App_Data/credentials.json", FileMode.Open, FileAccess.Read))
        //    {
        //        string credPath = "token.json";
        //        credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
        //            GoogleClientSecrets.Load(stream).Secrets,
        //            new[] { DriveService.Scope.DriveFile },
        //            "user",
        //            CancellationToken.None,
        //            new FileDataStore(credPath, true));
        //    }

        //    var service = new DriveService(new BaseClientService.Initializer()
        //    {
        //        HttpClientInitializer = credential,
        //        ApplicationName = "FYP-OMTS"
        //    });

        //    var fileMetadata = new Google.Apis.Drive.v3.Data.File()
        //    {
        //        Name = fileName
        //    };

        //    using (var stream = new MemoryStream(fileBytes))
        //    {
        //        var request = service.Files.Create(fileMetadata, stream, "application/octet-stream");
        //        request.Fields = "id, webViewLink";
        //        var file = await request.UploadAsync();

        //        if (file.Status == Google.Apis.Upload.UploadStatus.Completed)
        //        {
        //            var uploadedFile = request.ResponseBody;
        //            string embedLink = $"https://drive.google.com/file/d/{uploadedFile.Id}/preview"; // Construct the embed link
        //            return embedLink; // Return the embed link
        //        }
        //        else
        //        {
        //            throw new Exception("Failed to upload file to Google Drive.");
        //        }
        //    }
        //}

        private void ShowSuccessMessage(string message, string redirectUrl)
        {
            string script = $"alert('{message}'); window.location.href = '{redirectUrl}';";
            ClientScript.RegisterStartupScript(this.GetType(), "Popup", script, true);
        }

        private void ShowErrorMessage(string message)
        {
            string script = $"alert('{message}');";
            ClientScript.RegisterStartupScript(this.GetType(), "Popup", script, true);
        }
    }
}