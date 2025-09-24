<%@ Page Title="" Language="C#" Async="true" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="AM_ForgotPassword.aspx.cs" Inherits="fyp.AM_ForgotPassword" %>

<asp:Content ID="ContentTitle" ContentPlaceHolderID="title" runat="server">Forgot Password</asp:Content>
<asp:Content ID="ContentStyle" ContentPlaceHolderID="css" runat="server">
    <link rel="stylesheet" type="text/css" href="#" />
    <style>
        .reset-steps {
            display: flex;
            justify-content: space-between;
            margin-bottom: 20px;
            position: relative;
        }
        
        .step {
            width: 30%;
            text-align: center;
            padding: 10px;
            background-color: #f0f0f0;
            border-radius: 5px;
            position: relative;
            z-index: 1;
        }
        
        .step.active {
            background-color: #4CAF50;
            color: white;
            font-weight: bold;
        }
        
        .step-connection {
            position: absolute;
            height: 2px;
            background-color: #ddd;
            top: 50%;
            left: 15%;
            right: 15%;
            z-index: 0;
        }
        
        .error-message {
            color: #ff0000;
            margin: 10px 0;
        }
        
        .success-message {
            color: #4CAF50;
            margin: 10px 0;
        }
    </style>
</asp:Content>
<asp:Content ID="ContentScript" ContentPlaceHolderID="script" runat="server">
    <script type="module">
        // Import Firebase functions
        import { initializeApp } from "https://www.gstatic.com/firebasejs/11.3.1/firebase-app.js";
        import { getAuth, sendPasswordResetEmail } from "https://www.gstatic.com/firebasejs/11.3.1/firebase-auth.js";

        // Your Firebase configuration
        const firebaseConfig = {
            apiKey: "AIzaSyD7OIF-dnrsre_YaDVuWjw527whdmaSoi0",
            authDomain: "pgy-omts.firebaseapp.com",
            databaseURL: "https://pgy-omts-default-rtdb.asia-southeast1.firebasedatabase.app",
            projectId: "pgy-omts",
            storageBucket: "pgy-omts.firebasestorage.app",
            messagingSenderId: "388216837936",
            appId: "1:388216837936:web:bd346048b18f5313fc5af5",
            measurementId: "G-BMBETQNEMC"
        };
        // Initialize Firebase
        const app = initializeApp(firebaseConfig);
        const auth = getAuth(app);

        // Function to send password reset email
        window.sendResetEmail = async function (email) {
            try {
                await sendPasswordResetEmail(auth, email);
                return true;
            } catch (error) {
                console.error("Error sending reset email:", error);
                return false;
            }
        };
    </script>
</asp:Content>

<asp:Content ID="ContentMain" ContentPlaceHolderID="MainContent" runat="server">
    <body>
        <!-- Reset Password Container -->
        <div class="login-container">
            <!-- Heading -->
            <h2><asp:Literal ID="litResetHeading" runat="server" Text="<%$ Resources:Resources, Reset_Password_Heading %>" /></h2>
            <p class="subtext">
                <asp:Literal ID="litResetSubtext" runat="server" Text="<%$ Resources:Resources, Reset_Password_Info %>" />
            </p>

            <!-- Reset Steps -->
            <div class="reset-steps">
                <div class="step-connection"></div>
                <div id="stepEmail" runat="server" class="step active"><%= GetGlobalResourceObject("Resources", "stepEmail") %></div>
                <div id="stepSecurity" runat="server" class="step"><%= GetGlobalResourceObject("Resources", "stepSecurity") %></div>
                <div id="stepReset" runat="server" class="step"><%= GetGlobalResourceObject("Resources", "stepReset") %></div>
            </div>

            <!-- Error/Success Messages -->
            <asp:Label ID="lblMessage" runat="server" CssClass="error-message" Visible="false"></asp:Label>

            <!-- Step 1: Email Panel -->
            <asp:Panel ID="pnlEmail" runat="server" Visible="true">
                <div class="form-group">
                    <label for="txtEmail">
                        <%= GetGlobalResourceObject("Resources", "Label_Email") %>:
                    </label>
                    <asp:TextBox ID="txtEmail" runat="server" CssClass="input-text" />
                </div>

                <div class="form-group">
                    <asp:Button ID="btnContinue" runat="server" CssClass="btn" 
                        Text="<%$ Resources:Resources, Button_Continue %>" 
                        OnClick="btnContinue_Click" />
                </div>
            </asp:Panel>

            <!-- Step 2: Security Question Panel -->
            <asp:Panel ID="pnlSecurityQuestion" runat="server" Visible="false">
                <div class="form-group">
                    <label for="lblSecurityQuestion">
                        <%= GetGlobalResourceObject("Resources", "Label_SecurityQuestion") %>:
                    </label>
                    <asp:Label ID="lblSecurityQuestion" runat="server" CssClass="security-question"></asp:Label>
                </div>

                <div class="form-group">
                    <label for="txtSecurityAnswer">
                        <%= GetGlobalResourceObject("Resources", "Label_SecurityAnswer") %>:
                    </label>
                    <asp:TextBox ID="txtSecurityAnswer" runat="server" CssClass="input-text" />
                </div>

                <div class="form-group">
                    <asp:Button ID="btnVerifyAnswer" runat="server" CssClass="btn" 
                        Text="<%$ Resources:Resources, Button_Verify %>" 
                        OnClick="btnVerifyAnswer_Click" />
                </div>
            </asp:Panel>

            <!-- Step 3: Password Reset Panel -->
            <asp:Panel ID="pnlResetPassword" runat="server" Visible="false">
                <div class="form-group">
                    <p class="success-message">
                        <asp:Literal ID="litVerificationSuccess" runat="server" Text="<%$ Resources:Resources, Security_Answer_Correct %>" />
                    </p>
                </div>

                <div class="form-group">
                    <asp:Button ID="btnSendResetEmail" runat="server" CssClass="btn" 
                        Text="<%$ Resources:Resources, Button_SendResetEmail %>" 
                        OnClick="btnSendResetEmail_Click" />
                </div>
            </asp:Panel>

            <!-- Back to Login Link -->
            <p class="alternate-link">
                <asp:HyperLink ID="lnkBackToLogin" runat="server" NavigateUrl="AM_LoginEmail.aspx" Text="<%$ Resources:Resources, Link_BackToLogin %>" />
            </p>
        </div>
    </body>
</asp:Content>