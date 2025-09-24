<%@ Page Title="" Async="true" Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="MA_AchievementApplication.aspx.cs" Inherits="fyp.MA_AchievementApplication" %>


<asp:Content ID="ContentTitle" ContentPlaceHolderID="title" runat="server">Achievement Application</asp:Content>
<asp:Content ID="ContentStyle" ContentPlaceHolderID="css" runat="server">
    <link rel="stylesheet" type="text/css" href="#" />
    <style>
       
    </style>
</asp:Content>
<asp:Content ID="ContentScript" ContentPlaceHolderID="script" runat="server">
    <script>
        function previewImage(event) {
            // Get the file input element
            var fileInput = event.target;

            // Check if a file is selected
            if (fileInput.files && fileInput.files[0]) {
                // Get the selected file
                var file = fileInput.files[0];

                // Create a FileReader to read the file
                var reader = new FileReader();

                // Set up the FileReader onload event
                reader.onload = function (e) {
                    // Set the src attribute of the img element to the file's data URL
                    var imgElement = document.getElementById('attchPreview');
                    imgElement.src = e.target.result;
                };

                // Read the file as a data URL (base64 encoded)
                reader.readAsDataURL(file);
            }
        }
    </script>  
</asp:Content>
<asp:Content ID="ContentMain" ContentPlaceHolderID="MainContent" runat="server">
    <body>
    
        <!-- Main Content Layout -->
        <div class="container">
            <!-- Sidebar -->
            <div class="sidebar">
                <asp:Button ID="btnInformation" runat="server" CssClass='sidebar-btn' Text="<%$ Resources:Resources, SideBtnInfo %>" PostBackUrl="~/OMTS_Pages/OMTS_MA/MA_Information.aspx" />
                <asp:Button ID="btnDashboard" runat="server" CssClass='sidebar-btn' Text="<%$ Resources:Resources, SideBtnDb %>" PostBackUrl="~/OMTS_Pages/OMTS_MA/MA_AchievementDashboard.aspx" />
                <asp:Button ID="btnApplicationForm" runat="server" CssClass='sidebar-btn active' Text="<%$ Resources:Resources, SideBtnMAApp %>" PostBackUrl="~/OMTS_Pages/OMTS_MA/MA_AchievementApplication.aspx" />
                <asp:Button ID="btnReviewApp" runat="server" CssClass='sidebar-btn' Text="<%$ Resources:Resources, SideBtnRA %>" PostBackUrl="~/OMTS_Pages/OMTS_MA/MA_ReviewApp.aspx" />
                <asp:Button ID="btnAwardBadge" runat="server" CssClass='sidebar-btn' Text="<%$ Resources:Resources, SideBtnAB%>" PostBackUrl="~/OMTS_Pages/OMTS_MA/MA_AwardBadges.aspx" />
                <asp:Button ID="btnMaterials" runat="server" CssClass='sidebar-btn' Text="<%$ Resources:Resources, SideBtnMAMat%>" PostBackUrl="~/OMTS_Pages/OMTS_MA/MA_Materials.aspx" />

            </div>

             <!-- Main Content Area -->
             <div class="main-content">
                 <h2><asp:Literal ID="litHeading" runat="server" Text="<%$ Resources:Resources, Heading_AchievementApplication %>" /></h2>
                  <!-- Invisible Table for Layout -->
                
                <table class="form-table">
                    <tr>
                        <td colspan="2"><asp:Label ID="lblApplication" runat="server" Text="New" Visible="False" /></td>
                    </tr>
                    <tr>
                        <td><%= GetGlobalResourceObject("Resources", "Label_AAEvent") %>:</td>
                        <td>
                            <asp:DropDownList ID="ddlEvent" runat="server" CssClass="text-input"></asp:DropDownList>
                        </td>
                    </tr>
                    <tr>
                        <td colspan="2"  >
                            <p class="subtext">
                                <asp:Literal ID="litEvent" runat="server" Text="<%$Resources:Resources, EventNote %>"/>
                            </p>
                        </td>
                    </tr>
                    <tr>
                        <td><%= GetGlobalResourceObject("Resources", "Label_AARole") %>:</td>
                        <td>
                            <asp:DropDownList ID="ddlRole" runat="server" CssClass="text-input"></asp:DropDownList>
                        </td>
                    </tr>
                    <tr>
                        <td><%= GetGlobalResourceObject("Resources", "Label_AALearningOutcome") %>:</td>
                        <td><asp:TextBox ID="txtLearningOutcome" runat="server" CssClass="text-input" /></td>
                    </tr>
                    <tr>
                        <td><%= GetGlobalResourceObject("Resources", "Label_AAUploadAttachment") %>:</td>
                        <td>
                                 <asp:TextBox ID="txtAttachment" runat="server" CssClass="form-control" style="width:200%;" placeholder="Enter Google Drive link (must be publicly accessible)"></asp:TextBox>
                        </td>
                    </tr>

                    <tr>
                        <td colspan="2"  >
                            <p class="subtext">
                                <asp:Literal ID="litAttchNote" runat="server" Text="<%$Resources:Resources, ApplAttachmentNote %>"/>
                            </p>
                        </td>
                    </tr>
                </table>

                 <!-- Buttons Section -->
                 <div class="form-buttons">
                     <asp:Button ID="btnSubmit" runat="server" Text="<%$ Resources:Resources, Button_Submit %>" CssClass="btn" OnClick="btnSubmit_Click" />
                 </div>
            </div>
        </div>
        </body>
</asp:Content>
