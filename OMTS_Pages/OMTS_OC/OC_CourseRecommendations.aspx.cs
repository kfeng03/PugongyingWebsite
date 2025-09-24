using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Threading.Tasks;

namespace fyp
{
    public partial class OC_CourseRecommendations : System.Web.UI.Page
    {
        private FirebaseHelper firebaseHelper = new FirebaseHelper();
        private string currentUserID;
        private User currentUser;

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

            // Get the current user's ID
            currentUserID = Session["UserID"]?.ToString();

            if (!IsPostBack)
            {
                // Set up sidebar navigation based on user role
                string userRole = Session["UserRole"]?.ToString();
                btnMyCourse.Visible = userRole == "Member" || userRole == "Admin";
                btnJoinCourse.Visible = userRole == "Member" || userRole == "Admin";
                btnCompletedCourse.Visible = userRole == "Member" || userRole == "Admin";
                btnRecommendations.Visible = userRole == "Member" || userRole == "Admin";
                btnCourseManagement.Visible = userRole == "Staff" || userRole == "Admin";

                try
                {
                    // Show loading indicator
                    lblMessage.Text = "Loading your personalized recommendations...";
                    lblMessage.ForeColor = System.Drawing.Color.Blue;
                    lblMessage.Visible = true;

                    // Load user and recommendations
                    await LoadRecommendations();
                }
                catch (Exception ex)
                {
                    lblMessage.Text = "Error loading recommendations: " + ex.Message;
                    lblMessage.ForeColor = System.Drawing.Color.Red;
                    lblMessage.Visible = true;
                }
            }
        }

        private async Task LoadRecommendations()
        {
            try
            {
                // Load current user with full details
                currentUser = await firebaseHelper.GetUserById(currentUserID);
                if (currentUser == null)
                {
                    ShowError("Could not load user data.");
                    return;
                }

                // Load all active courses
                var allCourses = await firebaseHelper.GetActiveCourses();
                if (!allCourses.Any())
                {
                    ShowNoRecommendations("No courses are currently available.");
                    return;
                }

                // Get user's enrolled and completed courses
                var enrolledCourses = currentUser.MemberData?.CourseID_List ?? new List<string>();
                var completedCourseIds = currentUser.MemberData?.CompletedCourseID_List ?? new List<string>();

                // Filter for available courses (not enrolled, not completed)
                var availableCourses = allCourses.Where(c =>
                    !enrolledCourses.Contains(c.CourseID) &&
                    !completedCourseIds.Contains(c.CourseID)
                ).ToList();

                if (!availableCourses.Any())
                {
                    ShowNoRecommendations("You've enrolled in or completed all available courses.");
                    return;
                }

                // Get completed courses as objects for better recommendations
                var completedCourses = new List<Course>();
                foreach (var courseId in completedCourseIds)
                {
                    var course = await firebaseHelper.GetCourseById(courseId);
                    if (course != null)
                    {
                        completedCourses.Add(course);
                    }
                }

                // Get interest-based recommendations using Gemini API
                await LoadInterestBasedRecommendations(availableCourses, completedCourses);

                // Use traditional methods for these categories
                await LoadPopularCourseRecommendations(availableCourses);
                await LoadSimilarToCompletedRecommendations(availableCourses, completedCourseIds);

                // Hide loading message
                lblMessage.Visible = false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in LoadRecommendations: {ex.Message}");
                ShowError("An error occurred while loading recommendations: " + ex.Message);
            }
        }

        private async Task LoadInterestBasedRecommendations(List<Course> availableCourses, List<Course> completedCourses)
        {
            try
            {
                // Get user interests
                var userInterests = currentUser.MemberData?.Interest_List ?? new List<string>();

                if (!userInterests.Any())
                {
                    pnlInterestRecommendations.Visible = true;
                    lblNoInterestRecommendations.Visible = true;
                    rptInterestRecommendations.Visible = false;
                    return;
                }

                // Use Gemini API for interest-based recommendations
                Dictionary<string, string> recommendations;

                try
                {
                    // Call Gemini API for recommendations
                    recommendations = await firebaseHelper.GetGeminiCourseRecommendations(
                        currentUser,
                        availableCourses,
                        completedCourses
                    );
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error calling Gemini API: {ex.Message}");

                    // Fallback to local recommendations if API fails
                    recommendations = firebaseHelper.GetLocalCourseRecommendations(
                        currentUser,
                        availableCourses
                    );
                }

                if (recommendations.Any())
                {
                    // Create a list for the repeater
                    var recommendedCourses = new List<dynamic>();

                    foreach (var entry in recommendations)
                    {
                        string courseId = entry.Key;
                        string reason = entry.Value;

                        // Find the course in available courses
                        var course = availableCourses.FirstOrDefault(c => c.CourseID == courseId);
                        if (course != null)
                        {
                            recommendedCourses.Add(new
                            {
                                course.CourseID,
                                course.CourseName,
                                course.CourseCategory,
                                course.NumberOfStudents,
                                MatchReason = reason
                            });
                        }
                    }

                    if (recommendedCourses.Any())
                    {
                        rptInterestRecommendations.DataSource = recommendedCourses;
                        rptInterestRecommendations.DataBind();
                        pnlInterestRecommendations.Visible = true;
                        lblNoInterestRecommendations.Visible = false;
                        return;
                    }
                }

                // If no recommendations or empty result
                pnlInterestRecommendations.Visible = true;
                lblNoInterestRecommendations.Visible = true;
                rptInterestRecommendations.Visible = false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in LoadInterestBasedRecommendations: {ex.Message}");
                pnlInterestRecommendations.Visible = false;
            }
        }

        private async Task LoadPopularCourseRecommendations(List<Course> availableCourses)
        {
            try
            {
                // Get all users to analyze enrollment patterns
                var allUsers = await firebaseHelper.GetUsers();

                // Calculate course popularity by counting enrollments
                var coursePopularity = new Dictionary<string, int>();

                foreach (var user in allUsers)
                {
                    if (user.MemberData?.CourseID_List != null)
                    {
                        foreach (var courseId in user.MemberData.CourseID_List)
                        {
                            if (coursePopularity.ContainsKey(courseId))
                            {
                                coursePopularity[courseId]++;
                            }
                            else
                            {
                                coursePopularity[courseId] = 1;
                            }
                        }
                    }

                    // Also include completed courses in popularity
                    if (user.MemberData?.CompletedCourseID_List != null)
                    {
                        foreach (var courseId in user.MemberData.CompletedCourseID_List)
                        {
                            if (coursePopularity.ContainsKey(courseId))
                            {
                                coursePopularity[courseId]++;
                            }
                            else
                            {
                                coursePopularity[courseId] = 1;
                            }
                        }
                    }
                }

                // Get popular courses that are available to the user
                var popularCourses = availableCourses
                    .Where(c => coursePopularity.ContainsKey(c.CourseID))
                    .Select(c => new {
                        Course = c,
                        Popularity = coursePopularity.ContainsKey(c.CourseID) ? coursePopularity[c.CourseID] : 0,
                        EnrollmentRate = CalculateEnrollmentRate(c.NumberOfStudents, coursePopularity.ContainsKey(c.CourseID) ? coursePopularity[c.CourseID] : 0)
                    })
                    .OrderByDescending(item => item.Popularity)
                    .Take(5)
                    .ToList();

                if (popularCourses.Any())
                {
                    rptPopularCourses.DataSource = popularCourses.Select(item => new {
                        item.Course.CourseID,
                        item.Course.CourseName,
                        item.Course.CourseCategory,
                        item.Course.NumberOfStudents,
                        EnrollmentCount = item.Popularity,
                        EnrollmentRate = item.EnrollmentRate + "% enrollment rate"
                    });
                    rptPopularCourses.DataBind();
                    pnlPopularCourses.Visible = true;
                    lblNoPopularCourses.Visible = false;
                }
                else
                {
                    pnlPopularCourses.Visible = true;
                    lblNoPopularCourses.Visible = true;
                    rptPopularCourses.Visible = false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading popular courses: {ex.Message}");
                pnlPopularCourses.Visible = false;
            }
        }

        private string CalculateEnrollmentRate(int maxStudents, int currentEnrollments)
        {
            if (maxStudents <= 0) return "0";

            double rate = (double)currentEnrollments / maxStudents * 100;
            return rate.ToString("0");
        }

        private async Task LoadSimilarToCompletedRecommendations(List<Course> availableCourses, List<string> completedCourseIds)
        {
            if (!completedCourseIds.Any())
            {
                pnlSimilarCourses.Visible = false;
                return;
            }

            try
            {
                // Get details of completed courses
                var completedCourses = new List<Course>();
                foreach (var courseId in completedCourseIds)
                {
                    var course = await firebaseHelper.GetCourseById(courseId);
                    if (course != null)
                    {
                        completedCourses.Add(course);
                    }
                }

                if (!completedCourses.Any())
                {
                    pnlSimilarCourses.Visible = false;
                    return;
                }

                // Find similar courses based on categories of completed courses
                var completedCategories = completedCourses
                    .Select(c => c.CourseCategory)
                    .Where(c => !string.IsNullOrEmpty(c))
                    .Distinct()
                    .ToList();

                var similarCourses = availableCourses
                    .Where(c => completedCategories.Contains(c.CourseCategory))
                    .Select(course => new {
                        Course = course,
                        CompletedCourse = completedCourses.FirstOrDefault(c => c.CourseCategory == course.CourseCategory)?.CourseName
                    })
                    .Take(5)
                    .ToList();

                if (similarCourses.Any())
                {
                    rptSimilarCourses.DataSource = similarCourses.Select(item => new {
                        item.Course.CourseID,
                        item.Course.CourseName,
                        item.Course.CourseCategory,
                        item.Course.NumberOfStudents,
                        RecommendationReason = $"Similar to {item.CompletedCourse} that you completed"
                    });
                    rptSimilarCourses.DataBind();
                    pnlSimilarCourses.Visible = true;
                    lblNoSimilarCourses.Visible = false;
                }
                else
                {
                    pnlSimilarCourses.Visible = true;
                    lblNoSimilarCourses.Visible = true;
                    rptSimilarCourses.Visible = false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading similar courses: {ex.Message}");
                pnlSimilarCourses.Visible = false;
            }
        }

        private void ShowError(string message)
        {
            lblMessage.Text = message;
            lblMessage.ForeColor = System.Drawing.Color.Red;
            lblMessage.Visible = true;

            // Hide all recommendation panels
            pnlInterestRecommendations.Visible = false;
            pnlPopularCourses.Visible = false;
            pnlSimilarCourses.Visible = false;
        }

        private void ShowNoRecommendations(string message)
        {
            lblNoRecommendations.Text = message;
            lblNoRecommendations.Visible = true;

            // Hide all recommendation panels
            pnlInterestRecommendations.Visible = false;
            pnlPopularCourses.Visible = false;
            pnlSimilarCourses.Visible = false;

            lblMessage.Visible = false;
        }

        // Add the missing event handlers that are referenced in the ASPX file
        protected void btnViewDetails_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            string courseId = btn.CommandArgument;

            if (!string.IsNullOrEmpty(courseId))
            {
                // Store the current page as referrer
                Session["CourseDetailsReferrer"] = Request.Url.AbsolutePath;
                Response.Redirect($"OC_CourseDetails.aspx?id={courseId}");
            }
        }

        protected async void btnJoin_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            string courseId = btn.CommandArgument;

            if (string.IsNullOrEmpty(courseId))
            {
                ShowError("Error: Course ID is missing.");
                return;
            }

            try
            {
                // Reload the current user to get the latest data
                currentUser = await firebaseHelper.GetUserById(currentUserID);

                if (currentUser == null)
                {
                    ShowError("Error: Could not load user data.");
                    return;
                }

                // Check if already enrolled
                if (currentUser.MemberData?.CourseID_List != null &&
                    currentUser.MemberData.CourseID_List.Contains(courseId))
                {
                    lblMessage.Text = "You are already enrolled in this course.";
                    lblMessage.ForeColor = System.Drawing.Color.Blue;
                    lblMessage.Visible = true;
                    return;
                }

                // Use the dedicated method to add course to user
                await firebaseHelper.AddCourseToUserList(currentUserID, courseId);

                // Show success message
                lblMessage.Text = "You have successfully joined the course!";
                lblMessage.ForeColor = System.Drawing.Color.Green;
                lblMessage.Visible = true;

                // Reload recommendations
                await LoadRecommendations();
            }
            catch (Exception ex)
            {
                ShowError($"Error joining course: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Join course error: {ex.Message}");
            }
        }
    }
}