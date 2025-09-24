using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace fyp
{
    public partial class OC_CourseManagement : Page
    {
        private FirebaseHelper firebaseHelper = new FirebaseHelper();

        private bool IsUserLoggedIn()
        {
            return Session["UserID"] != null || Session["FirebaseUID"] != null;
        }

        protected async void Page_Load(object sender, EventArgs e)
        {
            // Check if the user is logged in
            if (!IsUserLoggedIn())
            {
                Response.Redirect("~/OMTS_Pages/OMTS_AM/AM_LoginGoogle.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            // Check user role
            string userRole = Session["UserRole"]?.ToString();
            if (userRole != "Staff" && userRole != "Admin")
            {
                Response.Redirect("OC_Courses.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            if (!IsPostBack)
            {
                // Set sidebar button visibility
                SetSidebarButtonVisibility(userRole);

                // Load active courses
                await LoadActiveCourses();
            }
        }

        private void SetSidebarButtonVisibility(string userRole)
        {
            btnMyCourse.Visible = userRole == "Member" || userRole == "Admin";
            btnJoinCourse.Visible = userRole == "Member" || userRole == "Admin";
            btnCompletedCourse.Visible = userRole == "Member" || userRole == "Admin";
            btnCourseManagement.Visible = userRole == "Staff" || userRole == "Admin";
        }

        private async Task LoadActiveCourses()
        {
            try
            {
                // Get active courses from Firebase
                var courses = await firebaseHelper.GetActiveCourses();

                if (courses == null || !courses.Any())
                {
                    lblMessage.Text = "No active courses available.";
                    lblMessage.ForeColor = System.Drawing.Color.Blue;
                    rptCourses.DataSource = null;
                    rptCourses.DataBind();
                    return;
                }

                // Clear any previous messages
                lblMessage.Text = "";

                // Bind courses to repeater
                rptCourses.DataSource = courses;
                rptCourses.DataBind();
            }
            catch (Exception ex)
            {
                lblMessage.Text = $"Error loading courses: {ex.Message}";
                lblMessage.ForeColor = System.Drawing.Color.Red;
            }
        }

        protected void btnEdit_Click(object sender, EventArgs e)
        {
            LinkButton btn = (LinkButton)sender;
            string courseId = btn.CommandArgument;

            // Redirect to Edit Course Page with the Course ID
            Response.Redirect($"OC_EditCourse.aspx?courseId={courseId}");
        }

        protected void btnParticipants_Click(object sender, EventArgs e)
        {
            LinkButton btn = (LinkButton)sender;
            string courseId = btn.CommandArgument;

            // Redirect to Participants Page with the Course ID
            Response.Redirect($"OC_CourseParticipants.aspx?id={courseId}");
        }

        protected async void btnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                Button btn = (Button)sender;
                string courseId = btn.CommandArgument;

                if (string.IsNullOrEmpty(courseId))
                {
                    lblMessage.Text = "Invalid course ID.";
                    lblMessage.ForeColor = System.Drawing.Color.Red;
                    return;
                }

                // Soft delete the course
                await firebaseHelper.SoftDeleteCourse(courseId);

                // Refresh the course list
                await LoadActiveCourses();

                lblMessage.Text = "Course successfully deleted.";
                lblMessage.ForeColor = System.Drawing.Color.Green;
            }
            catch (Exception ex)
            {
                lblMessage.Text = $"Error deleting course: {ex.Message}";
                lblMessage.ForeColor = System.Drawing.Color.Red;
            }
        }

        protected void btnRecycleBin_Click(object sender, EventArgs e)
        {
            // Redirect to Recycle Bin page
            Response.Redirect("OC_RecycleBin.aspx");
        }
    }
}