<%@ Page Title="Payment Return" Language="C#" MasterPageFile="~/Portfolio_Pages/Portfolio.Master" AutoEventWireup="true" CodeBehind="PaymentReturn.aspx.cs" Inherits="PGY.PaymentReturn" %>

<asp:Content ID="Content1" ContentPlaceHolderID="headContent" runat="server">
    <title>Payment Status - PGY Camp</title>
    <style>
        .payment-container { max-width: 600px; margin: 50px auto; padding: 20px; text-align: center; }
        .text-success { color: #28a745; font-weight: bold; }
        .text-danger { color: #dc3545; font-weight: bold; }
        .student-info { margin: 20px 0; padding: 15px; background: #f8f9fa; border-radius: 5px; }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="content" runat="server">
    <form runat="server">
    <div class="payment-container">
        <h2>Payment Status</h2>
        
        <asp:Label ID="lblMessage" runat="server" CssClass="message" />
        
        <div class="student-info" id="studentInfo" runat="server" visible="false">
            <h3>Registration Details</h3>
            <p><strong>Student Name:</strong> <asp:Literal ID="ltStudentName" runat="server" /></p>
            <p><strong>Email:</strong> <asp:Literal ID="ltEmail" runat="server" /></p>
            <p><strong>Payment Status:</strong> <asp:Literal ID="ltStatus" runat="server" /></p>
        </div>
        
        <div style="margin-top: 20px;">
            <asp:Button ID="btnReturnHome" runat="server" Text="Return to Home" 
                CssClass="btn btn-primary" OnClick="btnReturnHome_Click" Visible="false" />
        </div>
    </div>
    </form>
</asp:Content>