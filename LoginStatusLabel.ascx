<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="LoginStatusLabel.ascx.cs" Inherits="fyp.LoginStatusLabel" %>

<div>
    <asp:Label ID="lblInfo" CssClass="lbl" runat="server" style="color: black;text-decoration: none;font-size: 14px;padding: 4px 0px;"></asp:Label>

    <asp:Button ID="btnLogin" CssClass="button" runat="server" Text="Login" OnClick="btnLogin_Click" CausesValidation="false" ValidateRequestMode="Disabled" Visible="False" 
        style="
        text-align: center;
        border-radius: 25px;
        font-size:14px;
        border: 1px solid black;
        background: transparent;
        color: black;
        cursor: pointer;
        overflow: hidden;
        text-decoration: none;
        padding:2px"
        />

   <asp:Button ID="btnProfile" CssClass="button" runat="server" Text="Profile" OnClick="btnProfile_Click" CausesValidation="false" ValidateRequestMode="Disabled" Visible="False" 
        style="
        text-align: center;
        border-radius: 25px;
        font-size:14px;
        border: 1px solid black;
        background: transparent;
        color: black;
        cursor: pointer;
        overflow: hidden;
        text-decoration: none;
        padding:2px"
        />

    <asp:Button ID="btnLogout" CssClass="button" runat="server" Text="Logout" OnClick="btnLogout_Click" CausesValidation="false" ValidateRequestMode="Disabled" Visible="False" 
        style="
        text-align: center;
        border-radius: 25px;
        font-size: 14px;
        border: 1px solid black;
        background: transparent;
        color: black;
        cursor: pointer;
        overflow: hidden;
        text-decoration: none;
        padding:2px"/>
<%--
    <asp:DropDownList ID="ddlLanguages" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlLanguages_SelectedIndexChanged">
        <asp:ListItem Text="English" Value="en" />
        <asp:ListItem Text="Chinese" Value="zh" />
        <asp:ListItem Text="Malay" Value="ms" />
    </asp:DropDownList>--%>
     

    <br />
</div>
<p>
    <asp:LoginStatus ID="LoginStatus1" runat="server" Visible="False" />

    </p>

