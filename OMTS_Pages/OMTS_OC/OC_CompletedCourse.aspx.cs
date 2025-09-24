using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace fyp
{
    public partial class OC_CompletedCourse : System.Web.UI.Page
    {
        private FirebaseHelper firebaseHelper = new FirebaseHelper();
        private string currentUserID;

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

            currentUserID = Session["UserID"]?.ToString();

            if (!IsPostBack)
            {
                string userRole = Session["UserRole"]?.ToString();

                // Set button visibility
                btnMyCourse.Visible = userRole == "Member" || userRole == "Admin";
                btnJoinCourse.Visible = userRole == "Member" || userRole == "Admin";
                btnCompletedCourse.Visible = userRole == "Member" || userRole == "Admin";
                btnCourseManagement.Visible = userRole == "Staff" || userRole == "Admin";

                try
                {
                    await LoadCompletedCourses();
                }
                catch (Exception ex)
                {
                    lblMessage.Text = "Error loading completed courses: " + ex.Message;
                    lblMessage.ForeColor = System.Drawing.Color.Red;
                    lblNoCourses.Visible = true;
                }
            }
        }

        private async Task LoadCompletedCourses()
        {
            try
            {
                // Use our new method to get completed courses
                var completedCourses = await firebaseHelper.GetUserCompletedCourses(currentUserID);

                if (!completedCourses.Any())
                {
                    rptCompletedCourses.DataSource = null;
                    rptCompletedCourses.DataBind();
                    lblNoCourses.Visible = true;
                    return;
                }

                // Prepare data for display - including completion dates
                var coursesForDisplay = completedCourses.Select((course, index) => new
                {
                    CourseID = course.CourseID,
                    CourseName = course.CourseName,
                    CourseCategory = course.CourseCategory ?? "Uncategorized",
                    CoursePictureUrl = course.CoursePictureUrl,
                    // Since we don't store actual completion dates, simulate them
                    // In a real implementation, you'd want to store the actual completion date
                    CompletionDate = DateTime.Now.AddDays(-index * 7)
                }).ToList();

                rptCompletedCourses.DataSource = coursesForDisplay;
                rptCompletedCourses.DataBind();
                lblNoCourses.Visible = false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading completed courses: {ex.Message}");
                throw; // Re-throw to be handled by the caller
            }
        }

        protected void btnViewCourse_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            string courseId = btn.CommandArgument;

            if (!string.IsNullOrEmpty(courseId))
            {
                // Store referrer for back button
                Session["CourseDetailsReferrer"] = Request.Url.AbsolutePath;
                Response.Redirect($"OC_CourseDetails.aspx?id={courseId}");
            }
        }

        protected async void btnViewCertificate_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            string courseId = btn.CommandArgument;

            if (string.IsNullOrEmpty(courseId))
            {
                return;
            }

            try
            {
                // Get course and user details
                var course = await firebaseHelper.GetCourseById(courseId);
                var user = await firebaseHelper.GetUserById(currentUserID);

                if (course == null || user == null)
                {
                    lblMessage.Text = "Error: Could not load course or user details.";
                    lblMessage.ForeColor = System.Drawing.Color.Red;
                    return;
                }

                // Populate certificate details
                litUserName.Text = user.Username;
                litCourseName.Text = course.CourseName;

                // In a real app, you would get the actual completion date
                // For now, just use the current date
                litCompletionDate.Text = DateTime.Now.ToString("MMMM dd, yyyy");

                // Show the certificate panel
                pnlCertificate.Visible = true;
                ScriptManager.RegisterStartupScript(this, GetType(), "ShowCertificate",
                    "document.querySelector('.certificate-container').style.display = 'flex';", true);
            }
            catch (Exception ex)
            {
                lblMessage.Text = "Error displaying certificate: " + ex.Message;
                lblMessage.ForeColor = System.Drawing.Color.Red;
            }
        }
    }
}