# TODO.md - Cập Nhật Cấu Trúc Dữ Liệu Trả Về API theo PlanDTO.md

Mục tiêu: Điều chỉnh cấu trúc dữ liệu trả về của API để tuân theo định dạng chuẩn gồm `id`, `type`, `attributes`, và `relationships`, trong đó `type` của `relationships` sẽ linh hoạt hơn để mô tả bản chất của mối quan hệ hoặc vai trò của thực thể liên quan.

## Bước 1: Tạo Các Model/DTOs Cơ Sở và Attributes DTOs

### 1.1. Tạo Model `ResourceObject.cs`

Đây là DTO wrapper cơ sở cho mỗi thực thể trả về.

```csharp
// Application/Common/Models/ResourceObject.cs
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Application.Common.Models
{
    public class ResourceObject<TAttributes> where TAttributes : class
    {
        [JsonPropertyOrder(1)]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyOrder(2)]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyOrder(3)]
        public TAttributes Attributes { get; set; } = null!;

        [JsonPropertyOrder(4)]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] // Không hiển thị nếu không có relationships
        public List<RelationshipObject>? Relationships { get; set; }
    }
}
```

### 1.2. Tạo Model `RelationshipObject.cs`

DTO này mô tả một mối quan hệ.

```csharp
// Application/Common/Models/RelationshipObject.cs
using System.Text.Json.Serialization;

namespace Application.Common.Models
{
    public class RelationshipObject
    {
        [JsonPropertyOrder(1)]
        public string Id { get; set; } = string.Empty; // ID của thực thể liên quan

        [JsonPropertyOrder(2)]
        public string Type { get; set; } = string.Empty; // Loại của mối quan hệ hoặc vai trò, ví dụ "author", "artist", "cover_art"
    }
}
```

### 1.3. Tạo Các DTOs `Attributes`

Với mỗi entity chính, chúng ta sẽ tạo một DTO `...AttributesDto.cs` chứa các thuộc tính của nó (trừ Id và relationships).

*   **AuthorAttributesDto.cs:**

    ```csharp
    // Application/Common/DTOs/Authors/AuthorAttributesDto.cs
    namespace Application.Common.DTOs.Authors
    {
        public class AuthorAttributesDto
        {
            public string Name { get; set; } = string.Empty;
            public string? Biography { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime UpdatedAt { get; set; }
        }
    }
    ```

*   **MangaAttributesDto.cs:**

    ```csharp
    // Application/Common/DTOs/Mangas/MangaAttributesDto.cs
    using Domain.Enums; 

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
        }
    }
    ```

*   **TagAttributesDto.cs:**

    ```csharp
    // Application/Common/DTOs/Tags/TagAttributesDto.cs
    namespace Application.Common.DTOs.Tags
    {
        public class TagAttributesDto
        {
            public string Name { get; set; } = string.Empty;
            public Guid TagGroupId { get; set; } 
            public string TagGroupName { get; set; } = string.Empty; 
            public DateTime CreatedAt { get; set; }
            public DateTime UpdatedAt { get; set; }
        }
    }
    ```

*   **TagGroupAttributesDto.cs:**

    ```csharp
    // Application/Common/DTOs/TagGroups/TagGroupAttributesDto.cs
    namespace Application.Common.DTOs.TagGroups
    {
        public class TagGroupAttributesDto
        {
            public string Name { get; set; } = string.Empty;
            public DateTime CreatedAt { get; set; }
            public DateTime UpdatedAt { get; set; }
        }
    }
    ```

*   **ChapterAttributesDto.cs:**

    ```csharp
    // Application/Common/DTOs/Chapters/ChapterAttributesDto.cs
    namespace Application.Common.DTOs.Chapters
    {
        public class ChapterAttributesDto
        {
            public string? Volume { get; set; }
            public string? ChapterNumber { get; set; }
            public string? Title { get; set; }
            public int PagesCount { get; set; }
            public DateTime PublishAt { get; set; }
            public DateTime ReadableAt { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime UpdatedAt { get; set; }
        }
    }
    ```

*   **ChapterPageAttributesDto.cs:**

    ```csharp
    // Application/Common/DTOs/Chapters/ChapterPageAttributesDto.cs
    namespace Application.Common.DTOs.Chapters
    {
        public class ChapterPageAttributesDto
        {
            public int PageNumber { get; set; }
            public string PublicId { get; set; } = string.Empty;
        }
    }
    ```

*   **CoverArtAttributesDto.cs:**

    ```csharp
    // Application/Common/DTOs/CoverArts/CoverArtAttributesDto.cs
    namespace Application.Common.DTOs.CoverArts
    {
        public class CoverArtAttributesDto
        {
            public string? Volume { get; set; }
            public string PublicId { get; set; } = string.Empty;
            public string? Description { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime UpdatedAt { get; set; }
        }
    }
    ```

*   **UserAttributesDto.cs:**

    ```csharp
    // Application/Common/DTOs/Users/UserAttributesDto.cs
    namespace Application.Common.DTOs.Users
    {
        public class UserAttributesDto
        {
            public string Username { get; set; } = string.Empty;
        }
    }
    ```

*   **TranslatedMangaAttributesDto.cs:**

    ```csharp
    // Application/Common/DTOs/TranslatedMangas/TranslatedMangaAttributesDto.cs
    namespace Application.Common.DTOs.TranslatedMangas
    {
        public class TranslatedMangaAttributesDto
        {
            public string LanguageKey { get; set; } = string.Empty;
            public string Title { get; set; } = string.Empty;
            public string? Description { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime UpdatedAt { get; set; }
        }
    }
    ```

## Bước 2: Cập Nhật Mapping Profile

Cập nhật `MappingProfile.cs` để thêm các mapping từ Entity sang các `...AttributesDto` mới.

```csharp
// Application/Common/Mappings/MappingProfile.cs
using AutoMapper;
using Application.Common.DTOs.Authors;
using Application.Common.DTOs.Chapters;
using Application.Common.DTOs.CoverArts;
using Application.Common.DTOs.Mangas;
using Application.Common.DTOs.TagGroups;
using Application.Common.DTOs.Tags;
using Application.Common.DTOs.TranslatedMangas;
using Application.Common.DTOs.Users;
using Domain.Entities;
using Application.Features.Mangas.Commands.CreateManga;
using Application.Features.Authors.Commands.CreateAuthor;
using Application.Features.TagGroups.Commands.CreateTagGroup;
using Application.Features.Tags.Commands.CreateTag;
using Application.Features.TranslatedMangas.Commands.CreateTranslatedManga;
using Application.Features.Chapters.Commands.CreateChapter;

namespace Application.Common.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // User
            CreateMap<User, UserDto>(); 
            CreateMap<User, UserAttributesDto>(); 

            // Author
            CreateMap<Author, AuthorDto>(); 
            CreateMap<Author, AuthorAttributesDto>(); 
            CreateMap<CreateAuthorDto, Author>(); 
            CreateMap<UpdateAuthorDto, Author>(); 
            CreateMap<CreateAuthorCommand, Author>(); 

            // TagGroup
            CreateMap<TagGroup, TagGroupDto>(); 
            CreateMap<TagGroup, TagGroupAttributesDto>(); 
            CreateMap<CreateTagGroupDto, TagGroup>();
            CreateMap<UpdateTagGroupDto, TagGroup>();
            CreateMap<CreateTagGroupCommand, TagGroup>();

            // Tag
            CreateMap<Tag, TagDto>()
                .ForMember(dest => dest.TagGroupName, opt => opt.MapFrom(src => src.TagGroup != null ? src.TagGroup.Name : string.Empty)); 
            CreateMap<Tag, TagAttributesDto>() 
                 .ForMember(dest => dest.TagGroupName, opt => opt.MapFrom(src => src.TagGroup != null ? src.TagGroup.Name : string.Empty));
            CreateMap<CreateTagDto, Tag>();
            CreateMap<UpdateTagDto, Tag>();
            CreateMap<CreateTagCommand, Tag>();

            // Manga
            CreateMap<Manga, MangaDto>() 
                .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.MangaTags.Select(mt => mt.Tag)))
                .ForMember(dest => dest.Authors, opt => opt.MapFrom(src => src.MangaAuthors.Select(ma => ma.Author)));
            CreateMap<Manga, MangaAttributesDto>(); 
            CreateMap<CreateMangaDto, Manga>(); 
            CreateMap<UpdateMangaDto, Manga>(); 
            CreateMap<CreateMangaCommand, Manga>(); 

            // TranslatedManga
            CreateMap<TranslatedManga, TranslatedMangaDto>(); 
            CreateMap<TranslatedManga, TranslatedMangaAttributesDto>(); 
            CreateMap<CreateTranslatedMangaDto, TranslatedManga>();
            CreateMap<UpdateTranslatedMangaDto, TranslatedManga>();
            CreateMap<CreateTranslatedMangaCommand, TranslatedManga>();

            // CoverArt
            CreateMap<CoverArt, CoverArtDto>(); 
            CreateMap<CoverArt, CoverArtAttributesDto>(); 
            CreateMap<CreateCoverArtDto, CoverArt>(); 
            
            // ChapterPage
            CreateMap<ChapterPage, ChapterPageDto>(); 
            CreateMap<ChapterPage, ChapterPageAttributesDto>(); 
            CreateMap<CreateChapterPageDto, ChapterPage>(); 
            CreateMap<UpdateChapterPageDto, ChapterPage>(); 
            
            // Chapter
            CreateMap<Chapter, ChapterDto>() 
                .ForMember(dest => dest.Uploader, opt => opt.MapFrom(src => src.User))
                .ForMember(dest => dest.PagesCount, opt => opt.MapFrom(src => src.ChapterPages.Count))
                .ForMember(dest => dest.ChapterPages, opt => opt.MapFrom(src => src.ChapterPages.OrderBy(p => p.PageNumber)));
            CreateMap<Chapter, ChapterAttributesDto>() 
                .ForMember(dest => dest.PagesCount, opt => opt.MapFrom(src => src.ChapterPages.Count));
            CreateMap<CreateChapterDto, Chapter>();
            CreateMap<UpdateChapterDto, Chapter>();
            CreateMap<CreateChapterCommand, Chapter>();
        }
    }
}
```

## Bước 3: Cập Nhật Query Handlers

Cung cấp mã nguồn đầy đủ cho các Query Handler và file Query tương ứng.

### 3.1. Author Queries

*   **`GetAuthorByIdQuery.cs`**

    ```csharp
    // Application/Features/Authors/Queries/GetAuthorById/GetAuthorByIdQuery.cs
    using Application.Common.DTOs.Authors;
    using Application.Common.Models; // Cho ResourceObject
    using MediatR;

    namespace Application.Features.Authors.Queries.GetAuthorById
    {
        public class GetAuthorByIdQuery : IRequest<ResourceObject<AuthorAttributesDto>?>
        {
            public Guid AuthorId { get; set; }
        }
    }
    ```

*   **`GetAuthorByIdQueryHandler.cs`**

    ```csharp
    // Application/Features/Authors/Queries/GetAuthorById/GetAuthorByIdQueryHandler.cs
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
    ```

*   **`GetAuthorsQuery.cs`**

    ```csharp
    // Application/Features/Authors/Queries/GetAuthors/GetAuthorsQuery.cs
    using Application.Common.DTOs;
    using Application.Common.DTOs.Authors;
    using Application.Common.Models; // Cho ResourceObject
    using MediatR;

    namespace Application.Features.Authors.Queries.GetAuthors
    {
        public class GetAuthorsQuery : IRequest<PagedResult<ResourceObject<AuthorAttributesDto>>>
        {
            public int Offset { get; set; } = 0;
            public int Limit { get; set; } = 20;
            public string? NameFilter { get; set; }
            public string OrderBy { get; set; } = "Name"; 
            public bool Ascending { get; set; } = true;
        }
    }
    ```

*   **`GetAuthorsQueryHandler.cs`**

    ```csharp
    // Application/Features/Authors/Queries/GetAuthors/GetAuthorsQueryHandler.cs
    using Application.Common.DTOs;
    using Application.Common.DTOs.Authors;
    using Application.Common.Models;
    using Application.Contracts.Persistence;
    using AutoMapper;
    using Domain.Entities;
    using MediatR;
    using Microsoft.Extensions.Logging;
    using System.Linq.Expressions;
    using Application.Common.Extensions;

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
    ```

### 3.2. Manga Queries

*   **`GetMangaByIdQuery.cs`**

    ```csharp
    // Application/Features/Mangas/Queries/GetMangaById/GetMangaByIdQuery.cs
    using Application.Common.DTOs.Mangas;
    using Application.Common.Models; 
    using MediatR;

    namespace Application.Features.Mangas.Queries.GetMangaById
    {
        public class GetMangaByIdQuery : IRequest<ResourceObject<MangaAttributesDto>?>
        {
            public Guid MangaId { get; set; }
        }
    }
    ```
*   **`GetMangaByIdQueryHandler.cs`**

    ```csharp
    // Application/Features/Mangas/Queries/GetMangaById/GetMangaByIdQueryHandler.cs
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
    ```

*   **`GetMangasQuery.cs`**

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
            public PublicationDemographic? DemographicFilter { get; set; }
            public string? OriginalLanguageFilter { get; set; }
            public int? YearFilter { get; set; }
            public List<Guid>? TagIdsFilter { get; set; } 
            public List<Guid>? AuthorIdsFilter { get; set; } 
            public string OrderBy { get; set; } = "UpdatedAt"; 
            public bool Ascending { get; set; } = false; 
        }
    }
    ```

*   **`GetMangasQueryHandler.cs`**

    ```csharp
    // Application/Features/Mangas/Queries/GetMangas/GetMangasQueryHandler.cs
    using Application.Common.DTOs;
    using Application.Common.DTOs.Mangas;
    using Application.Common.Models; 
    using Application.Contracts.Persistence;
    using AutoMapper;
    using Domain.Entities;
    using Domain.Enums;
    using MediatR;
    using Microsoft.EntityFrameworkCore; 
    using Microsoft.Extensions.Logging;
    using System.Linq.Expressions; 
    using Application.Common.Extensions; 

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
                    predicate = predicate.And(m => m.MangaAuthors.Any(ma => request.AuthorIdsFilter.Contains(ma.AuthorId)));
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
                foreach (var manga in pagedMangas.Items)
                {
                    var mangaAttributes = _mapper.Map<MangaAttributesDto>(manga);
                    var relationships = new List<RelationshipObject>();

                    if (manga.MangaAuthors != null)
                    {
                        foreach (var mangaAuthor in manga.MangaAuthors)
                        {
                            if (mangaAuthor.Author != null) {
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
                            if (mangaTag.Tag != null) {
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

### 3.3. Tag Queries

*   **`GetTagByIdQuery.cs`**

    ```csharp
    // Application/Features/Tags/Queries/GetTagById/GetTagByIdQuery.cs
    using Application.Common.DTOs.Tags;
    using Application.Common.Models;
    using MediatR;

    namespace Application.Features.Tags.Queries.GetTagById
    {
        public class GetTagByIdQuery : IRequest<ResourceObject<TagAttributesDto>?>
        {
            public Guid TagId { get; set; }
        }
    }
    ```

*   **`GetTagByIdQueryHandler.cs`**

    ```csharp
    // Application/Features/Tags/Queries/GetTagById/GetTagByIdQueryHandler.cs
    using Application.Common.DTOs.Tags;
    using Application.Common.Models;
    using Application.Contracts.Persistence;
    using AutoMapper;
    using MediatR;
    using Microsoft.Extensions.Logging;
    using Domain.Entities;

    namespace Application.Features.Tags.Queries.GetTagById
    {
        public class GetTagByIdQueryHandler : IRequestHandler<GetTagByIdQuery, ResourceObject<TagAttributesDto>?>
        {
            private readonly IUnitOfWork _unitOfWork;
            private readonly IMapper _mapper;
            private readonly ILogger<GetTagByIdQueryHandler> _logger;

            public GetTagByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<GetTagByIdQueryHandler> logger)
            {
                _unitOfWork = unitOfWork;
                _mapper = mapper;
                _logger = logger;
            }

            public async Task<ResourceObject<TagAttributesDto>?> Handle(GetTagByIdQuery request, CancellationToken cancellationToken)
            {
                _logger.LogInformation("GetTagByIdQueryHandler.Handle - Lấy tag với ID: {TagId}", request.TagId);
                
                var tag = await _unitOfWork.TagRepository.FindFirstOrDefaultAsync(
                    t => t.TagId == request.TagId,
                    includeProperties: "TagGroup" 
                );

                if (tag == null)
                {
                    _logger.LogWarning("Không tìm thấy tag với ID: {TagId}", request.TagId);
                    return null;
                }
                
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
                
                return new ResourceObject<TagAttributesDto>
                {
                    Id = tag.TagId.ToString(),
                    Type = "tag",
                    Attributes = attributes,
                    Relationships = relationships.Any() ? relationships : null
                };
            }
        }
    }
    ```

*   **`GetTagsQuery.cs`**

    ```csharp
    // Application/Features/Tags/Queries/GetTags/GetTagsQuery.cs
    using Application.Common.DTOs;
    using Application.Common.DTOs.Tags;
    using Application.Common.Models;
    using MediatR;
    using System;

    namespace Application.Features.Tags.Queries.GetTags
    {
        public class GetTagsQuery : IRequest<PagedResult<ResourceObject<TagAttributesDto>>>
        {
            public int Offset { get; set; } = 0;
            public int Limit { get; set; } = 100;
            public Guid? TagGroupId { get; set; }
            public string? NameFilter { get; set; }
            public string OrderBy { get; set; } = "Name"; 
            public bool Ascending { get; set; } = true;
        }
    }
    ```

*   **`GetTagsQueryHandler.cs`**

    ```csharp
    // Application/Features/Tags/Queries/GetTags/GetTagsQueryHandler.cs
    using Application.Common.DTOs;
    using Application.Common.DTOs.Tags;
    using Application.Common.Models;
    using Application.Contracts.Persistence;
    using AutoMapper;
    using Domain.Entities;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using System.Linq.Expressions;
    using Application.Common.Extensions;

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
                    tagResourceObjects.Add(new ResourceObject<TagAttributesDto>
                    {
                        Id = tag.TagId.ToString(),
                        Type = "tag",
                        Attributes = attributes,
                        Relationships = relationships.Any() ? relationships : null
                    });
                }
                return new PagedResult<ResourceObject<TagAttributesDto>>(tagResourceObjects, pagedTags.Total, request.Offset, request.Limit);
            }
        }
    }
    ```

### 3.4. TagGroup Queries

*   **`GetTagGroupByIdQuery.cs`**

    ```csharp
    // Application/Features/TagGroups/Queries/GetTagGroupById/GetTagGroupByIdQuery.cs
    using Application.Common.DTOs.TagGroups;
    using Application.Common.Models;
    using MediatR;

    namespace Application.Features.TagGroups.Queries.GetTagGroupById
    {
        public class GetTagGroupByIdQuery : IRequest<ResourceObject<TagGroupAttributesDto>?>
        {
            public Guid TagGroupId { get; set; }
            public bool IncludeTags { get; set; } = false;
        }
    }
    ```

*   **`GetTagGroupByIdQueryHandler.cs`**

    ```csharp
    // Application/Features/TagGroups/Queries/GetTagGroupById/GetTagGroupByIdQueryHandler.cs
    using Application.Common.DTOs.TagGroups;
    using Application.Common.Models;
    using Application.Contracts.Persistence;
    using AutoMapper;
    using MediatR;
    using Microsoft.Extensions.Logging;
    using Domain.Entities;

    namespace Application.Features.TagGroups.Queries.GetTagGroupById
    {
        public class GetTagGroupByIdQueryHandler : IRequestHandler<GetTagGroupByIdQuery, ResourceObject<TagGroupAttributesDto>?>
        {
            private readonly IUnitOfWork _unitOfWork;
            private readonly IMapper _mapper;
            private readonly ILogger<GetTagGroupByIdQueryHandler> _logger;

            public GetTagGroupByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<GetTagGroupByIdQueryHandler> logger)
            {
                _unitOfWork = unitOfWork;
                _mapper = mapper;
                _logger = logger;
            }

            public async Task<ResourceObject<TagGroupAttributesDto>?> Handle(GetTagGroupByIdQuery request, CancellationToken cancellationToken)
            {
                _logger.LogInformation("GetTagGroupByIdQueryHandler.Handle - Lấy tag group với ID: {TagGroupId}, IncludeTags: {IncludeTags}", request.TagGroupId, request.IncludeTags);
                
                TagGroup? tagGroup;
                if (request.IncludeTags)
                {
                    tagGroup = await _unitOfWork.TagGroupRepository.GetTagGroupWithTagsAsync(request.TagGroupId);
                }
                else
                {
                    tagGroup = await _unitOfWork.TagGroupRepository.GetByIdAsync(request.TagGroupId);
                }
                
                if (tagGroup == null)
                {
                    _logger.LogWarning("Không tìm thấy tag group với ID: {TagGroupId}", request.TagGroupId);
                    return null;
                }

                var attributes = _mapper.Map<TagGroupAttributesDto>(tagGroup);
                var relationships = new List<RelationshipObject>();

                if (request.IncludeTags && tagGroup.Tags != null)
                {
                    foreach(var tag in tagGroup.Tags)
                    {
                        relationships.Add(new RelationshipObject
                        {
                            Id = tag.TagId.ToString(),
                            Type = "tag"
                        });
                    }
                }
                
                return new ResourceObject<TagGroupAttributesDto>
                {
                    Id = tagGroup.TagGroupId.ToString(),
                    Type = "tag_group",
                    Attributes = attributes,
                    Relationships = relationships.Any() ? relationships : null
                };
            }
        }
    }
    ```

*   **`GetTagGroupsQuery.cs`**

    ```csharp
    // Application/Features/TagGroups/Queries/GetTagGroups/GetTagGroupsQuery.cs
    using Application.Common.DTOs;
    using Application.Common.DTOs.TagGroups;
    using Application.Common.Models;
    using MediatR;

    namespace Application.Features.TagGroups.Queries.GetTagGroups
    {
        public class GetTagGroupsQuery : IRequest<PagedResult<ResourceObject<TagGroupAttributesDto>>>
        {
            public int Offset { get; set; } = 0;
            public int Limit { get; set; } = 100;
            public string? NameFilter { get; set; }
            public string OrderBy { get; set; } = "Name";
            public bool Ascending { get; set; } = true;
            // public bool IncludeTags { get; set; } = false; 
        }
    }
    ```

*   **`GetTagGroupsQueryHandler.cs`**

    ```csharp
    // Application/Features/TagGroups/Queries/GetTagGroups/GetTagGroupsQueryHandler.cs
    using Application.Common.DTOs;
    using Application.Common.DTOs.TagGroups;
    using Application.Common.Models;
    using Application.Contracts.Persistence;
    using AutoMapper;
    using Domain.Entities;
    using MediatR;
    using Microsoft.Extensions.Logging;
    using System.Linq.Expressions;
    using Application.Common.Extensions;

    namespace Application.Features.TagGroups.Queries.GetTagGroups
    {
        public class GetTagGroupsQueryHandler : IRequestHandler<GetTagGroupsQuery, PagedResult<ResourceObject<TagGroupAttributesDto>>>
        {
            private readonly IUnitOfWork _unitOfWork;
            private readonly IMapper _mapper;
            private readonly ILogger<GetTagGroupsQueryHandler> _logger;

            public GetTagGroupsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<GetTagGroupsQueryHandler> logger)
            {
                _unitOfWork = unitOfWork;
                _mapper = mapper;
                _logger = logger;
            }

            public async Task<PagedResult<ResourceObject<TagGroupAttributesDto>>> Handle(GetTagGroupsQuery request, CancellationToken cancellationToken)
            {
                _logger.LogInformation("GetTagGroupsQueryHandler.Handle called with request: {@GetTagGroupsQuery}", request);

                Expression<Func<TagGroup, bool>>? filter = null;
                if (!string.IsNullOrWhiteSpace(request.NameFilter))
                {
                    filter = tg => tg.Name.Contains(request.NameFilter);
                }
                
                Func<IQueryable<TagGroup>, IOrderedQueryable<TagGroup>> orderBy;
                 switch (request.OrderBy?.ToLowerInvariant())
                {
                    case "name":
                    default: 
                        orderBy = q => request.Ascending ? q.OrderBy(tg => tg.Name) : q.OrderByDescending(tg => tg.Name);
                        break;
                }
                
                var pagedTagGroups = await _unitOfWork.TagGroupRepository.GetPagedAsync(
                    request.Offset,
                    request.Limit,
                    filter,
                    orderBy
                );

                var resourceObjects = new List<ResourceObject<TagGroupAttributesDto>>();
                foreach(var tg in pagedTagGroups.Items)
                {
                    var attributes = _mapper.Map<TagGroupAttributesDto>(tg);
                    var relationships = new List<RelationshipObject>();
                    // Logic to add relationships if IncludeTags was true and implemented
                    resourceObjects.Add(new ResourceObject<TagGroupAttributesDto>
                    {
                        Id = tg.TagGroupId.ToString(),
                        Type = "tag_group",
                        Attributes = attributes,
                        Relationships = relationships.Any() ? relationships : null
                    });
                }
                return new PagedResult<ResourceObject<TagGroupAttributesDto>>(resourceObjects, pagedTagGroups.Total, request.Offset, request.Limit);
            }
        }
    }
    ```

### 3.5. Chapter Queries

*   **`GetChapterByIdQuery.cs`**

    ```csharp
    // Application/Features/Chapters/Queries/GetChapterById/GetChapterByIdQuery.cs
    using Application.Common.DTOs.Chapters;
    using Application.Common.Models;
    using MediatR;

    namespace Application.Features.Chapters.Queries.GetChapterById
    {
        public class GetChapterByIdQuery : IRequest<ResourceObject<ChapterAttributesDto>?>
        {
            public Guid ChapterId { get; set; }
        }
    }
    ```

*   **`GetChapterByIdQueryHandler.cs`**

    ```csharp
    // Application/Features/Chapters/Queries/GetChapterById/GetChapterByIdQueryHandler.cs
    using Application.Common.DTOs.Chapters;
    using Application.Common.Models;
    using Application.Contracts.Persistence;
    using AutoMapper;
    using MediatR;
    using Microsoft.Extensions.Logging;
    using Domain.Entities;

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
    ```

*   **`GetChaptersByTranslatedMangaQuery.cs`**

    ```csharp
    // Application/Features/Chapters/Queries/GetChaptersByTranslatedManga/GetChaptersByTranslatedMangaQuery.cs
    using Application.Common.DTOs;
    using Application.Common.DTOs.Chapters;
    using Application.Common.Models;
    using MediatR;
    using System.Collections.Generic;

    namespace Application.Features.Chapters.Queries.GetChaptersByTranslatedManga
    {
        public class GetChaptersByTranslatedMangaQuery : IRequest<PagedResult<ResourceObject<ChapterAttributesDto>>>
        {
            public Guid TranslatedMangaId { get; set; }
            public int Offset { get; set; } = 0;
            public int Limit { get; set; } = 20; 
            public string OrderBy { get; set; } = "ChapterNumber"; 
            public bool Ascending { get; set; } = true;
        }
    }
    ```

*   **`GetChaptersByTranslatedMangaQueryHandler.cs`**

    ```csharp
    // Application/Features/Chapters/Queries/GetChaptersByTranslatedManga/GetChaptersByTranslatedMangaQueryHandler.cs
    using Application.Common.DTOs;
    using Application.Common.DTOs.Chapters;
    using Application.Common.Models;
    using Application.Contracts.Persistence;
    using AutoMapper;
    using Domain.Entities;
    using MediatR;
    using Microsoft.EntityFrameworkCore; 
    using Microsoft.Extensions.Logging;
    using System.Linq.Expressions;
    using Application.Common.Extensions;

    namespace Application.Features.Chapters.Queries.GetChaptersByTranslatedManga
    {
        public class GetChaptersByTranslatedMangaQueryHandler : IRequestHandler<GetChaptersByTranslatedMangaQuery, PagedResult<ResourceObject<ChapterAttributesDto>>>
        {
            private readonly IUnitOfWork _unitOfWork;
            private readonly IMapper _mapper;
            private readonly ILogger<GetChaptersByTranslatedMangaQueryHandler> _logger;

            public GetChaptersByTranslatedMangaQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<GetChaptersByTranslatedMangaQueryHandler> logger)
            {
                _unitOfWork = unitOfWork;
                _mapper = mapper;
                _logger = logger;
            }

            public async Task<PagedResult<ResourceObject<ChapterAttributesDto>>> Handle(GetChaptersByTranslatedMangaQuery request, CancellationToken cancellationToken)
            {
                _logger.LogInformation("GetChaptersByTranslatedMangaQueryHandler.Handle - Lấy chapters cho TranslatedMangaId: {TranslatedMangaId}", request.TranslatedMangaId);

                var translatedMangaExists = await _unitOfWork.TranslatedMangaRepository.ExistsAsync(request.TranslatedMangaId);
                if (!translatedMangaExists)
                {
                    _logger.LogWarning("Không tìm thấy TranslatedManga với ID: {TranslatedMangaId} khi lấy danh sách chapter.", request.TranslatedMangaId);
                    return new PagedResult<ResourceObject<ChapterAttributesDto>>(new List<ResourceObject<ChapterAttributesDto>>(), 0, request.Offset, request.Limit);
                }

                Expression<Func<Chapter, bool>> filter = c => c.TranslatedMangaId == request.TranslatedMangaId;
                
                Func<IQueryable<Chapter>, IOrderedQueryable<Chapter>> orderBy;
                switch (request.OrderBy?.ToLowerInvariant())
                {
                    case "volume":
                        orderBy = q => request.Ascending ? 
                                       q.OrderBy(c => c.Volume).ThenBy(c => c.ChapterNumber) : 
                                       q.OrderByDescending(c => c.Volume).ThenByDescending(c => c.ChapterNumber);
                        break;
                    case "publishat":
                        orderBy = q => request.Ascending ? q.OrderBy(c => c.PublishAt) : q.OrderByDescending(c => c.PublishAt);
                        break;
                    case "chapternumber":
                    default: 
                        orderBy = q => request.Ascending ? 
                                       q.OrderBy(c => c.ChapterNumber).ThenBy(c => c.Volume) : 
                                       q.OrderByDescending(c => c.ChapterNumber).ThenByDescending(c => c.Volume);
                        break;
                }

                var pagedChapters = await _unitOfWork.ChapterRepository.GetPagedAsync(
                    request.Offset,
                    request.Limit,
                    filter,
                    orderBy,
                    includeProperties: "User,ChapterPages,TranslatedManga.Manga" 
                );
                
                var resourceObjects = new List<ResourceObject<ChapterAttributesDto>>();
                foreach(var chapter in pagedChapters.Items)
                {
                    var attributes = _mapper.Map<ChapterAttributesDto>(chapter);
                    var relationships = new List<RelationshipObject>();
                    if (chapter.User != null)
                    {
                        relationships.Add(new RelationshipObject { Id = chapter.User.UserId.ToString(), Type = "user" });
                    }
                    if (chapter.TranslatedManga?.Manga != null)
                    {
                        relationships.Add(new RelationshipObject { Id = chapter.TranslatedManga.Manga.MangaId.ToString(), Type = "manga" });
                    }
                    resourceObjects.Add(new ResourceObject<ChapterAttributesDto>
                    {
                        Id = chapter.ChapterId.ToString(),
                        Type = "chapter",
                        Attributes = attributes,
                        Relationships = relationships.Any() ? relationships : null
                    });
                }
                return new PagedResult<ResourceObject<ChapterAttributesDto>>(resourceObjects, pagedChapters.Total, request.Offset, request.Limit);
            }
        }
    }
    ```

*   **`GetChapterPagesQuery.cs`**

    ```csharp
    // Application/Features/Chapters/Queries/GetChapterPages/GetChapterPagesQuery.cs
    using Application.Common.DTOs;
    using Application.Common.DTOs.Chapters;
    using Application.Common.Models;
    using MediatR;

    namespace Application.Features.Chapters.Queries.GetChapterPages
    {
        public class GetChapterPagesQuery : IRequest<PagedResult<ResourceObject<ChapterPageAttributesDto>>>
        {
            public Guid ChapterId { get; set; }
            public int Offset { get; set; } = 0;
            public int Limit { get; set; } = 20; 
        }
    }
    ```

*   **`GetChapterPagesQueryHandler.cs`**

    ```csharp
    // Application/Features/Chapters/Queries/GetChapterPages/GetChapterPagesQueryHandler.cs
    using Application.Common.DTOs;
    using Application.Common.DTOs.Chapters;
    using Application.Common.Models;
    using Application.Contracts.Persistence;
    using AutoMapper;
    using Domain.Entities; 
    using MediatR;
    using Microsoft.Extensions.Logging;
    using System.Linq.Expressions; 

    namespace Application.Features.Chapters.Queries.GetChapterPages
    {
        public class GetChapterPagesQueryHandler : IRequestHandler<GetChapterPagesQuery, PagedResult<ResourceObject<ChapterPageAttributesDto>>>
        {
            private readonly IUnitOfWork _unitOfWork; 
            private readonly IMapper _mapper;
            private readonly ILogger<GetChapterPagesQueryHandler> _logger;

            public GetChapterPagesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<GetChapterPagesQueryHandler> logger)
            {
                _unitOfWork = unitOfWork;
                _mapper = mapper;
                _logger = logger;
            }

            public async Task<PagedResult<ResourceObject<ChapterPageAttributesDto>>> Handle(GetChapterPagesQuery request, CancellationToken cancellationToken)
            {
                _logger.LogInformation("GetChapterPagesQueryHandler.Handle - Lấy các trang cho ChapterId: {ChapterId}, Offset: {Offset}, Limit: {Limit}",
                    request.ChapterId, request.Offset, request.Limit);

                var chapterExists = await _unitOfWork.ChapterRepository.ExistsAsync(request.ChapterId);
                if (!chapterExists)
                {
                    _logger.LogWarning("Không tìm thấy Chapter với ID: {ChapterId} khi lấy danh sách trang.", request.ChapterId);
                    return new PagedResult<ResourceObject<ChapterPageAttributesDto>>(new List<ResourceObject<ChapterPageAttributesDto>>(), 0, request.Offset, request.Limit);
                }
                
                var chapterWithPages = await _unitOfWork.ChapterRepository.GetChapterWithPagesAsync(request.ChapterId);
                if (chapterWithPages == null || chapterWithPages.ChapterPages == null)
                {
                     _logger.LogWarning("Chapter with ID {ChapterId} found but has no pages.", request.ChapterId);
                     return new PagedResult<ResourceObject<ChapterPageAttributesDto>>(new List<ResourceObject<ChapterPageAttributesDto>>(), 0, request.Offset, request.Limit);
                }

                var allPages = chapterWithPages.ChapterPages.OrderBy(p => p.PageNumber).ToList();
                var totalCount = allPages.Count;
                
                var items = allPages.Skip(request.Offset)
                                    .Take(request.Limit)
                                    .ToList();

                var resourceObjects = items.Select(page => {
                    var attributes = _mapper.Map<ChapterPageAttributesDto>(page);
                    var relationships = new List<RelationshipObject>
                    {
                        new RelationshipObject { Id = page.ChapterId.ToString(), Type = "chapter" }
                    };
                    return new ResourceObject<ChapterPageAttributesDto>
                    {
                        Id = page.PageId.ToString(),
                        Type = "chapter_page", 
                        Attributes = attributes,
                        Relationships = relationships // ChapterPage always has a relationship to chapter
                    };
                }).ToList();
                
                return new PagedResult<ResourceObject<ChapterPageAttributesDto>>(resourceObjects, totalCount, request.Offset, request.Limit);
            }
        }
    }
    ```

### 3.6. CoverArt Queries

*   **`GetCoverArtByIdQuery.cs`**

    ```csharp
    // Application/Features/CoverArts/Queries/GetCoverArtById/GetCoverArtByIdQuery.cs
    using Application.Common.DTOs.CoverArts;
    using Application.Common.Models;
    using MediatR;

    namespace Application.Features.CoverArts.Queries.GetCoverArtById
    {
        public class GetCoverArtByIdQuery : IRequest<ResourceObject<CoverArtAttributesDto>?>
        {
            public Guid CoverId { get; set; }
        }
    }
    ```

*   **`GetCoverArtByIdQueryHandler.cs`**

    ```csharp
    // Application/Features/CoverArts/Queries/GetCoverArtById/GetCoverArtByIdQueryHandler.cs
    using Application.Common.DTOs.CoverArts;
    using Application.Common.Models;
    using Application.Contracts.Persistence;
    using AutoMapper;
    using MediatR;
    using Microsoft.Extensions.Logging;
    using Domain.Entities;

    namespace Application.Features.CoverArts.Queries.GetCoverArtById
    {
        public class GetCoverArtByIdQueryHandler : IRequestHandler<GetCoverArtByIdQuery, ResourceObject<CoverArtAttributesDto>?>
        {
            private readonly IUnitOfWork _unitOfWork;
            private readonly IMapper _mapper;
            private readonly ILogger<GetCoverArtByIdQueryHandler> _logger;

            public GetCoverArtByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<GetCoverArtByIdQueryHandler> logger)
            {
                _unitOfWork = unitOfWork;
                _mapper = mapper;
                _logger = logger;
            }

            public async Task<ResourceObject<CoverArtAttributesDto>?> Handle(GetCoverArtByIdQuery request, CancellationToken cancellationToken)
            {
                _logger.LogInformation("GetCoverArtByIdQueryHandler.Handle - Lấy cover art với ID: {CoverId}", request.CoverId);
                var coverArt = await _unitOfWork.CoverArtRepository.FindFirstOrDefaultAsync(
                    ca => ca.CoverId == request.CoverId,
                    includeProperties: "Manga" 
                );

                if (coverArt == null)
                {
                    _logger.LogWarning("Không tìm thấy cover art với ID: {CoverId}", request.CoverId);
                    return null;
                }
                var attributes = _mapper.Map<CoverArtAttributesDto>(coverArt);
                var relationships = new List<RelationshipObject>();
                if (coverArt.Manga != null)
                {
                    relationships.Add(new RelationshipObject
                    {
                        Id = coverArt.Manga.MangaId.ToString(),
                        Type = "manga"
                    });
                }

                return new ResourceObject<CoverArtAttributesDto>
                {
                    Id = coverArt.CoverId.ToString(),
                    Type = "cover_art",
                    Attributes = attributes,
                    Relationships = relationships.Any() ? relationships : null
                };
            }
        }
    }
    ```

*   **`GetCoverArtsByMangaQuery.cs`**

    ```csharp
    // Application/Features/CoverArts/Queries/GetCoverArtsByManga/GetCoverArtsByMangaQuery.cs
    using Application.Common.DTOs;
    using Application.Common.DTOs.CoverArts;
    using Application.Common.Models;
    using MediatR;

    namespace Application.Features.CoverArts.Queries.GetCoverArtsByManga
    {
        public class GetCoverArtsByMangaQuery : IRequest<PagedResult<ResourceObject<CoverArtAttributesDto>>>
        {
            public Guid MangaId { get; set; }
            public int Offset { get; set; } = 0;
            public int Limit { get; set; } = 20;
        }
    }
    ```

*   **`GetCoverArtsByMangaQueryHandler.cs`**

    ```csharp
    // Application/Features/CoverArts/Queries/GetCoverArtsByManga/GetCoverArtsByMangaQueryHandler.cs
    using Application.Common.DTOs;
    using Application.Common.DTOs.CoverArts;
    using Application.Common.Models;
    using Application.Contracts.Persistence;
    using AutoMapper;
    using Domain.Entities;
    using MediatR;
    using Microsoft.Extensions.Logging;
    using System.Linq.Expressions;
    using Application.Common.Extensions;

    namespace Application.Features.CoverArts.Queries.GetCoverArtsByManga
    {
        public class GetCoverArtsByMangaQueryHandler : IRequestHandler<GetCoverArtsByMangaQuery, PagedResult<ResourceObject<CoverArtAttributesDto>>>
        {
            private readonly IUnitOfWork _unitOfWork;
            private readonly IMapper _mapper;
            private readonly ILogger<GetCoverArtsByMangaQueryHandler> _logger;

            public GetCoverArtsByMangaQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<GetCoverArtsByMangaQueryHandler> logger)
            {
                _unitOfWork = unitOfWork;
                _mapper = mapper;
                _logger = logger;
            }

            public async Task<PagedResult<ResourceObject<CoverArtAttributesDto>>> Handle(GetCoverArtsByMangaQuery request, CancellationToken cancellationToken)
            {
                _logger.LogInformation("GetCoverArtsByMangaQueryHandler.Handle - Lấy cover arts cho MangaId: {MangaId}", request.MangaId);

                var mangaExists = await _unitOfWork.MangaRepository.ExistsAsync(request.MangaId);
                if (!mangaExists)
                {
                    _logger.LogWarning("Không tìm thấy Manga với ID: {MangaId} khi lấy cover arts.", request.MangaId);
                    return new PagedResult<ResourceObject<CoverArtAttributesDto>>(new List<ResourceObject<CoverArtAttributesDto>>(), 0, request.Offset, request.Limit);
                }

                Expression<Func<CoverArt, bool>> filter = ca => ca.MangaId == request.MangaId;
                Func<IQueryable<CoverArt>, IOrderedQueryable<CoverArt>> orderBy = q => q.OrderByDescending(ca => ca.CreatedAt);

                var pagedCoverArts = await _unitOfWork.CoverArtRepository.GetPagedAsync(
                    request.Offset,
                    request.Limit,
                    filter,
                    orderBy,
                    includeProperties: "Manga" 
                );

                var resourceObjects = new List<ResourceObject<CoverArtAttributesDto>>();
                foreach(var coverArt in pagedCoverArts.Items)
                {
                    var attributes = _mapper.Map<CoverArtAttributesDto>(coverArt);
                    var relationships = new List<RelationshipObject>();
                    if (coverArt.Manga != null) 
                    {
                        relationships.Add(new RelationshipObject { Id = coverArt.Manga.MangaId.ToString(), Type = "manga" });
                    }
                    resourceObjects.Add(new ResourceObject<CoverArtAttributesDto>
                    {
                        Id = coverArt.CoverId.ToString(),
                        Type = "cover_art",
                        Attributes = attributes,
                        Relationships = relationships.Any() ? relationships : null
                    });
                }
                return new PagedResult<ResourceObject<CoverArtAttributesDto>>(resourceObjects, pagedCoverArts.Total, request.Offset, request.Limit);
            }
        }
    }
    ```

### 3.7. TranslatedManga Queries

*   **`GetTranslatedMangaByIdQuery.cs`**

    ```csharp
    // Application/Features/TranslatedMangas/Queries/GetTranslatedMangaById/GetTranslatedMangaByIdQuery.cs
    using Application.Common.DTOs.TranslatedMangas;
    using Application.Common.Models;
    using MediatR;

    namespace Application.Features.TranslatedMangas.Queries.GetTranslatedMangaById
    {
        public class GetTranslatedMangaByIdQuery : IRequest<ResourceObject<TranslatedMangaAttributesDto>?>
        {
            public Guid TranslatedMangaId { get; set; }
        }
    }
    ```

*   **`GetTranslatedMangaByIdQueryHandler.cs`**

    ```csharp
    // Application/Features/TranslatedMangas/Queries/GetTranslatedMangaById/GetTranslatedMangaByIdQueryHandler.cs
    using Application.Common.DTOs.TranslatedMangas;
    using Application.Common.Models;
    using Application.Contracts.Persistence;
    using AutoMapper;
    using MediatR;
    using Microsoft.Extensions.Logging;
    using Domain.Entities;

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
    ```

*   **`GetTranslatedMangasByMangaQuery.cs`**

    ```csharp
    // Application/Features/TranslatedMangas/Queries/GetTranslatedMangasByManga/GetTranslatedMangasByMangaQuery.cs
    using Application.Common.DTOs;
    using Application.Common.DTOs.TranslatedMangas;
    using Application.Common.Models;
    using MediatR;
    using System; 

    namespace Application.Features.TranslatedMangas.Queries.GetTranslatedMangasByManga
    {
        public class GetTranslatedMangasByMangaQuery : IRequest<PagedResult<ResourceObject<TranslatedMangaAttributesDto>>>
        {
            public Guid MangaId { get; set; }
            public int Offset { get; set; } = 0;
            public int Limit { get; set; } = 20;
            public string OrderBy { get; set; } = "LanguageKey"; 
            public bool Ascending { get; set; } = true;
        }
    }
    ```

*   **`GetTranslatedMangasByMangaQueryHandler.cs`**

    ```csharp
    // Application/Features/TranslatedMangas/Queries/GetTranslatedMangasByManga/GetTranslatedMangasByMangaQueryHandler.cs
    using Application.Common.DTOs;
    using Application.Common.DTOs.TranslatedMangas;
    using Application.Common.Models;
    using Application.Contracts.Persistence;
    using AutoMapper;
    using Domain.Entities;
    using MediatR;
    using Microsoft.Extensions.Logging;
    using System.Linq.Expressions;
    using Application.Common.Extensions;

    namespace Application.Features.TranslatedMangas.Queries.GetTranslatedMangasByManga
    {
        public class GetTranslatedMangasByMangaQueryHandler : IRequestHandler<GetTranslatedMangasByMangaQuery, PagedResult<ResourceObject<TranslatedMangaAttributesDto>>>
        {
            private readonly IUnitOfWork _unitOfWork;
            private readonly IMapper _mapper;
            private readonly ILogger<GetTranslatedMangasByMangaQueryHandler> _logger;

            public GetTranslatedMangasByMangaQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<GetTranslatedMangasByMangaQueryHandler> logger)
            {
                _unitOfWork = unitOfWork;
                _mapper = mapper;
                _logger = logger;
            }

            public async Task<PagedResult<ResourceObject<TranslatedMangaAttributesDto>>> Handle(GetTranslatedMangasByMangaQuery request, CancellationToken cancellationToken)
            {
                _logger.LogInformation("GetTranslatedMangasByMangaQueryHandler.Handle - Lấy translated mangas cho MangaId: {MangaId}", request.MangaId);

                var mangaExists = await _unitOfWork.MangaRepository.ExistsAsync(request.MangaId);
                if (!mangaExists)
                {
                    _logger.LogWarning("Không tìm thấy Manga với ID: {MangaId} khi lấy translated mangas.", request.MangaId);
                    return new PagedResult<ResourceObject<TranslatedMangaAttributesDto>>(new List<ResourceObject<TranslatedMangaAttributesDto>>(), 0, request.Offset, request.Limit);
                }

                Expression<Func<TranslatedManga, bool>> filter = tm => tm.MangaId == request.MangaId;
                
                Func<IQueryable<TranslatedManga>, IOrderedQueryable<TranslatedManga>> orderBy;
                switch (request.OrderBy?.ToLowerInvariant())
                {
                    case "title":
                         orderBy = q => request.Ascending ? q.OrderBy(tm => tm.Title) : q.OrderByDescending(tm => tm.Title);
                        break;
                    case "languagekey":
                    default: 
                        orderBy = q => request.Ascending ? q.OrderBy(tm => tm.LanguageKey) : q.OrderByDescending(tm => tm.LanguageKey);
                        break;
                }

                var pagedTranslatedMangas = await _unitOfWork.TranslatedMangaRepository.GetPagedAsync(
                    request.Offset,
                    request.Limit,
                    filter,
                    orderBy,
                    includeProperties: "Manga" 
                );

                var resourceObjects = new List<ResourceObject<TranslatedMangaAttributesDto>>();
                foreach(var tm in pagedTranslatedMangas.Items)
                {
                    var attributes = _mapper.Map<TranslatedMangaAttributesDto>(tm);
                     var relationships = new List<RelationshipObject>();
                    if (tm.Manga != null)
                    {
                        relationships.Add(new RelationshipObject { Id = tm.Manga.MangaId.ToString(), Type = "manga" });
                    }
                    resourceObjects.Add(new ResourceObject<TranslatedMangaAttributesDto>
                    {
                        Id = tm.TranslatedMangaId.ToString(),
                        Type = "translated_manga",
                        Attributes = attributes,
                        Relationships = relationships.Any() ? relationships : null
                    });
                }
                return new PagedResult<ResourceObject<TranslatedMangaAttributesDto>>(resourceObjects, pagedTranslatedMangas.Total, request.Offset, request.Limit);
            }
        }
    }
    ```

## Bước 4: Cập Nhật Các Response DTOs Chung

Các lớp `ApiResponse.cs` và `ApiCollectionResponse.cs` đã được thiết kế để nhận kiểu `TData`. Khi các Query Handler trả về `ResourceObject<TAttributesDto>`, các lớp response này sẽ tự động hoạt động đúng.

*   `Application/Common/Responses/ApiResponse.cs`:
    *   `TData` sẽ trở thành `ResourceObject<TAttributesDto>`.
*   `Application/Common/Responses/ApiCollectionResponse.cs`:
    *   `TData` sẽ trở thành `ResourceObject<TAttributesDto>`, và `Data` sẽ là `List<ResourceObject<TAttributesDto>>`.

## Bước 5: Cập Nhật Các API Controllers

Thay đổi kiểu trả về của các Action Methods và các `[ProducesResponseType]` attributes để phản ánh cấu trúc mới.

### 1. `AuthorsController.cs`

```csharp
// File: MangaReaderDB/Controllers/AuthorsController.cs
using Application.Common.DTOs.Authors;
using Application.Common.Models; // For ResourceObject
using Application.Common.Responses;
using Application.Exceptions;
using Application.Features.Authors.Commands.CreateAuthor;
using Application.Features.Authors.Commands.DeleteAuthor;
using Application.Features.Authors.Commands.UpdateAuthor;
using Application.Features.Authors.Queries.GetAuthorById;
using Application.Features.Authors.Queries.GetAuthors;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MangaReaderDB.Controllers
{
    public class AuthorsController : BaseApiController
    {
        private readonly IValidator<CreateAuthorDto> _createAuthorDtoValidator;
        private readonly IValidator<UpdateAuthorDto> _updateAuthorDtoValidator;
        private readonly ILogger<AuthorsController> _logger;

        public AuthorsController(
            IValidator<CreateAuthorDto> createAuthorDtoValidator,
            IValidator<UpdateAuthorDto> updateAuthorDtoValidator,
            ILogger<AuthorsController> logger)
        {
            _createAuthorDtoValidator = createAuthorDtoValidator;
            _updateAuthorDtoValidator = updateAuthorDtoValidator;
            _logger = logger;
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<ResourceObject<AuthorAttributesDto>>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateAuthor([FromBody] CreateAuthorDto createAuthorDto)
        {
            var validationResult = await _createAuthorDtoValidator.ValidateAsync(createAuthorDto);
            if (!validationResult.IsValid)
            {
                throw new Application.Exceptions.ValidationException(validationResult.Errors);
            }

            var command = new CreateAuthorCommand 
            { 
                Name = createAuthorDto.Name, 
                Biography = createAuthorDto.Biography 
            };
            var authorId = await Mediator.Send(command);
            
            var authorResource = await Mediator.Send(new GetAuthorByIdQuery { AuthorId = authorId });
            if (authorResource == null)
            {
                 _logger.LogError($"FATAL: Author with ID {authorId} was not found after creation!");
                 return StatusCode(StatusCodes.Status500InternalServerError, 
                    new ApiErrorResponse(new ApiError(500, "Creation Error", "Failed to retrieve resource after creation.")));
            }
            return Created(nameof(GetAuthorById), new { id = authorId }, authorResource);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<ResourceObject<AuthorAttributesDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAuthorById(Guid id)
        {
            var query = new GetAuthorByIdQuery { AuthorId = id };
            var authorResource = await Mediator.Send(query);
            if (authorResource == null)
            {
                throw new NotFoundException(nameof(Domain.Entities.Author), id);
            }
            return Ok(authorResource);
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiCollectionResponse<ResourceObject<AuthorAttributesDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAuthors([FromQuery] GetAuthorsQuery query)
        {
            var result = await Mediator.Send(query); // QueryHandler trả về PagedResult<ResourceObject<AuthorAttributesDto>>
            return Ok(result);
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateAuthor(Guid id, [FromBody] UpdateAuthorDto updateAuthorDto)
        {
            var validationResult = await _updateAuthorDtoValidator.ValidateAsync(updateAuthorDto);
            if (!validationResult.IsValid)
            {
                throw new Application.Exceptions.ValidationException(validationResult.Errors);
            }

            var command = new UpdateAuthorCommand
            {
                AuthorId = id,
                Name = updateAuthorDto.Name,
                Biography = updateAuthorDto.Biography
            };
            await Mediator.Send(command); 
            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteAuthor(Guid id)
        {
            var command = new DeleteAuthorCommand { AuthorId = id };
            await Mediator.Send(command);
            return NoContent();
        }
    }
}
```

### 2. `MangasController.cs`

```csharp
// File: MangaReaderDB/Controllers/MangasController.cs
using Application.Common.DTOs.Mangas;
using Application.Common.Models; // For ResourceObject
using Application.Common.Responses;
using Application.Exceptions;
using Application.Features.Mangas.Commands.AddMangaAuthor;
using Application.Features.Mangas.Commands.AddMangaTag;
using Application.Features.Mangas.Commands.CreateManga;
using Application.Features.Mangas.Commands.DeleteManga;
using Application.Features.Mangas.Commands.RemoveMangaAuthor;
using Application.Features.Mangas.Commands.RemoveMangaTag;
using Application.Features.Mangas.Commands.UpdateManga;
using Application.Features.Mangas.Queries.GetMangaById;
using Application.Features.Mangas.Queries.GetMangas;
using Domain.Enums;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MangaReaderDB.Controllers
{
    public class MangasController : BaseApiController
    {
        private readonly IValidator<CreateMangaDto> _createMangaDtoValidator;
        private readonly IValidator<UpdateMangaDto> _updateMangaDtoValidator;
        private readonly ILogger<MangasController> _logger;

        public MangasController(
            IValidator<CreateMangaDto> createMangaDtoValidator,
            IValidator<UpdateMangaDto> updateMangaDtoValidator,
            ILogger<MangasController> logger)
        {
            _createMangaDtoValidator = createMangaDtoValidator;
            _updateMangaDtoValidator = updateMangaDtoValidator;
            _logger = logger;
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<ResourceObject<MangaAttributesDto>>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateManga([FromBody] CreateMangaDto createDto)
        {
            var validationResult = await _createMangaDtoValidator.ValidateAsync(createDto);
            if (!validationResult.IsValid)
            {
                throw new Application.Exceptions.ValidationException(validationResult.Errors);
            }

            var command = new CreateMangaCommand
            {
                Title = createDto.Title,
                OriginalLanguage = createDto.OriginalLanguage,
                PublicationDemographic = createDto.PublicationDemographic,
                Status = createDto.Status,
                Year = createDto.Year,
                ContentRating = createDto.ContentRating
            };
            var mangaId = await Mediator.Send(command);
            var mangaResource = await Mediator.Send(new GetMangaByIdQuery { MangaId = mangaId });

            if (mangaResource == null)
            {
                 _logger.LogError($"FATAL: Manga with ID {mangaId} was not found after creation!");
                 return StatusCode(StatusCodes.Status500InternalServerError, 
                    new ApiErrorResponse(new ApiError(500, "Creation Error", "Failed to retrieve resource after creation.")));
            }
            return Created(nameof(GetMangaById), new { id = mangaId }, mangaResource);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<ResourceObject<MangaAttributesDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetMangaById(Guid id)
        {
            var query = new GetMangaByIdQuery { MangaId = id };
            var mangaResource = await Mediator.Send(query);
            if (mangaResource == null)
            {
                throw new NotFoundException(nameof(Domain.Entities.Manga), id);
            }
            return Ok(mangaResource);
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiCollectionResponse<ResourceObject<MangaAttributesDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMangas([FromQuery] GetMangasQuery query)
        {
            var result = await Mediator.Send(query);
            return Ok(result);
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateManga(Guid id, [FromBody] UpdateMangaDto updateDto)
        {
            var validationResult = await _updateMangaDtoValidator.ValidateAsync(updateDto);
            if (!validationResult.IsValid)
            {
                throw new Application.Exceptions.ValidationException(validationResult.Errors);
            }

            var command = new UpdateMangaCommand
            {
                MangaId = id,
                Title = updateDto.Title,
                OriginalLanguage = updateDto.OriginalLanguage,
                PublicationDemographic = updateDto.PublicationDemographic,
                Status = updateDto.Status,
                Year = updateDto.Year,
                ContentRating = updateDto.ContentRating,
                IsLocked = updateDto.IsLocked
            };
            await Mediator.Send(command);
            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteManga(Guid id)
        {
            var command = new DeleteMangaCommand { MangaId = id };
            await Mediator.Send(command);
            return NoContent();
        }

        [HttpPost("{mangaId:guid}/tags")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddMangaTag(Guid mangaId, [FromBody] MangaTagInputDto input)
        {
            if (input.TagId == Guid.Empty) 
            {
                throw new Application.Exceptions.ValidationException(nameof(input.TagId), "TagId is required.");
            }
            var command = new AddMangaTagCommand { MangaId = mangaId, TagId = input.TagId };
            await Mediator.Send(command);
            return NoContent();
        }

        [HttpDelete("{mangaId:guid}/tags/{tagId:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RemoveMangaTag(Guid mangaId, Guid tagId)
        {
            var command = new RemoveMangaTagCommand { MangaId = mangaId, TagId = tagId };
            await Mediator.Send(command);
            return NoContent();
        }

        [HttpPost("{mangaId:guid}/authors")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddMangaAuthor(Guid mangaId, [FromBody] MangaAuthorInputDto input)
        {
            if (input.AuthorId == Guid.Empty)
            {
                 throw new Application.Exceptions.ValidationException(nameof(input.AuthorId), "AuthorId is required.");
            }
            if (!Enum.IsDefined(typeof(MangaStaffRole), input.Role))
            {
                throw new Application.Exceptions.ValidationException(nameof(input.Role), "Invalid Role.");
            }
            var command = new AddMangaAuthorCommand { MangaId = mangaId, AuthorId = input.AuthorId, Role = input.Role };
            await Mediator.Send(command);
            return NoContent();
        }

        [HttpDelete("{mangaId:guid}/authors/{authorId:guid}/role/{role}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RemoveMangaAuthor(Guid mangaId, Guid authorId, MangaStaffRole role)
        {
            var command = new RemoveMangaAuthorCommand { MangaId = mangaId, AuthorId = authorId, Role = role };
            await Mediator.Send(command);
            return NoContent();
        }
    }
}
```

### 3. `TagGroupsController.cs`

```csharp
// File: MangaReaderDB/Controllers/TagGroupsController.cs
using Application.Common.DTOs.TagGroups;
using Application.Common.Models; // For ResourceObject
using Application.Common.Responses;
using Application.Exceptions;
using Application.Features.TagGroups.Commands.CreateTagGroup;
using Application.Features.TagGroups.Commands.DeleteTagGroup;
using Application.Features.TagGroups.Commands.UpdateTagGroup;
using Application.Features.TagGroups.Queries.GetTagGroupById;
using Application.Features.TagGroups.Queries.GetTagGroups;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MangaReaderDB.Controllers
{
    public class TagGroupsController : BaseApiController
    {
        private readonly IValidator<CreateTagGroupDto> _createTagGroupDtoValidator;
        private readonly IValidator<UpdateTagGroupDto> _updateTagGroupDtoValidator;
        private readonly ILogger<TagGroupsController> _logger;

        public TagGroupsController(
            IValidator<CreateTagGroupDto> createTagGroupDtoValidator,
            IValidator<UpdateTagGroupDto> updateTagGroupDtoValidator,
            ILogger<TagGroupsController> logger)
        {
            _createTagGroupDtoValidator = createTagGroupDtoValidator;
            _updateTagGroupDtoValidator = updateTagGroupDtoValidator;
            _logger = logger;
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<ResourceObject<TagGroupAttributesDto>>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateTagGroup([FromBody] CreateTagGroupDto createDto)
        {
            var validationResult = await _createTagGroupDtoValidator.ValidateAsync(createDto);
            if (!validationResult.IsValid)
            {
                throw new Application.Exceptions.ValidationException(validationResult.Errors);
            }

            var command = new CreateTagGroupCommand { Name = createDto.Name };
            var tagGroupId = await Mediator.Send(command);
            var tagGroupResource = await Mediator.Send(new GetTagGroupByIdQuery { TagGroupId = tagGroupId });

            if (tagGroupResource == null)
            {
                 _logger.LogError($"FATAL: TagGroup with ID {tagGroupId} was not found after creation!");
                 return StatusCode(StatusCodes.Status500InternalServerError, 
                    new ApiErrorResponse(new ApiError(500, "Creation Error", "Failed to retrieve resource after creation.")));
            }
            return Created(nameof(GetTagGroupById), new { id = tagGroupId }, tagGroupResource);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<ResourceObject<TagGroupAttributesDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetTagGroupById(Guid id)
        {
            var query = new GetTagGroupByIdQuery { TagGroupId = id };
            var tagGroupResource = await Mediator.Send(query);
            if (tagGroupResource == null)
            {
                throw new NotFoundException(nameof(Domain.Entities.TagGroup), id);
            }
            return Ok(tagGroupResource);
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiCollectionResponse<ResourceObject<TagGroupAttributesDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTagGroups([FromQuery] GetTagGroupsQuery query)
        {
            var result = await Mediator.Send(query);
            return Ok(result);
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateTagGroup(Guid id, [FromBody] UpdateTagGroupDto updateDto)
        {
            var validationResult = await _updateTagGroupDtoValidator.ValidateAsync(updateDto);
            if (!validationResult.IsValid)
            {
                throw new Application.Exceptions.ValidationException(validationResult.Errors);
            }

            var command = new UpdateTagGroupCommand { TagGroupId = id, Name = updateDto.Name };
            await Mediator.Send(command);
            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)] 
        public async Task<IActionResult> DeleteTagGroup(Guid id)
        {
            var command = new DeleteTagGroupCommand { TagGroupId = id };
            await Mediator.Send(command);
            return NoContent();
        }
    }
}
```

### 4. `TagsController.cs`

```csharp
// File: MangaReaderDB/Controllers/TagsController.cs
using Application.Common.DTOs.Tags;
using Application.Common.Models; // For ResourceObject
using Application.Common.Responses;
using Application.Exceptions;
using Application.Features.Tags.Commands.CreateTag;
using Application.Features.Tags.Commands.DeleteTag;
using Application.Features.Tags.Commands.UpdateTag;
using Application.Features.Tags.Queries.GetTagById;
using Application.Features.Tags.Queries.GetTags;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MangaReaderDB.Controllers
{
    public class TagsController : BaseApiController
    {
        private readonly IValidator<CreateTagDto> _createTagDtoValidator;
        private readonly IValidator<UpdateTagDto> _updateTagDtoValidator;
        private readonly ILogger<TagsController> _logger;

        public TagsController(
            IValidator<CreateTagDto> createTagDtoValidator,
            IValidator<UpdateTagDto> updateTagDtoValidator,
            ILogger<TagsController> logger)
        {
            _createTagDtoValidator = createTagDtoValidator;
            _updateTagDtoValidator = updateTagDtoValidator;
            _logger = logger;
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<ResourceObject<TagAttributesDto>>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)] 
        public async Task<IActionResult> CreateTag([FromBody] CreateTagDto createDto)
        {
            var validationResult = await _createTagDtoValidator.ValidateAsync(createDto);
            if (!validationResult.IsValid)
            {
                throw new Application.Exceptions.ValidationException(validationResult.Errors);
            }

            var command = new CreateTagCommand { Name = createDto.Name, TagGroupId = createDto.TagGroupId };
            var tagId = await Mediator.Send(command);
            var tagResource = await Mediator.Send(new GetTagByIdQuery { TagId = tagId });
            
            if (tagResource == null)
            {
                _logger.LogError($"FATAL: Tag with ID {tagId} was not found after creation!");
                 return StatusCode(StatusCodes.Status500InternalServerError, 
                    new ApiErrorResponse(new ApiError(500, "Creation Error", "Failed to retrieve resource after creation.")));
            }
            return Created(nameof(GetTagById), new { id = tagId }, tagResource);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<ResourceObject<TagAttributesDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetTagById(Guid id)
        {
            var query = new GetTagByIdQuery { TagId = id };
            var tagResource = await Mediator.Send(query);
            if (tagResource == null)
            {
                throw new NotFoundException(nameof(Domain.Entities.Tag), id);
            }
            return Ok(tagResource);
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiCollectionResponse<ResourceObject<TagAttributesDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTags([FromQuery] GetTagsQuery query)
        {
            var result = await Mediator.Send(query);
            return Ok(result);
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateTag(Guid id, [FromBody] UpdateTagDto updateDto)
        {
            var validationResult = await _updateTagDtoValidator.ValidateAsync(updateDto);
            if (!validationResult.IsValid)
            {
                throw new Application.Exceptions.ValidationException(validationResult.Errors);
            }

            var command = new UpdateTagCommand { TagId = id, Name = updateDto.Name, TagGroupId = updateDto.TagGroupId };
            await Mediator.Send(command);
            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteTag(Guid id)
        {
            var command = new DeleteTagCommand { TagId = id };
            await Mediator.Send(command);
            return NoContent();
        }
    }
}
```

### 5. `TranslatedMangasController.cs`

```csharp
// File: MangaReaderDB/Controllers/TranslatedMangasController.cs
using Application.Common.DTOs.TranslatedMangas;
using Application.Common.Models; // For ResourceObject
using Application.Common.Responses;
using Application.Exceptions;
using Application.Features.TranslatedMangas.Commands.CreateTranslatedManga;
using Application.Features.TranslatedMangas.Commands.DeleteTranslatedManga;
using Application.Features.TranslatedMangas.Commands.UpdateTranslatedManga;
using Application.Features.TranslatedMangas.Queries.GetTranslatedMangaById;
using Application.Features.TranslatedMangas.Queries.GetTranslatedMangasByManga;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MangaReaderDB.Controllers
{
    public class TranslatedMangasController : BaseApiController
    {
        private readonly IValidator<CreateTranslatedMangaDto> _createDtoValidator;
        private readonly IValidator<UpdateTranslatedMangaDto> _updateDtoValidator;
        private readonly ILogger<TranslatedMangasController> _logger;

        public TranslatedMangasController(
            IValidator<CreateTranslatedMangaDto> createDtoValidator,
            IValidator<UpdateTranslatedMangaDto> updateDtoValidator,
            ILogger<TranslatedMangasController> logger)
        {
            _createDtoValidator = createDtoValidator;
            _updateDtoValidator = updateDtoValidator;
            _logger = logger;
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<ResourceObject<TranslatedMangaAttributesDto>>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)] 
        public async Task<IActionResult> CreateTranslatedManga([FromBody] CreateTranslatedMangaDto createDto)
        {
            var validationResult = await _createDtoValidator.ValidateAsync(createDto);
            if (!validationResult.IsValid)
            {
                throw new Application.Exceptions.ValidationException(validationResult.Errors);
            }

            var command = new CreateTranslatedMangaCommand
            {
                MangaId = createDto.MangaId,
                LanguageKey = createDto.LanguageKey,
                Title = createDto.Title,
                Description = createDto.Description
            };
            var translatedMangaId = await Mediator.Send(command);
            var translatedMangaResource = await Mediator.Send(new GetTranslatedMangaByIdQuery { TranslatedMangaId = translatedMangaId });
            
            if(translatedMangaResource == null)
            {
                _logger.LogError($"FATAL: TranslatedManga with ID {translatedMangaId} was not found after creation!");
                 return StatusCode(StatusCodes.Status500InternalServerError, 
                    new ApiErrorResponse(new ApiError(500, "Creation Error", "Failed to retrieve resource after creation.")));
            }
            return Created(nameof(GetTranslatedMangaById), new { id = translatedMangaId }, translatedMangaResource);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<ResourceObject<TranslatedMangaAttributesDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetTranslatedMangaById(Guid id)
        {
            var query = new GetTranslatedMangaByIdQuery { TranslatedMangaId = id };
            var translatedMangaResource = await Mediator.Send(query);
            if (translatedMangaResource == null)
            {
                 throw new NotFoundException(nameof(Domain.Entities.TranslatedManga), id);
            }
            return Ok(translatedMangaResource);
        }

        [HttpGet("/mangas/{mangaId:guid}/translations")] 
        [ProducesResponseType(typeof(ApiCollectionResponse<ResourceObject<TranslatedMangaAttributesDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTranslatedMangasByManga(Guid mangaId, [FromQuery] GetTranslatedMangasByMangaQuery query)
        {
            query.MangaId = mangaId;
            var result = await Mediator.Send(query);
            return Ok(result);
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateTranslatedManga(Guid id, [FromBody] UpdateTranslatedMangaDto updateDto)
        {
            var validationResult = await _updateDtoValidator.ValidateAsync(updateDto);
            if (!validationResult.IsValid)
            {
                throw new Application.Exceptions.ValidationException(validationResult.Errors);
            }

            var command = new UpdateTranslatedMangaCommand
            {
                TranslatedMangaId = id,
                LanguageKey = updateDto.LanguageKey,
                Title = updateDto.Title,
                Description = updateDto.Description
            };
            await Mediator.Send(command);
            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteTranslatedManga(Guid id)
        {
            var command = new DeleteTranslatedMangaCommand { TranslatedMangaId = id };
            await Mediator.Send(command);
            return NoContent();
        }
    }
}
```

### 6. `CoverArtsController.cs`

```csharp
// File: MangaReaderDB/Controllers/CoverArtsController.cs
using Application.Common.DTOs.CoverArts;
using Application.Common.Models; // For ResourceObject
using Application.Common.Responses;
using Application.Exceptions;
using Application.Features.CoverArts.Commands.DeleteCoverArt;
using Application.Features.CoverArts.Commands.UploadCoverArtImage;
using Application.Features.CoverArts.Queries.GetCoverArtById;
using Application.Features.CoverArts.Queries.GetCoverArtsByManga;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MangaReaderDB.Controllers
{
    public class CoverArtsController : BaseApiController
    {
        private readonly IValidator<CreateCoverArtDto> _createCoverArtDtoValidator;
        private readonly ILogger<CoverArtsController> _logger;

        public CoverArtsController(
            IValidator<CreateCoverArtDto> createCoverArtDtoValidator,
            ILogger<CoverArtsController> logger)
        {
            _createCoverArtDtoValidator = createCoverArtDtoValidator;
            _logger = logger;
        }

        [HttpPost("/mangas/{mangaId:guid}/covers")]
        [ProducesResponseType(typeof(ApiResponse<ResourceObject<CoverArtAttributesDto>>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UploadCoverArtImage(Guid mangaId, IFormFile file, [FromForm] string? volume, [FromForm] string? description)
        {
            if (file == null || file.Length == 0)
            {
                throw new Application.Exceptions.ValidationException("file", "File is required.");
            }
            if (file.Length > 5 * 1024 * 1024) 
            {
                throw new Application.Exceptions.ValidationException("file", "File size cannot exceed 5MB.");
            }
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(ext) || !allowedExtensions.Contains(ext))
            {
                 throw new Application.Exceptions.ValidationException("file", "Invalid file type. Allowed types are: " + string.Join(", ", allowedExtensions));
            }

            var metadataDto = new CreateCoverArtDto { MangaId = mangaId, Volume = volume, Description = description };
            var validationResult = await _createCoverArtDtoValidator.ValidateAsync(metadataDto);
            if (!validationResult.IsValid)
            {
                throw new Application.Exceptions.ValidationException(validationResult.Errors);
            }

            using var stream = file.OpenReadStream();
            var command = new UploadCoverArtImageCommand
            {
                MangaId = mangaId,
                Volume = volume,
                Description = description,
                ImageStream = stream,
                OriginalFileName = file.FileName,
                ContentType = file.ContentType
            };

            var coverId = await Mediator.Send(command);
            var coverArtResource = await Mediator.Send(new GetCoverArtByIdQuery { CoverId = coverId });

            if (coverArtResource == null)
            {
                _logger.LogError($"FATAL: CoverArt with ID {coverId} was not found after creation!");
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new ApiErrorResponse(new ApiError(500, "Creation Error", "Failed to retrieve resource after creation.")));
            }
            return Created(nameof(GetCoverArtById), new { id = coverId }, coverArtResource);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<ResourceObject<CoverArtAttributesDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCoverArtById(Guid id)
        {
            var query = new GetCoverArtByIdQuery { CoverId = id };
            var coverArtResource = await Mediator.Send(query);
            if (coverArtResource == null)
            {
                 throw new NotFoundException(nameof(Domain.Entities.CoverArt), id);
            }
            return Ok(coverArtResource);
        }

        [HttpGet("/mangas/{mangaId:guid}/covers")]
        [ProducesResponseType(typeof(ApiCollectionResponse<ResourceObject<CoverArtAttributesDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCoverArtsByManga(Guid mangaId, [FromQuery] GetCoverArtsByMangaQuery query)
        {
            query.MangaId = mangaId;
            var result = await Mediator.Send(query);
            return Ok(result);
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteCoverArt(Guid id)
        {
            var command = new DeleteCoverArtCommand { CoverId = id };
            await Mediator.Send(command);
            return NoContent();
        }
    }
}
```

### 7. `ChaptersController.cs` và `ChapterPagesController.cs`

```csharp
// File: MangaReaderDB/Controllers/ChaptersController.cs
using Application.Common.DTOs.Chapters;
using Application.Common.Models; // For ResourceObject
using Application.Common.Responses;
using Application.Exceptions;
using Application.Features.Chapters.Commands.CreateChapter;
using Application.Features.Chapters.Commands.CreateChapterPageEntry;
using Application.Features.Chapters.Commands.DeleteChapter;
using Application.Features.Chapters.Commands.DeleteChapterPage;
using Application.Features.Chapters.Commands.UpdateChapter;
using Application.Features.Chapters.Commands.UpdateChapterPageDetails;
using Application.Features.Chapters.Commands.UploadChapterPageImage;
using Application.Features.Chapters.Queries.GetChapterById;
using Application.Features.Chapters.Queries.GetChapterPages;
using Application.Features.Chapters.Queries.GetChaptersByTranslatedManga;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MangaReaderDB.Controllers
{
    public class ChaptersController : BaseApiController
    {
        private readonly IValidator<CreateChapterDto> _createChapterDtoValidator;
        private readonly IValidator<UpdateChapterDto> _updateChapterDtoValidator;
        private readonly IValidator<CreateChapterPageDto> _createChapterPageDtoValidator;
        private readonly ILogger<ChaptersController> _logger;

        public ChaptersController(
            IValidator<CreateChapterDto> createChapterDtoValidator,
            IValidator<UpdateChapterDto> updateChapterDtoValidator,
            IValidator<CreateChapterPageDto> createChapterPageDtoValidator,
            ILogger<ChaptersController> logger)
        {
            _createChapterDtoValidator = createChapterDtoValidator;
            _updateChapterDtoValidator = updateChapterDtoValidator;
            _createChapterPageDtoValidator = createChapterPageDtoValidator;
            _logger = logger;
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<ResourceObject<ChapterAttributesDto>>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreateChapter([FromBody] CreateChapterDto createDto)
        {
            var validationResult = await _createChapterDtoValidator.ValidateAsync(createDto);
            if (!validationResult.IsValid)
            {
                throw new Application.Exceptions.ValidationException(validationResult.Errors);
            }
            
            // Giả sử UploadedByUserId được lấy từ context người dùng đã xác thực
            // Tạm thời để trống hoặc gán một giá trị mặc định nếu cần thiết cho command.
            // int currentUserId = ... ; // Lấy từ HttpContext.User hoặc service
            var command = new CreateChapterCommand
            {
                TranslatedMangaId = createDto.TranslatedMangaId,
                UploadedByUserId = createDto.UploadedByUserId, // Cần thay thế bằng UserId từ context
                Volume = createDto.Volume,
                ChapterNumber = createDto.ChapterNumber,
                Title = createDto.Title,
                PublishAt = createDto.PublishAt,
                ReadableAt = createDto.ReadableAt
            };
            var chapterId = await Mediator.Send(command);
            var chapterResource = await Mediator.Send(new GetChapterByIdQuery { ChapterId = chapterId });

            if (chapterResource == null)
            {
                _logger.LogError($"FATAL: Chapter with ID {chapterId} was not found after creation!");
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new ApiErrorResponse(new ApiError(500, "Creation Error", "Failed to retrieve resource after creation.")));
            }
            return Created(nameof(GetChapterById), new { id = chapterId }, chapterResource);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<ResourceObject<ChapterAttributesDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetChapterById(Guid id)
        {
            var query = new GetChapterByIdQuery { ChapterId = id };
            var chapterResource = await Mediator.Send(query);
            if (chapterResource == null)
            {
                throw new NotFoundException(nameof(Domain.Entities.Chapter), id);
            }
            return Ok(chapterResource);
        }

        [HttpGet("/translatedmangas/{translatedMangaId:guid}/chapters")] 
        [ProducesResponseType(typeof(ApiCollectionResponse<ResourceObject<ChapterAttributesDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetChaptersByTranslatedManga(Guid translatedMangaId, [FromQuery] GetChaptersByTranslatedMangaQuery query)
        {
            query.TranslatedMangaId = translatedMangaId;
            var result = await Mediator.Send(query);
            return Ok(result);
        }
        
        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateChapter(Guid id, [FromBody] UpdateChapterDto updateDto)
        {
            var validationResult = await _updateChapterDtoValidator.ValidateAsync(updateDto);
            if (!validationResult.IsValid)
            {
                throw new Application.Exceptions.ValidationException(validationResult.Errors);
            }

            var command = new UpdateChapterCommand
            {
                ChapterId = id,
                Volume = updateDto.Volume,
                ChapterNumber = updateDto.ChapterNumber,
                Title = updateDto.Title,
                PublishAt = updateDto.PublishAt,
                ReadableAt = updateDto.ReadableAt
            };
            await Mediator.Send(command);
            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteChapter(Guid id)
        {
            var command = new DeleteChapterCommand { ChapterId = id };
            await Mediator.Send(command);
            return NoContent();
        }

        [HttpPost("{chapterId:guid}/pages/entry")] 
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status201Created)] // Payload là { "pageId": "guid" }
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreateChapterPageEntry(Guid chapterId, [FromBody] CreateChapterPageDto createPageDto)
        {
            var validationResult = await _createChapterPageDtoValidator.ValidateAsync(createPageDto);
            if (!validationResult.IsValid)
            {
                 throw new Application.Exceptions.ValidationException(validationResult.Errors);
            }
            
            var command = new CreateChapterPageEntryCommand 
            { 
                ChapterId = chapterId, 
                PageNumber = createPageDto.PageNumber
            };
            var pageId = await Mediator.Send(command);

            var responsePayload = new { PageId = pageId };
            
            // Trả về 201 Created với Location header trỏ đến action upload image cho page này
            // Action "UploadChapterPageImage" nằm trong "ChapterPagesController"
            return CreatedAtAction(
                actionName: nameof(ChapterPagesController.UploadChapterPageImage), 
                controllerName: "ChapterPages", 
                routeValues: new { pageId = pageId }, 
                value: new ApiResponse<object>(responsePayload)
            );
        }

        [HttpGet("{chapterId:guid}/pages")]
        // Theo PlanDTO, ChapterPageDto không được bọc trong ResourceObject cho list này.
        [ProducesResponseType(typeof(ApiCollectionResponse<ChapterPageDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetChapterPages(Guid chapterId, [FromQuery] GetChapterPagesQuery query)
        {
            query.ChapterId = chapterId;
            var result = await Mediator.Send(query); // Handler trả về PagedResult<ChapterPageDto>
            return Ok(result); // BaseApiController sẽ bọc trong ApiCollectionResponse<ChapterPageDto>
        }
    }

    // Tách riêng ChapterPagesController để quản lý các endpoint liên quan đến ChapterPage
    [Route("chapterpages")]
    public class ChapterPagesController : BaseApiController
    {
        private readonly IValidator<UpdateChapterPageDto> _updateChapterPageDtoValidator;
        private readonly ILogger<ChapterPagesController> _logger;

        public ChapterPagesController(
            IValidator<UpdateChapterPageDto> updateChapterPageDtoValidator,
            ILogger<ChapterPagesController> logger)
        {
            _updateChapterPageDtoValidator = updateChapterPageDtoValidator;
            _logger = logger;
        }

        [HttpPost("{pageId:guid}/image")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)] // Payload là { "publicId": "string" }
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)] // Cho lỗi file
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)] // Cho pageId
        public async Task<IActionResult> UploadChapterPageImage(Guid pageId, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                throw new Application.Exceptions.ValidationException("file", "File is required.");
            }
            if (file.Length > 5 * 1024 * 1024) 
            {
                 throw new Application.Exceptions.ValidationException("file", "File size cannot exceed 5MB.");
            }
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(ext) || !allowedExtensions.Contains(ext))
            {
                throw new Application.Exceptions.ValidationException("file", "Invalid file type. Allowed types are: " + string.Join(", ", allowedExtensions));
            }

            using var stream = file.OpenReadStream();
            var command = new UploadChapterPageImageCommand
            {
                ChapterPageId = pageId,
                ImageStream = stream,
                OriginalFileName = file.FileName,
                ContentType = file.ContentType
            };
            var publicId = await Mediator.Send(command); // Handler sẽ throw NotFoundException nếu pageId không tồn tại
            
            var responsePayload = new { PublicId = publicId };
            return Ok(responsePayload); // BaseApiController.Ok sẽ bọc trong ApiResponse<object>
        }

        [HttpPut("{pageId:guid}/details")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateChapterPageDetails(Guid pageId, [FromBody] UpdateChapterPageDto updateDto)
        {
            var validationResult = await _updateChapterPageDtoValidator.ValidateAsync(updateDto);
            if (!validationResult.IsValid)
            {
                throw new Application.Exceptions.ValidationException(validationResult.Errors);
            }

            var command = new UpdateChapterPageDetailsCommand
            {
                PageId = pageId,
                PageNumber = updateDto.PageNumber
            };
            await Mediator.Send(command);
            return NoContent();
        }

        [HttpDelete("{pageId:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteChapterPage(Guid pageId)
        {
            var command = new DeleteChapterPageCommand { PageId = pageId };
            await Mediator.Send(command);
            return NoContent();
        }
    }
}
```

## Bước 6: Cập Nhật Tài Liệu API

Cập nhật `docs/api_conventions.md` để mô tả chi tiết cấu trúc response JSON mới, bao gồm:

*   Cấu trúc chung của `ResourceObject`.
*   Ý nghĩa của `id`, `type`, `attributes`, `relationships`.
*   Làm rõ cách `type` trong `relationships` được xác định (ví dụ: "author", "artist", "cover_art", "tag", "user", "chapter", "manga").
*   Cung cấp các ví dụ JSON minh họa cho các endpoint GET khác nhau.

**Ví dụ cập nhật `api_conventions.md` (một phần):**

```markdown
<!-- docs/api_conventions.md -->
// ... (các phần khác của file) ...

## 6. Cấu Trúc Response Body (JSON)

Tất cả các response thành công (200 OK, 201 Created) trả về dữ liệu sẽ tuân theo cấu trúc sau:

### 6.1. Response Cho Một Đối Tượng Đơn Lẻ

```json
{
  "result": "ok", // Luôn là "ok" cho response thành công
  "response": "entity", // Loại response, ví dụ "entity" hoặc "collection"
  "data": {
    "id": "string (GUID)",
    "type": "string (loại của resource, ví dụ: 'manga', 'author')",
    "attributes": {
      // Các thuộc tính cụ thể của resource (trừ id và relationships)
      // Ví dụ cho MangaAttributesDto:
      // "title": "One Piece",
      // "originalLanguage": "ja",
      // "status": "Ongoing",
      // ...
    },
    "relationships": [
      {
        "id": "string (GUID của entity liên quan)",
        "type": "string (loại của MỐI QUAN HỆ hoặc VAI TRÒ, ví dụ: 'author', 'artist', 'tag', 'cover_art')"
      }
      // ... các relationships khác ...
    ]
  }
}
```

*   **`data.id`**: ID của tài nguyên chính (luôn là GUID dưới dạng chuỗi).
*   **`data.type`**: Loại của tài nguyên chính (ví dụ: `"manga"`, `"author"`, `"tag"`, `"chapter"`, `"cover_art"`). Được viết bằng snake_case, số ít.
*   **`data.attributes`**: Một object chứa tất cả các thuộc tính của tài nguyên (tương ứng với `...AttributesDto`).
*   **`data.relationships`**: (Tùy chọn, có thể không có nếu không có mối quan hệ) Một mảng các đối tượng `RelationshipObject`.
    *   **`id`**: ID của thực thể liên quan.
    *   **`type`**: Mô tả vai trò hoặc bản chất của mối quan hệ đó đối với thực thể gốc.
        *   Ví dụ, đối với một Manga:
            *   Relationship tới Author với vai trò `Author`: `{ "id": "author-guid", "type": "author" }`
            *   Relationship tới Author với vai trò `Artist`: `{ "id": "artist-guid", "type": "artist" }`
            *   Relationship tới Tag: `{ "id": "tag-guid", "type": "tag" }`
            *   Relationship tới CoverArt chính: `{ "id": "coverart-guid", "type": "cover_art" }`
        *   Đối với một Chapter:
            *   Relationship tới User (uploader): `{ "id": "user-id", "type": "user" }` (hoặc `"uploader"`)
            *   Relationship tới Manga (manga gốc của chapter): `{ "id": "manga-guid", "type": "manga" }`

### 6.2. Response Cho Danh Sách Đối Tượng (Collection)

```json
{
  "result": "ok",
  "response": "collection",
  "data": [
    {
      "id": "string (GUID)",
      "type": "string (loại của resource)",
      "attributes": { /* ... */ },
      "relationships": [ /* ... */ ]
    }
    // ... các resource objects khác ...
  ],
  "limit": 10,
  "offset": 0,
  "total": 100
}
```
*   Trường `data` là một mảng các `ResourceObject` như mô tả ở mục 6.1.
*   `limit`, `offset`, `total` là các thông tin phân trang.

// ... (các phần khác của file) ...
```