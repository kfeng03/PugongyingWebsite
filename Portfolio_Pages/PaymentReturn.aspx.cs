using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.UI;

namespace PGY
{
    public partial class PaymentReturn : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Billplz 使用这种格式：billplz[paid]=true
                string paidStatus = Request.QueryString["billplz[paid]"];
                string billId = Request.QueryString["billplz[id]"];
                string studentIdStr = Request.QueryString["studentId"];

                // 调试：显示所有参数
                System.Diagnostics.Debug.WriteLine("PaymentReturn Parameters:");
                foreach (string key in Request.QueryString.AllKeys)
                {
                    System.Diagnostics.Debug.WriteLine(key + " = " + Request.QueryString[key]);
                }

                if (!string.IsNullOrEmpty(studentIdStr) && int.TryParse(studentIdStr, out int studentId))
                {
                    if (paidStatus == "true")
                    {
                        // 支付成功处理
                        lblMessage.Text = "Payment Successful! Thank you for your registration.";
                        lblMessage.CssClass = "text-success";

                        // 更新数据库
                        UpdatePaymentStatus(studentId, billId, "paid", 100.00m);
                    }
                    else
                    {
                        // 支付失败处理
                        lblMessage.Text = "Payment not completed. Please try again.";
                        lblMessage.CssClass = "text-danger";

                        // 更新数据库
                        UpdatePaymentStatus(studentId, billId, "failed", 0.00m);
                    }

                    DisplayStudentInfo(studentId);
                }
            }
        }
        private void UpdatePaymentStatus(int studentId, string billId, string paymentStatus, decimal amount)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["StudentRegistrationDB"].ConnectionString;

            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = @"
                    UPDATE Students 
                    SET PaymentID = @PaymentID, 
                        PaymentStatus = @PaymentStatus,
                        PaymentAmount = @PaymentAmount
                    WHERE StudentID = @StudentID";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@PaymentID", billId);
                    cmd.Parameters.AddWithValue("@PaymentStatus", paymentStatus);
                    cmd.Parameters.AddWithValue("@PaymentAmount", amount);
                    cmd.Parameters.AddWithValue("@StudentID", studentId);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void DisplayStudentInfo(int studentId)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["StudentRegistrationDB"].ConnectionString;

            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT EnglishName, ChineseName, Email, PaymentStatus FROM Students WHERE StudentID = @StudentID";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@StudentID", studentId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            studentInfo.Visible = true;
                            ltStudentName.Text = reader["EnglishName"].ToString() + " (" + reader["ChineseName"].ToString() + ")";
                            ltEmail.Text = reader["Email"].ToString();
                            ltStatus.Text = reader["PaymentStatus"].ToString();
                        }
                    }
                }
            }
        }

        protected void btnReturnHome_Click(object sender, EventArgs e)
        {
            Response.Redirect("Homepage.aspx");
        }
    }
}