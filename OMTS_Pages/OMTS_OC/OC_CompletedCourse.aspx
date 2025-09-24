<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="OC_CompletedCourse.aspx.cs" 
Inherits="fyp.OC_CompletedCourse" MasterPageFile="~/Site1.Master" Async="true" %>

<asp:Content ID="ContentTitle" ContentPlaceHolderID="title" runat="server">Completed Courses</asp:Content>

<asp:Content ID="ContentStyle" ContentPlaceHolderID="css" runat="server">
    <style>
        .course-list {
            display: flex;
            flex-wrap: wrap;
            gap: 20px;
            margin-top: 20px;
        }

        .course-item {
            width: 300px;
            background-color: #ffffff;
            border-radius: 8px;
            box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
            overflow: hidden;
            transition: transform 0.2s ease, box-shadow 0.2s ease;
            display: flex;
            flex-direction: column;
        }

        .course-item:hover {
            transform: translateY(-5px);
            box-shadow: 0 8px 16px rgba(0, 0, 0, 0.1);
        }

        .course-image {
            width: 100%;
            height: 160px;
            object-fit: cover;
        }

        .course-content {
            padding: 20px;
            flex-grow: 1;
            display: flex;
            flex-direction: column;
        }

        .course-title {
            font-size: 18px;
            font-weight: 600;
            color: #333;
            margin-bottom: 10px;
        }

        .course-category {
            display: inline-block;
            background-color: #e3f2fd;
            color: #0d6efd;
            padding: 4px 10px;
            border-radius: 50px;
            font-size: 12px;
            margin-bottom: 15px;
        }

        .course-completion {
            display: flex;
            align-items: center;
            margin-top: auto;
            margin-bottom: 10px;
        }

        .completion-date {
            font-size: 14px;
            color: #666;
        }

        .completion-badge {
            margin-left: auto;
            background-color: #4CAF50;
            color: white;
            font-size: 12px;
            font-weight: 500;
            padding: 4px 8px;
            border-radius: 4px;
        }

        .action-buttons {
            display: flex;
            gap: 10px;
        }

        .btn {
            flex: 1;
            padding: 10px;
            border: none;
            border-radius: 6px;
            cursor: pointer;
            font-weight: 500;
            font-size: 14px;
            text-align: center;
            transition: background-color 0.3s ease;
        }

        .btn-primary {
            background-color: #007bff;
            color: white;
        }

        .btn-primary:hover {
            background-color: #0069d9;
        }

        .btn-success {
            background-color: #28a745;
            color: white;
        }

        .btn-success:hover {
            background-color: #218838;
        }

        .empty-message {
            width: 100%;
            padding: 30px;
            text-align: center;
            background-color: #f8f9fa;
            border-radius: 8px;
            font-size: 16px;
            color: #6c757d;
            margin-top: 20px;
        }

        .certificate-container {
            position: fixed;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            background-color: rgba(0, 0, 0, 0.7);
            display: flex;
            justify-content: center;
            align-items: center;
            z-index: 1000;
        }

        .certificate {
            width: 90%;
            max-width: 800px;
            background-color: #fff;
            border-radius: 8px;
            padding: 40px;
            text-align: center;
            position: relative;
        }

        .certificate-title {
            font-size: 24px;
            font-weight: bold;
            margin-bottom: 20px;
        }

        .certificate-content {
            margin-bottom: 30px;
            line-height: 1.6;
        }

        .certificate-close {
            position: absolute;
            top: 10px;
            right: 15px;
            font-size: 24px;
            cursor: pointer;
            color: #555;
        }

        .certificate-print {
            padding: 10px 20px;
            background-color: #007bff;
            color: white;
            border: none;
            border-radius: 4px;
            cursor: pointer;
            margin-top: 20px;
        }
    </style>
</asp:Content>

<asp:Content ID="ContentMain" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container">
        <!-- Sidebar -->
        <div class="sidebar">
            <asp:Button ID="btnMyCourse" runat="server" CssClass="sidebar-btn" 
                Text="My Courses" PostBackUrl="~/OMTS_Pages/OMTS_OC/OC_Courses.aspx" />
            <asp:Button ID="btnJoinCourse" runat="server" CssClass="sidebar-btn" 
                Text="Join Course" PostBackUrl="~/OMTS_Pages/OMTS_OC/OC_JoinCourse.aspx" />
            <asp:Button ID="btnCompletedCourse" runat="server" CssClass="sidebar-btn active" 
                Text="Completed Courses" PostBackUrl="~/OMTS_Pages/OMTS_OC/OC_CompletedCourse.aspx" />
            <asp:Button ID="btnRecommendations" runat="server" CssClass="sidebar-btn" 
                Text="Recommendations" PostBackUrl="~/OMTS_Pages/OMTS_OC/OC_CourseRecommendations.aspx" 
                Visible='<%# Session["UserRole"]?.ToString() == "Member" || Session["UserRole"]?.ToString() == "Admin" %>' />
            <asp:Button ID="btnCourseManagement" runat="server" CssClass="sidebar-btn" 
                Text="Course Management" PostBackUrl="~/OMTS_Pages/OMTS_OC/OC_CourseManagement.aspx" Visible="false" />
        </div>

        <!-- Main Content Area -->
        <div class="main-content">
            <h2 class="page-header">Completed Courses</h2>

            <!-- Error/Success Message -->
            <asp:Label ID="lblMessage" runat="server" CssClass="message"></asp:Label>

            <!-- Completed Course List -->
            <div class="course-list">
                <asp:Label ID="lblNoCourses" runat="server" 
                    Text="You haven't completed any courses yet." 
                    CssClass="empty-message" Visible="false"></asp:Label>

                <asp:Repeater ID="rptCompletedCourses" runat="server">
                    <ItemTemplate>
                        <div class="course-item">
                            <div class="course-content">
                                <div class="course-title"><%# Eval("CourseName") %></div>
                                <div class="course-category"><%# Eval("CourseCategory") %></div>
                                <div class="course-completion">
                                    <span class="completion-date">
                                        Completed on: <%# Eval("CompletionDate", "{0:MMM dd, yyyy}") %>
                                    </span>
                                    <span class="completion-badge">100% Complete</span>
                                </div>
                                <div class="action-buttons">
                                    <asp:Button ID="btnViewCourse" runat="server" 
                                        Text="View Course" 
                                        CssClass="btn btn-primary"
                                        CommandArgument='<%# Eval("CourseID") %>' 
                                        OnClick="btnViewCourse_Click" />
                                    <asp:Button ID="btnViewCertificate" runat="server" 
                                        Text="View Certificate" 
                                        CssClass="btn btn-success"
                                        CommandArgument='<%# Eval("CourseID") %>' 
                                        OnClick="btnViewCertificate_Click" />
                                </div>
                            </div>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
            </div>

            <!-- Certificate Modal -->
            <asp:Panel ID="pnlCertificate" runat="server" CssClass="certificate-container" Visible="false">
                <div class="certificate">
                    <span class="certificate-close" onclick="closeCertificate()">×</span>
                    <div class="certificate-title">Certificate of Completion</div>
                    <div class="certificate-content">
                        This is to certify that
                        <br />
                        <strong><asp:Literal ID="litUserName" runat="server"></asp:Literal></strong>
                        <br />
                        has successfully completed the course
                        <br />
                        <strong><asp:Literal ID="litCourseName" runat="server"></asp:Literal></strong>
                        <br />
                        on
                        <br />
                        <asp:Literal ID="litCompletionDate" runat="server"></asp:Literal>
                    </div>
                    <button class="certificate-print" onclick="printCertificate()">
                        Print Certificate
                    </button>
                </div>
            </asp:Panel>
        </div>
    </div>

    <script>
        function closeCertificate() {
            document.querySelector('.certificate-container').style.display = 'none';
        }

        function printCertificate() {
            window.print();
        }
    </script>
</asp:Content>