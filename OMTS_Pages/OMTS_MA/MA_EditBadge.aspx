<%@ Page Async="true" Language="C#" AutoEventWireup="true" EnableEventValidation="false" CodeBehind="MA_EditBadge.aspx.cs" Inherits="fyp.MA_EditBadge" MaintainScrollPositionOnPostBack="true" MasterPageFile="~/Site1.Master" %>


<asp:Content ID="ContentTitle" ContentPlaceHolderID="title" runat="server">Manage Materials</asp:Content>
<asp:Content ID="ContentStyle" ContentPlaceHolderID="css" runat="server">
    <link rel="stylesheet" type="text/css" href="#" />
    <style>
        .popup, .edit-popup {
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

        .points {
            width: 15%;
        }

        .popup-table {
            width: 100%;
        }

        .popupform-control {
            width: 600px;
            padding: 8px;
            border: 1px solid #ccc;
            border-radius: 4px;
            font-size: 1rem;
        }
    </style>
</asp:Content>
<asp:Content ID="ContentScript" ContentPlaceHolderID="script" runat="server">
    <script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>
    <script type="text/javascript">
        $(document).ready(function () {
            
            // Increment points
            $("#btnIncrement").click(function (e) {
                e.preventDefault();
                var pointsInput = $("#<%= txtEditPointsAwarded.ClientID %>");
                var currentValue = parseInt(pointsInput.val()) || 0;
                pointsInput.val(currentValue + 5);
            });

            // Decrement points
            $("#btnDecrement").click(function (e) {
                e.preventDefault();
                var pointsInput = $("#<%= txtEditPointsAwarded.ClientID %>");
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
                    <asp:Button ID="btnReviewApp" runat="server" CssClass='sidebar-btn' Text="<%$ Resources:Resources, SideBtnRA %>" PostBackUrl="~/OMTS_Pages/OMTS_MA/MA_ReviewApp.aspx" />
                    <asp:Button ID="btnAwardBadge" runat="server" CssClass='sidebar-btn' Text="<%$ Resources:Resources, SideBtnAB%>" PostBackUrl="~/OMTS_Pages/OMTS_MA/MA_AwardBadges.aspx" />
                    <asp:Button ID="btnMaterials" runat="server" CssClass='sidebar-btn active' Text="<%$ Resources:Resources, SideBtnMAMat%>" PostBackUrl="~/OMTS_Pages/OMTS_MA/MA_Materials.aspx" />
                </div>

                <!-- Main Content Area -->
                <div class="main-content">
                    <h2><asp:Literal ID="litHeading1" runat="server" Text="Editing Badge" /></h2>
                        <table class="form-table">
                            <tr>
                                <td colspan="2">
                                    <asp:Label ID="lblMessage" runat="server" />
                                </td>
                            </tr>
                            <tr>
                                <td>Badge ID:</td>
                                <td>
                                    <asp:Label ID="lblEditBadgeId" runat="server" />
                                </td>
                            </tr>
                            <tr>
                                <td><%= GetGlobalResourceObject("Resources", "Label_BadgeName") %>:</td>
                                <td>
                                    <asp:TextBox ID="txtEditBadgeName" runat="server" CssClass="form-control"></asp:TextBox>
                                </td>
                            </tr>
                            <tr>
                                <td><%= GetGlobalResourceObject("Resources", "Label_BadgePicture") %>:</td>
                                <td>
                                    <div class="badge-pic-container">
                                        <img id="editBadgePreview" src="badge.png" alt="Badge Picture" class="uploaded-pic" />
                                        <div class="upload-overlay">
                                            <i class="fa fa-camera"></i>
                                            <span>Click to Upload</span>
                                        </div>
                                        <asp:FileUpload ID="fuEditBadge" runat="server" CssClass="file-upload" onchange="previewImage(event)" />
                                    </div>
                                </td>
                            </tr>
                            <tr>
                                <td><%= GetGlobalResourceObject("Resources", "Label_BadgeDesc") %>:</td>
                                <td>
                                    <asp:TextBox ID="txtEditBadgeDescription" runat="server" CssClass="popupform-control" TextMode="MultiLine"></asp:TextBox>
                                </td>
                            </tr>
                            <tr>
                                <td><%= GetGlobalResourceObject("Resources", "Label_Points") %>: </td>
                                <td>
                                    <div class="points-section">
                                        <button id="btnDecrementEdit">-</button>
                                        <asp:TextBox ID="txtEditPointsAwarded" runat="server" TextMode="Number" value="0" CssClass="points"></asp:TextBox>
                                        <button id="btnIncrementEdit">+</button>
                                    </div>
                                </td>
                            </tr>
                            <tr>
                                <td colspan="2">
                                    <asp:Button ID="btnSaveEditBadge" runat="server" Text="Save" CssClass="btn" OnClick="btnSaveEditBadge_Click" />
                                   <asp:Button ID="btnCloseEditPopup" class="btn btn-close-popup" runat="server" Text="<%$ Resources:Resources, Button_Cancel %>" OnClick="btnCancel_Click" />
                                </td>
                            </tr>
                        </table>
                    </div>
                    
                </div>
    </body>
</asp:Content>
