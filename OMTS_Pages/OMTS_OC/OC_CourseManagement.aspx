<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="OC_CourseManagement.aspx.cs" 
Inherits="fyp.OC_CourseManagement" MasterPageFile="~/Site1.Master" Async="true" %>

<asp:Content ID="ContentTitle" ContentPlaceHolderID="title" runat="server">Course Management</asp:Content>

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
        }

        .course-name {
            font-size: 18px;
            font-weight: bold;
            margin-bottom: 10px;
        }

        .course-description {
            flex-grow: 1;
            margin-bottom: 10px;
        }

        .course-details {
            font-size: 14px;
            color: #666;
            margin-bottom: 10px;
        }

        .button-container {
            display: flex;
            flex-direction: column;
            gap: 10px;
        }

        .btn {
            width: 100%;
            padding: 8px;
            border: none;
            border-radius: 4px;
            cursor: pointer;
            transition: background-color 0.3s ease;
            text-align: center;
        }

        .btn-primary {
            background-color: #2196F3;
            color: white;
        }

        .btn-primary:hover {
            background-color: #0b7dda;
        }

        .btn-purple {
            background-color: #6f42c1;
            color: white;
        }
        
        .btn-purple:hover {
            background-color: #5e35b1;
        }

        .btn-danger {
            background-color: #f44336;
            color: white;
        }

        .btn-danger:hover {
            background-color: #d32f2f;
        }

        .add-course-card {
            background-color: #e0e0e0;
            color: #666;
            display: flex;
            justify-content: center;
            align-items: center;
            font-size: 18px;
            font-weight: bold;
            width: 250px;
            height: 200px;
            cursor: pointer;
        }
        
        .header-actions {
            display: flex;
            justify-content: space-between;
            align-items: center;
            margin-bottom: 25px;
        }
        
        .recycle-btn {
            background-color: #28a745;
            color: white;
            border: none;
            border-radius: 6px;
            padding: 8px 16px;
            cursor: pointer;
            transition: all 0.2s ease;
            font-weight: 500;
            font-size: 14px;
        }

        .recycle-btn:hover {
            background-color: #218838;
        }
        .button-container {
            display: flex;
            flex-direction: column;
            gap: 10px;
            align-items: center;  /* Center the buttons horizontally */
        }

        .btn {
            width: 90%;          /* Make buttons slightly narrower than container for better appearance */
            padding: 8px 0;      /* Keep vertical padding, adjust horizontal to 0 */
            border: none;
            border-radius: 4px;
            cursor: pointer;
            transition: background-color 0.3s ease;
            text-align: center;
            display: inline-block; /* Ensure consistent display behavior */
            text-decoration: none; /* Remove underline from link buttons */
            font-size: 14px;      /* Consistent font size */
            font-weight: normal;  /* Consistent font weight */
            line-height: 1.5;     /* Consistent line height */
        }

        /* Adjust hover states to only change background color, not text decoration */
        .btn:hover {
            text-decoration: none;
        }

        .btn-primary {
            background-color: #2196F3;
            color: white;
        }

        .btn-primary:hover {
            background-color: #0b7dda;
        }

        .btn-purple {
            background-color: #6f42c1;
            color: white;
        }

        .btn-purple:hover {
            background-color: #5e35b1;
        }

        .btn-danger {
            background-color: #f44336;
            color: white;
        }

        .btn-danger:hover {
            background-color: #d32f2f;
        }
    </style>
</asp:Content>

<asp:Content ID="ContentMain" ContentPlaceHolderID="MainContent" runat="server">
    <body>
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
                <asp:Button ID="btnCourseManagement" runat="server" CssClass="sidebar-btn active" 
                    Text="Course Management" PostBackUrl="~/OMTS_Pages/OMTS_OC/OC_CourseManagement.aspx" />
            </div>

            <!-- Main Content Area -->
            <div class="main-content">
                <div class="header-actions">
                    <h2 class="page-header">Course Management</h2>
                    <asp:Button ID="btnRecycleBin" runat="server" Text="Recycle Bin" 
                        OnClick="btnRecycleBin_Click" CssClass="recycle-btn" />
                </div>

                <!-- Error/Success Message -->
                <asp:Label ID="lblMessage" runat="server" CssClass="message"></asp:Label>

                <!-- Course List Section -->
                <div class="course-list">
                    <!-- Add Course Card -->
                    <asp:LinkButton ID="lnkAddCourse" runat="server" 
                        CssClass="course-item add-course-card" 
                        PostBackUrl="~/OMTS_Pages/OMTS_OC/OC_AddCourse.aspx">
                        Add Course
                    </asp:LinkButton>

                    <!-- Course Repeater -->
                    <asp:Repeater ID="rptCourses" runat="server">
                        <ItemTemplate>
                            <div class="course-item">
                                <div class="course-name">
                                    <%# Eval("CourseName") %>
                                </div>
                                
                                <div class="course-details">
                                    Students: <%# Eval("NumberOfStudents") %>
                                </div>
                                <div class="button-container">
                                    <asp:LinkButton ID="btnParticipants" runat="server" 
                                        CssClass="btn btn-purple"
                                        CommandArgument='<%# Eval("CourseID") %>' 
                                        OnClick="btnParticipants_Click">
                                        Participants
                                    </asp:LinkButton>
                                    <asp:LinkButton ID="btnEdit" runat="server" 
                                        CssClass="btn btn-primary"
                                        CommandArgument='<%# Eval("CourseID") %>' 
                                        OnClick="btnEdit_Click">
                                        Edit
                                    </asp:LinkButton>
                                    <asp:Button ID="btnDelete" runat="server" 
                                        Text="Delete" 
                                        CssClass="btn btn-danger"
                                        CommandArgument='<%# Eval("CourseID") %>' 
                                        OnClick="btnDelete_Click"
                                        OnClientClick="confirmDelete(this); return false;" />
                                </div>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>
            </div>
        </div>
    </body>
</asp:Content>