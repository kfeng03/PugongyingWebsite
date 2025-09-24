<%@ Page Language="C#" Async="true" AutoEventWireup="true" CodeBehind="Admin_AccList.aspx.cs" Inherits="fyp.AccList" MasterPageFile="~/Site1.Master" %>

<asp:Content ID="ContentTitle" ContentPlaceHolderID="title" runat="server">Account Management</asp:Content>
<asp:Content ID="ContentStyle" ContentPlaceHolderID="css" runat="server">
    <link rel="stylesheet" type="text/css" href="#" />
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.0.0-beta3/css/all.min.css" />
    <style>
    
    .popup, .analysis-popup {
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

    .edit-role-icon {
        cursor: pointer;
        color: black; 
        margin-left: 10px; /* Add some spacing */
        font-size: 16px; /* Adjust size */
    }

    .edit-role-icon:hover {
        color:gray;
    }
    </style>
</asp:Content>
<asp:Content ID="ContentScript" ContentPlaceHolderID="script" runat="server">
<script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>
<script type="text/javascript">
    $(document).ready(function () {
        // Show pop-up when "Edit Role" button is clicked
        $(".edit-role-icon").click(function (e) {
            e.preventDefault(); // Prevent postback

            // Get the User ID from the button's CommandArgument
            var userId = $(this).attr("data-userid");
            // Set the User ID in the pop-up
            $("#<%= lblEditUserId.ClientID %>").text(userId);
            // Redirect to the same page with the userId as a query string parameter
            window.location.href = "Admin_AccList.aspx?userId=" + userId + "&popup=1";;
        });

        // Hide pop-up when "Close" button is clicked
        $("#btnCloseEditRolePopup").click(function () {
            $(".overlay").fadeOut();
            $(".popup").fadeOut();
        });

        // Hide pop-up when "Save" button is clicked (optional)
        $("#<%= btnSaveRole.ClientID %>").click(function () {
            $(".overlay").fadeOut();
            $(".popup").fadeOut();
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
    <asp:Button ID="btnAccMgmt" runat="server" CssClass='<%# (Request.Url.AbsolutePath.EndsWith("Admin_AccList.aspx")) ? "sidebar-btn active" : "sidebar-btn" %>' Text="<%$ Resources:Resources, SideBtnAM %>" PostBackUrl="~/OMTS_Pages/OMTS_Admin/Admin_AccList.aspx" />
    <asp:Button ID="btnCustom" runat="server" CssClass='<%# (Request.Url.AbsolutePath.EndsWith("Admin_Customization.aspx")) ? "sidebar-btn active" : "sidebar-btn" %>' Text="<%$ Resources:Resources, SideBtnWC %>" PostBackUrl="~/OMTS_Pages/OMTS_Admin/Admin_Customization.aspx" />
    <asp:Button ID="btnCertManager" runat="server" CssClass='<%# (Request.Url.AbsolutePath.EndsWith("Admin_TemplateManager.aspx")) ? "sidebar-btn active" : "sidebar-btn" %>' Text="<%$ Resources:Resources, SideBtnTM %>" PostBackUrl="~/OMTS_Pages/OMTS_Admin/Admin_TemplateManager.aspx" />
</div>

            <!-- Main Content Area -->
            <div class="main-content">
                <div>
                    <h2 class="page-header">
                        <asp:Literal ID="litUserAccounts" runat="server" Text="<%$ Resources:Resources, Heading_ListOfUserAccounts %>" />
                    </h2>

                    <table class="list-table">
                        <thead>
                            <tr>
                                <th><%= GetGlobalResourceObject("Resources", "TableHeader_UserID") %></th>
                                <th><%= GetGlobalResourceObject("Resources", "TableHeader_Username") %></th>
                                <th><%= GetGlobalResourceObject("Resources", "TableHeader_PhoneNumber") %></th>
                                <th><%= GetGlobalResourceObject("Resources", "TableHeader_EmailAddress") %></th>
                                <th><%= GetGlobalResourceObject("Resources", "TableHeader_Role") %></th>
                                <th></th>
                            </tr>
                        </thead>
                        <tbody>
                            <asp:Repeater ID="rptAccounts" runat="server" OnItemDataBound="rptAccounts_ItemDataBound">
                                <ItemTemplate>
                                    <tr>
                                        <td><%# Eval("UserId") %></td>
                                        <td><%# Eval("Username") %></td>
                                        <td><%# Eval("PhoneNumber") %></td>
                                        <td><%# Eval("Email") %></td>
                                        <td>
                                            <%# Eval("Role") %>
                                             <i class="fas fa-pencil-alt edit-role-icon" data-userid='<%# Eval("UserId") %>' ></i>
                                        </td>
                                        <td>
                                            <asp:Button ID="btnResetPassword" runat="server" Text="Reset Password" CssClass="btn" 
                                                CommandArgument='<%# Eval("UserId") %>' OnClick="btnResetPassword_Click" />
                                            <asp:Button ID="btnDisableAccount" runat="server" Text="Disable Account" CssClass="btn btn-danger" style="background-color:darkred;"
                                                CommandArgument='<%# Eval("UserId") %>' OnClick="btnDisableAccount_Click" />
                                            <asp:Button ID="btnEnableAccount" runat="server" Text="Enable Account" CssClass="btn"  style="background-color:darkgreen;"
                                                CommandArgument='<%# Eval("UserId") %>' OnClick="btnEnableAccount_Click"/>
                                        </td>
                                    </tr>
                                </ItemTemplate>
                            </asp:Repeater>
                        </tbody>
                    </table>

                    <!-- Pop-up Window for Edit Role -->
                    <div class="overlay"></div>
                    <div class="popup" style="width:30%;">
                        <h2>Edit Role</h2>
                        <table class="popup-table" >
                            <tr style="line-height:2;">
                                <td>User ID:</td>
                                <td>
                                    <asp:Label ID="lblEditUserId" runat="server" />
                                </td>
                            </tr>
                            <tr style="line-height:2;">
                                <td>Username:</td>
                                <td>
                                    <asp:Label ID="lblEditUsername" runat="server" />
                                </td>
                            </tr>
                             <tr style="line-height:2;">
                                <td>Phone Number:</td>
                                <td>
                                    <asp:Label ID="lblEditHP" runat="server" />
                                </td>
                            </tr>
                            <tr style="line-height:2;">
                                <td>Email Address:</td>
                                <td>
                                    <asp:Label ID="lblEditEmail" runat="server" />
                                </td>
                            </tr>
                            <tr >
                                <td><p>Role:</p></td>
                                <td>
                                    <asp:DropDownList ID="ddlEditRole" runat="server" CssClass="ddl">
                                        <asp:ListItem Text="Member" Value="Member" />
                                        <asp:ListItem Text="Staff" Value="Staff" />
                                        <asp:ListItem Text="Admin" Value="Admin" />
                                    </asp:DropDownList>
                                </td>
                            </tr>
                            <tr style="line-height:2;">
                                <td colspan="2">
                                    <asp:Button ID="btnSaveRole" runat="server" Text="Save" CssClass="btn" OnClick="btnSaveRole_Click" />
                                    <button id="btnCloseEditRolePopup" class="btn">Cancel</button>
                                </td>
                            </tr>
                        </table>
                    </div>
                </div>
            </div>
        </div>
</body>
</asp:Content>

