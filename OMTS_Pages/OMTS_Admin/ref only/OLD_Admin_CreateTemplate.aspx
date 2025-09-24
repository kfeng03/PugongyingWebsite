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
        
        .popup {
            display: none;
            position: fixed;
            top: 50%;
            left: 50%;
            transform: translate(-50%, -50%);
            background-color: white;
            padding: 20px;
            border-radius: 5px;
            box-shadow: 0 2px 10px rgba(0, 0, 0, 0.3);
            z-index: 1000;
            width: 85%;
            max-width: 900px;
            max-height: 85vh;
            overflow-y: auto;
        }
        
        .popup-title {
            margin-top: 0;
            padding-bottom: 10px;
            border-bottom: 1px solid #eee;
            cursor: move;
        }
        
        .popup-footer {
            display: flex;
            justify-content: space-between;
            margin-top: 20px;
            padding-top: 15px;
            border-top: 1px solid #eee;
        }
        
        .popup-footer .btn-group-right {
            display: flex;
            gap: 10px;
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
</asp:Content>
<asp:Content ID="ContentMain" ContentPlaceHolderID="MainContent" runat="server">
    <!-- Main Content Layout -->
    <div class="container">
        <!-- Sidebar -->
            <!-- Sidebar -->
            <div class="sidebar">
                <asp:Button ID="btnAccMgmt" runat="server" CssClass='<%# (Request.Url.AbsolutePath.EndsWith("Admin_AccList.aspx")) ? "sidebar-btn active" : "sidebar-btn" %>' Text="<%$ Resources:Resources, SideBtnAM %>" PostBackUrl="~/Admin_AccList.aspx" />
                <asp:Button ID="btnCustom" runat="server" CssClass='<%# (Request.Url.AbsolutePath.EndsWith("Admin_Customization.aspx")) ? "sidebar-btn active" : "sidebar-btn" %>' Text="<%$ Resources:Resources, SideBtnWC %>" PostBackUrl="~/Admin_Customization.aspx" />
                <asp:Button ID="btnCertManager" runat="server" CssClass='<%# (Request.Url.AbsolutePath.EndsWith("Admin_CreateTemplate.aspx")) ? "sidebar-btn active" : "sidebar-btn" %>' Text="<%$ Resources:Resources, SideBtnTM %>" PostBackUrl="~/Admin_TemplateManager.aspx" />
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
                    <tr>
                        <td>
                            <label for="fuTemplate">Backgound Image:</label>
                        </td>
                        <td>
                            <asp:FileUpload ID="fuTemplate" runat="server" CssClass="form-control" accept=".jpg,.jpeg,.png,.gif" />
                        </td>
                    </tr>

               </table>
            </div>
            
            
            <div class="form-group">
                <asp:Button ID="btnUploadAnalyze" runat="server" CssClass="btn" Text="Upload & Analyze" OnClick="btnUploadAnalyze_Click" />
                <asp:Button ID="btnViewResult" runat="server" CssClass="btn" Text="View Analysis Result" OnClick="btnViewResult_Click" Visible="false" />
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
        <!-- Analysis Results Popup -->
        <div id="analysisPopup" class="popup" runat="server">
            <h2 class="popup-title">Step 1 of 2: Field Placement</h2>
    
            <div class="popup-content">
                <asp:Literal ID="litAnalysisResult" runat="server"></asp:Literal>
            </div>
    
            <div class="popup-footer">
                <div></div>
                <div class="btn-group-right">
                    <asp:Button ID="btnProceedToBackgroundStep" runat="server" CssClass="btn" Text="Next" OnClick="btnProceedToBackgroundStep_Click" />
                </div>
            </div>
        </div>
        
       <%-- <!-- Field Editor Popup -->
        <div id="fieldEditorPopup" class="popup" runat="server">
            <h2 id="fieldEditorTitle" class="popup-title">Edit Field</h2>
            
            <div class="popup-content">
                <div class="field-editor-form">
                    <!-- Hidden fields for state tracking -->
                    <asp:HiddenField ID="hdnFieldIndex" runat="server" Value="-1" />
                    <asp:HiddenField ID="hdnOriginalFieldName" runat="server" Value="" />
                    
                    <div class="full-width">
                        <label for="txtFieldName">Field Name:</label>
                        <asp:TextBox ID="txtFieldName" runat="server" CssClass="form-control"></asp:TextBox>
                    </div>
                    
                    <div>
                        <label for="ddlFieldType">Field Type:</label>
                        <asp:DropDownList ID="ddlFieldType" runat="server" CssClass="form-control">
                            <asp:ListItem Text="Text" Value="Text" />
                            <asp:ListItem Text="Date" Value="Date" />
                            <asp:ListItem Text="Number" Value="Number" />
                            <asp:ListItem Text="Signature" Value="Signature" />
                        </asp:DropDownList>
                    </div>
                    
                    <div>
                        <label for="chkRequired">Required:</label>
                        <asp:CheckBox ID="chkRequired" runat="server" Checked="true" />
                    </div>
                    
                    <div class="full-width">
                        <label>Position and Size:</label>
                        <div class="field-editor-coordinates">
                            <div>
                                <label for="txtX">X:</label>
                                <asp:TextBox ID="txtX" runat="server" CssClass="form-control" TextMode="Number"></asp:TextBox>
                            </div>
                            <div>
                                <label for="txtY">Y:</label>
                                <asp:TextBox ID="txtY" runat="server" CssClass="form-control" TextMode="Number"></asp:TextBox>
                            </div>
                            <div>
                                <label for="txtWidth">Width:</label>
                                <asp:TextBox ID="txtWidth" runat="server" CssClass="form-control" TextMode="Number"></asp:TextBox>
                            </div>
                            <div>
                                <label for="txtHeight">Height:</label>
                                <asp:TextBox ID="txtHeight" runat="server" CssClass="form-control" TextMode="Number"></asp:TextBox>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
            
            <div class="popup-footer">
                <button type="button" class="btn btn-danger" onclick="deleteField()">Delete Field</button>
                <div class="btn-group-right">
                    <asp:Button ID="btnUpdateField" runat="server" CssClass="btn" Text="Save Changes" OnClick="btnUpdateField_Click" />
                    <asp:Button ID="btnCancelFieldEdit" runat="server" CssClass="btn btn-secondary" Text="Cancel" OnClick="btnCancelFieldEdit_Click" />
                    <asp:Button ID="btnDeleteField" runat="server" CssClass="btn" Text="Delete" OnClick="btnDeleteField_Click" style="display: none;" />
                </div>
            </div>
        </div>--%>

        <!-- Background Change Popup -->
        <div id="backgroundChangePopup" class="popup" runat="server">
            <h2 class="popup-title">Step 2 of 2: Background Selection</h2>
    
            <div class="popup-content">
                <p>Do you want to change the background image for your certificate template?</p>
        
                <div class="form-group">
                    <label for="fuNewBackground">Select New Background Image:</label>
                    <asp:FileUpload ID="fuNewBackground" runat="server" CssClass="form-control" accept=".jpg,.jpeg,.png,.gif" />
                </div>
        
                <p class="instruction">Note: If you don't upload a new image, the current background will be used.</p>
            </div>
    
            <div class="popup-footer">
                <div>
                    <asp:Button ID="btnSkipBackgroundChange" runat="server" CssClass="btn btn-secondary" Text="Save Current Background" OnClick="btnSkipBackgroundChange_Click" />
                </div>
                <div class="btn-group-right">
                    <asp:Button ID="btnUploadBackground" runat="server" CssClass="btn" Text="Upload New Background" OnClick="btnUploadBackground_Click" />
                </div>
            </div>
        </div>
        <!-- Mini Field Editor -->
        <div id="miniFieldEditor" class="mini-field-editor">
                    <asp:HiddenField ID="hdnFieldIndex" runat="server" Value="-1" />
                    <asp:HiddenField ID="hdnOriginalFieldName" runat="server" Value="" />
            <div class="form-group" style="cursor: move; background-color: #f5f5f5; padding: 5px; margin: -10px -10px 10px -10px; border-bottom: 1px solid #ddd;">
                <strong id="miniEditorTitle">Edit Field</strong>
            </div>
            <div class="form-group">
                <label for="miniFieldName">Field Name:</label>
                <input type="text" id="miniFieldName" class="form-control" />
            </div>
            <div class="form-group">
                <label for="miniFieldType">Type:</label>
                <select id="miniFieldType" class="form-control">
                    <option value="Text">Text</option>
                    <option value="Date">Date</option>
                    <option value="Number">Number</option>
                    <option value="Signature">Signature</option>
                </select>
            </div>
            <div class="form-group">
                <div style="display:grid; grid-template-columns:1fr 1fr 1fr 1fr; gap:5px;">
                    <div>
                        <label for="miniFieldX">X:</label>
                        <input type="number" id="miniFieldX" class="form-control" />
                    </div>
                    <div>
                        <label for="miniFieldY">Y:</label>
                        <input type="number" id="miniFieldY" class="form-control" />
                    </div>
                    <div>
                        <label for="miniFieldWidth">W:</label>
                        <input type="number" id="miniFieldWidth" class="form-control" />
                    </div>
                    <div>
                        <label for="miniFieldHeight">H:</label>
                        <input type="number" id="miniFieldHeight" class="form-control" />
                    </div>
                </div>
            </div>
            <div class="btn-group">
                <button type="button" class="btn btn-sm btn-danger" onclick="miniDeleteField()">Delete</button>
                <button type="button" class="btn btn-sm btn-secondary" onclick="miniCloseEditor()">Close</button>
            </div>
            <input type="hidden" id="miniFieldIndex" value="-1" />
        </div>

        <!-- Hidden fields for data persistence -->
        <asp:HiddenField ID="hdnFieldsData" runat="server" Value="" />
        <asp:HiddenField ID="hdnDeletedFieldIndex" runat="server" Value="-1" />
        </div>
</asp:Content>