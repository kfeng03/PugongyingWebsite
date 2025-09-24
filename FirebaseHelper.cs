using Firebase.Auth;
using FirebaseAdmin.Auth;
using Firebase.Database;
using Firebase.Database.Query;
using Firebase.Storage;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using System.Management.Instrumentation;
using static fyp.MA_AchievementDashboard;
using System.Drawing;
using System.Net.Http;

namespace fyp
{
    public class FirebaseHelper
    {
        private readonly FirebaseClient _firebase;
        private readonly FirebaseStorage _firebaseStorage;
        private readonly FirebaseAuthProvider _authProvider;
        private readonly FirebaseAdmin.Auth.FirebaseAuth _firebaseAuth;

        public FirebaseHelper()
        {
            _firebase = new FirebaseClient("https://pgy-omts-default-rtdb.asia-southeast1.firebasedatabase.app");
            _authProvider = new FirebaseAuthProvider(new FirebaseConfig("AIzaSyD7OIF-dnrsre_YaDVuWjw527whdmaSoi0"));

            // Initialize Firebase Admin SDK
            if (FirebaseApp.DefaultInstance == null)
            {
                FirebaseApp.Create(new AppOptions()
                {
                    Credential = GoogleCredential.FromFile(HttpContext.Current.Server.MapPath("~/App_Data/pgy-omts-firebase-adminsdk-fbsvc-3e8c3d5cc9.json"))
                });
            }

            // Get the FirebaseAuth instance
            _firebaseAuth = FirebaseAdmin.Auth.FirebaseAuth.GetAuth(FirebaseApp.DefaultInstance);
        }

        // --------------------- GEMINI API RECOMMENDATIONS ---------------------
        private static readonly HttpClient _httpClient = new HttpClient();
        private const string GeminiApiKey = "AIzaSyCRxx_Bnp_KrVbl3Y54TKitULH4qBEnMvE"; 
        private const string GeminiApiUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-pro:generateContent";

        /// <summary>
        /// Get course recommendations from Gemini API
        /// </summary>
        public async Task<Dictionary<string, string>> GetGeminiCourseRecommendations(
            User user,
            List<Course> availableCourses,
            List<Course> completedCourses = null)
        {
            try
            {
                // Initialize HTTP client if needed
                if (_httpClient.DefaultRequestHeaders.Accept.Count == 0)
                {
                    _httpClient.DefaultRequestHeaders.Accept.Clear();
                    _httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                }

                // Prepare the prompt for Gemini
                string prompt = BuildRecommendationPrompt(user, availableCourses, completedCourses);

                // Call Gemini API
                string response = await CallGeminiAPI(prompt);

                // Parse the response
                return ParseGeminiResponse(response, availableCourses);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Gemini API error: {ex.Message}");
                return new Dictionary<string, string>();
            }
        }

        private string BuildRecommendationPrompt(User user, List<Course> availableCourses, List<Course> completedCourses)
        {
            var userInterests = user?.MemberData?.Interest_List ?? new List<string>();

            // Build JSON data for courses
            var coursesJson = new System.Text.StringBuilder();
            foreach (var course in availableCourses)
            {
                coursesJson.AppendLine($"{{");
                coursesJson.AppendLine($"  \"id\": \"{course.CourseID}\",");
                coursesJson.AppendLine($"  \"name\": \"{EscapeJsonString(course.CourseName)}\",");
                coursesJson.AppendLine($"  \"category\": \"{EscapeJsonString(course.CourseCategory)}\",");
                coursesJson.AppendLine($"  \"description\": \"{EscapeJsonString(course.CourseDescription)}\"");
                coursesJson.AppendLine($"}},");
            }

            // Build JSON for completed courses if available
            var completedCoursesJson = new System.Text.StringBuilder();
            if (completedCourses != null && completedCourses.Count > 0)
            {
                foreach (var course in completedCourses)
                {
                    completedCoursesJson.AppendLine($"{{");
                    completedCoursesJson.AppendLine($"  \"id\": \"{course.CourseID}\",");
                    completedCoursesJson.AppendLine($"  \"name\": \"{EscapeJsonString(course.CourseName)}\",");
                    completedCoursesJson.AppendLine($"  \"category\": \"{EscapeJsonString(course.CourseCategory)}\"");
                    completedCoursesJson.AppendLine($"}},");
                }
            }

            // Build the complete prompt
            System.Text.StringBuilder promptBuilder = new System.Text.StringBuilder();
            promptBuilder.AppendLine("You are a course recommendation system. Given the following information:");
            promptBuilder.AppendLine("");
            promptBuilder.AppendLine("User Interests:");
            if (userInterests.Count > 0)
            {
                foreach (var interest in userInterests)
                {
                    promptBuilder.AppendLine($"- {interest}");
                }
            }
            else
            {
                promptBuilder.AppendLine("- No specific interests provided");
            }

            promptBuilder.AppendLine("");
            promptBuilder.AppendLine("Available Courses:");
            promptBuilder.AppendLine("[");
            promptBuilder.Append(coursesJson.ToString().TrimEnd(','));
            promptBuilder.AppendLine("]");

            if (completedCourses != null && completedCourses.Count > 0)
            {
                promptBuilder.AppendLine("");
                promptBuilder.AppendLine("Completed Courses:");
                promptBuilder.AppendLine("[");
                promptBuilder.Append(completedCoursesJson.ToString().TrimEnd(','));
                promptBuilder.AppendLine("]");
            }

            promptBuilder.AppendLine("");
            promptBuilder.AppendLine("Please recommend up to 5 courses from the available courses list that would be most relevant to this user based on their interests and completed courses (if any). For each recommended course, provide a brief explanation why it's a good match.");
            promptBuilder.AppendLine("");
            promptBuilder.AppendLine("Format your response as a valid JSON array of objects with 'courseId' and 'reason' for each recommendation:");
            promptBuilder.AppendLine("[");
            promptBuilder.AppendLine("  {\"courseId\": \"course-id-1\", \"reason\": \"This course matches your interest in...\"},");
            promptBuilder.AppendLine("  {\"courseId\": \"course-id-2\", \"reason\": \"Based on your completion of...\"}");
            promptBuilder.AppendLine("]");

            return promptBuilder.ToString();
        }

        private string EscapeJsonString(string text)
        {
            if (string.IsNullOrEmpty(text))
                return "";

            return text.Replace("\\", "\\\\")
                      .Replace("\"", "\\\"")
                      .Replace("\n", "\\n")
                      .Replace("\r", "\\r")
                      .Replace("\t", "\\t");
        }

        private async Task<string> CallGeminiAPI(string prompt)
        {
            try
            {
                // Create request body
                var requestBody = new
                {
                    contents = new[]
                    {
                new
                {
                    parts = new[]
                    {
                        new { text = prompt }
                    }
                }
            },
                    generationConfig = new
                    {
                        temperature = 0.2,
                        maxOutputTokens = 1024
                    }
                };

                string jsonRequest = Newtonsoft.Json.JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(jsonRequest, System.Text.Encoding.UTF8, "application/json");

                // Make the API call with API key as query parameter
                string url = $"{GeminiApiUrl}?key={GeminiApiKey}";
                var response = await _httpClient.PostAsync(url, content);
                response.EnsureSuccessStatusCode();

                string jsonResponse = await response.Content.ReadAsStringAsync();
                return jsonResponse;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Gemini API call error: {ex.Message}");
                throw;
            }
        }

        private Dictionary<string, string> ParseGeminiResponse(string jsonResponse, List<Course> availableCourses)
        {
            var recommendations = new Dictionary<string, string>();

            try
            {
                // Parse the Gemini response
                JObject responseObj = JObject.Parse(jsonResponse);

                // Extract the text response
                string textResponse = responseObj["candidates"][0]["content"]["parts"][0]["text"].ToString();

                // The response should contain a JSON array - extract it
                int jsonStartIndex = textResponse.IndexOf('[');
                int jsonEndIndex = textResponse.LastIndexOf(']');

                if (jsonStartIndex >= 0 && jsonEndIndex > jsonStartIndex)
                {
                    string jsonContent = textResponse.Substring(jsonStartIndex, jsonEndIndex - jsonStartIndex + 1);
                    JArray recommendationsArray = JArray.Parse(jsonContent);

                    // Process each recommendation
                    foreach (JObject item in recommendationsArray)
                    {
                        string courseId = item["courseId"].ToString();
                        string reason = item["reason"].ToString();

                        // Verify this is a valid course ID
                        if (availableCourses.Exists(c => c.CourseID == courseId))
                        {
                            recommendations[courseId] = reason;
                        }
                    }
                }

                return recommendations;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error parsing Gemini response: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Response: {jsonResponse}");
                return recommendations; // Return empty dictionary on error
            }
        }
        public async Task<Dictionary<string, string>> GetGeneralCourseRecommendations(List<Course> availableCourses, int count = 5)
        {
            try
            {
                // Initialize HTTP client if needed
                if (_httpClient.DefaultRequestHeaders.Accept.Count == 0)
                {
                    _httpClient.DefaultRequestHeaders.Accept.Clear();
                    _httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                }

                // Build a prompt that doesn't include user information
                string prompt = BuildGeneralRecommendationPrompt(availableCourses, count);

                // Call Gemini API
                string response = await CallGeminiAPI(prompt);

                // Parse the response
                return ParseGeminiResponse(response, availableCourses);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Gemini API error for general recommendations: {ex.Message}");
                return GetLocalGeneralRecommendations(availableCourses, count);
            }
        }

        private string BuildGeneralRecommendationPrompt(List<Course> availableCourses, int count)
        {
            // Build JSON data for courses
            var coursesJson = new System.Text.StringBuilder();
            foreach (var course in availableCourses)
            {
                coursesJson.AppendLine($"{{");
                coursesJson.AppendLine($"  \"id\": \"{course.CourseID}\",");
                coursesJson.AppendLine($"  \"name\": \"{EscapeJsonString(course.CourseName)}\",");
                coursesJson.AppendLine($"  \"category\": \"{EscapeJsonString(course.CourseCategory)}\",");
                coursesJson.AppendLine($"  \"description\": \"{EscapeJsonString(course.CourseDescription)}\"");
                coursesJson.AppendLine($"}},");
            }

            // Build the complete prompt
            System.Text.StringBuilder promptBuilder = new System.Text.StringBuilder();
            promptBuilder.AppendLine("You are a course recommendation system. You need to recommend courses that would be generally appealing to a wide audience.");
            promptBuilder.AppendLine("");
            promptBuilder.AppendLine("Available Courses:");
            promptBuilder.AppendLine("[");
            promptBuilder.Append(coursesJson.ToString().TrimEnd(','));
            promptBuilder.AppendLine("]");

            promptBuilder.AppendLine("");
            promptBuilder.AppendLine($"Please recommend {count} courses from the available courses list that would be most appealing to a general audience. Consider factors like subject popularity, practical value, and wide applicability.");
            promptBuilder.AppendLine("");
            promptBuilder.AppendLine("For each recommended course, provide a brief explanation on why this course would be valuable for most people.");
            promptBuilder.AppendLine("");
            promptBuilder.AppendLine("Format your response as a valid JSON array of objects with 'courseId' and 'reason' for each recommendation:");
            promptBuilder.AppendLine("[");
            promptBuilder.AppendLine("  {\"courseId\": \"course-id-1\", \"reason\": \"This course teaches essential skills that...\"},");
            promptBuilder.AppendLine("  {\"courseId\": \"course-id-2\", \"reason\": \"A popular subject that many people...\"}");
            promptBuilder.AppendLine("]");

            return promptBuilder.ToString();
        }

        public Dictionary<string, string> GetLocalGeneralRecommendations(List<Course> availableCourses, int count = 5)
        {
            var recommendations = new Dictionary<string, string>();

            // Popular categories that tend to be generally interesting
            var popularCategories = new List<string> {
        "Programming", "Web Development", "Data Science",
        "Business", "Leadership", "Technology",
        "Design", "Marketing", "Communication"
    };

            // First get courses from popular categories
            var categorizedCourses = availableCourses
                .Where(c => popularCategories.Contains(c.CourseCategory))
                .Take(count)
                .ToList();

            foreach (var course in categorizedCourses)
            {
                recommendations[course.CourseID] = $"This {course.CourseCategory} course covers topics that are in high demand.";

                if (recommendations.Count >= count)
                    break;
            }

            // If we still need more recommendations, add other courses
            if (recommendations.Count < count)
            {
                var remainingCourses = availableCourses
                    .Where(c => !recommendations.ContainsKey(c.CourseID))
                    .Take(count - recommendations.Count);

                foreach (var course in remainingCourses)
                {
                    recommendations[course.CourseID] = "This course covers valuable skills that can benefit many learners.";
                }
            }

            return recommendations;
        }
        /// <summary>
        /// Get local recommendations without calling the Gemini API (fallback method)
        /// </summary>
        public Dictionary<string, string> GetLocalCourseRecommendations(User user, List<Course> availableCourses)
        {
            var recommendations = new Dictionary<string, string>();
            var userInterests = user?.MemberData?.Interest_List ?? new List<string>();

            // Simple algorithm for when API is not available
            // 1. First try to match based on interests
            var interestBasedCourses = new List<Course>();

            foreach (var course in availableCourses)
            {
                if (userInterests.Contains(course.CourseCategory))
                {
                    recommendations[course.CourseID] = $"This course matches your interest in {course.CourseCategory}.";
                    interestBasedCourses.Add(course);

                    // Limit to 3 interest-based recommendations
                    if (recommendations.Count >= 3)
                        break;
                }
            }

            // 2. If we don't have enough matches, add more based on a simple scoring
            if (recommendations.Count < 3)
            {
                var remainingCourses = availableCourses.Except(interestBasedCourses).ToList();

                var scoredCourses = remainingCourses.Select(course => {
                    int score = 0;

                    // Score based on keywords in title matching interests
                    foreach (var interest in userInterests)
                    {
                        if (course.CourseName?.ToLower().Contains(interest.ToLower()) == true)
                        {
                            score += 5;
                        }

                        if (course.CourseDescription?.ToLower().Contains(interest.ToLower()) == true)
                        {
                            score += 3;
                        }
                    }

                    return new { Course = course, Score = score };
                })
                .OrderByDescending(item => item.Score)
                .Take(3 - recommendations.Count);

                foreach (var scoredCourse in scoredCourses)
                {
                    string reason = scoredCourse.Score > 0
                        ? "This course contains topics related to your interests."
                        : "This course might be interesting for you.";

                    recommendations[scoredCourse.Course.CourseID] = reason;
                }
            }

            return recommendations;
        }
        // --------------------- COURSE MANAGEMENT ---------------------
        public async Task<string> GenerateNextCourseID()
        {
            try
            {
                var courses = await _firebase.Child("courses").OnceAsync<Course>();

                int maxID = 0;
                foreach (var course in courses)
                {
                    string courseKey = course.Key;
                    if (courseKey.StartsWith("C") && int.TryParse(courseKey.Substring(1), out int numericID))
                    {
                        if (numericID > maxID)
                        {
                            maxID = numericID;
                        }
                    }
                }

                int nextID = maxID + 1;
                return "C" + nextID.ToString("D3"); // Format as C001, C002, etc.
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error generating course ID: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> UpdateUserInterests(string userId, List<string> interests)
        {
            try
            {
                // Get the current user first to ensure we have the latest data
                var user = await GetUserById(userId);
                if (user == null)
                {
                    return false;
                }

                // Initialize MemberData if it doesn't exist
                if (user.MemberData == null)
                {
                    user.MemberData = new MemberData();
                }

                // Update the interest list
                user.MemberData.Interest_List = interests;

                // Replace 'firebase' with your actual Firebase reference variable
                // For example, if your class uses '_firebaseClient', use that instead:
                await _firebase
                    .Child("users")
                    .Child(userId)
                    .Child("MemberData")
                    .PutAsync(user.MemberData);

                System.Diagnostics.Debug.WriteLine($"Updated interests for user {userId}: {string.Join(", ", interests)}");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating user interests: {ex.Message}");
                return false;
            }
        }

        // Get user interests
        public async Task<List<string>> GetUserInterests(string userId)
        {
            try
            {
                var user = await GetUserById(userId);
                if (user == null || user.MemberData == null || user.MemberData.Interest_List == null)
                {
                    return new List<string>();
                }

                return user.MemberData.Interest_List;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting user interests: {ex.Message}");
                return new List<string>();
            }
        }

        // Add a new course
        public async Task<bool> AddCourse(Course course)
        {
            try
            {
                // Generate a new course ID if not provided
                if (string.IsNullOrEmpty(course.CourseID))
                {
                    course.CourseID = await GenerateNextCourseID();
                }

                // Ensure CourseCategory is not null
                course.CourseCategory = course.CourseCategory ?? "Uncategorized";

                // Set default values if not specified
                course.IsDeleted = false;
                course.CreatedDate = DateTime.UtcNow.ToString("yyyy-MM-dd");

                // Save to Firebase
                await _firebase
                    .Child("courses")
                    .Child(course.CourseID)
                    .PutAsync(course);

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error adding course: {ex.Message}");
                return false;
            }
        }

        // Get all active courses
        public async Task<List<Course>> GetActiveCourses()
        {
            try
            {
                var coursesResult = await _firebase
                    .Child("courses")
                    .OnceAsync<Course>();

                return coursesResult
                    .Where(c => c.Object.IsDeleted == false)
                    .Select(c => new Course
                    {
                        CourseID = c.Key,
                        CourseName = c.Object.CourseName,
                        CourseDescription = c.Object.CourseDescription,
                        CourseCategory = c.Object.CourseCategory ?? "Uncategorized", // Explicitly handle CategoryResult
                        NumberOfStudents = c.Object.NumberOfStudents,
                        CoursePictureUrl = c.Object.CoursePictureUrl,
                        CreatedDate = c.Object.CreatedDate,
                        CourseMaterials = c.Object.CourseMaterials ?? new Dictionary<string, string>()
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error fetching active courses: {ex.Message}");
                return new List<Course>();
            }
        }

        // Get all courses (both active and deleted)
        public async Task<List<Course>> GetCourses()
        {
            try
            {
                var coursesResult = await _firebase
                    .Child("courses")
                    .OnceAsync<Course>();

                return coursesResult
                    .Select(c => new Course
                    {
                        CourseID = c.Key,
                        CourseName = c.Object.CourseName,
                        CourseDescription = c.Object.CourseDescription,
                        CourseCategory = c.Object.CourseCategory,
                        NumberOfStudents = c.Object.NumberOfStudents,
                        CoursePictureUrl = c.Object.CoursePictureUrl,
                        CreatedDate = c.Object.CreatedDate,
                        IsDeleted = c.Object.IsDeleted,
                        CourseMaterials = c.Object.CourseMaterials ?? new Dictionary<string, string>()
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error fetching all courses: {ex.Message}");
                return new List<Course>();
            }
        }

        // Get deleted courses
        public async Task<List<Course>> GetDeletedCourses()
        {
            try
            {
                var coursesResult = await _firebase
                    .Child("courses")
                    .OnceAsync<Course>();

                return coursesResult
                    .Where(c => c.Object.IsDeleted == true)
                    .Select(c => new Course
                    {
                        CourseID = c.Key,
                        CourseName = c.Object.CourseName,
                        CourseDescription = c.Object.CourseDescription,
                        NumberOfStudents = c.Object.NumberOfStudents,
                        CourseCategory = c.Object.CourseCategory,
                        CoursePictureUrl = c.Object.CoursePictureUrl,
                        CourseMaterials = c.Object.CourseMaterials ?? new Dictionary<string, string>()
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error fetching deleted courses: {ex.Message}");
                return new List<Course>();
            }
        }

        // Get a specific course by ID
        public async Task<Course> GetCourseById(string courseId)
        {
            try
            {
                var course = await _firebase
                    .Child("courses")
                    .Child(courseId)
                    .OnceSingleAsync<Course>();

                if (course != null)
                {
                    course.CourseID = courseId;
                }

                return course;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error fetching course by ID: {ex.Message}");
                return null;
            }
        }

        // Update an existing course
        public async Task UpdateCourse(Course course)
        {
            try
            {
                if (string.IsNullOrEmpty(course.CourseID))
                {
                    throw new ArgumentException("Course ID is required for update");
                }

                // Ensure CourseCategory is not null
                course.CourseCategory = course.CourseCategory ?? "Uncategorized";

                // Update only specific fields to prevent overwriting
                var updates = new Dictionary<string, object>
                {
                    { "CourseName", course.CourseName },
                    { "CourseDescription", course.CourseDescription },
                    { "CourseCategory", course.CourseCategory }, // Explicitly include CourseCategory
                    { "NumberOfStudents", course.NumberOfStudents },
                    { "CoursePictureUrl", course.CoursePictureUrl },
                    { "CourseMaterials", course.CourseMaterials }
                };

                await _firebase
                    .Child("courses")
                    .Child(course.CourseID)
                    .PatchAsync(updates);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating course: {ex.Message}");
                throw;
            }
        }

        // Soft delete a course
        public async Task SoftDeleteCourse(string courseId)
        {
            try
            {
                await _firebase
                    .Child("courses")
                    .Child(courseId)
                    .Child("IsDeleted")
                    .PutAsync(true);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error soft deleting course: {ex.Message}");
                throw;
            }
        }

        // Restore a deleted course
        public async Task RestoreCourse(string courseId)
        {
            try
            {
                await _firebase
                    .Child("courses")
                    .Child(courseId)
                    .Child("IsDeleted")
                    .PutAsync(false);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error restoring course: {ex.Message}");
                throw;
            }
        }

        // Upload course file (picture or materials)
        public async Task<string> UploadCourseFileToFirebase(HttpPostedFile file)
        {
            try
            {
                if (file == null || file.ContentLength == 0) return null;

                string fileName = Path.GetFileName(file.FileName);
                using (var stream = file.InputStream)
                {
                    var uploadTask = await _firebaseStorage
                        .Child("course_materials")
                        .Child(fileName)
                        .PutAsync(stream);

                    return uploadTask;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error uploading course file: {ex.Message}");
                throw;
            }
        }

        // General file upload method (from second file)
        public async Task<string> UploadFileToFirebase(HttpPostedFile file)
        {
            try
            {
                if (file == null || file.ContentLength == 0) return null;

                string fileName = Path.GetFileName(file.FileName);
                using (var stream = file.InputStream)
                {
                    var uploadTask = await _firebaseStorage
                        .Child("course_materials")
                        .Child(fileName)
                        .PutAsync(stream);

                    return uploadTask;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("File upload failed: " + ex.Message);
            }
        }

        public async Task UpdateUserCourseList(string userId, List<string> courseIdList)
        {
            try
            {
                // Create a dictionary targeting just the CourseID_List field
                var updates = new Dictionary<string, object>
                {
                    { "MemberData/CourseID_List", courseIdList }
                };

                // Update only the specified field
                await _firebase
                    .Child("users")
                    .Child(userId)
                    .PatchAsync(updates);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error updating user course list: " + ex.Message);
                throw; // Re-throw the exception to handle it in the calling method
            }
        }

        public async Task DeleteCourse(string courseId)
        {
            try
            {
                // Check if the course exists before attempting to delete
                var courses = await _firebase.Child("courses").OnceAsync<Course>();
                bool courseExists = courses.Any(c => c.Key == courseId);

                if (!courseExists)
                {
                    throw new Exception("Course not found with ID: " + courseId);
                }

                // Permanently delete the course
                await _firebase.Child("courses").Child(courseId).DeleteAsync();

                // Log success for debugging
                System.Diagnostics.Debug.WriteLine($"Successfully deleted course: {courseId}");
            }
            catch (Exception ex)
            {
                // Log the exact error for debugging
                System.Diagnostics.Debug.WriteLine($"Error in DeleteCourse method: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                throw; // Rethrow the exception to be handled by the caller
            }
        }

        public async Task AddCourseToUserList(string userId, string courseId)
        {
            try
            {
                // First get the current user
                var user = await GetUserById(userId);
                if (user == null)
                {
                    throw new Exception("User not found");
                }

                // Initialize MemberData if needed
                if (user.MemberData == null)
                {
                    user.MemberData = new MemberData();
                }

                // Initialize CourseID_List if needed
                if (user.MemberData.CourseID_List == null)
                {
                    user.MemberData.CourseID_List = new List<string>();
                }

                // Check if course is already in the list
                if (user.MemberData.CourseID_List.Contains(courseId))
                {
                    return; // Already joined
                }

                // Add the course to the list
                user.MemberData.CourseID_List.Add(courseId);

                // Update the user record in Firebase
                var updates = new Dictionary<string, object>
                {
                    { "MemberData/CourseID_List", user.MemberData.CourseID_List }
                };

                await _firebase
                    .Child("users")
                    .Child(userId)
                    .PatchAsync(updates);

                // Debug log
                System.Diagnostics.Debug.WriteLine($"Added course {courseId} to user {userId}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error adding course to user: {ex.Message}");
                throw; // Re-throw for handling in caller
            }
        }

        public bool IsValidGoogleDriveLink(string url)
        {
            if (string.IsNullOrEmpty(url))
                return false;

            Uri uriResult;
            bool isValidUrl = Uri.TryCreate(url, UriKind.Absolute, out uriResult) &&
                             (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

            if (!isValidUrl)
                return false;

            // Check if it's a Google Drive link
            return url.Contains("drive.google.com");
        }

        public string ConvertToEmbeddableGoogleDriveLink(string url)
        {
            if (string.IsNullOrEmpty(url))
                return url;

            if (!url.Contains("drive.google.com"))
                return url;

            // Convert to preview-only link if it's a Google Drive file
            if (url.Contains("/file/d/"))
            {
                string fileId = ExtractGoogleDriveFileId(url);
                if (!string.IsNullOrEmpty(fileId))
                {
                    // Convert to view format
                    return $"https://drive.google.com/file/d/{fileId}/view";
                }
            }

            return url;
        }

        private string ExtractGoogleDriveFileId(string url)
        {
            try
            {
                // Handle different Google Drive URL formats
                string fileId = "";

                if (url.Contains("/file/d/"))
                {
                    // Format: https://drive.google.com/file/d/FILE_ID/view
                    int startIndex = url.IndexOf("/file/d/") + 9;
                    int endIndex = url.IndexOf("/", startIndex);
                    if (endIndex == -1) endIndex = url.Length;
                    fileId = url.Substring(startIndex, endIndex - startIndex);
                }
                else if (url.Contains("id="))
                {
                    // Format: https://drive.google.com/open?id=FILE_ID
                    Uri uri = new Uri(url);
                    var query = HttpUtility.ParseQueryString(uri.Query);
                    fileId = query["id"];
                }

                return fileId;
            }
            catch
            {
                return "";
            }
        }

        public async Task UpdateUserCompletedCourses(string userId, Dictionary<string, object> updates)
        {
            try
            {
                // Update only the specified fields
                await _firebase
                    .Child("users")
                    .Child(userId)
                    .PatchAsync(updates);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error updating user completed courses: " + ex.Message);
                throw; // Re-throw the exception to handle it in the calling method
            }
        }

        public async Task<List<Course>> GetUserCompletedCourses(string userId)
        {
            try
            {
                // Get the user first to access their MemberData
                var user = await GetUserById(userId);
                if (user == null || user.MemberData == null)
                {
                    return new List<Course>();
                }

                // Check if they have a completed course list
                if (user.MemberData.CompletedCourseID_List == null ||
                    !user.MemberData.CompletedCourseID_List.Any())
                {
                    return new List<Course>();
                }

                // Get all active courses
                var allCourses = await GetActiveCourses();

                // Filter for courses the user has completed
                var completedCourses = allCourses
                    .Where(course => user.MemberData.CompletedCourseID_List.Contains(course.CourseID))
                    .ToList();

                return completedCourses;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error fetching completed courses: {ex.Message}");
                return new List<Course>();
            }
        }

        public async Task ToggleCourseCompletionStatus(string userId, string courseId, bool markAsComplete)
        {
            try
            {
                // Get the user
                var user = await GetUserById(userId);
                if (user == null)
                {
                    throw new Exception("User not found.");
                }

                // Initialize MemberData if needed
                if (user.MemberData == null)
                {
                    user.MemberData = new MemberData();
                }

                // Initialize CompletedCourseID_List if needed
                if (user.MemberData.CompletedCourseID_List == null)
                {
                    user.MemberData.CompletedCourseID_List = new List<string>();
                }

                // Update completion status
                if (markAsComplete && !user.MemberData.CompletedCourseID_List.Contains(courseId))
                {
                    // Add to completed courses
                    user.MemberData.CompletedCourseID_List.Add(courseId);
                }
                else if (!markAsComplete && user.MemberData.CompletedCourseID_List.Contains(courseId))
                {
                    // Remove from completed courses
                    user.MemberData.CompletedCourseID_List.Remove(courseId);
                }

                // Update in Firebase
                var updates = new Dictionary<string, object>
                {
                    { "MemberData/CompletedCourseID_List", user.MemberData.CompletedCourseID_List }
                };

                await UpdateUserCompletedCourses(userId, updates);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error toggling course completion status: {ex.Message}");
                throw; // Re-throw for handling in caller
            }
        }

        // --------------------- EVENT MANAGEMENT ---------------------
        public async Task<string> GenerateNextEventID()
        {
            var events = await _firebase.Child("events").OnceAsync<Event>();

            int maxID = 0;
            foreach (var evt in events)
            {
                string eventKey = evt.Key;
                if (eventKey.StartsWith("E") && int.TryParse(eventKey.Substring(1), out int numericID))
                {
                    if (numericID > maxID)
                    {
                        maxID = numericID;
                    }
                }
            }

            int nextID = maxID + 1;
            return "E" + nextID.ToString("D3");
        }

        public async Task<bool> AddEvent(Event newEvent)
        {
            try
            {
                string newEventID = await GenerateNextEventID();
                newEvent.EventID = newEventID;

                await _firebase.Child("events").Child(newEventID).PutAsync(newEvent);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error adding event: " + ex.Message);
                return false;
            }
        }

        public async Task<string> UploadEventFileToFirebase(HttpPostedFile file)
        {
            try
            {
                if (file == null || file.ContentLength == 0) return null;

                string fileName = Path.GetFileName(file.FileName);
                using (var stream = file.InputStream)
                {
                    var uploadTask = await _firebaseStorage
                        .Child("event_materials")
                        .Child(fileName)
                        .PutAsync(stream);

                    return uploadTask;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("File upload failed: " + ex.Message);
            }
        }

        public async Task<List<Event>> GetActiveEvents()
        {
            var allEvents = await GetEvents();

            // Update the status of each event based on date/time
            foreach (var evt in allEvents)
            {
                evt.UpdateEventStatus();
            }

            return allEvents.Where(e => !e.IsDeleted).ToList();
        }

        public async Task<List<Event>> GetUpcomingEvents()
        {
            var activeEvents = await GetActiveEvents();
            return activeEvents.Where(e => e.EventStatus == "Upcoming").ToList();
        }

        public async Task<List<Event>> GetCompletedEvents()
        {
            var activeEvents = await GetActiveEvents();
            return activeEvents.Where(e => e.EventStatus == "Completed").ToList();
        }

        public async Task<List<Event>> GetDeletedEvents()
        {
            var allEvents = await GetEvents();
            return allEvents.Where(e => e.IsDeleted).ToList();
        }

        public async Task<List<Event>> GetEvents()
        {
            try
            {
                var events = await _firebase
                    .Child("events")
                    .OnceAsync<Event>();

                return events.Select(e => new Event
                {
                    EventID = e.Key,
                    EventTitle = e.Object.EventTitle,
                    EventDescription = e.Object.EventDescription,
                    EventDate = e.Object.EventDate,
                    EventTime = e.Object.EventTime,
                    EventLocation = e.Object.EventLocation,
                    OrganizerName = e.Object.OrganizerName,
                    MaxParticipants = e.Object.MaxParticipants,
                    EventPictureUrl = e.Object.EventPictureUrl,
                    EventMaterials = e.Object.EventMaterials ?? new Dictionary<string, string>(),
                    IsDeleted = e.Object.IsDeleted,
                    EventStartDate = e.Object.EventStartDate,
                    EventEndDate = e.Object.EventEndDate,
                    RegistrationStartDate = e.Object.RegistrationStartDate
                }).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error fetching events: " + ex.Message);
                return new List<Event>();
            }
        }

        public async Task<Event> GetEventById(string eventId)
        {
            return await _firebase.Child("events").Child(eventId).OnceSingleAsync<Event>();
        }

        public async Task UpdateEvent(Event updateEvent)
        {
            await _firebase.Child("events").Child(updateEvent.EventID).PutAsync(updateEvent);
        }

        public async Task SoftDeleteEvent(string eventId)
        {
            await _firebase.Child("events").Child(eventId).PatchAsync(new { IsDeleted = true });
        }

        public async Task RestoreEvent(string eventId)
        {
            await _firebase.Child("events").Child(eventId).PatchAsync(new { IsDeleted = false });
        }

        public async Task DeleteEvent(string eventId)
        {
            try
            {
                // Check if the event exists before attempting to delete
                var events = await _firebase.Child("events").OnceAsync<Event>();
                bool eventExists = events.Any(e => e.Key == eventId);

                if (!eventExists)
                {
                    throw new Exception("Event not found with ID: " + eventId);
                }

                // Permanently delete the event
                await _firebase.Child("events").Child(eventId).DeleteAsync();

                // Log success for debugging
                System.Diagnostics.Debug.WriteLine($"Successfully deleted event: {eventId}");
            }
            catch (Exception ex)
            {
                // Log the exact error for debugging
                System.Diagnostics.Debug.WriteLine($"Error in DeleteEvent method: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                throw; // Rethrow the exception to be handled by the caller
            }
        }

        // New method for updating user's event list
        public async Task UpdateUserEventList(string userId, List<string> eventIdList)
        {
            try
            {
                // Create a dictionary targeting just the EventID_List field
                var updates = new Dictionary<string, object>
                {
                    { "MemberData/EventID_List", eventIdList }
                };

                // Update only the specified field
                await _firebase
                    .Child("users")
                    .Child(userId)
                    .PatchAsync(updates);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error updating user event list: " + ex.Message);
                throw; // Re-throw the exception to handle it in the calling method
            }
        }

        public async Task<int> GetEventParticipantCount(string eventId)
        {
            try
            {
                // Get all users from Firebase
                var users = await _firebase
                    .Child("users")
                    .OnceAsync<User>();

                // Count how many users have this event in their EventID_List
                int count = 0;
                foreach (var user in users)
                {
                    var userData = user.Object;
                    if (userData.MemberData != null &&
                        userData.MemberData.EventID_List != null &&
                        userData.MemberData.EventID_List.Contains(eventId))
                    {
                        count++;
                    }
                }

                return count;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error counting event participants: {ex.Message}");
                return 0; // Return 0 if there's an error
            }
        }

        // --------------------- BADGE MANAGEMENT ---------------------
        public async Task<string> GenerateNextBadgeID()
        {
            var badges = await _firebase
                .Child("badges")
                .OnceAsync<Badge>();

            int maxID = 0;

            foreach (var badge in badges)
            {
                string badgeKey = badge.Key;
                if (badgeKey.StartsWith("B") && int.TryParse(badgeKey.Substring(1), out int numericID))
                {
                    if (numericID > maxID)
                    {
                        maxID = numericID;
                    }
                }
            }

            int nextID = maxID + 1;
            return "B" + nextID.ToString("D3"); //return B + number that is formatted as a 3-digit string
        }

        public async Task<bool> AddBadge(Badge badge)
        {
            try
            {
                // Generate the next badge ID
                string newBadgeID = await GenerateNextBadgeID();
                badge.BadgeId = newBadgeID;

                // Save the badge to Firebase under the unique ID
                await _firebase
                    .Child("badges") // Ensure this matches your Firebase structure
                    .Child(newBadgeID) // Use the unique ID as the key
                    .PutAsync(badge); // Save the badge object under the unique ID

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding badge to Firebase: " + ex.Message);
            }
        }

        public async Task<List<Badge>> GetBadges()
        {
            var badges = await _firebase
                .Child("badges")
                .OnceAsync<Badge>();

            var badgeList = badges.Select(b => new Badge
            {
                BadgeId = b.Key,
                BadgeName = b.Object.BadgeName,
                BadgeDesc = b.Object.BadgeDesc,
                BadgeIconUrl = b.Object.BadgeIconUrl,
                BadgePoints = b.Object.BadgePoints,
                IsDeleted = b.Object.IsDeleted
            }).ToList();

            return badgeList;
        }

        public async Task UpdateBadge(Badge badge)
        {
            await _firebase
                .Child("badges")
                .Child(badge.BadgeId)
                .PutAsync(badge);
        }

        public async Task<Badge> GetBadgeById(string badgeId)
        {
            return await _firebase
                .Child("badges")
                .Child(badgeId)
                .OnceSingleAsync<Badge>();
        }

        public async Task SoftDeleteBadge(string badgeId)
        {
            await _firebase.Child("badges").Child(badgeId).PatchAsync(new { IsDeleted = true });
        }

        public async Task RestoreBadge(string badgeId)
        {
            await _firebase.Child("badges").Child(badgeId).PatchAsync(new { IsDeleted = false });
        }

        public async Task<List<Badge>> GetActiveBadges()
        {
            var allBadges = await GetBadges();
            return allBadges.Where(b => !b.IsDeleted).ToList();
        }

        public async Task<List<Badge>> GetDeletedBadges()
        {
            var allBadges = await GetBadges();
            return allBadges.Where(b => b.IsDeleted).ToList();
        }

        public async Task DeleteBadge(string badgeId)
        {
            // Permanently delete the badge from Firebase
            await _firebase.Child("badges").Child(badgeId).DeleteAsync();
        }

        // --------------------- NEW TEMPLATE MANAGEMENT (JSON-based) ---------------------
        // Generate next template ID for new JSON-based templates
        public async Task<string> GenerateNextJsonTemplateID()
        {
            var templates = await _firebase
                .Child("jsonTemplates")
                .OnceAsync<Template>();

            int maxID = 0;

            foreach (var template in templates)
            {
                string templateKey = template.Key;
                if (templateKey.StartsWith("T") && int.TryParse(templateKey.Substring(1), out int numericID))
                {
                    if (numericID > maxID)
                    {
                        maxID = numericID;
                    }
                }
            }

            int nextID = maxID + 1;
            return "T" + nextID.ToString("D3"); //return T + number that is formatted as a 3-digit string
        }

        public async Task<bool> AddJsonTemplate(Template template)
        {
            try
            {
                // Generate the next template ID
                string newTemplateID = await GenerateNextJsonTemplateID();
                template.TemplateId = newTemplateID;

                // Set creation date
                template.CreatedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                template.LastModified = template.CreatedDate;

                // Save the template to Firebase under the unique ID in jsonTemplates node
                await _firebase
                    .Child("jsonTemplates") // Use separate node for JSON-based templates
                    .Child(newTemplateID) // Use the unique ID as the key
                    .PutAsync(template); // Save the template object under the unique ID

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding JSON template to Firebase: " + ex.Message);
            }
        }

        public async Task<List<Template>> GetJsonTemplates()
        {
            var templates = await _firebase
                .Child("jsonTemplates")
                .OnceAsync<Template>();

            var templateList = templates.Select(t => new Template
            {
                TemplateId = t.Key,
                TemplateName = t.Object.TemplateName,
                TemplateDescription = t.Object.TemplateDescription,
                TemplateType = t.Object.TemplateType,
                TemplateData = t.Object.TemplateData,
                TemplateImageUrl = t.Object.TemplateImageUrl,
                CreatedBy = t.Object.CreatedBy,
                CreatedDate = t.Object.CreatedDate,
                LastModified = t.Object.LastModified,
                IsDeleted = t.Object.IsDeleted,
                IsActive = t.Object.IsActive
            }).ToList();

            return templateList;
        }

        public async Task UpdateJsonTemplate(Template template)
        {
            template.LastModified = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            await _firebase
                .Child("jsonTemplates")
                .Child(template.TemplateId)
                .PutAsync(template);
        }

        public async Task<Template> GetJsonTemplateById(string templateId)
        {
            return await _firebase
                .Child("jsonTemplates")
                .Child(templateId)
                .OnceSingleAsync<Template>();
        }

        public async Task SoftDeleteJsonTemplate(string templateId)
        {
            await _firebase.Child("jsonTemplates").Child(templateId).PatchAsync(new { IsDeleted = true });
        }

        public async Task RestoreJsonTemplate(string templateId)
        {
            await _firebase.Child("jsonTemplates").Child(templateId).PatchAsync(new { IsDeleted = false });
        }

        public async Task<List<Template>> GetActiveJsonTemplates()
        {
            var allTemplates = await GetJsonTemplates();
            return allTemplates.Where(t => !t.IsDeleted && t.IsActive).ToList();
        }

        public async Task<List<Template>> GetDeletedJsonTemplates()
        {
            var allTemplates = await GetJsonTemplates();
            return allTemplates.Where(t => t.IsDeleted).ToList();
        }

        public async Task DeleteJsonTemplate(string templateId)
        {
            // Permanently delete the template from Firebase
            await _firebase.Child("jsonTemplates").Child(templateId).DeleteAsync();
        }

        public async Task<List<Template>> GetJsonTemplatesByType(string templateType)
        {
            var allTemplates = await GetJsonTemplates();
            return allTemplates.Where(t => !t.IsDeleted && t.TemplateType == templateType).ToList();
        }

        public async Task UpdateUserBadges(string userId, Dictionary<string, object> updates)
        {
            try
            {
                // Update only the specified fields
                await _firebase
                    .Child("users")
                    .Child(userId)
                    .PatchAsync(updates);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error updating user badges: " + ex.Message);
                throw; // Re-throw the exception to handle it in the calling method
            }
        }

        // --------------------- EVENT FEEDBACK MANAGEMENT ---------------------
        // Generate next feedback ID
        public async Task<string> GenerateNextEventFeedbackID()
        {
            var feedbacks = await _firebase
                .Child("eventFeedbacks")
                .OnceAsync<EventFeedback>();

            int maxID = 0;

            foreach (var feedback in feedbacks)
            {
                string feedbackKey = feedback.Key;
                if (feedbackKey.StartsWith("F") && int.TryParse(feedbackKey.Substring(1), out int numericID))
                {
                    if (numericID > maxID)
                    {
                        maxID = numericID;
                    }
                }
            }

            int nextID = maxID + 1;
            return "F" + nextID.ToString("D3");
        }

        // Save event feedback
        public async Task SaveEventFeedback(EventFeedback feedback)
        {
            try
            {
                // Generate feedback ID if not provided
                if (string.IsNullOrEmpty(feedback.FeedbackID))
                {
                    feedback.FeedbackID = await GenerateNextEventFeedbackID();
                }

                // Get user details to include username
                var user = await GetUserById(feedback.UserID);
                if (user != null)
                {
                    feedback.UserName = user.Username;
                }

                // Save feedback to Firebase
                await _firebase
                    .Child("eventFeedbacks")
                    .Child(feedback.FeedbackID)
                    .PutAsync(feedback);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving event feedback: {ex.Message}");
                throw;
            }
        }

        // Get event feedbacks by event ID
        public async Task<List<EventFeedback>> GetEventFeedbacksByEventID(string eventId)
        {
            try
            {
                var feedbacks = await _firebase
                    .Child("eventFeedbacks")
                    .OnceAsync<EventFeedback>();

                return feedbacks
                    .Where(f => f.Object.EventID == eventId)
                    .Select(f => new EventFeedback
                    {
                        FeedbackID = f.Key,
                        EventID = f.Object.EventID,
                        UserID = f.Object.UserID,
                        UserName = f.Object.UserName ?? "Unknown User",
                        FeedbackText = f.Object.FeedbackText,
                        SubmittedDate = f.Object.SubmittedDate
                    })
                    .OrderByDescending(f => f.SubmittedDate)
                    .ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error fetching event feedbacks: {ex.Message}");
                return new List<EventFeedback>();
            }
        }

        // --------------------- USER MANAGEMENT ---------------------
        public async Task<User> GetUserById(string userId)
        {
            try
            {
                // Fetch user details by Designated User ID
                var userData = await _firebase
                    .Child("users")
                    .Child(userId)
                    .OnceSingleAsync<Dictionary<string, object>>();

                if (userData != null)
                {
                    // Parse MemberData if it exists
                    MemberData memberData = null;
                    if (userData.ContainsKey("MemberData"))
                    {
                        var memberDataJObject = userData["MemberData"] as JObject;
                        if (memberDataJObject != null)
                        {
                            memberData = memberDataJObject.ToObject<MemberData>();
                        }
                        else
                        {
                            // Handle case where memberData is not a JObject
                            var memberDataDict = userData["MemberData"] as Dictionary<string, object>;
                            if (memberDataDict != null)
                            {
                                memberData = new MemberData
                                {
                                    Points = memberDataDict.ContainsKey("Points") ? Convert.ToInt32(memberDataDict["Points"]) : 0,
                                    Level = memberDataDict.ContainsKey("Level") ? Convert.ToInt32(memberDataDict["Level"]) : 0,
                                    TestimonialApplicationStatus = memberDataDict.ContainsKey("TestimonialApplicationStatus")
                                        ? memberDataDict["TestimonialApplicationStatus"].ToString()
                                        : "N/A",
                                    // Initialize lists as empty if not present
                                    ApplicationID_List = new List<string>(),
                                    BadgeID_List = new List<string>(),
                                    CourseID_List = new List<string>(),
                                    EventID_List = new List<string>(),
                                    Interest_List = new List<string>()
                                };
                            }
                        }
                    }

                    // Create and return the User object
                    return new User
                    {
                        FirebaseUid = userData.ContainsKey("FirebaseUid") ? userData["FirebaseUid"].ToString() : null,
                        UserId = userId,
                        Username = userData.ContainsKey("Username") ? userData["Username"].ToString() : "not set",
                        PhoneNumber = userData.ContainsKey("PhoneNumber") ? userData["PhoneNumber"].ToString() : "not set",
                        ProfilePictureUrl = userData.ContainsKey("ProfilePictureUrl") ? userData["ProfilePictureUrl"].ToString() : null,
                        Email = userData.ContainsKey("Email") ? userData["Email"].ToString() : null,
                        Role = userData.ContainsKey("Role") ? userData["Role"].ToString() : "Member",
                        JoinDate = userData.ContainsKey("JoinDate") ? userData["JoinDate"].ToString() : null,
                        MemberData = memberData,
                        SecurityQuestion = userData.ContainsKey("SecurityQuestion") ? userData["SecurityQuestion"].ToString() : null,
                        SecurityAnswer = userData.ContainsKey("SecurityAnswer") ? userData["SecurityAnswer"].ToString() : null,
                        AccountStatus = userData.ContainsKey("AccountStatus") ? userData["AccountStatus"].ToString() : "enabled",
                    };
                }

                return null; // User not found
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error fetching user by Designated User ID: " + ex.Message);
                return null;
            }
        }

        public async Task<(string Role, string UserId, string Username)> GetUserByFirebaseUid(string firebaseUid)
        {
            try
            {
                // Fetch all users
                var users = await _firebase
                    .Child("users")
                    .OnceAsync<Dictionary<string, object>>();

                // Search for the user with the matching Firebase UID
                foreach (var user in users)
                {
                    var userData = user.Object;
                    if (userData.ContainsKey("FirebaseUid") && userData["FirebaseUid"].ToString() == firebaseUid)
                    {
                        // Extract role and designated user ID
                        string role = userData.ContainsKey("Role") ? userData["Role"].ToString() : "Member"; // Default role
                        string userId = user.Key; // Designated user ID (e.g., U001, U002)
                        string username = userData.ContainsKey("Username") ? userData["Username"].ToString() : userId; // ID as Default name
                        return (role, userId, username);
                    }
                }

                return (null, null, null); // User not found
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error fetching role and user ID by Firebase UID: " + ex.Message);
                return (null, null, null);
            }
        }

        public async Task UpdateUserPersonalInfo(User user)
        {
            // Create a dictionary targeting the specific fields to update
            var updates = new Dictionary<string, object>
                    {
                        { "Username", user.Username },
                        { "Role", user.Role},
                        { "MemberData/Interest_List", user.MemberData?.Interest_List ?? new List<string>() }
                    };

            // Update only the specified fields
            await _firebase
                .Child("users")
                .Child(user.UserId)
                .PatchAsync(updates);
        }

        public async Task UpdateUserSecurityInfo(User user)
        {
            // Create a dictionary targeting the specific fields to update
            var updates = new Dictionary<string, object>
            {
                { "Email", user.Email },
                { "PhoneNumber", user.PhoneNumber },
                { "SecurityQuestion", user.SecurityQuestion },
                { "SecurityAnswer", user.SecurityAnswer }
            };

            // Update only the specified fields
            await _firebase
                .Child("users")
                .Child(user.UserId)
                .PatchAsync(updates);
        }

        public async Task UpdateUserProfilePicture(string userId, string profilePictureUrl)
        {
            try
            {
                // Update only the ProfilePictureUrl field
                var updates = new Dictionary<string, object>
                {
                    { "ProfilePictureUrl", profilePictureUrl }
                };

                // Update user in Firebase
                await _firebase
                    .Child("users")
                    .Child(userId)
                    .PatchAsync(updates);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating profile picture URL: {ex.Message}", ex);
            }
        }

        public async Task SendPasswordResetEmailAsync(string email)
        {
            try
            {
                // Send a password reset email using the FirebaseAuthProvider
                await _authProvider.SendPasswordResetEmailAsync(email);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error sending password reset email: " + ex.Message);
                throw; // Re-throw the exception to handle it in the calling method
            }
        }

        public async Task DisableUserAccount(string userId, string firebaseUid)
        {
            try
            {
                // Update the user's account status in the Realtime Database
                var updates = new Dictionary<string, object>
                    {
                        { "AccountStatus", "disabled" }
                    };

                await _firebase
                    .Child("users")
                    .Child(userId)
                    .PatchAsync(updates);

                // Disable the user in Firebase Authentication using the Admin SDK
                var updateArgs = new UserRecordArgs
                {
                    Uid = firebaseUid,
                    Disabled = true
                };

                await _firebaseAuth.UpdateUserAsync(updateArgs);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error disabling user account: " + ex.Message);
                throw; // Re-throw the exception to handle it in the calling method
            }
        }

        public async Task EnableUserAccount(string userId, string firebaseUid)
        {
            try
            {
                // Create a dictionary to update the user's status
                var updates = new Dictionary<string, object>
                {
                    { "AccountStatus", "enabled" } // Update the status to "enabled"
                };

                // Update the user's status in Firebase
                await _firebase
                    .Child("users")
                    .Child(userId)
                    .PatchAsync(updates);

                // Enable the user in Firebase Authentication using the Admin SDK
                var updateArgs = new UserRecordArgs
                {
                    Uid = firebaseUid,
                    Disabled = false
                };

                await _firebaseAuth.UpdateUserAsync(updateArgs);
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as needed
                throw new Exception("Failed to enable user account.", ex);
            }
        }

        // Method to link phone number to an existing account
        public async Task LinkPhoneToAccount(string firebaseUid, string phoneNumber)
        {
            try
            {
                // Update the user's phone number in the Realtime Database
                var userRecord = await _firebaseAuth.GetUserAsync(firebaseUid);
                string userId = "";

                // Find the user's custom ID from Realtime Database
                var users = await _firebase.Child("users").OnceAsync<Dictionary<string, object>>();
                foreach (var user in users)
                {
                    var userData = user.Object;
                    if (userData.ContainsKey("FirebaseUid") && userData["FirebaseUid"].ToString() == firebaseUid)
                    {
                        userId = user.Key;
                        break;
                    }
                }

                if (string.IsNullOrEmpty(userId))
                {
                    throw new Exception("User not found in database");
                }

                // Update phone number in Realtime Database
                var updates = new Dictionary<string, object>
                {
                    { "PhoneNumber", phoneNumber }
                };

                await _firebase
                    .Child("users")
                    .Child(userId)
                    .PatchAsync(updates);

                // Note: Firebase Authentication doesn't have a direct method to link phone numbers
                // Phone verification must be handled separately through the client-side Firebase SDK

                return;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error linking phone to account: {ex.Message}");
                throw new Exception($"Failed to link phone: {ex.Message}", ex);
            }
        }

        public async Task LinkEmailToAccount(string firebaseUid, string email, string password)
        {
            try
            {
                // Check if email already exists using a more reliable method
                bool emailExists = false;
                try
                {
                    // Attempt to get user by email
                    var existingUser = await _firebaseAuth.GetUserByEmailAsync(email);
                    emailExists = true; // Email exists if we get here
                }
                catch (Exception ex)
                {
                    // Most likely a user-not-found exception, which is what we want
                    System.Diagnostics.Debug.WriteLine($"Email check exception (expected for new emails): {ex.Message}");
                    emailExists = false;
                }

                if (emailExists)
                {
                    throw new Exception("This email is already associated with an account.");
                }

                // Update the user's email and password using Admin SDK
                var userArgs = new UserRecordArgs
                {
                    Uid = firebaseUid,
                    Email = email,
                    EmailVerified = false, // Start as unverified
                    Password = password
                };

                // Update the user record
                await _firebaseAuth.UpdateUserAsync(userArgs);

                // Send verification email using Firebase Auth Provider
                var authProvider = new FirebaseAuthProvider(new FirebaseConfig("AIzaSyAqpGAELltYzFuKiCjl4o71VszV6Px7J6w"));

                // First, get a custom token for the user
                string customToken = await _firebaseAuth.CreateCustomTokenAsync(firebaseUid);

                // Sign in with the custom token to get an ID token
                var authResult = await authProvider.SignInWithCustomTokenAsync(customToken);

                // Send the verification email
                await authProvider.SendEmailVerificationAsync(authResult.FirebaseToken);

                return;
            }
            catch (Exception ex)
            {
                // Log the error
                System.Diagnostics.Debug.WriteLine($"Error linking email to account: {ex.Message}");
                throw new Exception($"Failed to link email: {ex.Message}", ex);
            }
        }

        // Helper method to send verification email
        private async Task SendVerificationEmail(string email, string verificationLink)
        {
            try
            {
                // Use a service like SendGrid, MailKit, etc.
                // This is a placeholder where you'd implement your email sending logic

                // Example using System.Net.Mail (you'd need to configure SMTP settings):
                using (var client = new System.Net.Mail.SmtpClient("smtp.gmail.com", 587))
                {
                    client.EnableSsl = true;
                    client.UseDefaultCredentials = false;
                    client.Credentials = new System.Net.NetworkCredential("your-email@gmail.com", "your-app-password");

                    var message = new System.Net.Mail.MailMessage();
                    message.From = new System.Net.Mail.MailAddress("your-email@gmail.com");
                    message.To.Add(email);
                    message.Subject = "Verify your email address";
                    message.Body = $"Please verify your email by clicking on this link: {verificationLink}";
                    message.IsBodyHtml = true;

                    await client.SendMailAsync(message);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error sending verification email: {ex.Message}");
                // We don't want to throw here, as the email already got linked
                // Just log the error
            }
        }

        // Check if an email is already registered
        public async Task<bool> IsEmailRegistered(string email)
        {
            try
            {
                // Attempt to get user by email
                var user = await _firebaseAuth.GetUserByEmailAsync(email);
                return true; // Email exists
            }
            catch (Exception)
            {
                // Any exception here typically means the user doesn't exist
                return false;
            }
        }

        public async Task<bool> HasEmail(string firebaseUid)
        {
            try
            {
                var userRecord = await _firebaseAuth.GetUserAsync(firebaseUid);
                return !string.IsNullOrEmpty(userRecord.Email);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to check user email: " + ex.Message);
            }
        }

        public async Task<List<User>> GetUsers()
        {
            var users = await _firebase
                .Child("users")
                .OnceAsync<Dictionary<string, object>>();

            var userList = users.Select(u => new User
            {
                UserId = u.Key,
                Username = u.Object.ContainsKey("Username") ? u.Object["Username"].ToString() : "not set",
                PhoneNumber = u.Object.ContainsKey("PhoneNumber") ? u.Object["PhoneNumber"].ToString() : "not set",
                Email = u.Object.ContainsKey("Email") ? u.Object["Email"].ToString() : null,
                Role = u.Object.ContainsKey("Role") ? u.Object["Role"].ToString() : "Member",
                FirebaseUid = u.Object.ContainsKey("FirebaseUid") ? u.Object["FirebaseUid"].ToString() : null,
                AccountStatus = u.Object.ContainsKey("AccountStatus") ? u.Object["AccountStatus"].ToString() : "enabled",
                MemberData = u.Object.ContainsKey("MemberData") ? ParseMemberData(u.Object["MemberData"]) : null,
                SecurityQuestion = u.Object.ContainsKey("SecurityQuestion") ? u.Object["SecurityQuestion"].ToString() : null,
                SecurityAnswer = u.Object.ContainsKey("SecurityAnswer") ? u.Object["SecurityAnswer"].ToString() : null
            }).ToList();

            return userList;
        }

        private MemberData ParseMemberData(object memberDataObj)
        {
            if (memberDataObj is JObject memberDataJObject)
            {
                return memberDataJObject.ToObject<MemberData>();
            }
            else if (memberDataObj is Dictionary<string, object> memberDataDict)
            {
                var memberData = new MemberData
                {
                    Points = memberDataDict.ContainsKey("Points") ? Convert.ToInt32(memberDataDict["Points"]) : 0,
                    Level = memberDataDict.ContainsKey("Level") ? Convert.ToInt32(memberDataDict["Level"]) : 0,
                    TestimonialApplicationStatus = memberDataDict.ContainsKey("TestimonialApplicationStatus")
                        ? memberDataDict["TestimonialApplicationStatus"].ToString()
                        : "N/A"
                };

                // Handle BadgeID_List with special care to maintain the ID:status format
                if (memberDataDict.ContainsKey("BadgeID_List"))
                {
                    if (memberDataDict["BadgeID_List"] is JArray badgeArray)
                    {
                        memberData.BadgeID_List = badgeArray.ToObject<List<string>>();

                        // Convert old format badges to new format if needed
                        for (int i = 0; i < memberData.BadgeID_List.Count; i++)
                        {
                            string badge = memberData.BadgeID_List[i];
                            if (!badge.Contains(":"))
                            {
                                // If badge doesn't have a status, assume it's already claimed
                                memberData.BadgeID_List[i] = $"{badge}:claimed";
                            }
                        }
                    }
                    else
                    {
                        memberData.BadgeID_List = new List<string>();
                    }
                }
                else
                {
                    memberData.BadgeID_List = new List<string>();
                }

                // Parse other lists
                memberData.CompletedCourseID_List = memberDataDict.ContainsKey("CompletedCourseID_List")
                    ? ((JArray)memberDataDict["CompletedCourseID_List"]).ToObject<List<string>>()
                    : new List<string>();

                memberData.ApplicationID_List = memberDataDict.ContainsKey("ApplicationID_List")
                    ? ((JArray)memberDataDict["ApplicationID_List"]).ToObject<List<string>>()
                    : new List<string>();

                memberData.CourseID_List = memberDataDict.ContainsKey("CourseID_List")
                    ? ((JArray)memberDataDict["CourseID_List"]).ToObject<List<string>>()
                    : new List<string>();

                memberData.EventID_List = memberDataDict.ContainsKey("EventID_List")
                    ? ((JArray)memberDataDict["EventID_List"]).ToObject<List<string>>()
                    : new List<string>();

                memberData.Interest_List = memberDataDict.ContainsKey("Interest_List")
                    ? ((JArray)memberDataDict["Interest_List"]).ToObject<List<string>>()
                    : new List<string>();

                return memberData;
            }
            return null;
        }

        // --------------------- ACHIEVEMENT MANAGEMENT ---------------------
        public async Task<string> GenerateNextAchivementApplicationID()
        {
            var applications = await _firebase
                .Child("achievementApplications")
                .OnceAsync<AchievementApplication>();

            int maxID = 0;

            foreach (var application in applications)
            {
                string applicationKey = application.Key;
                if (applicationKey.StartsWith("A") && int.TryParse(applicationKey.Substring(1), out int numericID))
                {
                    if (numericID > maxID)
                    {
                        maxID = numericID;
                    }
                }
            }

            int nextID = maxID + 1;
            return "A" + nextID.ToString("D3"); //return A + number that is formatted as a 3-digit string
        }

        public async Task SaveAchievementApplication(AchievementApplication application)
        {
            try
            {
                // Generate the next application ID
                string newAppID = await GenerateNextAchivementApplicationID();
                application.ApplicationId = newAppID;

                // Save the application to Firebase Database
                await _firebase
                    .Child("achievementApplications")
                    .Child(newAppID)
                    .PutAsync(application);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to save application.", ex);
            }
        }

        public async Task<List<AchievementApplication>> GetAchievementApplications()
        {
            try
            {
                var applications = await _firebase
                    .Child("achievementApplications")
                    .OnceAsync<AchievementApplication>();

                return applications.Select(a => new AchievementApplication
                {
                    ApplicationId = a.Key,
                    ApplicantRefId = a.Object.ApplicantRefId,
                    EventId = a.Object.EventId,
                    ParticipatedRole = a.Object.ParticipatedRole,
                    LearningOutcome = a.Object.LearningOutcome,
                    Comment = a.Object.Comment,
                    SupportingDocUrl = a.Object.SupportingDocUrl,
                    Status = a.Object.Status
                }).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to fetch applications.", ex);
            }
        }

        public async Task<AchievementApplication> GetAchievementApplicationById(string applicationId)
        {
            try
            {
                var application = await _firebase
                    .Child("achievementApplications")
                    .Child(applicationId)
                    .OnceSingleAsync<AchievementApplication>();

                return application;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to fetch application.", ex);
            }
        }

        public async Task UpdateAchievementApplication(string applicationId, AchievementApplication application)
        {
            try
            {
                await _firebase
                    .Child("achievementApplications")
                    .Child(applicationId)
                    .PutAsync(application);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to update application.", ex);
            }
        }

        // --------------------- MEDIA & INTRODUCTION MANAGEMENT ---------------------
        // Add these methods to the FirebaseHelper class
        public async Task UpdateIntroductionMedia(string mediaKey, IntroductionMedia media)
        {
            try
            {
                // Create a path for storing media assets
                await _firebase
                    .Child("media_assets")
                    .Child(mediaKey)
                    .PutAsync(media);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating introduction media: {ex.Message}", ex);
            }
        }

        public async Task<IntroductionMedia> GetIntroductionMedia(string mediaKey)
        {
            try
            {
                var mediaAsset = await _firebase
                    .Child("media_assets")
                    .Child(mediaKey)
                    .OnceSingleAsync<IntroductionMedia>();

                return mediaAsset;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting introduction media: {ex.Message}");
                return null;
            }
        }

        // --------------------- CERTIFICATE & TEMPLATE MANAGEMENT ---------------------
        // In the FirebaseHelper class, add these methods for certificate template management:

        public async Task StoreTemplateFieldCoordinates(string templateId, Dictionary<string, FieldData> fieldCoordinates)
        {
            try
            {
                // Convert field data to Firebase-compatible format
                var templateData = new Dictionary<string, object>
                {
                    { "templateId", templateId },
                    { "createdAt", DateTime.UtcNow.ToString("o") },
                    { "fields", ConvertFieldsToFirebaseFormat(fieldCoordinates) }
                };

                // Save to Firebase under templates
                await _firebase
                    .Child("templates")
                    .Child(templateId)
                    .PutAsync(templateData);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error storing template field coordinates: {ex.Message}");
                throw;
            }
        }
        public async Task<Dictionary<string, FieldData>> GetTemplateFieldCoordinates(string templateId)
        {
            try
            {
                var templateData = await _firebase
                    .Child("templates")
                    .Child(templateId)
                    .OnceSingleAsync<Dictionary<string, object>>();

                if (templateData != null && templateData.ContainsKey("fields"))
                {
                    return ConvertFirebaseToFieldData(templateData["fields"]);
                }

                return new Dictionary<string, FieldData>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error retrieving template field coordinates: {ex.Message}");
                throw;
            }
        }
        public async Task<string> GetTemplateUrl(string templateId)
        {
            try
            {
                var templateData = await _firebase
                    .Child("templates")
                    .Child(templateId)
                    .OnceSingleAsync<Dictionary<string, object>>();

                if (templateData != null && templateData.ContainsKey("templateUrl"))
                {
                    return templateData["templateUrl"].ToString();
                }

                return string.Empty;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting template URL: {ex.Message}");
                throw;
            }
        }
        public async Task<string> GenerateNextTemplateID()
        {
            var templates = await _firebase
                .Child("templates")
                .OnceAsync<CertificateTemplate>();

            int maxID = 0;

            foreach (var template in templates)
            {
                string templateKey = template.Key;
                if (templateKey.StartsWith("T") && int.TryParse(templateKey.Substring(1), out int numericID))
                {
                    if (numericID > maxID)
                    {
                        maxID = numericID;
                    }
                }
            }

            int nextID = maxID + 1;
            return "T" + nextID.ToString("D3"); //return T + number that is formatted as a 3-digit string
        }
        public async Task SetTestimonialTemplate(string templateId)
        {
            try
            {
                // First, remove testimonial flag from all existing templates
                var allTemplates = await GetAllTemplates();
                foreach (var template in allTemplates.Where(t => t.IsTestimonialTemplate))
                {
                    var updateData = new Dictionary<string, object>
                    {
                        { "isTestimonialTemplate", false }
                    };
                    await UpdateTemplateData(template.TemplateId, updateData);
                }

                // Then set the new template as testimonial template
                var updateNewTemplate = new Dictionary<string, object>
                {
                    { "isTestimonialTemplate", true }
                };
                await UpdateTemplateData(templateId, updateNewTemplate);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error setting testimonial template: {ex.Message}");
                throw;
            }
        }
        public async Task SoftDeleteTemplate(string templateId)
        {
            try
            {
                // Check if this is the current testimonial template
                var template = await GetTemplateById(templateId);
                if (template.IsTestimonialTemplate)
                {
                    throw new InvalidOperationException("Cannot delete the current testimonial template.");
                }

                var updateData = new Dictionary<string, object>
                {
                    { "isDeleted", true },
                    { "isActive", false }
                };

                await UpdateTemplateData(templateId, updateData);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error soft deleting template: {ex.Message}");
                throw;
            }
        }
        public async Task UpdateTemplateData(string templateId, Dictionary<string, object> templateData)
        {
            try
            {
                // Assume we have a Firebase reference to templates
                await _firebase
                    .Child("templates")
                    .Child(templateId)
                    .PatchAsync(templateData);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating template data: {ex.Message}");
                throw;
            }
        }
        public async Task<List<CertificateTemplate>> GetAllTemplates()
        {
            try
            {
                var templates = await _firebase
                    .Child("templates")
                    .OnceAsync<Dictionary<string, object>>();

                return templates.Select(t =>
                    CertificateTemplate.FromFirebaseObject(t.Key, t.Object)).ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error retrieving templates: {ex.Message}");
                throw;
            }
        }
        public async Task<CertificateTemplate> GetTemplateById(string templateId)
        {
            try
            {
                var templateData = await _firebase
                    .Child("templates")
                    .Child(templateId)
                    .OnceSingleAsync<Dictionary<string, object>>();

                // Extensive debug logging
                System.Diagnostics.Debug.WriteLine($"Retrieving Template ID: {templateId}");
                System.Diagnostics.Debug.WriteLine("Raw Template Data:");
                foreach (var kvp in templateData)
                {
                    System.Diagnostics.Debug.WriteLine($"Key: {kvp.Key}, Value: {kvp.Value}");
                }

                if (templateData != null)
                {
                    // Check if fields exist
                    if (templateData.ContainsKey("fields"))
                    {
                        System.Diagnostics.Debug.WriteLine("Fields exist in template data:");
                        var fieldsData = templateData["fields"];

                        // Debug the type and content of fields
                        System.Diagnostics.Debug.WriteLine($"Fields data type: {fieldsData?.GetType().Name}");

                        if (fieldsData is Dictionary<string, object> fieldsDict)
                        {
                            System.Diagnostics.Debug.WriteLine($"Number of fields: {fieldsDict.Count}");
                            foreach (var field in fieldsDict)
                            {
                                System.Diagnostics.Debug.WriteLine($"Field key: {field.Key}");
                                System.Diagnostics.Debug.WriteLine($"Field value type: {field.Value?.GetType().Name}");
                            }
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("Fields is not a Dictionary<string, object>");
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("No 'fields' key found in template data");
                    }

                    return CertificateTemplate.FromFirebaseObject(templateId, templateData);
                }

                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error retrieving template by ID: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                throw;
            }
        }
        public async Task DeleteTemplate(string templateId)
        {
            try
            {
                await _firebase
                    .Child("templates")
                    .Child(templateId)
                    .DeleteAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting template: {ex.Message}");
                throw;
            }
        }
        private Dictionary<string, object> ConvertFieldsToFirebaseFormat(Dictionary<string, FieldData> fieldCoordinates)
        {
            var result = new Dictionary<string, object>();

            foreach (var field in fieldCoordinates)
            {
                result[field.Key] = new Dictionary<string, object>
                {
                    { "type", field.Value.Type },
                    { "label", field.Value.Label },
                    { "x", field.Value.Bounds.X },
                    { "y", field.Value.Bounds.Y },
                    { "width", field.Value.Bounds.Width },
                    { "height", field.Value.Bounds.Height },
                    { "isRequired", field.Value.IsRequired }
                };
            }

            return result;
        }
        private Dictionary<string, FieldData> ConvertFirebaseToFieldData(object firebaseData)
        {
            var result = new Dictionary<string, FieldData>();

            try
            {
                if (firebaseData is Dictionary<string, object> fieldsDict)
                {
                    foreach (var field in fieldsDict)
                    {
                        string fieldName = field.Key;

                        // If the field value is a dictionary, extract field properties
                        if (field.Value is Dictionary<string, object> fieldDict)
                        {
                            var fieldData = new FieldData
                            {
                                Type = fieldDict.TryGetValue("type", out object type) ? type.ToString() : "Text",
                                Label = fieldDict.TryGetValue("label", out object label) ? label.ToString() : fieldName
                            };

                            // Check if required is specified
                            if (fieldDict.TryGetValue("isRequired", out object required))
                            {
                                fieldData.IsRequired = Convert.ToBoolean(required);
                            }

                            // Parse bounds
                            if (fieldDict.TryGetValue("x", out object x) &&
                                fieldDict.TryGetValue("y", out object y) &&
                                fieldDict.TryGetValue("width", out object width) &&
                                fieldDict.TryGetValue("height", out object height))
                            {
                                fieldData.Bounds = new System.Drawing.Rectangle(
                                    Convert.ToInt32(x),
                                    Convert.ToInt32(y),
                                    Convert.ToInt32(width),
                                    Convert.ToInt32(height)
                                );
                            }

                            result[fieldName] = fieldData;
                        }
                        else
                        {
                            // Log for debugging purposes
                            System.Diagnostics.Debug.WriteLine($"Field '{fieldName}' value is not a dictionary: {field.Value?.GetType().Name ?? "null"}");
                        }
                    }
                }
                else if (firebaseData != null)
                {
                    // Log for debugging purposes
                    System.Diagnostics.Debug.WriteLine($"FirebaseData is not a dictionary: {firebaseData.GetType().Name}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error converting Firebase field data: {ex.Message}");
            }

            return result;
        }
        public async Task StoreGeneratedCertificate(string userId, string certificateId, Dictionary<string, object> certificateData)
        {
            try
            {
                // Store in Firebase under certificates collection
                await _firebase
                    .Child("certificates")
                    .Child(certificateId)
                    .PutAsync(certificateData);

                // Also add reference to the user's certificates list
                await _firebase
                    .Child("users")
                    .Child(userId)
                    .Child("certificates")
                    .Child(certificateId)
                    .PutAsync(certificateData);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error storing generated certificate: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Stores a generated certificate in Firebase (legacy method for backward compatibility)
        /// </summary>
        public async Task StoreGeneratedCertificate(string userId, string templateId, string certificateUrl)
        {
            try
            {
                // Generate a unique certificate ID
                string certificateId = $"cert_{Guid.NewGuid():N}";

                // Create certificate data
                var certificateData = new Dictionary<string, object>
        {
            { "url", certificateUrl },
            { "templateId", templateId },
            { "generatedAt", DateTime.UtcNow.ToString("o") }
        };

                // Store in Firebase
                await StoreGeneratedCertificate(userId, certificateId, certificateData);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error storing generated certificate: {ex.Message}");
                throw;
            }
        }

        // --------------------- CUSTOMIZATION ---------------------
        public async Task<SiteSettings> GetSiteSettingsData()
        {
            try
            {
                var settingsData = await _firebase
                    .Child("site_settings")
                    .OnceSingleAsync<SiteSettings>();

                return settingsData;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting site settings: {ex.Message}");
                return null;
            }
        }

        public async Task SaveSiteSettingsData(SiteSettings settings)
        {
            try
            {
                await _firebase
                    .Child("site_settings")
                    .PutAsync(settings);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving site settings: {ex.Message}");
                throw;
            }
        }
        public async Task<string> GetSiteCustomCss()
        {
            try
            {
                var cssData = await _firebase
                    .Child("site_settings")
                    .Child("customCss")
                    .OnceSingleAsync<string>();

                return cssData;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting custom CSS: {ex.Message}");
                return null;
            }
        }
        public async Task SaveSiteCustomCss(string css)
        {
            try
            {
                await _firebase
                    .Child("site_settings")
                    .Child("customCss")
                    .PutAsync(css);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving custom CSS: {ex.Message}");
                throw;
            }
        }

        public async Task<string> AddStructuredTemplate(Dictionary<string, object> metadata)
        {
            string newTemplateID = await GenerateNextTemplateID();
            metadata["TemplateId"] = newTemplateID;
            await _firebase
                .Child("templates")
                .Child(newTemplateID)
                .PutAsync(metadata);
            return newTemplateID;
        }

        public async Task UpdateStructuredTemplate(string templateId, Dictionary<string, object> metadata)
        {
            metadata["TemplateId"] = templateId;
            await _firebase
                .Child("templates")
                .Child(templateId)
                .PutAsync(metadata);
        }
    }

    public class FieldData
    {
        public string Type { get; set; } = "Text";
        public string Label { get; set; }
        public Rectangle Bounds { get; set; }
        public bool IsRequired { get; set; }

        public FieldData()
        {
            Bounds = new Rectangle(0, 0, 100, 30);
            IsRequired = false;
            Type = "Text";
        }

        public static FieldData FromFirebaseObject(string fieldName, Dictionary<string, object> data)
        {
            var field = new FieldData
            {
                Label = fieldName
            };

            if (data == null)
                return field;

            // Handle potential JObject values
            object typeObj = data.ContainsKey("type") ? data["type"] : null;
            object requiredObj = data.ContainsKey("isRequired") ? data["isRequired"] : null;

            // Parse field type
            if (typeObj != null)
            {
                field.Type = typeObj is JValue jValue ? jValue.ToString() : typeObj.ToString();
            }

            // Parse isRequired
            if (requiredObj != null)
            {
                field.IsRequired = requiredObj is JValue jBoolValue
                    ? Convert.ToBoolean(jBoolValue.Value)
                    : Convert.ToBoolean(requiredObj);
            }

            // Parse bounds
            int x = 0, y = 0, width = 100, height = 30;

            x = ParseIntValue(data, "x", x);
            y = ParseIntValue(data, "y", y);
            width = ParseIntValue(data, "width", width);
            height = ParseIntValue(data, "height", height);

            field.Bounds = new Rectangle(x, y, width, height);

            return field;
        }


        // Helper method to parse integer values, handling both direct values and JValue
        private static int ParseIntValue(Dictionary<string, object> data, string key, int defaultValue)
        {
            if (!data.ContainsKey(key))
                return defaultValue;

            object value = data[key];

            if (value is JValue jValue)
            {
                return Convert.ToInt32(jValue.Value);
            }

            return Convert.ToInt32(value);
        }

    }

    public class AchievementApplication
    {
        public string ApplicationId { get; set; }
        public string ApplicantRefId { get; set; } // ID of the user submitting the application
        public string EventId { get; set; } // ID of the event
        public string ParticipatedRole { get; set; } // Role of the user in the event
        public string LearningOutcome { get; set; } // Learning outcome description
        public string Comment { get; set; } // Comment from supervisor
        public string SupportingDocUrl { get; set; } // URL of the uploaded file (e.g., Google Drive link)
        public string Status { get; set; } // Status of the application (e.g., "Pending", "Approved", "Rejected")
    }

    public class Event
    {
        public string EventID { get; set; }
        public string EventTitle { get; set; }
        public string EventDescription { get; set; }
        public string EventStartDate { get; set; }
        public string EventEndDate { get; set; }
        public string EventDate { get; set; } // Keeping this for backward compatibility
        public string EventTime { get; set; }
        public string EventLocation { get; set; }
        public string OrganizerName { get; set; }
        public int MaxParticipants { get; set; }
        public string EventPictureUrl { get; set; }
        public Dictionary<string, string> EventMaterials { get; set; }
        public bool IsDeleted { get; set; } = false;
        public string EventStatus { get; set; } = "Upcoming";
        public string RegistrationStartDate { get; set; } // When users can start joining

        public DateTime GetEventDateTime()
        {
            // For backward compatibility, use EventDate if EventStartDate is not set
            string dateToUse = !string.IsNullOrEmpty(EventStartDate) ? EventStartDate : EventDate;

            // Parse date and time strings to create a DateTime
            if (string.IsNullOrEmpty(dateToUse) || string.IsNullOrEmpty(EventTime))
                return DateTime.MinValue;

            try
            {
                DateTime date = DateTime.Parse(dateToUse);
                TimeSpan time = TimeSpan.Parse(EventTime);
                return new DateTime(date.Year, date.Month, date.Day, time.Hours, time.Minutes, 0);
            }
            catch
            {
                return DateTime.MinValue;
            }
        }

        public DateTime GetEventEndDateTime()
        {
            // If no end date is specified, use start date
            string dateToUse = !string.IsNullOrEmpty(EventEndDate) ? EventEndDate :
                              (!string.IsNullOrEmpty(EventStartDate) ? EventStartDate : EventDate);

            if (string.IsNullOrEmpty(dateToUse) || string.IsNullOrEmpty(EventTime))
                return DateTime.MinValue;

            try
            {
                DateTime date = DateTime.Parse(dateToUse);
                TimeSpan time = TimeSpan.Parse(EventTime);
                return new DateTime(date.Year, date.Month, date.Day, time.Hours, time.Minutes, 0);
            }
            catch
            {
                return DateTime.MinValue;
            }
        }

        public DateTime GetRegistrationStartDateTime()
        {
            if (string.IsNullOrEmpty(RegistrationStartDate))
                return DateTime.MinValue;

            try
            {
                return DateTime.Parse(RegistrationStartDate);
            }
            catch
            {
                return DateTime.MinValue;
            }
        }

        public bool IsEventInPast()
        {
            // If there's an end date, check if that date is past
            DateTime eventEndDateTime = GetEventEndDateTime();
            if (eventEndDateTime != DateTime.MinValue)
                return eventEndDateTime < DateTime.Now;

            // Otherwise, check start date as before
            DateTime eventDateTime = GetEventDateTime();
            return eventDateTime != DateTime.MinValue && eventDateTime < DateTime.Now;
        }

        public void UpdateEventStatus()
        {
            DateTime now = DateTime.Now;

            // Get event dates
            DateTime startDateTime = GetEventDateTime();
            DateTime endDateTime = GetEventEndDateTime();

            // If the event has an end date and that date is in the past, it's completed
            if (endDateTime != DateTime.MinValue && endDateTime < now)
            {
                EventStatus = "Completed";
            }
            // If the event has started but not yet ended, it's in progress
            else if (startDateTime != DateTime.MinValue && startDateTime <= now &&
                    (endDateTime == DateTime.MinValue || endDateTime >= now))
            {
                EventStatus = "In Progress";
            }
            // Otherwise, it's upcoming
            else
            {
                EventStatus = "Upcoming";
            }
        }

        public bool IsRegistrationOpen()
        {
            DateTime now = DateTime.Now;

            // If registration date is specified, check if current date is past registration start date
            DateTime registrationStartDate = GetRegistrationStartDateTime();
            if (registrationStartDate != DateTime.MinValue && registrationStartDate > now)
                return false;

            // Get event dates
            DateTime eventStartDateTime = GetEventDateTime();

            // Registration is open if the event hasn't started yet
            return eventStartDateTime != DateTime.MinValue && eventStartDateTime > now;
        }
    }

    public class Course
    {
        public string CourseID { get; set; }
        public string CourseName { get; set; }
        public string CourseDescription { get; set; }
        public string CourseCategory { get; set; }  // Added CourseCategory  
        public int NumberOfStudents { get; set; }
        public string CoursePictureUrl { get; set; }
        public Dictionary<string, string> CourseMaterials { get; set; }
        public bool IsDeleted { get; set; } = false;
        public string CreatedDate { get; set; }
        public string PdfDocumentUrl { get; set; }
    }

    public class Badge
    {
        public string BadgeId { get; set; }
        public string BadgeName { get; set; }
        public string BadgeIconUrl { get; set; }
        public string BadgeDesc { get; set; }
        public int BadgePoints { get; set; }
        public bool IsDeleted { get; set; } = false;
    }

    public class Template
    {
        public string TemplateId { get; set; }
        public string TemplateName { get; set; }
        public string TemplateDescription { get; set; }
        public string TemplateType { get; set; } // "Certificate", "Badge", "Testimonial", etc.
        public string TemplateData { get; set; } // JSON string containing the template structure
        public string TemplateImageUrl { get; set; } // Background image URL
        public string CreatedBy { get; set; }
        public string CreatedDate { get; set; }
        public string LastModified { get; set; }
        public bool IsDeleted { get; set; } = false;
        public bool IsActive { get; set; } = true;
    }

    public class User
    {
        public string FirebaseUid { get; set; }// Firebase Authentication UID
        public string UserId { get; set; }// Custom UID (e.g., U001, U002)
        public string Username { get; set; }
        public string PhoneNumber { get; set; }
        public string ProfilePictureUrl { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string JoinDate { get; set; } // Join date in ISO format
        public string SecurityQuestion { get; set; } // Security question
        public string SecurityAnswer { get; set; } // Security answer
        public string AccountStatus { get; set; } // Account status (active/disabled)
        public MemberData MemberData { get; set; } // Nested MemberData object
    }

    public class MemberData
    {
        public int Points { get; set; }
        public int Level { get; set; }
        public List<string> ApplicationID_List { get; set; }
        public List<string> BadgeID_List { get; set; }
        public List<string> CourseID_List { get; set; }
        public List<string> EventID_List { get; set; }
        public List<string> Interest_List { get; set; }
        public string TestimonialApplicationStatus { get; set; }
        public List<string> CompletedCourseID_List { get; set; }

        // Helper methods for badge status

        // Check if member has a specific badge
        public bool HasBadge(string badgeId)
        {
            if (BadgeID_List == null)
                return false;
            foreach (string badgeEntry in BadgeID_List)
            {
                string[] parts = badgeEntry.Split(':');
                if (parts.Length > 0 && parts[0] == badgeId)
                    return true;
            }
            return false;
        }

        // Get the status of a specific badge
        public string GetBadgeStatus(string badgeId)
        {
            if (BadgeID_List == null)
                return null;
            foreach (string badgeEntry in BadgeID_List)
            {
                string[] parts = badgeEntry.Split(':');
                if (parts.Length > 0 && parts[0] == badgeId)
                {
                    return parts.Length > 1 ? parts[1] : "unknown";
                }
            }
            return null; // Badge not found
        }

        // Update badge status (returns true if successful)
        public bool UpdateBadgeStatus(string badgeId, string newStatus)
        {
            if (BadgeID_List == null)
                return false;
            for (int i = 0; i < BadgeID_List.Count; i++)
            {
                string badgeEntry = BadgeID_List[i];
                string[] parts = badgeEntry.Split(':');
                if (parts.Length > 0 && parts[0] == badgeId)
                {
                    // Update the badge status
                    BadgeID_List[i] = $"{badgeId}:{newStatus}";
                    return true;
                }
            }
            return false; // Badge not found
        }

        // Get all badges with a specific status
        public List<string> GetBadgesByStatus(string status)
        {
            List<string> result = new List<string>();
            if (BadgeID_List == null)
                return result;
            foreach (string badgeEntry in BadgeID_List)
            {
                string[] parts = badgeEntry.Split(':');
                if (parts.Length > 1 && parts[1] == status)
                {
                    result.Add(parts[0]);
                }
            }
            return result;
        }

        // Helper method to check if a course is completed
        public bool IsCourseCompleted(string courseId)
        {
            return CompletedCourseID_List != null && CompletedCourseID_List.Contains(courseId);
        }

        // Helper method to get all completed courses
        public List<string> GetCompletedCourses()
        {
            return CompletedCourseID_List ?? new List<string>();
        }

        // Helper method to get all in-progress courses (enrolled but not completed)
        public List<string> GetInProgressCourses()
        {
            if (CourseID_List == null)
                return new List<string>();

            if (CompletedCourseID_List == null)
                return new List<string>(CourseID_List);

            return CourseID_List.Where(courseId => !CompletedCourseID_List.Contains(courseId)).ToList();
        }
    }

    public class EventFeedback
    {
        public string FeedbackID { get; set; }
        public string EventID { get; set; }
        public string UserID { get; set; }
        public string FeedbackText { get; set; }
        public string SubmittedDate { get; set; }
        public string UserName { get; set; } // Optional: to store username for display
    }

    public class IntroductionMedia
    {
        public string Title { get; set; }
        public string MediaType { get; set; } // "video", "pdf", or "image"
        public string MediaUrl { get; set; }
        public string UpdatedAt { get; set; }
    }

    public class CertificateTemplate
    {
        public string TemplateId { get; set; }
        public string TemplateUrl { get; set; }
        public string TemplateName { get; set; }
        public DateTime UploadedAt { get; set; }
        public string CreatedBy { get; set; }
        public int ImageWidth { get; set; }
        public int ImageHeight { get; set; }
        public Dictionary<string, FieldData> Fields { get; set; }
        public string Orientation { get; set; }

        // New properties for template type and usage
        public string TemplateType { get; set; } // "Testimonial", "Event", etc.
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        public bool IsTestimonialTemplate { get; set; } = false;

        public CertificateTemplate()
        {
            Fields = new Dictionary<string, FieldData>();
            UploadedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Converts the template to a Firebase-compatible dictionary
        /// </summary>
        public Dictionary<string, object> ToFirebaseObject()
        {
            var result = new Dictionary<string, object>
            {
                { "templateId", TemplateId },
                { "templateUrl", TemplateUrl },
                { "templateName", TemplateName },
                { "uploadedAt", UploadedAt.ToString("o") },
                { "createdBy", CreatedBy },
                { "imageWidth", ImageWidth },
                { "imageHeight", ImageHeight },
                {"orientation", Orientation },
                { "fields", ConvertFieldsToFirebaseFormat() },
                { "templateType", TemplateType },
                { "isActive", IsActive },
                { "isDeleted", IsDeleted },
                { "isTestimonialTemplate", IsTestimonialTemplate }
            };

            return result;
        }

        /// <summary>
        /// Creates a certificate template from Firebase data
        /// </summary>
        public static CertificateTemplate FromFirebaseObject(string templateId, Dictionary<string, object> data)
        {
            try
            {
                var template = new CertificateTemplate
                {
                    TemplateId = templateId,
                    TemplateUrl = data.ContainsKey("templateUrl") ? data["templateUrl"].ToString() : string.Empty,
                    TemplateName = data.ContainsKey("templateName") ? data["templateName"].ToString() : "Untitled Template",
                    CreatedBy = data.ContainsKey("createdBy") ? data["createdBy"].ToString() : string.Empty,
                    TemplateType = data.ContainsKey("templateType") ? data["templateType"].ToString() : "Generic",
                    IsActive = data.ContainsKey("isActive") ? Convert.ToBoolean(data["isActive"]) : true,
                    ImageWidth = data.ContainsKey("imageWidth") ? Convert.ToInt32(data["imageWidth"]) : 0,
                    ImageHeight = data.ContainsKey("imageHeight") ? Convert.ToInt32(data["imageHeight"]) : 0,
                    Orientation = data.ContainsKey("orientation") ? data["orientation"].ToString() : string.Empty,
                    IsDeleted = data.ContainsKey("isDeleted") ? Convert.ToBoolean(data["isDeleted"]) : false,
                    IsTestimonialTemplate = data.ContainsKey("isTestimonialTemplate") ?
                        Convert.ToBoolean(data["isTestimonialTemplate"]) : false
                };

                // Parse upload date
                if (data.ContainsKey("uploadedAt") && DateTime.TryParse(data["uploadedAt"].ToString(), out DateTime uploadedAt))
                {
                    template.UploadedAt = uploadedAt;
                }

                // Parse image dimensions
                if (data.ContainsKey("imageWidth"))
                {
                    template.ImageWidth = Convert.ToInt32(data["imageWidth"]);
                }

                if (data.ContainsKey("imageHeight"))
                {
                    template.ImageHeight = Convert.ToInt32(data["imageHeight"]);
                }

                // Parsing fields with JObject support
                if (data.ContainsKey("fields"))
                {
                    object fieldsObject = data["fields"];

                    // Handle both JObject and Dictionary
                    Dictionary<string, object> fieldsDict;
                    if (fieldsObject is JObject jObject)
                    {
                        // Convert JObject to Dictionary
                        fieldsDict = jObject.ToObject<Dictionary<string, object>>();
                    }
                    else if (fieldsObject is Dictionary<string, object> existingDict)
                    {
                        fieldsDict = existingDict;
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"Unexpected fields type: {fieldsObject?.GetType().Name}");
                        return template;
                    }

                    // Parse fields
                    foreach (var field in fieldsDict)
                    {
                        try
                        {
                            // Handle potential JObject for field value
                            Dictionary<string, object> fieldValueDict;
                            if (field.Value is JObject fieldJObject)
                            {
                                fieldValueDict = fieldJObject.ToObject<Dictionary<string, object>>();
                            }
                            else if (field.Value is Dictionary<string, object> existingFieldDict)
                            {
                                fieldValueDict = existingFieldDict;
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine($"Unexpected field value type for {field.Key}: {field.Value?.GetType().Name}");
                                continue;
                            }

                            template.Fields[field.Key] = FieldData.FromFirebaseObject(field.Key, fieldValueDict);
                        }
                        catch (Exception fieldEx)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error parsing field {field.Key}: {fieldEx.Message}");
                        }
                    }
                }

                return template;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in FromFirebaseObject: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                throw;
            }
        }

        /// <summary>
        /// Converts fields to a Firebase-compatible format
        /// </summary>
        private Dictionary<string, object> ConvertFieldsToFirebaseFormat()
        {
            var result = new Dictionary<string, object>();

            foreach (var field in Fields)
            {
                result[field.Key] = new Dictionary<string, object>
            {
                { "type", field.Value.Type },
                { "label", field.Value.Label },
                { "x", field.Value.Bounds.X },
                { "y", field.Value.Bounds.Y },
                { "width", field.Value.Bounds.Width },
                { "height", field.Value.Bounds.Height },
                { "isRequired", field.Value.IsRequired }
            };
            }

            return result;
        }
    }

    // Update your SiteSettings class in CombinedFirebaseHelper.cs
    public class SiteSettings
    {
        // Make all properties public
        public string OrganizationName { get; set; }
        public string LogoUrl { get; set; }
        public string Theme { get; set; }
        public string Font { get; set; }
        public string SystemLanguage { get; set; } // Renamed from DefaultLanguage for clarity
        public int MaxPointsPerLevel { get; set; }

        // Constructor with default values
        public SiteSettings()
        {
            OrganizationName = "Organization Name";
            LogoUrl = "";
            Theme = "theme1";
            Font = "Arial";
            SystemLanguage = "en"; // Default to English
            MaxPointsPerLevel = 100; // Default value
        }
    }


}
