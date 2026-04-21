using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieReviewsMVC.Models;

namespace MovieReviewsMVC.Controllers
{
    public class MoviesController : Controller
    {
        private readonly AppDbContext _context;

        public MoviesController(AppDbContext context)
        {
            _context = context;
        }

        // Главная страница со списком фильмов и фильтрами
        public async Task<IActionResult> Index(string? genre, int? year, double? minRating)
        {
            var movies = _context.Movies.AsQueryable();

            // Применяем фильтры
            if (!string.IsNullOrEmpty(genre))
            {
                movies = movies.Where(m => m.Genre == genre);
            }

            if (year.HasValue)
            {
                movies = movies.Where(m => m.Year == year.Value);
            }

            if (minRating.HasValue)
            {
                movies = movies.Where(m => m.Rating >= minRating.Value);
            }

            // Получаем уникальные жанры для выпадающего списка
            ViewBag.Genres = await _context.Movies.Select(m => m.Genre).Distinct().OrderBy(g => g).ToListAsync();
            ViewBag.Years = await _context.Movies.Select(m => m.Year).Distinct().OrderByDescending(y => y).ToListAsync();

            // Сохраняем выбранные фильтры для отображения в представлении
            ViewBag.SelectedGenre = genre;
            ViewBag.SelectedYear = year;
            ViewBag.SelectedMinRating = minRating;

            return View(await movies.ToListAsync());
        }

        // Детальная страница фильма
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movies.FirstOrDefaultAsync(m => m.Id == id);
            if (movie == null)
            {
                return NotFound();
            }

            return View(movie);
        }
    }
}