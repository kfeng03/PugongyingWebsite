using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace fyp
{
    public partial class OC_RecycleBin : Page
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

            // Check user role - only staff and admin can access this page
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
                btnMyCourse.Visible = userRole == "Member" || userRole == "Admin";
                btnJoinCourse.Visible = userRole == "Member" || userRole == "Admin";
                btnCompletedCourse.Visible = userRole == "Member" || userRole == "Admin";
                btnCourseManagement.Visible = userRole == "Staff" || userRole == "Admin";

                await LoadDeletedCourses();
            }
        }

        private async Task LoadDeletedCourses()
        {
            try
            {
                var deletedCourses = await firebaseHelper.GetDeletedCourses();

                if (deletedCourses == null || !deletedCourses.Any())
                {
                    lblMessage.Text = "No deleted courses found.";
                    lblMessage.ForeColor = Color.Blue;
                    lblMessage.Visible = true;
                    rptDeletedCourses.DataSource = null;
                    rptDeletedCourses.DataBind();
                    return;
                }

                rptDeletedCourses.DataSource = deletedCourses;
                rptDeletedCourses.DataBind();
                lblMessage.Visible = false;
            }
            catch (Exception ex)
            {
                lblMessage.Text = "Error loading deleted courses: " + ex.Message;
                lblMessage.ForeColor = Color.Red;
                lblMessage.Visible = true;
            }
        }

        protected async void btnRestore_Click(object sender, EventArgs e)
        {
            try
            {
                var btnRestore = (LinkButton)sender;
                string courseId = btnRestore.CommandArgument;

                if (string.IsNullOrEmpty(courseId))
                {
                    lblMessage.Text = "Invalid course ID.";
                    lblMessage.ForeColor = Color.Red;
                    lblMessage.Visible = true;
                    return;
                }

                // Restore the course
                await firebaseHelper.RestoreCourse(courseId);

                // Show success message
                lblMessage.Text = "Course restored successfully.";
                lblMessage.ForeColor = Color.Green;
                lblMessage.Visible = true;

                // Refresh the course list
                await LoadDeletedCourses();
            }
            catch (Exception ex)
            {
                lblMessage.Text = "Error restoring course: " + ex.Message;
                lblMessage.ForeColor = Color.Red;
                lblMessage.Visible = true;
            }
        }

        protected async void btnPermanentDelete_Click(object sender, EventArgs e)
        {
            try
            {
                var btn = (LinkButton)sender;
                string courseId = btn.CommandArgument;

                if (string.IsNullOrEmpty(courseId))
                {
                    lblMessage.Text = "Invalid course ID.";
                    lblMessage.ForeColor = Color.Red;
                    lblMessage.Visible = true;
                    return;
                }

                // Permanently delete the course
                await firebaseHelper.DeleteCourse(courseId);

                // Show success message
                lblMessage.Text = "Course permanently deleted.";
                lblMessage.ForeColor = Color.Green;
                lblMessage.Visible = true;

                // Refresh the course list
                await LoadDeletedCourses();
            }
            catch (Exception ex)
            {
                lblMessage.Text = "Error deleting course: " + ex.Message;
                lblMessage.ForeColor = Color.Red;
                lblMessage.Visible = true;
            }
        }

        protected void btnBackToCourseManagement_Click(object sender, EventArgs e)
        {
            Response.Redirect("OC_CourseManagement.aspx");
        }

        protected async void btnEmptyRecycleBin_Click(object sender, EventArgs e)
        {
            try
            {
                var deletedCourses = await firebaseHelper.GetDeletedCourses();

                if (deletedCourses == null || !deletedCourses.Any())
                {
                    lblMessage.Text = "No deleted courses to empty.";
                    lblMessage.ForeColor = Color.Blue;
                    lblMessage.Visible = true;
                    return;
                }

                // Permanently delete all courses in recycle bin
                foreach (var course in deletedCourses)
                {
                    await firebaseHelper.DeleteCourse(course.CourseID);
                }

                // Show success message
                lblMessage.Text = "Recycle bin emptied successfully.";
                lblMessage.ForeColor = Color.Green;
                lblMessage.Visible = true;

                // Refresh the course list
                await LoadDeletedCourses();
            }
            catch (Exception ex)
            {
                lblMessage.Text = "Error emptying recycle bin: " + ex.Message;
                lblMessage.ForeColor = Color.Red;
                lblMessage.Visible = true;
            }
        }
    }
}