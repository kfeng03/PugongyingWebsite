using System;
using System.Web.UI;
using RestSharp;
using Newtonsoft.Json;
using System.Configuration;
using System.Data.SqlClient;
using System.Text;

namespace PGY
{
    public partial class PaymentPrep : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (Request.QueryString["studentId"] != null)
                {
                    int studentId = Convert.ToInt32(Request.QueryString["studentId"]);
                    CreateBillPlzBill(studentId);
                }
                else
                {
                    Response.Redirect("RegistrationForm.aspx");
                }
            }
        }

        private void CreateBillPlzBill(int studentId)
        {
            // 从配置文件获取 Billplz 设置
            string apiKey = ConfigurationManager.AppSettings["BillplzApiKey"];
            string collectionId = ConfigurationManager.AppSettings["BillplzCollectionId"];
            bool isSandbox = Convert.ToBoolean(ConfigurationManager.AppSettings["BillplzUseSandbox"]);

            // 动态生成 URL，避免 ngrok 链接变化的问题
            string currentDomain = Request.Url.Scheme + "://" + Request.Url.Authority;
            string callbackUrl = currentDomain + "/Callback.aspx";
            string returnUrl = currentDomain + "/PaymentReturn.aspx?studentId=" + studentId;

            string billplzBaseUrl = isSandbox ?
                "https://www.billplz-sandbox.com/api/v3/bills" :
                "https://www.billplz.com/api/v3/bills";

            // 从数据库获取学生信息
            var student = GetStudentData(studentId);
            if (student == null)
            {
                ClientScript.RegisterStartupScript(this.GetType(), "alert", "alert('Student not found.');", true);
                return;
            }

            try
            {
                var client = new RestClient(billplzBaseUrl);
                var request = new RestRequest("", Method.Post);

                // Billplz 使用 Basic Auth，格式为 API_KEY:
                string auth = Convert.ToBase64String(Encoding.UTF8.GetBytes(apiKey + ":"));
                request.AddHeader("Authorization", "Basic " + auth);
                request.AddHeader("Accept", "application/json");

                // 添加调试信息
                System.Diagnostics.Debug.WriteLine("Billplz API URL: " + billplzBaseUrl);
                System.Diagnostics.Debug.WriteLine("Callback URL: " + callbackUrl);
                System.Diagnostics.Debug.WriteLine("Return URL: " + returnUrl);

                // 添加必要参数
                request.AddParameter("collection_id", collectionId);
                request.AddParameter("email", student.Email);
                request.AddParameter("mobile", student.PhoneNumber);
                request.AddParameter("name", student.EnglishName);
                request.AddParameter("amount", "10000"); // RM 100.00 (以分计算)
                request.AddParameter("callback_url", callbackUrl);
                request.AddParameter("description", "PGY Camp Registration - " + student.EnglishName);
                request.AddParameter("redirect_url", returnUrl);
                request.AddParameter("reference", studentId.ToString()); // 用于回调识别

                // 设置请求超时
                //client.Timeout = 30000; // 30秒

                RestResponse response = client.Execute(request);

                // 记录完整的响应信息
                System.Diagnostics.Debug.WriteLine("Billplz Response Status: " + response.StatusCode);
                System.Diagnostics.Debug.WriteLine("Billplz Response Content: " + response.Content);
                System.Diagnostics.Debug.WriteLine("Billplz Error: " + response.ErrorMessage);

                if (response.IsSuccessful && !string.IsNullOrEmpty(response.Content))
                {
                    // 解析 Billplz 响应
                    var billResponse = JsonConvert.DeserializeObject<BillplzResponse>(response.Content);

                    if (billResponse != null && !string.IsNullOrEmpty(billResponse.url))
                    {
                        // 更新数据库中的 PaymentID
                        UpdateStudentPaymentId(studentId, billResponse.id);

                        // 重定向到 Billplz 支付页面
                        Response.Redirect(billResponse.url);
                    }
                    else
                    {
                        ClientScript.RegisterStartupScript(this.GetType(), "alert", "alert('Invalid response from payment gateway.');", true);
                    }
                }
                else
                {
                    string errorMessage = "Failed to create payment.";
                    if (!string.IsNullOrEmpty(response.ErrorMessage))
                    {
                        errorMessage += " Error: " + response.ErrorMessage;
                    }
                    if (!string.IsNullOrEmpty(response.Content))
                    {
                        errorMessage += " Content: " + response.Content;
                    }

                    ClientScript.RegisterStartupScript(this.GetType(), "alert", $"alert('{errorMessage}');", true);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Exception in CreateBillPlzBill: " + ex.ToString());
                ClientScript.RegisterStartupScript(this.GetType(), "alert", $"alert('Error: {ex.Message}');", true);
            }
        }
        private StudentData GetStudentData(int studentId)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["StudentRegistrationDB"].ConnectionString;

            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT EnglishName, ChineseName, Email, PhoneNumber FROM Students WHERE StudentID = @StudentID";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@StudentID", studentId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new StudentData
                            {
                                EnglishName = reader["EnglishName"].ToString(),
                                ChineseName = reader["ChineseName"].ToString(),
                                Email = reader["Email"].ToString(),
                                PhoneNumber = reader["PhoneNumber"].ToString()
                            };
                        }
                    }
                }
            }
            return null;
        }

        private void UpdateStudentPaymentId(int studentId, string paymentId)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["StudentRegistrationDB"].ConnectionString;

            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "UPDATE Students SET PaymentID = @PaymentID WHERE StudentID = @StudentID";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@PaymentID", paymentId);
                    cmd.Parameters.AddWithValue("@StudentID", studentId);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }

    public class BillplzResponse
    {
        public string id { get; set; }
        public string url { get; set; }
    }

    public class StudentData
    {
        public string EnglishName { get; set; }
        public string ChineseName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
    }
}