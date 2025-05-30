using Application.Common.DTOs;
using Application.Common.DTOs.CoverArts;
using Application.Common.Models;
using Application.Contracts.Persistence;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace Application.Features.CoverArts.Queries.GetCoverArtsByManga
{
    public class GetCoverArtsByMangaQueryHandler : IRequestHandler<GetCoverArtsByMangaQuery, PagedResult<ResourceObject<CoverArtAttributesDto>>>
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

        public async Task<PagedResult<ResourceObject<CoverArtAttributesDto>>> Handle(GetCoverArtsByMangaQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("GetCoverArtsByMangaQueryHandler.Handle - Lấy cover arts cho MangaId: {MangaId}", request.MangaId);

            var mangaExists = await _unitOfWork.MangaRepository.ExistsAsync(request.MangaId);
            if (!mangaExists)
            {
                _logger.LogWarning("Không tìm thấy Manga với ID: {MangaId} khi lấy cover arts.", request.MangaId);
                return new PagedResult<ResourceObject<CoverArtAttributesDto>>(new List<ResourceObject<CoverArtAttributesDto>>(), 0, request.Offset, request.Limit);
            }

            Expression<Func<CoverArt, bool>> filter = ca => ca.MangaId == request.MangaId;
            Func<IQueryable<CoverArt>, IOrderedQueryable<CoverArt>> orderBy = q => q.OrderByDescending(ca => ca.CreatedAt);

            var pagedCoverArts = await _unitOfWork.CoverArtRepository.GetPagedAsync(
                request.Offset,
                request.Limit,
                filter,
                orderBy,
                includeProperties: "Manga" 
            );

            var resourceObjects = new List<ResourceObject<CoverArtAttributesDto>>();
            foreach(var coverArt in pagedCoverArts.Items)
            {
                var attributes = _mapper.Map<CoverArtAttributesDto>(coverArt);
                var relationships = new List<RelationshipObject>();
                if (coverArt.Manga != null) 
                {
                    relationships.Add(new RelationshipObject { Id = coverArt.Manga.MangaId.ToString(), Type = "manga" });
                }
                resourceObjects.Add(new ResourceObject<CoverArtAttributesDto>
                {
                    Id = coverArt.CoverId.ToString(),
                    Type = "cover_art",
                    Attributes = attributes,
                    Relationships = relationships.Any() ? relationships : null
                });
            }
            return new PagedResult<ResourceObject<CoverArtAttributesDto>>(resourceObjects, pagedCoverArts.Total, request.Offset, request.Limit);
        }
    }
} 