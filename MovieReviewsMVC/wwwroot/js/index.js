document.addEventListener('DOMContentLoaded', function () {
    // 1. Слайдер для главного баннера
    class MainSlider {
        constructor() {
            this.slides = document.querySelectorAll('.slide');
            this.dots = document.querySelectorAll('.dot');
            this.prevArrow = document.querySelector('.slider-nav .prev-arrow');
            this.nextArrow = document.querySelector('.slider-nav .next-arrow');
            this.currentSlide = 0;
            this.interval = null;

            // ЕСЛИ НЕТ СЛАЙДОВ - НЕ ИНИЦИАЛИЗИРУЕМ
            if (this.slides.length === 0) {
                return;
            }

            this.init();
        }

        init() {
            if (this.prevArrow) this.prevArrow.addEventListener('click', () => this.prevSlide());
            if (this.nextArrow) this.nextArrow.addEventListener('click', () => this.nextSlide());

            this.dots.forEach((dot, index) => {
                dot.addEventListener('click', () => this.goToSlide(index));
            });

            this.startAutoPlay();

            const sliderBlock = document.querySelector('.slider-block');
            if (sliderBlock) {
                sliderBlock.addEventListener('mouseenter', () => this.stopAutoPlay());
                sliderBlock.addEventListener('mouseleave', () => this.startAutoPlay());
            }

            document.querySelectorAll('.slider-bg').forEach(img => {
                img.addEventListener('click', () => this.openImageModal(img));
            });
        }

        showSlide(index) {
            this.slides.forEach(slide => slide.classList.remove('active'));
            this.dots.forEach(dot => dot.classList.remove('active'));

            this.slides[index].classList.add('active');
            this.dots[index].classList.add('active');
            this.currentSlide = index;
        }

        nextSlide() {
            let nextIndex = (this.currentSlide + 1) % this.slides.length;
            this.showSlide(nextIndex);
        }

        prevSlide() {
            let prevIndex = (this.currentSlide - 1 + this.slides.length) % this.slides.length;
            this.showSlide(prevIndex);
        }

        goToSlide(index) {
            this.showSlide(index);
        }

        startAutoPlay() {
            this.interval = setInterval(() => this.nextSlide(), 5000);
        }

        stopAutoPlay() {
            if (this.interval) {
                clearInterval(this.interval);
                this.interval = null;
            }
        }

        openImageModal(img) {
            const modal = document.getElementById('imageModal');
            const modalImg = document.getElementById('modalImage');
            const caption = document.getElementById('modalCaption');

            if (modal && modalImg && caption) {
                modalImg.src = img.src;
                modalImg.alt = img.alt;
                caption.textContent = img.alt;
                modal.style.display = 'block';
                document.body.style.overflow = 'hidden';
            }
        }
    }

    // 2. Карусель для блока "Лучшее"
    class BestCarousel {
        constructor() {
            this.container = document.querySelector('.best-carousel-container');
            this.items = document.querySelectorAll('.best-item');
            this.prevArrow = document.querySelector('.carousel-arrow.prev-arrow');
            this.nextArrow = document.querySelector('.carousel-arrow.next-arrow');
            this.currentIndex = 0;
            this.itemWidth = 380;
            this.visibleItems = this.calculateVisibleItems();

            this.init();

            window.addEventListener('resize', () => {
                this.visibleItems = this.calculateVisibleItems();
                this.updateCarousel();
            });
        }

        calculateVisibleItems() {
            const carouselWrapper = document.querySelector('.best-carousel-wrapper');
            if (!carouselWrapper) return 3;
            const containerWidth = carouselWrapper.offsetWidth;
            return Math.max(1, Math.floor(containerWidth / this.itemWidth));
        }

        init() {
            if (this.prevArrow && this.nextArrow) {
                this.prevArrow.addEventListener('click', () => this.prev());
                this.nextArrow.addEventListener('click', () => this.next());
            }

            this.items.forEach(item => {
                const img = item.querySelector('img');
                if (img) {
                    img.addEventListener('click', () => this.openImageModal(img));
                }
            });

            this.updateCarousel();
        }

        updateCarousel() {
            if (!this.container) return;
            const translateX = -this.currentIndex * this.itemWidth;
            this.container.style.transform = `translateX(${translateX}px)`;
            this.updateArrows();
        }

        updateArrows() {
            if (!this.prevArrow || !this.nextArrow) return;

            const maxIndex = Math.max(0, this.items.length - this.visibleItems);

            this.prevArrow.disabled = this.currentIndex === 0;
            this.prevArrow.style.opacity = this.currentIndex === 0 ? '0.3' : '1';

            this.nextArrow.disabled = this.currentIndex >= maxIndex;
            this.nextArrow.style.opacity = this.currentIndex >= maxIndex ? '0.3' : '1';
        }

        next() {
            const maxIndex = Math.max(0, this.items.length - this.visibleItems);
            if (this.currentIndex < maxIndex) {
                this.currentIndex++;
                this.updateCarousel();
            }
        }

        prev() {
            if (this.currentIndex > 0) {
                this.currentIndex--;
                this.updateCarousel();
            }
        }

        openImageModal(img) {
            const modal = document.getElementById('imageModal');
            const modalImg = document.getElementById('modalImage');
            const caption = document.getElementById('modalCaption');

            if (modal && modalImg && caption) {
                modalImg.src = img.src;
                modalImg.alt = img.alt;
                caption.textContent = img.alt;
                modal.style.display = 'block';
                document.body.style.overflow = 'hidden';
            }
        }
    }

    // 3. Система управления видео
    class VideoManager {
        constructor() {
            this.videoPreviews = document.querySelectorAll('.video-preview');
            this.playButtons = document.querySelectorAll('.play-fullscreen-btn');
            this.init();
        }

        init() {
            this.playButtons.forEach(btn => {
                btn.addEventListener('click', (e) => {
                    e.stopPropagation();
                    this.openVideo(btn);
                });
            });

            this.videoPreviews.forEach(preview => {
                preview.addEventListener('click', (e) => {
                    if (!e.target.closest('.play-fullscreen-btn')) {
                        const videoSrc = preview.getAttribute('data-src');
                        if (videoSrc) {
                            this.openVideoModal(videoSrc);
                        }
                    }
                });
            });

            this.initModal();
        }

        openVideo(button) {
            const videoSrc = button.getAttribute('data-src');
            if (videoSrc) this.openVideoModal(videoSrc);
        }

        openVideoModal(videoSrc) {
            if (!videoSrc) return;

            const modal = document.getElementById('videoModal');
            const videoFrame = document.getElementById('videoFrame');

            if (!modal || !videoFrame) return;

            let autoplaySrc = videoSrc;
            if (autoplaySrc.includes('?')) {
                autoplaySrc += '&autoplay=1';
            } else {
                autoplaySrc += '?autoplay=1';
            }

            videoFrame.src = autoplaySrc;
            modal.style.display = 'block';
            document.body.style.overflow = 'hidden';
        }

        initModal() {
            const modal = document.getElementById('videoModal');
            const closeButtons = document.querySelectorAll('.modal-close');
            const videoFrame = document.getElementById('videoFrame');

            closeButtons.forEach(btn => {
                btn.addEventListener('click', () => {
                    if (modal && videoFrame) {
                        videoFrame.src = '';
                        modal.style.display = 'none';
                        document.body.style.overflow = 'auto';
                    }
                });
            });

            if (modal) {
                modal.addEventListener('click', (e) => {
                    if (e.target === modal && videoFrame) {
                        videoFrame.src = '';
                        modal.style.display = 'none';
                        document.body.style.overflow = 'auto';
                    }
                });
            }

            document.addEventListener('keydown', (e) => {
                if (e.key === 'Escape' && modal && modal.style.display === 'block') {
                    if (videoFrame) videoFrame.src = '';
                    modal.style.display = 'none';
                    document.body.style.overflow = 'auto';
                }
            });
        }
    }

    // 4. Кнопка возврата наверх
    class BackToTop {
        constructor() {
            this.button = document.getElementById('backToTop');
            this.init();
        }

        init() {
            if (this.button) {
                window.addEventListener('scroll', () => this.toggleVisibility());
                this.button.addEventListener('click', () => this.scrollToTop());
            }
        }

        toggleVisibility() {
            if (window.pageYOffset > 300) {
                this.button.classList.add('visible');
            } else {
                this.button.classList.remove('visible');
            }
        }

        scrollToTop() {
            window.scrollTo({ top: 0, behavior: 'smooth' });
        }
    }

    // 5. Закрытие модальных окон изображений
    function initImageModal() {
        const modal = document.getElementById('imageModal');
        const closeBtn = document.querySelector('#imageModal .modal-close');

        if (closeBtn) {
            closeBtn.addEventListener('click', () => {
                if (modal) {
                    modal.style.display = 'none';
                    document.body.style.overflow = 'auto';
                }
            });
        }

        if (modal) {
            modal.addEventListener('click', (e) => {
                if (e.target === modal) {
                    modal.style.display = 'none';
                    document.body.style.overflow = 'auto';
                }
            });
        }
    }

    // Инициализация всех компонентов
    try {
        new MainSlider();
        new BestCarousel();
        new VideoManager();
        new BackToTop();
        initImageModal();
        console.log('Все компоненты инициализированы успешно!');
    } catch (error) {
        console.error('Ошибка при инициализации компонентов:', error);
    }
});