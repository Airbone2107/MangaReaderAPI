using Application.Common.DTOs;
using Application.Common.DTOs.TranslatedMangas;
using Application.Contracts.Persistence;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using Application.Common.Extensions;

namespace Application.Features.TranslatedMangas.Queries.GetTranslatedMangasByManga
{
    public class GetTranslatedMangasByMangaQueryHandler : IRequestHandler<GetTranslatedMangasByMangaQuery, PagedResult<TranslatedMangaDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetTranslatedMangasByMangaQueryHandler> _logger;

        public GetTranslatedMangasByMangaQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<GetTranslatedMangasByMangaQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<PagedResult<TranslatedMangaDto>> Handle(GetTranslatedMangasByMangaQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("GetTranslatedMangasByMangaQueryHandler.Handle - Lấy translated mangas cho MangaId: {MangaId}", request.MangaId);

            // Kiểm tra sự tồn tại của Manga
            var mangaExists = await _unitOfWork.MangaRepository.ExistsAsync(request.MangaId);
            if (!mangaExists)
            {
                _logger.LogWarning("Không tìm thấy Manga với ID: {MangaId} khi lấy translated mangas.", request.MangaId);
                return new PagedResult<TranslatedMangaDto>(new List<TranslatedMangaDto>(), 0, request.Offset, request.Limit);
            }

            // Build filter predicate
            Expression<Func<TranslatedManga, bool>> filter = tm => tm.MangaId == request.MangaId;
            // TODO: [Improvement] Thêm bộ lọc theo LanguageKey

            // Build OrderBy function
            Func<IQueryable<TranslatedManga>, IOrderedQueryable<TranslatedManga>> orderBy;
            switch (request.OrderBy?.ToLowerInvariant())
            {
                case "title":
                     orderBy = q => request.Ascending ? q.OrderBy(tm => tm.Title) : q.OrderByDescending(tm => tm.Title);
                    break;
                case "languagekey":
                default: // Mặc định sắp xếp theo LanguageKey
                    orderBy = q => request.Ascending ? q.OrderBy(tm => tm.LanguageKey) : q.OrderByDescending(tm => tm.LanguageKey);
                    break;
            }

            // Use GetPagedAsync with filter and orderby
            var pagedTranslatedMangas = await _unitOfWork.TranslatedMangaRepository.GetPagedAsync(
                request.Offset,
                request.Limit,
                filter,
                orderBy
                // Không cần include gì đặc biệt cho TranslatedMangaDto hiện tại
            );

            var translatedMangaDtos = _mapper.Map<List<TranslatedMangaDto>>(pagedTranslatedMangas.Items);
            return new PagedResult<TranslatedMangaDto>(translatedMangaDtos, pagedTranslatedMangas.Total, request.Offset, request.Limit);
        }
    }
} 