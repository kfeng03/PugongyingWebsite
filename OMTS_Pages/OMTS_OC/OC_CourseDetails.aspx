<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="OC_CourseDetails.aspx.cs" 
Inherits="fyp.OC_CourseDetails" MasterPageFile="~/Site1.Master" Async="true" %>

<asp:Content ID="ContentTitle" ContentPlaceHolderID="title" runat="server">Course Details</asp:Content>

<asp:Content ID="ContentStyle" ContentPlaceHolderID="css" runat="server">
    <style>
        .course-details-container {
            background-color: #ffffff;
            border-radius: 8px;
            box-shadow: 0 2px 10px rgba(0, 0, 0, 0.05);
            padding: 30px;
            margin-top: 20px;
        }

        .course-header {
            margin-bottom: 25px;
        }

        .course-name {
            font-size: 28px;
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
            font-size: 14px;
            margin-bottom: 15px;
        }

        .course-image {
            width: 100%;
            max-height: 300px;
            object-fit: cover;
            border-radius: 8px;
            margin-bottom: 20px;
        }

        .course-description {
            font-size: 16px;
            line-height: 1.6;
            color: #555;
            margin-bottom: 30px;
            white-space: pre-line;
        }

        .section-title {
            font-size: 20px;
            font-weight: 600;
            color: #333;
            margin: 30px 0 15px 0;
            border-bottom: 2px solid #f0f0f0;
            padding-bottom: 10px;
        }

        .materials-list {
            display: flex;
            flex-direction: column;
            gap: 10px;
        }

        .material-item {
            padding: 15px;
            background-color: #f9f9f9;
            border-radius: 8px;
            transition: background-color 0.3s ease;
        }

        .material-item:hover {
            background-color: #f0f0f0;
        }

        .material-link {
            color: #007bff;
            text-decoration: none;
            display: flex;
            align-items: center;
            gap: 10px;
        }

        .material-link i {
            font-size: 18px;
        }

        .material-link:hover {
            text-decoration: underline;
        }

        .locked-material {
            color: #6c757d;
            cursor: not-allowed;
        }

        .no-materials {
            padding: 20px;
            background-color: #f8f9fa;
            border-radius: 8px;
            text-align: center;
            color: #6c757d;
        }

        .action-buttons {
            display: flex;
            gap: 15px;
            margin-top: 30px;
        }

        .btn {
            padding: 10px 20px;
            border: none;
            border-radius: 6px;
            cursor: pointer;
            font-weight: 500;
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

        .btn-secondary {
            background-color: #6c757d;
            color: white;
        }

        .btn-secondary:hover {
            background-color: #5a6268;
        }

        .error-message {
            color: #dc3545;
            margin: 15px 0;
            padding: 10px;
            background-color: #f8d7da;
            border-radius: 6px;
        }
        
        /* Enrolled users list styles */
        .enrolled-users-container {
            margin-top: 30px;
        }
        
        .enrolled-users-list {
            width: 100%;
            border-collapse: collapse;
            margin-top: 15px;
        }
        
        .enrolled-users-list th {
            background-color: #f2f2f2;
            padding: 10px;
            text-align: left;
            border: 1px solid #ddd;
        }
        
        .enrolled-users-list td {
            padding: 8px 10px;
            border: 1px solid #ddd;
        }
        
        .enrolled-users-list tr:nth-child(even) {
            background-color: #f9f9f9;
        }
        
        .enrolled-users-list tr:hover {
            background-color: #f0f0f0;
        }
        
        .no-users-message {
            padding: 15px;
            background-color: #f8f9fa;
            border-radius: 8px;
            text-align: center;
            color: #6c757d;
            margin-top: 15px;
        }
        .btn-danger {
            background-color: #dc3545;
            color: white;
        }

        .btn-danger:hover {
            background-color: #c82333;
        }
        /* Certificate Modal Styles */
        .certificate-container {
            position: fixed;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            background-color: rgba(0, 0, 0, 0.7);
            display: none; /* Hidden by default */
            justify-content: center;
            align-items: center;
            z-index: 1000;
        }

        .certificate {
            width: 90%;
            max-width: 800px;
            background-color: #fff;
            border: 15px solid #366092;
            border-radius: 8px;
            padding: 40px;
            text-align: center;
            position: relative;
        }

        .certificate-title {
            font-size: 28px;
            font-weight: bold;
            color: #366092;
            margin-bottom: 20px;
        }

        .certificate-content {
            margin-bottom: 30px;
            line-height: 1.6;
            font-size: 18px;
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

        @media print {
            .sidebar, .nav-bar, .action-buttons, .certificate-close, .certificate-print, .user-section {
                display: none !important;
            }
    
            body, html {
                margin: 0;
                padding: 0;
                background-color: white;
            }
    
            .certificate {
                width: 100%;
                height: 100%;
                border: 15px solid #366092 !important;
                box-sizing: border-box;
                -webkit-print-color-adjust: exact;
                print-color-adjust: exact;
            }
    
            .main-content {
                width: 100% !important;
                padding: 0 !important;
                margin: 0 !important;
            }
    
            .container {
                display: block !important;
                width: 100% !important;
                height: 100% !important;
                padding: 0 !important;
                margin: 0 !important;
            }
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
            <asp:Button ID="btnCompletedCourse" runat="server" CssClass="sidebar-btn" 
                Text="Completed Courses" PostBackUrl="~/OMTS_Pages/OMTS_OC/OC_CompletedCourse.aspx" />
            <asp:Button ID="btnRecommendations" runat="server" CssClass="sidebar-btn" 
                Text="Recommendations" PostBackUrl="~/OMTS_Pages/OMTS_OC/OC_CourseRecommendations.aspx" 
                Visible='<%# Session["UserRole"]?.ToString() == "Member" || Session["UserRole"]?.ToString() == "Admin" %>' />
            <asp:Button ID="btnCourseManagement" runat="server" CssClass="sidebar-btn" 
                Text="Course Management" PostBackUrl="~/OMTS_Pages/OMTS_OC/OC_CourseManagement.aspx" Visible="false" />
        </div>

        <!-- Main Content Area -->
        <div class="main-content">
            <!-- Error/Success Message -->
            <asp:Label ID="lblMessage" runat="server" CssClass="error-message" Visible="false"></asp:Label>

            <!-- Course Details Panel -->
            <asp:Panel ID="panelCourseDetails" runat="server" CssClass="course-details-container">
                <!-- Course Header -->
                <div class="course-header">
                    <div class="course-name">
                        <asp:Literal ID="litCourseName" runat="server"></asp:Literal>
                    </div>
                    <div class="course-category">
                        <i class="fas fa-tag"></i>
                        <asp:Literal ID="litCourseCategory" runat="server"></asp:Literal>
                    </div>
                </div>


                <!-- Course Description -->
                <div class="course-description">
                    <asp:Literal ID="litCourseDescription" runat="server"></asp:Literal>
                </div>

                <!-- Course Materials Section -->
                <div class="section-title">Course Materials</div>
                <div id="divNoMaterials" runat="server" class="no-materials">
                    No materials available for this course yet.
                </div>
                <div class="materials-list">
                    <asp:Repeater ID="rptMaterials" runat="server" OnItemDataBound="rptMaterials_ItemDataBound">
                        <ItemTemplate>
                            <div class="material-item">
                                <asp:HyperLink ID="hlMaterial" runat="server" CssClass="material-link" Target="_blank">
                                    <i class="fas fa-file-alt"></i>
                                    <%# Container.DataItem %>
                                </asp:HyperLink>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>
                
                <!-- Enrolled Users Section (visible only to staff~/OMTS_Pages/OMTS_Admin/Admin) -->
                <asp:Panel ID="divEnrolledUsers" runat="server" CssClass="enrolled-users-container" Visible="false">
                    <div class="section-title">Enrolled Users</div>
                    <asp:GridView ID="rptEnrolledUsers" runat="server" CssClass="enrolled-users-list" AutoGenerateColumns="false">
                        <Columns>
                            <asp:BoundField DataField="UserID" HeaderText="User ID" />
                            <asp:BoundField DataField="Username" HeaderText="Username" />
                            <asp:BoundField DataField="Email" HeaderText="Email" />
                        </Columns>
                    </asp:GridView>
                </asp:Panel>
                
                <asp:Panel ID="divNoEnrolledUsers" runat="server" CssClass="no-users-message" Visible="false">
                    No users are currently enrolled in this course.
                </asp:Panel>

                <!-- Action Buttons -->
                <div class="action-buttons">
                    <asp:Button ID="btnBack" runat="server" Text="Back to Courses" 
                        CssClass="btn btn-secondary" OnClick="btnBack_Click" />
                    <asp:Button ID="btnJoinClass" runat="server" Text="Join Course" 
                        CssClass="btn btn-success" OnClick="btnJoinClass_Click" />
                    <asp:Button ID="btnLeaveCourse" runat="server" Text="Leave Course" 
                        CssClass="btn btn-danger" OnClick="btnLeaveCourse_Click" 
                        OnClientClick="return confirm('Are you sure you want to leave this course? You can rejoin later if needed.');" />
                </div>
            </asp:Panel>
        </div>
    </div>

    <!-- Font Awesome for icons -->
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/5.15.4/css/all.min.css">
</asp:Content>