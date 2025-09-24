<%@ Page Language="C#" Async="true"  AutoEventWireup="true" CodeBehind="AM_LoginEmail.aspx.cs" Inherits="fyp.LoginEmail" MasterPageFile="~/Site1.Master"  %>

<asp:Content ID="ContentTitle" ContentPlaceHolderID="title" runat="server">Login with Email</asp:Content>
<asp:Content ID="ContentStyle" ContentPlaceHolderID="css" runat="server">
    <link rel="stylesheet" type="text/css" href="#" />
</asp:Content>
<asp:Content ID="ContentScript" ContentPlaceHolderID="script" runat="server">
    <%--<!-- Firebase SDK -->
    <script src="https://www.gstatic.com/firebasejs/11.3.1/firebase-app.js"></script>
    <script src="https://www.gstatic.com/firebasejs/11.3.1/firebase-auth.js"></script>

    <script type="module">
        // Import Firebase functions
        import { initializeApp } from "https://www.gstatic.com/firebasejs/11.3.1/firebase-app.js";
        import { getAuth, signInWithEmailAndPassword } from "https://www.gstatic.com/firebasejs/11.3.1/firebase-auth.js";

        // Your Firebase configuration
        const firebaseConfig = {
            apiKey: "AIzaSyAqpGAELltYzFuKiCjl4o71VszV6Px7J6w",
            authDomain: "fyp-omts.firebaseapp.com",
            databaseURL: "https://fyp-omts-default-rtdb.asia-southeast1.firebasedatabase.app",
            projectId: "fyp-omts",
            storageBucket: "fyp-omts.firebasestorage.app",
            messagingSenderId: "626937341844",
            appId: "1:626937341844:web:47af08577caea46f569181"
        };

        // Initialize Firebase
        const app = initializeApp(firebaseConfig);
        const auth = getAuth(app);

        // Function to handle login
        function loginWithEmail() {
            const email = document.getElementById('<%= txtEmail.ClientID %>').value;
            const password = document.getElementById('<%= txtPw.ClientID %>').value;

            signInWithEmailAndPassword(auth, email, password)
                .then((userCredential) => {
                    // Login successful
                    const user = userCredential.user;
                    console.log("Login successful!", user);
                    alert("Login successful!");
                    window.location.href = "AM_UpdatePersonalInfo.aspx"; // Redirect to dashboard
                })
                .catch((error) => {
                    // Handle errors
                    console.error("Error logging in:", error);
                    alert("Login failed. Please check your email and password.");
                });
        }

        // Attach event listener to the login button
        document.getElementById('<%= btnLogin.ClientID %>').addEventListener('click', (e) => {
            e.preventDefault(); // Prevent form submission
            loginWithEmail();
        });
    </script>--%>
</asp:Content>
<asp:Content ID="ContentMain" ContentPlaceHolderID="MainContent" runat="server">
        
  
        <!-- Login Container -->
        <div class="login-container">
            <!-- Heading -->
            <h2><asp:Literal ID="litLoginHeading" runat="server" Text="<%$ Resources:Resources, Login2_Heading %>" /></h2>
            <p class="subtext">
                <asp:Literal ID="litLoginSubtext" runat="server" Text="<%$ Resources:Resources, Login2_Info %>"   />
            </p>

            <!-- Email Input -->
            <div class="form-group">
                <asp:Label ID="litEmail" runat="server" Text="<%$ Resources:Resources, Label_Email %>">
                </asp:Label>
                <asp:TextBox ID="txtEmail" runat="server" CssClass="input-text" />
            </div>

            <!-- PW Input -->
            <div class="form-group">
                <asp:Label ID="litPw" runat="server" Text="<%$ Resources:Resources, Label_Pw %>">
                </asp:Label>
                <asp:TextBox ID="txtPw" runat="server" CssClass="input-text" TextMode="Password" />
            </div>

            <!-- Login Button -->
            <div class="form-group">
                <asp:Button ID="btnLogin" runat="server" ClientIDMode="Static" CssClass="btn" Text="<%$ Resources:Resources, Button_Login %>" OnClick="btnLogin_Click" />
            </div>

            <!-- PW reset Hyperlink using email-->
            <p class="alternate-link">
                <asp:HyperLink ID="lnkCreate" runat="server" ClientIDMode="Static" Text="<%$ Resources:Resources, Link_CreateAcc %>"  NavigateUrl="AM_CreateAccount.aspx"  />
            </p>
            <p  class="alternate-link">
                <asp:HyperLink ID="lnkPWreset" runat="server" Text="<%$ Resources:Resources, Link_ForgotPassword %>" NavigateUrl="AM_ForgotPassword.aspx"  />
                </p>
            <p  class="alternate-link">
                <asp:HyperLink ID="lnkLoginGoogle" runat="server" Text="Login with Google" NavigateUrl="AM_LoginGoogle.aspx"  />
            </p>
        </div>
    </asp:Content>
