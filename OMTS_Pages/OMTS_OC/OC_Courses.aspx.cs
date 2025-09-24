using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace fyp
{
    public partial class OC_Courses : System.Web.UI.Page
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
            System.Diagnostics.Debug.WriteLine($"Current User ID: {currentUserID}");

            if (!IsPostBack)
            {
                string userRole = Session["UserRole"]?.ToString();

                // Set button visibility
                btnMyCourse.Visible = userRole == "Member" || userRole == "Admin";
                btnJoinCourse.Visible = userRole == "Member" || userRole == "Admin";
                btnCompletedCourse.Visible = userRole == "Member" || userRole == "Admin";
                btnCourseManagement.Visible = userRole == "Staff" || userRole == "Admin";

                await LoadMyCourses();
            }
        }

        private async Task LoadMyCourses()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Loading my courses...");

                // Get the current user with full details
                var user = await firebaseHelper.GetUserById(currentUserID);
                System.Diagnostics.Debug.WriteLine($"User loaded: {user != null}");

                if (user == null)
                {
                    System.Diagnostics.Debug.WriteLine("User is null");
                    rptMyCourses.DataSource = null;
                    rptMyCourses.DataBind();
                    lblNoCourses.Visible = true;
                    return;
                }

                // Debug user's MemberData
                System.Diagnostics.Debug.WriteLine($"MemberData exists: {user.MemberData != null}");
                if (user.MemberData != null)
                {
                    System.Diagnostics.Debug.WriteLine($"CourseID_List exists: {user.MemberData.CourseID_List != null}");
                    if (user.MemberData.CourseID_List != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"CourseID_List count: {user.MemberData.CourseID_List.Count}");
                        System.Diagnostics.Debug.WriteLine($"CourseID_List: {string.Join(", ", user.MemberData.CourseID_List)}");
                    }
                }

                // Check if user has joined any courses
                if (user.MemberData?.CourseID_List == null || !user.MemberData.CourseID_List.Any())
                {
                    System.Diagnostics.Debug.WriteLine("No courses found for this user");
                    rptMyCourses.DataSource = null;
                    rptMyCourses.DataBind();
                    lblNoCourses.Visible = true;
                    return;
                }

                // Get all courses
                System.Diagnostics.Debug.WriteLine("Getting all courses");
                var allCourses = await firebaseHelper.GetActiveCourses();
                System.Diagnostics.Debug.WriteLine($"Total courses: {allCourses.Count}");

                // Get the completed courses list (if it exists)
                var completedCourseIds = user.MemberData.CompletedCourseID_List ?? new List<string>();

                // Filter for courses the user has joined but not completed
                var myCourses = allCourses
                    .Where(c => user.MemberData.CourseID_List.Contains(c.CourseID) &&
                              !completedCourseIds.Contains(c.CourseID))
                    .ToList();

                System.Diagnostics.Debug.WriteLine($"Filtered courses: {myCourses.Count}");
                foreach (var course in myCourses)
                {
                    System.Diagnostics.Debug.WriteLine($"Course: {course.CourseID} - {course.CourseName}");
                }

                if (!myCourses.Any())
                {
                    System.Diagnostics.Debug.WriteLine("No matching courses found");
                    rptMyCourses.DataSource = null;
                    rptMyCourses.DataBind();
                    lblNoCourses.Visible = true;
                    return;
                }

                // Create data for display (without progress)
                var coursesForDisplay = myCourses
                    .Select(course => new
                    {
                        CourseID = course.CourseID,
                        CourseName = course.CourseName,
                        CourseCategory = course.CourseCategory ?? "Uncategorized"
                    })
                    .ToList();

                System.Diagnostics.Debug.WriteLine("Binding courses to repeater");
                rptMyCourses.DataSource = coursesForDisplay;
                rptMyCourses.DataBind();
                lblNoCourses.Visible = false;
                System.Diagnostics.Debug.WriteLine("Courses bound successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading courses: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");

                lblMessage.Text = "Error loading courses: " + ex.Message;
                lblMessage.ForeColor = System.Drawing.Color.Red;
                lblNoCourses.Visible = true;
            }
        }

        protected void btnViewCourse_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            string courseId = btn.CommandArgument;

            if (!string.IsNullOrEmpty(courseId))
            {
                // Save referrer for back button
                Session["CourseDetailsReferrer"] = Request.Url.AbsolutePath;
                Response.Redirect($"OC_CourseDetails.aspx?id={courseId}");
            }
        }
    }
}