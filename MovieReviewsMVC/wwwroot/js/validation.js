document.addEventListener('DOMContentLoaded', function () {
    // Ленивая загрузка фотографий
    const loadMoreBtn = document.querySelector('.load-more-photos-btn');
    const hiddenPhotos = document.querySelectorAll('.hidden-photo');

    if (loadMoreBtn && hiddenPhotos.length > 0) {
        loadMoreBtn.addEventListener('click', function () {
            // Показываем следующие 5 скрытых фото
            let photosShown = 0;
            hiddenPhotos.forEach(photo => {
                if (photosShown < 5 && photo.style.display !== 'block') {
                    photo.style.display = 'block';
                    photosShown++;
                }
            });

            // Скрываем кнопку если все фото показаны
            const remainingHidden = document.querySelectorAll('.hidden-photo[style=""]').length;
            if (remainingHidden === 0) {
                loadMoreBtn.style.display = 'none';
            }
        });
    }

    // Скрытие/раскрытие текста
    const readMoreBtn = document.querySelector('.read-more-btn');
    const hiddenText = document.querySelector('.hidden-text');
    const readMoreText = document.querySelector('.read-more-text');
    const readLessText = document.querySelector('.read-less-text');
    const readMoreIcon = readMoreBtn?.querySelector('i');

    if (readMoreBtn && hiddenText) {
        readMoreBtn.addEventListener('click', function () {
            if (hiddenText.style.display === 'block') {
                // Скрываем текст
                hiddenText.style.display = 'none';
                readMoreText.style.display = 'inline';
                readLessText.style.display = 'none';
                readMoreIcon.style.transform = 'rotate(0deg)';
            } else {
                // Показываем текст
                hiddenText.style.display = 'block';
                readMoreText.style.display = 'none';
                readLessText.style.display = 'inline';
                readMoreIcon.style.transform = 'rotate(180deg)';
            }
        });
    }

    // Плавная анимация для иконки
    if (readMoreIcon) {
        readMoreIcon.style.transition = 'transform 0.3s ease';
    }
});