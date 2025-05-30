using Application.Common.DTOs.Chapters;
using Application.Common.Models;
using Application.Contracts.Persistence;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Chapters.Queries.GetChapterById
{
    public class GetChapterByIdQueryHandler : IRequestHandler<GetChapterByIdQuery, ResourceObject<ChapterAttributesDto>?>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetChapterByIdQueryHandler> _logger;

        public GetChapterByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<GetChapterByIdQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResourceObject<ChapterAttributesDto>?> Handle(GetChapterByIdQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("GetChapterByIdQueryHandler.Handle - Lấy chapter với ID: {ChapterId}", request.ChapterId);
            
            var chapter = await _unitOfWork.ChapterRepository.FindFirstOrDefaultAsync(
                predicate: c => c.ChapterId == request.ChapterId,
                includeProperties: "User,ChapterPages,TranslatedManga.Manga" 
            );

            if (chapter == null)
            {
                _logger.LogWarning("Không tìm thấy chapter với ID: {ChapterId}", request.ChapterId);
                return null;
            }
            
            var attributes = _mapper.Map<ChapterAttributesDto>(chapter);
            var relationships = new List<RelationshipObject>();

            if (chapter.User != null)
            {
                relationships.Add(new RelationshipObject
                {
                    Id = chapter.User.UserId.ToString(),
                    Type = "user" 
                });
            }
            
            if (chapter.TranslatedManga?.Manga != null) 
            {
                 relationships.Add(new RelationshipObject
                {
                    Id = chapter.TranslatedManga.Manga.MangaId.ToString(),
                    Type = "manga"
                });
            }
            
            return new ResourceObject<ChapterAttributesDto>
            {
                Id = chapter.ChapterId.ToString(),
                Type = "chapter",
                Attributes = attributes,
                Relationships = relationships.Any() ? relationships : null
            };
        }
    }
} 