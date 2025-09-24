<%@ Page Title="" Language="C#" MasterPageFile="~/Portfolio_Pages/Portfolio.Master" AutoEventWireup="true" CodeBehind="RegistrationForm.aspx.cs" Inherits="PGY.WebForm3" %>
<asp:Content ID="Content1" ContentPlaceHolderID="headContent" runat="server">
    <title>学生注册表单 - Student Registration Form</title>
    <style>
        .container { max-width: 800px; margin: 0 auto; background: white; padding: 30px; border-radius: 10px; box-shadow: 0 0 10px rgba(0,0,0,0.1); height: calc(100vh + 100px); margin-top: 100px; height:auto;margin-bottom: 20px}
        .container h2{font-weight:bold; text-shadow:none;}
        .form-group { margin-bottom: 15px; }
        label { display: block; margin-bottom: 5px; font-weight: bold; }
        .required { color: red; }
        input[type="text"], input[type="email"], input[type="number"], select, textarea {
            width: 100%; padding: 8px; border: 1px solid #ddd; border-radius: 4px; box-sizing: border-box;
        }
        .address-group { display: grid; grid-template-columns: 1fr 1fr 1fr; gap: 10px; }
        .radio-group, .checkbox-group { display: flex; gap: 15px; }
        .radio-group input, .checkbox-group input { width: auto; margin-right: 5px; }
        .btn { background-color: #007bff; color: white; padding: 12px 24px; border: none; border-radius: 4px; cursor: pointer; font-size: 16px; }
        .btn:hover { background-color: #0056b3; }
        .summary-box { background-color: #f8f9fa; border: 1px solid #dee2e6; padding: 15px; border-radius: 5px; margin: 20px 0; }
        .error { color: red; margin-top: 5px; }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="background" runat="server">
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="content" runat="server">
    <form id="form1" runat="server">
        <div class="container">
            <h2>学生注册表单 / Student Registration Form</h2>
            
            <div class="form-group">
                <label>学生姓名（中）<span class="required">*</span></label>
                <asp:TextBox ID="txtChineseName" runat="server" required="true"></asp:TextBox>
                <asp:RequiredFieldValidator ID="rfvChineseName" runat="server" ControlToValidate="txtChineseName" ErrorMessage="请输入中文姓名" CssClass="error"></asp:RequiredFieldValidator>
            </div>

            <div class="form-group">
                <label>学生姓名（英）<span class="required">*</span></label>
                <asp:TextBox ID="txtEnglishName" runat="server" required="true"></asp:TextBox>
                <asp:RequiredFieldValidator ID="rfvEnglishName" runat="server" ControlToValidate="txtEnglishName" ErrorMessage="Please enter English name" CssClass="error"></asp:RequiredFieldValidator>
            </div>
            <div class="form-group">
                <label>性别 / Gender <span class="required">*</span></label>
                <div class="radio-group">
                    <asp:RadioButton ID="rbMale" runat="server" GroupName="Gender" Text="男/Male" value="Male" />
                    <asp:RadioButton ID="rbFemale" runat="server" GroupName="Gender" Text="女/Female" value="Female" />
                </div>
            </div>

            <div class="form-group">
                <label>年龄 / Age <span class="required">*</span></label>
                <asp:TextBox ID="txtAge" runat="server" TextMode="Number" required="true"></asp:TextBox>
                <asp:RequiredFieldValidator ID="rfvAge" runat="server" ControlToValidate="txtAge" ErrorMessage="请输入年龄" CssClass="error"></asp:RequiredFieldValidator>
            </div>

            <div class="form-group">
                <label>身份证号码 / ID Number <span class="required">*</span></label>
                <asp:TextBox ID="txtIDNumber" runat="server" required="true"></asp:TextBox>
                <asp:RequiredFieldValidator ID="rfvIDNumber" runat="server" ControlToValidate="txtIDNumber" ErrorMessage="请输入身份证号码" CssClass="error"></asp:RequiredFieldValidator>
            </div>

            <div class="form-group">
                <label>地址 / Address <span class="required">*</span></label>
                <asp:TextBox ID="txtAddress" runat="server" required="true"></asp:TextBox>
                <div class="address-group" style="margin-top: 10px;">
                    <div>
                        <label>City <span class="required">*</span></label>
                        <asp:TextBox ID="txtCity" runat="server" required="true"></asp:TextBox>
                    </div>
                    <div>
                        <label>State/Province <span class="required">*</span></label>
                        <asp:TextBox ID="txtState" runat="server" required="true"></asp:TextBox>
                    </div>
                    <div>
                        <label>Zip/Postal <span class="required">*</span></label>
                        <asp:TextBox ID="txtZip" runat="server" required="true"></asp:TextBox>
                    </div>
                </div>
            </div>

            <div class="form-group">
                <label>学校 / School <span class="required">*</span></label>
                <asp:TextBox ID="txtSchool" runat="server" required="true"></asp:TextBox>
                <asp:RequiredFieldValidator ID="rfvSchool" runat="server" ControlToValidate="txtSchool" ErrorMessage="请输入学校名称" CssClass="error"></asp:RequiredFieldValidator>
            </div>

            <div class="form-group">
                <label>电邮 / Email <span class="required">*</span></label>
                <asp:TextBox ID="txtEmail" runat="server" TextMode="Email" required="true"></asp:TextBox>
                <asp:RequiredFieldValidator ID="rfvEmail" runat="server" ControlToValidate="txtEmail" ErrorMessage="请输入邮箱地址" CssClass="error"></asp:RequiredFieldValidator>
                <asp:RegularExpressionValidator ID="revEmail" runat="server" ControlToValidate="txtEmail" ErrorMessage="请输入有效的邮箱地址" ValidationExpression="\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*" CssClass="error"></asp:RegularExpressionValidator>
            </div>

            <div class="form-group">
                <label>联络电话 / Contact Phone <span class="required">*</span></label>
                <asp:TextBox ID="txtPhone" runat="server" required="true"></asp:TextBox>
                <asp:RequiredFieldValidator ID="rfvPhone" runat="server" ControlToValidate="txtPhone" ErrorMessage="请输入联络电话" CssClass="error"></asp:RequiredFieldValidator>
            </div>

            <div class="form-group">
                <label>营衣尺寸 / Uniform Size <span class="required">*</span></label>
                <asp:DropDownList ID="ddlUniformSize" runat="server" required="true">
                    <asp:ListItem Text="请选择 / Please Select" Value=""></asp:ListItem>
                    <asp:ListItem Text="S" Value="S"></asp:ListItem>
                    <asp:ListItem Text="M" Value="M"></asp:ListItem>
                    <asp:ListItem Text="L" Value="L"></asp:ListItem>
                    <asp:ListItem Text="XL" Value="XL"></asp:ListItem>
                    <asp:ListItem Text="XXL" Value="XXL"></asp:ListItem>
                </asp:DropDownList>
            </div>

            <div class="form-group">
                <label>饮食 / Dietary Preference <span class="required">*</span></label>
                <div class="radio-group">
                    <asp:RadioButton ID="rbOmnivore" runat="server" GroupName="Diet" Text="荤食/Omnivore" value="Omnivore" />
                    <asp:RadioButton ID="rbVegetarian" runat="server" GroupName="Diet" Text="素食/Vegetarian" value="Vegetarian" />
                    <asp:RadioButton ID="rbHalal" runat="server" GroupName="Diet" Text="清真/Halal" value="Halal" />
                </div>
            </div>

            <div class="form-group">
                <label>食物敏感（如有）/ Food Allergies (if any)</label>
                <asp:TextBox ID="txtFoodAllergies" runat="server" TextMode="MultiLine" Rows="3"></asp:TextBox>
            </div>

            <div class="form-group">
                <label>疾病 (如有) / Medical Conditions (if any)</label>
                <asp:TextBox ID="txtMedicalConditions" runat="server" TextMode="MultiLine" Rows="3"></asp:TextBox>
            </div>

            <div class="form-group">
                <label>紧急联络人姓名 / Emergency Contact Name <span class="required">*</span></label>
                <asp:TextBox ID="txtEmergencyName" runat="server" required="true"></asp:TextBox>
                <asp:RequiredFieldValidator ID="rfvEmergencyName" runat="server" ControlToValidate="txtEmergencyName" ErrorMessage="请输入紧急联络人姓名" CssClass="error"></asp:RequiredFieldValidator>
            </div>

            <div class="form-group">
                <label>紧急联络人联络号码 / Emergency Contact Phone <span class="required">*</span></label>
                <asp:TextBox ID="txtEmergencyPhone" runat="server" required="true"></asp:TextBox>
                <asp:RequiredFieldValidator ID="rfvEmergencyPhone" runat="server" ControlToValidate="txtEmergencyPhone" ErrorMessage="请输入紧急联络人电话" CssClass="error"></asp:RequiredFieldValidator>
            </div>

            <div class="form-group">
                <label>紧急联络人关系 / Emergency Contact Relationship <span class="required">*</span></label>
                <asp:TextBox ID="txtEmergencyRelation" runat="server" required="true"></asp:TextBox>
                <asp:RequiredFieldValidator ID="rfvEmergencyRelation" runat="server" ControlToValidate="txtEmergencyRelation" ErrorMessage="请输入与紧急联络人的关系" CssClass="error"></asp:RequiredFieldValidator>
            </div>

            <div class="summary-box">
                <h3>Registration Details / 报名详情</h3>
                <p><strong>Registration Fee / 营费: $100.00</strong><br />营费包括Tshirt、证书以及膳食</p>
            </div>

            <div class="form-group">
                <asp:Button ID="btnSubmit" runat="server" Text="Proceed to Payment / 进行付款" CssClass="btn" OnClick="btnSubmit_Click" />
            </div>

            <asp:HiddenField ID="hfPaymentID" runat="server" />
            <asp:Label ID="lblMessage" runat="server" CssClass="error"></asp:Label>
        </div>
    </form>
</asp:Content>
