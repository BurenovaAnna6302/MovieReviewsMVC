using Microsoft.AspNetCore.Mvc;

namespace MovieReviewsMVC.Controllers
{
    public class PagesController : Controller
    {
        // Главная страница
        public IActionResult Index()
        {
            return View();
        }

        // О нас
        public IActionResult About()
        {
            return View();
        }

        // Страница актера
        public IActionResult Actor()
        {
            return View();
        }

        // Каталог актеров
        public IActionResult CatalogActors()
        {
            return View();
        }

        // Избранное
        public IActionResult CatalogFavorites()
        {
            return View();
        }

        // Каталог фильмов
        public IActionResult CatalogFilm()
        {
            return View();
        }

        // Каталог сериалов
        public IActionResult CatalogSerial()
        {
            return View();
        }

        // Эксклюзивы
        public IActionResult Exclusive()
        {
            return View();
        }

        // Страница фильма
        public IActionResult Film()
        {
            return View();
        }

        // Страница сериала
        public IActionResult Serial()
        {
            return View();
        }

        // Страница добавления сериала
        public IActionResult AddSeries()
        {
            return View();
        }
    }
}