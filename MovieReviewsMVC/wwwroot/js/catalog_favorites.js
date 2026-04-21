// catalog_favorites.js - ДЛЯ ASP.NET CORE MVC

const API_BASE_URL = '/api/seriesapi';

// Состояние приложения
let favorites = [];
let currentUserId = 'default_user';

// DOM элементы
const favoritesGrid = document.getElementById('favoritesGrid');
const emptyFavorites = document.getElementById('emptyFavorites');
const favoriteToast = document.getElementById('favoriteToast');
const toastMessage = document.getElementById('toastMessage');

// Инициализация страницы
document.addEventListener('DOMContentLoaded', async function () {
    console.log('Загрузка страницы избранного...');
    await loadFavorites();
});

// Загрузка избранного с сервера
async function loadFavorites() {
    try {
        if (favoritesGrid) {
            favoritesGrid.innerHTML = '<div class="loading">Загрузка избранного...</div>';
        }

        const response = await fetch(`${API_BASE_URL}/favorites?userId=${currentUserId}`);

        if (!response.ok) {
            throw new Error(`Ошибка HTTP: ${response.status}`);
        }

        const favoritesData = await response.json();
        console.log('Избранное загружено:', favoritesData);

        // Обновляем localStorage
        const favoriteIds = favoritesData.map(item => item.id);
        localStorage.setItem('favorites', JSON.stringify(favoriteIds));

        // Отправляем событие об изменении избранного
        window.dispatchEvent(new CustomEvent('favoritesUpdated', { detail: { favorites: favoriteIds } }));

        if (favoritesData.length === 0) {
            showEmptyState();
        } else {
            renderFavorites(favoritesData);
        }
    } catch (error) {
        console.error('Ошибка загрузки избранного:', error);
        if (favoritesGrid) {
            favoritesGrid.innerHTML = `<div class="error">Ошибка загрузки данных: ${error.message}</div>`;
        }
    }
}

// Отрисовка карточек избранного
function renderFavorites(favoritesData) {
    if (favoritesGrid) {
        favoritesGrid.innerHTML = '';
    }

    if (emptyFavorites) {
        emptyFavorites.style.display = 'none';
    }

    favoritesData.forEach(item => {
        const card = createFavoriteCard(item);
        if (favoritesGrid) {
            favoritesGrid.appendChild(card);
        }
    });

    addRemoveHandlers();
}

// Создание карточки избранного
function createFavoriteCard(item) {
    const card = document.createElement('div');
    card.className = 'favorite-card';
    card.setAttribute('data-id', item.id);

    // Формируем путь к постеру
    let posterPath = '/img/placeholder.png';
    if (item.posterUrl) {
        posterPath = item.posterUrl.startsWith('/') ? item.posterUrl : '/' + item.posterUrl;
    }

    // Формируем годы
    const years = item.yearEnd ? `${item.yearStart}-${item.yearEnd}` : `${item.yearStart}`;

    // Формируем жанр
    const genre = item.genre || 'Не указан';

    // Формируем описание
    const description = item.shortDescription ||
        (item.fullDescription ? item.fullDescription.substring(0, 150) + '...' : 'Описание отсутствует');

    // Формируем актеров для hover-карточки
    const cast = item.cast || 'Информация отсутствует';

    // Формируем рейтинг для hover-карточки
    let rating = '?';
    if (item.rating !== null && item.rating !== undefined) {
        const ratingNum = parseFloat(item.rating);
        if (!isNaN(ratingNum)) {
            rating = ratingNum.toFixed(1);
        }
    }

    card.innerHTML = `
        <a href="/Pages/Serial?id=${item.id}" style="text-decoration: none; color: inherit;">
            <img src="${posterPath}" onerror="this.src='/img/placeholder.png';" alt="${escapeHtml(item.title)}" />
            <div class="favorite-content">
                <p class="title_content">${escapeHtml(item.title)}</p>
                <p class="favorite-desc" style="display: none;">${escapeHtml(description)}</p>
                <div class="favorite-meta">
                    <p>Год: ${years}</p>
                    <p>Жанр: ${escapeHtml(genre)}</p>
                </div>
            </div>
            <div class="favorite-hover-card">
                <div class="hover-rating">
                    <i class="fas fa-star"></i>
                    <span>${rating}</span>
                    <span style="color: #aaa; margin-left: 5px;">/ 10</span>
                </div>
                <div class="hover-description">
                    ${escapeHtml(description)}
                </div>
                <div class="hover-cast">
                    <span>В ролях:</span> ${escapeHtml(cast)}
                </div>
                <div class="hover-year">
                    ${item.yearStart} - ${item.yearEnd || 'настоящее время'}
                </div>
            </div>
        </a>
        <button class="remove-favorite-btn" data-id="${item.id}" title="Удалить из избранного">
            <i class="fas fa-times"></i>
        </button>
    `;

    return card;
}

// Экранирование HTML
function escapeHtml(text) {
    if (!text) return '';
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}

// Добавление обработчиков для кнопок удаления
function addRemoveHandlers() {
    document.querySelectorAll('.remove-favorite-btn').forEach(btn => {
        btn.addEventListener('click', async function (e) {
            e.preventDefault();
            e.stopPropagation();

            const seriesId = this.getAttribute('data-id');
            await removeFromFavorites(seriesId, this);
        });
    });
}

// Удаление из избранного
async function removeFromFavorites(seriesId, buttonElement) {
    try {
        const response = await fetch(`${API_BASE_URL}/favorites`, {
            method: 'DELETE',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                seriesId: parseInt(seriesId),
                userId: currentUserId
            })
        });

        if (!response.ok) {
            throw new Error('Ошибка удаления');
        }

        const result = await response.json();

        if (result.success) {
            // Обновляем localStorage
            let favorites = JSON.parse(localStorage.getItem('favorites')) || [];
            favorites = favorites.filter(id => id != seriesId);
            localStorage.setItem('favorites', JSON.stringify(favorites));

            // Отправляем событие об изменении избранного
            window.dispatchEvent(new CustomEvent('favoritesUpdated', { detail: { favorites: favorites } }));

            const card = buttonElement.closest('.favorite-card');
            if (card) {
                card.style.animation = 'fadeOut 0.3s ease';
                setTimeout(() => {
                    card.remove();
                    showToast('Сериал удален из избранного');

                    if (document.querySelectorAll('.favorite-card').length === 0) {
                        showEmptyState();
                    }
                }, 300);
            }
        }
    } catch (error) {
        console.error('Ошибка удаления:', error);
        showToast('Ошибка при удалении', 'error');
    }
}

// Показ пустого состояния
function showEmptyState() {
    if (favoritesGrid) {
        favoritesGrid.innerHTML = '';
    }
    if (emptyFavorites) {
        emptyFavorites.style.display = 'block';
    }
}

// Показ уведомления
function showToast(message, type = 'success') {
    if (!favoriteToast || !toastMessage) return;

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