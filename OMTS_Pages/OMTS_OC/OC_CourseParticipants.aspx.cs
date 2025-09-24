using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace fyp
{
    public partial class OC_CourseParticipants : System.Web.UI.Page
    {
        private FirebaseHelper firebaseHelper = new FirebaseHelper();
        private string courseId;
        private Course currentCourse;

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

            // Check user role - only staff and admin can access this page
            string userRole = Session["UserRole"]?.ToString();
            if (userRole != "Staff" && userRole != "Admin")
            {
                Response.Redirect("OC_Courses.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            // Get the course ID from the query string
            courseId = Request.QueryString["id"];
            if (string.IsNullOrEmpty(courseId))
            {
                Response.Redirect("OC_CourseManagement.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            if (!IsPostBack)
            {
                // Set visibility for each button based on the user's role
                btnMyCourse.Visible = userRole == "Member" || userRole == "Admin";
                btnJoinCourse.Visible = userRole == "Member" || userRole == "Admin";
                btnCompletedCourse.Visible = userRole == "Member" || userRole == "Admin";
                btnCourseManagement.Visible = userRole == "Staff" || userRole == "Admin";

                await LoadCourseAndParticipants();
            }
        }

        private async Task LoadCourseAndParticipants()
        {
            try
            {
                // Get course details
                currentCourse = await firebaseHelper.GetCourseById(courseId);
                if (currentCourse == null)
                {
                    lblMessage.Text = "Course not found.";
                    return;
                }

                // Set course information
                litCourseName.Text = currentCourse.CourseName;

                // Display course info with category
                string courseInfo = $"Category: {currentCourse.CourseCategory ?? "Uncategorized"}";
                courseInfo += $" | Maximum Students: {currentCourse.NumberOfStudents}";

                if (!string.IsNullOrEmpty(currentCourse.CreatedDate))
                    courseInfo += $" | Created: {currentCourse.CreatedDate}";

                litCourseInfo.Text = courseInfo;

                // Load participants
                await LoadParticipants();
            }
            catch (Exception ex)
            {
                lblMessage.Text = "Error loading course details: " + ex.Message;
            }
        }

        private async Task LoadParticipants()
        {
            try
            {
                // Get all users from Firebase
                var allUsers = await firebaseHelper.GetUsers();
                if (allUsers == null || !allUsers.Any())
                {
                    return;
                }

                // Filter users who have this course in their CourseID_List
                var participants = allUsers
                    .Where(u => u.MemberData?.CourseID_List != null &&
                                u.MemberData.CourseID_List.Contains(courseId))
                    .Select(u => new
                    {
                        UserId = u.UserId,
                        Username = u.Username,
                        Email = u.Email,
                        PhoneNumber = u.PhoneNumber,
                        JoinDate = u.JoinDate,
                        IsCompleted = u.MemberData?.CompletedCourseID_List != null &&
                                     u.MemberData.CompletedCourseID_List.Contains(courseId)
                    })
                    .ToList();

                // Bind participants to the GridView
                gvParticipants.DataSource = participants;
                gvParticipants.DataBind();
            }
            catch (Exception ex)
            {
                lblMessage.Text = "Error loading participants: " + ex.Message;
                lblMessage.ForeColor = System.Drawing.Color.Red;
            }
        }

        protected void btnBack_Click(object sender, EventArgs e)
        {
            Response.Redirect("OC_CourseManagement.aspx");
        }

        protected void btnExport_Click(object sender, EventArgs e)
        {
            try
            {
                // Create CSV content
                StringBuilder csv = new StringBuilder();

                // Add headers
                csv.AppendLine("User ID,Username,Email,Phone Number,Join Date");

                // Add data from GridView
                foreach (GridViewRow row in gvParticipants.Rows)
                {
                    string userId = row.Cells[0].Text;
                    string username = row.Cells[1].Text;
                    string email = row.Cells[2].Text;
                    string phoneNumber = row.Cells[3].Text;
                    string joinDate = row.Cells[4].Text;

                    // Properly escape values with quotes if they contain commas
                    username = EscapeCsvValue(username);
                    email = EscapeCsvValue(email);
                    phoneNumber = EscapeCsvValue(phoneNumber);
                    joinDate = EscapeCsvValue(joinDate);

                    csv.AppendLine($"{userId},{username},{email},{phoneNumber},{joinDate}");
                }

                // Set response headers for file download
                Response.Clear();
                Response.Buffer = true;
                Response.AddHeader("content-disposition",
                    $"attachment;filename=Participants_{currentCourse.CourseName.Replace(" ", "_")}_{DateTime.Now.ToString("yyyyMMdd")}.csv");
                Response.Charset = "";
                Response.ContentType = "application/text";
                Response.Output.Write(csv.ToString());
                Response.Flush();
                Response.End();
            }
            catch (Exception ex)
            {
                lblMessage.Text = "Error exporting participants: " + ex.Message;
            }
        }
        protected async void btnMarkComplete_Command(object sender, CommandEventArgs e)
        {
            try
            {
                string userId = e.CommandArgument.ToString();
                bool markAsComplete = e.CommandName == "MarkComplete";

                // Get the user
                var user = await firebaseHelper.GetUserById(userId);
                if (user == null)
                {
                    lblMessage.Text = "Error: User not found.";
                    lblMessage.ForeColor = System.Drawing.Color.Red;
                    return;
                }

                // Initialize lists if needed
                if (user.MemberData == null)
                {
                    user.MemberData = new MemberData();
                }

                if (user.MemberData.CompletedCourseID_List == null)
                {
                    user.MemberData.CompletedCourseID_List = new List<string>();
                }

                // Update completion status
                if (markAsComplete && !user.MemberData.CompletedCourseID_List.Contains(courseId))
                {
                    // Add to completed courses
                    user.MemberData.CompletedCourseID_List.Add(courseId);
                    lblMessage.Text = $"Course marked as complete for user {user.Username}.";
                }
                else if (!markAsComplete && user.MemberData.CompletedCourseID_List.Contains(courseId))
                {
                    // Remove from completed courses
                    user.MemberData.CompletedCourseID_List.Remove(courseId);
                    lblMessage.Text = $"Course marked as incomplete for user {user.Username}.";
                }
                else
                {
                    // Status already set correctly
                    lblMessage.Text = markAsComplete
                        ? "Course is already marked as complete for this user."
                        : "Course is already marked as incomplete for this user.";
                    lblMessage.ForeColor = System.Drawing.Color.Blue;
                    return;
                }

                // Update in Firebase
                var updates = new Dictionary<string, object>
        {
            { "MemberData/CompletedCourseID_List", user.MemberData.CompletedCourseID_List }
        };

                await firebaseHelper.UpdateUserCompletedCourses(userId, updates);

                // Show success message
                lblMessage.ForeColor = System.Drawing.Color.Green;

                // Refresh the participants list
                await LoadParticipants();
            }
            catch (Exception ex)
            {
                lblMessage.Text = "Error updating completion status: " + ex.Message;
                lblMessage.ForeColor = System.Drawing.Color.Red;
            }
        }

        private string EscapeCsvValue(string value)
        {
            // If value contains comma, quotes, or newline, wrap in quotes and escape inner quotes
            if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
            {
                return "\"" + value.Replace("\"", "\"\"") + "\"";
            }
            return value;
        }
    }
}