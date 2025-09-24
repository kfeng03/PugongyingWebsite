<%@ Page Title="" Language="C#" Async="true" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="MA_AchievementDashboard.aspx.cs" Inherits="fyp.MA_AchievementDashboard" %>
<asp:Content ID="ContentTitle" ContentPlaceHolderID="title" runat="server">My Dashboard</asp:Content>
<asp:Content ID="ContentStyle" ContentPlaceHolderID="css" runat="server">
     <link rel="stylesheet" type="text/css" href="#" />
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/4.7.0/css/font-awesome.min.css">
    <style>

        .dashboard-content {
            display: flex;
            gap: 20px;
        }
        .profile-card {
            width: 30%;
            background: white;
            padding: 15px;
            text-align: center;
            border-radius: 10px;
            box-shadow: 2px 2px 5px rgba(0, 0, 0, 0.1);
        }
        .profile-avatar {
            width: 200px;
            height: 200px;
            border-radius: 50%;
            object-fit: cover;
        }
        .stats {
            display: flex;
            justify-content: center;
            gap: 15px;
        }
        .history-link {
            color: blue;
            text-decoration: underline;
            cursor: pointer;
        }
        .achievement-panel {
            width: 65%;
            background: white;
            padding: 15px;
            border-radius: 10px;
            box-shadow: 2px 2px 5px rgba(0, 0, 0, 0.1);
        }
        .level-section {
            margin-bottom: 15px;
        }
        .level-bar {
            display: flex;
            align-items: center;
            gap: 10px; /* Add spacing between elements */
            position: relative; /* Required for absolute positioning of the text */
        }

        .badge-in-popup{
            width:100px;
            height:100px;
            display: block;
            margin: 0 auto;
            margin: 0 0 0 -20%;
            -webkit-border-radius: 50%;
            -moz-border-radius: 50%;
            border-radius: 50%;  
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
        .badges-grid {
            display: grid;
            grid-template-columns: repeat(8, 1fr);
            gap: 20px;
        }
        .badge-earned {
            width: 60px;
            height: 60px;
            border: 2px solid green;
            border-radius: 50%;
            object-fit: contain;
            transition: transform 0.2s;
        }
        .badge-earned:hover {
            transform: scale(1.1);
        }
        .badge-locked {
            border: 2px solid black;
            border-radius: 50%;
            object-fit: contain;
            opacity: 0.3;
            filter: grayscale(100%);
        }
        .badge-new {
            object-fit: contain;
            border: 2px solid #FFD700;
            border-radius: 50%;
        }
        .lock-icon {
            width: 20px;
            height: 20px;
            position: absolute;
        }
        .new-icon {
            width: 20px;
            font-size:20px;
            color: red;
            position:absolute;
            transform:translateX(-50%);
            font-weight: bold;
            background: #FFD700;
            border-radius: 50%;
            display: flex;
            justify-content: center;
            align-items: center;
        }
        /* Pop-up container */
        .popup1, .popup2 {
            display: none; /* Hidden by default */
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
            border-radius:10px;
        }
         /* Overlay background */
         .overlay {
             display: none; /* Hidden by default */
             position: fixed;
             top: 0;
             left: 0;
             width: 100%;
             height: 100%;
             background-color: rgba(0, 0, 0, 0.5);
             z-index: 999;
         }
         .points{
             width:15%;
         }
         .popup-table{
             width:100%;
         }
         .popupform-control{
             width:600px;
             height:200px;
             padding: 8px;
             border: 1px solid #ccc;
             border-radius: 4px;
             font-size: 1rem;
         }
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

    </style>
</asp:Content>
<asp:Content ID="ContentScript" ContentPlaceHolderID="script" runat="server">
     <script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>
     <script type="text/javascript">
         $(document).ready(function () {
             // Link history button to redirect with query string instead of showing popup directly
             $("#<%= lnkHistory.ClientID %>").click(function (e) {
                 e.preventDefault(); // Prevent default behavior
                 window.location.href = "MA_AchievementDashboard.aspx?view=history";
                 return false;
             });

             // Hide pop-up when "Close" button is clicked
             $(".btn-close-popup, #btnClosePopup").click(function () {
                 $(".overlay").fadeOut();
                 $(".popup1").fadeOut();
                 $(".popup2").fadeOut();

                 // Redirect to main dashboard without query string
                 if (window.location.search) {
                     window.location.href = "MA_AchievementDashboard.aspx";
                 }
             });
         });
         $(document).ready(function () {
             // Update Export Testimonial button to directly trigger postback instead of showing popup
             $("#<%= btnExport.ClientID %>").click(function (e) {
                 // Show a loading message before the postback
                 alert("Generating your testimonial. This may take a moment...");

                 // The postback will proceed normally to the btnExport_Click method
                 // No need to prevent default or return false
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
                <asp:Button ID="btnInformation" runat="server" CssClass='sidebar-btn' Text="<%$ Resources:Resources, SideBtnInfo %>" PostBackUrl="~/OMTS_Pages/OMTS_MA/MA_Information.aspx" />
                <asp:Button ID="btnDashboard" runat="server" CssClass='sidebar-btn active' Text="<%$ Resources:Resources, SideBtnDb %>" PostBackUrl="~/OMTS_Pages/OMTS_MA/MA_AchievementDashboard.aspx" />
                <asp:Button ID="btnApplicationForm" runat="server" CssClass='sidebar-btn' Text="<%$ Resources:Resources, SideBtnMAApp %>" PostBackUrl="~/OMTS_Pages/OMTS_MA/MA_AchievementApplication.aspx" />
                <asp:Button ID="btnReviewApp" runat="server" CssClass='sidebar-btn' Text="<%$ Resources:Resources, SideBtnRA %>" PostBackUrl="~/OMTS_Pages/OMTS_MA/MA_ReviewApp.aspx" />
                <asp:Button ID="btnAwardBadge" runat="server" CssClass='sidebar-btn' Text="<%$ Resources:Resources, SideBtnAB%>" PostBackUrl="~/OMTS_Pages/OMTS_MA/MA_AwardBadges.aspx" />
                <asp:Button ID="btnMaterials" runat="server" CssClass='sidebar-btn' Text="<%$ Resources:Resources, SideBtnMAMat%>" PostBackUrl="~/OMTS_Pages/OMTS_MA/MA_Materials.aspx" />
            </div>
             <!-- Main Content Area -->
             <div class="main-content">
                 <h2 class="page-header">
                    <asp:Literal ID="litMyDashboard" runat="server" Text="<%$ Resources:Resources, Heading_MyDashboard%>" />
                </h2>
                <div class="dashboard-container">
                    <div class="dashboard-content">
                        <!-- User Profile Panel -->
                        <div class="profile-card">
                            <asp:Image ID="imgProfile" runat="server" CssClass="profile-avatar" ImageUrl="~/Images/default-avatar.png" />
                            <h3><asp:Label ID="lblUsername" runat="server" Text="[Name]"></asp:Label></h3>
                            <p><%= GetGlobalResourceObject("Resources", "Label_MemberSince") %> 
                                <asp:Label ID="lblJoinDate" runat="server" Text="[JoinDate]"></asp:Label>
                            </p>
                            <div class="stats">
                                <div>
                                    <span class="icon">🏆</span>
                                    <asp:Label ID="lblAchievements" runat="server" Text="[num]"></asp:Label>
                                    <span><%= GetGlobalResourceObject("Resources", "Label_Achievements") %></span>
                                </div>
                                <div>
                                    <span class="icon">🚩</span>
                                    <asp:Label ID="lblCoursesCompleted" runat="server" Text="[num]"></asp:Label>
                                    <span><%= GetGlobalResourceObject("Resources", "Label_CoursesCompleted") %></span>
                                </div>
                            </div>
                            <div class="alternate-link">
                                 <asp:LinkButton ID="lnkHistory" runat="server" CssClass="history-link"
                                Text="<%$ Resources:Resources, Link_AchievementHistory %>" />
                            </div>
                        </div>
                        <!-- Level Progress & Badges -->
                        <div class="achievement-panel">
                            <div class="level-section">
                                <label><b><%= GetGlobalResourceObject("Resources", "Label_CurrentLevel") %>:</b></label>
                                <div class="level-bar">
                                      <asp:Label ID="lblCurrLvl" runat="server" Text="currLvl"  style="padding:3%;"/>                                    
                                         <!-- Progress Bar -->
                                     <div class="progress-container">
                                         <div id="progressBar" runat="server" class="progress-bar">
                                         </div>
                                        <asp:Label ID="lblPoints" runat="server" CssClass="progress-text" Text="curr/max Points"></asp:Label>
                                     </div>
                                     <asp:Label ID="lblNextLvl" runat="server" Text="nextLvl" style="padding:3%;"/>
                                    <asp:Button ID="btnLevelUp" runat="server" CssClass="btn" Text="<%$ Resources:Resources, Button_LevelUp %>"  OnClick="btnLevelUp_Click" />
                                  </div>
                                   <div class="subtext"><%= GetGlobalResourceObject("Resources", "Tip_EarnPoints") %></div>
                            </div>
                            <div class="badges-section" >
                                <label"><b><%= GetGlobalResourceObject("Resources", "Label_BadgesEarned") %>:</b></label>
                                <div class="badges-grid"  style="margin-top:1%;">
                                    <asp:Repeater ID="rptBadges" runat="server" OnItemDataBound="rptBadges_ItemDataBound">
                                        <ItemTemplate>
                                            <div class="badge-icon" >
                                                <asp:Panel ID="pnlNewIcon" runat="server" CssClass="new-icon" Visible='<%# Eval("Status").ToString() == "new" %>'>
                                                    !
                                                </asp:Panel>
                                                <asp:Panel ID="pnlLockIcon" runat="server" CssClass="lock-icon" Visible='<%# Eval("Status").ToString() == "locked" %>'>
                                                    <i class="fa fa-lock" style="font-size:40px;color:black;opacity:1.0;margin-left:70%;margin-top:70%;"></i>
                                                </asp:Panel>
                                                <asp:Image ID="imgBadge" runat="server" 
                                                    CssClass='<%# GetBadgeCssClass(Eval("Status").ToString()) %>'
                                                    ImageUrl='<%# Eval("BadgeIconUrl") %>'
                                                    AlternateText='<%# Eval("BadgeName") %>' 
                                                    style="display: block;
                                                            margin: 0 auto;
                                                            height: 70px;
                                                            width: 70px; 
                                                            margin: 0 0 0 -20%;
                                                            -webkit-border-radius: 50%;
                                                            -moz-border-radius: 50%;
                                                            border-radius: 50%;  "/>
                                                

                                            </div>
                                        </ItemTemplate>
                                    </asp:Repeater>
                                </div>
                            </div>
                            <div class="form-buttons">
                                <asp:Button ID="btnExport" runat="server" CssClass="btn" Text="<%$ Resources:Resources, Button_ExportTestimonial %>" style="margin-top:5%;" OnClick="btnExport_Click" />
                                <br />
                                <asp:Label ID="lblMessage" runat="server" Text="Label" Visible="false"></asp:Label>
                            </div>
                        </div>
                        
                        <div class="overlay"></div>
                         <!-- Pop-up Window for Badges-->
                        <div class="popup1">
                            <h2><asp:Literal ID="litBadgeInfo" runat="server" Text="<%$Resources:Resources, Heading_BadgeInfo %>"/></h2>
                            <table class="form-table">
                                <tr>
                                    <td><%= GetGlobalResourceObject("Resources", "Label_BadgeName") %>:</td> 
                                    <td>
                                        <b><asp:Literal ID="litBadgeNameValue" runat="server" Text="[Badge Name Placeholder]" /></b>
                                    </td>
                                </tr>
                                <tr>
                                    <td><%= GetGlobalResourceObject("Resources", "Label_BadgePicture") %>:</td> 
                                    <td>
                                        <div>
                                         <asp:Image ID="imgBadge" runat="server" ImageUrl='#' CssClass="badge-in-popup"/>
                                        </div>
                                    </td>
                                </tr>
                                <tr>
                                    <td><%= GetGlobalResourceObject("Resources", "Label_BadgeDesc") %>:</td> 
                                    <td>
                                        <b><asp:Literal ID="litBadgeDescValue" runat="server" Text="[Description Placeholder]" /></b>
                                    </td>
                                </tr>
                                <tr>
                                    <td><%= GetGlobalResourceObject("Resources", "Label_Points") %>:</td> 
                                    <td>
                                        <b><asp:Literal ID="litPointValue" runat="server" Text="[Points Placeholder]" /></b>
                                    </td>
                                </tr>
                             </table>  
                            <asp:Label runat="server" ID="lblUnlockNote" Visible="false"><i>Click Unlock to claim this badge and earn the associated points.<br /></i></asp:Label>
                            <asp:HiddenField ID="hdnSelectedBadgeId" runat="server" />
                            <asp:Button ID="btnUnlock" runat="server" CssClass="btn" Text="<%$ Resources:Resources, Button_Unlock %>" OnClick="btnUnlock_Click" />
                            <button id="btnClosePopup" type="button" class="btn btn-close-popup"><%= GetGlobalResourceObject("Resources", "Button_Close") ?? "Close" %></button>
                                   
                        </div>
                         <!-- Pop-up Window for Applications History-->
                        <div class="popup2">
                            <h2 class="page-header">
                                <asp:Literal ID="litRecentApplication" runat="server" Text="<%$ Resources:Resources, Heading_ListOfRecentApplications %>" />
                            </h2>
                             

                            <asp:GridView ID="gvApplications" runat="server" AutoGenerateColumns="False" 
                                CssClass="list-table" OnRowCommand="gvApplications_RowCommand"
                                EmptyDataText="You haven't submitted any applications yet." Width="100%" GridLines="None">
                                <Columns>
                                    <asp:BoundField DataField="ApplicationId" HeaderText="ID" />
                                    <asp:BoundField DataField="EventName" HeaderText="Event" />
                                    <asp:BoundField DataField="ParticipatedRole" HeaderText="Role" />
                                    <asp:TemplateField HeaderText="Status">
                                        <ItemTemplate>
                                            <asp:Label ID="lblStatus" runat="server" 
                                                Text='<%# Eval("Status") %>' 
                                                CssClass='<%# GetStatusCssClass(Eval("Status").ToString()) %>' />
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Action">
                                        <ItemTemplate>
                                            <asp:LinkButton ID="lnkViewDetails" runat="server" Text="View Details" CssClass="action-link"
                                                CommandName="ViewDetails" CommandArgument='<%# Eval("ApplicationId") %>' />
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                </Columns>
                            </asp:GridView>
                             
                            <button id="btnClosePopup2" type="button" class="btn btn-close-popup"><%= GetGlobalResourceObject("Resources", "Button_Close") ?? "Close" %></button>
                        </div>

                        
                </div>
                 </div>
            </div>
</body>
</asp:Content>