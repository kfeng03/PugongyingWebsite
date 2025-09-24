<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="EM_CompletedEvent.aspx.cs" 
Inherits="fyp.EM_CompletedEvent" MasterPageFile="~/Site1.Master" Async="true" %>

<asp:Content ID="ContentTitle" ContentPlaceHolderID="title" runat="server">Completed Events</asp:Content>

<asp:Content ID="ContentStyle" ContentPlaceHolderID="css" runat="server">
    <style>
        .event-list {
            display: flex;
            flex-wrap: wrap;
            gap: 20px;
            margin-top: 20px;
        }

        .event-item {
            background-color: #ffffff;
            border-radius: 8px;
            box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
            padding: 20px;
            width: 97%;
            display: flex;
            justify-content: space-between;
            align-items: center;
            transition: transform 0.2s ease, box-shadow 0.2s ease;
            border-left: 4px solid #007bff;
        }

        .event-item:hover {
            transform: translateY(-5px);
            box-shadow: 0 8px 16px rgba(0, 0, 0, 0.1);
        }

        .event-image {
            width: 100%;
            height: 160px;
            object-fit: cover;
        }

        .event-content {
            padding: 20px;
            flex-grow: 1;
            display: flex;
            flex-direction: column;
        }

        .event-title {
            font-size: 18px;
            font-weight: 600;
            color: #333;
            margin-bottom: 10px;
        }

        .event-details {
            display: flex;
            flex-direction: column;
            gap: 8px;
            margin-bottom: 15px;
        }

        .event-detail {
            font-size: 14px;
            color: #666;
        }

        .btn {
            flex: 1;
            padding: 10px;
            border: none;
            border-radius: 6px;
            cursor: pointer;
            font-weight: 500;
            font-size: 14px;
            text-align: center;
            transition: background-color 0.3s ease;
        }

        .btn-primary {
            background-color: #007bff;
            color: white;
        }

        .btn-primary:hover {
            background-color: #0069d9;
        }

        .btn-success {
            background-color: #28a745;
            color: white;
        }

        .btn-success:hover {
            background-color: #218838;
        }

        .empty-message {
            width: 100%;
            padding: 30px;
            text-align: center;
            background-color: #f8f9fa;
            border-radius: 8px;
            font-size: 16px;
            color: #6c757d;
            margin-top: 20px;
        }

        .feedback-modal {
            position: fixed;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            background-color: rgba(0, 0, 0, 0.5);
            display: flex;
            justify-content: center;
            align-items: center;
            z-index: 1000;
        }

        .feedback-container {
            background-color: white;
            padding: 30px;
            border-radius: 10px;
            width: 500px;
            max-width: 90%;
        }

        .feedback-title {
            font-size: 20px;
            font-weight: bold;
            margin-bottom: 20px;
            text-align: center;
        }

        .feedback-textarea {
            width: 100%;
            min-height: 150px;
            margin-bottom: 20px;
            padding: 10px;
            border: 1px solid #ddd;
            border-radius: 5px;
            resize: vertical;
        }

        .feedback-buttons {
            display: flex;
            justify-content: space-between;
        }

        .status-completed {
            background-color: #28a745;
            color: white;
            padding: 5px 10px;
            border-radius: 4px;
            font-size: 12px;
            margin-bottom: 10px;
        }

        /* Certificate Modal Styles */
        .certificate-container {
            position: fixed;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            background-color: rgba(0, 0, 0, 0.7);
            display: none; /* Hidden by default */
            justify-content: center;
            align-items: center;
            z-index: 1000;
        }

        .certificate {
            width: 90%;
            max-width: 800px;
            background-color: #fff;
            border: 15px solid #366092;
            border-radius: 8px;
            padding: 40px;
            text-align: center;
            position: relative;
        }

        .certificate-title {
            font-size: 28px;
            font-weight: bold;
            color: #366092;
            margin-bottom: 20px;
        }

        .certificate-content {
            margin-bottom: 30px;
            line-height: 1.6;
            font-size: 18px;
        }

        .certificate-close {
            position: absolute;
            top: 10px;
            right: 15px;
            font-size: 24px;
            cursor: pointer;
            color: #555;
        }

        .certificate-print {
            padding: 10px 20px;
            background-color: #007bff;
            color: white;
            border: none;
            border-radius: 4px;
            cursor: pointer;
            margin-top: 20px;
        }

        /* Styling for the action buttons container */
        .action-buttons {
            display: flex;
            gap: 10px;
            margin-top: 15px;
        }

        @media print {
            .sidebar, .nav-bar, .action-buttons, .certificate-close, .certificate-print, .user-section {
                display: none !important;
            }
            
            body, html {
                margin: 0;
                padding: 0;
                background-color: white;
            }
            
            .certificate {
                width: 100%;
                height: 100%;
                border: 15px solid #366092 !important;
                box-sizing: border-box;
                -webkit-print-color-adjust: exact;
                print-color-adjust: exact;
            }
            
            .main-content {
                width: 100% !important;
                padding: 0 !important;
                margin: 0 !important;
            }
            
            .container {
                display: block !important;
                width: 100% !important;
                height: 100% !important;
                padding: 0 !important;
                margin: 0 !important;
            }
        }
    </style>
</asp:Content>

<asp:Content ID="ContentMain" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container">
        <!-- Sidebar -->
        <div class="sidebar">
            <asp:Button ID="btnMyEvents" runat="server" CssClass='<%# (Request.Url.AbsolutePath.EndsWith("EM_MyEvents.aspx")) ? "sidebar-btn active" : "sidebar-btn" %>' 
                Text="My Events" PostBackUrl="~/OMTS_Pages/OMTS_EM/EM_MyEvents.aspx" />
            <asp:Button ID="btnJoinEvent" runat="server" CssClass='<%# (Request.Url.AbsolutePath.EndsWith("EM_JoinEvent.aspx")) ? "sidebar-btn active" : "sidebar-btn" %>' 
                Text="Join Event" PostBackUrl="~/OMTS_Pages/OMTS_EM/EM_JoinEvent.aspx" />
            <asp:Button ID="btnCompletedEvent" runat="server" CssClass='<%# (Request.Url.AbsolutePath.EndsWith("EM_CompletedEvent.aspx")) ? "sidebar-btn active" : "sidebar-btn" %>' 
                Text="Completed Events" PostBackUrl="~/OMTS_Pages/OMTS_EM/EM_CompletedEvent.aspx" />
            <asp:Button ID="btnEventManagement" runat="server" CssClass='<%# (Request.Url.AbsolutePath.EndsWith("EM_EventManagement.aspx")) ? "sidebar-btn active" : "sidebar-btn" %>' 
                Text="Event Management" PostBackUrl="~/OMTS_Pages/OMTS_EM/EM_EventManagement.aspx" />
            <asp:Button ID="btnEventFeedbacks" runat="server" CssClass='<%# (Request.Url.AbsolutePath.EndsWith("EM_EventFeedbacks.aspx")) ? "sidebar-btn active" : "sidebar-btn" %>' 
                Text="Event Feedbacks" PostBackUrl="~/OMTS_Pages/OMTS_EM/EM_EventFeedbacks.aspx" />
        </div>

        <!-- Main Content Area -->
        <div class="main-content">
            <h2 class="page-header">Completed Events</h2>

            <!-- Error/Success Message -->
            <asp:Label ID="lblMessage" runat="server" CssClass="message"></asp:Label>

            <!-- Completed Event List -->
            <div class="event-list">
                <asp:Label ID="lblNoEvents" runat="server" 
                    Text="You haven't completed any events yet." 
                    CssClass="empty-message" Visible="false"></asp:Label>

                <asp:Repeater ID="rptCompletedEvents" runat="server">
                    <ItemTemplate>
                        <div class="event-item">
                            <div class="event-content">
                                <div class="event-title">
                                    <%# Eval("EventTitle") %>
                                </div>
                                <div class="status-completed">Completed</div>
                                <div class="event-details">
                                    <div class="event-detail">
                                        <strong>Date:</strong> <%# Eval("EventStartDate") %>
                                    </div>
                                    <div class="event-detail">
                                        <strong>Location:</strong> <%# Eval("EventLocation") %>
                                    </div>
                                </div>
                                <div class="action-buttons">
                                    <asp:Button ID="btnViewDetails" runat="server" 
                                        Text="View Details" 
                                        CssClass="btn btn-primary"
                                        CommandArgument='<%# Eval("EventID") %>' 
                                        OnClick="btnViewDetails_Click" />
                                    
                                    <asp:Button ID="btnGiveFeedback" runat="server" 
                                        Text="Give Feedback" 
                                        CssClass="btn btn-success"
                                        CommandArgument='<%# Eval("EventID") %>' 
                                        OnClick="btnGiveFeedback_Click" />

                                    <asp:Button ID="btnViewCertificate" runat="server" 
                                        Text="View Certificate" 
                                        CssClass="btn btn-success"
                                        CommandArgument='<%# Eval("EventID") %>' 
                                        OnClick="btnViewCertificate_Click" />
                                </div>
                            </div>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
            </div>

            <!-- Feedback Modal -->
            <asp:Panel ID="pnlFeedbackModal" runat="server" CssClass="feedback-modal" Visible="false">
                <div class="feedback-container">
                    <div class="feedback-title">Event Feedback</div>
                    <asp:TextBox ID="txtFeedback" runat="server" CssClass="feedback-textarea" 
                        TextMode="MultiLine" placeholder="Please share your feedback about the event..."></asp:TextBox>
                    
                    <div class="feedback-buttons">
                        <asp:Button ID="btnSubmitFeedback" runat="server" Text="Submit Feedback" 
                            CssClass="btn btn-success" OnClick="btnSubmitFeedback_Click" />
                        <asp:Button ID="btnCancelFeedback" runat="server" Text="Cancel" 
                            CssClass="btn btn-primary" OnClick="btnCancelFeedback_Click" />
                    </div>
                </div>
            </asp:Panel>

            <!-- Certificate Modal -->
            <asp:Panel ID="pnlCertificate" runat="server" CssClass="certificate-container" Visible="false">
                <div class="certificate">
                    <span class="certificate-close" onclick="closeCertificate()">×</span>
                    <div class="certificate-title">Certificate of Participation</div>
                    <div class="certificate-content">
                        This is to certify that
                        <br />
                        <strong><asp:Literal ID="litUserName" runat="server"></asp:Literal></strong>
                        <br />
                        has successfully participated in the event
                        <br />
                        <strong><asp:Literal ID="litEventName" runat="server"></asp:Literal></strong>
                        <br />
                        on
                        <br />
                        <asp:Literal ID="litEventDate" runat="server"></asp:Literal>
                    </div>
                    <button class="certificate-print" onclick="printCertificate()">
                        Print Certificate
                    </button>
                </div>
            </asp:Panel>
        </div>
    </div>

    <script>
        function closeCertificate() {
            document.querySelector('.certificate-container').style.display = 'none';
        }

        function printCertificate() {
            window.print();
        }
    </script>
</asp:Content>