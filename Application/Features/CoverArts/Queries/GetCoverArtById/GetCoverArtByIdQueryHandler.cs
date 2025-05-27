using Application.Common.DTOs.CoverArts;
using Application.Common.Models;
using Application.Contracts.Persistence;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Domain.Entities;

namespace Application.Features.CoverArts.Queries.GetCoverArtById
{
    public class GetCoverArtByIdQueryHandler : IRequestHandler<GetCoverArtByIdQuery, ResourceObject<CoverArtAttributesDto>?>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetCoverArtByIdQueryHandler> _logger;

        public GetCoverArtByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<GetCoverArtByIdQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResourceObject<CoverArtAttributesDto>?> Handle(GetCoverArtByIdQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("GetCoverArtByIdQueryHandler.Handle - Lấy cover art với ID: {CoverId}", request.CoverId);
            var coverArt = await _unitOfWork.CoverArtRepository.FindFirstOrDefaultAsync(
                ca => ca.CoverId == request.CoverId,
                includeProperties: "Manga" 
            );

            if (coverArt == null)
            {
                _logger.LogWarning("Không tìm thấy cover art với ID: {CoverId}", request.CoverId);
                return null;
            }
            var attributes = _mapper.Map<CoverArtAttributesDto>(coverArt);
            var relationships = new List<RelationshipObject>();
            if (coverArt.Manga != null)
            {
                relationships.Add(new RelationshipObject
                {
                    Id = coverArt.Manga.MangaId.ToString(),
                    Type = "manga"
                });
            }

            return new ResourceObject<CoverArtAttributesDto>
            {
                Id = coverArt.CoverId.ToString(),
                Type = "cover_art",
                Attributes = attributes,
                Relationships = relationships.Any() ? relationships : null
            };
        }
    }
} 