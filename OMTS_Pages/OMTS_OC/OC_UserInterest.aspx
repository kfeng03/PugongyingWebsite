<%@ Page Language="C#" Async="true" AutoEventWireup="true" CodeBehind="OC_UserInterests.aspx.cs" Inherits="fyp.OC_UserInterests" MasterPageFile="~/Site1.Master" %>

<asp:Content ID="ContentTitle" ContentPlaceHolderID="title" runat="server">My Interests</asp:Content>

<asp:Content ID="ContentStyle" ContentPlaceHolderID="css" runat="server">
    <style>
        .interests-container {
            background-color: #ffffff;
            border-radius: 8px;
            box-shadow: 0 2px 10px rgba(0, 0, 0, 0.05);
            padding: 30px;
            margin-top: 20px;
        }
        
        .section-header {
            font-size: 20px;
            font-weight: 600;
            color: #333;
            margin-bottom: 20px;
            padding-bottom: 10px;
            border-bottom: 2px solid #f0f0f0;
        }
        
        .interests-description {
            color: #666;
            font-size: 16px;
            margin-bottom: 25px;
            line-height: 1.5;
        }
        
        .interests-form {
            margin-top: 20px;
        }
        
        .form-group {
            margin-bottom: 20px;
        }
        
        .form-label {
            display: block;
            margin-bottom: 8px;
            font-weight: 500;
            color: #333;
        }
        
        .form-control {
            width: 100%;
            padding: 10px;
            border: 1px solid #ddd;
            border-radius: 4px;
            font-size: 15px;
        }
        
        .checkbox-list {
            display: flex;
            flex-wrap: wrap;
            gap: 15px;
            margin-top: 10px;
        }
        
        .checkbox-item {
            background-color: #f8f9fa;
            border: 1px solid #ddd;
            border-radius: 4px;
            padding: 10px 15px;
            display: flex;
            align-items: center;
            cursor: pointer;
            transition: all 0.2s ease;
        }
        
        .checkbox-item:hover {
            background-color: #e9ecef;
        }
        
        .checkbox-item input {
            margin-right: 8px;
        }
        
        .btn-container {
            margin-top: 30px;
            display: flex;
            gap: 10px;
        }
        
        .btn-save {
            padding: 10px 20px;
            background-color: #007bff;
            color: white;
            border: none;
            border-radius: 4px;
            cursor: pointer;
            font-weight: 500;
            transition: background-color 0.3s;
        }
        
        .btn-save:hover {
            background-color: #0069d9;
        }
        
        .current-interests {
            margin-top: 30px;
        }
        
        .interest-tag {
            display: inline-block;
            background-color: #e3f2fd;
            color: #0d6efd;
            padding: 5px 10px;
            border-radius: 4px;
            margin-right: 10px;
            margin-bottom: 10px;
            font-size: 14px;
        }
        
        .success-message {
            padding: 15px;
            background-color: #d4edda;
            border-color: #c3e6cb;
            color: #155724;
            border-radius: 4px;
            margin-bottom: 20px;
        }
        
        .error-message {
            padding: 15px;
            background-color: #f8d7da;
            border-color: #f5c6cb;
            color: #721c24;
            border-radius: 4px;
            margin-bottom: 20px;
        }
        
        .custom-interest-container {
            display: flex;
            gap: 10px;
            margin-top: 15px;
        }
        
        .add-btn {
            padding: 10px 15px;
            background-color: #28a745;
            color: white;
            border: none;
            border-radius: 4px;
            cursor: pointer;
            transition: background-color 0.3s;
        }
        
        .add-btn:hover {
            background-color: #218838;
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
                Text="Recommendations" PostBackUrl="~/OMTS_Pages/OMTS_OC/OC_CourseRecommendations.aspx" />
            <asp:Button ID="btnInterests" runat="server" CssClass="sidebar-btn active" 
                Text="My Interests" PostBackUrl="~/OMTS_Pages/OMTS_OC/OC_UserInterests.aspx" />
            <asp:Button ID="btnCourseManagement" runat="server" CssClass="sidebar-btn" 
                Text="Course Management" PostBackUrl="~/OMTS_Pages/OMTS_OC/OC_CourseManagement.aspx" Visible="false" />
        </div>

        <!-- Main Content Area -->
        <div class="main-content">
            <h2 class="page-header">My Interests</h2>
            
            <div class="interests-container">
                <!-- Message Display -->
                <asp:Label ID="lblMessage" runat="server" Visible="false"></asp:Label>
                
                <div class="interests-description">
                    Your interests help us recommend courses that match what you want to learn. 
                    Select from the categories below or add your own custom interests.
                </div>
                
                <!-- Current Interests Section -->
                <div class="section-header">Your Current Interests</div>
                <div class="current-interests">
                    <asp:Panel ID="pnlNoInterests" runat="server" Visible="false">
                        <p>You haven't added any interests yet. Add some below to get personalized course recommendations!</p>
                    </asp:Panel>
                    
                    <asp:Repeater ID="rptUserInterests" runat="server">
                        <ItemTemplate>
                            <span class="interest-tag">
                                <%# Container.DataItem %>
                                <asp:LinkButton ID="btnRemoveInterest" runat="server" 
                                                Text="×" 
                                                CommandName="Remove"
                                                CommandArgument='<%# Container.DataItem %>'
                                                OnCommand="btnRemoveInterest_Command"
                                                style="margin-left: 5px; font-weight: bold;"/>
                            </span>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>
                
                <!-- Add Interests Form -->
                <div class="interests-form">
                    <div class="section-header">Add Interests</div>
                    
                    <!-- Predefined Categories -->
                    <div class="form-group">
                        <label class="form-label">Popular Categories</label>
                        <div class="checkbox-list">
                            <asp:CheckBoxList ID="cblInterestCategories" runat="server" RepeatLayout="Flow" CssClass="interest-checkboxes">
                                <asp:ListItem Text="Programming" Value="Programming"></asp:ListItem>
                                <asp:ListItem Text="Web Development" Value="Web Development"></asp:ListItem>
                                <asp:ListItem Text="Data Science" Value="Data Science"></asp:ListItem>
                                <asp:ListItem Text="Artificial Intelligence" Value="Artificial Intelligence"></asp:ListItem>
                                <asp:ListItem Text="Mobile Development" Value="Mobile Development"></asp:ListItem>
                                <asp:ListItem Text="Cloud Computing" Value="Cloud Computing"></asp:ListItem>
                                <asp:ListItem Text="Cybersecurity" Value="Cybersecurity"></asp:ListItem>
                                <asp:ListItem Text="Business" Value="Business"></asp:ListItem>
                                <asp:ListItem Text="Design" Value="Design"></asp:ListItem>
                                <asp:ListItem Text="Marketing" Value="Marketing"></asp:ListItem>
                                <asp:ListItem Text="Finance" Value="Finance"></asp:ListItem>
                                <asp:ListItem Text="Language Learning" Value="Language Learning"></asp:ListItem>
                            </asp:CheckBoxList>
                        </div>
                    </div>
                    
                    <!-- Custom Interest -->
                    <div class="form-group">
                        <label class="form-label">Add Custom Interest</label>
                        <div class="custom-interest-container">
                            <asp:TextBox ID="txtCustomInterest" runat="server" CssClass="form-control" placeholder="Enter your interest (e.g., Python, Machine Learning, etc.)"></asp:TextBox>
                            <asp:Button ID="btnAddCustomInterest" runat="server" Text="Add" CssClass="add-btn" OnClick="btnAddCustomInterest_Click" />
                        </div>
                    </div>
                    
                    <!-- Save Button -->
                    <div class="btn-container">
                        <asp:Button ID="btnSaveInterests" runat="server" Text="Save Interests" CssClass="btn-save" OnClick="btnSaveInterests_Click" />
                    </div>
                </div>
            </div>