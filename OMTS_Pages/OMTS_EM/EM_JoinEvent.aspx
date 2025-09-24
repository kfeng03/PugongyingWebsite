<%@ Page Language="C#" Async="true" AutoEventWireup="true" CodeBehind="EM_JoinEvent.aspx.cs" Inherits="fyp.EM_JoinEvent" MasterPageFile="~/Site1.Master" %>

<asp:Content ID="ContentTitle" ContentPlaceHolderID="title" runat="server">Join Events</asp:Content>

<asp:Content ID="ContentStyle" ContentPlaceHolderID="css" runat="server">
    <style>
        /* Modern event list styling consistent with other pages */
        .page-header {
            font-size: 24px;
            font-weight: 600;
            color: #333;
            margin-bottom: 25px;
            padding-bottom: 10px;
            border-bottom: 2px solid #f0f0f0;
        }
        
        .search-container {
            margin-bottom: 20px;
            display: flex;
            gap: 10px;
        }
        
        .search-box {
            flex: 1;
            padding: 10px 12px;
            border: 1px solid #ddd;
            border-radius: 6px;
            font-size: 14px;
            transition: border-color 0.3s ease;
        }
        
        .search-box:focus {
            border-color: #007bff;
            outline: none;
            box-shadow: 0 0 0 2px rgba(0, 123, 255, 0.25);
        }
        
        .search-btn {
            padding: 10px 16px;
            background-color: #007bff;
            color: white;
            border: none;
            border-radius: 6px;
            cursor: pointer;
            font-weight: 500;
            transition: background-color 0.3s ease;
        }
        
        .search-btn:hover {
            background-color: #0069d9;
        }
        
        .event-list {
            display: flex;
            flex-direction: column;
            gap: 15px;
            margin-top: 20px;
            width: 100%;
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
            transform: translateY(-3px);
            box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
        }

        .event-info {
            display: flex;
            flex-direction: column;
            gap: 8px;
        }

        .event-title {
            font-size: 18px;
            font-weight: 600;
            color: #333;
        }

        .event-details {
            display: flex;
            gap: 15px;
            flex-wrap: wrap;
            font-size: 14px;
            color: #666;
        }
        
        .event-detail {
            display: flex;
            align-items: center;
            gap: 6px;
        }
        
        .event-detail i {
            font-size: 16px;
            color: #007bff;
        }
        
        .event-status {
            padding: 4px 10px;
            border-radius: 50px;
            font-size: 12px;
            font-weight: 500;
            display: inline-block;
        }
        
        .status-upcoming {
            background-color: #e3f2fd;
            color: #0d6efd;
        }
        
        .status-in-progress {
            background-color: #e8f5e9;
            color: #2e7d32;
        }
        
        .status-completed {
            background-color: #ffebee;
            color: #c62828;
        }

        .button-container {
            display: flex;
            gap: 10px;
        }

        .btn {
            padding: 8px 16px;
            border: none;
            border-radius: 6px;
            cursor: pointer;
            font-weight: 500;
            font-size: 14px;
            transition: all 0.2s ease;
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
        
        /* Badge styles */
        .joined-badge {
            background-color: #4CAF50;
            color: white;
            padding: 5px 10px;
            border-radius: 4px;
            font-size: 14px;
            text-align: center;
            display: inline-block;
        }
        
        .badge {
            padding: 5px 10px;
            border-radius: 4px;
            font-size: 14px;
            text-align: center;
            display: inline-block;
        }
        
        .badge-warning {
            background-color: #FF9800;
            color: white;
        }
        
        .badge-purple {
            background-color: #9C27B0;
            color: white;
        }
        
        .badge-secondary {
            background-color: #6c757d;
            color: white;
        }
        
        .badge-info {
            background-color: #17a2b8;
            color: white;
        }
        
        .badge-danger {
            background-color: #dc3545;
            color: white;
        }
        
        .error-message {
            color: #dc3545;
            margin-top: 15px;
            font-size: 14px;
            padding: 10px;
            background-color: #f8d7da;
            border-radius: 4px;
            display: inline-block;
        }
        
        .empty-message {
            margin-top: 20px;
            padding: 20px;
            background-color: #f8f9fa;
            border-radius: 8px;
            text-align: center;
            color: #6c757d;
            font-size: 16px;
            border: 1px solid #dee2e6;
        }
        
        /* Icon classes for buttons */
        .icon-btn::before {
            font-family: "Font Awesome 5 Free";
            font-weight: 900;
            margin-right: 8px;
        }
        
        .icon-eye::before {
            content: "\f06e";
        }
        
        .icon-join::before {
            content: "\f234";
        }
    </style>
</asp:Content>

<asp:Content ID="ContentMain" ContentPlaceHolderID="MainContent" runat="server">
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
                Text="Event Management" PostBackUrl="~/OMTS_Pages/OMTS_EM/EM_EventManagement.aspx" />
            <asp:Button ID="btnEventFeedbacks" runat="server" CssClass='<%# (Request.Url.AbsolutePath.EndsWith("EM_EventFeedbacks.aspx")) ? "sidebar-btn active" : "sidebar-btn" %>' 
                Text="Event Feedbacks" PostBackUrl="~/OMTS_Pages/OMTS_EM/EM_EventFeedbacks.aspx" />
        </div>

        <!-- Main Content Area -->
        <div class="main-content">
            <h2 class="page-header">Available Events</h2>

            <!-- Search Box -->
            <div class="search-container">
                <asp:TextBox ID="txtSearch" runat="server" placeholder="Search events..." CssClass="search-box"></asp:TextBox>
                <asp:Button ID="btnSearch" runat="server" Text="Search" OnClick="btnSearch_Click" CssClass="search-btn" />
            </div>

            <!-- Message Label for Errors/Notifications -->
            <asp:Label ID="lblMessage" runat="server" ForeColor="Red" EnableViewState="false" Visible="false"></asp:Label>

            <!-- Available Events List -->
            <div class="event-list">
                <asp:Label ID="lblNoEvents" runat="server" Text="No upcoming events found." CssClass="empty-message" Visible="false"></asp:Label>
                
                <asp:Repeater ID="rptAvailableEvents" runat="server" OnItemDataBound="rptAvailableEvents_ItemDataBound">
                    <ItemTemplate>
                        <div class="event-item">
                            <div class="event-info">
                                <div class="event-title"><%# Eval("EventTitle") %></div>
                                <div class="event-details">
                                    <div class="event-detail">
                                        <i class="fas fa-calendar"></i>
                                        <span><%# Eval("EventStartDate") != null && !string.IsNullOrEmpty(Eval("EventStartDate").ToString()) 
                                                ? Eval("EventStartDate") : Eval("EventDate") %> 
                                            <%# Eval("EventEndDate") != null && !string.IsNullOrEmpty(Eval("EventEndDate").ToString()) 
                                                ? " - " + Eval("EventEndDate") : "" %>
                                        </span>
                                    </div>
                                    <div class="event-detail">
                                        <i class="fas fa-clock"></i>
                                        <span><%# Eval("EventTime") %></span>
                                    </div>
                                    <div class="event-detail">
                                        <i class="fas fa-map-marker-alt"></i>
                                        <span><%# Eval("EventLocation") %></span>
                                    </div>
                                    <div class="event-detail">
                                        <i class="fas fa-users"></i>
                                        <span><asp:Label ID="lblParticipants" runat="server" Text='<%# "Participants: 0/" + Eval("MaxParticipants") %>'></asp:Label></span>
                                    </div>
                                    <div class="event-detail">
                                        <span class="event-status <%# GetStatusCssClass(Eval("EventStatus").ToString()) %>">
                                            <i class="fas fa-circle"></i> <%# Eval("EventStatus") %>
                                        </span>
                                    </div>
                                </div>
                                <div runat="server" visible='<%# Eval("RegistrationStartDate") != null && !string.IsNullOrEmpty(Eval("RegistrationStartDate").ToString()) %>'>
                                    <div class="event-detail">
                                        <i class="fas fa-calendar-plus"></i>
                                        <span>Registration Opens: <%# Eval("RegistrationStartDate") %></span>
                                    </div>
                                </div>
                            </div>

                            <!-- Buttons Container -->
                            <div id="Div1" runat="server" class="button-container">
                                <asp:Button ID="btnViewDetails" runat="server" Text="View Details" 
                                    CssClass="btn btn-primary icon-btn icon-eye"
                                    CommandArgument='<%# Eval("EventID") %>' 
                                    OnClick="btnViewDetails_Click" />
            
                                <asp:Button ID="btnJoin" runat="server" Text="Join" 
                                    CssClass="btn btn-success icon-btn icon-join"
                                    CommandArgument='<%# Eval("EventID") %>' 
                                    OnClick="btnJoin_Click" 
                                    Visible="false" />
                                
                                <asp:Label ID="lblJoined" runat="server" Text="Joined" CssClass="joined-badge" Visible="false"></asp:Label>
                                <asp:Label ID="lblStatus" runat="server" CssClass="badge" Visible="false"></asp:Label>
                            </div>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
            </div>
        </div>
    </div>

    <!-- Font Awesome CDN for icons -->
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/5.15.4/css/all.min.css">
</asp:Content>