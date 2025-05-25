using Application.Common.DTOs.TranslatedMangas;
using Application.Common.DTOs.Chapters; // Cần nếu IncludeChapters=true
using Application.Contracts.Persistence;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Domain.Entities; // Cần cho TranslatedManga

namespace Application.Features.TranslatedMangas.Queries.GetTranslatedMangaById
{
    public class GetTranslatedMangaByIdQueryHandler : IRequestHandler<GetTranslatedMangaByIdQuery, TranslatedMangaDto?>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetTranslatedMangaByIdQueryHandler> _logger;

        public GetTranslatedMangaByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<GetTranslatedMangaByIdQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<TranslatedMangaDto?> Handle(GetTranslatedMangaByIdQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("GetTranslatedMangaByIdQueryHandler.Handle - Lấy translated manga với ID: {TranslatedMangaId}", request.TranslatedMangaId);
            
            // TODO: [Improvement] Nếu Query có tùy chọn IncludeChapters, sử dụng FindFirstOrDefaultAsync với includeProperties: "Chapters"
            // ITagGroupRepository.GetTagGroupWithTagsAsync là một ví dụ về cách tạo phương thức repo để include.
            // Nên tạo một phương thức tương tự trong ITranslatedMangaRepository và TranslatedMangaRepository
            // Task<TranslatedManga?> GetByIdWithChaptersAsync(Guid translatedMangaId);
            // Sau đó gọi ở đây nếu IncludeChapters = true.
            
            var translatedManga = await _unitOfWork.TranslatedMangaRepository.GetByIdAsync(request.TranslatedMangaId);

            if (translatedManga == null)
            {
                _logger.LogWarning("Không tìm thấy translated manga với ID: {TranslatedMangaId}", request.TranslatedMangaId);
                return null;
            }
            
            // TODO: [Improvement] Nếu Query có tùy chọn IncludeChapters VÀ TranslatedMangaDto được cập nhật để chứa List<ChapterDto>,
            // cần đảm bảo mapping profile xử lý việc map TranslatedManga.Chapters sang TranslatedMangaDto.Chapters.
            // MappingProfile hiện tại chỉ map TranslatedManga -> TranslatedMangaDto mà không có property Chapters.
            // Nếu cập nhật TranslatedMangaDto và MappingProfile, phần này sẽ tự động hoạt động khi TranslatedManga entity đã load Chapters.

            return _mapper.Map<TranslatedMangaDto>(translatedManga);
        }
    }
} 