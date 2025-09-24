<%@ Page Title="" Async="true" Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="MA_ReviewAppDetail.aspx.cs" Inherits="fyp.MA_ReviewAppDetail" %>
<asp:Content ID="ContentTitle" ContentPlaceHolderID="title" runat="server">Reviewing Application</asp:Content>
<asp:Content ID="ContentStyle" ContentPlaceHolderID="css" runat="server">
     <link rel="stylesheet" type="text/css" href="#" />
     <style>
         /* Pop-up container */
         .popup {
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
         .back-link {
             margin-bottom: 15px;
             display: block;
         }
     </style>
</asp:Content>
<asp:Content ID="ContentScript" ContentPlaceHolderID="script" runat="server">
        <script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>
        <script type="text/javascript">
            $(document).ready(function () {
                // Show pop-up when "Create New Badge" button is clicked
                $("#<%= btnApprove.ClientID %>, #<%= btnReject.ClientID %>").click(function (e) {
                    e.preventDefault(); // Prevent postback
                    // Determine which button was clicked
                    var isApprove = $(this).attr("id") === "<%= btnApprove.ClientID %>";
                        // Set hidden field for action type
                        $("#<%= hdnAction.ClientID %>").val(isApprove ? "approve" : "reject");

                        // Enable/disable points section based on button clicked
                        if (isApprove) {
                            $("#<%= txtPointsAwarded.ClientID %>").prop("disabled", false); // Enable textbox
                            $("#btnIncrement").prop("disabled", false);
                            $("#btnDecrement").prop("disabled", false);
                            $(".popup h2").text("Approve Application");
                        } else {
                            $("#<%= txtPointsAwarded.ClientID %>").val("0").prop("disabled", true); // Set value to 0 and disable
                            $("#btnIncrement").prop("disabled", true);
                            $("#btnDecrement").prop("disabled", true);
                            $(".popup h2").text("Reject Application");
                        }
                        // Show pop-up and overlay
                        $(".overlay").fadeIn();
                        $(".popup").fadeIn();
                    });
                // Hide pop-up when "Close" button is clicked
                $("#btnClosePopup").click(function () {
                    $(".overlay").fadeOut();
                    $(".popup").fadeOut();
                });
                // Increment points
                $("#btnIncrement").click(function (e) {
                    e.preventDefault();
                    var pointsInput = $("#<%= txtPointsAwarded.ClientID %>");
                        var currentValue = parseInt(pointsInput.val()) || 0;
                        pointsInput.val(currentValue + 5);
                    });
                // Decrement points
                $("#btnDecrement").click(function (e) {
                    e.preventDefault();
                    var pointsInput = $("#<%= txtPointsAwarded.ClientID %>");
                        var currentValue = parseInt(pointsInput.val()) || 0;
                        if (currentValue > 0) {
                            pointsInput.val(currentValue - 5);
                        }
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
                <asp:Button ID="btnDashboard" runat="server" CssClass='sidebar-btn' Text="<%$ Resources:Resources, SideBtnDb %>" PostBackUrl="~/OMTS_Pages/OMTS_MA/MA_AchievementDashboard.aspx" />
                <asp:Button ID="btnApplicationForm" runat="server" CssClass='sidebar-btn' Text="<%$ Resources:Resources, SideBtnMAApp %>" PostBackUrl="~/OMTS_Pages/OMTS_MA/MA_AchievementApplication.aspx" />
                <asp:Button ID="btnReviewApp" runat="server" CssClass='sidebar-btn active' Text="<%$ Resources:Resources, SideBtnRA %>" PostBackUrl="~/OMTS_Pages/OMTS_MA/MA_ReviewApp.aspx" />
                <asp:Button ID="btnAwardBadge" runat="server" CssClass='sidebar-btn' Text="<%$ Resources:Resources, SideBtnAB%>" PostBackUrl="~/OMTS_Pages/OMTS_MA/MA_AwardBadges.aspx" />
                <asp:Button ID="btnMaterials" runat="server" CssClass='sidebar-btn' Text="<%$ Resources:Resources, SideBtnMAMat%>" PostBackUrl="~/OMTS_Pages/OMTS_MA/MA_Materials.aspx" />
            </div>
                <!-- Main Content Area -->
                <div class="main-content">
                    <h2>
                        <a class="lbl" href="javascript:history.back();" style="text-decoration: none; margin-right: 10px; font-size: 1.2em;">&#10216;</a>
                        <asp:Literal ID="litHeading" runat="server" Text="<%$ Resources:Resources, Heading_ReviewApp %>" />
                    </h2>



                    <table class="form-table">
                        <tr>
                            <td><%= GetGlobalResourceObject("Resources", "Label_ApplicationID") %>:</td> 
                            <td>
                                <b><asp:Literal ID="litApplicationId" runat="server" Text="" /></b>
                            </td>
                        </tr>
                        <tr>
                            <td><%= GetGlobalResourceObject("Resources", "Label_EventName") %>:</td> 
                            <td>
                                <b><asp:Literal ID="litEventNameValue" runat="server" Text="" /></b>
                            </td>
                        </tr>
                        <tr>
                            <td><%= GetGlobalResourceObject("Resources", "Label_ApplicantName") %>:</td> 
                            <td>
                                <b><asp:Literal ID="litApplNameValue" runat="server" Text="" /></b>
                            </td>
                        </tr>
                        <tr>
                            <td><%= GetGlobalResourceObject("Resources", "Label_AARole") %>:</td> 
                            <td>
                                <b><asp:Literal ID="litRoleValue" runat="server" Text="" /></b>
                            </td>
                        </tr>
                        <tr>
                            <td><%= GetGlobalResourceObject("Resources", "Label_AALearningOutcome") %>:</td> 
                            <td>
                                <b><asp:Literal ID="litOutcomeText" runat="server" Text="" /></b>
                            </td>
                        </tr>
                        <tr>
                            <td><%= GetGlobalResourceObject("Resources", "Label_Attachments") %>:</td> 
                            <td>
&nbsp;<div id="divNoAttch" runat="server" class="no-materials">
                                    No materials uploaded.
                                </div>
                                <!-- PDF Container -->
                                <div id="divPdfContainer" runat="server" class="media-container" visible="false">
                                    <!-- PDF Embed (for Google Drive) -->
                                    <asp:Literal ID="litPdfEmbed" runat="server" />
                                </div>
                            </td>
                        </tr>
                        <tr id="trExistingComments" runat="server" visible="false">
                            <td><%= GetGlobalResourceObject("Resources", "Label_PreviousComments") %>:</td> 
                            <td>
                                <b><asp:Literal ID="litExistingComments" runat="server" Text="" /></b>
                            </td>
                        </tr>
                        <tr>
                            <td><%= GetGlobalResourceObject("Resources", "Label_Status") %>:</td> 
                            <td><asp:Label ID="lblStatus" runat="server" CssClass="status-pending"></asp:Label></td>

                        </tr>
                    </table>                    
                    
                    <!-- Approve/Reject Buttons - Only visible for pending applications -->
                    <asp:Panel ID="pnlActions" runat="server">
                        <div class="actions">
                            <asp:Button ID="btnApprove" runat="server" CssClass="btn btn-approve" Text="<%$ Resources:Resources, Button_Approve %>" />
                            <asp:Button ID="btnReject" runat="server" CssClass="btn btn-reject" Text="<%$ Resources:Resources, Button_Reject %>"  />
                        </div>
                    </asp:Panel>

                    <!-- Hidden field to store action (approve/reject) -->
                    <asp:HiddenField ID="hdnAction" runat="server" />
                      
                    <!-- Pop-up Window for Approve/Reject -->
                    <div class="overlay"></div>
                    <div class="popup">
                        <h2><asp:Literal ID="litProceed" runat="server" Text="<%$Resources:Resources, Heading_Proceed %>"/></h2>
                        <asp:Literal ID="litDetails" runat="server" Text="<%$Resources:Resources, Note_ProceedTip %>" />
                        <table class="popup-table">
                            <tr>
                                <td><%= GetGlobalResourceObject("Resources", "Label_Comment") %>: </td>
                                <td>
                                    <asp:TextBox ID="txtComment" runat="server" CssClass="popupform-control" TextMode="MultiLine" placeholder="<%$ Resources:Resources, Placeholder_Comment %>"></asp:TextBox>
                                </td>
                            </tr>
                            <tr>
                                <td><%= GetGlobalResourceObject("Resources", "Label_Points") %>: </td>
                                <td>
                                    <div class="points-section">
                                        <button id="btnDecrement">-</button>
                                        <asp:TextBox ID="txtPointsAwarded" runat="server" TextMode="Number" value="0" CssClass="points"></asp:TextBox>
                                        <button id="btnIncrement">+</button>
                                    </div>
                                </td>
                            </tr>
                            <tr>
                                <td colspan="2">
                                    <asp:Button ID="btnProceed" runat="server" Text="Proceed" CssClass="btn" OnClick="btnProceed_Click" />
                                    <button id="btnClosePopup" type="button" class="btn">Close</button>
                                </td>
                            </tr>
                        </table>
                    </div>
                </div>
            </div>
</body>
</asp:Content>