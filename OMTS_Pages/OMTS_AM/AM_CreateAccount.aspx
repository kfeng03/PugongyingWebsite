<%@ Page Title="" Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="AM_CreateAccount.aspx.cs" Inherits="fyp.AM_CreateAccount" %>

<asp:Content ID="ContentTitle" ContentPlaceHolderID="title" runat="server">Register with Email</asp:Content>
<asp:Content ID="ContentStyle" ContentPlaceHolderID="css" runat="server">
    <link rel="stylesheet" type="text/css" href="#" />
</asp:Content>
<asp:Content ID="ContentScript" ContentPlaceHolderID="script" runat="server">
    <!-- Firebase SDK -->
    <script src="https://www.gstatic.com/firebasejs/11.3.1/firebase-app.js"></script>
    <script src="https://www.gstatic.com/firebasejs/11.3.1/firebase-auth.js"></script>

    <script type="module">
        // Import Firebase functions
        import { initializeApp } from "https://www.gstatic.com/firebasejs/11.3.1/firebase-app.js";
        import { getAuth, createUserWithEmailAndPassword } from "https://www.gstatic.com/firebasejs/11.3.1/firebase-auth.js";

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

        async function generateCustomUserId() {
            const databaseUrl = "https://pgy-omts-default-rtdb.asia-southeast1.firebasedatabase.app";
            const endpoint = `${databaseUrl}/users.json`;

            try {
                // Fetch all users from the database
                const response = await fetch(endpoint);
                const users = await response.json();

                // Find the maximum custom UID
                let maxId = 0;
                if (users) {
                    Object.keys(users).forEach((key) => {
                        const userId = users[key].UserId;
                        if (userId.startsWith("U")) {
                            const numericId = parseInt(userId.substring(1), 10);
                            if (numericId > maxId) {
                                maxId = numericId;
                            }
                        }
                    });
                }

                // Generate the next custom UID
                const nextId = maxId + 1;
                const customUserId = `U${nextId.toString().padStart(3, "0")}`; // Format as U001, U002, etc.

                return customUserId;
            } catch (error) {
                console.error("Error generating custom user ID:", error);
                throw error;
            }
        }

        // Function to handle registration
        async function registerWithEmail() {
            const email = document.getElementById('<%= txtEmail.ClientID %>').value;
            const password = document.getElementById('<%= txtPassword.ClientID %>').value;

            try {
                // Create user with Firebase Authentication
                const userCredential = await createUserWithEmailAndPassword(auth, email, password);
                const user = userCredential.user;

                console.log("Firebase Authentication successful! User UID:", user.uid);

                // Generate custom user ID
                const customUserId = await generateCustomUserId();

                // Prepare user data for database
                const userData = {
                    UserId: customUserId, // Use custom UID
                    FirebaseUid: user.uid, // Store Firebase UID for mapping
                    Username: customUserId,
                    PhoneNumber: "not set",
                    Email: email,
                    Role: "Member",
                    JoinDate: new Date().toUTCString(), // Add join date in ISO format
                    MemberData: {
                        Points: 0,
                        Level: 1,
                        ApplicationID_List: {},
                        BadgeID_List: { },
                        CourseID_List: {},
                        EventID_List: {},
                        TestimonialApplicationStatus: "N/A"
                    }
                };

                // Add user data to Firebase Realtime Database
                await addUserToDatabase(customUserId, userData);

                // Show success message
                alert("Registration successful!");
                window.location.href = "AM_LoginEmail.aspx"; // Redirect to login page
            } catch (error) {
                console.error("Error during registration:", error);
                alert("Registration failed. Please try again.\n" + error);
            }
        }

        // Function to add user data to Firebase Realtime Database
        async function addUserToDatabase(customUserId, userData) {
            try {
                const databaseUrl = "https://pgy-omts-default-rtdb.asia-southeast1.firebasedatabase.app";
                const endpoint = `${databaseUrl}/users/${customUserId}.json`;

                const response = await fetch(endpoint, {
                    method: "PUT",
                    headers: {
                        "Content-Type": "application/json"
                    },
                    body: JSON.stringify(userData)
                });

                if (response.ok) {
                    console.log("User data added to Firebase Realtime Database!");
                } else {
                    console.error("Failed to add user data to Firebase Realtime Database.");
                }
            } catch (error) {
                console.error("Error adding user data to Firebase:", error);
            }
        }

        // Attach event listener to the register button
        document.getElementById('<%= btnRegister.ClientID %>').addEventListener('click', (e) => {
            e.preventDefault(); // Prevent form submission
            registerWithEmail();
        });
    </script>
</asp:Content>

<asp:Content ID="ContentMain" ContentPlaceHolderID="MainContent" runat="server">
    <body>
       
            <!-- Registration Container -->
            <div class="login-container">
                <!-- Heading -->
                <h2><asp:Literal ID="litRegisterHeading" runat="server" Text="<%$ Resources:Resources, Link_CreateAcc %>" /></h2>
                <p class="subtext">
                    <asp:Literal ID="litRegisterSubtext" runat="server" Text="<%$ Resources:Resources, Register_Info %>" />
                </p>

                <!-- Email Input -->
                <div class="form-group">
                    <asp:Label ID="litEmail" runat="server" Text="<%$ Resources:Resources, Label_Email %>">
                    </asp:Label>
                    <asp:TextBox ID="txtEmail" runat="server" CssClass="input-text" />
                </div>

                <!-- PW Input -->
                <div class="form-group">
                    <asp:Label ID="litPassword" runat="server" Text="<%$ Resources:Resources, Label_Pw %>">
                    </asp:Label>
                    <asp:TextBox ID="txtPassword" runat="server" CssClass="input-text" TextMode="Password" />
                </div>

                <!-- Register Button -->
                <div class="form-group">
                    <asp:Button ID="btnRegister" runat="server" ClientIDMode="Static" CssClass="btn" Text="<%$ Resources:Resources, Button_Register %>" />
                </div>

                <!-- Login Hyperlink -->
                <p class="alternate-link">
                    <asp:HyperLink ID="lnkLogin" runat="server" NavigateUrl="AM_LoginEmail.aspx" Text="<%$ Resources:Resources, Link_Login %>" />
                </p>
            </div>
    </body>
</asp:Content>