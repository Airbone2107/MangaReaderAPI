using Application.Common.DTOs;
using Application.Common.DTOs.Mangas;
using Application.Contracts.Persistence;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore; // Cần cho Include và ThenInclude
using Microsoft.Extensions.Logging;
using System.Linq.Expressions; // Cần cho Expression
using Application.Common.Extensions; // Thêm using này

namespace Application.Features.Mangas.Queries.GetMangas
{
    public class GetMangasQueryHandler : IRequestHandler<GetMangasQuery, PagedResult<MangaDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetMangasQueryHandler> _logger;

        public GetMangasQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<GetMangasQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<PagedResult<MangaDto>> Handle(GetMangasQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("GetMangasQueryHandler.Handle called with request: {@GetMangasQuery}", request);

            // Build filter predicate
            Expression<Func<Manga, bool>> predicate = m => true; // Start with a true condition

            if (!string.IsNullOrWhiteSpace(request.TitleFilter))
            {
                // TODO: [Improvement] Use EF.Functions.Like or full-text search for more efficient/flexible text searching
                predicate = predicate.And(m => m.Title.Contains(request.TitleFilter));
            }
            if (request.StatusFilter.HasValue)
            {
                predicate = predicate.And(m => m.Status == request.StatusFilter.Value);
            }
            if (request.ContentRatingFilter.HasValue)
            {
                predicate = predicate.And(m => m.ContentRating == request.ContentRatingFilter.Value);
            }
            if (request.DemographicFilter.HasValue)
            {
                predicate = predicate.And(m => m.PublicationDemographic == request.DemographicFilter.Value);
            }
            if (!string.IsNullOrWhiteSpace(request.OriginalLanguageFilter))
            {
                predicate = predicate.And(m => m.OriginalLanguage == request.OriginalLanguageFilter);
            }
            if (request.YearFilter.HasValue)
            {
                predicate = predicate.And(m => m.Year == request.YearFilter.Value);
            }
            if (request.TagIdsFilter != null && request.TagIdsFilter.Any())
            {
                predicate = predicate.And(m => m.MangaTags.Any(mt => request.TagIdsFilter.Contains(mt.TagId)));
            }
            if (request.AuthorIdsFilter != null && request.AuthorIdsFilter.Any())
            {
                 // TODO: [Improvement] Consider filtering by specific role (Author/Artist) if MangaAuthorInputDto had Role
                predicate = predicate.And(m => m.MangaAuthors.Any(ma => request.AuthorIdsFilter.Contains(ma.AuthorId)));
            }
            // TODO: [Improvement] Add filter for TranslatedManga.LanguageKey
            // if (!string.IsNullOrWhiteSpace(request.LanguageFilter))
            // {
            //     predicate = predicate.And(m => m.TranslatedMangas.Any(tm => tm.LanguageKey == request.LanguageFilter));
            // }


            // Build OrderBy function
            Func<IQueryable<Manga>, IOrderedQueryable<Manga>> orderBy;
            switch (request.OrderBy?.ToLowerInvariant())
            {
                case "title":
                    orderBy = q => request.Ascending ? q.OrderBy(m => m.Title) : q.OrderByDescending(m => m.Title);
                    break;
                case "year":
                    orderBy = q => request.Ascending ? q.OrderBy(m => m.Year) : q.OrderByDescending(m => m.Year);
                    break;
                case "createdat":
                    orderBy = q => request.Ascending ? q.OrderBy(m => m.CreatedAt) : q.OrderByDescending(m => m.CreatedAt);
                    break;
                case "updatedat":
                default:
                    orderBy = q => request.Ascending ? q.OrderBy(m => m.UpdatedAt) : q.OrderByDescending(m => m.UpdatedAt);
                    break;
            }

            // Use GetPagedAsync with the constructed filter and orderby, and includes
            var pagedMangas = await _unitOfWork.MangaRepository.GetPagedAsync(
                request.Offset,
                request.Limit,
                predicate,
                orderBy,
                // Include necessary navigations for mapping to MangaDto
                // Ensure these includes are configured in GenericRepository.GetPagedAsync
                includeProperties: "MangaTags.Tag.TagGroup,MangaAuthors.Author,CoverArts,TranslatedMangas"
            );

            // Map the paged results to DTOs
            var mangaDtos = _mapper.Map<List<MangaDto>>(pagedMangas.Items);

            // Return the paged result of DTOs
            return new PagedResult<MangaDto>(mangaDtos, pagedMangas.Total, request.Offset, request.Limit);
        }
    }
} 