using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace fyp
{
    public partial class MA_EditBadge : System.Web.UI.Page
    {
        private string _badgeId;

        private bool IsUserLoggedIn()
        {
            // Check if the user is logged in (ADD IN ALL PAGES THAT NEED ACCESS)
            // Check if the session contains the UserID or FirebaseUID
            return Session["UserID"] != null || Session["FirebaseUID"] != null;
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

                // Get the badgeID from the query string
                _badgeId = Request.QueryString["BadgeId"];
                if (!string.IsNullOrEmpty(_badgeId))
                {
                    await LoadBadgeDetails(_badgeId);
                }
                lblEditBadgeId.Text = _badgeId;
            }
        }

        private async Task LoadBadgeDetails(string badgeId)
        {
            var firebaseHelper = new FirebaseHelper();
            var badge = await firebaseHelper.GetBadgeById(badgeId);

            if (badge != null)
            {
                // Populate the form with course details
                txtEditBadgeName.Text = badge.BadgeName;
                txtEditBadgeDescription.Text = badge.BadgeDesc;
                txtEditPointsAwarded.Text = badge.BadgePoints.ToString();
                // Handle picture
            }
        }

        protected async void btnSaveEditBadge_Click(object sender, EventArgs e)
        {
            try
            {
                var firebaseHelper = new FirebaseHelper();
                string badgeId = Request.QueryString["BadgeId"];

                // Get updated course details
                var badge = new Badge
                {
                    BadgeId = badgeId, // Include CourseID
                    BadgeName = txtEditBadgeName.Text.Trim(),
                    BadgeDesc = txtEditBadgeDescription.Text.Trim(),
                    BadgePoints = int.Parse(txtEditPointsAwarded.Text)
                    // Handle other fields...
                };

                await firebaseHelper.UpdateBadge(badge);
                Response.Redirect("MA_Materials.aspx");
            }
            catch (Exception ex)
            {
                lblMessage.Text = "Error: " + ex.Message;
                lblMessage.ForeColor = Color.Red;
            }
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            // Redirect back to Course Management page
            Response.Redirect("MA_Materials.aspx");
        }
    }
}