<%@ Page Language="C#" Async="true" AutoEventWireup="true" CodeBehind="MA_ReviewApp.aspx.cs" Inherits="fyp.MA_ReviewApp" MasterPageFile="~/Site1.Master" %>
<asp:Content ID="ContentTitle" ContentPlaceHolderID="title" runat="server">Application List</asp:Content>
<asp:Content ID="ContentStyle" ContentPlaceHolderID="css" runat="server">
    <link rel="stylesheet" type="text/css" href="#" />
    <style>
        .status-pending {
            color: #ffc107;
            font-weight: bold;
        }
        .status-approved {
            color: #28a745;
            font-weight: bold;
        }
        .status-rejected {
            color: #dc3545;
            font-weight: bold;
        }
        .truncate-text {
            max-width: 200px;
            white-space: nowrap;
            overflow: hidden;
            text-overflow: ellipsis;
            display: inline-block;
        }
    </style>
</asp:Content>
<asp:Content ID="ContentScript" ContentPlaceHolderID="script" runat="server">
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
                <asp:Button ID="btnReviewApp" runat="server" CssClass='sidebar-btn active' Text="<%$ Resources:Resources, SideBtnRA %>" PostBackUrl="~/OMTS_Pages/OMTS_MA/MA_ReviewApp.aspx" />
                <asp:Button ID="btnAwardBadge" runat="server" CssClass='sidebar-btn' Text="<%$ Resources:Resources, SideBtnAB%>" PostBackUrl="~/OMTS_Pages/OMTS_MA/MA_AwardBadges.aspx" />
                <asp:Button ID="btnMaterials" runat="server" CssClass='sidebar-btn' Text="<%$ Resources:Resources, SideBtnMAMat%>" PostBackUrl="~/OMTS_Pages/OMTS_MA/MA_Materials.aspx" />
            </div>
            <!-- Main Content Area -->
            <div class="main-content">
                <div>
                    <h2 class="page-header">
                        <asp:Literal ID="litRecentApplication" runat="server" Text="<%$ Resources:Resources, Heading_ListOfRecentApplications %>" />
                    </h2>
                    
                    <!-- Filter Options -->
                    <div class="filter-section">
                        <asp:DropDownList ID="ddlStatusFilter" CssClass="ddl" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlStatusFilter_SelectedIndexChanged">
                            <asp:ListItem Text="All Applications" Value="All" Selected="True"></asp:ListItem>
                            <asp:ListItem Text="Pending" Value="Pending"></asp:ListItem>
                            <asp:ListItem Text="Approved" Value="Approved"></asp:ListItem>
                            <asp:ListItem Text="Rejected" Value="Rejected"></asp:ListItem>
                        </asp:DropDownList>
                    </div>
                    
                    <!-- Applications GridView -->
                    <asp:GridView ID="gvApplications" runat="server" AutoGenerateColumns="False" 
                        CssClass="list-table" OnRowCommand="gvApplications_RowCommand" OnRowDataBound="gvApplications_RowDataBound"
                        EmptyDataText="No applications found." Width="100%" GridLines="None">
                        <Columns>
                            <asp:BoundField DataField="ApplicationId" HeaderText="ID" SortExpression="ApplicationId" />
                            <asp:BoundField DataField="ApplicantName" HeaderText="Applicant" SortExpression="ApplicantName" />
                            <asp:BoundField DataField="EventName" HeaderText="Event" SortExpression="EventName" />
                            <asp:BoundField DataField="ParticipatedRole" HeaderText="Role" SortExpression="ParticipatedRole" />
                            <asp:TemplateField HeaderText="Learning Outcome">
                                <ItemTemplate>
                                    <span class="truncate-text" title='<%# Eval("LearningOutcome") %>'>
                                        <%# Eval("LearningOutcome") %>
                                    </span>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Attachment">
                                <ItemTemplate>
                                    <asp:HyperLink ID="lnkAttachment" runat="server" Text="View Document" 
                                        NavigateUrl='<%# Eval("SupportingDocUrl") %>' Target="_blank"
                                        Visible='<%# Eval("SupportingDocUrl") != null && !string.IsNullOrEmpty(Eval("SupportingDocUrl").ToString()) %>' />
                                    <asp:Label ID="lblNoAttachment" runat="server" Text="No attachment" 
                                        Visible='<%# Eval("SupportingDocUrl") == null || string.IsNullOrEmpty(Eval("SupportingDocUrl").ToString()) %>' />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Status">
                                <ItemTemplate>
                                    <asp:Label ID="lblStatus" runat="server" 
                                        Text='<%# Eval("Status") != null ? Eval("Status") : "Pending" %>' 
                                        CssClass='<%# GetStatusCssClass(Eval("Status") != null ? Eval("Status").ToString() : "Pending") %>' />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Action">
                                <ItemTemplate>
                                    <asp:LinkButton ID="lnkReview" runat="server" Text="Review" CssClass="action-link"
                                        CommandName="ReviewApplication" CommandArgument='<%# Eval("ApplicationId") %>'
                                        Visible='<%# Eval("Status") != null && Eval("Status").ToString() == "Pending" %>' />
                                    <asp:LinkButton ID="lnkViewDetails" runat="server" Text="View Details" CssClass="action-link"
                                        CommandName="ViewDetails" CommandArgument='<%# Eval("ApplicationId") %>'
                                        Visible='<%# Eval("Status") == null || Eval("Status").ToString() != "Pending" %>' />
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                </div>
            </div>
        </div>
    </body>
</asp:Content>