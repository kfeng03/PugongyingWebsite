<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="AM_verifyOTP.aspx.cs" MasterPageFile="~/Site1.Master" Inherits="fyp.verifyOTP" %>

<asp:Content ID="ContentTitle" ContentPlaceHolderID="title" runat="server">Verification</asp:Content>
<asp:Content ID="ContentStyle" ContentPlaceHolderID="css" runat="server">
    <link rel="stylesheet" type="text/css" href="#" />
</asp:Content>
<asp:Content ID="ContentScript" ContentPlaceHolderID="script" runat="server">
<script>
    function moveToNext(current, nextFieldID) {
        if (current.value.length === 1) {
            document.getElementById(nextFieldID)?.focus();
        }
    }

    // Enable Backspace Navigation
    document.addEventListener("keydown", function (event) {
        if (event.key === "Backspace") {
            let current = document.activeElement;
            if (current.classList.contains("input-pin") && current.value === "") {
                let previous = current.previousElementSibling;
                if (previous && previous.classList.contains("input-pin")) {
                    previous.focus();
                }
            }
        }
    });

</script>  
</asp:Content>

<asp:Content ID="ContentMain" ContentPlaceHolderID="MainContent" runat="server">
    <body>
   
        <!-- OTP Verification Container -->
        <div class="verify-container">
            <!-- Heading -->
            <h1><asp:Literal ID="litOtpHeading" runat="server" Text="<%$ Resources:Resources, Heading_Proceed%>" /></h1>
            <p class="otp-verification-subtext">
                <asp:Literal ID="litOtpSubtext" runat="server" Text="<%$ Resources:Resources, Subtext_Otp_Verification %>" />
            </p>

            <!-- Pin Number Input -->
            <div class="form-group">
                <label for="txtPin">
                    <asp:Literal ID="litPinNumber" runat="server" Text="<%$ Resources:Resources, Label_Otp_PinNumber %>" />
                </label>
                <div class="pin-container">
                    <input type="text" maxlength="1" class="input-pin" id="pin1" oninput="moveToNext(this, 'pin2')">
                    <input type="text" maxlength="1" class="input-pin" id="pin2" oninput="moveToNext(this, 'pin3')">
                    <input type="text" maxlength="1" class="input-pin" id="pin3" oninput="moveToNext(this, 'pin4')">
                    <input type="text" maxlength="1" class="input-pin" id="pin4" oninput="moveToNext(this, 'pin5')">
                    <input type="text" maxlength="1" class="input-pin" id="pin5" oninput="moveToNext(this, 'pin6')">
                    <input type="text" maxlength="1" class="input-pin" id="pin6">
                </div>
            </div>

            <!-- Hyperlinks for Actions -->
            <div class="form-group">
                <p class="alternate-link">
                    <asp:HyperLink ID="lnkResendOtp" runat="server" Text="<%$ Resources:Resources, Link_Otp_ResendCode %>" NavigateUrl="~/ResendOtp.aspx" />
                </p>
            </div>

            <!-- Verify Button -->
            <div class="form-group">
                <asp:Button ID="btnVerify" runat="server" CssClass="btn" Text="<%$ Resources:Resources, Button_Otp_Verify %>" />
            </div>
        </div>
        </body>
</asp:Content>
