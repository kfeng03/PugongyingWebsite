using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text;
using System.Web;

namespace fyp
{
    public partial class OC_CourseDetails : System.Web.UI.Page
    {
        private FirebaseHelper firebaseHelper = new FirebaseHelper();
        private string courseId;
        private Course currentCourse;
        private User currentUser;
        private string currentUserId;
        private bool isEnrolled = false;

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

            // Get current user ID
            currentUserId = Session["UserID"]?.ToString();

            // Get the course ID from the query string
            courseId = Request.QueryString["id"];
            if (string.IsNullOrEmpty(courseId))
            {
                lblMessage.Text = "Course ID is required.";
                lblMessage.ForeColor = Color.Red;
                lblMessage.Visible = true;
                panelCourseDetails.Visible = false;
                return;
            }

            if (!IsPostBack)
            {
                // Set sidebar button visibility based on user role
                string userRole = Session["UserRole"]?.ToString();
                btnMyCourse.Visible = userRole == "Member" || userRole == "Admin";
                btnJoinCourse.Visible = userRole == "Member" || userRole == "Admin";
                btnCompletedCourse.Visible = userRole == "Member" || userRole == "Admin";
                btnCourseManagement.Visible = userRole == "Staff" || userRole == "Admin";

                // Load course details and check enrollment status
                await LoadCourseDetails();
            }
        }

        private async Task LoadCourseDetails()
        {
            try
            {
                // Load course details
                currentCourse = await firebaseHelper.GetCourseById(courseId);
                if (currentCourse == null)
                {
                    lblMessage.Text = "Course not found.";
                    lblMessage.ForeColor = Color.Red;
                    lblMessage.Visible = true;
                    panelCourseDetails.Visible = false;
                    return;
                }

                // Load user details to check enrollment
                currentUser = await firebaseHelper.GetUserById(currentUserId);
                if (currentUser != null && currentUser.MemberData != null)
                {
                    isEnrolled = currentUser.MemberData.CourseID_List != null &&
                                 currentUser.MemberData.CourseID_List.Contains(courseId);
                }

                // Set course details
                litCourseName.Text = currentCourse.CourseName;
                litCourseDescription.Text = currentCourse.CourseDescription;
                litCourseCategory.Text = currentCourse.CourseCategory ?? "Uncategorized";



                // Show appropriate action buttons based on enrollment status
                btnJoinClass.Visible = !isEnrolled;
                btnLeaveCourse.Visible = isEnrolled;

                // Display course materials if available
                if (currentCourse.CourseMaterials != null && currentCourse.CourseMaterials.Count > 0)
                {
                    rptMaterials.DataSource = currentCourse.CourseMaterials.Keys.ToList();
                    rptMaterials.DataBind();
                    divNoMaterials.Visible = false;
                }
                else
                {
                    rptMaterials.DataSource = null;
                    rptMaterials.DataBind();
                    divNoMaterials.Visible = true;
                }

                // If the user is staff or admin, load enrolled user list
                string userRole = Session["UserRole"]?.ToString();
                if (userRole == "Staff" || userRole == "Admin")
                {
                    try
                    {
                        await LoadEnrolledUsers();
                    }
                    catch (Exception ex)
                    {
                        // Just log the error, don't let it break the page
                        System.Diagnostics.Debug.WriteLine($"Error loading enrolled users: {ex.Message}");
                    }
                }

                // Show the course details panel
                panelCourseDetails.Visible = true;
                lblMessage.Visible = false;
            }
            catch (Exception ex)
            {
                lblMessage.Text = $"Error loading course details: {ex.Message}";
                lblMessage.ForeColor = Color.Red;
                lblMessage.Visible = true;
                panelCourseDetails.Visible = false;
            }
        }

        private async Task LoadEnrolledUsers()
        {
            try
            {
                // Get all users
                var allUsers = await firebaseHelper.GetUsers();

                // Filter users who are enrolled in this course
                var enrolledUsers = allUsers
                    .Where(u => u.MemberData?.CourseID_List != null &&
                           u.MemberData.CourseID_List.Contains(courseId))
                    .Select(u => new {
                        UserID = u.UserId,
                        Username = u.Username,
                        Email = u.Email
                    })
                    .ToList();

                // Bind the enrolled users to the repeater
                if (enrolledUsers.Any())
                {
                    if (rptEnrolledUsers != null)
                    {
                        rptEnrolledUsers.DataSource = enrolledUsers;
                        rptEnrolledUsers.DataBind();
                        divEnrolledUsers.Visible = true;
                        divNoEnrolledUsers.Visible = false;
                    }
                }
                else
                {
                    divEnrolledUsers.Visible = false;
                    divNoEnrolledUsers.Visible = true;
                }
            }
            catch (Exception ex)
            {
                // Log error
                System.Diagnostics.Debug.WriteLine($"Error loading enrolled users: {ex.Message}");
                // Don't let this break the page - just hide the sections
                divEnrolledUsers.Visible = false;
                divNoEnrolledUsers.Visible = false;
            }
        }
        private string StandardizeGoogleDriveLink(string url)
        {
            try
            {
                // If URL is empty, return empty
                if (string.IsNullOrEmpty(url)) return string.Empty;

                string fileId = string.Empty;

                // Format: https://drive.google.com/file/d/FILE_ID/view or similar
                if (url.Contains("/file/d/"))
                {
                    int startIndex = url.IndexOf("/file/d/") + 9;
                    int endIndex = url.IndexOf("/", startIndex);
                    if (endIndex == -1) endIndex = url.Length;
                    fileId = url.Substring(startIndex, endIndex - startIndex);

                    // Remove any existing "1" at the beginning
                    if (fileId.StartsWith("1"))
                    {
                        fileId = fileId.Substring(1);
                    }

                    // Always add "1" at the beginning
                    fileId = "1" + fileId;

                    return $"https://drive.google.com/file/d/{fileId}/view?usp=sharing";
                }

                // Format: https://drive.google.com/open?id=FILE_ID
                else if (url.Contains("id="))
                {
                    Uri uri = new Uri(url);
                    var query = HttpUtility.ParseQueryString(uri.Query);
                    fileId = query["id"];

                    if (!string.IsNullOrEmpty(fileId))
                    {
                        // Remove any existing "1" at the beginning
                        if (fileId.StartsWith("1"))
                        {
                            fileId = fileId.Substring(1);
                        }

                        // Always add "1" at the beginning
                        fileId = "1" + fileId;

                        return $"https://drive.google.com/file/d/{fileId}/view?usp=sharing";
                    }
                }

                // If we couldn't parse it into a standard format, return the original
                return url;
            }
            catch
            {
                // If any error occurs, return the original URL
                return url;
            }
        }
        protected void rptMaterials_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                string materialName = e.Item.DataItem.ToString();
                HyperLink hlMaterial = (HyperLink)e.Item.FindControl("hlMaterial");

                if (hlMaterial != null && currentCourse?.CourseMaterials != null &&
                    currentCourse.CourseMaterials.ContainsKey(materialName))
                {
                    string materialUrl = currentCourse.CourseMaterials[materialName];

                    // Make sure the link is properly formatted for Google Drive links
                    if (materialUrl.Contains("drive.google.com"))
                    {
                        materialUrl = StandardizeGoogleDriveLink(materialUrl);
                        hlMaterial.Text = $"<i class='fas fa-file-pdf'></i> {materialName} (Google Drive)";
                    }
                    else
                    {
                        hlMaterial.Text = $"<i class='fas fa-file'></i> {materialName}";
                    }

                    hlMaterial.NavigateUrl = materialUrl;
                    hlMaterial.Target = "_blank"; // Open in new tab

                    // Only make materials accessible to enrolled users
                    hlMaterial.Enabled = isEnrolled;

                    // Add visual indication that materials are locked for non-enrolled users
                    if (!isEnrolled)
                    {
                        hlMaterial.CssClass = "material-link locked-material";
                        hlMaterial.ToolTip = "Enroll in the course to access this material";
                    }
                }
            }
        }



        protected void btnBack_Click(object sender, EventArgs e)
        {
            // Check if we have a referrer stored in session
            if (Session["CourseDetailsReferrer"] != null)
            {
                Response.Redirect(Session["CourseDetailsReferrer"].ToString());
                return;
            }

            // Default fallback - Go to My Courses
            Response.Redirect("OC_Courses.aspx");
        }
        protected async void btnLeaveCourse_Click(object sender, EventArgs e)
        {
            try
            {
                // Verify user is logged in
                if (string.IsNullOrEmpty(currentUserId))
                {
                    lblMessage.Text = "You must be logged in to leave a course.";
                    lblMessage.ForeColor = Color.Red;
                    lblMessage.Visible = true;
                    return;
                }

                // Reload user data to get latest state
                currentUser = await firebaseHelper.GetUserById(currentUserId);
                if (currentUser == null)
                {
                    lblMessage.Text = "Error retrieving user data.";
                    lblMessage.ForeColor = Color.Red;
                    lblMessage.Visible = true;
                    return;
                }

                // Check if user is enrolled in this course
                if (currentUser.MemberData?.CourseID_List == null ||
                    !currentUser.MemberData.CourseID_List.Contains(courseId))
                {
                    lblMessage.Text = "You are not enrolled in this course.";
                    lblMessage.ForeColor = Color.Red;
                    lblMessage.Visible = true;
                    return;
                }

                // Remove the course from user's course list
                currentUser.MemberData.CourseID_List.Remove(courseId);

                // Update user data in Firebase
                await firebaseHelper.UpdateUserCourseList(currentUserId, currentUser.MemberData.CourseID_List);

                // Show success message
                lblMessage.Text = "You have successfully left this course.";
                lblMessage.ForeColor = Color.Green;
                lblMessage.Visible = true;

                // Refresh the page to update the UI
                await LoadCourseDetails();
            }
            catch (Exception ex)
            {
                lblMessage.Text = $"Error leaving course: {ex.Message}";
                lblMessage.ForeColor = Color.Red;
                lblMessage.Visible = true;
            }
        }
        protected async void btnJoinClass_Click(object sender, EventArgs e)
        {
            try
            {
                // Verify user is logged in and user ID is available
                if (string.IsNullOrEmpty(currentUserId))
                {
                    // Double-check session
                    currentUserId = Session["UserID"]?.ToString();

                    if (string.IsNullOrEmpty(currentUserId))
                    {
                        // Redirect to login if no user ID is found
                        Response.Redirect("AM_LoginEmail.aspx", false);
                        Context.ApplicationInstance.CompleteRequest();
                        return;
                    }
                }

                // Reload user data with additional error handling
                try
                {
                    currentUser = await firebaseHelper.GetUserById(currentUserId);
                }
                catch (Exception getUserEx)
                {
                    System.Diagnostics.Debug.WriteLine($"Error retrieving user: {getUserEx.Message}");
                    lblMessage.Text = "Error retrieving user data. Please try again.";
                    lblMessage.ForeColor = Color.Red;
                    lblMessage.Visible = true;
                    return;
                }

                // Verify user was retrieved successfully
                if (currentUser == null)
                {
                    lblMessage.Text = "User not found. Please log in again.";
                    lblMessage.ForeColor = Color.Red;
                    lblMessage.Visible = true;

                    // Clear session and redirect to login
                    Session.Clear();
                    Response.Redirect("AM_LoginEmail.aspx", false);
                    Context.ApplicationInstance.CompleteRequest();
                    return;
                }

                // Rest of the existing method remains the same...
                // Check if user is already enrolled
                if (currentUser.MemberData?.CourseID_List != null &&
                    currentUser.MemberData.CourseID_List.Contains(courseId))
                {
                    lblMessage.Text = "You are already enrolled in this course.";
                    lblMessage.ForeColor = Color.Blue;
                    lblMessage.Visible = true;
                    await LoadCourseDetails(); // Refresh the page
                    return;
                }

                // Initialize MemberData if needed
                if (currentUser.MemberData == null)
                {
                    currentUser.MemberData = new MemberData
                    {
                        CourseID_List = new List<string>()
                    };
                }
                else if (currentUser.MemberData.CourseID_List == null)
                {
                    currentUser.MemberData.CourseID_List = new List<string>();
                }

                // Add the course to user's course list
                currentUser.MemberData.CourseID_List.Add(courseId);

                // Update user data in Firebase using the method we already have for updating event lists
                await firebaseHelper.UpdateUserCourseList(currentUserId, currentUser.MemberData.CourseID_List);

                // Show success message
                lblMessage.Text = "You have successfully joined this course!";
                lblMessage.ForeColor = Color.Green;
                lblMessage.Visible = true;

                // Refresh the page to show enrolled state
                await LoadCourseDetails();
            }
            catch (Exception ex)
            {
                // Log the full exception for debugging
                System.Diagnostics.Debug.WriteLine($"Unexpected error joining course: {ex}");

                lblMessage.Text = $"Unexpected error: {ex.Message}. Please try again.";
                lblMessage.ForeColor = Color.Red;
                lblMessage.Visible = true;
            }
        }
    }
}