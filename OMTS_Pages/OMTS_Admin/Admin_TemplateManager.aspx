<%@ Page Language="C#" AutoEventWireup="true" Async="true" CodeBehind="Admin_TemplateManager.aspx.cs" MasterPageFile="~/Site1.Master" Inherits="fyp.Admin_TemplateManager" %>

<asp:Content ID="ContentTitle" ContentPlaceHolderID="title" runat="server">Template Manager</asp:Content>
<asp:Content ID="ContentStyle" ContentPlaceHolderID="css" runat="server">
    <style>
        .template-list {
            display: grid;
            grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
            gap: 20px;
            margin-top: 20px;
        }
        
        .template-card {
            border: 1px solid #ddd;
            border-radius: 8px;
            overflow: hidden;
            box-shadow: 0 2px 5px rgba(0,0,0,0.1);
            transition: transform 0.2s;
        }
        
        .template-card:hover {
            transform: translateY(-5px);
            box-shadow: 0 5px 15px rgba(0,0,0,0.1);
        }
        
        .template-image {
            width: 100%;
            height: 200px;
            object-fit: contain;
            border-bottom: 1px solid #eee;
        }
        
        .template-info {
            padding: 15px;
        }
        
        .template-title {
            font-size: 16px;
            font-weight: bold;
            margin-bottom: 5px;
            overflow: hidden;
            text-overflow: ellipsis;
            white-space: nowrap;
        }
        
        .template-meta {
            font-size: 12px;
            color: #666;
            margin-bottom: 10px;
        }
        
        .template-actions {
            display: flex;
            justify-content: space-between;
            padding: 10px 15px;
            border-top: 1px solid #eee;
        }
        
        .current-template-indicator {
            position: absolute;
            top: 10px;
            right: 10px;
            background-color: #4CAF50;
            color: white;
            padding: 5px 10px;
            border-radius: 4px;
            font-size: 12px;
            font-weight: bold;
        }
        
        .filter-section {
            margin-bottom: 20px;
        }
        
        .filter-field {
            width: 200px;
            display: inline-block;
            margin-right: 10px;
        }
        
        .status-message {
            padding: 15px;
            margin-bottom: 20px;
            border-radius: 5px;
        }
        
        .alert-success {
            background-color: #dff0d8;
            border: 1px solid #d6e9c6;
            color: #3c763d;
        }
        
        .alert-danger {
            background-color: #f2dede;
            border: 1px solid #ebccd1;
            color: #a94442;
        }
    </style>
</asp:Content>

<asp:Content ID="ContentScript" ContentPlaceHolderID="script" runat="server">
    <script>
        function confirmDelete(templateName) {
            return confirm("Are you sure you want to delete the template '" + templateName + "'? This action cannot be undone.");
        }
    </script>
</asp:Content>

<asp:Content ID="ContentMain" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container">
        <!-- Sidebar -->
        <div class="sidebar">
    <asp:Button ID="btnAccMgmt" runat="server" CssClass='<%# (Request.Url.AbsolutePath.EndsWith("Admin_AccList.aspx")) ? "sidebar-btn active" : "sidebar-btn" %>' Text="<%$ Resources:Resources, SideBtnAM %>" PostBackUrl="~/OMTS_Pages/OMTS_Admin/Admin_AccList.aspx" />
    <asp:Button ID="btnCustom" runat="server" CssClass='<%# (Request.Url.AbsolutePath.EndsWith("Admin_Customization.aspx")) ? "sidebar-btn active" : "sidebar-btn" %>' Text="<%$ Resources:Resources, SideBtnWC %>" PostBackUrl="~/OMTS_Pages/OMTS_Admin/Admin_Customization.aspx" />
    <asp:Button ID="btnCertManager" runat="server" CssClass='<%# (Request.Url.AbsolutePath.EndsWith("Admin_TemplateManager.aspx")) ? "sidebar-btn active" : "sidebar-btn" %>' Text="<%$ Resources:Resources, SideBtnTM %>" PostBackUrl="~/OMTS_Pages/OMTS_Admin/Admin_TemplateManager.aspx" />
</div>

        <!-- Main Content Area -->
        <div class="main-content">
            <h2>
                <asp:Literal ID="litHeading" runat="server" Text="<%$ Resources:Resources, SideBtnTM %>" />
            </h2>
            
            <!-- Status Message Panel -->
            <asp:Panel ID="pnlMessage" runat="server" CssClass="status-message" Visible="false">
                <asp:Label ID="lblMessage" runat="server" Text=""></asp:Label>
            </asp:Panel>
            
            <!-- Filters Section -->
            <div class="filter-section">
                <asp:DropDownList ID="ddlTemplateType" runat="server" CssClass="form-control filter-field" AutoPostBack="true" OnSelectedIndexChanged="ddlTemplateType_SelectedIndexChanged">
                    <asp:ListItem Text="All Templates" Value="All" />
                    <asp:ListItem Text="Testimonial Templates" Value="Testimonial" />
                    <asp:ListItem Text="Event Templates" Value="Event" />
                </asp:DropDownList>
                
                <asp:Button ID="btnAddNewTemplate" runat="server" Text="Add New Template" CssClass="btn" PostBackUrl="~/OMTS_Pages/OMTS_Admin/Admin_CreateTemplate.aspx" />
            </div>
            
            <!-- Templates List -->
            <div class="template-list">
                <asp:Repeater ID="rptTemplates" runat="server" OnItemCommand="rptTemplates_ItemCommand">
                    <ItemTemplate>
                        <div class="template-card" style="position: relative;">
                            <!-- Current Template Indicator -->
                            <asp:Panel ID="pnlCurrentTemplate" runat="server" CssClass="current-template-indicator" Visible='<%# Convert.ToBoolean(Eval("IsTestimonialTemplate")) %>'>
                                Default
                            </asp:Panel>
                            
                            <!-- Template Image -->
                            <img src='<%# Eval("TemplateUrl") %>' alt='<%# Eval("TemplateName") %>' class="template-image" />
                            
                            <!-- Template Information -->
                            <div class="template-info">
                                <div class="template-title"><%# Eval("TemplateName") %></div>
                                <div class="template-meta">
                                    <div>Type: <%# Eval("TemplateType") %></div>
                                    <div>Created: <%# ((DateTime)Eval("UploadedAt")).ToString("MMM d, yyyy") %></div>
                                    <div>Fields: <%# ((Dictionary<string, fyp.FieldData>)Eval("Fields")).Count %></div>
                                </div>
                            </div>
                            
                            <!-- Template Actions -->
                            <div class="template-actions">
                                <asp:Button ID="lnkSelectTemplate" runat="server" 
                                    Visible='<%# !Convert.ToBoolean(Eval("IsTestimonialTemplate")) && Eval("TemplateType").ToString() == "Testimonial" %>'
                                    CommandName="SelectTestimonial" CommandArgument='<%# Eval("TemplateId") %>'
                                    Text="Set as Default" CssClass="btn" />
                                
                                <asp:Button ID="lnkEditTemplate" runat="server" 
                                    CommandName="EditTemplate" CommandArgument='<%# Eval("TemplateId") %>'
                                    Text="Edit" CssClass="btn" />
                                
                                <asp:Button ID="lnkDeleteTemplate" runat="server" 
                                    CommandName="DeleteTemplate" CommandArgument='<%# Eval("TemplateId") %>'
                                    OnClientClick='<%# "return confirmDelete(\"" + Eval("TemplateName") + "\");" %>'
                                    Text="Delete" CssClass="btn btn-danger" />
                            </div>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
            </div>
            
            <!-- No Templates Message -->
            <asp:Panel ID="pnlNoTemplates" runat="server" Visible="false">
                <div class="no-data-message">
                    <p><asp:Literal ID="litTxt" runat="server" Text="<%$ Resources:Resources, Label_NoTemplate %>" /></p>
                </div>
            </asp:Panel>
        </div>
    </div>
</asp:Content>