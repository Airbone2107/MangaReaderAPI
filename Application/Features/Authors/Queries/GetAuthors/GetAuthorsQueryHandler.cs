using Application.Common.DTOs;
using Application.Common.DTOs.Authors;
using Application.Common.Models;
using Application.Contracts.Persistence;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace Application.Features.Authors.Queries.GetAuthors
{
    public class GetAuthorsQueryHandler : IRequestHandler<GetAuthorsQuery, PagedResult<ResourceObject<AuthorAttributesDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetAuthorsQueryHandler> _logger;

        public GetAuthorsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<GetAuthorsQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<PagedResult<ResourceObject<AuthorAttributesDto>>> Handle(GetAuthorsQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("GetAuthorsQueryHandler.Handle - Lấy danh sách tác giả với Offset: {Offset}, Limit: {Limit}, NameFilter: {NameFilter}",
                request.Offset, request.Limit, request.NameFilter);

            Expression<Func<Author, bool>>? filter = null;
            if (!string.IsNullOrWhiteSpace(request.NameFilter))
            {
                filter = author => author.Name.Contains(request.NameFilter);
            }

            Func<IQueryable<Author>, IOrderedQueryable<Author>> orderBy = q => 
                request.OrderBy?.ToLowerInvariant() == "name" && request.Ascending ? q.OrderBy(a => a.Name) :
                request.OrderBy?.ToLowerInvariant() == "name" && !request.Ascending ? q.OrderByDescending(a => a.Name) :
                q.OrderBy(a => a.Name); 

            var pagedAuthors = await _unitOfWork.AuthorRepository.GetPagedAsync(
                request.Offset,
                request.Limit,
                filter,
                orderBy,
                includeProperties: "MangaAuthors.Manga"
            );

            var authorResourceObjects = new List<ResourceObject<AuthorAttributesDto>>();
            foreach(var author in pagedAuthors.Items)
            {
                var attributes = _mapper.Map<AuthorAttributesDto>(author);
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
                authorResourceObjects.Add(new ResourceObject<AuthorAttributesDto>
                {
                    Id = author.AuthorId.ToString(),
                    Type = "author",
                    Attributes = attributes,
                    Relationships = relationships.Any() ? relationships : null
                });
            }
            
            return new PagedResult<ResourceObject<AuthorAttributesDto>>(authorResourceObjects, pagedAuthors.Total, request.Offset, request.Limit);
        }
    }
} 