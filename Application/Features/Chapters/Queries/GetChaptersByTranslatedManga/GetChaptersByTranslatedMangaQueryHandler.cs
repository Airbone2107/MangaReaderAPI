using Application.Common.DTOs;
using Application.Common.DTOs.Chapters;
using Application.Common.Models;
using Application.Contracts.Persistence;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace Application.Features.Chapters.Queries.GetChaptersByTranslatedManga
{
    public class GetChaptersByTranslatedMangaQueryHandler : IRequestHandler<GetChaptersByTranslatedMangaQuery, PagedResult<ResourceObject<ChapterAttributesDto>>>
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

        public async Task<PagedResult<ResourceObject<ChapterAttributesDto>>> Handle(GetChaptersByTranslatedMangaQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("GetChaptersByTranslatedMangaQueryHandler.Handle - Lấy chapters cho TranslatedMangaId: {TranslatedMangaId}", request.TranslatedMangaId);

            // Kiểm tra sự tồn tại của TranslatedManga (tùy chọn, có thể bỏ qua nếu logic cho phép trả về rỗng cho Manga không tồn tại)
            var translatedMangaExists = await _unitOfWork.TranslatedMangaRepository.ExistsAsync(request.TranslatedMangaId);
            if (!translatedMangaExists)
            {
                _logger.LogWarning("Không tìm thấy TranslatedManga với ID: {TranslatedMangaId} khi lấy danh sách chapter.", request.TranslatedMangaId);
                // Trả về kết quả rỗng nếu TranslatedManga không tồn tại.
                return new PagedResult<ResourceObject<ChapterAttributesDto>>(new List<ResourceObject<ChapterAttributesDto>>(), 0, request.Offset, request.Limit);
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
                request.Offset,
                request.Limit,
                filter,
                orderBy,
                includeProperties: "User,ChapterPages,TranslatedManga.Manga" // Bao gồm User, ChapterPages và TranslatedManga.Manga
            );
            
            // AutoMapper sẽ tự động map và sắp xếp ChapterPages bên trong mỗi ChapterDto

            var resourceObjects = new List<ResourceObject<ChapterAttributesDto>>();
            foreach(var chapter in pagedChapters.Items)
            {
                var attributes = _mapper.Map<ChapterAttributesDto>(chapter);
                var relationships = new List<RelationshipObject>();
                if (chapter.User != null)
                {
                    relationships.Add(new RelationshipObject { Id = chapter.User.UserId.ToString(), Type = "user" });
                }
                if (chapter.TranslatedManga?.Manga != null)
                {
                    relationships.Add(new RelationshipObject { Id = chapter.TranslatedManga.Manga.MangaId.ToString(), Type = "manga" });
                }
                resourceObjects.Add(new ResourceObject<ChapterAttributesDto>
                {
                    Id = chapter.ChapterId.ToString(),
                    Type = "chapter",
                    Attributes = attributes,
                    Relationships = relationships.Any() ? relationships : null
                });
            }
            return new PagedResult<ResourceObject<ChapterAttributesDto>>(resourceObjects, pagedChapters.Total, request.Offset, request.Limit);
        }
    }
} 