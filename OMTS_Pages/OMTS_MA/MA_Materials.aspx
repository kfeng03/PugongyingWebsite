<%@ Page Async="true" Language="C#" AutoEventWireup="true" CodeBehind="MA_Materials.aspx.cs" MaintainScrollPositionOnPostBack="true" Inherits="fyp.MA_Material" MasterPageFile="~/Site1.Master"  %>

<asp:Content ID="ContentTitle" ContentPlaceHolderID="title" runat="server">Manage Materials</asp:Content>
<asp:Content ID="ContentStyle" ContentPlaceHolderID="css" runat="server">
    <link rel="stylesheet" type="text/css" href="#" />
    <style>
        .popup, .analysis-popup, .badge-popup, .delete-popup, .recycle-bin-popup {
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
            max-height: 80vh;
            overflow-y: auto;
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

        /* New step navigation styles */
        .step { display: none; }
        .step.active { display: block; }
        .step-buttons { margin-top: 20px; }

        .points{
            width:15%;
        }

        .popup-table{
            width:100%;
        }

        .popupform-control{
            width:600px;
            padding: 8px;
            border: 1px solid #ccc;
            border-radius: 4px;
            font-size: 1rem;
        }


        .badge-item .badge-details {
            flex: 1;
        }

        .badge-item .badge-buttons {
            gap: 10px;
            padding:3%;
        }

        .header-actions {
            display: flex;
            justify-content: space-between;
            align-items: center;
            margin-bottom: 20px;
        }

        /* Add these styles to your ContentStyle section in MA_Materials.aspx */

        /* Canvas container styles */
        .canvas-container {
            width: 100%;
            text-align: center;
            margin: 20px 0;
            overflow: auto;
            max-height: 500px;
            border: 1px solid #ddd;
            background-color: #f9f9f9;
        }

        #visualizationCanvas {
            max-width: 100%;
            height: auto;
            display: block;
            margin: 0 auto;
            background-color: white;
        }

        /* Field legend styles */
        .field-legend {
            display: flex;
            flex-wrap: wrap;
            gap: 20px;
            justify-content: center;
            margin: 10px 0;
            padding: 10px;
            background-color: #f5f5f5;
            border-radius: 5px;
        }

        .legend-item {
            display: flex;
            align-items: center;
            gap: 5px;
        }

        .color-box {
            width: 20px;
            height: 20px;
            border: 1px solid #666;
        }

        .text-field {
            background-color: rgba(0, 162, 232, 0.4);
        }

        .date-field {
            background-color: rgba(255, 127, 39, 0.4);
        }

        .number-field {
            background-color: rgba(181, 230, 29, 0.4);
        }

        /* Analysis results styles */
        .text-analysis {
            background-color: #f9f9f9;
            border: 1px solid #ddd;
            padding: 15px;
            border-radius: 5px;
            margin: 20px 0;
            max-height: 200px;
            overflow-y: auto;
        }

        .text-analysis pre {
            margin: 0;
            white-space: pre-wrap;
            font-family: Consolas, monospace;
            font-size: 14px;
            line-height: 1.4;
        }

        /* Analysis popup styles */
        .analysis-popup {
            width: 80% !important;
            max-width: 1000px;
            max-height: 90vh;
            overflow-y: auto;
        }

        /* Step navigation */
        .step-buttons {
            margin-top: 20px;
            display: flex;
            justify-content: space-between;
        }

        .form-control {
            width:70%;
        }
        
    </style>
</asp:Content>
<asp:Content ID="ContentScript" ContentPlaceHolderID="script" runat="server">
    <script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>
    <script type="text/javascript">
        $(document).ready(function () {
            // Function to preview image
            function previewImage(input, previewId) {
                if (input.files && input.files[0]) {
                    var reader = new FileReader();
                    reader.onload = function (e) {
                        $('#' + previewId).attr('src', e.target.result);
                    }
                    reader.readAsDataURL(input.files[0]);
                }
            }

            $(document).ready(function () {
                $("#<%= fuInfoImage.ClientID %>").change(function () {
                previewImage(this, '<%= imgInfoPreview.ClientID %>');
                 });
            });


        });

        // Function to show/hide popups - controlled by server
        function togglePopup(popupId, show) {
            if (show) {
                $(".overlay").fadeIn();
                $("#" + popupId).fadeIn();
            } else {
                $(".overlay").fadeOut();
                $("#" + popupId).fadeOut();
            }
        }

        $(document).ready(function () {
            // Function to preview image
            function previewImage(input, previewId) {
                if (input.files && input.files[0]) {
                    var reader = new FileReader();
                    reader.onload = function (e) {
                        $('#' + previewId).attr('src', e.target.result);
                    }
                    reader.readAsDataURL(input.files[0]);
                }
            }

            // Set up image preview for badge icon upload
            $("#<%= fuBadge.ClientID %>").change(function () {
            previewImage(this, '<%= badgePreview.ClientID %>');
        });
        });

        // Draw fields on canvas function
        function drawFieldsOnCanvas(templateUrl, fieldData) {
            const canvas = document.getElementById('visualizationCanvas');
            if (!canvas) return;

            const ctx = canvas.getContext('2d');

            // Clear the canvas
            ctx.clearRect(0, 0, canvas.width, canvas.height);

            // Draw the template image as background
            const img = new Image();
            img.onload = function () {
                // Resize canvas to match image
                canvas.width = img.width;
                canvas.height = img.height;

                // Draw image
                ctx.drawImage(img, 0, 0);

                // Draw each field rectangle
                for (const field of fieldData) {
                    // Set color based on field type
                    let color;
                    switch (field.type) {
                        case 'Date': color = 'rgba(255, 127, 39, 0.4)'; break;
                        case 'Number': color = 'rgba(181, 230, 29, 0.4)'; break;
                        default: color = 'rgba(0, 162, 232, 0.4)'; break; // Text
                    }

                    // Draw rectangle
                    ctx.fillStyle = color;
                    ctx.fillRect(field.x, field.y, field.width, field.height);

                    // Draw border
                    ctx.strokeStyle = color.replace('0.4', '0.8');
                    ctx.lineWidth = 2;
                    ctx.strokeRect(field.x, field.y, field.width, field.height);

                    // Draw label
                    const labelText = field.label;
                    ctx.font = 'bold 12px Arial';
                    const textWidth = ctx.measureText(labelText).width;

                    // Background for label
                    ctx.fillStyle = 'white';
                    ctx.fillRect(field.x, field.y - 20, textWidth + 10, 20);
                    ctx.strokeStyle = '#666';
                    ctx.lineWidth = 1;
                    ctx.strokeRect(field.x, field.y - 20, textWidth + 10, 20);

                    // Label text
                    ctx.fillStyle = 'black';
                    ctx.fillText(labelText, field.x + 5, field.y - 5);

                    // Field type and required info (smaller text inside the field)
                    const infoText = `Type: ${field.type}, Required: ${field.required ? 'Yes' : 'No'}`;
                    ctx.font = 'italic 10px Arial';
                    ctx.fillStyle = 'black';

                    // Background for info text
                    const infoWidth = ctx.measureText(infoText).width;
                    ctx.fillStyle = 'rgba(255, 255, 255, 0.8)';
                    ctx.fillRect(field.x + 5, field.y + 5, infoWidth + 10, 16);

                    // Info text
                    ctx.fillStyle = 'black';
                    ctx.fillText(infoText, field.x + 10, field.y + 16);
                }
            };

            // Handle image loading errors
            img.onerror = function () {
                // Draw error message on canvas
                ctx.fillStyle = '#f8f8f8';
                ctx.fillRect(0, 0, 400, 200);
                ctx.font = '14px Arial';
                ctx.fillStyle = 'red';
                ctx.fillText('Error loading template image', 20, 50);
                console.error('Failed to load image:', templateUrl);
            };

            // Set image source to start loading
            img.src = templateUrl;
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
                <asp:Button ID="btnApplicationForm" runat="server" CssClass='sidebar-btn' Text="<%$ Resources:Resources, SideBtnMAApp %>" PostBackUrl="~/OMTS_Pages/OMTS_MA/MA_AchievementApplication.aspx" />
                <asp:Button ID="btnReviewApp" runat="server" CssClass='sidebar-btn' Text="<%$ Resources:Resources, SideBtnRA %>" PostBackUrl="~/OMTS_Pages/OMTS_MA/MA_ReviewApp.aspx" />
                <asp:Button ID="btnAwardBadge" runat="server" CssClass='sidebar-btn' Text="<%$ Resources:Resources, SideBtnAB%>" PostBackUrl="~/OMTS_Pages/OMTS_MA/MA_AwardBadges.aspx" />
                <asp:Button ID="btnMaterials" runat="server" CssClass='sidebar-btn active' Text="<%$ Resources:Resources, SideBtnMAMat%>" PostBackUrl="~/OMTS_Pages/OMTS_MA/MA_Materials.aspx" />
            </div>

             <!-- Main Content Area -->
             <div class="main-content">
                <h2><asp:Literal ID="litHeading1" runat="server" Text="<%$ Resources:Resources, Heading_IntroMedia %>" /></h2>
                <p class="subtext">
                    <asp:Literal ID="litIntroMedia" runat="server" Text="<%$Resources:Resources, Note_IntroMedia %>"/>
                </p>

                <!-- Media Type Selection -->
                 <div class="form-table">
                     <table>
                         <tr>
                             <td>
                            <label for="rblMediaType"><%= GetGlobalResourceObject("Resources", "Label_MediaType") ?? "Media Type" %>:</label></td>
                             <td>
                            <asp:RadioButtonList ID="rblMediaType" runat="server" AutoPostBack="true" 
                                OnSelectedIndexChanged="rblMediaType_SelectedIndexChanged" RepeatDirection="Horizontal" CssClass="radio-list">
                                <asp:ListItem Text="Video Explanation" Value="video" Selected="True" />
                                <asp:ListItem Text="PDF Guide" Value="pdf" />
                                <asp:ListItem Text="Infograph Image" Value="image" />
                            </asp:RadioButtonList></td>
                        </tr>
                        <tr>
                            <td>
                            <label for="txtMediaTitle"><%= GetGlobalResourceObject("Resources", "Label_MediaTitle") ?? "Media Title" %>:</label></td>
                            <td>
                            <asp:TextBox ID="txtMediaTitle" runat="server" CssClass="form-control" placeholder="Enter a title for this media" /></td>
                        </tr>
                        
                        <!-- Video Link Panel -->
                        <asp:Panel ID="pnlVideoLink" runat="server" CssClass="media-panel">
                            <tr>
                                <td>
                                <label for="txtVideoLink"><%= GetGlobalResourceObject("Resources", "Label_VideoLink") ?? "Video Link" %>:</label></td>
                                <td><asp:TextBox ID="txtVideoLink" runat="server" CssClass="form-control" placeholder="Enter YouTube or Google Drive video link" /></td>
                            </tr>
                            <tr><td colspan="2"><p class="subtext">For Google Drive videos, make sure the link has public or organization access.</p></td>
                            </tr>
                        </asp:Panel>
                          
                        <!-- PDF Link Panel -->
                        <asp:Panel ID="pnlPdfLink" runat="server" CssClass="media-panel" Visible="false">
                             <tr>
                             <td>    
                            <label for="txtPdfLink"><%= GetGlobalResourceObject("Resources", "Label_PDFLink") ?? "PDF Link" %>:</label></td>
                             <td>
                                 <asp:TextBox ID="txtPdfLink" runat="server" CssClass="form-control" placeholder="Enter Google Drive or other PDF document link" />
                                 
                             </td>
                                 </tr>
                            <tr>
                                <td colspan="2">
                                <p class="subtext">For Google Drive PDF, make sure the link has public or organization access.</p>
                                    </td>
                            </tr>
                        </asp:Panel>

                        <!-- Image Upload Panel -->
                        <asp:Panel ID="pnlImageUpload" runat="server" CssClass="media-panel" Visible="false">
                            <tr>
                                <td>
                                <label for="fuInfoImage"><%= GetGlobalResourceObject("Resources", "Label_ImageUpload") ?? "Upload File" %>:</label></td>
                                <td>
                                <span class="fileupload-container">
                                    <img id="imgInfoPreview" src="media.png" alt="Infographic Preview" class="uploaded-pic" runat="server" />
                                    <div class="upload-overlay">
                                        <i class="fa fa-camera"></i>
                                        <span>Click to Upload</span>
                                    </div>
                                    <asp:FileUpload ID="fuInfoImage" runat="server" CssClass="file-upload" />
                                </span>
                                </td>
                                </tr>
                            <tr>
                                <td colspan="2">
                                    <p class="subtext">Select a JPG, PNG or GIF image file for your infographic.</p>
                                </td>
                            </tr>
                        </asp:Panel>

                             <tr><td colspan="2">
                                 <span class="form-buttons">
                            <asp:Button ID="btnSubmitMedia" runat="server" Text="<%$ Resources:Resources, Button_Submit %>" 
                                CssClass="btn"  style="margin-top: 0px;" OnClick="btnSubmitMedia_Click" /></span></td>
                             </tr>
                        </table>
                </div>
                 
                 <!-- Badge Management Section with Recycle Bin Button -->
                 <div class="header-actions">
                     <h2><asp:Literal ID="litHeading2" runat="server" Text="<%$ Resources:Resources, Heading_ManageBadge %>" /></h2>
                     <div>
                         <asp:Button ID="btnCreateBadge" runat="server" Text="<%$ Resources:Resources, Button_CreateBadge %>" class="btn" OnClick="btnCreateBadge_Click" />
                         <asp:Button ID="btnShowRecycleBin" runat="server" Text="Recycle Bin" OnClick="btnShowRecycleBin_Click" CssClass="btn" />
                     </div>
                 </div>
                 
                 <!-- Message Label for Errors -->
                 <asp:Label ID="lblMessage" runat="server" ForeColor="Red" EnableViewState="false"></asp:Label>
                 
                 <!-- Active Badges List -->
                 <div class="badges-gallery">
                    <asp:Repeater ID="rptBadges" runat="server">
                        <ItemTemplate>
                            <div class="badge-item">
                                <asp:Image ID="imgBadge" runat="server" 
                                ImageUrl='<%# Eval("BadgeIconUrl") %>'
                                AlternateText='<%# Eval("BadgeName") %>' 
                                CssClass="badge-detail-icon"/>
                                <div class="badge-details" style="width:70%; margin-left:10%;">
                                    <h4><%# Eval("BadgeName") %></h4>
                                    <p><%# Eval("BadgeDesc") %></p>
                                    <p style="font-size:11px;color:gray;">Points Awarded: <%# Eval("BadgePoints") %></p>
                                </div>
                                <div class="badge-buttons">
                                    <asp:Button ID="btnEdit" runat="server" Text="Edit" CssClass="btn" 
                                        CommandName="Edit" CommandArgument='<%# Eval("BadgeId") %>' 
                                        OnCommand="BadgeCommand" />
                                    <asp:Button ID="btnDelete" runat="server" Text="Remove" CssClass="btn btn-danger" 
                                        CommandName="Delete" CommandArgument='<%# Eval("BadgeId") %>' 
                                        OnCommand="BadgeCommand" />
                                </div>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                 </div>
                 
                 <h2><asp:Literal ID="litHeading3" runat="server" Text="<%$ Resources:Resources, Heading_TestimonialTemp %>" /></h2> 
                 <%--<table class="form-table">
                    <tr>
                        <td><%= GetGlobalResourceObject("Resources", "Label_TestimonialTemp") %>:</td> 
                        <td>
                            <div class="fileupload-container">
                                 <img id="refPreview" src="reference.png" alt="Ref Template Preview" class="uploaded-pic" />
                                <div class="upload-overlay">
                                    <i class="fa fa-camera"></i>
                                    <span>Click to Upload</span>
                                </div>
                                <asp:FileUpload ID="fuReference" runat="server" CssClass="file-upload" />
                            </div>
                           
                            <asp:Button ID="btnUploadAnalyze" runat="server" Text="<%$ Resources:Resources, Button_UploadAnalyze%>" 
                                CssClass="btn" OnClick="btnUploadAnalyze_Click" />
                            <asp:Button ID="btnViewResult" runat="server" Text="<%$ Resources:Resources, Button_ViewResult%>" 
                                CssClass="btn" Enabled="false" Visible="false" OnClick="btnViewResult_Click" />
                        </td>
                    </tr>
                 </table>--%>
                <p class="subtext">
                    <asp:Literal ID="litTestimonialTempSubtext" runat="server" Text="<%$Resources:Resources, Note_TestimonialTemp %>"/><br /><br />
                    <div class="alternate-link" style="text-align: left;">
                        <asp:LinkButton ID="lnkAnalyzer" runat="server" Text="<%$ Resources:Resources, Link_TestimonialTemp %>" PostBackUrl="~/OMTS_Pages/OMTS_Admin/Admin_TemplateManager.aspx"/>
                    </div>
                </p>

                 

                <!-- Overlay for Popups -->
                <div id="overlay" class="overlay" runat="server"></div>

                <!-- Create/Edit Badge Popup -->
                <div id="badgePopup" class="badge-popup" runat="server">
                    <h2><asp:Literal ID="litBadgePopupTitle" runat="server" Text="Badge"/></h2>
                    <table class="popup-table">
                        <tr>
                            <td><%= GetGlobalResourceObject("Resources", "Label_BadgeName") %>:</td>
                            <td>
                                <asp:TextBox ID="txtBadgeName" runat="server" CssClass="form-control"></asp:TextBox>
                                <asp:RequiredFieldValidator ID="rfvBadgeName" runat="server" 
                                    ControlToValidate="txtBadgeName" 
                                    ErrorMessage="Badge name is required" 
                                    Display="Dynamic" 
                                    ForeColor="Red"
                                    ValidationGroup="BadgeValidation"></asp:RequiredFieldValidator>
                            </td>
                        </tr>

                        <!-- Badge Icon Section -->
                        <tr>
                            <td><%= GetGlobalResourceObject("Resources", "Label_BadgePicture") %>:</td>
                            <td>
                                <div class="profile-pic-container">
                                    <asp:Image ID="badgePreview" runat="server" AlternateText="Badge Picture" ImageUrl="https://res.cloudinary.com/dkzzqgxce/image/upload/v1741189568/profile_placeholder_wbnidc.png/default-profile.png" />
                                    <div class="upload-overlay">
                                        <i class="fa fa-camera"></i>
                                        <span>Click to Upload</span>
                                    </div>
                                    <asp:FileUpload ID="fuBadge" runat="server" CssClass="file-upload" />
                                </div>
                                <asp:Button ID="btnRemoveBadgeIcon" runat="server" Text="Remove Image" CssClass="btn" 
                                    OnClick="btnRemoveBadgeIcon_Click" CausesValidation="false" />
                                <asp:HiddenField ID="hdnBadgeIconUrl" runat="server" />
                                <asp:HiddenField ID="hdnResetBadgeIcon" runat="server" Value="false" />
                            </td>
                        </tr>>
                        <tr>
                            <td><%= GetGlobalResourceObject("Resources", "Label_BadgeDesc") %>:</td>
                            <td>
                                <asp:TextBox ID="txtBadgeDescription" runat="server" CssClass="popupform-control" TextMode="MultiLine"></asp:TextBox>
                                <asp:RequiredFieldValidator ID="rfvBadgeDesc" runat="server" 
                                    ControlToValidate="txtBadgeDescription" 
                                    ErrorMessage="Badge description is required" 
                                    Display="Dynamic" 
                                    ForeColor="Red"
                                    ValidationGroup="BadgeValidation"></asp:RequiredFieldValidator>
                            </td>
                        </tr>
                        <tr>
                            <td><%= GetGlobalResourceObject("Resources", "Label_Points") %>: </td>
                            <td>
                                <div class="points-section">
                                    <asp:Button ID="btnDecrement" runat="server" Text="-" OnClick="btnDecrement_Click" CausesValidation="false" />
                                    <asp:TextBox ID="txtPointsAwarded" runat="server" TextMode="Number" Text="0" CssClass="points"></asp:TextBox>
                                    <asp:Button ID="btnIncrement" runat="server" Text="+" OnClick="btnIncrement_Click" CausesValidation="false" />
                                    <asp:RangeValidator ID="rvPoints" runat="server" 
                                        ControlToValidate="txtPointsAwarded" 
                                        MinimumValue="0" 
                                        MaximumValue="1000" 
                                        Type="Integer" 
                                        ErrorMessage="Points must be between 0 and 1000" 
                                        Display="Dynamic" 
                                        ForeColor="Red"
                                        ValidationGroup="BadgeValidation"></asp:RangeValidator>
                                </div>
                            </td>
                        </tr>
                        <tr>
                            <td colspan="2">
                                <asp:Label ID="lblBadgeMessage" runat="server" ForeColor="Red" />
                            </td>
                        </tr>
                        <tr>
                            <td colspan="2">
                                <asp:Button ID="btnSaveBadge" runat="server" Text="Save" CssClass="btn" 
                                    OnClick="btnSaveBadge_Click" ValidationGroup="BadgeValidation" />
                                <asp:Button ID="btnCancelBadge" runat="server" Text="Cancel" CssClass="btn" 
                                    OnClick="btnCancelBadge_Click" CausesValidation="false" />
                                <asp:HiddenField ID="hdnBadgeId" runat="server" />
                                <asp:HiddenField ID="hdnBadgeAction" runat="server" />
                            </td>
                        </tr>
                    </table>
                </div>

                <!-- Delete Badge Confirmation Popup -->
                <div id="deletePopup" class="delete-popup" runat="server">
                    <h2>Confirm Remove Badge</h2>
                    <p>Are you sure you want to remove badge: [<asp:Literal ID="litDeleteBadgeName" runat="server"></asp:Literal>] ?</p>
                    <p>This will move the badge to the recycle bin.</p>
                        <asp:Button ID="btnConfirmDelete" runat="server" Text="Remove" CssClass="btn btn-danger" style="background-color:darkred;"
                            OnClick="btnConfirmDelete_Click" />
                        <asp:Button ID="btnCancelDelete" runat="server" Text="Cancel" CssClass="btn" 
                            OnClick="btnCancelDelete_Click" />
                        <asp:HiddenField ID="hdnDeleteBadgeId" runat="server" />
                </div>
                
                <!-- Recycle Bin Popup -->
                <div id="recycleBinPopup" class="recycle-bin-popup" runat="server">
                    <h2>Badge Recycle Bin</h2>
                    <div class="badges-gallery">
                        <asp:Repeater ID="rptDeletedBadges" runat="server">
                            <ItemTemplate>
                                <div class="badge-item" style="width:45%;">
                                    <img src='<%# Eval("BadgeIconUrl") %>' alt="Badge" class="badge-icon" />
                                    <div class="badge-details">
                                        <h4><%# Eval("BadgeName") %></h4>
                                        <p><%# Eval("BadgeDesc") %></p>
                                        <p style="font-size:11px;color:gray;">Points Awarded: <%# Eval("BadgePoints") %></p>
                                    </div>
                                    <div class="badge-buttons">
                                        <div style="padding:5%; width:100%;">
                                        <asp:Button ID="btnRestore" runat="server" Text="Restore" CssClass="btn"
                                            CommandName="Restore" CommandArgument='<%# Eval("BadgeId") %>' 
                                            OnCommand="RecycleCommand" style="width:100%;"/>
                                            </div>
                                        <div  style="padding:5%;width:100%;">
                                        <asp:Button ID="btnPermanentDelete" runat="server" Text="Delete" CssClass="btn"
                                            CommandName="PermanentDelete" CommandArgument='<%# Eval("BadgeId") %>' 
                                            OnCommand="RecycleCommand" style="width:100%; background-color:darkred"/>
                                        </div>
                                    </div>
                                </div>
                            </ItemTemplate>
                        </asp:Repeater>
                    </div>
                    <div class="form-buttons">
                        <asp:Button ID="btnCloseRecycleBin" runat="server" Text="Close" CssClass="btn" 
                            OnClick="btnCloseRecycleBin_Click" />
                    </div>
                </div>
                
                <!-- Permanent Delete Confirmation Popup -->
                <div id="permanentDeletePopup" class="delete-popup" runat="server">
                    <h2>Confirm Permanent Deletion</h2>
                    <p>Are you sure you want to permanently delete badge: [<asp:Literal ID="litPermanentDeleteBadgeName" runat="server"></asp:Literal>] ?</p>
                    <p><strong>Warning:</strong> This action cannot be undone!</p>
                    <div class="form-buttons">
                        <asp:Button ID="btnConfirmPermanentDelete" runat="server" Text="Delete Permanently" CssClass="btn delete-btn" 
                            OnClick="btnConfirmPermanentDelete_Click" />
                        <asp:Button ID="btnCancelPermanentDelete" runat="server" Text="Cancel" CssClass="btn" 
                            OnClick="btnCancelPermanentDelete_Click" />
                        <asp:HiddenField ID="hdnPermanentDeleteBadgeId" runat="server" />
                    </div>
                </div>

      
            </div>
        </div>
    </body>
</asp:Content>