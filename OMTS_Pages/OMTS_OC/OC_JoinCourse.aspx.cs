using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace fyp
{
    public partial class OC_JoinCourse : System.Web.UI.Page
    {
        private FirebaseHelper firebaseHelper = new FirebaseHelper();
        private string currentUserID;
        private User currentUser;
        private List<Course> availableCourses;

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
                // Get the current user's role from the session
                string userRole = Session["UserRole"]?.ToString();

                // Set visibility for each button based on the user's role
                btnMyCourse.Visible = userRole == "Member" || userRole == "Admin";
                btnJoinCourse.Visible = userRole == "Member" || userRole == "Admin";
                btnCompletedCourse.Visible = userRole == "Member" || userRole == "Admin";
                btnRecommendations.Visible = userRole == "Member" || userRole == "Admin";
                btnCourseManagement.Visible = userRole == "Staff" || userRole == "Admin";

                try
                {
                    // Load the current user
                    currentUser = await firebaseHelper.GetUserById(currentUserID);
                    await LoadAvailableCourses();
                }
                catch (Exception ex)
                {
                    lblMessage.Text = "Error loading courses: " + ex.Message;
                    lblMessage.ForeColor = System.Drawing.Color.Red;
                    lblMessage.Visible = true;
                }
            }
        }

        private async Task LoadAvailableCourses(string searchText = "")
        {
            try
            {
                // Create a loading indicator for users
                lblMessage.Text = "Loading courses...";
                lblMessage.ForeColor = System.Drawing.Color.Blue;
                lblMessage.Visible = true;

                // Start with cached courses if available and this is just a filter operation
                List<Course> coursesToProcess;

                if (string.IsNullOrEmpty(searchText) || availableCourses == null)
                {
                    // Only load all courses from database when necessary
                    coursesToProcess = await firebaseHelper.GetActiveCourses();

                    // Cache the results for future searches
                    if (string.IsNullOrEmpty(searchText))
                        availableCourses = coursesToProcess;
                }
                else
                {
                    // Use the cached courses for filtering
                    coursesToProcess = availableCourses;
                }

                // Apply search filter if provided - doing this client-side is much faster
                List<Course> filteredCourses = coursesToProcess;
                if (!string.IsNullOrEmpty(searchText))
                {
                    string searchLower = searchText.ToLower();
                    filteredCourses = coursesToProcess.Where(c =>
                        (c.CourseName?.ToLower()?.Contains(searchLower) ?? false) ||
                        (c.CourseDescription?.ToLower()?.Contains(searchLower) ?? false) ||
                        (c.CourseCategory?.ToLower()?.Contains(searchLower) ?? false)
                    ).ToList();
                }

                // Load the current user if not already loaded (only once)
                if (currentUser == null)
                {
                    currentUser = await firebaseHelper.GetUserById(currentUserID);
                }

                // Get user's courses efficiently
                var enrolledCourses = currentUser?.MemberData?.CourseID_List ?? new List<string>();
                var completedCourses = currentUser?.MemberData?.CompletedCourseID_List ?? new List<string>();

                // Filter courses the user hasn't enrolled in or completed
                var availCourses = filteredCourses.Where(c =>
                    !enrolledCourses.Contains(c.CourseID) &&
                    !completedCourses.Contains(c.CourseID)
                ).ToList();

                if (!availCourses.Any())
                {
                    // No courses available
                    rptAvailableCourses.DataSource = null;
                    rptAvailableCourses.DataBind();
                    lblNoCourses.Visible = true;

                    // No recommendations either
                    rptRecommendedCourses.DataSource = null;
                    rptRecommendedCourses.DataBind();
                    lblNoRecommendations.Visible = true;
                    lblMessage.Visible = false;
                    return;
                }

                // Get recommended courses using Gemini API
                var recommendedCourses = await GetGeminiRecommendations(availCourses);

                // Display recommended courses
                if (recommendedCourses.Any())
                {
                    rptRecommendedCourses.DataSource = recommendedCourses;
                    rptRecommendedCourses.DataBind();
                    lblNoRecommendations.Visible = false;

                    // Filter out recommended courses from the main list to avoid duplication
                    var recommendedIds = recommendedCourses.Select(c => c.CourseID).ToList();
                    var remainingCourses = availCourses.Where(c => !recommendedIds.Contains(c.CourseID)).ToList();

                    rptAvailableCourses.DataSource = remainingCourses;
                    rptAvailableCourses.DataBind();
                    lblNoCourses.Visible = !remainingCourses.Any();
                }
                else
                {
                    // No recommendations, show all courses
                    rptRecommendedCourses.DataSource = null;
                    rptRecommendedCourses.DataBind();
                    lblNoRecommendations.Visible = true;

                    rptAvailableCourses.DataSource = availCourses;
                    rptAvailableCourses.DataBind();
                    lblNoCourses.Visible = false;
                }

                lblMessage.Visible = false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading available courses: {ex.Message}");
                lblMessage.Text = "Error loading courses: " + ex.Message;
                lblMessage.ForeColor = System.Drawing.Color.Red;
                lblMessage.Visible = true;
                throw; // Re-throw to be handled by the caller
            }
        }

        private async Task<List<dynamic>> GetGeminiRecommendations(List<Course> availableCourses)
        {
            try
            {
                // Check if we have user and courses data
                if (currentUser == null || availableCourses == null || !availableCourses.Any())
                {
                    return new List<dynamic>();
                }

                // Get completed courses for better recommendations
                List<Course> completedCourses = new List<Course>();

                if (currentUser.MemberData?.CompletedCourseID_List != null &&
                    currentUser.MemberData.CompletedCourseID_List.Any())
                {
                    foreach (var courseId in currentUser.MemberData.CompletedCourseID_List)
                    {
                        var course = await firebaseHelper.GetCourseById(courseId);
                        if (course != null)
                        {
                            completedCourses.Add(course);
                        }
                    }
                }

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

                // Build the list of recommended courses with reasons
                var recommendedCourses = new List<dynamic>();

                foreach (var recommendation in recommendations)
                {
                    string courseId = recommendation.Key;
                    string reason = recommendation.Value;

                    // Find the course
                    var course = availableCourses.FirstOrDefault(c => c.CourseID == courseId);
                    if (course != null)
                    {
                        recommendedCourses.Add(new
                        {
                            CourseID = course.CourseID,
                            CourseName = course.CourseName,
                            CourseCategory = course.CourseCategory ?? "Uncategorized",
                            NumberOfStudents = course.NumberOfStudents,
                            MatchReason = reason
                        });
                    }
                }

                return recommendedCourses;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting Gemini recommendations: {ex.Message}");

                // Fallback: get recommendations using the old method if all else fails
                return GetFallbackRecommendations(availableCourses);
            }
        }

        // Legacy method as final fallback
        private List<dynamic> GetFallbackRecommendations(List<Course> courses)
        {
            try
            {
                // Get user interests
                var userInterests = currentUser?.MemberData?.Interest_List ?? new List<string>();

                // If user has no interests or courses, return empty list
                if (!userInterests.Any() || !courses.Any())
                {
                    return new List<dynamic>();
                }

                // Simple scoring algorithm based on keyword matching
                var scoredCourses = courses.Select(course =>
                {
                    int score = 0;

                    // Score based on category match
                    if (userInterests.Contains(course.CourseCategory))
                    {
                        score += 10;
                    }

                    // Score based on keywords in title and description
                    foreach (var interest in userInterests)
                    {
                        if (course.CourseName != null && course.CourseName.ToLower().Contains(interest.ToLower()))
                        {
                            score += 5;
                        }

                        if (course.CourseDescription != null &&
                            course.CourseDescription.ToLower().Contains(interest.ToLower()))
                        {
                            score += 3;
                        }
                    }

                    return new { Course = course, Score = score, Reason = GetMatchReason(course, userInterests) };
                })
                .Where(item => item.Score > 0) // Only include courses with a positive score
                .OrderByDescending(item => item.Score)
                .Take(3);

                // Convert to dynamic list with reason
                var recommendations = new List<dynamic>();
                foreach (var item in scoredCourses)
                {
                    recommendations.Add(new
                    {
                        CourseID = item.Course.CourseID,
                        CourseName = item.Course.CourseName,
                        CourseCategory = item.Course.CourseCategory ?? "Uncategorized",
                        NumberOfStudents = item.Course.NumberOfStudents,
                        MatchReason = item.Reason
                    });
                }

                return recommendations;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting fallback recommendations: {ex.Message}");
                return new List<dynamic>();
            }
        }

        private string GetMatchReason(Course course, List<string> userInterests)
        {
            // Generate a reason based on why this course matched
            if (userInterests.Contains(course.CourseCategory))
            {
                return $"Matches your interest in {course.CourseCategory}";
            }

            // Check for interests in title or description
            foreach (var interest in userInterests)
            {
                if (course.CourseName != null && course.CourseName.ToLower().Contains(interest.ToLower()))
                {
                    return $"Contains topics related to your interest in {interest}";
                }
            }

            return "This course might be interesting for you";
        }

        protected void rptAvailableCourses_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                try
                {
                    var courseItem = (Course)e.Item.DataItem;
                    var btnJoin = (Button)e.Item.FindControl("btnJoin");
                    var lblParticipants = (Label)e.Item.FindControl("lblParticipants");

                    if (btnJoin != null)
                    {
                        // Check if the user has already joined this course
                        bool hasJoined = currentUser?.MemberData?.CourseID_List?.Contains(courseItem.CourseID) ?? false;

                        // Update the join button visibility
                        btnJoin.Visible = !hasJoined;
                    }

                    // Update participants count
                    if (lblParticipants != null)
                    {
                        int participantCount = GetCourseParticipantCount(courseItem.CourseID);
                        lblParticipants.Text = $"Students: {participantCount}/{courseItem.NumberOfStudents}";
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error in ItemDataBound: {ex.Message}");
                    // Just continue, don't let this break the page
                }
            }
        }

        protected void rptRecommendedCourses_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                try
                {
                    // Since we're using dynamic objects, we need to use reflection to get the CourseID
                    var courseItem = e.Item.DataItem;
                    var courseIdProperty = courseItem.GetType().GetProperty("CourseID");
                    string courseId = courseIdProperty?.GetValue(courseItem)?.ToString();

                    var btnJoin = (Button)e.Item.FindControl("btnJoin");
                    var lblParticipants = (Label)e.Item.FindControl("lblParticipants");

                    if (btnJoin != null && !string.IsNullOrEmpty(courseId))
                    {
                        // Check if the user has already joined this course
                        bool hasJoined = currentUser?.MemberData?.CourseID_List?.Contains(courseId) ?? false;

                        // Update the join button visibility
                        btnJoin.Visible = !hasJoined;
                    }

                    // Update participants count
                    if (lblParticipants != null && !string.IsNullOrEmpty(courseId))
                    {
                        int participantCount = GetCourseParticipantCount(courseId);

                        // Get number of students property dynamically
                        var numberOfStudentsProperty = courseItem.GetType().GetProperty("NumberOfStudents");
                        var numberOfStudents = numberOfStudentsProperty?.GetValue(courseItem)?.ToString() ?? "0";

                        lblParticipants.Text = $"Students: {participantCount}/{numberOfStudents}";
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error in recommended courses ItemDataBound: {ex.Message}");
                }
            }
        }

        private int GetCourseParticipantCount(string courseId)
        {
            // This is a simplified implementation
            // In a real implementation, you would query your database to get the actual count
            return 0; // Return 0 as a default
        }

        protected async void btnSearch_Click(object sender, EventArgs e)
        {
            try
            {
                // Show a loading indicator or disable the button during search
                btnSearch.Enabled = false;
                lblMessage.Text = "Searching...";
                lblMessage.ForeColor = System.Drawing.Color.Blue;
                lblMessage.Visible = true;

                string searchText = txtSearch.Text.Trim();

                // Use the async version directly, don't block with GetAwaiter().GetResult()
                await LoadAvailableCourses(searchText);

                // Clear the searching message when done
                lblMessage.Visible = false;
            }
            catch (Exception ex)
            {
                lblMessage.Text = "Error searching courses: " + ex.Message;
                lblMessage.ForeColor = System.Drawing.Color.Red;
                lblMessage.Visible = true;
                System.Diagnostics.Debug.WriteLine($"Search error: {ex.Message}");
            }
            finally
            {
                // Re-enable the search button
                btnSearch.Enabled = true;
            }
        }

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
        private async Task LoadFeaturedCourses(List<Course> availableCourses)
        {
            try
            {
                // Get general recommendations without using user data
                var generalRecommendations = await firebaseHelper.GetGeneralCourseRecommendations(availableCourses, 3);

                if (generalRecommendations.Any())
                {
                    // Create a list for the repeater
                    var featuredCourses = new List<dynamic>();

                    foreach (var entry in generalRecommendations)
                    {
                        string courseId = entry.Key;
                        string reason = entry.Value;

                        // Find the course in available courses
                        var course = availableCourses.FirstOrDefault(c => c.CourseID == courseId);
                        if (course != null)
                        {
                            featuredCourses.Add(new
                            {
                                CourseID = course.CourseID,
                                CourseName = course.CourseName,
                                CourseCategory = course.CourseCategory ?? "Uncategorized",
                                NumberOfStudents = course.NumberOfStudents,
                                FeatureReason = reason
                            });
                        }
                    }

                    // Bind data to your repeater for featured courses
                    // rptFeaturedCourses.DataSource = featuredCourses;
                    // rptFeaturedCourses.DataBind();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading featured courses: {ex.Message}");
            }
        }
        protected async void btnJoin_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            string courseId = btn.CommandArgument;

            if (string.IsNullOrEmpty(courseId))
            {
                lblMessage.Text = "Error: Course ID is missing.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                lblMessage.Visible = true;
                return;
            }

            try
            {
                // Reload the current user to get the latest data
                currentUser = await firebaseHelper.GetUserById(currentUserID);

                if (currentUser == null)
                {
                    lblMessage.Text = "Error: Could not load user data.";
                    lblMessage.ForeColor = System.Drawing.Color.Red;
                    lblMessage.Visible = true;
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

                // Reload the courses to update participant counts
                await LoadAvailableCourses();
            }
            catch (Exception ex)
            {
                lblMessage.Text = $"Error joining course: {ex.Message}";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                lblMessage.Visible = true;
                System.Diagnostics.Debug.WriteLine($"Join course error: {ex.Message}");
            }
        }
    }
}