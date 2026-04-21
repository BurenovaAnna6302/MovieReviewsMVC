// catalog_serial.js - ДЛЯ ASP.NET CORE MVC (BLOB)

const API_BASE_URL = '/api/seriesapi';

// Состояние приложения
let currentSeries = [];
let favorites = JSON.parse(localStorage.getItem('favorites')) || [];
let currentSort = 'new';
let currentFilters = {
    genre: 'all',
    yearFrom: 1990,
    yearTo: 2025,
    language: 'all'
};
let searchTimeout;
let hoverTimeout;

// DOM элементы
const seriesGrid = document.getElementById('seriesGrid');
const searchInput = document.getElementById('globalSearchInput');
const searchSuggestions = document.getElementById('globalSearchSuggestions');
const genreFilter = document.getElementById('genreFilter');
const yearFrom = document.getElementById('yearFrom');
const yearTo = document.getElementById('yearTo');
const languageFilter = document.getElementById('languageFilter');
const resetFiltersBtn = document.getElementById('resetFiltersBtn');
const favoriteToast = document.getElementById('favoriteToast');
const toastMessage = document.getElementById('toastMessage');
const sortBtns = document.querySelectorAll('.sort-btn');

// Инициализация приложения
async function init() {
    try {
        console.log('Инициализация каталога...');
        await loadGenres();
        await loadLanguages();
        await loadSeries();
        await checkFavoritesStatus();
        setupEventListeners();

        const activeSortBtn = document.querySelector(`.sort-btn[data-sort="${currentSort}"]`);
        if (activeSortBtn) activeSortBtn.classList.add('active');

        console.log('Каталог успешно инициализирован');
    } catch (error) {
        console.error('Ошибка инициализации:', error);
        showToast('Ошибка загрузки данных', 'error');
    }
}

// Загрузка жанров
async function loadGenres() {
    try {
        const response = await fetch(`${API_BASE_URL}/genres`);
        if (!response.ok) throw new Error('Ошибка загрузки жанров');

        const genres = await response.json();

        genreFilter.innerHTML = '<option value="all">Все жанры</option>';
        genres.forEach(genre => {
            const option = document.createElement('option');
            option.value = genre.id;
            option.textContent = genre.name;
            genreFilter.appendChild(option);
        });

        console.log('Жанры загружены:', genres.length);
    } catch (error) {
        console.error('Ошибка загрузки жанров:', error);
    }
}

// Загрузка языков
async function loadLanguages() {
    try {
        const response = await fetch(`${API_BASE_URL}/languages`);
        if (!response.ok) throw new Error('Ошибка загрузки языков');

        const languages = await response.json();

        languageFilter.innerHTML = '<option value="all">Все языки</option>';
        languages.forEach(lang => {
            const option = document.createElement('option');
            option.value = lang.id;
            option.textContent = lang.name;
            languageFilter.appendChild(option);
        });

        console.log('Языки загружены:', languages.length);
    } catch (error) {
        console.error('Ошибка загрузки языков:', error);
    }
}

// Загрузка всех сериалов
async function loadSeries() {
    try {
        seriesGrid.innerHTML = '<div class="loading">Загрузка сериалов...</div>';

        const response = await fetch(`${API_BASE_URL}`);
        if (!response.ok) throw new Error('Ошибка загрузки сериалов');

        const data = await response.json();
        currentSeries = sortSeries(data, currentSort);
        renderSeries(currentSeries);

        console.log('Сериалы загружены:', currentSeries.length);
    } catch (error) {
        console.error('Ошибка загрузки сериалов:', error);
        seriesGrid.innerHTML = '<div class="error">Ошибка загрузки данных. Проверьте подключение к серверу.</div>';
    }
}

// Применение фильтров
async function applyFilters() {
    try {
        seriesGrid.innerHTML = '<div class="loading">Применение фильтров...</div>';

        const params = new URLSearchParams({
            genre: genreFilter.value,
            yearFrom: yearFrom.value,
            yearTo: yearTo.value,
            language: languageFilter.value
        });

        const response = await fetch(`${API_BASE_URL}/filter?${params}`);
        if (!response.ok) throw new Error('Ошибка фильтрации');

        const filteredSeries = await response.json();

        currentFilters = {
            genre: genreFilter.value,
            yearFrom: parseInt(yearFrom.value) || 1990,
            yearTo: parseInt(yearTo.value) || 2025,
            language: languageFilter.value
        };

        currentSeries = sortSeries(filteredSeries, currentSort);
        renderSeries(currentSeries);
    } catch (error) {
        console.error('Ошибка фильтрации:', error);
        showToast('Ошибка при фильтрации', 'error');
    }
}

// Сброс фильтров
function resetFilters() {
    genreFilter.value = 'all';
    yearFrom.value = 1990;
    yearTo.value = 2025;
    languageFilter.value = 'all';

    if (searchInput) searchInput.value = '';

    currentFilters = {
        genre: 'all',
        yearFrom: 1990,
        yearTo: 2025,
        language: 'all'
    };

    loadSeries();
}

// Отрисовка карточек
function renderSeries(series) {
    seriesGrid.innerHTML = '';

    if (series.length === 0) {
        seriesGrid.innerHTML = '<div class="no-results">Сериалы не найдены</div>';
        return;
    }

    series.forEach(ser => {
        const isFavorite = favorites.includes(ser.id);
        const card = document.createElement('div');
        card.className = 'series-card';
        card.setAttribute('data-id', ser.id);
        card.setAttribute('data-title', ser.title);

        // Путь к картинке через API
        const posterPath = `${API_BASE_URL}/image/${ser.id}`;

        const yearEndText = ser.yearEnd ? ser.yearEnd : 'н.в.';
        const genreName = ser.genre || 'Не указан';

        card.innerHTML = `
            <a href="/Pages/Serial?id=${ser.id}" style="text-decoration: none; color: inherit;">
                <img src="${posterPath}" onerror="this.src='/img/placeholder.png';" alt="${escapeHtml(ser.title)}" />
            </a>
            <div class="favorite-btn ${isFavorite ? 'active' : ''}" data-id="${ser.id}">
                <i class="${isFavorite ? 'fas' : 'far'} fa-heart"></i>
            </div>
            <div class="series-hover-card" style="display: none;">
                <div class="hover-loader">Загрузка...</div>
            </div>
            <div class="series-content">
                <p class="series-title">${escapeHtml(ser.title)}</p>
                <div class="series-meta">
                    <p>Год: ${ser.yearStart}-${yearEndText}</p>
                    <p>Жанр: ${escapeHtml(genreName)}</p>
                </div>
            </div>
        `;

        seriesGrid.appendChild(card);
    });

    addEventListenersToCards();
    updateAllFavoriteButtons();
}

// Экранирование HTML
function escapeHtml(text) {
    if (!text) return '';
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}

// Добавление обработчиков
function addEventListenersToCards() {
    document.querySelectorAll('.favorite-btn').forEach(btn => {
        btn.addEventListener('click', handleFavoriteClick);
    });

    document.querySelectorAll('.series-card').forEach(card => {
        card.addEventListener('mouseenter', handleCardHover);
        card.addEventListener('mouseleave', handleCardLeave);
    });
}

// ==================== ПОИСК ====================

function setupSearch() {
    if (!searchInput) return;

    searchInput.addEventListener('input', function () {
        clearTimeout(searchTimeout);
        const query = this.value.trim();

        if (query.length < 2) {
            if (searchSuggestions) searchSuggestions.classList.remove('active');
            loadSeries();
            return;
        }

        searchTimeout = setTimeout(() => getSuggestions(query), 300);
    });

    searchInput.addEventListener('keypress', function (e) {
        if (e.key === 'Enter') {
            e.preventDefault();
            const query = this.value.trim();
            if (searchSuggestions) searchSuggestions.classList.remove('active');

            if (query) {
                performSearch(query);
            } else {
                loadSeries();
            }
        }
    });

    document.addEventListener('click', function (e) {
        if (searchSuggestions && searchInput &&
            !searchInput.contains(e.target) &&
            !searchSuggestions.contains(e.target)) {
            searchSuggestions.classList.remove('active');
        }
    });
}

async function getSuggestions(query) {
    try {
        const response = await fetch(`${API_BASE_URL}/suggestions?q=${encodeURIComponent(query)}`);
        if (!response.ok) throw new Error('Ошибка получения подсказок');

        const suggestions = await response.json();

        if (!searchSuggestions) return;

        if (suggestions.length === 0) {
            searchSuggestions.classList.remove('active');
            return;
        }

        searchSuggestions.innerHTML = suggestions.map(ser => `
            <div class="suggestion-item" data-id="${ser.id}">
                <span class="suggestion-title">${escapeHtml(ser.title)}</span>
                <span class="suggestion-year">(${ser.yearStart})</span>
            </div>
        `).join('');

        searchSuggestions.classList.add('active');

        document.querySelectorAll('.suggestion-item').forEach(item => {
            item.addEventListener('click', function () {
                const id = this.getAttribute('data-id');
                const series = suggestions.find(s => s.id == id);
                if (series && searchInput) {
                    searchInput.value = series.title;
                    if (searchSuggestions) searchSuggestions.classList.remove('active');
                    performSearch(series.title);
                }
            });
        });
    } catch (error) {
        console.error('Ошибка получения подсказок:', error);
    }
}

async function performSearch(query) {
    try {
        seriesGrid.innerHTML = '<div class="loading">Поиск...</div>';

        const response = await fetch(`${API_BASE_URL}/search?q=${encodeURIComponent(query)}`);
        if (!response.ok) throw new Error('Ошибка поиска');

        const results = await response.json();

        currentSeries = sortSeries(results, currentSort);
        renderSeries(currentSeries);
    } catch (error) {
        console.error('Ошибка поиска:', error);
        showToast('Ошибка при поиске', 'error');
    }
}

// ==================== ИНФОРМАЦИЯ ПРИ НАВЕДЕНИИ ====================

async function handleCardHover(e) {
    const card = e.currentTarget;
    const seriesId = card.getAttribute('data-id');
    const hoverCard = card.querySelector('.series-hover-card');

    clearTimeout(hoverTimeout);

    hoverTimeout = setTimeout(async () => {
        try {
            if (!hoverCard) return;
            hoverCard.style.display = 'block';
            hoverCard.innerHTML = '<div class="hover-loader">Загрузка...</div>';

            const response = await fetch(`${API_BASE_URL}/${seriesId}/info`);
            if (!response.ok) throw new Error('Ошибка загрузки информации');

            const info = await response.json();

            const description = info.shortDescription ||
                (info.fullDescription ? info.fullDescription.substring(0, 150) + '...' : 'Описание отсутствует');
            const cast = info.cast || 'Информация отсутствует';

            hoverCard.innerHTML = `
                <div class="hover-rating">
                    <i class="fas fa-star"></i>
                    <span>${info.rating}</span>
                    <span style="color: #aaa; margin-left: 5px;">/ 10</span>
                </div>
                <div class="hover-description">
                    ${escapeHtml(description)}
                </div>
                <div class="hover-cast">
                    <span>В ролях:</span> ${escapeHtml(cast)}
                </div>
            `;
        } catch (error) {
            console.error('Ошибка загрузки информации:', error);
            if (hoverCard) {
                hoverCard.innerHTML = '<div class="hover-error">Ошибка загрузки информации</div>';
            }
        }
    }, 300);
}

function handleCardLeave(e) {
    const card = e.currentTarget;
    const hoverCard = card.querySelector('.series-hover-card');

    clearTimeout(hoverTimeout);
    if (hoverCard) {
        hoverCard.style.display = 'none';
    }
}

// ==================== ИЗБРАННОЕ ====================

async function handleFavoriteClick(e) {
    e.preventDefault();
    e.stopPropagation();

    const btn = e.currentTarget;
    const seriesId = parseInt(btn.getAttribute('data-id'));
    const isActive = btn.classList.contains('active');

    try {
        if (isActive) {
            const response = await fetch(`${API_BASE_URL}/favorites`, {
                method: 'DELETE',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ seriesId: seriesId })
            });

            if (!response.ok) throw new Error('Ошибка удаления из избранного');

            const result = await response.json();

            if (result.success) {
                favorites = favorites.filter(id => id !== seriesId);
                btn.classList.remove('active');
                btn.innerHTML = '<i class="far fa-heart"></i>';
                showToast(result.message);
            }
        } else {
            const response = await fetch(`${API_BASE_URL}/favorites`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ seriesId: seriesId })
            });

            if (!response.ok) throw new Error('Ошибка добавления в избранное');

            const result = await response.json();

            if (result.success) {
                favorites.push(seriesId);
                btn.classList.add('active');
                btn.innerHTML = '<i class="fas fa-heart"></i>';
                showToast(result.message);
            } else if (result.message === 'Сериал уже в избранном') {
                showToast(result.message);
            }
        }

        localStorage.setItem('favorites', JSON.stringify(favorites));
        window.dispatchEvent(new CustomEvent('favoritesUpdated', { detail: { favorites: favorites } }));
    } catch (error) {
        console.error('Ошибка при работе с избранным:', error);
        showToast('Ошибка при сохранении', 'error');
    }
}

async function checkFavoritesStatus() {
    try {
        const response = await fetch(`${API_BASE_URL}/favorites`);
        if (!response.ok) throw new Error('Ошибка проверки избранного');

        const favoritesFromServer = await response.json();
        favorites = favoritesFromServer.map(f => f.id);
        localStorage.setItem('favorites', JSON.stringify(favorites));

        console.log('Избранное загружено:', favorites.length);
    } catch (error) {
        console.error('Ошибка проверки избранного:', error);
        favorites = JSON.parse(localStorage.getItem('favorites')) || [];
    }
}

// Обновление кнопок избранного
function updateAllFavoriteButtons() {
    document.querySelectorAll('.favorite-btn').forEach(btn => {
        const seriesId = parseInt(btn.getAttribute('data-id'));
        const isFavorite = favorites.includes(seriesId);

        if (isFavorite) {
            btn.classList.add('active');
            btn.innerHTML = '<i class="fas fa-heart"></i>';
        } else {
            btn.classList.remove('active');
            btn.innerHTML = '<i class="far fa-heart"></i>';
        }
    });
}

// Слушаем изменения избранного
window.addEventListener('favoritesUpdated', function (event) {
    if (event.detail && event.detail.favorites) {
        favorites = event.detail.favorites;
        localStorage.setItem('favorites', JSON.stringify(favorites));
        updateAllFavoriteButtons();
    }
});

window.addEventListener('storage', function (event) {
    if (event.key === 'favorites') {
        const newFavorites = JSON.parse(event.newValue) || [];
        favorites = newFavorites;
        updateAllFavoriteButtons();
    }
});

// ==================== СОРТИРОВКА ====================

function setupSorting() {
    sortBtns.forEach(btn => {
        btn.addEventListener('click', function () {
            const sortType = this.getAttribute('data-sort');

            sortBtns.forEach(b => b.classList.remove('active'));
            this.classList.add('active');

            currentSort = sortType;
            const sorted = sortSeries(currentSeries, currentSort);
            currentSeries = sorted;
            renderSeries(sorted);
        });
    });
}

function sortSeries(series, type) {
    const sorted = [...series];

    switch (type) {
        case 'new':
            sorted.sort((a, b) => (b.yearStart || 0) - (a.yearStart || 0));
            break;
        case 'old':
            sorted.sort((a, b) => (a.yearStart || 0) - (b.yearStart || 0));
            break;
        case 'alphabet':
            sorted.sort((a, b) => (a.title || '').localeCompare(b.title || ''));
            break;
        case 'best':
            sorted.sort((a, b) => (b.rating || 0) - (a.rating || 0));
            break;
        case 'worst':
            sorted.sort((a, b) => (a.rating || 0) - (b.rating || 0));
            break;
    }

    return sorted;
}

// ==================== УВЕДОМЛЕНИЯ ====================

function showToast(message, type = 'success') {
    if (!toastMessage || !favoriteToast) return;

    toastMessage.textContent = message;
    favoriteToast.classList.add('show');

    if (type === 'error') {
        favoriteToast.style.backgroundColor = '#f44336';
    } else {
        favoriteToast.style.backgroundColor = '#0022FF';
    }

    setTimeout(() => {
        favoriteToast.classList.remove('show');
    }, 2000);
}

// ==================== ИНИЦИАЛИЗАЦИЯ ====================

function setupEventListeners() {
    setupSearch();
    setupSorting();

    if (resetFiltersBtn) resetFiltersBtn.addEventListener('click', resetFilters);

    if (genreFilter) genreFilter.addEventListener('change', applyFilters);
    if (languageFilter) languageFilter.addEventListener('change', applyFilters);
    if (yearFrom) yearFrom.addEventListener('change', applyFilters);
    if (yearTo) yearTo.addEventListener('change', applyFilters);
}

document.addEventListener('DOMContentLoaded', init);