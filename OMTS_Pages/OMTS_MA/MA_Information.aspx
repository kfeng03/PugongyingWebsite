<%@ Page Title="" Language="C#" Async="true" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="MA_Information.aspx.cs" Inherits="fyp.MA_Information" %>

<asp:Content ID="ContentTitle" ContentPlaceHolderID="title" runat="server">Member Achievement Information</asp:Content>
<asp:Content ID="ContentStyle" ContentPlaceHolderID="css" runat="server">
    <link rel="stylesheet" type="text/css" href="#" />
</asp:Content>
<asp:Content ID="ContentScript" ContentPlaceHolderID="script" runat="server">
</script>  
</asp:Content>
<asp:Content ID="ContentMain" ContentPlaceHolderID="MainContent" runat="server">
    <body>
    
        
        <!-- Main Content Layout -->
        <div class="container">
            <!-- Sidebar -->
            <div class="sidebar">
                <asp:Button ID="btnInformation" runat="server" CssClass='sidebar-btn active' Text="<%$ Resources:Resources, SideBtnInfo %>" PostBackUrl="~/OMTS_Pages/OMTS_MA/MA_Information.aspx" />
                <asp:Button ID="btnDashboard" runat="server" CssClass='sidebar-btn' Text="<%$ Resources:Resources, SideBtnDb %>" PostBackUrl="~/OMTS_Pages/OMTS_MA/MA_AchievementDashboard.aspx" />
                <asp:Button ID="btnApplicationForm" runat="server" CssClass='sidebar-btn' Text="<%$ Resources:Resources, SideBtnMAApp %>" PostBackUrl="~/OMTS_Pages/OMTS_MA/MA_AchievementApplication.aspx" />
                <asp:Button ID="btnReviewApp" runat="server" CssClass='sidebar-btn' Text="<%$ Resources:Resources, SideBtnRA %>" PostBackUrl="~/OMTS_Pages/OMTS_MA/MA_ReviewApp.aspx" />
                <asp:Button ID="btnAwardBadge" runat="server" CssClass='sidebar-btn' Text="<%$ Resources:Resources, SideBtnAB%>" PostBackUrl="~/OMTS_Pages/OMTS_MA/MA_AwardBadges.aspx" />
                <asp:Button ID="btnMaterials" runat="server" CssClass='sidebar-btn' Text="<%$ Resources:Resources, SideBtnMAMat%>" PostBackUrl="~/OMTS_Pages/OMTS_MA/MA_Materials.aspx" />

            </div>

            <!-- Main Content Area -->
            <div class="main-content">
                <div>
                    <h2 class="page-header">
                        <asp:Literal ID="litAchievementInfo" runat="server" Text="<%$ Resources:Resources, Heading_MemberAchievement %>" />
                    </h2>

                    <!-- Media Title -->
                    <h3 class="media-title">
                        <asp:Literal ID="litMediaTitle" runat="server" />
                    </h3>

                    <!-- No Media Panel -->
                    <asp:Panel ID="pnlNoMedia" runat="server" Visible="false" CssClass="no-media-panel">
                        <div class="alert alert-info">
                            No introduction media has been uploaded yet. Visit the Materials page to add introduction media.
                        </div>
                    </asp:Panel>

                    <!-- Video Container -->
                    <div id="divVideoContainer" runat="server" class="media-container">
                        <!-- YouTube Embed -->
                        <asp:Literal ID="litYouTubeEmbed" runat="server" />
    
                        <!-- Video Link (for Google Drive or others) -->
                        <asp:HyperLink ID="hlVideoLink" runat="server" Text="Watch Video" CssClass="btn btn-primary" Target="_blank" Visible="false" />
                    </div>

                    <!-- PDF Container -->
                    <div id="divPdfContainer" runat="server" class="media-container" visible="false">
                        <!-- PDF Embed (for Google Drive) -->
                        <asp:Literal ID="litPdfEmbed" runat="server" />
                    </div>

                    <!-- Image Container -->
                    <div id="divImageContainer" runat="server" class="media-container" visible="false">
                        <asp:Image ID="imgInfographic" runat="server" CssClass="info-image" />
                    </div>
                    
                    <h2 class="page-header">
                        <asp:Literal ID="litBadgesGallery" runat="server" Text="<%$ Resources:Resources, Heading_BadgesGallery %>" />
                    </h2>
                    
                    <!-- Repeater for Badges -->
                 <div class="badges-gallery">
                    <asp:Repeater ID="rptBadges" runat="server">
                        <ItemTemplate>
                            <div class="badge-item">
                                <asp:Image ID="imgBadge" runat="server" 
                                    ImageUrl='<%# Eval("BadgeIconUrl") %>'
                                    AlternateText='<%# Eval("BadgeName") %>' 
                                    CssClass="badge-detail-icon"/>
                                <div class="badge-details"  style="width:70%; margin-left:10%;">
                                    <h4><%# Eval("BadgeName") %></h4>
                                    <p><%# Eval("BadgeDesc") %></p>
                                    <p style="font-size:11px;color:gray;">Points Awarded: <%# Eval("BadgePoints") %></p>
                                </div>
                            </div>
                           
                        </ItemTemplate>
                    </asp:Repeater>
                 </div>                    
                </div>
            </div>
        </div>
</body>
</asp:Content>
