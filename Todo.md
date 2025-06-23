Chào bạn, tôi đã xem xét yêu cầu của bạn về việc cập nhật hai endpoint danh sách manga và chi tiết manga. Để đáp ứng yêu cầu, chúng ta cần thay đổi logic xử lý tham số `includes` trong các handler tương ứng.

Dưới đây là mã nguồn đầy đủ cho các tệp đã được thay đổi.

### 1. Cập nhật Handler cho Endpoint Chi tiết Manga

File này được cập nhật để khi `includes[]=cover_art` được truyền vào, nó sẽ trả về đầy đủ `attributes` của các ảnh bìa trong `relationships`. Logic cho `includes[]=author` đã hoạt động đúng, nhưng tôi sẽ đảm bảo nó nhất quán.

<!-- file path="Application\Features\Mangas\Queries\GetMangaById\GetMangaByIdQueryHandler.cs" -->
```csharp
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
```

### 2. Cập nhật Handler cho Endpoint Danh sách Manga

File này được cập nhật để:
- Khi `includes[]=author`, trả về đầy đủ `attributes` của tác giả/họa sĩ.
- Khi `includes[]=cover_art`:
    - ID của relationship là `CoverId` (thay vì `PublicId` như trước).
    - Trả về đầy đủ `attributes` của ảnh bìa (chỉ lấy ảnh bìa chính).

<!-- file path="Application\Features\Mangas\Queries\GetMangas\GetMangasQueryHandler.cs" -->
```csharp
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
            
            // Cập nhật logic cho PublicationDemographicsFilter
            if (request.PublicationDemographicsFilter != null && request.PublicationDemographicsFilter.Any())
            {
                // Đảm bảo rằng PublicationDemographic của Manga không null trước khi kiểm tra Contains
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

            // --- Xử lý IncludedTags ---
            if (request.IncludedTags != null && request.IncludedTags.Any())
            {
                // Mặc định là "AND" nếu không cung cấp hoặc rỗng
                string includedMode = string.IsNullOrWhiteSpace(request.IncludedTagsMode) ? "AND" : request.IncludedTagsMode.ToUpper();

                if (includedMode == "OR")
                {
                    _logger.LogInformation("Applying IncludedTags with OR mode. Tags: {Tags}", string.Join(",", request.IncludedTags));
                    predicate = predicate.And(m => m.MangaTags.Any(mt => request.IncludedTags.Contains(mt.TagId)));
                }
                else // Mặc định là AND
                {
                    _logger.LogInformation("Applying IncludedTags with AND mode. Tags: {Tags}", string.Join(",", request.IncludedTags));
                    // Manga phải chứa TẤT CẢ các tag trong request.IncludedTags
                    // Tức là, với mỗi tagId trong request.IncludedTags, Manga phải có một MangaTag tương ứng.
                    foreach (var tagId in request.IncludedTags)
                    {
                        predicate = predicate.And(m => m.MangaTags.Any(mt => mt.TagId == tagId));
                    }
                    // Cách viết khác cho AND mode:
                    // predicate = predicate.And(m => request.IncludedTags.All(includedTagId => m.MangaTags.Any(mt => mt.TagId == includedTagId)));
                }
            }

            // --- Xử lý ExcludedTags ---
            if (request.ExcludedTags != null && request.ExcludedTags.Any())
            {
                // Mặc định là "OR" nếu không cung cấp hoặc rỗng
                string excludedMode = string.IsNullOrWhiteSpace(request.ExcludedTagsMode) ? "OR" : request.ExcludedTagsMode.ToUpper();

                if (excludedMode == "AND")
                {
                    _logger.LogInformation("Applying ExcludedTags with AND mode. Tags: {Tags}", string.Join(",", request.ExcludedTags));
                    // Manga KHÔNG được chứa TẤT CẢ các tag trong request.ExcludedTags
                    // Tức là, KHÔNG PHẢI (manga chứa TẤT CẢ các tag trong request.ExcludedTags)
                    predicate = predicate.And(m => !request.ExcludedTags.All(excludedTagId => m.MangaTags.Any(mt => mt.TagId == excludedTagId)));
                }
                else // Mặc định là OR
                {
                    _logger.LogInformation("Applying ExcludedTags with OR mode. Tags: {Tags}", string.Join(",", request.ExcludedTags));
                    // Manga KHÔNG được chứa BẤT KỲ tag nào trong request.ExcludedTags
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
            
            // Luôn include các navigation properties cần thiết cho tất cả các trường hợp (cover, author, tag)
            // CoverArts cũng cần cho Yêu cầu 1
            var pagedMangas = await _unitOfWork.MangaRepository.GetPagedAsync(
                request.Offset,
                request.Limit,
                predicate,
                orderBy,
                includeProperties: "MangaTags.Tag.TagGroup,MangaAuthors.Author,CoverArts"
            );

            var mangaResourceObjects = new List<ResourceObject<MangaAttributesDto>>();
            bool includeCoverArt = request.Includes?.Contains("cover_art", StringComparer.OrdinalIgnoreCase) ?? false;
            bool includeAuthorFull = request.Includes?.Contains("author", StringComparer.OrdinalIgnoreCase) ?? false;

            foreach (var manga in pagedMangas.Items)
            {
                var mangaAttributes = _mapper.Map<MangaAttributesDto>(manga);
                
                // Cập nhật cách populate Tags
                mangaAttributes.Tags = manga.MangaTags
                    .Select(mt => new ResourceObject<TagInMangaAttributesDto> // Sử dụng TagInMangaAttributesDto
                    {
                        Id = mt.Tag.TagId.ToString(),
                        Type = "tag",
                        Attributes = _mapper.Map<TagInMangaAttributesDto>(mt.Tag), // Map sang TagInMangaAttributesDto
                        Relationships = null // Không có relationships cho tag khi nhúng trong manga
                    })
                    .ToList();

                var relationships = new List<RelationshipObject>();

                // Xử lý Authors/Artists
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
                
                // Xử lý CoverArt (chỉ lấy cover chính)
                var primaryCover = manga.CoverArts?.OrderByDescending(ca => ca.CreatedAt).FirstOrDefault(); 
                if (primaryCover != null)
                {
                    relationships.Add(new RelationshipObject
                    {
                        Id = primaryCover.CoverId.ToString(), // ID là CoverId
                        Type = "cover_art",
                        Attributes = includeCoverArt
                            ? _mapper.Map<CoverArtAttributesDto>(primaryCover) // Full DTO
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
```

Với những thay đổi trên, cả hai endpoint giờ đây sẽ hoạt động nhất quán với nhau khi xử lý `includes` cho `author` và `cover_art` theo đúng yêu cầu của bạn. Nếu có bất kỳ câu hỏi nào khác, đừng ngần ngại hỏi nhé