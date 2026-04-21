using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieReviewsMVC.Models;

namespace MovieReviewsMVC.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SeriesApiController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SeriesApiController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/seriesapi
        [HttpGet]
        public async Task<IActionResult> GetAllSeries()
        {
            var series = await _context.Series
                .Include(s => s.Genre)
                .Include(s => s.Language)
                .OrderByDescending(s => s.YearStart)
                .Select(s => new
                {
                    s.Id,
                    s.Title,
                    s.TitleEn,
                    s.ShortDescription,
                    s.FullDescription,
                    s.YearStart,
                    s.YearEnd,
                    s.Rating,
                    s.Cast,
                    GenreId = s.GenreId,
                    Genre = s.Genre != null ? s.Genre.Name : null,
                    LanguageId = s.LanguageId,
                    Language = s.Language != null ? s.Language.Name : null
                })
                .ToListAsync();

            return Ok(series);
        }

        // GET: api/seriesapi/genres
        [HttpGet("genres")]
        public async Task<IActionResult> GetGenres()
        {
            var genres = await _context.Genres.OrderBy(g => g.Name).ToListAsync();
            return Ok(genres);
        }

        // GET: api/seriesapi/languages
        [HttpGet("languages")]
        public async Task<IActionResult> GetLanguages()
        {
            var languages = await _context.Languages.OrderBy(l => l.Name).ToListAsync();
            return Ok(languages);
        }

        // GET: api/seriesapi/filter
        [HttpGet("filter")]
        public async Task<IActionResult> FilterSeries(
            [FromQuery] string? genre,
            [FromQuery] int? yearFrom,
            [FromQuery] int? yearTo,
            [FromQuery] string? language)
        {
            var query = _context.Series
                .Include(s => s.Genre)
                .Include(s => s.Language)
                .AsQueryable();

            if (!string.IsNullOrEmpty(genre) && genre != "all")
            {
                var genreId = int.Parse(genre);
                query = query.Where(s => s.GenreId == genreId);
            }

            if (yearFrom.HasValue && yearTo.HasValue)
            {
                query = query.Where(s => s.YearStart >= yearFrom.Value && s.YearStart <= yearTo.Value);
            }

            if (!string.IsNullOrEmpty(language) && language != "all")
            {
                var languageId = int.Parse(language);
                query = query.Where(s => s.LanguageId == languageId);
            }

            var result = await query
                .Select(s => new
                {
                    s.Id,
                    s.Title,
                    s.TitleEn,
                    s.ShortDescription,
                    s.FullDescription,
                    s.YearStart,
                    s.YearEnd,
                    s.Rating,
                    s.Cast,
                    Genre = s.Genre != null ? s.Genre.Name : null,
                    Language = s.Language != null ? s.Language.Name : null
                })
                .OrderByDescending(s => s.YearStart)
                .ToListAsync();

            return Ok(result);
        }

        // GET: api/seriesapi/search
        [HttpGet("search")]
        public async Task<IActionResult> SearchSeries([FromQuery] string q)
        {
            if (string.IsNullOrEmpty(q) || q.Length < 2)
            {
                return Ok(new List<object>());
            }

            var results = await _context.Series
                .Where(s => EF.Functions.ILike(s.Title, $"%{q}%") ||
                           (s.TitleEn != null && EF.Functions.ILike(s.TitleEn, $"%{q}%")) ||
                           (s.ShortDescription != null && EF.Functions.ILike(s.ShortDescription, $"%{q}%")))
                .OrderByDescending(s => s.Rating)
                .Take(20)
                .Select(s => new
                {
                    s.Id,
                    s.Title,
                    s.TitleEn,
                    s.YearStart,
                    s.Rating,
                    Genre = s.Genre != null ? s.Genre.Name : null
                })
                .ToListAsync();

            return Ok(results);
        }

        // GET: api/seriesapi/suggestions
        [HttpGet("suggestions")]
        public async Task<IActionResult> GetSuggestions([FromQuery] string q)
        {
            if (string.IsNullOrEmpty(q) || q.Length < 2)
            {
                return Ok(new List<object>());
            }

            var suggestions = await _context.Series
                .Where(s => EF.Functions.ILike(s.Title, $"%{q}%") ||
                           (s.TitleEn != null && EF.Functions.ILike(s.TitleEn, $"%{q}%")))
                .OrderByDescending(s => s.Rating)
                .Take(5)
                .Select(s => new
                {
                    s.Id,
                    s.Title,
                    s.TitleEn,
                    s.YearStart
                })
                .ToListAsync();

            return Ok(suggestions);
        }

        // GET: api/seriesapi/{id}/info
        [HttpGet("{id}/info")]
        public async Task<IActionResult> GetSeriesInfo(int id)
        {
            var series = await _context.Series
                .Include(s => s.Genre)
                .Include(s => s.Language)
                .Where(s => s.Id == id)
                .Select(s => new
                {
                    s.Id,
                    s.Title,
                    s.ShortDescription,
                    s.FullDescription,
                    s.YearStart,
                    s.YearEnd,
                    s.Rating,
                    s.Cast,
                    Genre = s.Genre != null ? s.Genre.Name : null,
                    Language = s.Language != null ? s.Language.Name : null
                })
                .FirstOrDefaultAsync();

            if (series == null)
            {
                return NotFound(new { error = "Сериал не найден" });
            }

            return Ok(series);
        }

        // GET: api/seriesapi/image/{id}
        [HttpGet("image/{id}")]
        public async Task<IActionResult> GetImage(int id)
        {
            var series = await _context.Series.FindAsync(id);
            if (series?.PosterImage == null || series.PosterImage.Length == 0)
            {
                return NotFound();
            }
            return File(series.PosterImage, series.PosterContentType ?? "image/jpeg");
        }

        // ==================== ИЗБРАННОЕ ====================

        // GET: api/seriesapi/favorites
        [HttpGet("favorites")]
        public async Task<IActionResult> GetFavorites([FromQuery] string userId = "default_user")
        {
            var favorites = await _context.Favorites
                .Where(f => f.UserId == userId)
                .Include(f => f.Series)
                .ThenInclude(s => s!.Genre)
                .Include(f => f.Series)
                .ThenInclude(s => s!.Language)
                .Select(f => new
                {
                    f.Series!.Id,
                    f.Series.Title,
                    f.Series.YearStart,
                    f.Series.YearEnd,
                    f.Series.Rating,
                    Genre = f.Series.Genre != null ? f.Series.Genre.Name : null,
                    Language = f.Series.Language != null ? f.Series.Language.Name : null
                })
                .ToListAsync();

            return Ok(favorites);
        }

        // POST: api/seriesapi/favorites
        [HttpPost("favorites")]
        public async Task<IActionResult> AddToFavorites([FromBody] FavoriteRequest request)
        {
            var series = await _context.Series.FindAsync(request.SeriesId);
            if (series == null)
            {
                return NotFound(new { error = "Сериал не найден" });
            }

            var existing = await _context.Favorites
                .FirstOrDefaultAsync(f => f.SeriesId == request.SeriesId && f.UserId == (request.UserId ?? "default_user"));

            if (existing != null)
            {
                return Ok(new { success = false, message = $"Сериал \"{series.Title}\" уже в избранном" });
            }

            var favorite = new Favorite
            {
                SeriesId = request.SeriesId,
                UserId = request.UserId ?? "default_user",
                CreatedAt = DateTime.UtcNow
            };

            _context.Favorites.Add(favorite);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = $"Сериал \"{series.Title}\" добавлен в избранное" });
        }

        // DELETE: api/seriesapi/favorites
        [HttpDelete("favorites")]
        public async Task<IActionResult> RemoveFromFavorites([FromBody] FavoriteRequest request)
        {
            var favorite = await _context.Favorites
                .FirstOrDefaultAsync(f => f.SeriesId == request.SeriesId && f.UserId == (request.UserId ?? "default_user"));

            if (favorite != null)
            {
                _context.Favorites.Remove(favorite);
                await _context.SaveChangesAsync();
            }

            return Ok(new { success = true, message = "Сериал удален из избранного" });
        }

        // GET: api/seriesapi/favorites/check/{seriesId}
        [HttpGet("favorites/check/{seriesId}")]
        public async Task<IActionResult> CheckFavorite(int seriesId, [FromQuery] string userId = "default_user")
        {
            var isFavorite = await _context.Favorites
                .AnyAsync(f => f.SeriesId == seriesId && f.UserId == userId);

            return Ok(new { isFavorite });
        }

        // ==================== ДОБАВЛЕНИЕ НОВОГО СЕРИАЛА ====================

        // POST: api/seriesapi/upload
        [HttpPost("upload")]
        public async Task<IActionResult> UploadSeries(
     [FromForm] IFormFile posterImage,
     [FromForm] string Title,
     [FromForm] string TitleEn,
     [FromForm] string ShortDescription,
     [FromForm] string FullDescription,
     [FromForm] int YearStart,
     [FromForm] int? YearEnd,
     [FromForm] int GenreId,
     [FromForm] int LanguageId,
     [FromForm] string Rating,  // Принимаем как строку
     [FromForm] string Cast)
        {
            try
            {
                // Парсим рейтинг с поддержкой точки и запятой
                double ratingValue;
                string ratingStr = Rating.Replace(',', '.');  // Заменяем запятую на точку
                if (!double.TryParse(ratingStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out ratingValue))
                {
                    return BadRequest(new { error = "Рейтинг должен быть числом" });
                }

                Console.WriteLine("=== UPLOAD SERIES CALLED ===");
                Console.WriteLine($"Title: {Title}");
                Console.WriteLine($"YearStart: {YearStart}");
                Console.WriteLine($"GenreId: {GenreId}");
                Console.WriteLine($"LanguageId: {LanguageId}");
                Console.WriteLine($"Rating: {ratingValue}");
                Console.WriteLine($"Cast: {Cast}");
                Console.WriteLine($"ShortDescription: {ShortDescription}");
                Console.WriteLine($"FullDescription: {FullDescription}");
                Console.WriteLine($"posterImage: {posterImage?.FileName}");

                // Валидация
                if (string.IsNullOrEmpty(Title))
                    return BadRequest(new { error = "Название обязательно" });

                if (ratingValue < 0 || ratingValue > 10)
                    return BadRequest(new { error = "Рейтинг должен быть от 0 до 10" });

                if (YearStart < 1900 || YearStart > DateTime.Now.Year)
                    return BadRequest(new { error = "Год должен быть от 1900 до текущего" });

                if (GenreId <= 0)
                    return BadRequest(new { error = "Выберите жанр" });

                if (LanguageId <= 0)
                    return BadRequest(new { error = "Выберите язык" });

                if (string.IsNullOrEmpty(Cast))
                    return BadRequest(new { error = "Введите имена актёров" });

                if (string.IsNullOrEmpty(ShortDescription))
                    return BadRequest(new { error = "Введите краткое описание" });

                if (string.IsNullOrEmpty(FullDescription))
                    return BadRequest(new { error = "Введите полное описание" });

                if (posterImage == null || posterImage.Length == 0)
                    return BadRequest(new { error = "Загрузите постер" });

                if (posterImage.Length > 5 * 1024 * 1024)
                    return BadRequest(new { error = "Размер файла не должен превышать 5 МБ" });

                // Проверяем тип файла
                var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/webp" };
                if (!allowedTypes.Contains(posterImage.ContentType.ToLower()))
                    return BadRequest(new { error = "Допустимы только JPG, PNG и WEBP форматы" });

                // Проверяем существование жанра
                var genre = await _context.Genres.FindAsync(GenreId);
                if (genre == null)
                    return BadRequest(new { error = "Выбранный жанр не существует" });

                // Проверяем существование языка
                var language = await _context.Languages.FindAsync(LanguageId);
                if (language == null)
                    return BadRequest(new { error = "Выбранный язык не существует" });

                // Преобразуем файл в byte[]
                byte[] imageData;
                using (var memoryStream = new MemoryStream())
                {
                    await posterImage.CopyToAsync(memoryStream);
                    imageData = memoryStream.ToArray();
                }

                // Сохраняем в базу
                var series = new Series
                {
                    Title = Title,
                    TitleEn = TitleEn ?? Title,
                    ShortDescription = ShortDescription ?? "",
                    FullDescription = FullDescription ?? "",
                    YearStart = YearStart,
                    YearEnd = YearEnd,
                    GenreId = GenreId,
                    LanguageId = LanguageId,
                    Rating = ratingValue,
                    Cast = Cast ?? "",
                    PosterImage = imageData,
                    PosterContentType = posterImage.ContentType
                };

                _context.Series.Add(series);
                await _context.SaveChangesAsync();

                Console.WriteLine($"Сериал успешно добавлен с ID {series.Id}");

                return Ok(new { success = true, series = series });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ОШИБКА: {ex.Message}");
                Console.WriteLine($"СТЕК: {ex.StackTrace}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // GET: api/seriesapi/series/{id}
        [HttpGet("series/{id}")]
        public async Task<IActionResult> GetSeriesById(int id)
        {
            var series = await _context.Series
                .Include(s => s.Genre)
                .Include(s => s.Language)
                .Where(s => s.Id == id)
                .Select(s => new
                {
                    s.Id,
                    s.Title,
                    s.TitleEn,
                    s.ShortDescription,
                    s.FullDescription,
                    s.YearStart,
                    s.YearEnd,
                    s.Rating,
                    s.Cast,
                    GenreId = s.GenreId,
                    Genre = s.Genre != null ? s.Genre.Name : null,
                    LanguageId = s.LanguageId,
                    Language = s.Language != null ? s.Language.Name : null
                })
                .FirstOrDefaultAsync();

            if (series == null)
            {
                return NotFound(new { error = "Сериал не найден" });
            }

            return Ok(series);
        }
    }

    // Модель для запросов избранного
    public class FavoriteRequest
    {
        public int SeriesId { get; set; }
        public string? UserId { get; set; }
    }
}