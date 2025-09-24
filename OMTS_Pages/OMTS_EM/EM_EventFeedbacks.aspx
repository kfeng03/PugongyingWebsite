<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="EM_EventFeedbacks.aspx.cs" 
Inherits="fyp.EM_EventFeedbacks" MasterPageFile="~/Site1.Master" Async="true" %>

<asp:Content ID="ContentTitle" ContentPlaceHolderID="title" runat="server">Event Feedbacks</asp:Content>

<asp:Content ID="ContentStyle" ContentPlaceHolderID="css" runat="server">
    <style>
        .feedback-container {
            background-color: #f9f9fa;
            border-radius: 8px;
            padding: 20px;
            margin-bottom: 20px;
        }

        .feedback-header {
            display: flex;
            justify-content: space-between;
            align-items: center;
            margin-bottom: 20px;
        }

        .event-filter {
            display: flex;
            gap: 15px;
            margin-bottom: 20px;
        }

        .feedback-list {
            display: flex;
            flex-direction: column;
            gap: 15px;
        }

        .feedback-item {
            background-color: white;
            border-radius: 8px;
            padding: 15px;
            box-shadow: 0 2px 5px rgba(0,0,0,0.1);
        }

        .feedback-meta {
            display: flex;
            justify-content: space-between;
            margin-bottom: 10px;
            color: #666;
            font-size: 0.9em;
        }

        .feedback-text {
            margin-bottom: 10px;
        }

        .no-feedbacks {
            text-align: center;
            color: #888;
            padding: 20px;
            background-color: #f4f4f4;
            border-radius: 8px;
        }

        .event-select {
            padding: 8px;
            border-radius: 4px;
            border: 1px solid #ddd;
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
            <div class="feedback-container">
                <div class="feedback-header">
                    <h2>Event Feedbacks</h2>
                </div>

                <div class="event-filter">
                    <asp:DropDownList ID="ddlEventFilter" runat="server" 
                        CssClass="event-select" 
                        AutoPostBack="true" 
                        OnSelectedIndexChanged="ddlEventFilter_SelectedIndexChanged">
                    </asp:DropDownList>
                </div>

                <asp:Label ID="lblMessage" runat="server" CssClass="error-message" Visible="false"></asp:Label>

                <div class="feedback-list">
                    <asp:Repeater ID="rptEventFeedbacks" runat="server">
                        <ItemTemplate>
                            <div class="feedback-item">
                                <div class="feedback-meta">
                                    <strong><%# Eval("UserName") %></strong>
                                    <span><%# Eval("SubmittedDate") %></span>
                                </div>
                                <div class="feedback-text">
                                    <%# Eval("FeedbackText") %>
                                </div>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>

                    <asp:Label ID="lblNoFeedbacks" runat="server" 
                        CssClass="no-feedbacks" 
                        Text="No feedbacks found for the selected event." 
                        Visible="false"></asp:Label>
                </div>
            </div>
        </div>
    </div>
</asp:Content>