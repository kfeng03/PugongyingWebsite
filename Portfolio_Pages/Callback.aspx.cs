using System;
using System.IO;
using System.Web;
using System.Configuration;

namespace PGY
{
    public partial class Callback : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // Billplz 发送 POST 请求到回调URL
            if (Request.HttpMethod == "POST")
            {
                try
                {
                    // 读取原始POST数据
                    string rawData;
                    using (var reader = new StreamReader(Request.InputStream))
                    {
                        rawData = reader.ReadToEnd();
                    }

                    // 解析查询字符串格式的数据
                    var collection = HttpUtility.ParseQueryString(rawData);

                    string billId = collection["studentId"];
                    string state = collection["state"]; // "paid", "due", etc.
                    string paid = collection["paid"]; // "true" or "false"
                    string reference = collection["reference"]; // 这是我们传入的studentId
                    string amount = collection["amount"];

                    // 如果 paid=true，则状态为 paid
                    if (paid == "true" || paid == "1")
                    {
                        state = "paid";
                    }

                    if (!string.IsNullOrEmpty(reference) && int.TryParse(reference, out int studentId))
                    {
                        decimal paymentAmount = !string.IsNullOrEmpty(amount) ?
                            Convert.ToDecimal(amount) / 100 : 100.00m; // 金额以分计算，需要除以100

                        // 更新支付状态
                        UpdatePaymentStatus(studentId, billId, state, paymentAmount);
                    }

                    // 返回 200 OK 给 Billplz
                    Response.StatusCode = 200;
                    Response.Write("OK");
                    Response.End();
                }
                catch (Exception ex)
                {
                    // 记录错误
                    Response.StatusCode = 500;
                    Response.Write("Error: " + ex.Message);
                }
            }
        }
        private void UpdatePaymentStatus(int studentId, string billId, string paymentStatus, decimal amount)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["StudentRegistrationDB"].ConnectionString;

            using (var conn = new System.Data.SqlClient.SqlConnection(connectionString))
            {
                conn.Open();
                string query = @"
                    UPDATE Students 
                    SET PaymentID = @PaymentID, 
                        PaymentStatus = @PaymentStatus,
                        PaymentAmount = @PaymentAmount,
                        RegistrationDate = GETDATE()
                    WHERE StudentID = @StudentID";

                using (var cmd = new System.Data.SqlClient.SqlCommand(query, conn))
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