<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="OC_EditCourse.aspx.cs" 
Inherits="fyp.OC_EditCourse" MasterPageFile="~/Site1.Master" Async="true" %>

<asp:Content ID="ContentTitle" ContentPlaceHolderID="title" runat="server">Edit Course</asp:Content>

<asp:Content ID="ContentStyle" ContentPlaceHolderID="css" runat="server">
    <style>
        .form-group {
            margin-bottom: 20px;
        }

        .form-label {
            display: block;
            margin-bottom: 5px;
            font-weight: bold;
        }

        .form-control {
            width: 100%;
            padding: 8px;
            border: 1px solid #ddd;
            border-radius: 4px;
            box-sizing: border-box;
        }

        .text-area {
            min-height: 100px;
            resize: vertical;
        }

        .btn-container {
            margin-top: 30px;
            display: flex;
            gap: 10px;
        }

        .btn-save {
            padding: 10px 20px;
            background-color: #4CAF50;
            color: white;
            border: none;
            border-radius: 4px;
            cursor: pointer;
        }

        .btn-save:hover {
            background-color: #45a049;
        }

        .btn-cancel {
            padding: 10px 20px;
            background-color: #f44336;
            color: white;
            border: none;
            border-radius: 4px;
            cursor: pointer;
        }

        .btn-cancel:hover {
            background-color: #d32f2f;
        }

        .error-message {
            color: red;
            margin-top: 10px;
        }
        .added-materials-container {
            margin-top: 10px;
        }

        .material-item {
            display: flex;
            justify-content: space-between;
            align-items: center;
            padding: 8px 12px;
            margin-bottom: 8px;
            background-color: #f8f9fa;
            border-radius: 4px;
            border: 1px solid #dee2e6;
        }

        .btn-sm {
            padding: 5px 10px;
            font-size: 12px;
        }

        .no-materials {
            padding: 10px;
            color: #6c757d;
            font-style: italic;
        }
</style>
</asp:Content>

<asp:Content ID="ContentMain" ContentPlaceHolderID="MainContent" runat="server">
    <body>
    <div class="container">
        <!-- Sidebar -->
        <div class="sidebar">
            <asp:Button ID="btnMyCourse" runat="server" CssClass='sidebar-btn' 
                Text="My Courses" PostBackUrl="~/OMTS_Pages/OMTS_OC/OC_Courses.aspx" />
            <asp:Button ID="btnJoinCourse" runat="server" CssClass='sidebar-btn' 
                Text="Join Course" PostBackUrl="~/OMTS_Pages/OMTS_OC/OC_JoinCourse.aspx" />
            <asp:Button ID="btnCompletedCourse" runat="server" CssClass='sidebar-btn' 
                Text="Completed Courses" PostBackUrl="~/OMTS_Pages/OMTS_OC/OC_CompletedCourse.aspx" />
            <asp:Button ID="btnCourseManagement" runat="server" CssClass='sidebar-btn active' 
                Text="Course Management" PostBackUrl="~/OMTS_Pages/OMTS_OC/OC_CourseManagement.aspx" />
        </div>

        <!-- Main Content Area -->
        <div class="main-content">
            <h2 class="page-header">
                <asp:Literal ID="litAddCourse" runat="server" Text="Add New Course" />
            </h2>

            <!-- Course Form -->
            <div class="form-container">
                <!-- Course Title -->
                <div class="form-group">
                    <label for="txtCourseName" class="form-label">Course Name:</label>
                    <asp:TextBox ID="txtCourseName" runat="server" CssClass="form-control" 
                        placeholder="Enter course name"></asp:TextBox>
                </div>

                <!-- Course Description -->
                <div class="form-group">
                    <label for="txtCourseDescription" class="form-label">Course Description:</label>
                    <asp:TextBox ID="txtCourseDescription" runat="server" CssClass="form-control text-area" 
                        TextMode="MultiLine" Rows="5" placeholder="Enter course description"></asp:TextBox>
                </div>

                <!-- Course Category -->
                <div class="form-group">
                    <label for="ddlCourseCategory" class="form-label">Course Category:</label>
                    <asp:DropDownList ID="ddlCourseCategory" runat="server" CssClass="form-control">
                        <asp:ListItem Text="Select Category" Value="" />
                        <asp:ListItem Text="History" Value="History" />
                        <asp:ListItem Text="Leadership" Value="Leadership" />
                        <asp:ListItem Text="Language" Value="Language" />
                        <asp:ListItem Text="Camping" Value="Camping" />
                        <asp:ListItem Text="Technology" Value="Technology" />
                    </asp:DropDownList>
                </div>

                <!-- Maximum Students -->
                <div class="form-group">
                    <label for="ddlMaxStudents" class="form-label">Maximum Students:</label>
                    <asp:DropDownList ID="ddlMaxStudents" runat="server" CssClass="form-control">
                        <asp:ListItem Text="10" Value="10" />
                        <asp:ListItem Text="20" Value="20" />
                        <asp:ListItem Text="30" Value="30" />
                        <asp:ListItem Text="50" Value="50" />
                        <asp:ListItem Text="100" Value="100" />
                    </asp:DropDownList>
                </div>

                <!-- Google Drive Materials -->
                <div class="form-group">
                    <label for="txtDriveLink" class="form-label">Google Drive Material Link:</label>
                    <asp:TextBox ID="txtDriveLink" runat="server" CssClass="form-control" placeholder="Enter Google Drive link (must be publicly accessible)"></asp:TextBox>
                </div>

                <div class="form-group">
                    <label for="txtDriveLinkName" class="form-label">Material Name:</label>
                    <asp:TextBox ID="txtDriveLinkName" runat="server" CssClass="form-control" placeholder="Enter a name for this material"></asp:TextBox>
                </div>

                <div class="form-group">
                    <asp:Button ID="btnAddDriveLink" runat="server" Text="Add Google Drive Link" CssClass="btn btn-secondary" OnClick="btnAddDriveLink_Click" />
                </div>

                <div class="form-group">
                    <label class="form-label">Added Google Drive Materials:</label>
                    <div class="added-materials-container">
                        <asp:Repeater ID="rptAddedMaterials" runat="server" OnItemCommand="rptAddedMaterials_ItemCommand">
                            <ItemTemplate>
                                <div class="material-item">
                                    <span><%# Container.DataItem %> (Google Drive)</span>
                                    <asp:Button ID="btnRemove" runat="server" 
                                              Text="Remove" 
                                              CommandName="Remove" 
                                              CommandArgument='<%# Container.DataItem %>'
                                              CssClass="btn btn-danger btn-sm" />
                                </div>
                            </ItemTemplate>
                        </asp:Repeater>
        
                        <asp:Panel ID="pnlNoMaterials" runat="server" CssClass="no-materials" Visible="false">
                            No Google Drive materials added yet.
                        </asp:Panel>
                    </div>
                </div>
                <!-- Error Messages -->
                <asp:Label ID="lblMessage" runat="server" CssClass="error-message"></asp:Label>

                <!-- Form Buttons -->
                <div class="btn-container">
                    <asp:Button ID="btnSaveCourse" runat="server" Text="Save Course" 
                        CssClass="btn-save" OnClick="btnSaveCourse_Click" />
                    <asp:Button ID="btnCancel" runat="server" Text="Cancel" 
                        CssClass="btn-cancel" OnClick="btnCancel_Click" />
                </div>
            </div>
        </div>
    </div>
</body>
</asp:Content>