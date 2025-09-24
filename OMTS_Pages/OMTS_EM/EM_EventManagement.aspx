<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="EM_EventManagement.aspx.cs" 
Inherits="fyp.EM_EventManagement" MasterPageFile="~/Site1.Master" Async="true" %>

<asp:Content ID="ContentTitle" ContentPlaceHolderID="title" runat="server">Event Management</asp:Content>

<asp:Content ID="ContentStyle" ContentPlaceHolderID="css" runat="server">
    <style>
        /* Modern event list styling based on portfolio design */
        .page-header {
            font-size: 24px;
            font-weight: 600;
            color: #333;
            margin-bottom: 25px;
            padding-bottom: 10px;
            border-bottom: 2px solid #f0f0f0;
        }
        
        .header-actions {
            display: flex;
            justify-content: space-between;
            align-items: center;
            margin-bottom: 25px;
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
            display: inline-flex;
            align-items: center;
            gap: 6px;
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
        
        .btn-purple {
            background-color: #6f42c1;
            color: white;
        }
        
        .btn-purple:hover {
            background-color: #5e35b1;
        }

        .btn-success {
            background-color: #28a745;
            color: white;
        }

        .btn-success:hover {
            background-color: #218838;
        }
        
        .add-event-card {
            display: flex;
            align-items: center;
            justify-content: center;
            background-color: #f8f9fa;
            border: 2px dashed #dee2e6;
            border-radius: 8px;
            padding: 20px;
            cursor: pointer;
            transition: all 0.2s ease;
            height: 70px;
            text-decoration: none;
        }
        
        .add-event-card:hover {
            background-color: #e9ecef;
            border-color: #adb5bd;
        }
        
        .add-icon {
            display: flex;
            align-items: center;
            gap: 10px;
            font-size: 16px;
            font-weight: 500;
            color: #6c757d;
        }
        
        .add-icon i {
            font-size: 24px;
            color: #007bff;
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
        
        .recycle-btn {
            background-color: #28a745;
            color: white;
            border: none;
            border-radius: 6px;
            padding: 8px 16px;
            cursor: pointer;
            transition: all 0.2s ease;
            font-weight: 500;
            font-size: 14px;
        }

        .recycle-btn:hover {
            background-color: #218838;
        }
        
        /* Icon classes for buttons */
        .icon-btn::before {
            font-family: "Font Awesome 5 Free";
            font-weight: 900;
            margin-right: 8px;
        }
        
        .icon-users::before {
            content: "\f0c0";
        }
        
        .icon-edit::before {
            content: "\f044";
        }
        
        .icon-trash::before {
            content: "\f1f8";
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
            display: inline-flex;
            align-items: center;
            gap: 6px;
            text-decoration: none; 
        }

        .btn:hover {
            text-decoration: none;
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

        .btn-purple {
            background-color: #6f42c1;
            color: white;
        }

        .btn-purple:hover {
            background-color: #5e35b1;
        }

        /* Additional fix for any potential anchor styling */
        a.btn {
            text-decoration: none;
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
            <div class="header-actions">
                <h2 class="page-header">Event Management</h2>
                <asp:Button ID="btnRecycleBin" runat="server" Text="Recycle Bin" OnClick="btnRecycleBin_Click" CssClass="recycle-btn" />
            </div>

            <!-- Message Label for Errors -->
            <asp:Label ID="lblMessage" runat="server" CssClass="error-message" EnableViewState="false" Visible="false"></asp:Label>

            <!-- Event List Section -->
            <div class="event-list">
                <asp:LinkButton ID="lnkAddEvent" runat="server" CssClass="add-event-card" PostBackUrl="~/OMTS_Pages/OMTS_EM/EM_AddEvent.aspx">
                    <span class="add-icon">
                        <i class="fas fa-plus-circle"></i> Add New Event
                    </span>
                </asp:LinkButton>

                <asp:Repeater ID="rptEvents" runat="server">
                    <ItemTemplate>
                        <div class="event-item">
                            <div class="event-info">
                                <div class="event-title"><%# Eval("EventTitle") %></div>
                                <div class="event-details">
                                    <div class="event-detail">
                                        <i class="fas fa-calendar"></i>
                                        <span><%# Eval("EventDate") %></span>
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
                                        <span class="event-status <%# GetStatusClass(Eval("EventStatus").ToString()) %>">
                                            <%# Eval("EventStatus") %>
                                        </span>
                                    </div>
                                </div>
                            </div>

                            <!-- Buttons Container -->
                            <div class="button-container">
                                <!-- View Participants Button -->
                                <asp:LinkButton ID="btnParticipants" runat="server" CssClass="btn btn-purple icon-btn icon-users"
                                    CommandArgument='<%# Eval("EventID") %>' OnClick="btnParticipants_Click"
                                    Text="Participants" />
                                
                                <!-- Edit Button -->
                                <asp:LinkButton ID="btnEdit" runat="server" CssClass="btn btn-primary icon-btn icon-edit"
                                    CommandArgument='<%# Eval("EventID") %>' OnClick="btnEdit_Click"
                                    Text="Edit" />

                                <!-- Delete Button -->
                                <asp:Button ID="btnDelete" runat="server" Text="Delete" 
                                    CssClass="btn btn-danger icon-btn icon-trash"
                                    CommandArgument='<%# Eval("EventID") %>' 
                                    OnClick="btnDelete_Click"
                                    UseSubmitBehavior="false"
                                    OnClientClick="confirmDelete(this); return false;" />
                            </div>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
            </div>
        </div>
    </div>

    <!-- Font Awesome CDN for icons -->
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/5.15.4/css/all.min.css">
    
    <script type="text/javascript">
        function confirmDelete(button) {
            if (confirm("Are you sure you want to delete this event?")) {
                __doPostBack(button.name, '');
            }
        }
    </script>
</asp:Content>