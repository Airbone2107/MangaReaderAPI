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