<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="EM_GenerateCertificate.aspx.cs" 
Inherits="fyp.EM_GenerateCertificate" MasterPageFile="~/Site1.Master" Async="true" %>

<asp:Content ID="ContentTitle" ContentPlaceHolderID="title" runat="server">Event Certificate</asp:Content>

<asp:Content ID="ContentStyle" ContentPlaceHolderID="css" runat="server">
    <style>
        .certificate-container {
            width: 95%;
            margin: 20px auto;
            padding: 20px;
            border: 15px solid #366092;
            position: relative;
        }

        .certificate-header {
            text-align: center;
            margin-bottom: 40px;
        }

        .certificate-title {
            font-size: 36px;
            font-weight: bold;
            color: #366092;
            margin-bottom: 10px;
            font-family: 'Times New Roman', Times, serif;
        }

        .certificate-subtitle {
            font-size: 24px;
            color: #366092;
            margin-bottom: 30px;
            font-family: 'Times New Roman', Times, serif;
        }

        .certificate-body {
            text-align: center;
            margin-bottom: 40px;
            font-size: 18px;
            line-height: 1.5;
        }

        .participant-name {
            font-size: 28px;
            font-weight: bold;
            color: #366092;
            margin: 20px 0;
            font-family: 'Times New Roman', Times, serif;
        }

        .certificate-text {
            font-size: 18px;
            margin-bottom: 30px;
        }

        .event-name {
            font-size: 22px;
            font-weight: bold;
            margin: 10px 0;
        }

        .certificate-footer {
            display: flex;
            justify-content: space-between;
            margin-top: 60px;
        }

        .signature-section {
            text-align: center;
            width: 45%;
        }

        .signature-line {
            border-top: 1px solid #366092;
            margin: 0 auto;
            width: 80%;
        }

        .signature-name {
            margin-top: 10px;
            font-weight: bold;
        }

        .certificate-date {
            text-align: right;
            margin-top: 30px;
            font-style: italic;
        }
        
        .certificate-seal {
            position: absolute;
            bottom: 30px;
            right: 30px;
            width: 120px;
            height: 120px;
            opacity: 0.3;
        }
        
        .action-buttons {
            display: flex;
            gap: 10px;
            margin-top: 20px;
            justify-content: center;
        }

        .print-btn, .back-btn {
            padding: 10px 20px;
            font-size: 16px;
            border-radius: 5px;
            border: none;
            cursor: pointer;
            transition: background-color 0.3s ease;
        }

        .print-btn {
            background-color: #4CAF50;
            color: white;
        }

        .print-btn:hover {
            background-color: #3e8e41;
        }

        .back-btn {
            background-color: #2196F3;
            color: white;
        }

        .back-btn:hover {
            background-color: #0b7dda;
        }

        @media print {
            .sidebar, .nav-bar, .action-buttons, .user-section {
                display: none !important;
            }
            
            body, html {
                margin: 0;
                padding: 0;
                background-color: white;
            }
            
            .certificate-container {
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

<asp:Content ID="ContentScript" ContentPlaceHolderID="script" runat="server">
    <script type="text/javascript">
        function printCertificate() {
            window.print();
        }
    </script>
</asp:Content>

<asp:Content ID="ContentMain" ContentPlaceHolderID="MainContent" runat="server">
    <body>
        <!-- Main Content Layout -->
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
                    Text="Event Management" PostBackUrl="~/OMTS_Pages/OMTS_EM/EM_EventManagement.aspx" Visible="false" />
            </div>

            <!-- Main Content Area -->
            <div class="main-content">
                <!-- Certificate Display -->
                <div class="certificate-container">
                    <div class="certificate-header">
                        <div class="certificate-title">Certificate of Participation</div>
                        <div class="certificate-subtitle">This certifies that</div>
                    </div>
                    
                    <div class="certificate-body">
                        <div class="participant-name">
                            <asp:Literal ID="litParticipantName" runat="server"></asp:Literal>
                        </div>
                        
                        <div class="certificate-text">
                            has successfully participated in the event
                        </div>
                        
                        <div class="event-name">
                            <asp:Literal ID="litEventName" runat="server"></asp:Literal>
                        </div>
                        
                        <div class="certificate-text">
                            <asp:Literal ID="litEventDetails" runat="server"></asp:Literal>
                        </div>
                    </div>
                    
                    <div class="certificate-footer">
                        <div class="signature-section">
                            <div class="signature-line"></div>
                            <div class="signature-name">
                                <asp:Literal ID="litOrganizerName" runat="server"></asp:Literal>
                            </div>
                            <div>Event Organizer</div>
                        </div>
                        
                        <div class="signature-section">
                            <div class="signature-line"></div>
                            <div class="signature-name">
                                <asp:Literal ID="litOrganizationName" runat="server"></asp:Literal>
                            </div>
                            <div>Organization</div>
                        </div>
                    </div>
                    
                    <div class="certificate-date">
                        Issued on: <asp:Literal ID="litIssueDate" runat="server"></asp:Literal>
                    </div>
                    
                    <div class="certificate-seal">
                        <!-- You can add a seal or logo image here -->
                    </div>
                </div>
                
                <!-- Action Buttons -->
                <div class="action-buttons">
                    <button type="button" class="print-btn" onclick="printCertificate()">Print Certificate</button>
                    <asp:Button ID="btnBack" runat="server" Text="Back" CssClass="back-btn" OnClick="btnBack_Click" />
                </div>
            </div>
        </div>
    </body>
</asp:Content>