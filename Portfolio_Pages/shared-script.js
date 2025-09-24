// Reliable scroll system - Desktop/Mobile
let currentSection = 0;
let isScrolling = false;

let sections;
let indicators;
let mainContent;

// Wait for DOM to be ready before selecting elements
window.addEventListener('DOMContentLoaded', () => {
    sections = document.querySelectorAll('.section');
    indicators = document.querySelectorAll('.scroll-indicator');
    mainContent = document.querySelector('.main-content');

    if (!mainContent || sections.length === 0) {
        console.error('Main content or sections not found.');
        return;
    }

    // Initialize features only after ensuring elements exist
    initScrollSystem();
});

function isDesktop() {
    return window.innerWidth >= 1024 && !('ontouchstart' in window);
}

function updateActive(sectionIndex) {
    if (sectionIndex < 0 || sectionIndex >= sections.length) return;
    indicators.forEach((indicator, index) => {
        indicator.classList.toggle('active', index === sectionIndex);
    });
    currentSection = sectionIndex;
}

function getCurrentSectionFromScroll() {
    const windowHeight = window.innerHeight;
    let mostVisibleSection = 0;
    let maxVisibility = 0;

    sections.forEach((section, index) => {
        const rect = section.getBoundingClientRect();
        const visibleHeight = Math.max(
            0,
            Math.min(rect.bottom, windowHeight) - Math.max(rect.top, 0)
        );
        const visibility = visibleHeight / windowHeight;
        if (visibility > maxVisibility) {
            maxVisibility = visibility;
            mostVisibleSection = index;
        }
    });

    return mostVisibleSection;
}

function scrollToSection(sectionIndex) {
    if (
        sectionIndex < 0 ||
        sectionIndex >= sections.length ||
        isScrolling ||
        !mainContent
    )
        return;

    isScrolling = true;

    const targetTop = sections[sectionIndex].offsetTop;

    mainContent.scrollTo({
        top: targetTop,
        behavior: 'smooth'
    });

    updateActive(sectionIndex);

    setTimeout(() => {
        isScrolling = false;
    }, 1000);
}

function handleScroll() {
    if (isScrolling) return;
    clearTimeout(handleScroll._t);
    handleScroll._t = setTimeout(() => {
        const newSection = getCurrentSectionFromScroll();
        if (newSection !== currentSection) {
            updateActive(newSection);
        }
    }, 150);
}

function initScrollSystem() {
    // Desktop-only wheel and keyboard navigation
    if (isDesktop()) {
        let wheelDelta = 0;
        let wheelTimeout;

        mainContent.addEventListener(
            'wheel',
            (e) => {
                if (isScrolling) {
                    e.preventDefault();
                    return;
                }
                wheelDelta += e.deltaY;
                clearTimeout(wheelTimeout);
                wheelTimeout = setTimeout(() => {
                    if (Math.abs(wheelDelta) > 100) {
                        if (wheelDelta > 0 && currentSection < sections.length - 1) {
                            e.preventDefault();
                            scrollToSection(currentSection + 1);
                        } else if (wheelDelta < 0 && currentSection > 0) {
                            e.preventDefault();
                            scrollToSection(currentSection - 1);
                        }
                    }
                    wheelDelta = 0;
                }, 50);
            },
            { passive: false }
        );

        document.addEventListener('keydown', (e) => {
            if (isScrolling) return;
            switch (e.key) {
                case 'ArrowDown':
                case 'PageDown':
                    e.preventDefault();
                    if (currentSection < sections.length - 1)
                        scrollToSection(currentSection + 1);
                    break;
                case 'ArrowUp':
                case 'PageUp':
                    e.preventDefault();
                    if (currentSection > 0) scrollToSection(currentSection - 1);
                    break;
                case 'Home':
                    e.preventDefault();
                    scrollToSection(0);
                    break;
                case 'End':
                    e.preventDefault();
                    scrollToSection(sections.length - 1);
                    break;
            }
        });

        document.body.classList.add('desktop-mode');
    }

    // Scroll indicator clicks
    indicators.forEach((indicator, index) => {
        indicator.addEventListener('click', () => scrollToSection(index));
    });

    // Scroll event
    mainContent.addEventListener('scroll', handleScroll);

    // Fade-in animation observer
    const fadeObserver = new IntersectionObserver(
        (entries) => {
            entries.forEach((entry) => {
                if (entry.isIntersecting) entry.target.classList.add('fade-in');
            });
        },
        { threshold: 0.1, rootMargin: '0px 0px -50px 0px' }
    );
    document
        .querySelectorAll('.content-section')
        .forEach((el) => fadeObserver.observe(el));

    // Video error handling
    const video = document.querySelector('video');
    if (video) {
        video.addEventListener('error', function () {
            console.warn('Video failed to load, using fallback background');
            this.style.display = 'none';
        });
    }

    // Handle resize
    window.addEventListener('resize', () => {
        setTimeout(() => {
            const newSection = getCurrentSectionFromScroll();
            updateActive(newSection);
        }, 100);
    });

    // Mobile menu toggling
    const navLinks = document.getElementById('navLinks');
    document.querySelectorAll('.nav-links a').forEach((link) =>
        link.addEventListener('click', (e) => {
            if (navLinks) navLinks.classList.remove('mobile-active');
            const href = link.getAttribute('href');
            if (href && href.startsWith('#')) {
                e.preventDefault();
                const target = document.querySelector(href);
                if (target) {
                    const idx = Array.from(sections).indexOf(target);
                    if (idx !== -1) scrollToSection(idx);
                }
            }
        })
    );

    document.addEventListener('click', (e) => {
        const nav = document.querySelector('nav');
        if (navLinks && nav && !nav.contains(e.target)) {
            navLinks.classList.remove('mobile-active');
        }
    });

    // Anchor links
    document.querySelectorAll('a[href^="#"]').forEach((anchor) => {
        anchor.addEventListener('click', function (e) {
            const href = this.getAttribute('href');
            if (href === '#' || href === '') return;
            e.preventDefault();
            const target = document.querySelector(href);
            if (target) {
                const idx = Array.from(sections).indexOf(target);
                if (idx !== -1) {
                    scrollToSection(idx);
                } else {
                    target.scrollIntoView({ behavior: 'smooth', block: 'start' });
                }
            }
        });
    });

    // Initialize active indicator
    const initialSection = getCurrentSectionFromScroll();
    updateActive(initialSection);
}
