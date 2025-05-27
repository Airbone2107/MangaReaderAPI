using Application.Common.DTOs.Mangas;
using Application.Common.Models;
using Application.Contracts.Persistence;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Domain.Entities;
using Domain.Enums;

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
            var relationships = new List<RelationshipObject>();

            if (manga.MangaAuthors != null)
            {
                foreach (var mangaAuthor in manga.MangaAuthors)
                {
                    if (mangaAuthor.Author != null)
                    {
                        relationships.Add(new RelationshipObject
                        {
                            Id = mangaAuthor.Author.AuthorId.ToString(),
                            Type = mangaAuthor.Role == MangaStaffRole.Author ? "author" : "artist"
                        });
                    }
                }
            }

            if (manga.MangaTags != null)
            {
                foreach (var mangaTag in manga.MangaTags)
                {
                    if (mangaTag.Tag != null)
                    {
                        relationships.Add(new RelationshipObject
                        {
                            Id = mangaTag.Tag.TagId.ToString(),
                            Type = "tag" 
                        });
                    }
                }
            }
            
            var primaryCover = manga.CoverArts?.FirstOrDefault(); 
            if (primaryCover != null)
            {
                relationships.Add(new RelationshipObject
                {
                    Id = primaryCover.CoverId.ToString(),
                    Type = "cover_art"
                });
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