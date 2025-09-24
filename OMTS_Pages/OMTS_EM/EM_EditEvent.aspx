<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="EM_EditEvent.aspx.cs" 
Inherits="fyp.EM_EditEvent" MasterPageFile="~/Site1.Master" Async="true" %>

<asp:Content ID="ContentTitle" ContentPlaceHolderID="title" runat="server">Edit Event</asp:Content>

<asp:Content ID="ContentStyle" ContentPlaceHolderID="css" runat="server">
    <style>
        /* Modern form styling based on portfolio design */
        .page-header {
            font-size: 24px;
            font-weight: 600;
            color: #333;
            margin-bottom: 25px;
            padding-bottom: 10px;
            border-bottom: 2px solid #f0f0f0;
        }
        
        .form-container {
            background-color: #ffffff;
            border-radius: 8px;
            box-shadow: 0 2px 10px rgba(0, 0, 0, 0.05);
            padding: 30px;
            max-width: 95%;
        }
        
        .form-group {
            margin-bottom: 20px;
        }

        .form-label {
            display: block;
            margin-bottom: 6px;
            font-weight: 500;
            color: #333;
            font-size: 14px;
        }

        .form-control {
            width: 100%;
            padding: 10px 12px;
            border: 1px solid #ddd;
            border-radius: 6px;
            box-sizing: border-box;
            font-size: 14px;
            transition: border-color 0.3s ease;
        }
        
        .form-control:focus {
            border-color: #007bff;
            outline: none;
            box-shadow: 0 0 0 2px rgba(0, 123, 255, 0.25);
        }

        .text-area {
            min-height: 120px;
            resize: vertical;
        }

        .btn-container {
            margin-top: 30px;
            display: flex;
            gap: 15px;
        }

        .btn-primary {
            padding: 10px 20px;
            background-color: #007bff;
            color: white;
            border: none;
            border-radius: 6px;
            cursor: pointer;
            font-weight: 500;
            transition: background-color 0.3s ease;
        }

        .btn-primary:hover {
            background-color: #0069d9;
        }

        .btn-danger {
            padding: 10px 20px;
            background-color: #dc3545;
            color: white;
            border: none;
            border-radius: 6px;
            cursor: pointer;
            font-weight: 500;
            transition: background-color 0.3s ease;
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
        
        /* Improved input controls */
        select.form-control {
            appearance: none;
            -webkit-appearance: none;
            -moz-appearance: none;
            background-image: url("data:image/svg+xml;charset=UTF-8,%3csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='none' stroke='currentColor' stroke-width='2' stroke-linecap='round' stroke-linejoin='round'%3e%3cpolyline points='6 9 12 15 18 9'%3e%3c/polyline%3e%3c/svg%3e");
            background-repeat: no-repeat;
            background-position: right 10px center;
            background-size: 1em;
            padding-right: 30px;
        }
        
        /* Two-column layout for form fields */
        .form-row {
            display: flex;
            flex-wrap: wrap;
            margin-right: -10px;
            margin-left: -10px;
        }
        
        .form-col {
            flex: 0 0 50%;
            max-width: 50%;
            padding-right: 10px;
            padding-left: 10px;
            box-sizing: border-box;
        }
        
        /* Responsive adjustments */
        @media (max-width: 768px) {
            .form-col {
                flex: 0 0 100%;
                max-width: 100%;
            }
        }
    </style>
</asp:Content>

<asp:Content ID="ContentMain" ContentPlaceHolderID="MainContent" runat="server">
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
            <asp:Button ID="btnEventManagement" runat="server" CssClass='<%# (Request.Url.AbsolutePath.EndsWith("EM_EventManagement.aspx")) ? "sidebar-btn active" : "sidebar-btn" %>' 
                Text="Event Management" PostBackUrl="~/OMTS_Pages/OMTS_EM/EM_EventManagement.aspx" />
            <asp:Button ID="btnEventFeedbacks" runat="server" CssClass='<%# (Request.Url.AbsolutePath.EndsWith("EM_EventFeedbacks.aspx")) ? "sidebar-btn active" : "sidebar-btn" %>' Text="Event Feedbacks" PostBackUrl="~/OMTS_Pages/OMTS_EM/EM_EventFeedbacks.aspx" Visible="false" />
        </div>

        <!-- Main Content Area -->
        <div class="main-content">
            <h2 class="page-header">
                <asp:Literal ID="litEditEvent" runat="server" Text="Edit Event" />
            </h2>

            <!-- Hidden Field for Event ID -->
            <asp:HiddenField ID="hdnEventID" runat="server" />

            <!-- Event Form -->
            <div class="form-container">
                <div class="form-row">
                    <div class="form-col">
                        <!-- Event Title -->
                        <div class="form-group">
                            <label for="txtEventTitle" class="form-label">Event Title:</label>
                            <asp:TextBox ID="txtEventTitle" runat="server" CssClass="form-control" placeholder="Enter event title"></asp:TextBox>
                        </div>
                    </div>
                    <div class="form-col">
                        <!-- Event Location -->
                        <div class="form-group">
                            <label for="txtEventLocation" class="form-label">Event Location:</label>
                            <asp:TextBox ID="txtEventLocation" runat="server" CssClass="form-control" placeholder="Enter event location"></asp:TextBox>
                        </div>
                    </div>
                </div>

                <!-- Event Description -->
                <div class="form-group">
                    <label for="txtEventDescription" class="form-label">Event Description:</label>
                    <asp:TextBox ID="txtEventDescription" runat="server" CssClass="form-control text-area" 
                        TextMode="MultiLine" Rows="5" placeholder="Enter event description"></asp:TextBox>
                </div>

                <div class="form-row">
                    <div class="form-col">
                        <!-- Event Start Date -->
                        <div class="form-group">
                            <label for="txtEventStartDate" class="form-label">Event Start Date:</label>
                            <asp:TextBox ID="txtEventStartDate" runat="server" CssClass="form-control" TextMode="Date"></asp:TextBox>
                        </div>
                    </div>
                    <div class="form-col">
                        <!-- Event End Date -->
                        <div class="form-group">
                            <label for="txtEventEndDate" class="form-label">Event End Date:</label>
                            <asp:TextBox ID="txtEventEndDate" runat="server" CssClass="form-control" TextMode="Date"></asp:TextBox>
                        </div>
                    </div>
                </div>

                <div class="form-row">
                    <div class="form-col">
                        <!-- Event Time -->
                        <div class="form-group">
                            <label for="txtEventTime" class="form-label">Event Time:</label>
                            <asp:TextBox ID="txtEventTime" runat="server" CssClass="form-control" TextMode="Time"></asp:TextBox>
                        </div>
                    </div>
                    <div class="form-col">
                        <!-- Registration Start Date -->
                        <div class="form-group">
                            <label for="txtRegistrationStartDate" class="form-label">Registration Start Date:</label>
                            <asp:TextBox ID="txtRegistrationStartDate" runat="server" CssClass="form-control" TextMode="Date"></asp:TextBox>
                        </div>
                    </div>
                </div>
                
                <!-- Maximum Participants -->
                <div class="form-group">
                    <label for="ddlMaxParticipants" class="form-label">Maximum Participants:</label>
                    <asp:DropDownList ID="ddlMaxParticipants" runat="server" CssClass="form-control">
                        <asp:ListItem Value="10">10</asp:ListItem>
                        <asp:ListItem Value="20">20</asp:ListItem>
                        <asp:ListItem Value="30">30</asp:ListItem>
                        <asp:ListItem Value="50">50</asp:ListItem>
                        <asp:ListItem Value="100">100</asp:ListItem>
                        <asp:ListItem Value="200">200</asp:ListItem>
                        <asp:ListItem Value="500">500</asp:ListItem>
                        <asp:ListItem Value="1000">1000</asp:ListItem>
                    </asp:DropDownList>
                </div>

                <!-- Error Messages -->
                <asp:Label ID="lblMessage" runat="server" CssClass="error-message" Visible="false"></asp:Label>

                <!-- Form Buttons -->
                <div class="btn-container">
                    <asp:Button ID="btnUpdateEvent" runat="server" Text="Update Event" CssClass="btn-primary" OnClick="btnUpdateEvent_Click" />
                    <asp:Button ID="btnCancel" runat="server" Text="Cancel" CssClass="btn-danger" OnClick="btnCancel_Click" />
                </div>
            </div>
        </div>
    </div>
</asp:Content>