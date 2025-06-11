# TODO: Cập nhật API Endpoints cho Manga, Tag, và Author

Hướng dẫn này mô tả các bước cần thiết để cập nhật các API endpoint liên quan đến Manga, Tag, và Author theo các yêu cầu mới.

## Mục lục

1.  [**Bước 1:** Tạo DTO mới cho Tag khi được nhúng vào Manga](#buoc-1-tao-dto-moi-cho-tag-khi-duoc-nhung-vao-manga)
    1.1. [Tạo `Application/Common/DTOs/Tags/TagInMangaAttributesDto.cs`](#buoc-11-tao-taginmangaattributesdtocs)
2.  [**Bước 2:** Cập nhật các DTO hiện có](#buoc-2-cap-nhat-cac-dto-hien-co)
    2.1. [Cập nhật `Application/Common/DTOs/Tags/TagAttributesDto.cs`](#buoc-21-cap-nhat-tagattributesdtocs)
    2.2. [Cập nhật `Application/Common/DTOs/Mangas/MangaAttributesDto.cs`](#buoc-22-cap-nhat-mangaattributesdtocs)
3.  [**Bước 3:** Cập nhật Mapping Profiles](#buoc-3-cap-nhat-mapping-profiles)
    3.1. [Cập nhật `Application/Common/Mappings/MappingProfile.cs`](#buoc-31-cap-nhat-mappingprofilecs)
4.  [**Bước 4:** Cập nhật Queries và Query Handlers](#buoc-4-cap-nhat-queries-va-query-handlers)
    4.1. [Cập nhật `Application/Features/Mangas/Queries/GetMangas/GetMangasQuery.cs`](#buoc-41-cap-nhat-getmangasquerycs)
    4.2. [Cập nhật `Application/Features/Mangas/Queries/GetMangas/GetMangasQueryHandler.cs`](#buoc-42-cap-nhat-getmangasqueryhandlercs)
    4.3. [Cập nhật `Application/Features/Mangas/Queries/GetMangaById/GetMangaByIdQueryHandler.cs`](#buoc-43-cap-nhat-getmangabyidqueryhandlercs)
    4.4. [Cập nhật `Application/Features/Tags/Queries/GetTags/GetTagsQueryHandler.cs`](#buoc-44-cap-nhat-gettagsqueryhandlercs)
5.  [**Bước 5:** Cập nhật Tài liệu API](#buoc-5-cap-nhat-tai-lieu-api)
    5.1. [Cập nhật `MangaReaderApi.yaml`](#buoc-51-cap-nhat-mangareaderapiyaml)
    5.2. [Cập nhật `docs/api_conventions.md`](#buoc-52-cap-nhat-docsapiconventionsmd)
    5.3. [Cập nhật `MangaReaderAPI.md` (External API Guide)](#buoc-53-cap-nhat-mangareaderapimd)
6.  [**Bước 6:** Kiểm thử](#buoc-6-kiem-thu)

---

## Bước 1: Tạo DTO mới cho Tag khi được nhúng vào Manga

### Bước 1.1: Tạo `Application/Common/DTOs/Tags/TagInMangaAttributesDto.cs`

DTO này sẽ được sử dụng khi thông tin Tag được nhúng vào trong `MangaAttributesDto`. Nó chỉ chứa `Name` và `TagGroupName`.

```csharp
// Application/Common/DTOs/Tags/TagInMangaAttributesDto.cs
namespace Application.Common.DTOs.Tags
{
    public class TagInMangaAttributesDto
    {
        public string Name { get; set; } = string.Empty;
        public string TagGroupName { get; set; } = string.Empty;
    }
}
```

---

## Bước 2: Cập nhật các DTO hiện có

### Bước 2.1: Cập nhật `Application/Common/DTOs/Tags/TagAttributesDto.cs`

Bỏ thuộc tính `TagGroupId`. `CreatedAt` và `UpdatedAt` vẫn giữ lại vì DTO này được dùng cho endpoint `GET /tags` (nơi chúng cần hiển thị).

```csharp
// Application/Common/DTOs/Tags/TagAttributesDto.cs
namespace Application.Common.DTOs.Tags
{
    public class TagAttributesDto
    {
        public string Name { get; set; } = string.Empty;
        // Bỏ TagGroupId
        public string TagGroupName { get; set; } = string.Empty; 
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
```

### Bước 2.2: Cập nhật `Application/Common/DTOs/Mangas/MangaAttributesDto.cs`

Thay đổi kiểu của `Tags` từ `List<ResourceObject<TagAttributesDto>>` thành `List<ResourceObject<TagInMangaAttributesDto>>`.

```csharp
// Application/Common/DTOs/Mangas/MangaAttributesDto.cs
using Domain.Enums;
using Application.Common.DTOs.Tags; // Đảm bảo using đúng
using Application.Common.Models; 

namespace Application.Common.DTOs.Mangas
{
    public class MangaAttributesDto
    {
        public string Title { get; set; } = string.Empty;
        public string OriginalLanguage { get; set; } = string.Empty;
        public PublicationDemographic? PublicationDemographic { get; set; }
        public MangaStatus Status { get; set; }
        public int? Year { get; set; }
        public ContentRating ContentRating { get; set; }
        public bool IsLocked { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Thay đổi kiểu dữ liệu của Tags
        public List<ResourceObject<TagInMangaAttributesDto>> Tags { get; set; } = new List<ResourceObject<TagInMangaAttributesDto>>();
    }
}
```

---

## Bước 3: Cập nhật Mapping Profiles

### Bước 3.1: Cập nhật `Application/Common/Mappings/MappingProfile.cs`

Thêm mapping cho `Tag` sang `TagInMangaAttributesDto` và cập nhật mapping cho `Tag` sang `TagAttributesDto`.

```csharp
// Application/Common/Mappings/MappingProfile.cs
namespace Application.Common.Mappings
{
    using Application.Common.DTOs.Authors;
    using Application.Common.DTOs.Chapters;
    using Application.Common.DTOs.CoverArts;
    using Application.Common.DTOs.Mangas;
    using Application.Common.DTOs.TagGroups;
    using Application.Common.DTOs.Tags; // Đảm bảo using đúng
    using Application.Common.DTOs.TranslatedMangas;
    using Application.Common.DTOs.Users;
    using Application.Features.Authors.Commands.CreateAuthor;
    using Application.Features.Authors.Commands.UpdateAuthor;
    using Application.Features.Chapters.Commands.CreateChapter;
    using Application.Features.Chapters.Commands.UpdateChapter;
    using Application.Features.Mangas.Commands.CreateManga;
    using Application.Features.Mangas.Commands.UpdateManga;
    using Application.Features.TagGroups.Commands.CreateTagGroup;
    using Application.Features.TagGroups.Commands.UpdateTagGroup;
    using Application.Features.Tags.Commands.CreateTag;
    using Application.Features.Tags.Commands.UpdateTag;
    using Application.Features.TranslatedMangas.Commands.CreateTranslatedManga;
    using Application.Features.TranslatedMangas.Commands.UpdateTranslatedManga;
    using AutoMapper;
    using Domain.Entities;

    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // User
            CreateMap<User, UserAttributesDto>();

            // Author
            CreateMap<Author, AuthorAttributesDto>();
            CreateMap<CreateAuthorDto, Author>(); 
            CreateMap<UpdateAuthorDto, Author>(); 
            CreateMap<CreateAuthorCommand, Author>(); 
            CreateMap<UpdateAuthorCommand, Author>(); 

            // TagGroup
            CreateMap<TagGroup, TagGroupAttributesDto>();
            CreateMap<CreateTagGroupDto, TagGroup>(); 
            CreateMap<UpdateTagGroupDto, TagGroup>(); 
            CreateMap<CreateTagGroupCommand, TagGroup>(); 
            CreateMap<UpdateTagGroupCommand, TagGroup>(); 

            // Tag
            // Mapping cho TagAttributesDto (dùng cho /tags endpoint)
            CreateMap<Tag, TagAttributesDto>()
                .ForMember(dest => dest.TagGroupName, opt => opt.MapFrom(src => src.TagGroup != null ? src.TagGroup.Name : string.Empty));
                // TagGroupId đã bị bỏ khỏi TagAttributesDto

            // Mapping cho TagInMangaAttributesDto (dùng khi Tag nhúng vào Manga)
            CreateMap<Tag, TagInMangaAttributesDto>()
                .ForMember(dest => dest.TagGroupName, opt => opt.MapFrom(src => src.TagGroup != null ? src.TagGroup.Name : string.Empty));

            CreateMap<CreateTagDto, Tag>(); 
            CreateMap<UpdateTagDto, Tag>(); 
            CreateMap<CreateTagCommand, Tag>(); 
            CreateMap<UpdateTagCommand, Tag>(); 

            // Manga
            CreateMap<Manga, MangaAttributesDto>(); // Sẽ được cập nhật logic populate Tags trong Handler
            CreateMap<CreateMangaDto, Manga>(); 
            CreateMap<UpdateMangaDto, Manga>(); 
            CreateMap<CreateMangaCommand, Manga>(); 
            CreateMap<UpdateMangaCommand, Manga>(); 

            // TranslatedManga
            CreateMap<TranslatedManga, TranslatedMangaAttributesDto>();
            CreateMap<CreateTranslatedMangaDto, TranslatedManga>(); 
            CreateMap<UpdateTranslatedMangaDto, TranslatedManga>(); 
            CreateMap<CreateTranslatedMangaCommand, TranslatedManga>(); 
            CreateMap<UpdateTranslatedMangaCommand, TranslatedManga>(); 

            // CoverArt
            CreateMap<CoverArt, CoverArtAttributesDto>();
            CreateMap<CreateCoverArtDto, CoverArt>(); 

            // ChapterPage
            CreateMap<ChapterPage, ChapterPageAttributesDto>();
            CreateMap<CreateChapterPageDto, ChapterPage>(); 
            CreateMap<UpdateChapterPageDto, ChapterPage>(); 

            // Chapter
            CreateMap<Chapter, ChapterAttributesDto>()
                .ForMember(dest => dest.PagesCount, opt => opt.MapFrom(src => src.ChapterPages.Count));
            CreateMap<CreateChapterDto, Chapter>(); 
            CreateMap<UpdateChapterDto, Chapter>(); 
            CreateMap<CreateChapterCommand, Chapter>(); 
            CreateMap<UpdateChapterCommand, Chapter>(); 
        }
    }
}
```

---

## Bước 4: Cập nhật Queries và Query Handlers

### Bước 4.1: Cập nhật `Application/Features/Mangas/Queries/GetMangas/GetMangasQuery.cs`

Xóa thuộc tính `TagIdsFilter`.

```csharp
// Application/Features/Mangas/Queries/GetMangas/GetMangasQuery.cs
using Application.Common.DTOs;
using Application.Common.DTOs.Mangas;
using Application.Common.Models;
using Domain.Enums;
using MediatR;
using System.Collections.Generic;

namespace Application.Features.Mangas.Queries.GetMangas
{
    public class GetMangasQuery : IRequest<PagedResult<ResourceObject<MangaAttributesDto>>>
    {
        public int Offset { get; set; } = 0;
        public int Limit { get; set; } = 20;
        public string? TitleFilter { get; set; }
        public MangaStatus? StatusFilter { get; set; }
        public ContentRating? ContentRatingFilter { get; set; }
        public List<PublicationDemographic>? PublicationDemographicsFilter { get; set; }
        public string? OriginalLanguageFilter { get; set; }
        public int? YearFilter { get; set; }
        // Bỏ TagIdsFilter
        // public List<Guid>? TagIdsFilter { get; set; } 
        public List<Guid>? AuthorIdsFilter { get; set; } 
        
        public List<Guid>? IncludedTags { get; set; } // Giữ lại tên này để nhất quán với Mangadex
        public string? IncludedTagsMode { get; set; } 
        public List<Guid>? ExcludedTags { get; set; }
        public string? ExcludedTagsMode { get; set; }

        public string OrderBy { get; set; } = "UpdatedAt"; 
        public bool Ascending { get; set; } = false; 

        public List<string>? Includes { get; set; } 
    }
}
```

### Bước 4.2: Cập nhật `Application/Features/Mangas/Queries/GetMangas/GetMangasQueryHandler.cs`

*   Xóa logic xử lý `TagIdsFilter`.
*   Cập nhật cách populate `MangaAttributesDto.Tags` để sử dụng `TagInMangaAttributesDto` và không có `relationships`.
*   Khi include "author" hoặc "artist", tạo `RelationshipObject.Attributes` là một anonymous object chỉ chứa `Name` và `Biography`.

```csharp
// Application/Features/Mangas/Queries/GetMangas/GetMangasQueryHandler.cs
using Application.Common.DTOs;
using Application.Common.DTOs.Mangas;
using Application.Common.DTOs.Authors; 
using Application.Common.DTOs.Tags;    
using Application.Common.Extensions; 
using Application.Common.Models;
using Application.Contracts.Persistence;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions; 
using System.Linq; 

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

            Expression<Func<Manga, bool>> predicate = m => true; 

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
            // Bỏ logic cho TagIdsFilter
            if (request.AuthorIdsFilter != null && request.AuthorIdsFilter.Any())
            {
                predicate = predicate.And(m => m.MangaAuthors.Any(ma => request.AuthorIdsFilter.Contains(ma.AuthorId)));
            }

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
            
            var pagedMangas = await _unitOfWork.MangaRepository.GetPagedAsync(
                request.Offset,
                request.Limit,
                predicate,
                orderBy,
                includeProperties: "MangaTags.Tag.TagGroup,MangaAuthors.Author,CoverArts"
            );

            var mangaResourceObjects = new List<ResourceObject<MangaAttributesDto>>();
            bool includeAuthorFull = request.Includes?.Contains("author", StringComparer.OrdinalIgnoreCase) ?? false;
            //bool includeArtist = request.Includes?.Contains("artist", StringComparer.OrdinalIgnoreCase) ?? false; // Sẽ xử lý chung với includeAuthorFull
            bool includeCoverArt = request.Includes?.Contains("cover_art", StringComparer.OrdinalIgnoreCase) ?? false;


            foreach (var manga in pagedMangas.Items)
            {
                var mangaAttributes = _mapper.Map<MangaAttributesDto>(manga);
                
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

                if (manga.MangaAuthors != null)
                {
                    foreach (var mangaAuthor in manga.MangaAuthors)
                    {
                        if (mangaAuthor.Author != null) 
                        {
                            var relationshipType = mangaAuthor.Role == MangaStaffRole.Author ? "author" : "artist";
                            // Yêu cầu: Khi Include Author trong endpoint danh sách manga/chi tiết manga thì thật ra phải trả đầy đủ thông tin cho cả type author và type artist luôn.
                            // Nghĩa là nếu `includes` chứa "author", thì cả "author" và "artist" đều được populate attributes.
                            bool shouldIncludeAttributesForThisRelationship = includeAuthorFull;
                            
                            relationships.Add(new RelationshipObject
                            {
                                Id = mangaAuthor.Author.AuthorId.ToString(),
                                Type = relationshipType,
                                Attributes = shouldIncludeAttributesForThisRelationship 
                                    ? new { 
                                        mangaAuthor.Author.Name, 
                                        mangaAuthor.Author.Biography 
                                        // KHÔNG bao gồm CreatedAt, UpdatedAt
                                      } 
                                    : null
                            });
                        }
                    }
                }
                
                if (includeCoverArt)
                {
                    var primaryCover = manga.CoverArts?.OrderByDescending(ca => ca.CreatedAt).FirstOrDefault(); 
                    if (primaryCover != null && !string.IsNullOrEmpty(primaryCover.PublicId))
                    {
                        relationships.Add(new RelationshipObject
                        {
                            Id = primaryCover.PublicId, 
                            Type = "cover_art",
                            Attributes = null 
                        });
                    }
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

### Bước 4.3: Cập nhật `Application/Features/Mangas/Queries/GetMangaById/GetMangaByIdQueryHandler.cs`

*   Cập nhật cách populate `MangaAttributesDto.Tags` để sử dụng `TagInMangaAttributesDto` và không có `relationships`.
*   Khi include "author" hoặc "artist", tạo `RelationshipObject.Attributes` là một anonymous object chỉ chứa `Name` và `Biography`.

```csharp
// Application/Features/Mangas/Queries/GetMangaById/GetMangaByIdQueryHandler.cs
using Application.Common.DTOs.Authors; 
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
                .Select(mt => new ResourceObject<TagInMangaAttributesDto> // Sử dụng TagInMangaAttributesDto
                {
                    Id = mt.Tag.TagId.ToString(),
                    Type = "tag",
                    Attributes = _mapper.Map<TagInMangaAttributesDto>(mt.Tag), // Map sang TagInMangaAttributesDto
                    Relationships = null // Không có relationships cho tag khi nhúng trong manga
                })
                .ToList();
            
            var relationships = new List<RelationshipObject>();

            bool includeAuthorFull = request.Includes?.Contains("author", StringComparer.OrdinalIgnoreCase) ?? false;
            // bool includeArtist = request.Includes?.Contains("artist", StringComparer.OrdinalIgnoreCase) ?? false; // Xử lý chung với includeAuthorFull

            if (manga.MangaAuthors != null)
            {
                foreach (var mangaAuthor in manga.MangaAuthors)
                {
                    if (mangaAuthor.Author != null)
                    {
                        var relationshipType = mangaAuthor.Role == MangaStaffRole.Author ? "author" : "artist";
                        bool shouldIncludeAttributesForThisRelationship = includeAuthorFull;
                        
                        relationships.Add(new RelationshipObject
                        {
                            Id = mangaAuthor.Author.AuthorId.ToString(),
                            Type = relationshipType,
                            Attributes = shouldIncludeAttributesForThisRelationship 
                                ? new { 
                                    mangaAuthor.Author.Name, 
                                    mangaAuthor.Author.Biography
                                    // KHÔNG bao gồm CreatedAt, UpdatedAt
                                  } 
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
                    Attributes = null 
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
```

### Bước 4.4: Cập nhật `Application/Features/Tags/Queries/GetTags/GetTagsQueryHandler.cs`

Đảm bảo handler này trả về `TagAttributesDto` đầy đủ (`CreatedAt`, `UpdatedAt`, `TagGroupName`) và có `Relationships` (trỏ về `tag_group`) cho `ResourceObject`.

```csharp
// Application/Features/Tags/Queries/GetTags/GetTagsQueryHandler.cs
using Application.Common.DTOs;
using Application.Common.DTOs.Tags;
using Application.Common.Extensions;
using Application.Common.Models;
using Application.Contracts.Persistence;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace Application.Features.Tags.Queries.GetTags
{
    public class GetTagsQueryHandler : IRequestHandler<GetTagsQuery, PagedResult<ResourceObject<TagAttributesDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetTagsQueryHandler> _logger;

        public GetTagsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<GetTagsQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<PagedResult<ResourceObject<TagAttributesDto>>> Handle(GetTagsQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("GetTagsQueryHandler.Handle called with request: {@GetTagsQuery}", request);

            Expression<Func<Tag, bool>> predicate = t => true; 
            if (request.TagGroupId.HasValue)
            {
                predicate = predicate.And(t => t.TagGroupId == request.TagGroupId.Value);
            }
            if (!string.IsNullOrWhiteSpace(request.NameFilter))
            {
                predicate = predicate.And(t => t.Name.Contains(request.NameFilter));
            }

            Func<IQueryable<Tag>, IOrderedQueryable<Tag>> orderBy;
            switch (request.OrderBy?.ToLowerInvariant())
            {
                case "taggroupname":
                     orderBy = q => request.Ascending ? q.OrderBy(t => t.TagGroup.Name) : q.OrderByDescending(t => t.TagGroup.Name);
                    break;
                case "name":
                default:
                    orderBy = q => request.Ascending ? q.OrderBy(t => t.Name) : q.OrderByDescending(t => t.Name);
                    break;
            }

            var pagedTags = await _unitOfWork.TagRepository.GetPagedAsync(
                request.Offset,
                request.Limit,
                predicate,
                orderBy,
                includeProperties: "TagGroup" 
            );

            var tagResourceObjects = new List<ResourceObject<TagAttributesDto>>();
            foreach(var tag in pagedTags.Items)
            {
                // TagAttributesDto bây giờ đã bỏ TagGroupId, có CreatedAt, UpdatedAt
                var attributes = _mapper.Map<TagAttributesDto>(tag); 
                
                var relationships = new List<RelationshipObject>();
                if (tag.TagGroup != null)
                {
                    relationships.Add(new RelationshipObject
                    {
                        Id = tag.TagGroup.TagGroupId.ToString(),
                        Type = "tag_group"
                    });
                }
                // Đảm bảoCreatedAt và UpdatedAt được map chính xác vào attributes
                // mapping profile đã xử lý việc này.

                tagResourceObjects.Add(new ResourceObject<TagAttributesDto>
                {
                    Id = tag.TagId.ToString(),
                    Type = "tag",
                    Attributes = attributes, // Sẽ chứa CreatedAt, UpdatedAt
                    Relationships = relationships.Any() ? relationships : null // Sẽ có relationship với tag_group
                });
            }
            return new PagedResult<ResourceObject<TagAttributesDto>>(tagResourceObjects, pagedTags.Total, request.Offset, request.Limit);
        }
    }
}
```

---

## Bước 5: Cập nhật Tài liệu API

### Bước 5.1: Cập nhật `MangaReaderApi.yaml`

Cập nhật schema cho các DTO đã thay đổi (`TagAttributesDto`, `MangaAttributesDto`, `GetMangasQuery`).

**`TagAttributesDto` (trong schemas):**
*   Bỏ `tagGroupId`.
*   Đảm bảo `createdAt` và `updatedAt` có mặt (cho `GET /tags`).
*   Tạo schema mới `TagInMangaAttributesDto` chỉ có `name`, `tagGroupName`.

**`MangaAttributesDto` (trong schemas):**
*   Đảm bảo `tags` là mảng của `ResourceObject<TagInMangaAttributesDto>`.

**`GetMangas` (parameters):**
*   Xóa tham số `tagIdsFilter`.

*(Do nội dung file YAML dài, phần này chỉ mô tả thay đổi. Bạn cần cập nhật file YAML tương ứng)*

### Bước 5.2: Cập nhật `docs/api_conventions.md`

Phản ánh các thay đổi trong cấu trúc response của Manga và Tag, cũng như tham số của `GET /mangas`.

*(Do nội dung file MD dài, phần này chỉ mô tả thay đổi. Bạn cần cập nhật file MD tương ứng)*

*   **Cấu trúc Response Manga:**
    *   Mô tả `attributes.tags` là một mảng các `ResourceObject` với `attributes` là `TagInMangaAttributesDto` (chỉ có `name`, `tagGroupName`) và `relationships` là `null`.
    *   Mô tả `relationships` của Author/Artist sẽ có `attributes` (chứa `name`, `biography`) nếu được include, bỏ `createdAt`, `updatedAt`.
*   **Cấu trúc Response Tag (cho `GET /tags`):**
    *   Mô tả `attributes` chứa `name`, `tagGroupName`, `createdAt`, `updatedAt`.
    *   Mô tả `relationships` của `ResourceObject` chứa `tag_group`.
*   **Endpoint `GET /mangas`:**
    *   Xóa tham số `tagIdsFilter`.
    *   Cập nhật mô tả `includes` cho `author` để làm rõ việc trả về thông tin cho cả `author` và `artist`.

### Bước 5.3: Cập nhật `MangaReaderAPI.md` (External API Guide)

Tương tự như `api_conventions.md`, cập nhật hướng dẫn sử dụng cho người dùng API.

---

## Bước 6: Kiểm thử

Thực hiện kiểm thử kỹ lưỡng các API sau:

1.  **`GET /mangas`**:
    *   Không có `includes`.
    *   Với `includes=cover_art`.
    *   Với `includes=author`.
    *   Với `includes=author,cover_art`.
    *   Kiểm tra response của `attributes.tags` (chỉ có `name`, `tagGroupName`, không có `createdAt`, `updatedAt`, `relationships`).
    *   Kiểm tra response của `relationships` cho `author`/`artist` khi include (chỉ có `name`, `biography`).
    *   Đảm bảo không còn tham số `tagIdsFilter`.
2.  **`GET /mangas/{id}`**:
    *   Không có `includes`.
    *   Với `includes=author`.
    *   Kiểm tra response của `attributes.tags`.
    *   Kiểm tra response của `relationships` cho `author`/`artist` khi include.
3.  **`GET /tags`**:
    *   Kiểm tra `attributes` của Tag (có `name`, `tagGroupName`, `createdAt`, `updatedAt`).
    *   Kiểm tra `relationships` của `ResourceObject<TagAttributesDto>` (có `tag_group`).
4.  **`GET /tags/{id}`**:
    *   Tương tự như `GET /tags`.
5.  **`GET /authors` và `GET /authors/{id}`**:
    *   Đảm bảo vẫn trả về `createdAt` và `updatedAt` cho Author.

Hoàn tất các bước trên sẽ giúp API của bạn đáp ứng các yêu cầu mới một cách chính xác.
```