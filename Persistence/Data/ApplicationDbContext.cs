using Application.Common.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Persistence.Data
{
    /// <summary>
    /// Database context chính của ứng dụng quản lý tất cả các entity
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IApplicationDbContext
    {
        /// <summary>
        /// Khởi tạo một instance mới của ApplicationDbContext
        /// </summary>
        /// <param name="options">Tùy chọn kết nối</param>
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
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

        // Các phương thức SaveChangesAsync, Set<TEntity> đã được cung cấp bởi DbContext
        // và khớp với yêu cầu của IApplicationDbContext.
        // ChangeTracker và Database cũng là thuộc tính của DbContext.

        /// <summary>
        /// Cấu hình các entity và quan hệ giữa chúng khi tạo database
        /// </summary>
        /// <param name="modelBuilder">Builder để xây dựng mô hình dữ liệu</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Cấu hình RefreshToken
            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.HasKey(rt => rt.Id);
                entity.HasIndex(rt => rt.Token).IsUnique();
                entity.Property(rt => rt.Token).IsRequired();
                entity.HasOne(rt => rt.ApplicationUser)
                      .WithMany(u => u.RefreshTokens)
                      .HasForeignKey(rt => rt.ApplicationUserId);
            });

            // --- Author ---
            modelBuilder.Entity<Author>(entity =>
            {
                entity.HasKey(e => e.AuthorId);
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

                entity.HasOne(c => c.TranslatedManga)
                      .WithMany(tm => tm.Chapters)
                      .HasForeignKey(c => c.TranslatedMangaId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(c => c.User)
                      .WithMany(u => u.Chapters)
                      .HasForeignKey(c => c.UploadedByUserId)
                      .OnDelete(DeleteBehavior.Restrict);
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