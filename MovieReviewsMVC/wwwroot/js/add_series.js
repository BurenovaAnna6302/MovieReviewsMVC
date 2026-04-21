// add_series.js - ИСПРАВЛЕННЫЙ

const API_BASE_URL = '/api/seriesapi';

document.addEventListener('DOMContentLoaded', function () {
    console.log('Страница добавления сериала загружена');

    const form = document.getElementById('addSeriesForm');

    if (!form) {
        console.error('Форма не найдена!');
        return;
    }

    // Поля формы
    const title = document.getElementById('title');
    const yearStart = document.getElementById('yearStart');
    const yearEnd = document.getElementById('yearEnd');
    const genreId = document.getElementById('genreId');
    const languageId = document.getElementById('languageId');
    const cast = document.getElementById('cast');
    const rating = document.getElementById('rating');
    const shortDescription = document.getElementById('shortDescription');
    const fullDescription = document.getElementById('fullDescription');
    const posterImage = document.getElementById('posterImage');
    const submitBtn = document.querySelector('.btn-submit');

    const currentYear = new Date().getFullYear();

    if (yearStart) yearStart.max = currentYear;
    if (yearEnd) yearEnd.max = currentYear + 5;

    // Загрузка жанров
    async function loadGenres() {
        try {
            const response = await fetch(`${API_BASE_URL}/genres`);
            const genres = await response.json();
            genreId.innerHTML = '<option value="" disabled selected>Выберите жанр</option>';
            genres.forEach(genre => {
                const option = document.createElement('option');
                option.value = genre.id;
                option.textContent = genre.name;
                genreId.appendChild(option);
            });
            console.log('Жанры загружены');
        } catch (error) {
            console.error('Ошибка загрузки жанров:', error);
        }
    }

    // Загрузка языков
    async function loadLanguages() {
        try {
            const response = await fetch(`${API_BASE_URL}/languages`);
            const languages = await response.json();
            languageId.innerHTML = '<option value="" disabled selected>Выберите язык</option>';
            languages.forEach(lang => {
                const option = document.createElement('option');
                option.value = lang.id;
                option.textContent = lang.name;
                languageId.appendChild(option);
            });
            console.log('Языки загружены');
        } catch (error) {
            console.error('Ошибка загрузки языков:', error);
        }
    }

    loadGenres();
    loadLanguages();

    // Проверка файла
    if (posterImage) {
        posterImage.addEventListener('change', function () {
            const file = this.files[0];
            if (!file) return;

            console.log('Файл выбран:', file.name, file.size, file.type);

            if (!file.type.startsWith('image/')) {
                alert('Пожалуйста, выберите изображение');
                this.value = '';
                return;
            }

            if (file.size > 5 * 1024 * 1024) {
                alert('Размер файла не должен превышать 5 МБ');
                this.value = '';
                return;
            }
        });
    }

    // Отправка формы
    form.addEventListener('submit', async function (e) {
        e.preventDefault();

        console.log('Форма отправлена!');

        // Валидация
        if (!title.value.trim()) {
            alert('Введите название');
            return;
        }

        if (!yearStart.value) {
            alert('Введите год начала');
            return;
        }

        if (!genreId.value) {
            alert('Выберите жанр');
            return;
        }

        if (!languageId.value) {
            alert('Выберите язык');
            return;
        }

        if (!cast.value.trim()) {
            alert('Введите имена актёров');
            return;
        }

        if (!rating.value) {
            alert('Введите рейтинг');
            return;
        }

        const ratingValue = parseFloat(rating.value);
        if (isNaN(ratingValue) || ratingValue < 0 || ratingValue > 10) {
            alert('Рейтинг должен быть числом от 0 до 10');
            return;
        }

        if (!shortDescription.value.trim()) {
            alert('Введите краткое описание');
            return;
        }

        if (!fullDescription.value.trim()) {
            alert('Введите полное описание');
            return;
        }

        if (!posterImage.files || posterImage.files.length === 0) {
            alert('Выберите постер');
            return;
        }

        try {
            if (submitBtn) {
                submitBtn.disabled = true;
                submitBtn.innerHTML = 'Отправка...';
            }

            const formData = new FormData();
            formData.append('posterImage', posterImage.files[0]);
            formData.append('Title', title.value.trim());
            formData.append('TitleEn', title.value.trim());
            formData.append('ShortDescription', shortDescription.value.trim());
            formData.append('FullDescription', fullDescription.value.trim());
            formData.append('YearStart', parseInt(yearStart.value));
            formData.append('YearEnd', yearEnd.value ? parseInt(yearEnd.value) : '');
            formData.append('GenreId', parseInt(genreId.value));
            formData.append('LanguageId', parseInt(languageId.value));
            formData.append('Rating', rating.value);  // Строка (8.5 или 8,5)
            formData.append('Cast', cast.value.trim());

            console.log('Отправка данных на сервер...');

            const response = await fetch(`${API_BASE_URL}/upload`, {
                method: 'POST',
                body: formData
            });

            if (!response.ok) {
                const errorText = await response.text();
                console.error('Ошибка сервера:', response.status, errorText);
                throw new Error(`Ошибка сервера: ${response.status}`);
            }

            const result = await response.json();
            console.log('Ответ сервера:', result);

            if (result.success) {
                alert('Сериал успешно добавлен!');
                window.location.href = '/Pages/CatalogSerial';
            } else {
                alert('Ошибка: ' + (result.error || 'Неизвестная ошибка'));
                if (submitBtn) {
                    submitBtn.disabled = false;
                    submitBtn.innerHTML = 'Добавить сериал';
                }
            }
        } catch (error) {
            console.error('Ошибка:', error);
            alert('Ошибка при отправке: ' + error.message);
            if (submitBtn) {
                submitBtn.disabled = false;
                submitBtn.innerHTML = 'Добавить сериал';
            }
        }
    });
});