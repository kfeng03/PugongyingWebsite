<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="EM_EventDetails.aspx.cs" 
Inherits="fyp.EM_EventDetails" MasterPageFile="~/Site1.Master" Async="true" %>

<asp:Content ID="ContentTitle" ContentPlaceHolderID="title" runat="server">Event Details</asp:Content>

<asp:Content ID="ContentStyle" ContentPlaceHolderID="css" runat="server">
    <style>
        .event-details-container {
            width: 95%;
            margin-top: 20px;
        }

        .event-header {
            display: flex;
            justify-content: space-between;
            align-items: flex-start;
            margin-bottom: 20px;
        }

        .event-title {
            font-size: 24px;
            font-weight: bold;
            margin-bottom: 10px;
        }

        .event-status {
            font-size: 16px;
            font-weight: bold;
            padding: 8px 15px;
            border-radius: 5px;
            text-align: center;
        }

        .status-upcoming {
            background-color: #4CAF50;
            color: white;
        }
        
        .status-in-progress {
            background-color: #2196F3;
            color: white;
        }

        .status-completed {
            background-color: #F44336;
            color: white;
        }

        .event-organizer {
            font-size: 16px;
            color: #666;
            margin-bottom: 10px;
        }

        .event-datetime {
            font-size: 16px;
            margin-bottom: 10px;
        }

        .event-location {
            font-size: 16px;
            margin-bottom: 20px;
        }

        .event-description {
            background-color: #f9f9f9;
            padding: 20px;
            border-radius: 5px;
            margin-bottom: 20px;
            white-space: pre-line;
        }

        .event-participants {
            font-size: 16px;
            margin-bottom: 20px;
        }

        .event-materials {
            margin-top: 20px;
        }

        .materials-title {
            font-size: 18px;
            font-weight: bold;
            margin-bottom: 10px;
        }

        .materials-list {
            list-style: none;
            padding: 0;
        }

        .material-item {
            padding: 10px;
            background-color: #f9f9f9;
            margin-bottom: 5px;
            border-radius: 5px;
            display: flex;
            justify-content: space-between;
            align-items: center;
        }

        .download-link {
            color: #2196F3;
            text-decoration: none;
        }

        .download-link:hover {
            text-decoration: underline;
        }

        .event
        .event-image {
            max-width: 100%;
            max-height: 400px;
            object-fit: cover;
            border-radius: 5px;
            margin-bottom: 20px;
        }

        .action-buttons {
            display: flex;
            gap: 10px;
            margin-top: 20px;
        }

        .back-btn, .join-btn, .leave-btn {
            padding: 10px 20px;
            font-size: 16px;
            border-radius: 5px;
            border: none;
            cursor: pointer;
            transition: background-color 0.3s ease;
        }

        .back-btn {
            background-color: #2196F3;
            color: white;
        }

        .back-btn:hover {
            background-color: #0b7dda;
        }

        .join-btn {
            background-color: #4CAF50;
            color: white;
        }

        .join-btn:hover {
            background-color: #3e8e41;
        }

        .join-btn:disabled {
            background-color: #cccccc;
            color: #666666;
            cursor: not-allowed;
        }

        .leave-btn {
            background-color: #F44336;
            color: white;
        }

        .leave-btn:hover {
            background-color: #d32f2f;
        }
        
        .registration-info {
            font-size: 14px;
            color: #9C27B0;
            font-style: italic;
            margin-top: 5px;
        }
    </style>
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
                <asp:Button ID="btnEventFeedbacks" runat="server" CssClass='<%# (Request.Url.AbsolutePath.EndsWith("EM_EventFeedbacks.aspx")) ? "sidebar-btn active" : "sidebar-btn" %>' Text="Event Feedbacks" PostBackUrl="~/OMTS_Pages/OMTS_EM/EM_EventFeedbacks.aspx" Visible="false" />
            </div>

            <!-- Main Content Area -->
        <div class="main-content">
            <!-- Message Label for Errors/Notifications -->
            <asp:Label ID="lblMessage" runat="server" EnableViewState="false"></asp:Label>

            <!-- Event Details -->
            <div class="event-details-container">
                <div class="event-header">
                    <div>
                        <h2 class="event-title">
                            <asp:Literal ID="litEventTitle" runat="server"></asp:Literal>
                        </h2>
                        <div class="event-organizer">
                            Organizer: <asp:Literal ID="litOrganizerName" runat="server"></asp:Literal>
                        </div>
                    </div>
                    <div id="divEventStatus" runat="server" class="event-status">
                        <asp:Literal ID="litEventStatus" runat="server"></asp:Literal>
                    </div>
                </div>

                <!-- Event Image (if available) -->
                <asp:Image ID="imgEventPicture" runat="server" CssClass="event-image" Visible="false" />

                <div class="event-datetime">
                    <div>Start Date: <asp:Literal ID="litEventStartDate" runat="server"></asp:Literal> | 
                    Time: <asp:Literal ID="litEventTime" runat="server"></asp:Literal></div>
                    <div id="divEndDate" runat="server">End Date: <asp:Literal ID="litEventEndDate" runat="server"></asp:Literal></div>
                    <div id="divRegistration" runat="server" class="registration-info">Registration Opens: <asp:Literal ID="litRegistrationDate" runat="server"></asp:Literal></div>
                </div>

                <div class="event-location">
                    Location: <asp:Literal ID="litEventLocation" runat="server"></asp:Literal>
                </div>

                <div class="event-participants">
                    <asp:Literal ID="litParticipants" runat="server"></asp:Literal>
                </div>

                <div class="event-description">
                    <asp:Literal ID="litEventDescription" runat="server"></asp:Literal>
                </div>

                <!-- Event Materials -->
                <div class="event-materials">
                    <h3 class="materials-title">Event Materials</h3>
                    <asp:Panel ID="pnlNoMaterials" runat="server" Visible="false">
                        <p>No materials available for this event.</p>
                    </asp:Panel>
                    <ul class="materials-list">
                        <asp:Repeater ID="rptEventMaterials" runat="server" OnItemDataBound="rptEventMaterials_ItemDataBound">
                            <ItemTemplate>
                                <li class="material-item">
                                    <span><%# Container.DataItem %></span>
                                    <asp:HyperLink ID="hlDownload" runat="server" CssClass="download-link" Target="_blank">Download</asp:HyperLink>
                                </li>
                            </ItemTemplate>
                        </asp:Repeater>
                    </ul>
                </div>

                <!-- Action Buttons -->
                <div class="action-buttons">
                    <asp:Button ID="btnBack" runat="server" Text="Back" CssClass="back-btn" OnClick="btnBack_Click" />
                    <asp:Button ID="btnJoin" runat="server" Text="Join Event" CssClass="join-btn" OnClick="btnJoin_Click" Visible="false" />
                    <asp:Button ID="btnLeave" runat="server" Text="Leave Event" CssClass="leave-btn" OnClick="btnLeave_Click" Visible="false" />
                </div>
            </div>
        </div>
    </div>
    </body>
</asp:Content>