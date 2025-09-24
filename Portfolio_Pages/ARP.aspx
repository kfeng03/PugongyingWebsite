<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ARP.aspx.cs" Inherits="PGY.ARP" MasterPageFile="~/Portfolio_Pages/Portfolio.Master" %>
<asp:Content ContentPlaceholderID="headContent" runat="server">
    <title>卓越领袖认证</title>
    <style>
        .video-grid {
            display: grid;
            grid-template-columns: repeat(2, 1fr); /* 2 videos side by side */
            gap: 2rem;
            margin: 2rem 0;
            max-width: 100%;
        }

        .video-wrapper {
            position: relative;
            width: 100%;
            padding-bottom: 56.25%; /* 16:9 aspect ratio */
            height: 0;
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

        /* Awards carousel */
        .awards-section {
            text-align: center;
        }

        .carousel-container {
            margin: 2rem 0;
            overflow: hidden;
            border-radius: 15px;
            padding: 2rem 0;
        }

        .carousel-track {
            display: flex;
            animation: scroll 20s linear infinite;
            gap: 2rem;
        }

        .award-item {
            min-width: 200px;
            text-align: center;
        }

        .award-item img {
            width: 200px;
            height: 200px;
            border-radius: 50%;
            object-fit: cover;
            margin-bottom: 1rem;
            box-shadow: 0 5px 15px rgba(0, 0, 0, 0.2);
        }

        .award-item h4 {
            color: #fff;
            font-size: 1.1rem;
            font-weight: 500;
        }

        @keyframes scroll {
            0% {
                transform: translateX(0);
            }
            100% {
                transform: translateX(-106.5%); /*adjust here to get seemless animation for scroll*/
            }
        }

        /* Key activities highlight */
        .activities {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
            gap: 1.5rem;
            margin: 2rem 0;
        }

        .activity-item {
            text-align: center;
            padding: 1.5rem;
            background: rgba(102, 126, 234, 0.1);
            border-radius: 15px;
            border: 2px solid rgba(102, 126, 234, 0.2);
        }

        .activity-item h4 {
            color: white;
            font-size: 1.2rem;
            margin-bottom: 0.5rem;
        }

        /* Responsive Design */
        @media (max-width: 768px) {
            
            .carousel-track {
                display: flex;
                animation: scroll 12s linear infinite;
                gap: 0rem;
            }

            @keyframes scroll {
                0% {
                    transform: translateX(0);
                }
                100% {
                    transform: translateX(-340%); /*adjust here to get seemless animation for scroll*/
                }
            }

            .content-section {
                padding: 2rem;
                margin: 1rem;
                min-height: 60vh;
            }

            .content-section h3 {
                font-size: 2rem;
            }

            .video-grid {
                grid-template-columns: 1fr;
                gap: 1rem;
            }

            .video-wrapper {
                padding-bottom: 56.25%;
            }

            .section {
                padding: 1rem;
            }
        }

        @media (max-width: 480px) {
            .hero h1 {
                font-size: 2rem;
            }

            .content-section {
                padding: 1.5rem;
                min-height: 50vh;
            }

            .content-section h3 {
                font-size: 1.8rem;
            }

            .section {
                height: auto;
                min-height: 100vh;
            }

            .award-item {
                min-width: 250px;
            }

            .award-item img {
                width: 200px;
                height: 200px;
            }

            nav {
                padding: 1rem;
            }
        }

        /* Smooth scrolling */
        html {
            scroll-behavior: smooth;
        }

        /* Loading animation */
        .fade-in {
            animation: fadeInUp 0.8s ease forwards;
        }

        @keyframes fadeInUp {
            from {
                opacity: 0;
                transform: translateY(30px);
            }
            to {
                opacity: 1;
                transform: translateY(0);
            }
        }
    </style>
</asp:Content>
<asp:Content ContentPlaceholderID="background" runat="server">
    <!-- Video Background -->
    <div class="video-background">
        <video autoplay muted loop playsinline>
            <source src="videos/arp-1.mp4" type="video/mp4">
        </video>
    </div>
</asp:Content>

     
<asp:Content ContentPlaceholderID="content" runat="server">

     <div class="scroll-indicators">
         <div class="scroll-indicator active" data-section="0"></div>
         <div class="scroll-indicator" data-section="1"></div>
         <div class="scroll-indicator" data-section="2"></div>
         <div class="scroll-indicator" data-section="3"></div>
         <div class="scroll-indicator" data-section="4"></div>
         <div class="scroll-indicator" data-section="5"></div>
     </div>

    <!-- Main Content -->
    <div class="main-content" >
        <!-- Hero Section -->
        <section id="home" class="section" data-section="0">
            <div class="hero fade-in">
                <h1>ARPRM</h1>
                <h2>卓越领袖认证</h2>
                <h3>Anugerah Perdana Remaja Rakan Muda</h3>
            </div>
        </section>

        <!-- What is ARPRM Section -->
        <section id="about-arprm" class="section" data-section="1">
            <div class="content-section fade-in">
                <h3>什么是ARPRM？</h3>
                <p>Anugerah Remaja Perdana Rakan Muda (ARPRM) 是马来西亚青年体育部所推动的一项国际青年领袖奖励计划，其分为金银铜三种等级。该奖项与【爱丁堡公爵的国际奖（The Duke of Edinburgh International Award, DofE）】或现在更广为人知的【国际青年奖】一起获得。</p>
                <p>它于 1956 年在英国发起，至今在全球已有 140 个国家参与这项计划，旨在通过一系列挑战性的活动，培养青少年的领导力、团队协作、创新思维等多方面的素养。</p>
                <div class="activities">
                    <div class="activity-item">
                        <h4>社区服务</h4>
                        <p>服务社区，回馈社会</p>
                    </div>
                    <div class="activity-item">
                        <h4>技能培训</h4>
                        <p>培养实用技能</p>
                    </div>
                    <div class="activity-item">
                        <h4>体育活动</h4>
                        <p>强健体魄，锻炼毅力</p>
                    </div>
                    <div class="activity-item">
                        <h4>野外探险</h4>
                        <p>挑战自我，探索自然</p>
                    </div>
                </div>
            </div>
        </section>

        <!-- Partnership Section -->
        <section id="partnership" class="section" data-section="2">
            <div class="content-section fade-in">
                <h3>蒲公英文教工作坊与ARPRM</h3>
                <p>蒲公英文教工作坊将结合ARPRM计划，进一步优化已有21年底蕴的蒲公英领袖培训课程，融合"以生命影响生命"的志工精神、手把手教学、以及社区服务项目，培养更多新生代领袖。</p>
                <p>我们的综合培训方案结合传统文化教育与现代领导力培养，为青少年提供全方位的成长平台，让每位参与者都能在服务中成长，在挑战中蜕变。</p>
            </div>
        </section>

        <!-- Video Review Section -->
        <section id="videos" class="section" data-section="3">
            <div class="content-section fade-in">
                <h3>回顾短片</h3>
                <div class="video-grid">
                    <div class="video-wrapper">
                        <iframe src="https://www.youtube.com/embed/ufuXLti_21Q" frameborder="0" allowfullscreen></iframe>
                    </div>
                    <div class="video-wrapper">
                        <iframe src="https://www.youtube.com/embed/kC1_R7NCSZM" frameborder="0" allowfullscreen></iframe>
                    </div>
                </div>
            </div>
        </section>

        <!-- Awards Winners Section -->
        <section id="winners" class="section" data-section="4">
            <div class="content-section fade-in awards-section">
                <h3>我们的卓越领袖奖得主</h3>
                <div class="carousel-container">
                    <div class="carousel-track">
                        <!-- Duplicate items for seamless loop -->
                        <div class="award-item">
                            <img src="images/ARP/TLY.png" alt="Award Winner">
                            <h4>陈力扬</h4>
                        </div>
                        <div class="award-item">
                            <img src="images/ARP/TBH.png" alt="Award Winner">
                            <h4>张闻希</h4>
                        </div>
                        <div class="award-item">
                            <img src="images/ARP/BKF.png" alt="Award Winner">
                            <h4>马楷丰</h4>
                        </div>
                        <div class="award-item">
                            <img src="images/ARP/TBI.png" alt="Award Winner">
                            <h4>张闻育</h4>
                        </div>
                        <div class="award-item">
                            <img src="images/ARP/TLY.png" alt="Award Winner">
                            <h4>陈力扬</h4>
                        </div>
                        <div class="award-item">
                            <img src="images/ARP/TBH.png" alt="Award Winner">
                            <h4>张闻希</h4>
                        </div>
                        <div class="award-item">
                            <img src="images/ARP/BKF.png" alt="Award Winner">
                            <h4>马楷丰</h4>
                        </div>
                        <div class="award-item">
                            <img src="images/ARP/TBI.png" alt="Award Winner">
                            <h4>张闻育</h4>
                        </div>
                        
                    </div>
                </div>
            </div>
        </section>

        <!-- How to Participate Section -->
        <section id="participate" class="section" data-section="5">
            <div class="content-section fade-in">
                <h3>如何参与？</h3>
                <p>想要通过蒲公英文教工作坊参与ARPRM，首先需要成为2024年全国领袖培训课程（e代领袖）的营员或工委。随后，参与者也需要担任至少一场小小蒲公英分站营的工委。</p>
                <p>这个过程不仅让你获得宝贵的领导经验，更重要的是通过服务他人来实现自我成长，体验"以生命影响生命"的深刻意义。</p>
                <a href="#register" class="btn">报名e代领袖第四季</a>
            </div>
        </section>
    </div>

     <script>
        // Pause carousel animation on hover
        const carouselTrack = document.querySelector('.carousel-track');
        carouselTrack.addEventListener('mouseenter', () => {
            carouselTrack.style.animationPlayState = 'paused';
        });
        carouselTrack.addEventListener('mouseleave', () => {
            carouselTrack.style.animationPlayState = 'running';
        });
    </script>
</asp:Content>
