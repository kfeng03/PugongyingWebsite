<%@ Page Title="" Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="AM_LoginGoogle.aspx.cs" Inherits="fyp.AM_LoginGoogle" %>

<asp:Content ID="ContentTitle" ContentPlaceHolderID="title" runat="server">Login with Google</asp:Content>
<asp:Content ID="ContentStyle" ContentPlaceHolderID="css" runat="server">
    <style>
        .login-container {
            max-width: 400px;
            margin: 50px auto;
            padding: 30px;
            border: 1px solid #ddd;
            border-radius: 8px;
            box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
            background-color: #fff;
        }

        .google-signin-container {
            text-align: center;
            margin: 30px 0;
        }

        .google-signin-btn {
            display: inline-flex;
            align-items: center;
            justify-content: center;
            background-color: #4285f4;
            color: white;
            border: none;
            border-radius: 4px;
            padding: 12px 24px;
            font-size: 16px;
            font-weight: 500;
            cursor: pointer;
            transition: background-color 0.3s;
            text-decoration: none;
            min-width: 240px;
        }

        .google-signin-btn:hover {
            background-color: #357ae8;
        }

        .google-signin-btn:disabled {
            background-color: #ccc;
            cursor: not-allowed;
        }

        .google-icon {
            width: 20px;
            height: 20px;
            margin-right: 12px;
            background-color: white;
            border-radius: 2px;
            display: flex;
            align-items: center;
            justify-content: center;
        }

        .loading-spinner {
            display: none;
            margin: 20px auto;
            width: 40px;
            height: 40px;
            border: 4px solid #f3f3f3;
            border-top: 4px solid #4285f4;
            border-radius: 50%;
            animation: spin 1s linear infinite;
        }

        @keyframes spin {
            0% { transform: rotate(0deg); }
            100% { transform: rotate(360deg); }
        }

        .result-message {
            margin-top: 15px;
            padding: 10px;
            border-radius: 4px;
            text-align: center;
            display: none;
        }

        .success-message {
            background-color: #d4edda;
            color: #155724;
            border: 1px solid #c3e6cb;
        }

        .error-message {
            background-color: #f8d7da;
            color: #721c24;
            border: 1px solid #f5c6cb;
        }

        .divider {
            text-align: center;
            margin: 30px 0 20px 0;
            position: relative;
        }

        .divider::before {
            content: '';
            position: absolute;
            top: 50%;
            left: 0;
            right: 0;
            height: 1px;
            background-color: #ddd;
        }

        .divider span {
            background-color: white;
            padding: 0 15px;
            color: #666;
            font-size: 14px;
        }

        .alternate-link {
            text-align: center;
            margin-top: 20px;
        }

        .alternate-link a {
            color: #4285f4;
            text-decoration: none;
        }

        .alternate-link a:hover {
            text-decoration: underline;
        }

        .privacy-notice {
            margin-top: 20px;
            font-size: 12px;
            color: #666;
            text-align: center;
            line-height: 1.4;
        }
    </style>
</asp:Content>

<asp:Content ID="ContentScript" ContentPlaceHolderID="script" runat="server">
    <!-- jQuery for AJAX -->
    <script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>
    
    <!-- Firebase SDK -->
    <script src="https://www.gstatic.com/firebasejs/8.10.0/firebase-app.js"></script>
    <script src="https://www.gstatic.com/firebasejs/8.10.0/firebase-auth.js"></script>
    <script src="https://www.gstatic.com/firebasejs/8.10.0/firebase-database.js"></script>

    <script type="text/javascript">
        // Firebase configuration
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
        firebase.initializeApp(firebaseConfig);

        // Create references
        const auth = firebase.auth();
        const database = firebase.database();

        // Google Auth Provider
        const googleProvider = new firebase.auth.GoogleAuthProvider();
        googleProvider.addScope('email');
        googleProvider.addScope('profile');

        // Function to generate custom user ID
        async function generateCustomUserId() {
            try {
                // Query the database to get all users
                const usersSnapshot = await database.ref('users').once('value');
                const users = usersSnapshot.val() || {};

                // Find the maximum custom UID
                let maxId = 0;
                Object.keys(users).forEach((key) => {
                    const userId = key;
                    if (userId.startsWith("U")) {
                        const numericId = parseInt(userId.substring(1), 10);
                        if (!isNaN(numericId) && numericId > maxId) {
                            maxId = numericId;
                        }
                    }
                });

                // Generate the next custom UID
                const nextId = maxId + 1;
                const customUserId = `U${nextId.toString().padStart(3, "0")}`; // Format as U001, U002, etc.
                return customUserId;
            } catch (error) {
                console.error("Error generating custom user ID:", error);
                throw error;
            }
        }

        // Function to check if user exists in the database
        async function checkUserExists(firebaseUid) {
            try {
                // Query the database to find if the user with given Firebase UID exists
                const usersSnapshot = await database.ref('users').once('value');
                const users = usersSnapshot.val() || {};

                // Check each user in the database
                for (const userId in users) {
                    if (users[userId].FirebaseUid === firebaseUid) {
                        return { exists: true, userId: userId, userData: users[userId] };
                    }
                }

                // If no user is found with the given Firebase UID
                return { exists: false, userId: null, userData: null };
            } catch (error) {
                console.error("Error checking if user exists:", error);
                throw error;
            }
        }

        // Function to create a new user in the database
        async function createUserInDatabase(firebaseUid, email, displayName, photoURL) {
            try {
                // Generate a new custom user ID
                const customUserId = await generateCustomUserId();

                // Prepare user data
                const userData = {
                    UserId: customUserId,
                    FirebaseUid: firebaseUid,
                    Username: displayName || customUserId, // Use display name if available, otherwise use user ID
                    PhoneNumber: "", // Empty for Google authentication
                    Email: email,
                    ProfilePicture: photoURL || "", // Store Google profile picture URL
                    Role: "Member",
                    JoinDate: new Date().toUTCString(),
                    MemberData: {
                        Points: 0,
                        Level: 1,
                        ApplicationID_List: [],
                        BadgeID_List: [],
                        CourseID_List: [],
                        EventID_List: [],
                        Interest_List: [],
                        TestimonialApplicationStatus: "N/A"
                    }
                };

                // Add user to database
                await database.ref('users/' + customUserId).set(userData);

                return { success: true, userId: customUserId, userData: userData };
            } catch (error) {
                console.error("Error creating user in database:", error);
                throw error;
            }
        }

        // Function to save user session and redirect
        function saveSessionAndRedirect(firebaseUid, userId, username, role) {
            // Call the server-side method to set session variables
            $.ajax({
                type: "POST",
                url: "AM_LoginGoogle.aspx/CompleteLogin",
                data: JSON.stringify({
                    firebaseUid: firebaseUid,
                    userId: userId,
                    username: username,
                    role: role
                }),
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: function (response) {
                    let result = JSON.parse(response.d);
                    if (result.success) {
                        showMessage("Login successful! Redirecting...", 'success');
                        setTimeout(() => {
                            window.location.href = "AM_UpdatePersonalInfo.aspx";
                        }, 1500);
                    } else {
                        showMessage("Error: " + result.error, 'error');
                        hideLoading();
                    }
                },
                error: function (xhr, status, error) {
                    showMessage("Error: " + error, 'error');
                    hideLoading();
                    console.error(xhr.responseText);
                }
            });
        }

        // Function to sign in with Google
        async function signInWithGoogle() {
            try {
                showLoading();
                hideMessage();

                // Sign in with Google using popup
                const result = await auth.signInWithPopup(googleProvider);
                
                // Get user information
                const user = result.user;
                const firebaseUid = user.uid;
                const email = user.email;
                const displayName = user.displayName;
                const photoURL = user.photoURL;

                console.log("Google sign-in successful:", user);

                // Check if user already exists in the database
                const { exists, userId, userData } = await checkUserExists(firebaseUid);

                if (!exists) {
                    // Create new user in database
                    showMessage("Creating your account...", 'success');
                    const { success, userId: newUserId, userData: newUserData } = await createUserInDatabase(firebaseUid, email, displayName, photoURL);

                    if (success) {
                        showMessage("Account created successfully!", 'success');
                        // Save session and redirect
                        saveSessionAndRedirect(firebaseUid, newUserId, displayName || newUserId, "Member");
                    } else {
                        showMessage("Failed to create user account", 'error');
                        hideLoading();
                    }
                } else {
                    showMessage("Welcome back!", 'success');
                    // Get user role and username
                    const role = userData.Role || "Member";
                    const username = userData.Username || userId;

                    // Save session and redirect
                    saveSessionAndRedirect(firebaseUid, userId, username, role);
                }
            } catch (error) {
                console.error("Google sign-in error:", error);
                
                // Handle specific error cases
                if (error.code === 'auth/popup-closed-by-user') {
                    showMessage("Sign-in was cancelled", 'error');
                } else if (error.code === 'auth/popup-blocked') {
                    showMessage("Popup was blocked by browser. Please allow popups and try again.", 'error');
                } else if (error.code === 'auth/cancelled-popup-request') {
                    showMessage("Sign-in was cancelled", 'error');
                } else {
                    showMessage("Error signing in: " + error.message, 'error');
                }
                
                hideLoading();
            }
        }

        // Helper functions to show/hide loading and messages
        function showLoading() {
            document.getElementById('loadingSpinner').style.display = 'block';
            document.getElementById('btnGoogleSignIn').disabled = true;
        }

        function hideLoading() {
            document.getElementById('loadingSpinner').style.display = 'none';
            document.getElementById('btnGoogleSignIn').disabled = false;
        }

        function showMessage(message, type) {
            const messageElement = document.getElementById('resultMessage');
            messageElement.innerHTML = message;
            messageElement.className = `result-message ${type}-message`;
            messageElement.style.display = 'block';
        }

        function hideMessage() {
            const messageElement = document.getElementById('resultMessage');
            messageElement.style.display = 'none';
        }

        // Initialize authentication state listener
        document.addEventListener('DOMContentLoaded', function () {
            // Listen for authentication state changes
            auth.onAuthStateChanged(function(user) {
                if (user) {
                    console.log("User is signed in:", user);
                } else {
                    console.log("User is signed out");
                }
            });
        });
    </script>
</asp:Content>

<asp:Content ID="ContentMain" ContentPlaceHolderID="MainContent" runat="server">
    <div class="login-container">
        <!-- Heading -->
        <h2><asp:Literal ID="litLoginHeading" runat="server" Text="Login with Google" /></h2>
        <p class="subtext">
            <asp:Literal ID="litLoginSubtext" runat="server" Text="Sign in quickly and securely with your Google account" />
        </p>

        <!-- Google Sign-In Container -->
        <div class="google-signin-container">
            <button type="button" id="btnGoogleSignIn" onclick="signInWithGoogle()" class="google-signin-btn">
                <div class="google-icon">
                    <svg width="18" height="18" viewBox="0 0 18 18">
                        <path fill="#4285F4" d="M16.51 8H8.98v3h4.3c-.18 1-.74 1.48-1.6 2.04v2.01h2.6a7.8 7.8 0 0 0 2.38-5.88c0-.57-.05-.66-.15-1.18z"/>
                        <path fill="#34A853" d="M8.98 17c2.16 0 3.97-.72 5.3-1.94l-2.6-2.04a4.8 4.8 0 0 1-2.7.75 4.8 4.8 0 0 1-4.52-3.36H1.83v2.07A8 8 0 0 0 8.98 17z"/>
                        <path fill="#FBBC05" d="M4.46 10.41A4.8 4.8 0 0 1 4.21 9a4.8 4.8 0 0 1 .25-1.41V5.52H1.83a8 8 0 0 0 0 6.96l2.63-2.07z"/>
                        <path fill="#EA4335" d="M8.98 4.18c1.17 0 2.23.4 3.06 1.2l2.3-2.3A8 8 0 0 0 8.98 1 8 8 0 0 0 1.83 5.52L4.46 7.6A4.8 4.8 0 0 1 8.98 4.18z"/>
                    </svg>
                </div>
                Sign in with Google
            </button>
        </div>

        <!-- Loading Spinner -->
        <div id="loadingSpinner" class="loading-spinner"></div>

        <!-- Result Message Area -->
        <div id="resultMessage" class="result-message"></div>

        <!-- Divider -->
        
            <span>or</span>
        

        <!-- Alternative Login Links -->
        <div class="alternate-link">
            <asp:HyperLink ID="lnkLoginEmail" runat="server" NavigateUrl="AM_LoginEmail.aspx" Text="Login with Email" />
        </div>

        <!-- Privacy Notice -->
        <div class="privacy-notice">
            By signing in with Google, you agree to our Terms of Service and Privacy Policy.
            Your Google profile information will be used to create your account.
        </div>
    </div>
</asp:Content>