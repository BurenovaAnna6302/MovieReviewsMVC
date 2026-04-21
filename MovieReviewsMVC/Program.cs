using Microsoft.EntityFrameworkCore;
using MovieReviewsMVC.Models;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Добавляем контекст базы данных с PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Добавляем контроллеры с представлениями (MVC)
builder.Services.AddControllersWithViews();

// Добавляем API контроллеры с настройкой JSON
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

var app = builder.Build();

// Настройка HTTP pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Маршрутизация
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Pages}/{action=Index}/{id?}");

app.MapControllers();

// ==================== ИНИЦИАЛИЗАЦИЯ БАЗЫ ДАННЫХ ====================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<AppDbContext>();

    // Удаляем старую базу и создаем новую
    context.Database.EnsureDeleted();
    context.Database.EnsureCreated();

    // ==================== ЖАНРЫ ====================
    var genres = new List<Genre>
    {
        new Genre { Name = "Фэнтези" },
        new Genre { Name = "Драма" },
        new Genre { Name = "Криминал" },
        new Genre { Name = "Фантастика" }
    };
    context.Genres.AddRange(genres);

    // ==================== ЯЗЫКИ ====================
    var languages = new List<Language>
    {
        new Language { Name = "Русский" },
        new Language { Name = "Английский" },
        new Language { Name = "Немецкий" },
        new Language { Name = "Французский" }
    };
    context.Languages.AddRange(languages);

    context.SaveChanges();

    // Получаем ID жанров
    var fantasy = context.Genres.First(g => g.Name == "Фэнтези");
    var drama = context.Genres.First(g => g.Name == "Драма");
    var crime = context.Genres.First(g => g.Name == "Криминал");
    var sciFi = context.Genres.First(g => g.Name == "Фантастика");

    // Получаем ID языков
    var russian = context.Languages.First(l => l.Name == "Русский");
    var english = context.Languages.First(l => l.Name == "Английский");
    var german = context.Languages.First(l => l.Name == "Немецкий");
    var french = context.Languages.First(l => l.Name == "Французский");

    // ==================== ФУНКЦИЯ ДЛЯ ЗАГРУЗКИ КАРТИНКИ ====================
    byte[] LoadImage(string imagePath)
    {
        var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", imagePath);
        if (File.Exists(fullPath))
        {
            return File.ReadAllBytes(fullPath);
        }
        Console.WriteLine($"Файл не найден: {fullPath}");
        return null;
    }

    // ==================== ВСЕ СЕРИАЛЫ (BLOB) ====================

    // === ФЭНТЕЗИ (английские) ===
    context.Series.AddRange(
        new Series
        {
            Title = "Ведьмак",
            TitleEn = "The Witcher",
            ShortDescription = "Ведьмак Геральт путешествует по Континенту, убивая чудовищ.",
            FullDescription = "Ведьмак Геральт, мутант и убийца чудовищ, на своей верной лошади по кличке Плотва путешествует по Континенту.",
            YearStart = 2019,
            GenreId = fantasy.Id,
            LanguageId = english.Id,
            Rating = 8.2,
            Cast = "Генри Кавилл, Аня Чалотра, Фрейя Аллан",
            PosterImage = LoadImage("img/ведьмак.jpg"),
            PosterContentType = "image/jpeg"
        },
        new Series
        {
            Title = "Игра престолов",
            TitleEn = "Game of Thrones",
            ShortDescription = "Эпическая сага о борьбе за Железный трон.",
            FullDescription = "В мире Вестероса несколько благородных семейств борются за власть над Семью королевствами.",
            YearStart = 2011,
            YearEnd = 2019,
            GenreId = fantasy.Id,
            LanguageId = english.Id,
            Rating = 9.2,
            Cast = "Эмилия Кларк, Кит Харингтон, Питер Динклэйдж",
            PosterImage = LoadImage("img/игра_престолов.jpg"),
            PosterContentType = "image/jpeg"
        },
        new Series
        {
            Title = "Атака титанов",
            TitleEn = "Attack on Titan",
            ShortDescription = "Человечество борется за выживание против гигантских титанов.",
            FullDescription = "В мире, где остатки человечества живут за огромными стенами, защищаясь от гигантских титанов.",
            YearStart = 2013,
            YearEnd = 2023,
            GenreId = fantasy.Id,
            LanguageId = english.Id,
            Rating = 9.0,
            Cast = "Юки Кадзи, Юи Исикава",
            PosterImage = LoadImage("img/атака_титанов.jpg"),
            PosterContentType = "image/jpeg"
        }
    );

    // === КРИМИНАЛ (русский) ===
    context.Series.AddRange(
        new Series
        {
            Title = "Слово пацана",
            TitleEn = "Slovo Patsana",
            ShortDescription = "Криминальная драма о жизни в Казани в конце 80-х.",
            FullDescription = "История о молодежных уличных группировках в Казани конца 1980-х годов.",
            YearStart = 2023,
            GenreId = crime.Id,
            LanguageId = russian.Id,
            Rating = 8.5,
            Cast = "Иван Янковский, Рузиль Минекаев, Леон Кемстач",
            PosterImage = LoadImage("img/слово_пацана.jpg"),
            PosterContentType = "image/jpeg"
        }
    );

    // === КРИМИНАЛ (английские) ===
    context.Series.AddRange(
        new Series
        {
            Title = "Во все тяжкие",
            TitleEn = "Breaking Bad",
            ShortDescription = "Учитель химии начинает производство метамфетамина.",
            FullDescription = "Школьный учитель химии Уолтер Уайт узнает, что болен раком легких. Чтобы обеспечить будущее своей семьи, он начинает производство метамфетамина.",
            YearStart = 2008,
            YearEnd = 2013,
            GenreId = crime.Id,
            LanguageId = english.Id,
            Rating = 9.5,
            Cast = "Брайан Крэнстон, Аарон Пол, Анна Ганн",
            PosterImage = LoadImage("img/во_все_тяжкие.jpg"),
            PosterContentType = "image/jpeg"
        },
        new Series
        {
            Title = "Шерлок",
            TitleEn = "Sherlock",
            ShortDescription = "Современная адаптация историй о Шерлоке Холмсе.",
            FullDescription = "Детектив Шерлок Холмс и доктор Ватсон расследуют запутанные преступления в современном Лондоне.",
            YearStart = 2010,
            YearEnd = 2017,
            GenreId = crime.Id,
            LanguageId = english.Id,
            Rating = 8.8,
            Cast = "Бенедикт Камбербэтч, Мартин Фриман",
            PosterImage = LoadImage("img/шерлок.jpg"),
            PosterContentType = "image/jpeg"
        }
    );

    // === ФАНТАСТИКА (английские) ===
    context.Series.AddRange(
        new Series
        {
            Title = "Очень странные дела",
            TitleEn = "Stranger Things",
            ShortDescription = "Дети из маленького городка сталкиваются с потусторонними силами.",
            FullDescription = "После исчезновения мальчика группа друзей сталкивается с секретными экспериментами и потусторонними силами.",
            YearStart = 2016,
            YearEnd = 2025,
            GenreId = sciFi.Id,
            LanguageId = english.Id,
            Rating = 8.7,
            Cast = "Милли Бобби Браун, Финн Вулфхард, Дэвид Харбор",
            PosterImage = LoadImage("img/странные_дела.jpg"),
            PosterContentType = "image/jpeg"
        },
        new Series
        {
            Title = "Черное зеркало",
            TitleEn = "Black Mirror",
            ShortDescription = "Антология о темной стороне технологий.",
            FullDescription = "Антология научно-фантастических историй, исследующих темные аспекты современных технологий.",
            YearStart = 2011,
            YearEnd = 2023,
            GenreId = sciFi.Id,
            LanguageId = english.Id,
            Rating = 8.7,
            Cast = "Разные актеры",
            PosterImage = LoadImage("img/черное_зеркало.jpg"),
            PosterContentType = "image/jpeg"
        },
        new Series
        {
            Title = "Игра в кальмара",
            TitleEn = "Squid Game",
            ShortDescription = "Люди играют в смертельные игры ради денег.",
            FullDescription = "Группа людей с финансовыми проблемами принимает участие в тайных играх на выживание с огромным денежным призом.",
            YearStart = 2021,
            GenreId = sciFi.Id,
            LanguageId = english.Id,
            Rating = 8.0,
            Cast = "Ли Джон Джэ, Пак Хэ Су, Чон Хо Ён",
            PosterImage = LoadImage("img/игра_в_кальмара.jpg"),
            PosterContentType = "image/jpeg"
        }
    );

    // === ДРАМА (английские) ===
    context.Series.AddRange(
        new Series
        {
            Title = "Ход королевы",
            TitleEn = "The Queen's Gambit",
            ShortDescription = "Драма о гениальной шахматистке.",
            FullDescription = "История Бет Хармон — сироты, которая становится лучшей шахматисткой в мире.",
            YearStart = 2020,
            GenreId = drama.Id,
            LanguageId = english.Id,
            Rating = 8.5,
            Cast = "Аня Тейлор-Джой, Томас Броди-Сэнгстер",
            PosterImage = LoadImage("img/ход_королевы.jpg"),
            PosterContentType = "image/jpeg"
        },
        new Series
        {
            Title = "Корона",
            TitleEn = "The Crown",
            ShortDescription = "История правления королевы Елизаветы II.",
            FullDescription = "Историческая драма о жизни и правлении королевы Елизаветы II.",
            YearStart = 2016,
            YearEnd = 2023,
            GenreId = drama.Id,
            LanguageId = english.Id,
            Rating = 8.6,
            Cast = "Клэр Фой, Оливия Колман",
            PosterImage = LoadImage("img/корона.jpg"),
            PosterContentType = "image/jpeg"
        }
    );

    // === ДРАМА (русские) ===
    context.Series.AddRange(
        new Series
        {
            Title = "Триггер",
            TitleEn = "Trigger",
            ShortDescription = "Психолог с нестандартными методами работы.",
            FullDescription = "Психолог Артем Стрелецкий использует провокационные методы лечения.",
            YearStart = 2018,
            GenreId = drama.Id,
            LanguageId = russian.Id,
            Rating = 8.0,
            Cast = "Максим Матвеев, Светлана Иванова",
            PosterImage = LoadImage("img/триггер.jpg"),
            PosterContentType = "image/jpeg"
        }
    );

    // === КРИМИНАЛ (русские) ===
    context.Series.AddRange(
        new Series
        {
            Title = "Метод",
            TitleEn = "Method",
            ShortDescription = "Русский детективный сериал о необычном следователе.",
            FullDescription = "Следователь Меглин использует нестандартные методы расследования.",
            YearStart = 2015,
            GenreId = crime.Id,
            LanguageId = russian.Id,
            Rating = 8.2,
            Cast = "Константин Хабенский, Паулина Андреева",
            PosterImage = LoadImage("img/метод.jpg"),
            PosterContentType = "image/jpeg"
        }
    );

    // === ФЭНТЕЗИ (русские, немецкие, французские) ===
    context.Series.AddRange(
        new Series
        {
            Title = "Топи",
            TitleEn = "Bogs",
            ShortDescription = "Мистический триллер о группе людей в глухой деревне.",
            FullDescription = "Пятеро незнакомцев приезжают в деревню Топи, где происходят загадочные события.",
            YearStart = 2021,
            GenreId = fantasy.Id,
            LanguageId = russian.Id,
            Rating = 7.0,
            Cast = "Иван Янковский, Тихон Жизневский",
            PosterImage = LoadImage("img/топи.jpg"),
            PosterContentType = "image/jpeg"
        },
        new Series
        {
            Title = "Монастырь",
            TitleEn = "Monastery",
            ShortDescription = "Мистическая драма о тайнах старинного монастыря.",
            FullDescription = "Журналистка приезжает в отдаленный монастырь и сталкивается с древним злом.",
            YearStart = 2022,
            GenreId = fantasy.Id,
            LanguageId = russian.Id,
            Rating = 6.8,
            Cast = "Полина Максимова, Филипп Янковский",
            PosterImage = LoadImage("img/монастырь.jpg"),
            PosterContentType = "image/jpeg"
        },
        new Series
        {
            Title = "Колесо времени",
            TitleEn = "The Wheel of Time",
            ShortDescription = "Эпическое фэнтези по бестселлеру Роберта Джордана.",
            FullDescription = "Могущественная организация Айз Седай ищет человека, который спасет или уничтожит мир.",
            YearStart = 2021,
            GenreId = fantasy.Id,
            LanguageId = english.Id,
            Rating = 7.2,
            Cast = "Розамунд Пайк, Дэниел Хенни",
            PosterImage = LoadImage("img/колесо_времени.jpg"),
            PosterContentType = "image/jpeg"
        },
        new Series
        {
            Title = "Темный кристалл",
            TitleEn = "Dark Crystal",
            ShortDescription = "Эпическое фэнтези о мире магии и приключений.",
            FullDescription = "Приключения в мире, где магия и опасность подстерегают на каждом шагу.",
            YearStart = 2019,
            YearEnd = 2019,
            GenreId = fantasy.Id,
            LanguageId = german.Id,
            Rating = 8.0,
            Cast = "Немецкие актеры",
            PosterImage = LoadImage("img/темный_кристалл.jpg"),
            PosterContentType = "image/jpeg"
        },
        new Series
        {
            Title = "Вампиры",
            TitleEn = "Vampires",
            ShortDescription = "Французское фэнтези о мире вампиров.",
            FullDescription = "История о семье вампиров, живущих в современном Париже.",
            YearStart = 2020,
            GenreId = fantasy.Id,
            LanguageId = french.Id,
            Rating = 7.0,
            Cast = "Сюзанн Клеман, Уго Бекер",
            PosterImage = LoadImage("img/вампиры.jpg"),
            PosterContentType = "image/jpeg"
        },
        new Series
        {
            Title = "Варвар",
            TitleEn = "Barbare",
            ShortDescription = "Французское фэнтези с элементами комедии.",
            FullDescription = "Необычная история о варваре, попавшем в современный мир.",
            YearStart = 2019,
            GenreId = fantasy.Id,
            LanguageId = french.Id,
            Rating = 6.9,
            Cast = "Французские актеры",
            PosterImage = LoadImage("img/варвар.jpg"),
            PosterContentType = "image/jpeg"
        }
    );

    // === ДРАМА (английские, русские, немецкие, французские) ===
    context.Series.AddRange(
        new Series
        {
            Title = "Мир Дикого Запада",
            TitleEn = "Westworld",
            ShortDescription = "Футуристический парк развлечений выходит из-под контроля.",
            FullDescription = "В парке развлечений для богатых андроиды начинают восставать против людей.",
            YearStart = 2016,
            YearEnd = 2022,
            GenreId = drama.Id,
            LanguageId = english.Id,
            Rating = 8.5,
            Cast = "Эван Рэйчел Вуд, Энтони Хопкинс",
            PosterImage = LoadImage("img/мир_дикого_запада.jpg"),
            PosterContentType = "image/jpeg"
        },
        new Series
        {
            Title = "Наследники",
            TitleEn = "Succession",
            ShortDescription = "Борьба за контроль над медиа-империей.",
            FullDescription = "Семья медиа-магнатов борется за контроль над компанией.",
            YearStart = 2018,
            YearEnd = 2023,
            GenreId = drama.Id,
            LanguageId = english.Id,
            Rating = 8.8,
            Cast = "Брайан Кокс, Джереми Стронг",
            PosterImage = LoadImage("img/наследники.jpg"),
            PosterContentType = "image/jpeg"
        },
        new Series
        {
            Title = "Нулевой пациент",
            TitleEn = "Patient Zero",
            ShortDescription = "Драма о вспышке ВИЧ в СССР.",
            FullDescription = "История о первых случаях ВИЧ в Советском Союзе.",
            YearStart = 2022,
            GenreId = drama.Id,
            LanguageId = russian.Id,
            Rating = 8.2,
            Cast = "Никита Ефремов, Ирина Старшенбаум",
            PosterImage = LoadImage("img/нулевой_пациент.jpg"),
            PosterContentType = "image/jpeg"
        },
        new Series
        {
            Title = "Хрустальный",
            TitleEn = "Crystal",
            ShortDescription = "Детективная драма о поисках серийного убийцы.",
            FullDescription = "Следователь ищет маньяка, оставляющего кристаллы на местах преступлений.",
            YearStart = 2021,
            GenreId = drama.Id,
            LanguageId = russian.Id,
            Rating = 7.7,
            Cast = "Алексей Гуськов, Дарья Мороз",
            PosterImage = LoadImage("img/хрустальный.jpg"),
            PosterContentType = "image/jpeg"
        },
        new Series
        {
            Title = "Вавилон-Берлин",
            TitleEn = "Babylon Berlin",
            ShortDescription = "Немецкая историческая драма о Берлине 1920-х.",
            FullDescription = "Детектив Гереон Рат расследует преступления в бурлящем Берлине 1920-х годов.",
            YearStart = 2017,
            GenreId = drama.Id,
            LanguageId = german.Id,
            Rating = 8.4,
            Cast = "Фолькер Брух, Лив Лиза Фрис",
            PosterImage = LoadImage("img/вавилон_берлин.jpg"),
            PosterContentType = "image/jpeg"
        },
        new Series
        {
            Title = "Тьма",
            TitleEn = "Dark",
            ShortDescription = "Немецкий научно-фантастический триллер о путешествиях во времени.",
            FullDescription = "Исчезновение ребенка раскрывает тайны четырех семей и путешествий во времени.",
            YearStart = 2017,
            YearEnd = 2020,
            GenreId = drama.Id,
            LanguageId = german.Id,
            Rating = 8.7,
            Cast = "Луис Хофманн, Карина Визе",
            PosterImage = LoadImage("img/тьма.jpg"),
            PosterContentType = "image/jpeg"
        },
        new Series
        {
            Title = "Версаль",
            TitleEn = "Versailles",
            ShortDescription = "Французская историческая драма о короле Людовике XIV.",
            FullDescription = "История строительства Версальского дворца и правления короля Людовика XIV.",
            YearStart = 2015,
            YearEnd = 2018,
            GenreId = drama.Id,
            LanguageId = french.Id,
            Rating = 8.0,
            Cast = "Джордж Благден, Александр Влахос",
            PosterImage = LoadImage("img/версаль.jpg"),
            PosterContentType = "image/jpeg"
        },
        new Series
        {
            Title = "Люпен",
            TitleEn = "Lupin",
            ShortDescription = "Французский детективный сериал о грабителе.",
            FullDescription = "Вдохновленный историей Арсена Люпена, профессиональный вор решает отомстить за своего отца.",
            YearStart = 2021,
            GenreId = drama.Id,
            LanguageId = french.Id,
            Rating = 7.5,
            Cast = "Омар Си, Людивин Санье",
            PosterImage = LoadImage("img/люпен.jpg"),
            PosterContentType = "image/jpeg"
        }
    );

    // === КРИМИНАЛ (английские, русские, немецкие, французские) ===
    context.Series.AddRange(
        new Series
        {
            Title = "Острые козырьки",
            TitleEn = "Peaky Blinders",
            ShortDescription = "Британская криминальная драма о семье Шелби.",
            FullDescription = "Семья Шелби правит Бирмингемом после Первой мировой войны.",
            YearStart = 2013,
            YearEnd = 2022,
            GenreId = crime.Id,
            LanguageId = english.Id,
            Rating = 8.8,
            Cast = "Киллиан Мерфи, Том Харди",
            PosterImage = LoadImage("img/острые_козырьки.jpg"),
            PosterContentType = "image/jpeg"
        },
        new Series
        {
            Title = "Нарко",
            TitleEn = "Narcos",
            ShortDescription = "Криминальная драма о наркобаронах Колумбии.",
            FullDescription = "История Пабло Эскобара и других наркобаронов Колумбии.",
            YearStart = 2015,
            YearEnd = 2017,
            GenreId = crime.Id,
            LanguageId = english.Id,
            Rating = 8.8,
            Cast = "Вагнер Моура, Педро Паскаль",
            PosterImage = LoadImage("img/нарко.jpg"),
            PosterContentType = "image/jpeg"
        },
        new Series
        {
            Title = "Перевал Дятлова",
            TitleEn = "Dead Mountain",
            ShortDescription = "Русский детектив о загадочной гибели туристов.",
            FullDescription = "Расследование трагической гибели группы туристов на перевале Дятлова.",
            YearStart = 2020,
            GenreId = crime.Id,
            LanguageId = russian.Id,
            Rating = 7.5,
            Cast = "Петр Федоров, Мария Луговая",
            PosterImage = LoadImage("img/перевал_дятлова.jpg"),
            PosterContentType = "image/jpeg"
        },
        new Series
        {
            Title = "Король и Шут",
            TitleEn = "King and Jester",
            ShortDescription = "Биографическая драма о легендарной панк-группе.",
            FullDescription = "История группы Король и Шут, смесь реальности и мистики.",
            YearStart = 2023,
            GenreId = crime.Id,
            LanguageId = russian.Id,
            Rating = 8.0,
            Cast = "Константин Плотников, Влад Коноплёв",
            PosterImage = LoadImage("img/король_и_шут.jpg"),
            PosterContentType = "image/jpeg"
        },
        new Series
        {
            Title = "Как продавать наркотики онлайн (быстро)",
            TitleEn = "How to Sell Drugs Online (Fast)",
            ShortDescription = "Немецкий комедийно-криминальный сериал.",
            FullDescription = "Школьник создает онлайн-империю по продаже наркотиков.",
            YearStart = 2019,
            GenreId = crime.Id,
            LanguageId = german.Id,
            Rating = 7.8,
            Cast = "Максимилиан Мундт, Данна Лёвиц",
            PosterImage = LoadImage("img/наркотики_онлайн.jpg"),
            PosterContentType = "image/jpeg"
        },
        new Series
        {
            Title = "Связь",
            TitleEn = "Le Bureau des Légendes",
            ShortDescription = "Французский шпионский триллер.",
            FullDescription = "История о французских разведчиках и их сложной работе.",
            YearStart = 2015,
            YearEnd = 2017,
            GenreId = crime.Id,
            LanguageId = french.Id,
            Rating = 8.6,
            Cast = "Маттьё Кассовиц, Жан-Пьер Дарруссен",
            PosterImage = LoadImage("img/связь.jpg"),
            PosterContentType = "image/jpeg"
        },
        new Series
        {
            Title = "Код Лико",
            TitleEn = "Liaison",
            ShortDescription = "Французский триллер о кибератаках.",
            FullDescription = "Бывшая пара должна объединиться, чтобы остановить кибератаки.",
            YearStart = 2021,
            GenreId = crime.Id,
            LanguageId = french.Id,
            Rating = 7.2,
            Cast = "Венсан Кассель, Ева Грин",
            PosterImage = LoadImage("img/код_лико.jpg"),
            PosterContentType = "image/jpeg"
        }
    );

    // === ФАНТАСТИКА (русские, немецкие, французские) ===
    context.Series.AddRange(
        new Series
        {
            Title = "Эпидемия",
            TitleEn = "Epidemic",
            ShortDescription = "Русский постапокалиптический триллер.",
            FullDescription = "Москву поражает смертельный вирус, выжившие пытаются спастись.",
            YearStart = 2019,
            GenreId = sciFi.Id,
            LanguageId = russian.Id,
            Rating = 7.3,
            Cast = "Кирилл Кяро, Виктория Исакова",
            PosterImage = LoadImage("img/эпидемия.jpg"),
            PosterContentType = "image/jpeg"
        },
        new Series
        {
            Title = "Метаморфоза",
            TitleEn = "Metamorphosis",
            ShortDescription = "Русский фантастический триллер о мутации.",
            FullDescription = "Люди начинают мутировать, мир погружается в хаос.",
            YearStart = 2022,
            GenreId = sciFi.Id,
            LanguageId = russian.Id,
            Rating = 6.5,
            Cast = "Алексей Серебряков, Анна Михалкова",
            PosterImage = LoadImage("img/метаморфоза.jpg"),
            PosterContentType = "image/jpeg"
        },
        new Series
        {
            Title = "Первобытный",
            TitleEn = "Primitive",
            ShortDescription = "Немецкий научно-фантастический сериал.",
            FullDescription = "Группа ученых отправляется в опасную экспедицию.",
            YearStart = 2020,
            GenreId = sciFi.Id,
            LanguageId = german.Id,
            Rating = 7.0,
            Cast = "Немецкие актеры",
            PosterImage = LoadImage("img/первобытный.jpg"),
            PosterContentType = "image/jpeg"
        },
        new Series
        {
            Title = "Основание",
            TitleEn = "Foundation",
            ShortDescription = "Французская экранизация научной фантастики.",
            FullDescription = "Галактическая империя рушится, ученый предсказывает темные века.",
            YearStart = 2021,
            YearEnd = 2023,
            GenreId = sciFi.Id,
            LanguageId = french.Id,
            Rating = 7.6,
            Cast = "Лу Льобель, Марк Хант",
            PosterImage = LoadImage("img/основание.jpg"),
            PosterContentType = "image/jpeg"
        },
        new Series
        {
            Title = "Миссия",
            TitleEn = "Mission",
            ShortDescription = "Французский фантастический сериал о космосе.",
            FullDescription = "Экипаж космического корабля сталкивается с неизведанным.",
            YearStart = 2020,
            GenreId = sciFi.Id,
            LanguageId = french.Id,
            Rating = 7.1,
            Cast = "Французские актеры",
            PosterImage = LoadImage("img/миссия.jpg"),
            PosterContentType = "image/jpeg"
        },
        new Series
        {
            Title = "Рассказ служанки",
            TitleEn = "The Handmaid's Tale",
            ShortDescription = "Антиутопическая драма о тоталитарном режиме.",
            FullDescription = "В республике Гилеад женщины-служанки рожают детей для правящего класса.",
            YearStart = 2017,
            YearEnd = 2022,
            GenreId = sciFi.Id,
            LanguageId = english.Id,
            Rating = 8.4,
            Cast = "Элизабет Мосс, Ивонн Страховски",
            PosterImage = LoadImage("img/рассказ_служанки.jpg"),
            PosterContentType = "image/jpeg"
        }
    );

    context.SaveChanges();

    Console.WriteLine($"База данных инициализирована. Добавлено сериалов: {context.Series.Count()}");
}

app.Run();