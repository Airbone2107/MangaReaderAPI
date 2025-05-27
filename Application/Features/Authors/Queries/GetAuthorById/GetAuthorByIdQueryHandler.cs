using Application.Common.DTOs.Authors;
using Application.Common.Models;
using Application.Contracts.Persistence;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Domain.Entities; // Cần cho Author

namespace Application.Features.Authors.Queries.GetAuthorById
{
    public class GetAuthorByIdQueryHandler : IRequestHandler<GetAuthorByIdQuery, ResourceObject<AuthorAttributesDto>?>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetAuthorByIdQueryHandler> _logger;

        public GetAuthorByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<GetAuthorByIdQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResourceObject<AuthorAttributesDto>?> Handle(GetAuthorByIdQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("GetAuthorByIdQueryHandler.Handle - Lấy tác giả với ID: {AuthorId}", request.AuthorId);
            
            var author = await _unitOfWork.AuthorRepository.FindFirstOrDefaultAsync(
                a => a.AuthorId == request.AuthorId,
                includeProperties: "MangaAuthors.Manga" 
            );

            if (author == null)
            {
                _logger.LogWarning("Không tìm thấy tác giả với ID: {AuthorId}", request.AuthorId);
                return null;
            }

            var authorAttributes = _mapper.Map<AuthorAttributesDto>(author);
            var relationships = new List<RelationshipObject>();

            if (author.MangaAuthors != null)
            {
                foreach (var mangaAuthor in author.MangaAuthors)
                {
                    if (mangaAuthor.Manga != null)
                    {
                         relationships.Add(new RelationshipObject
                        {
                            Id = mangaAuthor.Manga.MangaId.ToString(),
                            Type = "manga" 
                        });
                    }
                }
            }

            var resourceObject = new ResourceObject<AuthorAttributesDto>
            {
                Id = author.AuthorId.ToString(),
                Type = "author",
                Attributes = authorAttributes,
                Relationships = relationships.Any() ? relationships : null
            };
            
            return resourceObject;
        }
    }
} 