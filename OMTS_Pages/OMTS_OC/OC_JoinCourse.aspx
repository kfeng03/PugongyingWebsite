<%@ Page Language="C#" Async="true" AutoEventWireup="true" CodeBehind="OC_JoinCourse.aspx.cs" Inherits="fyp.OC_JoinCourse" MasterPageFile="~/Site1.Master" %>

<asp:Content ID="ContentTitle" ContentPlaceHolderID="title" runat="server">Join Courses</asp:Content>

<asp:Content ID="ContentStyle" ContentPlaceHolderID="css" runat="server">
    <style>
        /* Consistent styling with event pages */
        .page-header {
            font-size: 24px;
            font-weight: 600;
            color: #333;
            margin-bottom: 25px;
            padding-bottom: 10px;
            border-bottom: 2px solid #f0f0f0;
        }

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

        .course-detail {
            margin-bottom: 10px;
            font-size: 14px;
            color: #666;
        }

        .empty-message {
            width: 100%;
            text-align: center;
            padding: 20px;
            background-color: #f8f9fa;
            border-radius: 8px;
            color: #6c757d;
        }

        .btn-view, .btn-join {
            background-color: #007bff;
            color: white;
            border: none;
            padding: 8px 16px;
            border-radius: 4px;
            cursor: pointer;
            transition: background-color 0.3s ease;
            margin-top: 10px;
            width: 100%;
            text-align: center;
        }

        .btn-view:hover {
            background-color: #0056b3;
        }

        .btn-join {
            background-color: #28a745;
            color: white;
            margin-top: 5px;
        }

        .btn-join:hover {
            background-color: #218838;
        }

        .button-container {
            display: flex;
            flex-direction: column;
            width: 100%;
        }

        .search-container {
            margin-bottom: 20px;
            display: flex;
            gap: 10px;
        }

        .search-box {
            flex: 1;
            padding: 10px 12px;
            border: 1px solid #ddd;
            border-radius: 6px;
            font-size: 14px;
            transition: border-color 0.3s ease;
        }

        .search-btn {
            padding: 10px 16px;
            background-color: #007bff;
            color: white;
            border: none;
            border-radius: 6px;
            cursor: pointer;
            font-weight: 500;
            transition: background-color 0.3s ease;
        }

        .btn {
            padding: 8px 16px;
            border: none;
            border-radius: 6px;
            cursor: pointer;
            font-weight: 500;
            font-size: 14px;
            transition: all 0.2s ease;
        }

        .btn-primary {
            background-color: #007bff;
            color: white;
        }

        .btn-success {
            background-color: #28a745;
            color: white;
        }

        .empty-message {
            margin-top: 20px;
            padding: 20px;
            background-color: #f8f9fa;
            border-radius: 8px;
            text-align: center;
            color: #6c757d;
            font-size: 16px;
            border: 1px solid #dee2e6;
        }

        /* Recommendation Section Styles */
        .recommendation-section {
            margin-bottom: 30px;
        }
        
        .recommendation-header {
            font-size: 20px;
            font-weight: 600;
            color: #333;
            margin-bottom: 10px;
            display: flex;
            align-items: center;
        }
        
        .recommendation-icon {
            color: #ff9800;
            margin-right: 8px;
        }
        
        .recommendation-description {
            color: #666;
            font-size: 14px;
            margin-bottom: 20px;
        }
        
        .recommended-list {
            margin-bottom: 20px;
        }
        
        .recommended-item {
            position: relative;
            border: 2px solid #e3f2fd;
            background-color: #f8fdff;
            box-shadow: 0 3px 10px rgba(0, 0, 0, 0.08);
        }
        
        .recommendation-badge {
            position: absolute;
            top: -10px;
            right: 10px;
            background-color: #ff9800;
            color: white;
            font-size: 12px;
            font-weight: bold;
            padding: 5px 10px;
            border-radius: 20px;
            box-shadow: 0 2px 5px rgba(0,0,0,0.2);
        }
        
        .course-match {
            margin-top: 8px;
            font-size: 14px;
            color: #28a745;
            font-weight: 500;
        }
        
        .match-icon {
            margin-right: 5px;
        }
        
        .section-divider {
            height: 1px;
            background-color: #e1e1e1;
            margin: 30px 0;
        }
        
        .all-courses-header {
            font-size: 20px;
            font-weight: 600;
            color: #333;
            margin-bottom: 15px;
        }
    </style>
</asp:Content>

<asp:Content ID="ContentMain" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container">
        <!-- Sidebar -->
        <div class="sidebar">
            <asp:Button ID="btnMyCourse" runat="server" CssClass='sidebar-btn' 
                Text="My Courses" PostBackUrl="~/OMTS_Pages/OMTS_OC/OC_Courses.aspx" />
            <asp:Button ID="btnJoinCourse" runat="server" CssClass='sidebar-btn active' 
                Text="Join Course" PostBackUrl="~/OMTS_Pages/OMTS_OC/OC_JoinCourse.aspx" />
            <asp:Button ID="btnCompletedCourse" runat="server" CssClass='sidebar-btn' 
                Text="Completed Courses" PostBackUrl="~/OMTS_Pages/OMTS_OC/OC_CompletedCourse.aspx" />
            <asp:Button ID="btnRecommendations" runat="server" CssClass='sidebar-btn' 
                Text="Recommendations" PostBackUrl="~/OMTS_Pages/OMTS_OC/OC_CourseRecommendations.aspx" />
            <asp:Button ID="btnCourseManagement" runat="server" CssClass='sidebar-btn' 
                Text="Course Management" PostBackUrl="~/OMTS_Pages/OMTS_OC/OC_CourseManagement.aspx" Visible="false" />
        </div>

        <!-- Main Content Area -->
        <div class="main-content">
            <!-- Recommended Courses Section -->
            <div class="recommendation-section">
                <h3 class="recommendation-header">
                    <i class="fas fa-star recommendation-icon"></i> Recommended For You
                </h3>
                
                <div class="recommendation-description">
                    Based on your interests and learning history
                </div>
                
                <div class="course-list recommended-list">
                    <asp:Label ID="lblNoRecommendations" runat="server" 
                        Text="We don't have any personalized recommendations for you yet. Try adding interests in your profile!" 
                        CssClass="empty-message" Visible="false"></asp:Label>

                    <asp:Repeater ID="rptRecommendedCourses" runat="server" OnItemDataBound="rptRecommendedCourses_ItemDataBound">
                        <ItemTemplate>
                            <div class="course-item recommended-item">
                                <div class="recommendation-badge">Recommended</div>
                                <div>
                                    <div class="course-title">
                                        <%# Eval("CourseName") %>
                                    </div>
                                    <div class="course-details">
                                        Category: <%# Eval("CourseCategory") %>
                                    </div>
                                    <div class="course-detail">
                                        <i class="fas fa-users"></i>
                                        <asp:Label ID="lblParticipants" runat="server" 
                                            Text='<%# "Students: 0/" + Eval("NumberOfStudents") %>'></asp:Label>
                                    </div>
                                    <div class="course-match">
                                        <i class="fas fa-check-circle match-icon"></i> Matches your interests
                                    </div>
                                </div>
                                <div class="button-container">
                                    <asp:Button ID="btnViewDetails" runat="server" Text="View Details" 
                                        CssClass="btn-view"
                                        CommandArgument='<%# Eval("CourseID") %>' 
                                        OnClick="btnViewDetails_Click" />
                        
                                    <asp:Button ID="btnJoin" runat="server" Text="Join" 
                                        CssClass="btn-join"
                                        CommandArgument='<%# Eval("CourseID") %>' 
                                        OnClick="btnJoin_Click" />
                                </div>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>
                
                <div class="section-divider"></div>
                
                <h3 class="all-courses-header">All Available Courses</h3>
            </div>

            <h2 class="page-header">Available Courses</h2>

            <!-- Search Box -->
            <div class="search-container">
                <asp:TextBox ID="txtSearch" runat="server" placeholder="Search courses..." 
                    CssClass="search-box"></asp:TextBox>
                <asp:Button ID="btnSearch" runat="server" Text="Search" 
                    OnClick="btnSearch_Click" CssClass="search-btn" />
            </div>

            <!-- Message Label for Errors/Notifications -->
            <asp:Label ID="lblMessage" runat="server" ForeColor="Red" 
                EnableViewState="false" Visible="false"></asp:Label>

            <!-- Available Courses List -->
            <div class="course-list">
                <asp:Label ID="lblNoCourses" runat="server" 
                    Text="No courses available." 
                    CssClass="empty-message" Visible="false"></asp:Label>
    
                <asp:Repeater ID="rptAvailableCourses" runat="server" 
                    OnItemDataBound="rptAvailableCourses_ItemDataBound">
                    <ItemTemplate>
                        <div class="course-item">
                            <div>
                                <div class="course-title">
                                    <%# Eval("CourseName") %>
                                </div>
                                <div class="course-details">
                                    Category: <%# Eval("CourseCategory") %>
                                </div>
                                <div class="course-detail">
                                    <i class="fas fa-users"></i>
                                    <asp:Label ID="lblParticipants" runat="server" 
                                        Text='<%# "Students: 0/" + Eval("NumberOfStudents") %>'></asp:Label>
                                </div>
                            </div>
                            <div class="button-container">
                                <asp:Button ID="btnViewDetails" runat="server" Text="View Details" 
                                    CssClass="btn-view"
                                    CommandArgument='<%# Eval("CourseID") %>' 
                                    OnClick="btnViewDetails_Click" />
                    
                                <asp:Button ID="btnJoin" runat="server" Text="Join" 
                                    CssClass="btn-join"
                                    CommandArgument='<%# Eval("CourseID") %>' 
                                    OnClick="btnJoin_Click" 
                                    Visible="false" />
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