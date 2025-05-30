using Application.Common.DTOs.TranslatedMangas;
using Application.Common.Models;
using Application.Contracts.Persistence;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.TranslatedMangas.Queries.GetTranslatedMangaById
{
    public class GetTranslatedMangaByIdQueryHandler : IRequestHandler<GetTranslatedMangaByIdQuery, ResourceObject<TranslatedMangaAttributesDto>?>
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

        public async Task<ResourceObject<TranslatedMangaAttributesDto>?> Handle(GetTranslatedMangaByIdQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("GetTranslatedMangaByIdQueryHandler.Handle - Lấy translated manga với ID: {TranslatedMangaId}", request.TranslatedMangaId);
            
            string includeProps = "Manga"; 

            var translatedManga = await _unitOfWork.TranslatedMangaRepository.FindFirstOrDefaultAsync(
                tm => tm.TranslatedMangaId == request.TranslatedMangaId,
                includeProperties: includeProps
            );

            if (translatedManga == null)
            {
                _logger.LogWarning("Không tìm thấy translated manga với ID: {TranslatedMangaId}", request.TranslatedMangaId);
                return null;
            }
            
            var attributes = _mapper.Map<TranslatedMangaAttributesDto>(translatedManga);
            var relationships = new List<RelationshipObject>();

            if (translatedManga.Manga != null)
            {
                relationships.Add(new RelationshipObject 
                { 
                    Id = translatedManga.Manga.MangaId.ToString(), 
                    Type = "manga" 
                });
            }

            return new ResourceObject<TranslatedMangaAttributesDto>
            {
                Id = translatedManga.TranslatedMangaId.ToString(),
                Type = "translated_manga", 
                Attributes = attributes,
                Relationships = relationships.Any() ? relationships : null
            };
        }
    }
} 