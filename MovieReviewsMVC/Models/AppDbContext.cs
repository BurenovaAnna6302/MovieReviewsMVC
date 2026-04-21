using Microsoft.EntityFrameworkCore;

namespace MovieReviewsMVC.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Movie> Movies { get; set; }
        public DbSet<Series> Series { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<Language> Languages { get; set; }
        public DbSet<Favorite> Favorites { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Series>().ToTable("Series");
            modelBuilder.Entity<Genre>().ToTable("Genres");
            modelBuilder.Entity<Language>().ToTable("Languages");
            modelBuilder.Entity<Favorite>().ToTable("Favorites");
            modelBuilder.Entity<Movie>().ToTable("Movies");

            modelBuilder.Entity<Favorite>()
                .HasIndex(f => new { f.SeriesId, f.UserId })
                .IsUnique();

            modelBuilder.Entity<Series>()
                .HasOne(s => s.Genre)
                .WithMany()
                .HasForeignKey(s => s.GenreId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Series>()
                .HasOne(s => s.Language)
                .WithMany()
                .HasForeignKey(s => s.LanguageId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}