<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Homepage.aspx.cs" Inherits="PGY.Homepage" MasterPageFile="~/Portfolio_Pages/Portfolio.Master" %>

<asp:Content ContentPlaceHolderID="headContent" runat="server">
    <title>蒲公英文教工作坊</title>
    <style>
        .full-div{
            display: flex; /* Makes the container a flex container */
            flex-direction: row; /* Default to row layout */
        }

        .left-div {
            flex: 2; /* Takes 2 units of available space */
        }
        .thumbnail{
            width:100%;
            padding-top:10%;
            padding-bottom:10%;
        }
        .right-div {
            flex: 1; /* Takes 1 unit of available space */
        }
        
        /* Media query for mobile devices */
        @media (max-width: 768px) {
            .full-div {
                flex-direction: column; /* Stack vertically on mobile */
            }
            
            .left-div, .right-div {
                flex: 1; /* Reset flex to equal distribution */
                width: 100%; /* Full width for both divs */
            }
            
            .thumbnail {
                padding-top: 5%;
                padding-bottom: 5%;
            }
        }
    </style>
</asp:Content>
<asp:Content ContentPlaceholderID="background" runat="server">
<div class="video-background">
    <video autoplay muted loop playsinline>
        <source src="videos/background_home-1.mp4" type="video/mp4">
    </video>
</div>
</asp:Content>

<asp:Content ContentPlaceholderID="content" runat="server">
    <!-- Scroll Indicators -->
    <div class="scroll-indicators">
        <div class="scroll-indicator active" data-section="0"></div>
        <div class="scroll-indicator" data-section="1"></div>
        <div class="scroll-indicator" data-section="2"></div>
        <div class="scroll-indicator" data-section="3"></div>
        <div class="scroll-indicator" data-section="4"></div>
    </div>
    <!-- Main Content -->
    <div class="main-content">
        <!-- Hero Section -->
        <section id="home" class="section" data-section="0">
            <div class="hero fade-in">
                <h1 class="fade-in" style="text-align:center;">蒲公英文教工作坊</h1>
                <h2 class="fade-in">一个理念 三大精神 三大价值观</h2>
            </div>
        </section>

        <!-- About Section -->
        <section id="about" class="section" data-section="1">
            <div class="content-section fade-in">
                <h3>什么是蒲公英？</h3>
                <div class="full-div">
                <div class="left-div">
                    <div class="value-item">
                        <h4>我们的理念</h4>
                        <p style="text-align:center;">深扎文化之根，远播希望之苗</p>
                        <h4>我们的精神</h4>
                        <p style="text-align:center;">扎根、成长、飞扬</p>
                        <h4>我们的价值观</h4>
                        <p style="text-align:center;">正面、积极、承担</p>
                    </div>
                </div>
                <div class="right-div"><img class="thumbnail" src="images/Home/Home_1.png" /></div>
                </div>
                <a href="About.aspx" class="btn">前往关于我们</a>
            </div>
        </section>

        <!-- E-Leader Section -->
        <section id="e-leader"  class="section" data-section="2">
            <div class="content-section fade-in">
                <h3>e代领袖</h3>
                <div class="full-div">
                <div class="left-div">
                <p style="margin-right:10%;">当中华文化遇上现代科技，到底会迸发出怎样的战火？<br />
                我们的全国领袖培训课程——《e代领袖》让你掌握领导能力、文化素养及科技运用的3大综合竞争力：文化素养、领导能力、科技运用，让青少年除了提升自己，还可以结交到来自全马各地的青少年朋友！</p>
                </div>
            
            <div class="right-div"><img class="thumbnail" src="images/Home/Home_2.png" /></div>
            </div>
                <a href="RegistrationForm.aspx" class="btn">前往e代领袖(测试报名表格)</a>

            </div>
        </section>

        <!-- Certification Section -->
        <section id="certification"  class="section" data-section="3">
            <div class="content-section fade-in">
                <h3>卓越领袖认证</h3>
                <div class="full-div">
                <div class="left-div">
                <p style="margin-right:10%;">爱丁堡公爵国际奖 The Duke of Edinburgh's International Award (DofE) 于1956年在英国发起，是为了培训14至24岁的青少年而设计的奖励计划。自1992年起，马来西亚青年体育部便致力于推广该奖项，并称作 ARPRM 。
                <br/>蒲公英文教工作坊将结合ARPRM计划，进一步优化已有22年底蕴的蒲公英领袖培训课程，融合"以生命影响生命"的志工精神、手把手教学、以及社区服务项目，培养更多新生代领袖。</p>
                </div>
            <div class="right-div"><img class="thumbnail" src="images/Home/Home_3.png" /></div>
            </div>
                <a href="ARP.aspx" class="btn">前往卓越领袖认证</a>

            </div>
        </section>

        <!-- Guidance Section -->
        <section id="guidance"  class="section" data-section="4">
            <div class="content-section fade-in">
                <h3>升学辅导</h3>
                <div class="full-div">
                    <div class="left-div">
                        <p style="margin-right:10%;">留学世界级大学，与中国Top 1%优秀生为伴？<br />
                        寰宇专才基地与蒲公英文教工作坊展开紧密的合作，进一步促进中华文化，拓展母语教育项目的合作。
                        该机构通过与中国顶尖大学的合作关系，为马来西亚学生提供更多选择，使他们有机会获得更高质量的国际教育。</p>
                    </div>       
                    <div class="right-div"><img class="thumbnail" src="images/Home/Home_4.png" /></div>
                </div>
                        <a href="edu.aspx" class="btn">前往升学辅导</a>
            </div>
        </section>




</asp:Content>