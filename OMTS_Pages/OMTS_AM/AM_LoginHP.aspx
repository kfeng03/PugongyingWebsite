<%@ Page Title="" Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="AM_LoginHP.aspx.cs" Inherits="fyp.AM_LoginHP" %>

<asp:Content ID="ContentTitle" ContentPlaceHolderID="title" runat="server">Login with Phone</asp:Content>
<asp:Content ID="ContentStyle" ContentPlaceHolderID="css" runat="server">
    <link rel="stylesheet" type="text/css" href="#" />

    <style>
        #phoneContainer {
            display: block;
        }
        #otpContainer {
            display: none;
        }
        .phone-input {
            width: 100%;
            padding: 10px;
            margin-bottom: 15px;
            border: 1px solid #ddd;
            border-radius: 4px;
        }
        .result-message {
            margin-top: 10px;
            color: green;
        }
        .error-message {
            margin-top: 10px;
            color: red;
        }
        .otp-container {
            text-align: center;
        }

        .otp-input-group {
            display: flex;
            justify-content: center;
            gap: 10px;
            margin: 15px 0;
        }

        .otp-input {
            width: 40px;
            height: 50px;
            text-align: center;
            font-size: 24px;
            border: 1px solid #ddd;
            border-radius: 4px;
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

        // Window onload handler
        window.onload = function () {
            // Initialize the recaptcha verifier
            window.recaptchaVerifier = new firebase.auth.RecaptchaVerifier('recaptcha-container', {
                'size': 'normal',
                'callback': (response) => {
                    // reCAPTCHA solved, enable sign-in button
                    document.getElementById('btnSendCode').disabled = false;
                },
                'expired-callback': () => {
                    // Reset reCAPTCHA widget
                    document.getElementById('btnSendCode').disabled = true;
                    window.recaptchaVerifier.reset();
                }
            });
            window.recaptchaVerifier.render();
        };

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
        async function createUserInDatabase(firebaseUid, phoneNumber) {
            try {
                // Generate a new custom user ID
                const customUserId = await generateCustomUserId();

                // Prepare user data
                const userData = {
                    UserId: customUserId,
                    FirebaseUid: firebaseUid,
                    Username: customUserId, // Default username is the user ID
                    PhoneNumber: phoneNumber,
                    Email: "", // Empty for phone authentication
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
                url: "AM_LoginHP.aspx/CompleteLogin",
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
                        window.location.href = "AM_UpdatePersonalInfo.aspx";
                    } else {
                        showError("Error: " + result.error);
                    }
                },
                error: function (xhr, status, error) {
                    showError("Error: " + error);
                    console.error(xhr.responseText);
                }
            });
        }

        // Function to send OTP
        function sendOTP() {
            const phoneNumber = document.getElementById('txtPhoneNumber').value;

            if (!phoneNumber) {
                showError("Please enter a valid phone number");
                return;
            }

            // Format the phone number (ensure it has a country code)
            const formattedPhoneNumber = formatPhoneNumber(phoneNumber);

            const appVerifier = window.recaptchaVerifier;
            firebase.auth().signInWithPhoneNumber(formattedPhoneNumber, appVerifier)
                .then((confirmationResult) => {
                    // SMS sent. Prompt user to enter the code.
                    window.confirmationResult = confirmationResult;
                    document.getElementById('phoneContainer').style.display = 'none';
                    document.getElementById('otpContainer').style.display = 'block';
                    showMessage("OTP sent successfully!");
                })
                .catch((error) => {
                    // Error; SMS not sent
                    showError("Error sending OTP: " + error.message);
                    console.error(error);
                    // Reset reCAPTCHA
                    window.recaptchaVerifier.reset();
                });
        }

        // Function to verify OTP and sign in
        async function verifyOTP() {
            const otp = document.getElementById('txtOTP').value;

            if (!otp) {
                showError("Please enter the OTP sent to your phone");
                return;
            }

            try {
                // Verify the OTP
                const result = await window.confirmationResult.confirm(otp);

                // User signed in successfully
                const user = result.user;
                const firebaseUid = user.uid;
                const phoneNumber = user.phoneNumber;

                // Check if user already exists in the database
                const { exists, userId, userData } = await checkUserExists(firebaseUid);

                if (!exists) {
                    // Create new user in database
                    const { success, userId: newUserId, userData: newUserData } = await createUserInDatabase(firebaseUid, phoneNumber);

                    if (success) {
                        showMessage("Account created successfully!");
                        // Save session and redirect
                        saveSessionAndRedirect(firebaseUid, newUserId, newUserId, "Member");
                    } else {
                        showError("Failed to create user account");
                    }
                } else {
                    showMessage("Login successful!");
                    // Get user role and username
                    const role = userData.Role || "Member";
                    const username = userData.Username || userId;

                    // Save session and redirect
                    saveSessionAndRedirect(firebaseUid, userId, username, role);
                }
            } catch (error) {
                showError("Error verifying OTP: " + error.message);
                console.error(error);
            }
        }

        // Helper function to format phone number
        function formatPhoneNumber(phoneNumber) {
            // If phone number doesn't start with '+', add '+60' (Malaysia country code)
            if (!phoneNumber.startsWith('+')) {
                // Remove leading '0' if present
                if (phoneNumber.startsWith('0')) {
                    phoneNumber = phoneNumber.substring(1);
                }
                // Add country code
                phoneNumber = '+60' + phoneNumber;
            }
            return phoneNumber;
        }

        // Helper functions to show messages
        function showMessage(message) {
            const messageElement = document.getElementById('resultMessage');
            messageElement.innerHTML = message;
            messageElement.className = 'result-message';
        }

        function showError(message) {
            const messageElement = document.getElementById('resultMessage');
            messageElement.innerHTML = message;
            messageElement.className = 'error-message';
        }

        // Replace the entire OTP-related event listener block with this
        document.addEventListener('DOMContentLoaded', function () {
            const otpInputs = document.querySelectorAll('.otp-input');

            otpInputs.forEach((input, index) => {
                input.addEventListener('input', function () {
                    // Ensure only one digit
                    this.value = this.value.replace(/[^0-9]/g, '').slice(0, 1);

                    // Automatically move to next input
                    if (this.value.length === 1 && index < otpInputs.length - 1) {
                        otpInputs[index + 1].focus();
                    }

                    // Collect OTP and update hidden input
                    const otp = Array.from(otpInputs).map(input => input.value).join('');
                    document.getElementById('txtOTP').value = otp;
                });

                input.addEventListener('keydown', function (e) {
                    // Allow backspace to move to previous input
                    if (e.key === 'Backspace' && this.value.length === 0 && index > 0) {
                        otpInputs[index - 1].focus();
                    }
                });
            });
        });
    </script>
</asp:Content>

<asp:Content ID="ContentMain" ContentPlaceHolderID="MainContent" runat="server">
    <div class="login-container">
        <h2>Phone Login Disabled</h2>
        <p>Login with phone number is no longer supported. Please <a href="AM_LoginGoogle.aspx">login with Google</a> instead.</p>
    </div>
</asp:Content>