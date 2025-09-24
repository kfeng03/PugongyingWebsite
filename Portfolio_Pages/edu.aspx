<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="edu.aspx.cs" Inherits="PGY.edu" MasterPageFile="~/Portfolio_Pages/Portfolio.Master" %>
<asp:Content ContentPlaceholderID="headContent" runat="server">

    <title>升学辅导</title>
    <style>
    
    .video-wrapper {
        position: relative;
        width: 100%;
        padding-bottom: 40%; /* 16:9 aspect ratio */
        border-radius: 15px;
        overflow: hidden;
        box-shadow: 0 10px 30px rgba(0, 0, 0, 0.2);
    }

    .video-wrapper iframe {
        position: absolute;
        top: 0;
        left: 0;
        width: 100%;
        height: 100%;
    }
    /* University carousel styling */
    .university-list {
        margin: 2rem 0;
        height: 300px; /* Fixed height to control space */
        overflow: hidden;
        position: relative;
        background: rgba(102, 126, 234, 0.05);
        border-radius: 15px;
        padding: 1rem;
    }

    .university-carousel {
        display: flex;
        flex-direction: column;
        animation: verticalScroll 20s linear infinite;
        gap: 0.5rem;
    }

    .university-carousel:hover {
        animation-play-state: paused;
    }

    .university-list ul {
        list-style: none;
        padding: 0;
        margin: 0;
        display: flex;
        flex-direction: column;
        gap: 0.5rem;
    }

    .university-list li {
        background: rgba(102, 126, 234, 0.1);
        padding: 1rem;
        border-radius: 10px;
        border-left: 4px solid #808080;
        font-size: 1rem;
        color: #ffff;
        min-height: 60px;
        display: flex;
        align-items: center;
        white-space: nowrap;
        flex-shrink: 0;
    }

    .university-list li:before {
        content: "🏛️";
        margin-right: 0.5rem;
        flex-shrink: 0;
    }

    /* Keyframes for vertical scrolling animation */
    @keyframes verticalScroll {
        0% {
            transform: translateY(0);
        }
        100% {
            transform: translateY(-50.25%);
        }
    }

    @keyframes fadeInOut {
        0%, 15% { opacity: 0; transform: translateY(-50%) translateX(20px); }
        20%, 80% { opacity: 1; transform: translateY(-50%) translateX(0); }
        85%, 100% { opacity: 0; transform: translateY(-50%) translateX(-20px); }
    }

    /* Success stories */
    .success-stories {
        display: grid;
        grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
        gap: 2rem;
        margin: 2rem 0;
    }

    .success-item {
        color: white;
        border-radius: 15px;
        text-align: center;
    }
    
        .success-item img {
            width: 200px;
            height: 200px;
            border-radius: 50%;
            object-fit: cover;
            margin-bottom: 1rem;
            box-shadow: 0 5px 15px rgba(0, 0, 0, 0.2);
        }

        .success-item h4 {
            font-size: 1.3rem;
            margin-bottom: 0.5rem;
        }

        .success-item p {
            font-size: 1rem;
            opacity: 0.9;
            text-align: center;
        }

    /* Contact info styling */
    .contact-info {
        background: rgba(102, 126, 234, 0.1);
        padding-left: 5%;
        border-radius: 15px;
    }

    .contact-section {
        margin: 1.5rem 0;
    }

        .contact-section h4 {
            color: #ffff;
            font-size: 1.3rem;
            margin-bottom: 1rem;
        }

        .contact-section p {
            margin: 0.5rem 0;
            font-size: 1.1rem;
        }

        .contact-section a {
            color: #667eea;
            text-decoration: none;
        }

            .contact-section a:hover {
                text-decoration: underline;
            }

    @media (max-width: 768px) {
        .success-stories {
            grid-template-columns: 1fr;
        }
    }

    
    /* Mobile responsiveness */
    @media (max-width: 768px) {
        .university-list {
            height: 250px;
            padding: 0.5rem;
        }
    
        .university-list li {
            font-size: 0.9rem;
            padding: 0.8rem;
            white-space: normal;
            min-height: 50px;
        }
    
        .fade-carousel {
            height: 100px;
        }
    }
    </style>
</asp:Content>

<asp:Content ContentPlaceHolderID="background" runat="server">
    <!-- Video Background -->
    <div class="video-background">
        <video autoplay muted loop playsinline>
            <source src="videos/edu.mp4" type="video/mp4">
        </video>
    </div>
</asp:Content>

<asp:Content ContentPlaceHolderID="content" runat="server">
     
     <!-- Scroll Indicators -->
     <div class="scroll-indicators">
         <div class="scroll-indicator active" data-section="0"></div>
         <div class="scroll-indicator" data-section="1"></div>
         <div class="scroll-indicator" data-section="2"></div>
         <div class="scroll-indicator" data-section="3"></div>
         <div class="scroll-indicator" data-section="4"></div>
         <div class="scroll-indicator" data-section="5"></div>
         <div class="scroll-indicator" data-section="6"></div>
     </div>

    <!-- Main Content -->
    <div class="main-content">
        <!-- Hero Section -->
        <section id="home" class="section"  data-section="0">
            <div class="hero fade-in">
                <h1>升学</h1>
                <h2>寰宇专才基地</h2>
                <h3>✕</h3>
                <h2>蒲公英文教工作坊</h2>
                <h4>赴华留学不是梦！</h4>
            </div>
        </section>

        <!-- Partnership Introduction -->
        <section id="partnership" class="section"  data-section="1">
            <div class="content-section fade-in">
                <h3>赴华留学不是梦！</h3>
                <p>寰宇专才基地与蒲公英文教工作坊强强联手，赴华留学不是梦！马来西亚中国一带一路促进会（寰宇专才基地）与蒲公英文教工作坊于2022年6月5日签署关于文化教育交流的合作谅解备忘录，在进一步促进中华文化，拓展母语教育的项目上展开紧密合作。</p>
            </div>
        </section>

        <!-- Student Testimonial Video -->
        <section id="testimonial" class="section" data-section="2">
            <div class="content-section fade-in">
                <h3>2023年寰宇预科班学生感想视频</h3>
                <p>听听我们学生的真实感受，了解他们在赴华留学路上的成长与收获。</p>
                <div class="video-grid">
                    <div class="video-wrapper" >
                        <iframe src="https://www.youtube.com/embed/HUXggEG1n3E" frameborder="0" allowfullscreen></iframe>
                    </div>
                </div>
            </div>
        </section>

        <!-- About Huanyu -->
        <section id="about-huanyu" class="section"  data-section="3">
            <div class="content-section fade-in">
                <h3>寰宇专才基地</h3>
                <p>马来西亚寰宇专才基地（Worldwide Expertise Centre）成立于2019年，旨在推动马来西亚与中国之间的教育、培训和学术交流合作。该机构致力于帮助学生赴中国留学，处理并成功办理了超过400份中国大学入学申请，同时组织了超过30所中国大专院校的互访考察活动。</p>
                <p>此外，该机构通过与中国顶尖大学的合作关系，为马来西亚学生提供更多选择，使他们有机会获得更高质量的国际教育。</p>
            </div>
        </section>

        <section id="universities" class="section" data-section="4">
            <div class="content-section fade-in">
                <h3>合作大学</h3>
                <p>寰宇专才基地得到了以下中国顶尖大学的官方认可和授权：</p>
        
                <!-- Option 1: Scrolling Carousel -->
                <div class="university-list">
                    <div class="university-carousel">
                        <ul>
                            <li>华南理工大学国际教育学院国际学生生源基地</li>
                            <li>北京理工大学国际学生生源基地</li>
                            <li>南开大学国际教育学院马来西亚代表处</li>
                            <li>东北林业大学国际交流学院驻马来西亚代表处</li>
                            <li>南方科技大学马来西亚授权宣传机构</li>
                            <li>天津大学国际教育学院马来西亚授权宣传机构</li>
                            <li>上海大学国际教育学院马来西亚指定招生机构</li>
                            <li>华东理工大学国际教育学院马来西亚指定招生机构</li>
                            <li>西安交通大学马来西亚国际学生生源基地</li>
                        </ul>
                        <!-- Duplicate for seamless loop -->
                        <ul>
                            <li>华南理工大学国际教育学院国际学生生源基地</li>
                            <li>北京理工大学国际学生生源基地</li>
                            <li>南开大学国际教育学院马来西亚代表处</li>
                            <li>东北林业大学国际交流学院驻马来西亚代表处</li>
                            <li>南方科技大学马来西亚授权宣传机构</li>
                            <li>天津大学国际教育学院马来西亚授权宣传机构</li>
                            <li>上海大学国际教育学院马来西亚指定招生机构</li>
                            <li>华东理工大学国际教育学院马来西亚指定招生机构</li>
                            <li>西安交通大学马来西亚国际学生生源基地</li>
                        </ul>
                    </div>
                </div>

            </div>
        </section>

        <!-- Success Stories -->
        <section id="success" class="section" data-section="5">
            <div class="content-section fade-in">
                <h3>成功案例</h3>
                <div class="success-stories">
                    <div class="success-item">
                        <img src="images/ARP/TBI.png" alt="Award Winner">
                        <h4>张闻育</h4>
                        <p>目前就读南开大学</p>
                    </div>
                    <div class="success-item">
                        <img src="images/ARP/TLY.png" alt="Award Winner">
                        <h4>陈力扬</h4>
                        <p>目前就读南方科技大学</p>
                    </div>
                    <div class="success-item">
                        <img src="images/ARP/TBH.png" alt="Award Winner">
                        <h4>张闻希</h4>
                        <p>目前就读南方科技大学</p>
                    </div>
                </div>
                <p>这些蒲公英家人已经在中国顶尖大学开始了他们的学术旅程，他们的成功证明了通过我们的合作项目，赴华留学的梦想完全可以实现。</p>
            </div>
        </section>

        <!-- Contact Information -->
        <section id="contact-info" class="section" data-section="6">
            <div class="content-section fade-in">
                <h3>了解更多</h3>
                <div class="contact-info">
                    <div class="contact-section">
                        <p><b>蒲公英升学署</b>
                        <br>Email: <a href="mailto:pgy.edu.2302@gmail.com">pgy.edu.2302@gmail.com</a></p>
                        <p><b>寰宇专才基地</b>
                        <br>电话/WhatsApp: <a href="tel:+60168277001">+6016-8277001</a>
                        <br>WeChat: worldwide_expertise
                        <br>Email: <a href="mailto:worldwide.expertise@gmail.com">worldwide.expertise@gmail.com</a>
                        <br>Website: <a href="https://www.huanyu.com.my/" target="_blank">https://www.huanyu.com.my/</a>
                        <br>Facebook: <a href="https://www.facebook.com/worldwide.expertise" target="_blank">@worldwide.expertise</a>
                        <br>Instagram: <a href="https://www.instagram.com/worldwide.expertise/" target="_blank">@worldwide.expertise</a>
                        <br>YouTube: <a href="https://www.youtube.com/@worldwide_huanyu" target="_blank">@worldwide_huanyu</a>
                        </br>
                    </div>
                </div>
                <a href="mailto:pgy.edu.2302@gmail.com" class="btn">立即咨询升学事宜</a>
            </div>
        </section>
    </div>

</asp:Content>
