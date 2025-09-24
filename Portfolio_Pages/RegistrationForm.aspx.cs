using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text;


namespace PGY
{
    public partial class WebForm3 : System.Web.UI.Page
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["StudentRegistrationDB"].ConnectionString;
        
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Initialize form
            }
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            try
            {
                if (Page.IsValid && ValidateForm())
                {
                    // Save to database first
                    int studentId = SaveStudentData();
                    
                    if (studentId > 0)
                    {
                        // Redirect to BillPlz for payment
                        RedirectToBillPlz(studentId);
                    }
                    else
                    {
                        lblMessage.Text = "Registration failed. Please try again.";
                    }
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = "An error occurred: " + ex.Message;
            }
        }

        private bool ValidateForm()
        {
            bool isValid = true;
            StringBuilder errors = new StringBuilder();

            // Validate gender selection
            if (!rbMale.Checked && !rbFemale.Checked)
            {
                errors.AppendLine("Please select gender.");
                isValid = false;
            }

            // Validate uniform size
            if (string.IsNullOrEmpty(ddlUniformSize.SelectedValue))
            {
                errors.AppendLine("Please select uniform size.");
                isValid = false;
            }

            // Validate dietary preference
            if (!rbOmnivore.Checked && !rbVegetarian.Checked && !rbHalal.Checked)
            {
                errors.AppendLine("Please select dietary preference.");
                isValid = false;
            }

            if (!isValid)
            {
                lblMessage.Text = errors.ToString().Replace("\n", "<br/>");
            }

            return isValid;
        }

        private int SaveStudentData()
        {
            int studentId = 0;
            
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = @"
                        INSERT INTO Students (
                            ChineseName, EnglishName, Gender, Age, IDNumber, Address, City, StateProvince, ZipPostal,
                            School, Email, PhoneNumber, UniformSize, DietaryPreference, FoodAllergies, MedicalConditions,
                            EmergencyContactName, EmergencyContactPhone, EmergencyContactRelation, PaymentAmount
                        ) VALUES (
                            @ChineseName, @EnglishName, @Gender, @Age, @IDNumber, @Address, @City, @StateProvince, @ZipPostal,
                            @School, @Email, @PhoneNumber, @UniformSize, @DietaryPreference, @FoodAllergies, @MedicalConditions,
                            @EmergencyContactName, @EmergencyContactPhone, @EmergencyContactRelation, @PaymentAmount
                        );
                        SELECT SCOPE_IDENTITY();";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        // Add parameters
                        cmd.Parameters.AddWithValue("@ChineseName", txtChineseName.Text.Trim());
                        cmd.Parameters.AddWithValue("@EnglishName", txtEnglishName.Text.Trim());
                        cmd.Parameters.AddWithValue("@Gender", rbMale.Checked ? "Male" : "Female");
                        cmd.Parameters.AddWithValue("@Age", Convert.ToInt32(txtAge.Text));
                        cmd.Parameters.AddWithValue("@IDNumber", txtIDNumber.Text.Trim());
                        cmd.Parameters.AddWithValue("@Address", txtAddress.Text.Trim());
                        cmd.Parameters.AddWithValue("@City", txtCity.Text.Trim());
                        cmd.Parameters.AddWithValue("@StateProvince", txtState.Text.Trim());
                        cmd.Parameters.AddWithValue("@ZipPostal", txtZip.Text.Trim());
                        cmd.Parameters.AddWithValue("@School", txtSchool.Text.Trim());
                        cmd.Parameters.AddWithValue("@Email", txtEmail.Text.Trim());
                        cmd.Parameters.AddWithValue("@PhoneNumber", txtPhone.Text.Trim());
                        cmd.Parameters.AddWithValue("@UniformSize", ddlUniformSize.SelectedValue);
                        cmd.Parameters.AddWithValue("@DietaryPreference", GetSelectedDietaryPreference());
                        cmd.Parameters.AddWithValue("@FoodAllergies", string.IsNullOrEmpty(txtFoodAllergies.Text.Trim()) ? (object)DBNull.Value : txtFoodAllergies.Text.Trim());
                        cmd.Parameters.AddWithValue("@MedicalConditions", string.IsNullOrEmpty(txtMedicalConditions.Text.Trim()) ? (object)DBNull.Value : txtMedicalConditions.Text.Trim());
                        cmd.Parameters.AddWithValue("@EmergencyContactName", txtEmergencyName.Text.Trim());
                        cmd.Parameters.AddWithValue("@EmergencyContactPhone", txtEmergencyPhone.Text.Trim());
                        cmd.Parameters.AddWithValue("@EmergencyContactRelation", txtEmergencyRelation.Text.Trim());
                        cmd.Parameters.AddWithValue("@PaymentAmount", 100.00); // Registration fee

                        object result = cmd.ExecuteScalar();
                        if (result != null)
                        {
                            studentId = Convert.ToInt32(result);
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Database error: " + ex.Message);
                }
            }

            return studentId;
        }

        private string GetSelectedDietaryPreference()
        {
            if (rbOmnivore.Checked) return "Omnivore";
            if (rbVegetarian.Checked) return "Vegetarian";
            if (rbHalal.Checked) return "Halal";
            return "";
        }

        private void RedirectToBillPlz(int studentId)
        {
            // 直接重定向到 PaymentPrep 页面，在那里创建账单
            Response.Redirect($"PaymentPrep.aspx?studentId={studentId}");
        }

        // Method to update payment status (called by BillPlz webhook)
        public static void UpdatePaymentStatus(int studentId, string billId, string paymentStatus, decimal amount)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["StudentRegistrationDB"].ConnectionString;
            
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = @"
                    UPDATE Students 
                    SET PaymentID = @PaymentID, 
                        PaymentStatus = @PaymentStatus,
                        PaymentAmount = @PaymentAmount
                    WHERE StudentID = @StudentID";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@PaymentID", billId);
                    cmd.Parameters.AddWithValue("@PaymentStatus", paymentStatus);
                    cmd.Parameters.AddWithValue("@PaymentAmount", amount);
                    cmd.Parameters.AddWithValue("@StudentID", studentId);
                    
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}