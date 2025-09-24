<%@ Page Title="" Language="C#" MasterPageFile="~/Portfolio_Pages/Portfolio.Master" AutoEventWireup="true" CodeBehind="About.aspx.cs" Inherits="PGY.WebForm1" %>

<asp:Content ContentPlaceHolderID="headContent" runat="server">
    <title>关于我们</title>
    <style>
        .bg-container {
            position: fixed;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            z-index: -10; /* Very low z-index to ensure it stays behind everything */
            overflow: hidden;
        }
        
        .bg-image {
            width: 100%;
            height: 100%;
            object-fit: cover; /* Ensures image covers entire area without distortion */
        }
        
        .overlay {
            position: absolute;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            background: linear-gradient(to bottom, rgba(0, 0, 0, 0.7), rgba(0, 0, 0, 0.3));
            z-index: -1;
        }
        
        /* Social Media Button Styles */
        .social-buttons {
            display: flex;
            flex-direction: column;
            gap: 15px;
            max-width: 400px;
            margin: 0 auto;
        }
        
        .social-btn {
            display: flex;
            align-items: center;
            padding: 12px 20px;
            border-radius: 8px;
            text-decoration: none;
            color: white;
            font-weight: 600;
            transition: all 0.3s ease;
            box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
        }
        
        .social-btn:hover {
            transform: translateY(-3px);
            box-shadow: 0 6px 12px rgba(0, 0, 0, 0.15);
        }
        
        .social-icon {
            width: 10%;
            height: auto;
        }

        .socialTxt{
            margin-left:10%;
        }
        
        .facebook {
            background-color: #3b5998;
        }
        
        .instagram {
            background: linear-gradient(45deg, #f09433, #e6683c, #dc2743, #cc2366, #bc1888);
        }
        
        .youtube {
            background-color: #c65c5c;
        }
        
        .tiktok {
            background-color: #000000;
        }
        
        .xiaohongshu {
            background-color: #a20a0a;
        }
        
        /* Content styling */
        .content-section p {
            margin-bottom: 20px;
            line-height: 1.8;
            text-align: justify;
        }
        
        /* Responsive adjustments */
        @media (max-width: 768px) {
            .social-buttons {
                width: 100%;
            }
            
            .social-btn {
                padding: 10px 15px;
            }
        }
    </style>
</asp:Content>

<asp:Content ContentPlaceholderID="background" runat="server">
<%--<div class="video-background">
    <video autoplay muted loop playsinline>
        <source src="#" type="video/mp4">
    </video>
</div>--%>
</asp:Content>

<asp:Content ContentPlaceholderID="content" runat="server">
    <!-- Scroll Indicators -->
    <div class="scroll-indicators">
        <div class="scroll-indicator active" data-section="0"></div>
        <div class="scroll-indicator" data-section="1"></div>
        <div class="scroll-indicator" data-section="2"></div>
        <div class="scroll-indicator" data-section="3"></div>
    </div>
    <!-- Main Content -->
    <div class="main-content">
        <div class="bg-container">
            <img class="bg-image" src="images/aboutBG0.jpg">
        </div>
        <!-- About PGY Section -->
        <section id="about-pgy" class="section" data-section="0">
            <div class="content-section fade-in">
                <h3>什么是蒲公英</h3>
                <p>蒲公英是一个通过办营会的方式、通过一系列精心策划的培训课程和实习活动，以中华历史和经典为营会主题，中华艺术为辅，贯穿于营会的每一项活动，再结合生命教育，提供年轻人一个学习成长、累积实战经验、为自己和他人创造价值的平台。</p>
                
                <p>蒲公英的缘起是珍贵的偶然。2002年，三两位年轻人因为热爱中华文化，想通过办营会的方式来让更多年轻人轻松学习自身的文化而开始。几经波折，绝境逢生，于2011年在没有任何资源的援助下自力更生，凭着多年来的口碑和工委团极力地护航，正式命名为蒲公英文教工作坊，简称蒲公英，重新出发。</p>
            </div>
        </section>

        <!-- Name Origin Section -->
        <section id="name-origin" class="section" data-section="1">
            <div class="content-section fade-in">
                <h3>蒲公英名字由来</h3>
                <p>蒲公英"花罢成絮，因风飞扬，落湿地即生"，生命力颇强。生命力强来自于它的根扎得很深，虽然在地面上的蒲公英只有10至25公分。蒲公英"扎根、成长、飞扬"的三种生命状态，非常符合本组织的精神、思想和方向。因此，蒲公英文教工作坊以"深扎文化之根，远播希望之苗"作为理念，贯彻于各项活动及发展工作，同时学习自身的文化，以自己的文化作为根基，继而站在自己文化的基础上，吸收外来优良的文化，让生命绽放异彩。</p>
            </div>
        </section>

        <!-- History Section -->
        <section id="history" class="section" data-section="2">
            <div class="content-section fade-in">
                <h3>蒲公英如何延续到今天</h3>
                <p>蒲公英之所以能够延续到今天是因为爱，爱这个大家庭，爱大家一起走过的日子，爱学员们因成长带来的感动，彼此都希望像蒲公英的花语一样"永不停止的爱"。</p>
                
                <p>为了让组织进入永续经营的模式、让组织活动更为系统化、以及为成员们提供更多的培训机会，蒲公英于2012年年底，分别在北马、霹雳、金马伦及雪隆主办了第一届蒲公英分区培训营。</p>
                
                <p>基于蒲公英分区营这模式能够让学员们更有效和更全面地提升他们的领导能力，积累实战的经验，蒲公英于2013年年底，从吉隆坡的一个点，扩展至全国八大营区。蒲公英以往一年只办一次的全国蒲公英干部培训，也增至一年分成三个阶段的MENTOR SYSTEM和HANDS ON TRAINING培训模式，即RCT蒲公英区域干部培训、CNT全国蒲公英干部培训和REC小小蒲公英分区营。学员层次也因此涵盖了大专生、中学生和小学生。</p>
                                
                <p>2020年，新冠肺炎爆发，导致线下活动无法进行，但这并不阻碍蒲公英继续前进的步伐。蒲公英顺时代而改变，把过去18年来办线下营会的经验与精髓转到线上，精心策划了 "e代领袖" 线上培训课程，并借助网络的力量惠及全马各州的学生。</p>
                
            </div>
        </section>


        <!-- Social Media Section -->
        <section id="social-media" class="section" data-section="3">
            <div class="content-section fade-in">
                <h3>关注蒲公英社交媒体</h3>
                <div class="social-buttons">
                    <a href="https://www.facebook.com/share/1Cf2n8g2nS/" class="social-btn facebook" target="_blank">
                        <img src="images/socialIcon/logo_fb.png" class="social-icon" />
                        <span class="socialTxt">蒲公英 Facebook</span>
                    </a>
                    <a href="https://www.instagram.com/pugongyingig?igsh=MWp3aDlqbmVhZjc2ZQ==" class="social-btn instagram" target="_blank">
                        <img src="images/socialIcon/logo_ig.png" class="social-icon" />
                        <span class="socialTxt">蒲公英 Instagram</span>
                    </a>
                    <a href="https://youtube.com/@pgy" class="social-btn youtube" target="_blank">
                        <img src="images/socialIcon/logo_yt.png" class="social-icon" />
                        <span class="socialTxt">蒲公英 Youtube</span>
                    </a>
                    <a href="https://www.douyin.com/user/MS4wLjABAAAAiK-ygxUe-VPyL3_xbWYEGCNtSssrPtrBXwIevc8L6039n0g5zAv1WDqSyLQa6LEL" class="social-btn tiktok" target="_blank">
                        <img src="images/socialIcon/logo_dy.png" class="social-icon" />
                        <span class="socialTxt">蒲公英 抖音</span>
                    </a>
                    <a href="https://www.xiaohongshu.com/user/profile/63f78c31000000001001f903?xhsshare=CopyLink&appuid=63f78c31000000001001f903&apptime=1678521685" class="social-btn xiaohongshu" target="_blank">
                        <img src="images/socialIcon/logo_xhs.png" class="social-icon" />
                        <span class="socialTxt">蒲公英 小红书</span>
                    </a>
                </div>
            </div>
        </section>
    </div>
</asp:Content>