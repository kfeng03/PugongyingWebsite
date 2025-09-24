<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="EM_RecycleBin.aspx.cs" 
Inherits="fyp.EM_RecycleBin" MasterPageFile="~/Site1.Master" Async="true" %>

<asp:Content ID="ContentTitle" ContentPlaceHolderID="title" runat="server">Event Recycle Bin</asp:Content>

<asp:Content ID="ContentStyle" ContentPlaceHolderID="css" runat="server">
    <style>
        .event-list {
            display: flex;
            flex-direction: column;
            gap: 15px;
            margin-top: 20px;
            width: 100%;
        }

        .event-item {
            width: 95%;
            padding: 15px;
            background-color: #f9f9f9;
            border: 1px solid #ddd;
            border-radius: 5px;
            display: flex;
            justify-content: space-between;
            align-items: center;
            transition: transform 0.2s ease, box-shadow 0.2s ease;
        }

        .event-item:hover {
            transform: translateY(-3px);
            box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
            background-color: #f0f0f0;
        }

        .event-info {
            display: flex;
            flex-direction: column;
            gap: 5px;
        }

        .event-title {
            font-size: 18px;
            font-weight: bold;
        }

        .event-organizer {
            font-size: 14px;
            color: #666;
        }

        .button-container {
            display: flex;
            gap: 10px;
        }

        .restore-btn, .delete-btn {
            padding: 8px 15px; 
            font-size: 14px; 
            min-width: 80px;  
            text-align: center;
        }
       
        .restore-btn {
            background-color: #4CAF50;
            color: white;
            border: none;
            border-radius: 5px;
            cursor: pointer;
            transition: background-color 0.3s ease;
        }

        .restore-btn:hover {
            background-color: #3e8e41;
        }

        .delete-btn {
            background-color: #ff4d4d; 
            color: white;
            border: none;
            border-radius: 5px;
            cursor: pointer;
            transition: background-color 0.3s ease;
        }

        .delete-btn:hover {
            background-color: #cc0000; 
        }

        .back-btn {
            background-color: #2196F3;
            color: white;
            border: none;
            border-radius: 5px;
            padding: 8px 15px;
            margin-top: 10px;
            cursor: pointer;
            transition: background-color 0.3s ease;
        }

        .back-btn:hover {
            background-color: #0b7dda;
        }

        .empty-message {
            margin-top: 20px;
            font-size: 16px;
            color: #666;
        }
    </style>
</asp:Content>

<asp:Content ID="ContentMain" ContentPlaceHolderID="MainContent" runat="server">
    <body>
            <!-- Main Content Layout -->
            <div class="container">
                <!-- Sidebar -->
                <div class="sidebar">
                    <asp:Button ID="btnMyEvents" runat="server" CssClass='<%# (Request.Url.AbsolutePath.EndsWith("EM_Events.aspx")) ? "sidebar-btn active" : "sidebar-btn" %>' 
                        Text="My Events" PostBackUrl="~/OMTS_Pages/OMTS_EM/EM_Events.aspx" />
                    <asp:Button ID="btnJoinEvent" runat="server" CssClass='<%# (Request.Url.AbsolutePath.EndsWith("EM_JoinEvent.aspx")) ? "sidebar-btn active" : "sidebar-btn" %>' 
                        Text="Join Event" PostBackUrl="~/OMTS_Pages/OMTS_EM/EM_JoinEvent.aspx" />
                    <asp:Button ID="btnCompletedEvent" runat="server" CssClass='<%# (Request.Url.AbsolutePath.EndsWith("EM_CompletedEvent.aspx")) ? "sidebar-btn active" : "sidebar-btn" %>' 
                        Text="Completed Events" PostBackUrl="~/OMTS_Pages/OMTS_EM/EM_CompletedEvent.aspx" />
                    <asp:Button ID="btnEventManagement" runat="server" CssClass='<%# (Request.Url.AbsolutePath.EndsWith("EM_RecycleBin.aspx")) ? "sidebar-btn active" : "sidebar-btn" %>' 
                        Text="Event Management" PostBackUrl="~/OMTS_Pages/OMTS_EM/EM_EventManagement.aspx" />
                    <asp:Button ID="btnEventFeedbacks" runat="server" CssClass='<%# (Request.Url.AbsolutePath.EndsWith("EM_EventFeedbacks.aspx")) ? "sidebar-btn active" : "sidebar-btn" %>' 
                        Text="Event Feedbacks" PostBackUrl="~/OMTS_Pages/OMTS_EM/EM_EventFeedbacks.aspx" />
                </div>

                <!-- Main Content Area -->
                <div class="main-content">
                    <h2 class="page-header">Event Recycle Bin</h2>
                    <asp:Button ID="btnBack" runat="server" Text="Back to Event Management" OnClick="btnBack_Click" CssClass="back-btn" />

                    <!-- Message Label for Errors -->
                    <asp:Label ID="lblMessage" runat="server" ForeColor="Red" EnableViewState="false"></asp:Label>

                    <!-- Deleted Event List -->
                    <div class="event-list">
                        <asp:Label ID="lblNoDeletedEvents" runat="server" Text="No deleted events found." CssClass="empty-message" Visible="false"></asp:Label>
                        <asp:Repeater ID="rptDeletedEvents" runat="server">
                            <ItemTemplate>
                                <div class="event-item">
                                    <div class="event-info">
                                        <div class="event-title"><%# Eval("EventTitle") %></div>
                                        <div class="event-organizer">Organizer: <%# Eval("OrganizerName") %></div>
                                    </div>

                                    <!-- Buttons Container -->
                                    <div class="button-container">
                                        <!-- Restore Button -->
                                        <asp:Button ID="btnRestore" runat="server" Text="Restore" 
                                            CssClass="restore-btn"
                                            CommandArgument='<%# Eval("EventID") %>' 
                                            OnClick="btnRestore_Click" />

                                        <!-- Permanent Delete Button -->
                                        <asp:Button ID="btnDelete" runat="server" Text="Delete Permanently" 
                                            CssClass="delete-btn"
                                            CommandArgument='<%# Eval("EventID") %>' 
                                            OnClick="btnDelete_Click"
                                            OnClientClick="return confirm('Are you sure you want to permanently delete this event? This action cannot be undone.');" />
                                    </div>
                                </div>
                            </ItemTemplate>
                        </asp:Repeater>
                    </div>
                </div>
            </div>
    </body>
</asp:Content>