<%@ Page Language="C#" Async="true" AutoEventWireup="true" CodeBehind="OC_Courses.aspx.cs" Inherits="fyp.OC_Courses" MasterPageFile="~/Site1.Master" %>
<asp:Content ID="ContentTitle" ContentPlaceHolderID="title" runat="server">My Courses</asp:Content>

<asp:Content ID="ContentStyle" ContentPlaceHolderID="css" runat="server">
    <style>
        .course-list {
            display: flex;
            flex-wrap: wrap;
            gap: 20px;
            margin-top: 20px;
        }

        .course-item {
            width: 250px;
            padding: 15px;
            background-color: #f9f9f9;
            border: 1px solid #ddd;
            border-radius: 5px;
            display: flex;
            flex-direction: column;
            justify-content: space-between;
            transition: transform 0.2s ease, box-shadow 0.2s ease;
        }

        .course-item:hover {
            transform: translateY(-5px);
            box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
        }

        .course-title {
            font-size: 18px;
            font-weight: bold;
            margin-bottom: 10px;
        }

        .course-details {
            font-size: 14px;
            color: #666;
            margin-bottom: 15px;
        }

        .empty-message {
            width: 100%;
            text-align: center;
            padding: 20px;
            background-color: #f8f9fa;
            border-radius: 8px;
            color: #6c757d;
        }

        .btn-view {
            background-color: #007bff;
            color: white;
            border: none;
            padding: 8px 16px;
            border-radius: 4px;
            cursor: pointer;
            transition: background-color 0.3s ease;
            margin-top: 10px;
        }

        .btn-view:hover {
            background-color: #0056b3;
        }
        
        .status-badge {
            display: inline-block;
            padding: 4px 8px;
            border-radius: 4px;
            font-size: 12px;
            margin-top: 5px;
            background-color: #28a745;
            color: white;
        }
    </style>
</asp:Content>

<asp:Content ID="ContentMain" ContentPlaceHolderID="MainContent" runat="server">
    <body>
        <div class="container">
            <!-- Sidebar -->
            <div class="sidebar">
                <asp:Button ID="btnMyCourse" runat="server" CssClass='sidebar-btn active' 
                    Text="My Courses" PostBackUrl="~/OMTS_Pages/OMTS_OC/OC_Courses.aspx" />
                <asp:Button ID="btnJoinCourse" runat="server" CssClass='sidebar-btn' 
                    Text="Join Course" PostBackUrl="~/OMTS_Pages/OMTS_OC/OC_JoinCourse.aspx" />
                <asp:Button ID="btnCompletedCourse" runat="server" CssClass='sidebar-btn' 
                    Text="Completed Courses" PostBackUrl="~/OMTS_Pages/OMTS_OC/OC_CompletedCourse.aspx" />
                <asp:Button ID="btnRecommendations" runat="server" CssClass="sidebar-btn" 
                    Text="Recommendations" PostBackUrl="~/OMTS_Pages/OMTS_OC/OC_CourseRecommendations.aspx" 
                    Visible='<%# Session["UserRole"]?.ToString() == "Member" || Session["UserRole"]?.ToString() == "Admin" %>' />
                <asp:Button ID="btnCourseManagement" runat="server" CssClass='sidebar-btn' 
                    Text="Course Management" PostBackUrl="~/OMTS_Pages/OMTS_OC/OC_CourseManagement.aspx" 
                    Visible="false" />
            </div>

            <!-- Main Content Area -->
            <div class="main-content">
                <h2 class="page-header">My Courses</h2>

                <!-- Error/Success Message -->
                <asp:Label ID="lblMessage" runat="server" CssClass="message"></asp:Label>

                <!-- Course List Section -->
                <div class="course-list">
                    <asp:Label ID="lblNoCourses" runat="server" 
                        Text="You are not enrolled in any courses yet." 
                        CssClass="empty-message" Visible="false"></asp:Label>

                    <asp:Repeater ID="rptMyCourses" runat="server">
                        <ItemTemplate>
                            <div class="course-item">
                                <div>
                                    <div class="course-title">
                                        <%# Eval("CourseName") %>
                                    </div>
                                    <div class="course-details">
                                        Category: <%# Eval("CourseCategory") %>
                                    </div>
                                    <div class="status-badge">
                                        Enrolled
                                    </div>
                                </div>
                                <asp:Button 
                                    ID="btnViewCourse" 
                                    runat="server" 
                                    Text="View Course" 
                                    CssClass="btn-view"
                                    CommandArgument='<%# Eval("CourseID") %>'
                                    OnClick="btnViewCourse_Click" />
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>
            </div>
        </div>
    </body>
</asp:Content>