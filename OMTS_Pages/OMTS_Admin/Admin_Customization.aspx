<%@ Page Language="C#" AutoEventWireup="true" Async="true" CodeBehind="Admin_Customization.aspx.cs" Inherits="fyp.WebForm1"  MaintainScrollPositionOnPostBack="true" MasterPageFile="~/Site1.Master" %>

<asp:Content ID="ContentTitle" ContentPlaceHolderID="title" runat="server">Website Customisation</asp:Content>
<asp:Content ID="ContentStyle" ContentPlaceHolderID="css" runat="server">
    <link rel="stylesheet" type="text/css" href="#" />
    <style>
        /* Add custom styles for the customization page */
        .color-box {
            width: 30px;
            height: 30px;
            display: inline-block;
            border: 1px solid #ccc;
            cursor: pointer;
            margin-right: 5px;
        }
        
        .selected-color {
            border: 2px solid black;
        }
        
        .color-palette {
            display: flex;
            flex-wrap: wrap;
            gap: 5px;
            margin-top: 5px;
        }
        
        .theme-preview {
            width: 200px;
            height: 150px;
            border: 1px solid #ddd;
            display: inline-block;
            margin-right: 10px;
            cursor: pointer;
            padding: 10px;
        }
        
        .theme-selected {
            border: 2px solid #4CAF50;
        }
        
        .save-indicator {
            color: green;
            margin-left: 10px;
            display: none;
        }
        
        .upload-preview {
            max-width: 200px;
            max-height: 100px;
            margin-top: 10px;
        }
    </style>
</asp:Content>
<asp:Content ID="ContentScript" ContentPlaceHolderID="script" runat="server">
<script>
    function previewImage(event) {
        var reader = new FileReader();
        reader.onload = function () {
            var output = document.getElementById('logoPreview');
            output.src = reader.result;
        }
        reader.readAsDataURL(event.target.files[0]);

        // Show save indicator
        document.getElementById('logoSaveIndicator').style.display = 'inline';
    }

    function selectTheme(themeId) {
        // Remove selected class from all themes
        var themes = document.getElementsByClassName('theme-preview');
        for (var i = 0; i < themes.length; i++) {
            themes[i].classList.remove('theme-selected');
        }

        // Add selected class to clicked theme
        document.getElementById(themeId).classList.add('theme-selected');

        // Set the hidden field value
        document.getElementById('<%= hdnSelectedTheme.ClientID %>').value = themeId;
        
        // Show save indicator
        document.getElementById('themeSaveIndicator').style.display = 'inline';
    }
    
    function selectLanguage(checkbox) {
        // Show save indicator
        document.getElementById('languageSaveIndicator').style.display = 'inline';
    }
    
    function selectColor(colorBox, colorValue) {
        // Remove selected class from all colors
        var colors = document.getElementsByClassName('color-box');
        for(var i = 0; i < colors.length; i++) {
            colors[i].classList.remove('selected-color');
        }
        
        // Add selected class to clicked color
        colorBox.classList.add('selected-color');

        // Show save indicator
        document.getElementById('colorSaveIndicator').style.display = 'inline';
    }
    // Add this to your existing script block
    function validateMaxPoints() {
        var input = document.getElementById('<%= txtMaxPointsPerLevel.ClientID %>');
        var value = parseInt(input.value);
        
        if (isNaN(value) || value < 1 || value > 1000) {
            input.setCustomValidity("Please enter a number between 1 and 1000");
            return false;
        } else {
            input.setCustomValidity("");
            document.getElementById('maxPointsSaveIndicator').style.display = 'inline';
            return true;
        }
    }
    
    // Add this to your document ready or window.onload
    document.addEventListener('DOMContentLoaded', function() {
        var maxPointsInput = document.getElementById('<%= txtMaxPointsPerLevel.ClientID %>');
        if (maxPointsInput) {
            maxPointsInput.addEventListener('change', validateMaxPoints);
            maxPointsInput.addEventListener('input', function () {
                document.getElementById('maxPointsSaveIndicator').style.display = 'inline';
            });
        }
    });
</script>  
</asp:Content>
<asp:Content ID="ContentMain" ContentPlaceHolderID="MainContent" runat="server">
<body>
    <!-- Main Content Layout -->
    <div class="container">
        <!-- Sidebar -->
        <div class="sidebar">
            <asp:Button ID="btnAccMgmt" runat="server" CssClass='<%# (Request.Url.AbsolutePath.EndsWith("Admin_AccList.aspx")) ? "sidebar-btn active" : "sidebar-btn" %>' Text="<%$ Resources:Resources, SideBtnAM %>" PostBackUrl="~/OMTS_Pages/OMTS_Admin/Admin_AccList.aspx" />
            <asp:Button ID="btnCustom" runat="server" CssClass='<%# (Request.Url.AbsolutePath.EndsWith("Admin_Customization.aspx")) ? "sidebar-btn active" : "sidebar-btn" %>' Text="<%$ Resources:Resources, SideBtnWC %>" PostBackUrl="~/OMTS_Pages/OMTS_Admin/Admin_Customization.aspx" />
            <asp:Button ID="btnCertManager" runat="server" CssClass='<%# (Request.Url.AbsolutePath.EndsWith("Admin_TemplateManager.aspx")) ? "sidebar-btn active" : "sidebar-btn" %>' Text="<%$ Resources:Resources, SideBtnTM %>" PostBackUrl="~/OMTS_Pages/OMTS_Admin/Admin_TemplateManager.aspx" />
        </div>

        <!-- Main Content Area -->
        <div class="main-content">
            <!-- Status Message -->
            <asp:Panel ID="pnlMessage" runat="server" CssClass="message" Visible="false">
                <asp:Label ID="lblMessage" runat="server" Text=""></asp:Label>
            </asp:Panel>
            
            <!-- General Settings Section -->
            <div>
                <h2 class="page-header">
                    <asp:Literal ID="litGeneralSettings" runat="server" Text="<%$ Resources:Resources, Heading_GeneralSettings %>" />
                </h2>
                <table class="form-table">
                    <tr>
                        <td><%= GetGlobalResourceObject("Resources", "Label_OrgName") %>:</td>
                        <td>
                            <asp:TextBox ID="txtOrgName" runat="server" class="text-input" />
                            <span id="orgNameSaveIndicator" class="save-indicator">✓ Changes will be saved</span>
                        </td>
                    </tr>
                    <tr>
                        <td><%= GetGlobalResourceObject("Resources", "Label_OrgLogo") %>:</td>
                        <td>
                            <div class="profile-pic-container">
                                <asp:Image ID="logoPreview" runat="server" AlternateText="Org LOgo" ImageUrl="https://res.cloudinary.com/dkzzqgxce/image/upload/v1741189568/profile_placeholder_wbnidc.png/default-profile.png" />
                                <div class="upload-overlay">
                                    <i class="fa fa-camera"></i>
                                    <span>Click to Upload</span>
                                </div>
                                <asp:FileUpload ID="fuLogo" runat="server" CssClass="file-upload" onchange="previewImage(event)" />
                            </div>
                            <span id="logoSaveIndicator" class="save-indicator">✓ Changes will be saved</span>
                        </td>
                    </tr>
                    <tr>
                        <td><%= GetGlobalResourceObject("Resources", "Label_Theme") %>:</td>
                        <td>
                            <div>
                                <div id="theme1" class="theme-preview" onclick="selectTheme('theme1')">
                                    <div style="background-color:  #f8f8f8; height: 30px; margin-bottom: 10px;"></div>
                                    <div style="background-color: whitesmoke; height: 90px; padding: 10px;">
                                        <div style="background-color: white; height: 70px;"></div>
                                    </div>
                                    <div style="text-align: center; margin-top: 20px;">Light Theme</div>
                                </div>
                                <div id="theme2" class="theme-preview" onclick="selectTheme('theme2')">
                                    <div style="background-color: #333; height: 30px; margin-bottom: 10px;"></div>
                                    <div style="background-color: #555; height: 90px; padding: 10px;">
                                        <div style="background-color: #777; height: 70px;"></div>
                                    </div>
                                    <div style="text-align: center; margin-top: 20px;">Dark Theme</div>
                                </div>
                                <%--<div id="theme3" class="theme-preview" onclick="selectTheme('theme3')">
                                    <div style="background-color: #2196F3; height: 30px; margin-bottom: 10px;"></div>
                                    <div style="background-color: #e3f2fd; height: 90px; padding: 10px;">
                                        <div style="background-color: white; height: 70px;"></div>
                                    </div>
                                    <div style="text-align: center; margin-top: 5px;">Blue Theme</div>
                                </div>--%>
                                <span id="themeSaveIndicator" class="save-indicator">✓ Changes will be saved</span>
                                <asp:HiddenField ID="hdnSelectedTheme" runat="server" />
                            </div>
                        </td>
                    </tr>
                    <tr>
                        <td><%= GetGlobalResourceObject("Resources", "Label_MaxPointsPerLevel") ?? "Max Points Per Level" %>:</td>
                        <td>
                            <asp:TextBox ID="txtMaxPointsPerLevel" runat="server" class="text-input" TextMode="Number" min="1" max="1000" />
                            <span id="maxPointsSaveIndicator" class="save-indicator">✓ Changes will be saved</span>
                            <br />
                            <asp:RegularExpressionValidator ID="regMaxPoints" runat="server"
                                ControlToValidate="txtMaxPointsPerLevel"
                                ValidationExpression="^[1-9][0-9]{0,3}$"
                                ErrorMessage="Please enter a valid number between 1 and 1000"
                                Display="Dynamic" CssClass="validator-message" />
                        </td>
                    </tr>
                   <%-- <tr>
                        <td><%= GetGlobalResourceObject("Resources", "Label_Font") %>:</td>
                        <td>
                            <asp:DropDownList ID="ddlFont" runat="server" class="text-input" AutoPostBack="true" OnSelectedIndexChanged="ddlFont_SelectedIndexChanged">
                                <asp:ListItem Text="Arial" Value="Arial"></asp:ListItem>
                                <asp:ListItem Text="Verdana" Value="Verdana"></asp:ListItem>
                                <asp:ListItem Text="Roboto" Value="Roboto"></asp:ListItem>
                                <asp:ListItem Text="Open Sans" Value="Open Sans"></asp:ListItem>
                                <asp:ListItem Text="Montserrat" Value="Montserrat"></asp:ListItem>
                            </asp:DropDownList>
                            <span id="fontSaveIndicator" class="save-indicator">✓ Changes will be saved</span>
                        </td>
                    </tr>--%>
                    <!-- Replace the language checkboxes with a dropdown -->
                    <tr>
                        <td><%= GetGlobalResourceObject("Resources", "Label_Languages") %>:</td>
                        <td>
                            <asp:DropDownList ID="ddlLanguage" runat="server" class="text-input" AutoPostBack="true" OnSelectedIndexChanged="ddlLanguage_SelectedIndexChanged">
                                <asp:ListItem Text="English" Value="en"></asp:ListItem>
                                <asp:ListItem Text="简体中文" Value="zh"></asp:ListItem>
                                
                            </asp:DropDownList>
                            <span id="languageSaveIndicator" class="save-indicator">✓ Changes will be saved</span>
                        </td>
                    </tr>
                </table>
                
                <div class="form-buttons">
                    <asp:Button ID="btnSaveGeneralSettings" runat="server" Text="Save Changes" CssClass="btn" OnClick="btnSaveGeneralSettings_Click" />
                </div>
            </div>

            <!-- Content Customization Section -->
            <div>
                <h2 class="page-header">
                    <asp:Literal ID="litContentCus" runat="server" Text="<%$ Resources:Resources, Heading_ContentCus %>" />
                </h2>
                <div class="alternate-link" style="text-align: left;"">
                    <asp:HyperLink ID="lnkCourse" runat="server" Text="<%$ Resources:Resources, Hyperlink_Courses %>" NavigateUrl="~/Course.aspx" /><br />
                    <asp:HyperLink ID="lnkEvents" runat="server" Text="<%$ Resources:Resources, Hyperlink_Events %>" NavigateUrl="~/Events.aspx" /><br />
                    <asp:HyperLink ID="lnkAchievement" runat="server" Text="<%$ Resources:Resources, Hyperlink_Achievements %>" NavigateUrl="~/Achievement.aspx" />
                </div>
            </div>
        </div>
    </div>
</body>
</asp:Content>