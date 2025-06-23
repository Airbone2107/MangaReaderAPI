using Application.Common.DTOs;
using Application.Common.DTOs.Authors;
using Application.Common.DTOs.CoverArts;
using Application.Common.DTOs.Mangas;
using Application.Common.DTOs.Tags;
using Application.Common.Extensions;
using Application.Common.Models;
using Application.Contracts.Persistence;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Linq.Expressions;

namespace Application.Features.Mangas.Queries.GetMangas
{
    public class GetMangasQueryHandler : IRequestHandler<GetMangasQuery, PagedResult<ResourceObject<MangaAttributesDto>>>
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

        public async Task<PagedResult<ResourceObject<MangaAttributesDto>>> Handle(GetMangasQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("GetMangasQueryHandler.Handle called with request: {@GetMangasQuery}", request);

            // Build filter predicate
            Expression<Func<Manga, bool>> predicate = m => true; // Start with a true condition

            if (!string.IsNullOrWhiteSpace(request.TitleFilter))
            {
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
            
            if (request.PublicationDemographicsFilter != null && request.PublicationDemographicsFilter.Any())
            {
                predicate = predicate.And(m => m.PublicationDemographic.HasValue && request.PublicationDemographicsFilter.Contains(m.PublicationDemographic.Value));
            }

            if (!string.IsNullOrWhiteSpace(request.OriginalLanguageFilter))
            {
                predicate = predicate.And(m => m.OriginalLanguage == request.OriginalLanguageFilter);
            }
            if (request.YearFilter.HasValue)
            {
                predicate = predicate.And(m => m.Year == request.YearFilter.Value);
            }

            // *** BẮT ĐẦU LOGIC LỌC MỚI ***
            if (request.Authors != null && request.Authors.Any())
            {
                predicate = predicate.And(m => m.MangaAuthors.Any(ma => ma.Role == MangaStaffRole.Author && request.Authors.Contains(ma.AuthorId)));
            }
            
            if (request.Artists != null && request.Artists.Any())
            {
                predicate = predicate.And(m => m.MangaAuthors.Any(ma => ma.Role == MangaStaffRole.Artist && request.Artists.Contains(ma.AuthorId)));
            }
            
            if (request.AvailableTranslatedLanguage != null && request.AvailableTranslatedLanguage.Any())
            {
                var languages = request.AvailableTranslatedLanguage.Select(l => l.ToLower()).ToList();
                predicate = predicate.And(m => m.TranslatedMangas.Any(tm => 
                    languages.Contains(tm.LanguageKey.ToLower()) && tm.Chapters.Any()
                ));
            }
            // *** KẾT THÚC LOGIC LỌC MỚI ***

            if (request.IncludedTags != null && request.IncludedTags.Any())
            {
                string includedMode = string.IsNullOrWhiteSpace(request.IncludedTagsMode) ? "AND" : request.IncludedTagsMode.ToUpper();
                if (includedMode == "OR")
                {
                    predicate = predicate.And(m => m.MangaTags.Any(mt => request.IncludedTags.Contains(mt.TagId)));
                }
                else
                {
                    foreach (var tagId in request.IncludedTags)
                    {
                        predicate = predicate.And(m => m.MangaTags.Any(mt => mt.TagId == tagId));
                    }
                }
            }
            
            if (request.ExcludedTags != null && request.ExcludedTags.Any())
            {
                string excludedMode = string.IsNullOrWhiteSpace(request.ExcludedTagsMode) ? "OR" : request.ExcludedTagsMode.ToUpper();
                if (excludedMode == "AND")
                {
                    predicate = predicate.And(m => !request.ExcludedTags.All(excludedTagId => m.MangaTags.Any(mt => mt.TagId == excludedTagId)));
                }
                else
                {
                    predicate = predicate.And(m => !m.MangaTags.Any(mt => request.ExcludedTags.Contains(mt.TagId)));
                }
            }


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
            
            // Cập nhật includeProperties để chứa dữ liệu cần thiết cho việc lọc và hiển thị
            var includeProperties = "MangaTags.Tag.TagGroup,MangaAuthors.Author,CoverArts,TranslatedMangas.Chapters";
            
            var pagedMangas = await _unitOfWork.MangaRepository.GetPagedAsync(
                request.Offset,
                request.Limit,
                predicate,
                orderBy,
                includeProperties: includeProperties
            );

            var mangaResourceObjects = new List<ResourceObject<MangaAttributesDto>>();
            bool includeCoverArt = request.Includes?.Contains("cover_art", StringComparer.OrdinalIgnoreCase) ?? false;
            bool includeAuthorFull = request.Includes?.Contains("author", StringComparer.OrdinalIgnoreCase) ?? false;
            bool includeArtistFull = request.Includes?.Contains("artist", StringComparer.OrdinalIgnoreCase) ?? false;

            foreach (var manga in pagedMangas.Items)
            {
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

                // *** TÍNH TOÁN VÀ GÁN NGÔN NGỮ KHẢ DỤNG ***
                mangaAttributes.AvailableTranslatedLanguages = manga.TranslatedMangas
                    .Where(tm => tm.Chapters.Any())
                    .Select(tm => tm.LanguageKey.ToUpper())
                    .Distinct()
                    .OrderBy(lang => lang)
                    .ToList();

                var relationships = new List<RelationshipObject>();

                if (manga.MangaAuthors != null)
                {
                    foreach (var mangaAuthor in manga.MangaAuthors)
                    {
                        if (mangaAuthor.Author != null) 
                        {
                            var isAuthorRole = mangaAuthor.Role == MangaStaffRole.Author;
                            var relationshipType = isAuthorRole ? "author" : "artist";
                            var shouldIncludeFull = (isAuthorRole && includeAuthorFull) || (!isAuthorRole && includeArtistFull);

                            relationships.Add(new RelationshipObject
                            {
                                Id = mangaAuthor.Author.AuthorId.ToString(),
                                Type = relationshipType,
                                Attributes = shouldIncludeFull 
                                    ? _mapper.Map<AuthorAttributesDto>(mangaAuthor.Author)
                                    : null
                            });
                        }
                    }
                }
                
                var primaryCover = manga.CoverArts?.OrderByDescending(ca => ca.CreatedAt).FirstOrDefault(); 
                if (primaryCover != null)
                {
                    relationships.Add(new RelationshipObject
                    {
                        Id = primaryCover.CoverId.ToString(),
                        Type = "cover_art",
                        Attributes = includeCoverArt
                            ? _mapper.Map<CoverArtAttributesDto>(primaryCover)
                            : null
                    });
                }

                mangaResourceObjects.Add(new ResourceObject<MangaAttributesDto>
                {
                    Id = manga.MangaId.ToString(),
                    Type = "manga",
                    Attributes = mangaAttributes,
                    Relationships = relationships.Any() ? relationships : null
                });
            }
            
            return new PagedResult<ResourceObject<MangaAttributesDto>>(mangaResourceObjects, pagedMangas.Total, request.Offset, request.Limit);
        }
    }
} 