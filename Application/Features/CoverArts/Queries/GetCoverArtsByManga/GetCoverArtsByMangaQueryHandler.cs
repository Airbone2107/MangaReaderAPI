using Application.Common.DTOs;
using Application.Common.DTOs.CoverArts;
using Application.Contracts.Persistence;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace Application.Features.CoverArts.Queries.GetCoverArtsByManga
{
    public class GetCoverArtsByMangaQueryHandler : IRequestHandler<GetCoverArtsByMangaQuery, PagedResult<CoverArtDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetCoverArtsByMangaQueryHandler> _logger;

        public GetCoverArtsByMangaQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<GetCoverArtsByMangaQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<PagedResult<CoverArtDto>> Handle(GetCoverArtsByMangaQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("GetCoverArtsByMangaQueryHandler.Handle - Lấy cover arts cho MangaId: {MangaId}", request.MangaId);

             // Kiểm tra sự tồn tại của Manga
            var mangaExists = await _unitOfWork.MangaRepository.ExistsAsync(request.MangaId);
            if (!mangaExists)
            {
                _logger.LogWarning("Không tìm thấy Manga với ID: {MangaId} khi lấy cover arts.", request.MangaId);
                return new PagedResult<CoverArtDto>(new List<CoverArtDto>(), 0, request.PageNumber, request.PageSize);
            }

            // Build filter predicate
            Expression<Func<CoverArt, bool>> filter = ca => ca.MangaId == request.MangaId;
            // TODO: [Improvement] Thêm bộ lọc theo Volume

            // Build OrderBy function
            Func<IQueryable<CoverArt>, IOrderedQueryable<CoverArt>> orderBy = q => q.OrderByDescending(ca => ca.CreatedAt); // Mặc định mới nhất lên trước
            // TODO: [Improvement] Thêm sắp xếp theo Volume

            var pagedCoverArts = await _unitOfWork.CoverArtRepository.GetPagedAsync(
                request.PageNumber,
                request.PageSize,
                filter,
                orderBy
            );

            var coverArtDtos = _mapper.Map<List<CoverArtDto>>(pagedCoverArts.Items);
            return new PagedResult<CoverArtDto>(coverArtDtos, pagedCoverArts.TotalCount, request.PageNumber, request.PageSize);
        }
    }
} 