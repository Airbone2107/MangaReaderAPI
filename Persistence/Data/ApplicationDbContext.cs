using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Persistence.Data
{
    /// <summary>
    /// Database context chính của ứng dụng quản lý tất cả các entity
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        /// <summary>
        /// Khởi tạo một instance mới của ApplicationDbContext
        /// </summary>
        /// <param name="options">Tùy chọn kết nối</param>
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Author> Authors { get; set; } = null!;
        public DbSet<Tag> Tags { get; set; } = null!;
        public DbSet<TagGroup> TagGroups { get; set; } = null!;
        public DbSet<Manga> Mangas { get; set; } = null!;
        public DbSet<Chapter> Chapters { get; set; } = null!;
        public DbSet<ChapterPage> ChapterPages { get; set; } = null!;
        public DbSet<CoverArt> CoverArts { get; set; } = null!;
        public DbSet<MangaAuthor> MangaAuthors { get; set; } = null!;
        public DbSet<MangaTag> MangaTags { get; set; } = null!;
        public DbSet<TranslatedManga> TranslatedMangas { get; set; } = null!;

        /// <summary>
        /// Cấu hình các entity và quan hệ giữa chúng khi tạo database
        /// </summary>
        /// <param name="modelBuilder">Builder để xây dựng mô hình dữ liệu</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- User ---
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.UserId);
                entity.HasIndex(u => u.Username).IsUnique();
            });

            // --- Author ---
            modelBuilder.Entity<Author>(entity =>
            {
                entity.HasKey(e => e.AuthorId);
                // Version đã bị xóa
            });

            // --- TagGroup (Mới) ---
            modelBuilder.Entity<TagGroup>(entity =>
            {
                entity.HasKey(tg => tg.TagGroupId);
                entity.HasIndex(tg => tg.Name).IsUnique();

                entity.HasMany(tg => tg.Tags)
                      .WithOne(t => t.TagGroup)
                      .HasForeignKey(t => t.TagGroupId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // --- Tag ---
            modelBuilder.Entity<Tag>(entity =>
            {
                entity.HasKey(e => e.TagId);
                // Thuộc tính Group (string) đã bị loại bỏ khỏi cấu hình này
                // entity.Property(e => e.Group) // Dòng này không còn nữa

                // Quan hệ với TagGroup đã được cấu hình trong TagGroup entity
            });

            // --- Manga ---
            modelBuilder.Entity<Manga>(entity =>
            {
                entity.HasKey(e => e.MangaId);

                entity.Property(m => m.Status)
                      .HasConversion(new EnumToStringConverter<MangaStatus>());

                entity.Property(m => m.ContentRating)
                      .HasConversion(new EnumToStringConverter<ContentRating>());

                entity.Property(m => m.PublicationDemographic)
                      .IsRequired(false)
                      .HasConversion(new EnumToStringConverter<PublicationDemographic>());
                // Version đã bị xóa

                // Quan hệ 1-N với TranslatedManga
                entity.HasMany(m => m.TranslatedMangas)
                      .WithOne(tm => tm.Manga)
                      .HasForeignKey(tm => tm.MangaId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // --- TranslatedManga ---
            modelBuilder.Entity<TranslatedManga>(entity =>
            {
                entity.HasKey(tm => tm.TranslatedMangaId);
                entity.HasIndex(tm => new { tm.MangaId, tm.LanguageKey }).IsUnique();
            });

            // --- Chapter ---
            modelBuilder.Entity<Chapter>(entity =>
            {
                entity.HasKey(e => e.ChapterId);

                // Quan hệ với TranslatedManga
                entity.HasOne(c => c.TranslatedManga)
                      .WithMany(tm => tm.Chapters)
                      .HasForeignKey(c => c.TranslatedMangaId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Quan hệ với User (người đăng tải)
                entity.HasOne(c => c.User)
                      .WithMany(u => u.Chapters)
                      .HasForeignKey(c => c.UploadedByUserId)
                      .OnDelete(DeleteBehavior.Cascade);
                
                // ExternalUrl đã bị loại bỏ, không cần cấu hình
            });

            // --- ChapterPage ---
            modelBuilder.Entity<ChapterPage>(entity =>
            {
                entity.HasKey(e => e.PageId);

                entity.HasOne(cp => cp.Chapter)
                      .WithMany(c => c.ChapterPages)
                      .HasForeignKey(cp => cp.ChapterId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(cp => new { cp.ChapterId, cp.PageNumber })
                      .IsUnique();
            });

            // --- CoverArt ---
            modelBuilder.Entity<CoverArt>(entity =>
            {
                entity.HasKey(e => e.CoverId);
                entity.HasOne(ca => ca.Manga)
                      .WithMany(m => m.CoverArts)
                      .HasForeignKey(ca => ca.MangaId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // --- MangaAuthor (Bảng join) ---
            modelBuilder.Entity<MangaAuthor>(entity =>
            {
                entity.HasKey(ma => new { ma.MangaId, ma.AuthorId, ma.Role });

                entity.Property(ma => ma.Role)
                      .HasConversion(new EnumToStringConverter<MangaStaffRole>());

                entity.HasOne(ma => ma.Manga)
                      .WithMany(m => m.MangaAuthors)
                      .HasForeignKey(ma => ma.MangaId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ma => ma.Author)
                      .WithMany(a => a.MangaAuthors)
                      .HasForeignKey(ma => ma.AuthorId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // --- MangaTag (Bảng join) ---
            modelBuilder.Entity<MangaTag>(entity =>
            {
                entity.HasKey(mt => new { mt.MangaId, mt.TagId });

                entity.HasOne(mt => mt.Manga)
                      .WithMany(m => m.MangaTags)
                      .HasForeignKey(mt => mt.MangaId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(mt => mt.Tag)
                      .WithMany(t => t.MangaTags)
                      .HasForeignKey(mt => mt.TagId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
