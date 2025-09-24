<%@ Page Language="C#" Async="true" AutoEventWireup="true" CodeBehind="AM_UpdateAccountSecurity.aspx.cs" Inherits="fyp.UpdateAccountSecurity" MasterPageFile="~/Site1.Master" %>

<asp:Content ID="ContentTitle" ContentPlaceHolderID="title" runat="server">Manage Account Security</asp:Content>
<asp:Content ID="ContentStyle" ContentPlaceHolderID="css" runat="server">
    <link rel="stylesheet" type="text/css" href="#" />
    <style>
        /* Password verification panel styles */
        .verification-panel {
            background-color: #f9f9f9;
            border: 1px solid #ddd;
            border-radius: 5px;
            padding: 20px;
            margin-bottom: 20px;
            box-shadow: 0 2px 5px rgba(0,0,0,0.1);
        }
        .verification-title {
            font-size: 18px;
            font-weight: bold;
            margin-bottom: 15px;
            color: #333;
        }
        .verification-instructions {
            margin-bottom: 15px;
            font-size: 14px;
            color: #555;
        }
        .verification-buttons {
            display: flex;
            gap: 10px;
            margin-top: 15px;
        }
    </style>
</asp:Content>
<asp:Content ID="ContentScript" ContentPlaceHolderID="script" runat="server">
<script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
<script type="text/javascript">
    function confirmSaveChanges() {
        Swal.fire({
            title: 'Attention',
            text: 'Are you sure you want to save changes?',
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#3085d6',
            cancelButtonColor: '#d33',
            confirmButtonText: 'Yes, save changes!'
        }).then((result) => {
            if (result.isConfirmed) {
                __doPostBack('<%= btnSaveChanges.UniqueID %>', '');
            }
        });
        return false; // Prevent default postback
    }

    function confirmDeleteAccount() {
        Swal.fire({
            title: 'Warning!',
            html: 'Are you sure you want to delete your account?<br><small>You will not be able to log in to the same account until the admin re-enables it. You will also be logged out automatically.</small>',
            icon: 'error',
            showCancelButton: true,
            confirmButtonColor: '#d33',
            cancelButtonColor: '#3085d6',
            confirmButtonText: 'Yes, delete account!'
        }).then((result) => {
            if (result.isConfirmed) {
                __doPostBack('<%= btnDeleteAccount.UniqueID %>', '');
            }
        });
        return false; // Prevent default postback
    }
</script>
</asp:Content>
<asp:Content ID="ContentMain" ContentPlaceHolderID="MainContent" runat="server">
<body>
        <!-- Main Content Layout -->
        <div class="container">
            <!-- Sidebar -->
            <div class="sidebar">
                <asp:Button ID="btnPersonalInfo" runat="server" CssClass='<%# (Request.Url.AbsolutePath.EndsWith("AM_UpdatePersonalInfo.aspx")) ? "sidebar-btn active" : "sidebar-btn" %>' Text="<%$ Resources:Resources, SideBtnPI %>" PostBackUrl="~/OMTS_Pages/OMTS_AM/AM_UpdatePersonalInfo.aspx" />
                <asp:Button ID="btnAccSecurity" runat="server" CssClass='<%# (Request.Url.AbsolutePath.EndsWith("AM_UpdateAccountSecurity.aspx")) ? "sidebar-btn active" : "sidebar-btn" %>' Text="<%$ Resources:Resources, SideBtnAS %>" PostBackUrl="~/OMTS_Pages/OMTS_AM/AM_UpdateAccountSecurity.aspx" />
            </div>

            <!-- Main Content Area -->
            <div class="main-content">
                <div>
                    <h2 class="page-header">
                        <asp:Literal ID="litSecuritySettings" runat="server" Text="<%$ Resources:Resources, Heading_UpdateAccountSecurity %>" />
                    </h2>

                    <!-- Password Verification Panel -->
                    <asp:Panel ID="pnlPasswordVerification" runat="server" CssClass="verification-panel" Visible="false">
                        <div class="verification-instructions">
                            <asp:Label ID="lblVerificationInstructions" runat="server" CssClass="lbl" Text="<%$ Resources:Resources, Text_PasswordVerificationInstructions %>" />
                        </div>
                        <table class="form-table">
                            <tr>
                                <td>
                                    <asp:Label ID="lblVerifyPassword" runat="server" Text="<%$ Resources:Resources, Label_CurrentPassword %>" />:
                                </td>
                                <td>
                                    <asp:TextBox ID="txtVerifyPassword" runat="server" TextMode="Password" CssClass="text-input"></asp:TextBox>
                                </td>
                            </tr>
                        </table>
                        <div class="verification-buttons">
                            <asp:Button ID="btnVerifyPassword" runat="server" CssClass="btn" Text="<%$ Resources:Resources, Button_Verify %>" OnClick="btnVerifyPassword_Click" />
                            <asp:Button ID="btnCancelVerification" runat="server" CssClass="btn" Text="<%$ Resources:Resources, Button_Cancel %>" OnClick="btnCancelVerification_Click" />
                        </div>
                    </asp:Panel>

                    <!-- Main Security Form -->
                    <asp:Panel ID="pnlSecurityForm" runat="server">
                        <!-- Invisible Table for Layout -->
                        <table class="form-table">
                            <tr>
                                <td><%= GetGlobalResourceObject("Resources", "Label_Email") %>:</td>
                                <td><asp:TextBox ID="txtEmail" runat="server" CssClass="text-input" Enabled="False" /></td>
                            </tr>
                            <tr>
                                <td><%= GetGlobalResourceObject("Resources", "Label_PhoneNumber") %>:</td>
                                <td><asp:TextBox ID="txtPhoneNumber" runat="server" CssClass="text-input" /></td>
                            </tr>
                            <tr>
                                <td>Reset Password<%--<%= GetGlobalResourceObject("Resources", "Label_ResetPw") %>:--%></td>
                                <td><asp:Button ID="btnResetPassword" runat="server" Text="Send Link to Email" CssClass="btn" OnClick="btnResetPassword_Click" /></td>
                            </tr>
                            <tr>
                                <td><%= GetGlobalResourceObject("Resources", "Label_SecurityQuestion") %>:</td>
                                <td>
                                    <asp:DropDownList ID="ddlSecurityQuestion" runat="server" CssClass="text-input"></asp:DropDownList>
                                </td>
                            </tr>
                            <tr>
                                <td><%= GetGlobalResourceObject("Resources", "Label_SecurityAnswer") %>:</td>
                                <td><asp:TextBox ID="txtAnswer" runat="server" CssClass="text-input" /></td>
                            </tr>
                        </table>

                        <!-- Buttons Section -->
                        <div class="form-buttons">
                            <asp:Button ID="btnSaveChanges" runat="server" Text="<%$ Resources:Resources, Button_SaveChanges %>" CssClass="btn" OnClick="btnSaveChanges_Click" OnClientClick="return confirmSaveChanges();"/>
                            <asp:Button ID="btnDeleteAccount" runat="server" Text="<%$ Resources:Resources, Button_DeleteAccount %>" CssClass="btn btn-danger" OnClick="btnDeleteAccount_Click" style="background-color:darkred;" OnClientClick="return confirmDeleteAccount();" />
                        </div>
                    </asp:Panel>
                </div>
            </div>
        </div>
</body>
</asp:Content>