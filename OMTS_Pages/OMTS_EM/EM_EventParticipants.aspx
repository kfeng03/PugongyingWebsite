<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="EM_EventParticipants.aspx.cs" 
Inherits="fyp.EM_EventParticipants" MasterPageFile="~/Site1.Master" Async="true" %>

<asp:Content ID="ContentTitle" ContentPlaceHolderID="title" runat="server">Event Participants</asp:Content>

<asp:Content ID="ContentStyle" ContentPlaceHolderID="css" runat="server">
    <style>
        .participants-container {
            margin-top: 20px;
            width: 100%;
        }
        
        .event-title {
            font-size: 24px;
            font-weight: bold;
            margin-bottom: 5px;
        }
        
        .event-info {
            font-size: 16px;
            color: #555;
            margin-bottom: 20px;
        }
        
        .participants-table {
            width: 100%;
            border-collapse: collapse;
            margin-top: 20px;
        }
        
        .participants-table th {
            background-color: #f2f2f2;
            padding: 10px;
            text-align: left;
            border: 1px solid #ddd;
        }
        
        .participants-table td {
            padding: 10px;
            border: 1px solid #ddd;
        }
        
        .participants-table tr:nth-child(even) {
            background-color: #f9f9f9;
        }
        
        .participants-table tr:hover {
            background-color: #f0f0f0;
        }
        
        .no-participants {
            margin-top: 20px;
            font-size: 16px;
            color: #666;
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
        
        .export-btn {
            background-color: #4CAF50;
            color: white;
            border: none;
            border-radius: 5px;
            padding: 8px 15px;
            margin-top: 10px;
            margin-left: 10px;
            cursor: pointer;
            transition: background-color 0.3s ease;
        }
        
        .export-btn:hover {
            background-color: #3e8e41;
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
                    Text="My Events" PostBackUrl="~/OMTS_Pages/OMTS_EM/EM_MyEvents.aspx" />
                <asp:Button ID="btnJoinEvent" runat="server" CssClass='<%# (Request.Url.AbsolutePath.EndsWith("EM_JoinEvent.aspx")) ? "sidebar-btn active" : "sidebar-btn" %>' 
                    Text="Join Event" PostBackUrl="~/OMTS_Pages/OMTS_EM/EM_JoinEvent.aspx" />
                <asp:Button ID="btnCompletedEvent" runat="server" CssClass='<%# (Request.Url.AbsolutePath.EndsWith("EM_CompletedEvent.aspx")) ? "sidebar-btn active" : "sidebar-btn" %>' 
                    Text="Completed Events" PostBackUrl="~/OMTS_Pages/OMTS_EM/EM_CompletedEvent.aspx" />
                <asp:Button ID="btnEventManagement" runat="server" CssClass='<%# (Request.Url.AbsolutePath.EndsWith("EM_EventManagement.aspx")) ? "sidebar-btn active" : "sidebar-btn" %>' 
                    Text="Event Management" PostBackUrl="~/OMTS_Pages/OMTS_EM/EM_EventManagement.aspx" />
            </div>

            <!-- Main Content Area -->
            <div class="main-content">
                <h2 class="page-header">Event Participants</h2>
                
                <!-- Message Label for Errors -->
                <asp:Label ID="lblMessage" runat="server" ForeColor="Red" EnableViewState="false"></asp:Label>
                
                <div class="participants-container">
                    <div class="event-title">
                        <asp:Literal ID="litEventTitle" runat="server"></asp:Literal>
                    </div>
                    <div class="event-info">
                        <asp:Literal ID="litEventInfo" runat="server"></asp:Literal>
                    </div>
                    
                    <div class="buttons-container">
                        <asp:Button ID="btnBack" runat="server" Text="Back to Event Management" CssClass="back-btn" OnClick="btnBack_Click" />
                    </div>
                    
                    <!-- Participants Table -->
                    <asp:GridView ID="gvParticipants" runat="server" CssClass="participants-table" AutoGenerateColumns="false"
                        EmptyDataText="No participants found for this event." Width="100%">
                        <Columns>
                            <asp:BoundField DataField="UserId" HeaderText="User ID" />
                            <asp:BoundField DataField="Username" HeaderText="Username" />
                            <asp:BoundField DataField="Email" HeaderText="Email" />
                            <asp:BoundField DataField="PhoneNumber" HeaderText="Phone Number" />
                        </Columns>
                    </asp:GridView>
                </div>
            </div>
        </div>
    </body>
</asp:Content>