<%@ Page Language="C#" Async="true" AutoEventWireup="true" CodeBehind="OC_CourseRecommendations.aspx.cs" 
Inherits="fyp.OC_CourseRecommendations" MasterPageFile="~/Site1.Master" %>

<asp:Content ID="ContentTitle" ContentPlaceHolderID="title" runat="server">Course Recommendations</asp:Content>

<asp:Content ID="ContentStyle" ContentPlaceHolderID="css" runat="server">
    <style>
        .page-header {
            font-size: 28px;
            font-weight: 600;
            color: #333;
            margin-bottom: 15px;
        }
        
        .page-description {
            color: #666;
            margin-bottom: 30px;
            font-size: 16px;
            line-height: 1.5;
        }
        
        .recommendation-section {
            margin-bottom: 40px;
            background-color: #fff;
            padding: 25px;
            border-radius: 8px;
            box-shadow: 0 2px 10px rgba(0, 0, 0, 0.05);
        }
        
        .recommendation-header {
            display: flex;
            align-items: center;
            margin-bottom: 15px;
        }
        
        .recommendation-title {
            font-size: 22px;
            font-weight: 600;
            color: #333;
            margin-left: 12px;
        }
        
        .recommendation-icon {
            font-size: 24px;
        }
        
        .recommendation-icon.interest {
            color: #4285f4; /* Google Blue */
        }
        
        .recommendation-icon.popular {
            color: #ea4335; /* Google Red */
        }
        
        .recommendation-icon.similar {
            color: #fbbc05; /* Google Yellow */
        }
        
        .recommendation-description {
            color: #666;
            margin-bottom: 20px;
            font-size: 15px;
            line-height: 1.5;
            padding-left: 36px;
        }
        
        .course-list {
            display: flex;
            flex-wrap: wrap;
            gap: 20px;
        }
        
        .course-item {
            width: 280px;
            background-color: #f9f9f9;
            border: 1px solid #eee;
            border-radius: 8px;
            padding: 20px;
            display: flex;
            flex-direction: column;
            justify-content: space-between;
            transition: transform 0.2s ease, box-shadow 0.2s ease;
            position: relative;
        }
        
        .course-item:hover {
            transform: translateY(-5px);
            box-shadow: 0 6px 12px rgba(0, 0, 0, 0.1);
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
        
        .course-detail {
            margin-bottom: 12px;
            font-size: 14px;
            color: #666;
        }
        
        .match-reason {
            background-color: #f0f8ff;
            padding: 8px 12px;
            border-radius: 6px;
            font-size: 13px;
            color: #0066cc;
            margin-bottom: 15px;
            border-left: 3px solid #0066cc;
        }
        
        .popular-stats {
            background-color: #fff0f0;
            padding: 8px 12px;
            border-radius: 6px;
            font-size: 13px;
            color: #cc3333;
            margin-bottom: 15px;
            border-left: 3px solid #cc3333;
        }
        
        .similar-reason {
            background-color: #fffde7;
            padding: 8px 12px;
            border-radius: 6px;
            font-size: 13px;
            color: #ff9800;
            margin-bottom: 15px;
            border-left: 3px solid #ff9800;
        }
        
        .button-container {
            display: flex;
            flex-direction: column;
            gap: 10px;
            margin-top: auto;
        }
        
        .btn {
            padding: 10px;
            border: none;
            border-radius: 6px;
            cursor: pointer;
            font-weight: 500;
            transition: background-color 0.3s ease;
            text-align: center;
            font-size: 14px;
        }
        
        .btn-view {
            background-color: #f1f3f4;
            color: #333;
        }
        
        .btn-view:hover {
            background-color: #e8eaed;
        }
        
        .btn-join {
            background-color: #4285f4;
            color: white;
        }
        
        .btn-join:hover {
            background-color: #3367d6;
        }
        
        .empty-message {
            padding: 15px;
            background-color: #f8f9fa;
            border-radius: 8px;
            text-align: center;
            color: #6c757d;
            font-style: italic;
            width: 100%;
        }
        
        .message {
            padding: 12px 20px;
            margin-bottom: 20px;
            border-radius: 6px;
            font-size: 15px;
            text-align: center;
        }
        
        .message.error {
            background-color: #f8d7da;
            color: #721c24;
        }
        
        .message.success {
            background-color: #d4edda;
            color: #155724;
        }
        
        .message.info {
            background-color: #cce5ff;
            color: #004085;
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
            <asp:Button ID="btnRecommendations" runat="server" CssClass="sidebar-btn active" 
                Text="Recommendations" PostBackUrl="~/OMTS_Pages/OMTS_OC/OC_CourseRecommendations.aspx" />
            <asp:Button ID="btnCourseManagement" runat="server" CssClass="sidebar-btn" 
                Text="Course Management" PostBackUrl="~/OMTS_Pages/OMTS_OC/OC_CourseManagement.aspx" Visible="false" />
        </div>

        <!-- Main Content Area -->
        <div class="main-content">
            <h1 class="page-header">Personalized Course Recommendations</h1>
            <p class="page-description">
                Discover courses tailored to your interests, popular among other users, 
                and similar to what you've already completed.
            </p>

            <!-- Status Messages -->
            <asp:Label ID="lblMessage" runat="server" CssClass="message info" Visible="false"></asp:Label>
            
            <!-- No Recommendations Message -->
            <asp:Label ID="lblNoRecommendations" runat="server" CssClass="empty-message" 
                Text="No recommendations available at this time." Visible="false"></asp:Label>
            
            <!-- Interest-Based Recommendations -->
            <asp:Panel ID="pnlInterestRecommendations" runat="server" CssClass="recommendation-section">
                <div class="recommendation-header">
                    <i class="fas fa-lightbulb recommendation-icon interest"></i>
                    <h2 class="recommendation-title">Based on Your Interests</h2>
                </div>
                <p class="recommendation-description">
                    These courses match your profile interests and might help you develop your skills 
                    in areas you care about.
                </p>
                
                <asp:Label ID="lblNoInterestRecommendations" runat="server" 
                    Text="No interest-based recommendations found. Try updating your interests in your profile." 
                    CssClass="empty-message" Visible="false"></asp:Label>
                
                <div class="course-list">
                    <asp:Repeater ID="rptInterestRecommendations" runat="server">
                        <ItemTemplate>
                            <div class="course-item">
                                <div>
                                    <div class="course-title"><%# Eval("CourseName") %></div>
                                    <div class="course-category"><%# Eval("CourseCategory") %></div>
                                    <div class="match-reason">
                                        <i class="fas fa-check-circle"></i> <%# Eval("MatchReason") %>
                                    </div>
                                    <div class="course-detail">
                                        <i class="fas fa-users"></i> Maximum students: <%# Eval("NumberOfStudents") %>
                                    </div>
                                </div>
                                <div class="button-container">
                                    <asp:Button ID="btnViewDetails" runat="server" Text="View Details" 
                                        CssClass="btn btn-view"
                                        CommandArgument='<%# Eval("CourseID") %>' 
                                        OnClick="btnViewDetails_Click" />
                        
                                    <asp:Button ID="btnJoin" runat="server" Text="Join Course" 
                                        CssClass="btn btn-join"
                                        CommandArgument='<%# Eval("CourseID") %>' 
                                        OnClick="btnJoin_Click" />
                                </div>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>
            </asp:Panel>
            
            <!-- Popular Courses -->
            <asp:Panel ID="pnlPopularCourses" runat="server" CssClass="recommendation-section">
                <div class="recommendation-header">
                    <i class="fas fa-fire recommendation-icon popular"></i>
                    <h2 class="recommendation-title">Popular Courses</h2>
                </div>
                <p class="recommendation-description">
                    These are the most popular courses among our members. Join the community 
                    and learn with others.
                </p>
                
                <asp:Label ID="lblNoPopularCourses" runat="server" 
                    Text="No popular courses found at this time." 
                    CssClass="empty-message" Visible="false"></asp:Label>
                
                <div class="course-list">
                    <asp:Repeater ID="rptPopularCourses" runat="server">
                        <ItemTemplate>
                            <div class="course-item">
                                <div>
                                    <div class="course-title"><%# Eval("CourseName") %></div>
                                    <div class="course-category"><%# Eval("CourseCategory") %></div>
                                    <div class="popular-stats">
                                        <i class="fas fa-chart-line"></i> <%# Eval("EnrollmentCount") %> users enrolled
                                        <br />
                                        <i class="fas fa-percentage"></i> <%# Eval("EnrollmentRate") %>
                                    </div>
                                    <div class="course-detail">
                                        <i class="fas fa-users"></i> Maximum students: <%# Eval("NumberOfStudents") %>
                                    </div>
                                </div>
                                <div class="button-container">
                                    <asp:Button ID="btnViewDetails" runat="server" Text="View Details" 
                                        CssClass="btn btn-view"
                                        CommandArgument='<%# Eval("CourseID") %>' 
                                        OnClick="btnViewDetails_Click" />
                        
                                    <asp:Button ID="btnJoin" runat="server" Text="Join Course" 
                                        CssClass="btn btn-join"
                                        CommandArgument='<%# Eval("CourseID") %>' 
                                        OnClick="btnJoin_Click" />
                                </div>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>
            </asp:Panel>
            
            <!-- Similar to Completed Courses -->
            <asp:Panel ID="pnlSimilarCourses" runat="server" CssClass="recommendation-section">
                <div class="recommendation-header">
                    <i class="fas fa-route recommendation-icon similar"></i>
                    <h2 class="recommendation-title">Continue Your Learning Path</h2>
                </div>
                <p class="recommendation-description">
                    Based on courses you've completed, these might be good next steps on your learning journey.
                </p>
                
                <asp:Label ID="lblNoSimilarCourses" runat="server" 
                    Text="Complete some courses to get recommendations for your learning path." 
                    CssClass="empty-message" Visible="false"></asp:Label>
                
                <div class="course-list">
                    <asp:Repeater ID="rptSimilarCourses" runat="server">
                        <ItemTemplate>
                            <div class="course-item">
                                <div>
                                    <div class="course-title"><%# Eval("CourseName") %></div>
                                    <div class="course-category"><%# Eval("CourseCategory") %></div>
                                    <div class="similar-reason">
                                        <i class="fas fa-graduation-cap"></i> <%# Eval("RecommendationReason") %>
                                    </div>
                                    <div class="course-detail">
                                        <i class="fas fa-users"></i> Maximum students: <%# Eval("NumberOfStudents") %>
                                    </div>
                                </div>
                                <div class="button-container">
                                    <asp:Button ID="btnViewDetails" runat="server" Text="View Details" 
                                        CssClass="btn btn-view"
                                        CommandArgument='<%# Eval("CourseID") %>' 
                                        OnClick="btnViewDetails_Click" />
                        
                                    <asp:Button ID="btnJoin" runat="server" Text="Join Course" 
                                        CssClass="btn btn-join"
                                        CommandArgument='<%# Eval("CourseID") %>' 
                                        OnClick="btnJoin_Click" />
                                </div>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>
            </asp:Panel>
        </div>
    </div>

    <!-- Font Awesome for icons -->
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/5.15.4/css/all.min.css">
</asp:Content>