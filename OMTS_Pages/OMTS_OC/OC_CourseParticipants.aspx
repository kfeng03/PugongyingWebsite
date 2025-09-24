<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="OC_CourseParticipants.aspx.cs" 
Inherits="fyp.OC_CourseParticipants" MasterPageFile="~/Site1.Master" Async="true" %>

<asp:Content ID="ContentTitle" ContentPlaceHolderID="title" runat="server">Course Participants</asp:Content>

<asp:Content ID="ContentStyle" ContentPlaceHolderID="css" runat="server">
    <style>
        .participants-container {
            margin-top: 20px;
            width: 100%;
        }
        
        .course-title {
            font-size: 24px;
            font-weight: bold;
            margin-bottom: 5px;
        }
        
        .course-info {
            font-size: 16px;
            color: #555;
            margin-bottom: 20px;
        }
        
        .participants-table {
            width: 100%;
            border-collapse: collapse;
            margin-top: 20px;
        }
        
        .participants-table th {
            background-color: #f2f2f2;
            padding: 10px;
            text-align: left;
            border: 1px solid #ddd;
        }
        
        .participants-table td {
            padding: 10px;
            border: 1px solid #ddd;
        }
        
        .participants-table tr:nth-child(even) {
            background-color: #f9f9f9;
        }
        
        .participants-table tr:hover {
            background-color: #f0f0f0;
        }
        
        .no-participants {
            margin-top: 20px;
            font-size: 16px;
            color: #666;
        }
        
        .back-btn {
            background-color: #2196F3;
            color: white;
            border: none;
            border-radius: 5px;
            padding: 8px 15px;
            margin-top: 10px;
            cursor: pointer;
            transition: background-color 0.3s ease;
        }

        .back-btn:hover {
            background-color: #0b7dda;
        }
        
        .export-btn {
            background-color: #4CAF50;
            color: white;
            border: none;
            border-radius: 5px;
            padding: 8px 15px;
            margin-top: 10px;
            margin-left: 10px;
            cursor: pointer;
            transition: background-color 0.3s ease;
        }
        
        .export-btn:hover {
            background-color: #3e8e41;
        }
        .status-completed {
            background-color: #28a745;
            color: white;
            padding: 4px 8px;
            border-radius: 4px;
            font-size: 12px;
            font-weight: bold;
        }

        .status-pending {
            background-color: #ffc107;
            color: #212529;
            padding: 4px 8px;
            border-radius: 4px;
            font-size: 12px;
            font-weight: bold;
        }

        .btn-sm {
            padding: 4px 8px;
            font-size: 12px;
            border-radius: 4px;
            border: none;
            cursor: pointer;
        }

        .btn-success {
            background-color: #28a745;
            color: white;
        }

        .btn-success:hover {
            background-color: #218838;
        }

        .btn-warning {
            background-color: #ffc107;
            color: #212529;
        }

        .btn-warning:hover {
            background-color: #e0a800;
        }
    </style>
</asp:Content>

<asp:Content ID="ContentMain" ContentPlaceHolderID="MainContent" runat="server">
    <body>
        <!-- Main Content Layout -->
        <div class="container">
            <!-- Sidebar -->
            <div class="sidebar">
                <asp:Button ID="btnMyCourse" runat="server" CssClass='<%# (Request.Url.AbsolutePath.EndsWith("OC_Courses.aspx")) ? "sidebar-btn active" : "sidebar-btn" %>' 
                    Text="My Courses" PostBackUrl="~/OMTS_Pages/OMTS_OC/OC_Courses.aspx" />
                <asp:Button ID="btnJoinCourse" runat="server" CssClass='<%# (Request.Url.AbsolutePath.EndsWith("OC_JoinCourse.aspx")) ? "sidebar-btn active" : "sidebar-btn" %>' 
                    Text="Join Course" PostBackUrl="~/OMTS_Pages/OMTS_OC/OC_JoinCourse.aspx" />
                <asp:Button ID="btnCompletedCourse" runat="server" CssClass='<%# (Request.Url.AbsolutePath.EndsWith("OC_CompletedCourse.aspx")) ? "sidebar-btn active" : "sidebar-btn" %>' 
                    Text="Completed Courses" PostBackUrl="~/OMTS_Pages/OMTS_OC/OC_CompletedCourse.aspx" />
                <asp:Button ID="btnRecommendations" runat="server" CssClass="sidebar-btn" 
                    Text="Recommendations" PostBackUrl="~/OMTS_Pages/OMTS_OC/OC_CourseRecommendations.aspx" 
                    Visible='<%# Session["UserRole"]?.ToString() == "Member" || Session["UserRole"]?.ToString() == "Admin" %>' />
                <asp:Button ID="btnCourseManagement" runat="server" CssClass='<%# (Request.Url.AbsolutePath.EndsWith("OC_CourseManagement.aspx")) ? "sidebar-btn active" : "sidebar-btn" %>' 
                    Text="Course Management" PostBackUrl="~/OMTS_Pages/OMTS_OC/OC_CourseManagement.aspx" />
            </div>

            <!-- Main Content Area -->
            <div class="main-content">
                <h2 class="page-header">Course Participants</h2>
                
                <!-- Message Label for Errors -->
                <asp:Label ID="lblMessage" runat="server" ForeColor="Red" EnableViewState="false"></asp:Label>
                
                <div class="participants-container">
                    <div class="course-title">
                        <asp:Literal ID="litCourseName" runat="server"></asp:Literal>
                    </div>
                    <div class="course-info">
                        <asp:Literal ID="litCourseInfo" runat="server"></asp:Literal>
                    </div>
                    
                    <div class="buttons-container">
                        <asp:Button ID="btnBack" runat="server" Text="Back to Course Management" CssClass="back-btn" OnClick="btnBack_Click" />
                    </div>
                    
                    <!-- Participants Table -->
                    <asp:GridView ID="gvParticipants" runat="server" CssClass="participants-table" AutoGenerateColumns="false"
                        EmptyDataText="No participants found for this course." Width="100%">
                        <Columns>
                            <asp:BoundField DataField="UserId" HeaderText="User ID" />
                            <asp:BoundField DataField="Username" HeaderText="Username" />
                            <asp:BoundField DataField="Email" HeaderText="Email" />
                            <asp:BoundField DataField="PhoneNumber" HeaderText="Phone Number" />
                            <asp:TemplateField HeaderText="Completion Status">
                                <ItemTemplate>
                                    <asp:Label ID="lblCompletionStatus" runat="server" 
                                        Text='<%# Eval("IsCompleted").ToString() == "True" ? "Completed" : "In Progress" %>'
                                        CssClass='<%# Eval("IsCompleted").ToString() == "True" ? "status-completed" : "status-pending" %>'></asp:Label>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Actions">
                                <ItemTemplate>
                                    <asp:Button ID="btnMarkComplete" runat="server" 
                                        Text='<%# Eval("IsCompleted").ToString() == "True" ? "Mark Incomplete" : "Mark Complete" %>' 
                                        CommandName='<%# Eval("IsCompleted").ToString() == "True" ? "MarkIncomplete" : "MarkComplete" %>'
                                        CommandArgument='<%# Eval("UserId") %>'
                                        CssClass='<%# Eval("IsCompleted").ToString() == "True" ? "btn-sm btn-warning" : "btn-sm btn-success" %>'
                                        OnCommand="btnMarkComplete_Command" />
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                </div>
            </div>
        </div>
    </body>
</asp:Content>