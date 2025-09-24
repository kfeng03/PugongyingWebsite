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
    public partial class OC_EditCourse : Page
    {
        private FirebaseHelper firebaseHelper = new FirebaseHelper();
        private string courseId;

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

            // Get course ID from query string  
            courseId = Request.QueryString["courseId"];
            if (string.IsNullOrEmpty(courseId))
            {
                lblMessage.Text = "Course ID is required.";
                lblMessage.ForeColor = Color.Red;
                return;
            }

            if (!IsPostBack)
            {
                PopulateCategoryDropdown();
                // Clear any previously added materials
                Session["TempDriveMaterials"] = new Dictionary<string, string>();

                // Load course details  
                await LoadCourseDetails(courseId);

                UpdateMaterialsList();
            }
        }

        private void PopulateCategoryDropdown()
        {
            ddlCourseCategory.Items.Clear();
            ddlCourseCategory.Items.Add(new ListItem("Select Category", ""));

            foreach (string category in CategoryUtility.GetAvailableCategories())
            {
                ddlCourseCategory.Items.Add(new ListItem(category, category));
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
        protected void btnAddDriveLink_Click(object sender, EventArgs e)
        {
            string driveLink = txtDriveLink.Text.Trim();
            string materialName = txtDriveLinkName.Text.Trim();

            // Input validation
            if (string.IsNullOrEmpty(driveLink))
            {
                lblMessage.Text = "Please enter a Google Drive link.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                lblMessage.Visible = true;
                return;
            }

            if (string.IsNullOrEmpty(materialName))
            {
                lblMessage.Text = "Please enter a name for this material.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                lblMessage.Visible = true;
                return;
            }

            // Validate it's a Google Drive link
            if (!driveLink.Contains("drive.google.com"))
            {
                lblMessage.Text = "Please enter a valid Google Drive link.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                lblMessage.Visible = true;
                return;
            }

            // Standardize and convert the link to a view link
            driveLink = StandardizeGoogleDriveLink(driveLink);

            // Ensure the session storage exists
            if (Session["TempDriveMaterials"] == null)
            {
                Session["TempDriveMaterials"] = new Dictionary<string, string>();
            }

            // Add to temporary materials dictionary
            var tempMaterials = (Dictionary<string, string>)Session["TempDriveMaterials"];

            // Check if this material name already exists
            if (tempMaterials.ContainsKey(materialName))
            {
                // Update the existing entry
                tempMaterials[materialName] = driveLink;
                lblMessage.Text = "Material updated successfully.";
            }
            else
            {
                // Add new entry
                tempMaterials.Add(materialName, driveLink);
                lblMessage.Text = "Material added successfully.";
            }

            // Update the displayed list
            UpdateMaterialsList();

            // Clear the input fields for next entry
            txtDriveLink.Text = "";
            txtDriveLinkName.Text = "";

            lblMessage.ForeColor = System.Drawing.Color.Green;
            lblMessage.Visible = true;

            // Debug to ensure materials are being stored
            System.Diagnostics.Debug.WriteLine($"Added/updated material: {materialName} => {driveLink}");
            System.Diagnostics.Debug.WriteLine($"Current material count: {tempMaterials.Count}");
        }
        private void UpdateMaterialsList()
        {
            if (Session["TempDriveMaterials"] != null)
            {
                var tempMaterials = (Dictionary<string, string>)Session["TempDriveMaterials"];
                rptAddedMaterials.DataSource = tempMaterials.Keys.ToList();
                rptAddedMaterials.DataBind();
            }
            else
            {
                rptAddedMaterials.DataSource = null;
                rptAddedMaterials.DataBind();
            }
        }
        protected void rptAddedMaterials_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "Remove")
            {
                string materialName = e.CommandArgument.ToString();

                if (Session["TempDriveMaterials"] != null)
                {
                    var tempMaterials = (Dictionary<string, string>)Session["TempDriveMaterials"];

                    if (tempMaterials.ContainsKey(materialName))
                    {
                        tempMaterials.Remove(materialName);

                        // Update the displayed list
                        UpdateMaterialsList();

                        // Show success message
                        lblMessage.Text = "Material removed successfully.";
                        lblMessage.ForeColor = System.Drawing.Color.Green;
                        lblMessage.Visible = true;

                        System.Diagnostics.Debug.WriteLine($"Removed material: {materialName}");
                        System.Diagnostics.Debug.WriteLine($"Current material count: {tempMaterials.Count}");
                    }
                }
            }
        }
        private async Task LoadCourseDetails(string courseId)
        {
            try
            {
                // Fetch course details from Firebase  
                var course = await firebaseHelper.GetCourseById(courseId);

                if (course == null)
                {
                    lblMessage.Text = "Course not found.";
                    lblMessage.ForeColor = Color.Red;
                    return;
                }

                // Populate form fields  
                txtCourseName.Text = course.CourseName;
                txtCourseDescription.Text = course.CourseDescription;

                // Set course category
                if (!string.IsNullOrEmpty(course.CourseCategory))
                {
                    ListItem categoryItem = ddlCourseCategory.Items.FindByValue(course.CourseCategory);
                    if (categoryItem != null)
                    {
                        ddlCourseCategory.SelectedValue = categoryItem.Value;
                    }
                }

                // Set max students  
                ListItem maxStudentsItem = ddlMaxStudents.Items.FindByValue(course.NumberOfStudents.ToString());
                if (maxStudentsItem != null)
                {
                    ddlMaxStudents.SelectedValue = maxStudentsItem.Value;
                }

                // Initialize materials list from existing course materials
                var tempMaterials = new Dictionary<string, string>();
                Session["TempDriveMaterials"] = tempMaterials;

                if (course.CourseMaterials != null && course.CourseMaterials.Count > 0)
                {
                    // Find Google Drive links in the materials
                    foreach (var material in course.CourseMaterials)
                    {
                        if (material.Value.Contains("drive.google.com"))
                        {
                            tempMaterials[material.Key] = material.Value;
                        }
                    }

                    // Update the displayed list
                    UpdateMaterialsList();
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = $"Error loading course details: {ex.Message}";
                lblMessage.ForeColor = Color.Red;
                System.Diagnostics.Debug.WriteLine($"Course Load Error: {ex}");
            }
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

        protected async void btnSaveCourse_Click(object sender, EventArgs e)
        {
            try
            {
                // Validate required fields  
                if (string.IsNullOrWhiteSpace(txtCourseName.Text))
                {
                    lblMessage.Text = "Course name is required.";
                    lblMessage.ForeColor = Color.Red;
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtCourseDescription.Text))
                {
                    lblMessage.Text = "Course description is required.";
                    lblMessage.ForeColor = Color.Red;
                    return;
                }

                // Get existing course to preserve materials that aren't Google Drive links
                var existingCourse = await firebaseHelper.GetCourseById(courseId);
                Dictionary<string, string> existingMaterials = new Dictionary<string, string>();

                if (existingCourse?.CourseMaterials != null)
                {
                    foreach (var material in existingCourse.CourseMaterials)
                    {
                        // Keep materials that aren't Google Drive links
                        if (!material.Value.Contains("drive.google.com"))
                        {
                            existingMaterials[material.Key] = material.Value;
                        }
                    }
                }

                // Prepare course update object  
                var course = new Course
                {
                    CourseID = courseId,
                    CourseName = txtCourseName.Text.Trim(),
                    CourseDescription = txtCourseDescription.Text.Trim(),
                    CourseCategory = ddlCourseCategory.SelectedValue,
                    NumberOfStudents = int.Parse(ddlMaxStudents.SelectedValue),
                    CourseMaterials = existingMaterials,
                    CoursePictureUrl = existingCourse?.CoursePictureUrl
                };

                

                // Add Google Drive materials if any were added
                if (Session["TempDriveMaterials"] != null)
                {
                    var tempMaterials = (Dictionary<string, string>)Session["TempDriveMaterials"];
                    foreach (var material in tempMaterials)
                    {
                        course.CourseMaterials[material.Key] = material.Value;
                    }
                }

                

                // Update course in Firebase  
                await firebaseHelper.UpdateCourse(course);

                // Clear temp materials
                Session["TempDriveMaterials"] = null;

                // Set success message  
                Session["SuccessMessage"] = "Course updated successfully!";
                Response.Redirect("OC_CourseManagement.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
            }
            catch (Exception ex)
            {
                lblMessage.Text = $"Error updating course: {ex.Message}";
                lblMessage.ForeColor = Color.Red;
                System.Diagnostics.Debug.WriteLine($"Course Update Error: {ex}");
            }
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            // Clear temp materials
            Session["TempDriveMaterials"] = null;

            // Redirect back to Course Management page  
            Response.Redirect("OC_CourseManagement.aspx", false);
            Context.ApplicationInstance.CompleteRequest();
        }
    }
}