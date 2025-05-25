using Application.Common.DTOs;
using Application.Common.DTOs.Chapters;
using Application.Contracts.Persistence;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore; // Cần cho Include
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace Application.Features.Chapters.Queries.GetChaptersByTranslatedManga
{
    public class GetChaptersByTranslatedMangaQueryHandler : IRequestHandler<GetChaptersByTranslatedMangaQuery, PagedResult<ChapterDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetChaptersByTranslatedMangaQueryHandler> _logger;

        public GetChaptersByTranslatedMangaQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<GetChaptersByTranslatedMangaQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<PagedResult<ChapterDto>> Handle(GetChaptersByTranslatedMangaQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("GetChaptersByTranslatedMangaQueryHandler.Handle - Lấy chapters cho TranslatedMangaId: {TranslatedMangaId}", request.TranslatedMangaId);

            // Kiểm tra sự tồn tại của TranslatedManga (tùy chọn, có thể bỏ qua nếu logic cho phép trả về rỗng cho Manga không tồn tại)
            var translatedMangaExists = await _unitOfWork.TranslatedMangaRepository.ExistsAsync(request.TranslatedMangaId);
            if (!translatedMangaExists)
            {
                _logger.LogWarning("Không tìm thấy TranslatedManga với ID: {TranslatedMangaId} khi lấy danh sách chapter.", request.TranslatedMangaId);
                // Trả về kết quả rỗng nếu TranslatedManga không tồn tại.
                return new PagedResult<ChapterDto>(new List<ChapterDto>(), 0, request.PageNumber, request.PageSize);
            }

            // Build filter predicate
            Expression<Func<Chapter, bool>> filter = c => c.TranslatedMangaId == request.TranslatedMangaId;
            // TODO: [Improvement] Thêm bộ lọc theo Volume, ChapterNumber nếu cần

            // Build OrderBy function
            Func<IQueryable<Chapter>, IOrderedQueryable<Chapter>> orderBy;
            switch (request.OrderBy?.ToLowerInvariant())
            {
                case "volume":
                    orderBy = q => request.Ascending ? 
                                   q.OrderBy(c => c.Volume).ThenBy(c => c.ChapterNumber) : 
                                   q.OrderByDescending(c => c.Volume).ThenByDescending(c => c.ChapterNumber);
                    break;
                case "publishat":
                    orderBy = q => request.Ascending ? q.OrderBy(c => c.PublishAt) : q.OrderByDescending(c => c.PublishAt);
                    break;
                case "chapternumber":
                default: // Mặc định sắp xếp theo ChapterNumber, sau đó Volume (cần cẩn thận với kiểu string, có thể cần custom logic sort cho số chapter)
                         // TODO: [Improvement] Implement custom sorting for ChapterNumber/Volume if they contain non-numeric parts (e.g., 1.5, 2a)
                    orderBy = q => request.Ascending ? 
                                   q.OrderBy(c => c.ChapterNumber).ThenBy(c => c.Volume) : 
                                   q.OrderByDescending(c => c.ChapterNumber).ThenByDescending(c => c.Volume);
                    break;
            }

            // Use GetPagedAsync with filter, orderby, and includes
            // Cần include User và ChapterPages cho mapping ChapterDto
            // Ensure includes are configured in GenericRepository.GetPagedAsync
             var pagedChapters = await _unitOfWork.ChapterRepository.GetPagedAsync(
                request.PageNumber,
                request.PageSize,
                filter,
                orderBy,
                includeProperties: "User,ChapterPages" // Bao gồm User và ChapterPages
            );
            
            // AutoMapper sẽ tự động map và sắp xếp ChapterPages bên trong mỗi ChapterDto

            var chapterDtos = _mapper.Map<List<ChapterDto>>(pagedChapters.Items);
            return new PagedResult<ChapterDto>(chapterDtos, pagedChapters.TotalCount, request.PageNumber, request.PageSize);
        }
    }
} 