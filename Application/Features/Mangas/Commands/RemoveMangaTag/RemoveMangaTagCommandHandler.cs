using Application.Contracts.Persistence;
using Application.Exceptions;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore; // For SingleOrDefaultAsync

namespace Application.Features.Mangas.Commands.RemoveMangaTag
{
    public class RemoveMangaTagCommandHandler : IRequestHandler<RemoveMangaTagCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<RemoveMangaTagCommandHandler> _logger;

        public RemoveMangaTagCommandHandler(IUnitOfWork unitOfWork, ILogger<RemoveMangaTagCommandHandler> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Unit> Handle(RemoveMangaTagCommand request, CancellationToken cancellationToken)
        {
            var manga = await _unitOfWork.MangaRepository.GetByIdAsync(request.MangaId);
            if (manga == null)
            {
                throw new NotFoundException(nameof(Manga), request.MangaId);
            }

            var tag = await _unitOfWork.TagRepository.GetByIdAsync(request.TagId);
            if (tag == null)
            {
                throw new NotFoundException(nameof(Tag), request.TagId);
            }

            // Tìm MangaTag entity để xóa. Cần truy cập DbContext hoặc có Repo cho MangaTag.
            // Cách 1: Load collection MangaTags của Manga
            var mangaEntity = await _unitOfWork.MangaRepository.FindFirstOrDefaultAsync(
                m => m.MangaId == request.MangaId,
                includeProperties: "MangaTags" // Rất quan trọng
            );

            if (mangaEntity == null) // Should not happen
            {
                throw new NotFoundException(nameof(Manga), request.MangaId);
            }

            var mangaTagToRemove = mangaEntity.MangaTags.SingleOrDefault(mt => mt.TagId == request.TagId);

            if (mangaTagToRemove != null)
            {
                mangaEntity.MangaTags.Remove(mangaTagToRemove); 
                // Không cần gọi UpdateAsync trên mangaEntity vì EF Core theo dõi thay đổi collection.
                // _context.Set<MangaTag>().Remove(mangaTagToRemove); // Nếu có repo cho MangaTag thì dùng DeleteAsync của nó
            }
            else
            {
                _logger.LogWarning("Tag {TagId} not found on Manga {MangaId} for removal.", request.TagId, request.MangaId);
                // Có thể throw NotFoundException hoặc trả về thành công nếu "không tìm thấy" cũng là một trạng thái chấp nhận được.
                // Trong trường hợp này, ta coi như không có gì để xóa.
                return Unit.Value;
            }
            
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Tag {TagId} removed from Manga {MangaId} successfully.", request.TagId, request.MangaId);
            return Unit.Value;
        }
    }
} 