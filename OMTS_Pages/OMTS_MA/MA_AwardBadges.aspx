<%@ Page Title="" Language="C#" Async="true" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="MA_AwardBadges.aspx.cs" Inherits="fyp.MA_AwardBadges" %>
<asp:Content ID="Content1" ContentPlaceHolderID="title" runat="server">Award Badges</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="css" runat="server">
    <link rel="stylesheet" type="text/css" href="#" />
    <style>

        .select-all-container {
            margin-bottom: 10px;
        }
        .summary-panel {
            margin-top: 20px;
            padding: 15px;
            background-color: #f8f9fa;
            border-radius: 5px;
            border: 1px solid #ddd;
        }
        .summary-header {
            font-weight: bold;
            margin-bottom: 10px;
            font-size: 16px;
        }
        .success-list, .failed-list {
            margin-top: 10px;
            margin-bottom: 15px;
        }
        .success-item {
            color: #28a745;
            margin: 5px 0;
        }
        .failed-item {
            color: #dc3545;
            margin: 5px 0;
        }
        .badge-info {
            font-weight: bold;
            margin-bottom: 10px;
        }
        #badgenote{
           margin-right: 15px; 
           white-space: nowrap;
        }
    </style>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="script" runat="server">
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="MainContent" runat="server">
    <body>
    
        <!-- Main Content Layout -->
        <div class="container">
            <!-- Sidebar -->
            <div class="sidebar">
                <asp:Button ID="btnInformation" runat="server" CssClass='sidebar-btn' Text="<%$ Resources:Resources, SideBtnInfo %>" PostBackUrl="~/OMTS_Pages/OMTS_MA/MA_Information.aspx" />
                <asp:Button ID="btnDashboard" runat="server" CssClass='sidebar-btn' Text="<%$ Resources:Resources, SideBtnDb %>" PostBackUrl="~/OMTS_Pages/OMTS_MA/MA_AchievementDashboard.aspx" />
                <asp:Button ID="btnApplicationForm" runat="server" CssClass='sidebar-btn' Text="<%$ Resources:Resources, SideBtnMAApp %>" PostBackUrl="~/OMTS_Pages/OMTS_MA/MA_AchievementApplication.aspx" />
                <asp:Button ID="btnReviewApp" runat="server" CssClass='sidebar-btn' Text="<%$ Resources:Resources, SideBtnRA %>" PostBackUrl="~/OMTS_Pages/OMTS_MA/MA_ReviewApp.aspx" />
                <asp:Button ID="btnAwardBadge" runat="server" CssClass='sidebar-btn active' Text="<%$ Resources:Resources, SideBtnAB%>" PostBackUrl="~/OMTS_Pages/OMTS_MA/MA_AwardBadges.aspx" />
                <asp:Button ID="btnMaterials" runat="server" CssClass='sidebar-btn' Text="<%$ Resources:Resources, SideBtnMAMat%>" PostBackUrl="~/OMTS_Pages/OMTS_MA/MA_Materials.aspx" />
            </div>
            <!-- Main Content Area -->
            <div class="main-content">
                <div>
                     <h2 class="page-header">
                         <asp:Literal ID="litAwardBadges" runat="server" Text="<%$ Resources:Resources, Heading_AwardBadges%>" />
                     </h2>
                     
                     <!-- Select All Checkbox with AutoPostBack -->
                     <div class="select-all-container">
                         <asp:CheckBox ID="selectAllCheckbox" runat="server" Text="Select All" AutoPostBack="true" OnCheckedChanged="selectAllCheckbox_CheckedChanged" />
                     </div>
                     
                    <asp:GridView ID="listTable" runat="server" AutoGenerateColumns="False" CssClass="list-table" style="border:hidden;" GridLines="None" OnRowDataBound="listTable_RowDataBound">
                        <Columns>
                            <asp:TemplateField>
                                <ItemTemplate>
                                    <asp:CheckBox ID="chkSelect" runat="server" CssClass="user-checkbox" AutoPostBack="true" OnCheckedChanged="chkSelect_CheckedChanged" />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="userId" HeaderText="User ID" />
                            <asp:BoundField DataField="username" HeaderText="Username" />
                            <asp:BoundField DataField="phoneNumber" HeaderText="Phone Number" />
                            <asp:BoundField DataField="email" HeaderText="Email Address" />
                            <asp:BoundField DataField="level" HeaderText="Level" />
                        </Columns>
                    </asp:GridView>
                    <div class="tab">
                        <div class="tab-content" style="display: flex; align-items: center; flex-wrap: nowrap; width: 100%;">
                            <asp:Label ID="lblAwardBadgeNote" runat="server" CssClass="badgenote"></asp:Label>
                            <asp:DropDownList ID="ddlBadges" runat="server" CssClass="ddl" style="margin-right: 10px; width: 30%;"></asp:DropDownList>
                            <asp:Button ID="btnConfirm" runat="server" Text="<%$ Resources:Resources, Button_Confirm %>" CssClass="btn" OnClick="btnConfirm_Click" style="width: 15%;"></asp:Button>
                        </div>
                    </div>
                    <!-- Summary Report Panel -->
                    <asp:Panel ID="pnlSummary" runat="server" CssClass="summary-panel" Visible="false">
                        <div class="summary-header">
                            <asp:Literal ID="litSummaryHeader" runat="server" Text="Badge Award Summary"></asp:Literal>
                        </div>
                        <div class="badge-info">
                            <asp:Literal ID="litBadgeInfo" runat="server"></asp:Literal>
                        </div>
                        <div class="success-section">
                            <strong>Successfully Awarded:</strong>
                            <asp:BulletedList ID="lstSuccessful" runat="server" CssClass="success-list" BulletStyle="Circle"></asp:BulletedList>
                        </div>
                        <div class="failed-section">
                            <strong>Not Awarded:</strong>
                            <asp:BulletedList ID="lstFailed" runat="server" CssClass="failed-list" BulletStyle="Circle"></asp:BulletedList>
                        </div>
                        <asp:Button ID="btnDone" runat="server" Text="Close" CssClass="btn" OnClick="btnDone_Click" />
                    </asp:Panel>
                </div>
            </div>
        </div>
    </body>
</asp:Content>