<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="OC_RecycleBin.aspx.cs" 
Inherits="fyp.OC_RecycleBin" MasterPageFile="~/Site1.Master" Async="true" %>

<asp:Content ID="ContentTitle" ContentPlaceHolderID="title" runat="server">Course Recycle Bin</asp:Content>

<asp:Content ID="ContentStyle" ContentPlaceHolderID="css" runat="server">
    <style>
        .header-actions {
            display: flex;
            justify-content: space-between;
            align-items: center;
            margin-bottom: 25px;
        }
        
        .course-list {
            display: flex;
            flex-wrap: wrap;
            gap: 20px;
            margin-top: 20px;
        }

        .course-item {
            width: 280px;
            background-color: #ffffff;
            border-radius: 8px;
            box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
            padding: 20px;
            display: flex;
            flex-direction: column;
            position: relative;
            overflow: hidden;
        }

        .course-item::before {
            content: "DELETED";
            position: absolute;
            top: 15px;
            right: -35px;
            background-color: #ff4d4d;
            color: white;
            font-size: 12px;
            font-weight: bold;
            padding: 5px 40px;
            transform: rotate(45deg);
            opacity: 0.8;
        }

        .course-name {
            font-size: 18px;
            font-weight: 600;
            color: #333;
            margin-bottom: 10px;
        }

        .course-description {
            color: #666;
            font-size: 14px;
            margin-bottom: 20px;
            flex-grow: 1;
            overflow: hidden;
            text-overflow: ellipsis;
            display: -webkit-box;
            -webkit-line-clamp: 3;
            -webkit-box-orient: vertical;
        }

        .button-container {
            display: flex;
            gap: 10px;
            margin-top: 15px;
        }

        .restore-btn {
            background-color: #4CAF50;
            color: white;
            border: none;
            padding: 8px 15px;
            border-radius: 4px;
            cursor: pointer;
            font-size: 14px;
            text-align: center;
            flex-grow: 1;
            transition: background-color 0.3s;
        }

        .restore-btn:hover {
            background-color: #45a049;
        }

        .delete-btn {
            background-color: #ff4d4d;
            color: white;
            border: none;
            padding: 8px 15px;
            border-radius: 4px;
            cursor: pointer;
            font-size: 14px;
            text-align: center;
            flex-grow: 1;
            transition: background-color 0.3s;
        }

        .delete-btn:hover {
            background-color: #cc0000;
        }

        .empty-bin-btn {
            background-color: #dc3545;
            color: white;
            border: none;
            padding: 10px 15px;
            border-radius: 4px;
            cursor: pointer;
            font-size: 14px;
            transition: background-color 0.3s;
        }

        .empty-bin-btn:hover {
            background-color: #c82333;
        }

        .back-btn {
            background-color: #007bff;
            color: white;
            border: none;
            padding: 10px 15px;
            border-radius: 4px;
            cursor: pointer;
            font-size: 14px;
            transition: background-color 0.3s;
        }

        .back-btn:hover {
            background-color: #0069d9;
        }

        .no-items {
            width: 100%;
            padding: 40px;
            text-align: center;
            background-color: #f8f9fa;
            border-radius: 8px;
            font-size: 16px;
            color: #6c757d;
            margin-top: 20px;
            border: 1px solid #dee2e6;
        }

        .error-message {
            color: #dc3545;
            margin: 15px 0;
            padding: 10px;
            background-color: #f8d7da;
            border-radius: 6px;
        }

        .success-message {
            color: #28a745;
            margin: 15px 0;
            padding: 10px;
            background-color: #d4edda;
            border-radius: 6px;
        }

        .warning-modal {
            position: fixed;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            background-color: rgba(0, 0, 0, 0.5);
            display: flex;
            align-items: center;
            justify-content: center;
            z-index: 1000;
        }

        .modal-content {
            background-color: white;
            padding: 30px;
            border-radius: 8px;
            max-width: 500px;
            text-align: center;
        }

        .modal-title {
            font-size: 20px;
            font-weight: bold;
            margin-bottom: 20px;
            color: #dc3545;
        }

        .modal-text {
            margin-bottom: 30px;
            line-height: 1.5;
        }

        .modal-buttons {
            display: flex;
            justify-content: center;
            gap: 15px;
        }

        .confirm-btn {
            background-color: #dc3545;
            color: white;
            border: none;
            padding: 10px 20px;
            border-radius: 4px;
            cursor: pointer;
        }

        .cancel-btn {
            background-color: #6c757d;
            color: white;
            border: none;
            padding: 10px 20px;
            border-radius: 4px;
            cursor: pointer;
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
            <asp:Button ID="btnCourseManagement" runat="server" CssClass="sidebar-btn active" 
                Text="Course Management" PostBackUrl="~/OMTS_Pages/OMTS_OC/OC_CourseManagement.aspx" />
        </div>

        <!-- Main Content Area -->
        <div class="main-content">
            <div class="header-actions">
                <h2 class="page-header">Course Recycle Bin</h2>
                <div class="action-buttons">
                    <asp:Button ID="btnEmptyRecycleBin" runat="server" 
                        CssClass="empty-bin-btn" 
                        Text="Empty Recycle Bin" 
                        OnClick="btnEmptyRecycleBin_Click"
                        OnClientClick="return confirm('Are you sure you want to permanently delete all courses in the recycle bin? This action cannot be undone.');" />
                    <asp:Button ID="btnBackToCourseManagement" runat="server" 
                        CssClass="back-btn" 
                        Text="Back to Courses" 
                        OnClick="btnBackToCourseManagement_Click" />
                </div>
            </div>

            <!-- Message Label for Errors/Notifications -->
            <asp:Label ID="lblMessage" runat="server" EnableViewState="false" Visible="false"></asp:Label>

            <!-- Deleted Course List -->
            <div class="course-list">
                <asp:Panel ID="pnlNoItems" runat="server" CssClass="no-items" Visible="false">
                    No deleted courses found in the recycle bin.
                </asp:Panel>

                <asp:Repeater ID="rptDeletedCourses" runat="server">
                    <ItemTemplate>
                        <div class="course-item">
                            <div class="course-name"><%# Eval("CourseName") %></div>
                            <div class="course-description"><%# Eval("CourseDescription") %></div>

                            <div class="button-container">
                                <asp:LinkButton ID="btnRestore" runat="server" 
                                    CssClass="restore-btn" 
                                    CommandArgument='<%# Eval("CourseID") %>' 
                                    OnClick="btnRestore_Click"
                                    ToolTip="Restore this course">
                                    <i class="fas fa-trash-restore"></i> Restore
                                </asp:LinkButton>
                                <asp:LinkButton ID="btnPermanentDelete" runat="server" 
                                    CssClass="delete-btn" 
                                    CommandArgument='<%# Eval("CourseID") %>' 
                                    OnClick="btnPermanentDelete_Click"
                                    OnClientClick="return confirm('Are you sure you want to permanently delete this course? This action cannot be undone.');"
                                    ToolTip="Permanently delete this course">
                                    <i class="fas fa-trash-alt"></i> Delete
                                </asp:LinkButton>
                            </div>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
            </div>
        </div>
    </div>

    <!-- Font Awesome for icons -->
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/5.15.4/css/all.min.css">
</asp:Content>