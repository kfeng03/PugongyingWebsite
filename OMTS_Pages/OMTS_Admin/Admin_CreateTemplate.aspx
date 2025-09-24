<%@ Page Language="C#" AutoEventWireup="true" Async="true" CodeBehind="Admin_CreateTemplate.aspx.cs" MaintainScrollPositionOnPostBack="true"  MasterPageFile="~/Site1.Master" Inherits="fyp.CertificateAnalyzer" %>

<asp:Content ID="ContentTitle" ContentPlaceHolderID="title" runat="server">Certificate Template Analyzer</asp:Content>
<asp:Content ID="ContentStyle" ContentPlaceHolderID="css" runat="server">
    <link rel="stylesheet" href="https://code.jquery.com/ui/1.13.2/themes/base/jquery-ui.css" />
    <style>
        
       

        .message {
            padding: 10px;
            margin-bottom: 15px;
            border-radius: 4px;
        }
        
        .message-success {
            background-color: #d4edda;
            color: #155724;
            border: 1px solid #c3e6cb;
        }
        
        .message-error {
            background-color: #f8d7da;
            color: #721c24;
            border: 1px solid #f5c6cb;
        }
        
        .message-warning {
            background-color: #fff3cd;
            color: #856404;
            border: 1px solid #ffeeba;
        }
        
        .message-info {
            background-color: #d1ecf1;
            color: #0c5460;
            border: 1px solid #bee5eb;
        }
        
        /* Popup styles */
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
        
        /* Make popup full-screen */
        .popup {
          display:none;
          position:fixed;
          inset:0;                 /* top:0; right:0; bottom:0; left:0 */
          background:rgba(0,0,0,.5);
          z-index:1000;
        }

        .popup-content{
          position:absolute;
          inset:2vw;               /* margin to create a “frame” */
          background:#fff;
          border-radius:10px;
          box-shadow:0 10px 30px rgba(0,0,0,.25);
          overflow:hidden;         /* no iframe scrollbars visible */
        }

        /* Fill the content with the iframe */
        #popupFrame{
          width:100%;
          height:100%;
          border:0;
          display:block;
        }

        
        /* Field visualization styles */
        .visualization-container {
            margin: 20px auto;
            background-color: #fff;
            box-shadow: 0 1px 3px rgba(0, 0, 0, 0.12);
        }
        
        .template-image {
            width: 100%;
            height: auto;
            display: block;
        }
        
        .field-marker {
            position: absolute;
            border: 2px solid;
            opacity: 0.6;
            box-sizing: border-box;
            cursor: move;
            transition: opacity 0.2s ease;
        }
        
        .field-marker:hover {
            opacity: 0.8 !important;
        }
        
        .text-field {
            border-color: #00A2E8 !important;
            background-color: rgba(0, 162, 232, 0.3) !important;
        }
        
        .date-field {
            border-color: #FF7F27 !important;
            background-color: rgba(255, 127, 39, 0.3) !important;
        }
        
        .number-field {
            border-color: #73BF44 !important;
            background-color: rgba(115, 191, 68, 0.3) !important;
        }
        
        .signature-field {
            border-color: #800080 !important;
            background-color: rgba(128, 0, 128, 0.3) !important;
        }
        
        .field-label {
            position: absolute;
            top: -20px;
            left: 0;
            background: white;
            border: 1px solid #888;
            padding: 2px 4px;
            font-size: 12px;
            white-space: nowrap;
            overflow: hidden;
            text-overflow: ellipsis;
            max-width: 150px;
            z-index: 10;
        }
        
        .resize-handle {
            position: absolute;
            bottom: 0;
            right: 0;
            width: 10px;
            height: 10px;
            background-color: white;
            border: 1px solid #666;
            cursor: se-resize;
        }
        
        /* New styles for A4 standardization */

        
        .field-editor-form {
            display: grid;
            grid-template-columns: 1fr 1fr;
            gap: 15px;
            margin-top: 15px;
        }
        
        .field-editor-form label {
            font-weight: bold;
            margin-bottom: 5px;
            display: block;
        }
        
        .field-editor-form .full-width {
            grid-column: 1 / span 2;
        }
        
        .field-editor-coordinates {
            display: grid;
            grid-template-columns: 1fr 1fr 1fr 1fr;
            gap: 10px;
        }
        
        .instruction {
            color: #666;
            font-style: italic;
            margin-bottom: 10px;
        }
        
        .controls {
            margin-bottom: 15px;
            display: flex;
            gap: 10px;
        }
        /* Add these styles to your existing style block */
        .mini-field-editor {
            position: absolute;
            background-color: white;
            border: 1px solid #ccc;
            border-radius: 5px;
            padding: 10px;
            z-index: 1010;
            box-shadow: 0 2px 10px rgba(0, 0, 0, 0.2);
            width: 30%;
            display: none;
        }

        .mini-field-editor .form-group {
            margin-bottom: 8px;
        }

        .mini-field-editor label {
            font-weight: bold;
            display: block;
            margin-bottom: 3px;
            font-size: 12px;
        }

        .mini-field-editor input,
        .mini-field-editor select {
            width: 100%;
            padding: 5px;
            font-size: 12px;
        }

        .mini-field-editor .btn-group {
            display: flex;
            justify-content: space-between;
            gap: 5px;
            margin-top: 10px;
        }

        .mini-field-editor .btn-sm {
            padding: 2px 8px;
            font-size: 11px;
        }

    </style>
</asp:Content>
<asp:Content ID="ContentScript" ContentPlaceHolderID="script" runat="server">
     <script>
         function showPopup(url) {
             var name = document.getElementById('<%= txtTemplateName.ClientID %>').value.trim();
             var orientation = document.querySelector('#<%= rblOrientation.ClientID %> input:checked');
             var type = document.querySelector('#<%= rblTemplateType.ClientID %> input:checked');
             if (!name) {
                 alert('Please enter a template name.');
                 return false;
             }
             if (!orientation) {
                 alert('Please select a template orientation.');
                 return false;
             }
             if (!type) {
                 alert('Please select a template type.');
                 return false;
             }
             if (url) {
                 document.getElementById("popupFrame").src = url;
             }
             document.getElementById("popupModal").style.display = "block";
             return true;
         }
         function hidePopup() {
             document.getElementById("popupModal").style.display = "none";
         }
         window.getTemplateMeta = function () {
             return {
                 templateName: document.getElementById('<%= txtTemplateName.ClientID %>').value,
                 orientation: document.querySelector('#<%= rblOrientation.ClientID %> input:checked').value,
                 templateType: document.querySelector('#<%= rblTemplateType.ClientID %> input:checked').value
             };
         };
     </script>
</asp:Content>
<asp:Content ID="ContentMain" ContentPlaceHolderID="MainContent" runat="server">

    <!-- Main Content Layout -->
    <div class="container">
        <!-- Sidebar -->
            <!-- Sidebar -->
            <div class="sidebar">
                <asp:Button ID="btnAccMgmt" runat="server" CssClass='<%# (Request.Url.AbsolutePath.EndsWith("Admin_AccList.aspx")) ? "sidebar-btn active" : "sidebar-btn" %>' Text="<%$ Resources:Resources, SideBtnAM %>" PostBackUrl="~/OMTS_Pages/OMTS_Admin/Admin_AccList.aspx" />
                <asp:Button ID="btnCustom" runat="server" CssClass='<%# (Request.Url.AbsolutePath.EndsWith("Admin_Customization.aspx")) ? "sidebar-btn active" : "sidebar-btn" %>' Text="<%$ Resources:Resources, SideBtnWC %>" PostBackUrl="~/OMTS_Pages/OMTS_Admin/Admin_Customization.aspx" />
                <asp:Button ID="btnCertManager" runat="server" CssClass='<%# (Request.Url.AbsolutePath.EndsWith("Admin_CreateTemplate.aspx")) ? "sidebar-btn active" : "sidebar-btn" %>' Text="<%$ Resources:Resources, SideBtnTM %>" PostBackUrl="~/OMTS_Pages/OMTS_Admin/Admin_TemplateManager.aspx" />
            </div>

     <!-- Main Content Area -->
     <div class="main-content">
            <h2><asp:Literal ID="litHeading1" runat="server" Text="Certificate Template Analyzer" /></h2>
            
            <asp:Panel ID="messagePanel" runat="server" CssClass="message" Visible="false">
                <asp:Label ID="lblMessage" runat="server" Text=""></asp:Label>
            </asp:Panel>
            
            <div class="a4-info">
                <p>Your certificate template will be automatically resized to fit A4 dimensions, ensuring consistent field positioning. <br /> Choose the orientation that best matches your template:</p>
                
                    <table class="form-table">
                    <tr>
                        <td><label for="txtTemplateName">Template Name:</label></td>
                        <td>
                            <asp:TextBox ID="txtTemplateName" runat="server" CssClass="text-input"></asp:TextBox>
                        </td>
                    </tr>
                    <tr>
                        <td><label for="ddlOrientation">Template Orientation:</label></td>
                        <td>
                            <asp:RadioButtonList ID="rblOrientation" runat="server" CssClass="radio-list" RepeatDirection="Horizontal" Width="443px">
                            <asp:ListItem Text="Portrait (Vertical)" Value="portrait" Selected="True" />
                            <asp:ListItem Text="Landscape (Horizontal)" Value="landscape" />
                            </asp:RadioButtonList>
                        </td>
                    </tr>
                    <tr>
                        <td><label for="ddl">Template for:</label></td>
                        <td>
                            <asp:RadioButtonList ID="rblTemplateType" runat="server" CssClass="radio-list" RepeatDirection="Horizontal">
                            <asp:ListItem Text="Testimonial Template" Value="Testimonial" Selected="True" />
                            <asp:ListItem Text="Event Certificate Template" Value="Event" />
                            </asp:RadioButtonList>
                        </td>
                    </tr>
                    
               </table>
            </div>
            
            
            <div class="form-group">
                <%--<asp:Button ID="btnUploadAnalyze" runat="server" CssClass="btn" Text="Upload & Analyze" OnClick="btnUploadAnalyze_Click" />
                <asp:Button ID="btnViewResult" runat="server" CssClass="btn" Text="View Analysis Result" OnClick="btnViewResult_Click" Visible="false" />--%>
                <asp:Button ID="btnOpenPopup" runat="server" CssClass="btn" Text="Open Popup" OnClientClick="showPopup(); return false;" OnClick="btnOpenPopup_Click" />
            </div>
            
            <div class="form-group">
                <h3>Instructions:</h3>
                <ol>
                    <li>Select the orientation that best matches your certificate template (portrait or landscape).</li>
                    <li>Upload a certificate or testimonial template image (JPG, PNG, or GIF).</li>
                    <li>Click "Upload & Analyze" to process the image with AI.</li>
                    <li>The system will identify areas where text should be placed (fields).</li>
                    <li>View the results to see detected fields highlighted on the template.</li>
                    <li>You can edit fields by dragging or resizing them directly on the template.</li>
                    <li>Double-click any field to edit its properties, or click '+ Add Field' to create a new field.</li>
                    <li>If your template contains placeholders, you can upload a clean version without placeholders.</li>
                    <li>Save the template when you're satisfied with the field configuration.</li>
                </ol>
                <p><strong>Tip:</strong> For best results, use templates with clear placeholder text or visible field spaces.</p>
            </div>
        </div>
        
        <div id="overlay" class="overlay" runat="server"></div>
        
        <!-- Popup Modal -->
        <div id="popupModal" class="popup" style="display:none;">
          <div class="popup-content">
            <iframe id="popupFrame" src="Admin_TemplateEditorInterface.aspx" frameborder="0"></iframe>
            <asp:Button ID="btnClosePopup" runat="server" Cssclass="btn btn-danger" OnClientClick="hidePopup()" style="background-color:darkred;position:absolute;top:10px; right:10px;z-index:2;" Text="Close"></asp:Button>
          </div>
        </div>


       


        <!-- Hidden fields for data persistence -->
        <asp:HiddenField ID="hdnFieldsData" runat="server" Value="" />
        <asp:HiddenField ID="hdnDeletedFieldIndex" runat="server" Value="-1" />
        </div>
</asp:Content>