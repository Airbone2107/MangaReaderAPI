using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Application.Common.Interfaces
{
    public interface IApplicationDbContext
    {
        DbSet<RefreshToken> RefreshTokens { get; }
        DbSet<Author> Authors { get; }
        DbSet<Tag> Tags { get; }
        DbSet<TagGroup> TagGroups { get; }
        DbSet<Manga> Mangas { get; }
        DbSet<Chapter> Chapters { get; }
        DbSet<ChapterPage> ChapterPages { get; }
        DbSet<CoverArt> CoverArts { get; }
        DbSet<MangaAuthor> MangaAuthors { get; }
        DbSet<MangaTag> MangaTags { get; }
        DbSet<TranslatedManga> TranslatedMangas { get; }

        ChangeTracker ChangeTracker { get; }
        DatabaseFacade Database { get; }

        /// <summary>
        /// Lưu các thay đổi vào cơ sở dữ liệu.
        /// </summary>
        /// <param name="cancellationToken">Token để hủy bỏ thao tác.</param>
        /// <returns>Số lượng state entries được ghi vào database.</returns>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
        
        /// <summary>
        /// Tạo một DbSet mới cho một entity type.
        /// DbSet này có thể được sử dụng để truy vấn và lưu các instance của TEntity.
        /// </summary>
        DbSet<TEntity> Set<TEntity>() where TEntity : class;
    }
} 