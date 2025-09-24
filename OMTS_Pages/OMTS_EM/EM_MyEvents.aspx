<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="EM_MyEvents.aspx.cs" 
Inherits="fyp.EM_MyEvents" MasterPageFile="~/Site1.Master" Async="true" %>

<asp:Content ID="ContentTitle" ContentPlaceHolderID="title" runat="server">My Events</asp:Content>

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

        .btn-danger {
            background-color: #dc3545;
            color: white;
        }

        .btn-danger:hover {
            background-color: #c82333;
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
        
        .success-message {
            color: #28a745;
            margin-top: 15px;
            font-size: 14px;
            padding: 10px;
            background-color: #d4edda;
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
        
        .icon-leave::before {
            content: "\f235";
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
            <h2 class="page-header">My Events</h2>

            <!-- Message Label for Errors/Notifications -->
            <asp:Label ID="lblMessage" runat="server" EnableViewState="false" Visible="false"></asp:Label>

            <!-- My Events List -->
            <div class="event-list">
                <asp:Label ID="lblNoEvents" runat="server" Text="You haven't joined any events yet. Check out the 'Join Event' section to find upcoming events!" CssClass="empty-message" Visible="false"></asp:Label>
                
                <asp:Repeater ID="rptMyEvents" runat="server">
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
                                        <span class="event-status <%# GetStatusCssClass(Eval("EventStatus").ToString()) %>">
                                            <i class="fas fa-circle"></i> <%# Eval("EventStatus") %>
                                        </span>
                                    </div>
                                </div>
                            </div>

                            <!-- Buttons Container -->
                            <div class="button-container">
                                <asp:Button ID="btnViewDetails" runat="server" Text="View Details" 
                                    CssClass="btn btn-primary icon-btn icon-eye"
                                    CommandArgument='<%# Eval("EventID") %>' 
                                    OnClick="btnViewDetails_Click" />
                                    
                                <!-- Leave Event Button -->
                                <asp:Button ID="btnLeave" runat="server" Text="Leave Event" 
                                    CssClass="btn btn-danger icon-btn icon-leave"
                                    CommandArgument='<%# Eval("EventID") %>' 
                                    OnClick="btnLeave_Click"
                                    OnClientClick="return confirm('Are you sure you want to leave this event? You can always join again later.');" />
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