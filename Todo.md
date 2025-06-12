# TODO: Khắc phục lỗi "already being tracked" khi xóa Manga

Lỗi "The instance of entity type 'TagGroup' cannot be tracked because another instance with the same key value for {'TagGroupId'} is already being tracked" xảy ra khi xóa Manga. Nguyên nhân là do truyền một entity không được theo dõi (loaded với `AsNoTracking()`) vào phương thức `DbSet.Remove()`, khiến EF Core cố gắng attach lại một graph object mà một phần của nó có thể đã được theo dõi từ trước.

## Các bước thực hiện

### Bước 1: Cập nhật `DeleteMangaCommandHandler.cs`

Thay đổi cách `DeleteMangaCommandHandler` xử lý việc xóa Manga.
1.  Sử dụng `GetMangaWithDetailsAsync` (vẫn giữ `AsNoTracking()` trong `MangaRepository`) để lấy thông tin cần thiết cho việc xóa các file liên quan trên Cloudinary.
2.  Sau khi xóa file, gọi `_unitOfWork.MangaRepository.DeleteAsync(request.MangaId)` (phiên bản nhận `Guid`) để xóa Manga khỏi cơ sở dữ liệu. Phương thức này trong `GenericRepository` sẽ đảm bảo Manga được tải và theo dõi đúng cách trước khi xóa.

**File cần thay đổi:** `Application\Features\Mangas\Commands\DeleteManga\DeleteMangaCommandHandler.cs`

**Nội dung file đầy đủ sau khi thay đổi:**

```csharp
// Application\Features\Mangas\Commands\DeleteManga\DeleteMangaCommandHandler.cs
using Application.Common.Interfaces; // Cho IPhotoAccessor
using Application.Contracts.Persistence;
using Application.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Mangas.Commands.DeleteManga
{
    public class DeleteMangaCommandHandler : IRequestHandler<DeleteMangaCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPhotoAccessor _photoAccessor;
        private readonly ILogger<DeleteMangaCommandHandler> _logger;

        public DeleteMangaCommandHandler(IUnitOfWork unitOfWork, IPhotoAccessor photoAccessor, ILogger<DeleteMangaCommandHandler> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _photoAccessor = photoAccessor ?? throw new ArgumentNullException(nameof(photoAccessor));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Unit> Handle(DeleteMangaCommand request, CancellationToken cancellationToken)
        {
            // Bước 1: Lấy thông tin chi tiết của manga (sử dụng instance không theo dõi cho việc đọc thông tin file)
            // GetMangaWithDetailsAsync sử dụng AsNoTracking(), phù hợp để lấy dữ liệu mà không làm thay đổi context.
            var mangaDetailsForFileSync = await _unitOfWork.MangaRepository.GetMangaWithDetailsAsync(request.MangaId);

            if (mangaDetailsForFileSync == null)
            {
                _logger.LogWarning("Manga with ID {MangaId} not found for deletion.", request.MangaId);
                throw new NotFoundException(nameof(Domain.Entities.Manga), request.MangaId);
            }

            // Bước 2: Xóa các file liên quan trên Cloudinary (dựa trên mangaDetailsForFileSync)
            // 2.1. Xóa ảnh bìa (CoverArts) khỏi Cloudinary
            if (mangaDetailsForFileSync.CoverArts != null && mangaDetailsForFileSync.CoverArts.Any())
            {
                foreach (var coverArt in mangaDetailsForFileSync.CoverArts.ToList())
                {
                    if (!string.IsNullOrEmpty(coverArt.PublicId))
                    {
                        var deletionResult = await _photoAccessor.DeletePhotoAsync(coverArt.PublicId);
                        if (deletionResult != "ok" && deletionResult != "not found")
                        {
                            _logger.LogWarning("Failed to delete cover art {PublicId} from Cloudinary for manga {MangaId}. Result: {DeletionResult}", coverArt.PublicId, request.MangaId, deletionResult);
                        }
                    }
                }
            }

            // 2.2. Xóa các trang của chapter (ChapterPages) khỏi Cloudinary
            if (mangaDetailsForFileSync.TranslatedMangas != null)
            {
                foreach (var translatedManga in mangaDetailsForFileSync.TranslatedMangas.ToList())
                {
                    // Cần lấy danh sách chapter thực tế từ DB, không phải từ mangaDetailsForFileSync.TranslatedMangas.Chapters
                    // vì mangaDetailsForFileSync không track các chapters này.
                    // Tuy nhiên, GetChaptersByTranslatedMangaAsync cũng có thể dùng AsNoTracking.
                    // Để xóa ảnh, chúng ta cần thông tin ChapterPage.PublicId.
                    // Cách đơn giản là lấy lại chapter với pages.
                    var chaptersInDb = await _unitOfWork.ChapterRepository.GetChaptersByTranslatedMangaAsync(translatedManga.TranslatedMangaId);
                    foreach (var chapterStub in chaptersInDb) // chapterStub từ GetChaptersByTranslatedMangaAsync có thể không có pages
                    {
                        // Lấy chapter với các trang của nó để xóa ảnh
                        var chapterWithPages = await _unitOfWork.ChapterRepository.GetChapterWithPagesAsync(chapterStub.ChapterId);
                        if (chapterWithPages?.ChapterPages != null && chapterWithPages.ChapterPages.Any())
                        {
                            foreach (var page in chapterWithPages.ChapterPages.ToList())
                            {
                                if (!string.IsNullOrEmpty(page.PublicId))
                                {
                                    var deletionResult = await _photoAccessor.DeletePhotoAsync(page.PublicId);
                                    if (deletionResult != "ok" && deletionResult != "not found")
                                    {
                                        _logger.LogWarning("Failed to delete chapter page {PublicId} from Cloudinary for chapter {ChapterId}. Result: {DeletionResult}", page.PublicId, chapterStub.ChapterId, deletionResult);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            
            // Bước 3: Xóa Manga khỏi DB bằng Id.
            // Phương thức DeleteAsync(Guid id) trong GenericRepository sẽ tìm entity bằng id (FindAsync sẽ track entity này)
            // sau đó gọi Remove() trên entity đã được track, tránh lỗi "already being tracked".
            // EF Core sẽ xử lý cascade delete cho các bảng liên kết như MangaTag, MangaAuthor, 
            // và các entity phụ thuộc như TranslatedManga, Chapter, CoverArt (nếu được cấu hình Cascade trong DbContext).
            await _unitOfWork.MangaRepository.DeleteAsync(request.MangaId);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Manga {MangaId} and its related data deleted successfully.", request.MangaId);
            return Unit.Value;
        }
    }
}
```

### Bước 2: Kiểm tra lại `GenericRepository.cs` (Không cần thay đổi)

Đảm bảo rằng phương thức `GetByIdAsync` (hoặc `FindAsync` được sử dụng bên trong nó) không sử dụng `AsNoTracking()` để khi `DeleteAsync(Guid id)` gọi nó, entity trả về sẽ được `DbContext` theo dõi.

**File (chỉ để tham khảo, không cần thay đổi):** `Persistence\Repositories\GenericRepository.cs`

```csharp
// Persistence\Repositories\GenericRepository.cs
// ... các using ...
namespace Persistence.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly ApplicationDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public GenericRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbSet = _context.Set<T>();
        }

        // GetByIdAsync sử dụng FindAsync, mà FindAsync luôn theo dõi entity nếu tìm thấy.
        public virtual async Task<T?> GetByIdAsync(Guid id)
        {
            return await _dbSet.FindAsync(id);
        }

        // ... các phương thức khác ...

        public virtual async Task DeleteAsync(Guid id)
        {
            var entity = await GetByIdAsync(id); // Entity này sẽ được theo dõi nếu tìm thấy
            if (entity != null)
            {
                await DeleteAsync(entity); // Gọi DeleteAsync(T entity) với entity đã được theo dõi
            }
        }
        
        public virtual Task DeleteAsync(T entity) // Phương thức này sẽ được gọi với entity đã được theo dõi
        {
            _dbSet.Remove(entity);
            return Task.CompletedTask;
        }
        // ... các phương thức khác ...
    }
}
```

### Bước 3: Kiểm tra cấu hình Cascade Delete trong `ApplicationDbContext.cs`

Đảm bảo rằng các mối quan hệ từ `Manga` đến các thực thể phụ thuộc (như `CoverArts`, `MangaAuthors`, `MangaTags`, `TranslatedMangas`) được cấu hình với `OnDelete(DeleteBehavior.Cascade)`. Điều này là cần thiết để khi `Manga` bị xóa, các dữ liệu liên quan này cũng tự động bị xóa.

**File (chỉ để tham khảo, giả định đã cấu hình đúng):** `Persistence\Data\ApplicationDbContext.cs`

```csharp
// Persistence\Data\ApplicationDbContext.cs
// ... các using ...
namespace Persistence.Data
{
    public class ApplicationDbContext : DbContext, IApplicationDbContext
    {
        // ... DbSet properties ...
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- Manga ---
            modelBuilder.Entity<Manga>(entity =>
            {
                // ... các cấu hình khác ...

                entity.HasMany(m => m.TranslatedMangas) // Quan hệ Manga -> TranslatedManga
                      .WithOne(tm => tm.Manga)
                      .HasForeignKey(tm => tm.MangaId)
                      .OnDelete(DeleteBehavior.Cascade); // Đảm bảo Cascade

                entity.HasMany(m => m.CoverArts)       // Quan hệ Manga -> CoverArt
                      .WithOne(ca => ca.Manga)
                      .HasForeignKey(ca => ca.MangaId)
                      .OnDelete(DeleteBehavior.Cascade); // Đảm bảo Cascade
                
                // MangaAuthors và MangaTags thường là many-to-many join tables,
                // EF Core sẽ tự động cấu hình cascade delete cho chúng khi Manga hoặc Author/Tag bị xóa.
                // Tuy nhiên, bạn có thể chỉ định rõ ràng nếu muốn.
            });
            
            // --- TranslatedManga ---
            modelBuilder.Entity<TranslatedManga>(entity =>
            {
                // ...
                entity.HasMany(tm => tm.Chapters)      // Quan hệ TranslatedManga -> Chapter
                      .WithOne(c => c.TranslatedManga)
                      .HasForeignKey(c => c.TranslatedMangaId)
                      .OnDelete(DeleteBehavior.Cascade); // Đảm bảo Cascade
            });

            // --- Chapter ---
            modelBuilder.Entity<Chapter>(entity =>
            {
                // ...
                entity.HasMany(c => c.ChapterPages)    // Quan hệ Chapter -> ChapterPage
                      .WithOne(cp => cp.Chapter)
                      .HasForeignKey(cp => cp.ChapterId)
                      .OnDelete(DeleteBehavior.Cascade); // Đảm bảo Cascade
            });
            
            // ... các cấu hình khác cho MangaAuthor, MangaTag nếu cần ...
            // Thông thường, cho bảng join many-to-many, khi một trong hai đầu bị xóa, bản ghi join sẽ bị xóa cascade.
            // Ví dụ cho MangaTag:
            modelBuilder.Entity<MangaTag>(entity =>
            {
                entity.HasKey(mt => new { mt.MangaId, mt.TagId });

                entity.HasOne(mt => mt.Manga)
                      .WithMany(m => m.MangaTags)
                      .HasForeignKey(mt => mt.MangaId)
                      .OnDelete(DeleteBehavior.Cascade); // Khi Manga xóa, MangaTag xóa

                entity.HasOne(mt => mt.Tag)
                      .WithMany(t => t.MangaTags)
                      .HasForeignKey(mt => mt.TagId)
                      .OnDelete(DeleteBehavior.Cascade); // Khi Tag xóa, MangaTag xóa
            });
            
             modelBuilder.Entity<MangaAuthor>(entity =>
            {
                entity.HasKey(ma => new { ma.MangaId, ma.AuthorId, ma.Role });

                entity.HasOne(ma => ma.Manga)
                      .WithMany(m => m.MangaAuthors)
                      .HasForeignKey(ma => ma.MangaId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ma => ma.Author)
                      .WithMany(a => a.MangaAuthors)
                      .HasForeignKey(ma => ma.AuthorId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
```