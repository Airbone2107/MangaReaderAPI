using Application.Common.DTOs.Authors;
using Application.Common.DTOs.CoverArts;
using Application.Common.DTOs.Mangas;
using Application.Common.DTOs.Tags;
using Application.Common.Models;
using Application.Contracts.Persistence;
using AutoMapper;
using Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

namespace Application.Features.Mangas.Queries.GetMangaById
{
    public class GetMangaByIdQueryHandler : IRequestHandler<GetMangaByIdQuery, ResourceObject<MangaAttributesDto>?>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetMangaByIdQueryHandler> _logger;

        public GetMangaByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<GetMangaByIdQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResourceObject<MangaAttributesDto>?> Handle(GetMangaByIdQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("GetMangaByIdQueryHandler.Handle - Lấy manga với ID: {MangaId}", request.MangaId);
            
            var manga = await _unitOfWork.MangaRepository.GetMangaWithDetailsAsync(request.MangaId);

            if (manga == null)
            {
                _logger.LogWarning("Không tìm thấy manga với ID: {MangaId}", request.MangaId);
                return null;
            }

            var mangaAttributes = _mapper.Map<MangaAttributesDto>(manga);
            
            mangaAttributes.Tags = manga.MangaTags
                .Select(mt => new ResourceObject<TagInMangaAttributesDto>
                {
                    Id = mt.Tag.TagId.ToString(),
                    Type = "tag",
                    Attributes = _mapper.Map<TagInMangaAttributesDto>(mt.Tag),
                    Relationships = null
                })
                .ToList();
            
            var relationships = new List<RelationshipObject>();

            bool includeAuthorFull = request.Includes?.Contains("author", StringComparer.OrdinalIgnoreCase) ?? false;
            bool includeCoverArt = request.Includes?.Contains("cover_art", StringComparer.OrdinalIgnoreCase) ?? false;

            if (manga.MangaAuthors != null)
            {
                foreach (var mangaAuthor in manga.MangaAuthors)
                {
                    if (mangaAuthor.Author != null)
                    {
                        var relationshipType = mangaAuthor.Role == MangaStaffRole.Author ? "author" : "artist";
                        
                        relationships.Add(new RelationshipObject
                        {
                            Id = mangaAuthor.Author.AuthorId.ToString(),
                            Type = relationshipType,
                            Attributes = includeAuthorFull 
                                ? _mapper.Map<AuthorAttributesDto>(mangaAuthor.Author)
                                : null
                        });
                    }
                }
            }
            
            if (manga.CoverArts != null && manga.CoverArts.Any())
            {
                foreach (var coverArt in manga.CoverArts)
                {
                    relationships.Add(new RelationshipObject
                    {
                        Id = coverArt.CoverId.ToString(),
                        Type = "cover_art",
                        Attributes = includeCoverArt
                            ? _mapper.Map<CoverArtAttributesDto>(coverArt)
                            : null
                    });
                }
            }
            
            var resourceObject = new ResourceObject<MangaAttributesDto>
            {
                Id = manga.MangaId.ToString(),
                Type = "manga",
                Attributes = mangaAttributes,
                Relationships = relationships.Any() ? relationships : null
            };
            
            return resourceObject;
        }
    }
} 