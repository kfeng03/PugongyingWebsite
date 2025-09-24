<%@ Page Title=""  Async="true"  Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="AM_UpdatePersonalInfo.aspx.cs" Inherits="fyp.UpdatePersonalInfo" %>

<asp:Content ID="ContentTitle" ContentPlaceHolderID="title" runat="server">Update Personal Information</asp:Content>
<asp:Content ID="ContentStyle" ContentPlaceHolderID="css" runat="server">
    <link rel="stylesheet" type="text/css" href="#" />
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.0.0-beta3/css/all.min.css" />
    <style>
        .dataContent{
            font-weight: bold;
            font-size:larger;
        }

        .profile-pic-actions {
            margin-top: 10px;
            text-align: center;
        }

        .btn-danger {
            background-color: #dc3545;
            color: white;
            border: none;
            padding: 8px 16px;
            border-radius: 4px;
            cursor: pointer;
        }

        .btn-danger:hover {
            background-color: #c82333;
        }
        
        .popup{
            display: none;
            position: fixed;
            top: 50%;
            left: 55%;
            transform: translate(-50%, -50%);
            width: 50%;
            padding: 20px;
            background-color: #fff;
            border: 1px solid #ccc;
            box-shadow: 0 4px 8px rgba(0, 0, 0, 0.2);
            z-index: 1000;
            border-radius: 10px;
        }

        .overlay {
            display: none;
            position: fixed;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            background-color: rgba(0, 0, 0, 0.5);
            z-index: 999;
        }

        .tag-container {
            display: flex;
            flex-wrap: wrap;
            gap: 10px;
        }

        .tag {
            padding: 5px 10px;
            background-color: #e0e0e0;
            border-radius: 15px;
            cursor: pointer;
            user-select: none;
        }

        .tag.selected {
            background-color: #4CAF50;
            color: white;
        }
        level-section {
            margin-bottom: 15px;
        }

        .level-bar {
            display: flex;
            align-items: center;
            gap: 10px; /* Add spacing between elements */
            position: relative; /* Required for absolute positioning of the text */
        }


        .progress-container {
            width: 200px;
            height: 15px;
            background-color: #e0e0e0;
            border-radius: 8px;
            position: relative;
            overflow: hidden;
        }

        .progress-bar {
            height: 100%;
            background-color: #4caf50; /* Green progress color */
            width: 0%; /* Default, updated dynamically */
            transition: width 0.5s ease-in-out;
        }

        .progress-text {
            font-size: 14px;
            font-weight:bold;
            position: absolute; /* Position the text absolutely within the level bar */
            left: 20%; /* Center horizontally */
            top: 10%; /* Center vertically */
            white-space: nowrap; /* Prevent text from wrapping */
        }

    </style>
</asp:Content>
<asp:Content ID="ContentScript" ContentPlaceHolderID="script" runat="server">
    <script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>
    <script type="text/javascript">
        $(document).ready(function () {           
        
        // Toggle tag selection
        $(".tag").click(function () {
            $(this).toggleClass("selected");
            // Update the hidden field with the current selection
            updateSelectedTags();
        });

        // Enable username editing and store the updated value
        $("#<%= btnEditUsername.ClientID %>").click(function () {
            $("#<%= txtUsername.ClientID %>").prop("disabled", false);
            $(this).hide();
        });

        function toggleEditMode() {
            // Toggle visibility of the Repeater and tag-container
            $("#<%= rptInterests.ClientID %>").toggle(); // Hide/Show Repeater
            $("#tagContainer").toggle(); // Show/Hide tag-container
        }
        // Function to update the hidden field with selected tags
        function updateSelectedTags() {
            var selectedTags = [];
            $(".tag.selected").each(function () {
                selectedTags.push($(this).attr("data-tag"));
            });

            // Update the hidden field with selected tags
            $("#<%= hdnSelectedInterests.ClientID %>").val(selectedTags.join(","));
            }
            
        });


        $(document).ready(function () {

            // File selection and preview
            $("#<%= fuProfilePic.ClientID %>").on('change', function(event) {
        // Check if a file is selected
        if (this.files && this.files[0]) {
            var reader = new FileReader();
            
            reader.onload = function(e) {
                // Update the profile preview image source
                $("#<%= profilePreview.ClientID %>").attr('src', e.target.result);
            }
            
            // Read the selected file as a data URL
            reader.readAsDataURL(this.files[0]);
            
        }
    });
        });
    </script>  
</asp:Content>
<asp:Content ID="ContentMain" ContentPlaceHolderID="MainContent" runat="server">
    <body>
    
        <!-- Main Content Layout -->
        <div class="container">
             <!-- Sidebar -->
             <div class="sidebar">
                 <asp:Button ID="btnPersonalInfo" runat="server" CssClass='<%# (Request.Url.AbsolutePath.EndsWith("AM_UpdatePersonalInfo.aspx")) ? "sidebar-btn active" : "sidebar-btn" %>' Text="<%$ Resources:Resources, SideBtnPI %>" PostBackUrl="~/OMTS_Pages/OMTS_AM/AM_UpdatePersonalInfo.aspx" />
                 <asp:Button ID="btnAccSecurity" runat="server" CssClass='<%# (Request.Url.AbsolutePath.EndsWith("AM_UpdateAccountSecurity.aspx")) ? "sidebar-btn active" : "sidebar-btn" %>' Text="<%$ Resources:Resources, SideBtnAS %>" PostBackUrl="~/OMTS_Pages/OMTS_AM/AM_UpdateAccountSecurity.aspx" />
             </div>

             <!-- Main Content Area -->
             <div class="main-content">
                 <h2><asp:Literal ID="litHeading" runat="server" Text="<%$ Resources:Resources, Heading_PersonalInfo %>" /></h2>
                            <!-- Wrap the Repeater and Popup in an UpdatePanel -->
                <table class="form-table">
                    <tr>
                        <td><asp:Label ID="lblUsername" runat="server" Text="<%$ Resources:Resources, Label_Username %>" /></td>
                        <td>
                            <asp:TextBox ID="txtUsername" runat="server" CssClass="form-control" Enabled="false" OnTextChanged="txtUsername_TextChanged" />
                            <asp:Button ID="btnEditUsername" runat="server" Text="<%$ Resources:Resources, Button_Edit %>" CssClass="btn" OnClick="btnEditUsername_Click" />
                        </td>
                    </tr>
        
                    <tr>
                        <td><asp:Label ID="lblProfilePic" runat="server" Text="<%$ Resources:Resources, Label_ProfilePicture %>" /></td>
                        <td>
                            <div class="profile-pic-container">
                                <asp:Image ID="profilePreview" runat="server" AlternateText="Profile Picture" ImageUrl="https://res.cloudinary.com/dkzzqgxce/image/upload/v1741189568/profile_placeholder_wbnidc.png/default-profile.png" />
                                <div class="upload-overlay" >
                                    <i class="fa fa-camera"></i>
                                    <span>Click to Upload</span>
                                </div>
                                <asp:FileUpload ID="fuProfilePic" runat="server" CssClass="file-upload" />
                                    
                            </div>
                            <asp:Button ID="btnRemoveProfilePic" runat="server" Text="Remove Profile Picture" 
    CssClass="btn" OnClick="btnRemoveProfilePic_Click" />
                            <asp:HiddenField ID="hdnResetProfilePic" runat="server" Value="false" />
                        </td>
                    </tr>
        
                    <tr>
                        <td><asp:Label ID="lblRole" runat="server" Text="<%$Resources:Resources,  Label_AARole %>" /></td>
                        <td>
                            <asp:Label ID="litRole" runat="server" CssClass="dataContent" />
                        </td>
                    </tr>
        
                    <tr>
                        <td><asp:Label ID="lblCurrentLevel" runat="server" Text="<%$ Resources:Resources, Label_CurrentLevel %>" /></td>
                        <td>
                            <div class="level-bar">
                                 <asp:Label ID="lblCurrLvl" runat="server" Text="currLvl"  style="padding-right:3%;"/>                                    
                                    <!-- Progress Bar -->
                                <div class="progress-container">
                                    <div id="progressBar" runat="server" class="progress-bar">
                                    </div>
                                <asp:Label ID="lblPoints" runat="server" CssClass="progress-text" Text="curr/max Points"></asp:Label>

                                </div>
                                <asp:Label ID="lblNextLvl" runat="server" Text="nextLvl" style="padding-left:3%;"/>
                            </div>
                        </td>
                    </tr>
        
                    <tr>
                        <td  colspan="2" class="alternate-link">
                            <asp:HyperLink ID="lnkAchievementDashboard" runat="server" Text="<%$ Resources:Resources, Hyperlink_AchievementDashboardText %>" NavigateUrl="~/OMTS_Pages/OMTS_MA/MA_AchievementDashboard.aspx" />
                        </td>
                    </tr>
        
                    <tr>
                        <td>
                            <asp:Label ID="lblAreaOfInterest" runat="server" Text="<%$ Resources:Resources, Label_AreaOfInterest %>" />

                        </td>
                        <td>
                            <!-- Repeater for Interests -->
                            <asp:Repeater ID="rptInterests" runat="server">
                                <ItemTemplate>
                                    <span class="interest-tag"><%# Eval("InterestName") %></span>
                                </ItemTemplate>
                            </asp:Repeater>

                            <asp:Panel ID="pnlTagContainer" runat="server" Visible="false">
                                <div class="tag-container" id="tagContainer">
                                    <!-- Predefined Tags -->
                                    <div class="tag" data-tag="History">History</div>
                                    <div class="tag" data-tag="Leadership">Leadership</div>
                                    <div class="tag" data-tag="Language">Language</div>
                                    <div class="tag" data-tag="Camping">Camping</div>
                                    <div class="tag" data-tag="Technology">Technology</div>
                                    <!-- Add more tags as needed -->
                                </div>
                            </asp:Panel>
                            <asp:HiddenField ID="hdnSelectedInterests" runat="server" />
                            <asp:Button ID="btnToggleEdit" runat="server" Text="Edit" OnClick="btnToggleEdit_Click" CssClass="btn"/>                                    
                                
                        </td>
                    </tr>
        
                    <tr>
                        <td colspan="2"  >
                            <p class="subtext">
                                <asp:Literal ID="litInterestNote" runat="server" Text="<%$Resources:Resources, InterestNote %>"/>
                            </p>
                        </td>
                    </tr>
                </table>
                 
                 <!-- Buttons Section -->
                 <div class="form-buttons">
                     <asp:Button ID="btnSaveChanges" runat="server" Text="<%$ Resources:Resources, Button_SaveChanges %>" CssClass="btn" OnClick="btnSaveChanges_Click" />
                 </div>
                 <asp:HiddenField ID="hdnUpdatedUsername" runat="server" />
                
            </div>
        </div>
        </body>
</asp:Content>
