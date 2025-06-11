# TODO: Cập nhật API Endpoint cho Manga

Tài liệu này mô tả các bước cần thiết để cập nhật các API endpoint liên quan đến Manga theo các yêu cầu mới.

## Mục lục

1.  [Chuẩn bị: Cập nhật Model và Query](#muc-0-chuan-bi)
    1.1. [Cập nhật `Application/Common/Models/RelationshipObject.cs`](#buoc-01-cap-nhat-relationshipobject)
    1.2. [Cập nhật `Application/Common/DTOs/Mangas/MangaAttributesDto.cs`](#buoc-02-cap-nhat-mangaattributesdto)
    1.3. [Cập nhật `Application/Features/Mangas/Queries/GetMangas/GetMangasQuery.cs`](#buoc-03-cap-nhat-getmangasquery)
    1.4. [Cập nhật `Application/Features/Mangas/Queries/GetMangaById/GetMangaByIdQuery.cs`](#buoc-04-cap-nhat-getmangabyidquery)
2.  [Chức năng 1: Include Cover cho API danh sách manga](#chuc-nang-1-include-cover-cho-api-danh-sach-manga)
    2.1. [Cập nhật `Application/Features/Mangas/Queries/GetMangas/GetMangasQueryHandler.cs`](#buoc-11-cap-nhat-getmangasqueryhandler-cho-cover)
3.  [Chức năng 2: Thông tin Tag luôn đi kèm trong manga/chi tiết manga](#chuc-nang-2-thong-tin-tag-luon-di-kem)
    3.1. [Cập nhật `Application/Features/Mangas/Queries/GetMangaById/GetMangaByIdQueryHandler.cs` (cho Tags)](#buoc-21-cap-nhat-getmangabyidqueryhandler-cho-tags)
    3.2. [Cập nhật `Application/Features/Mangas/Queries/GetMangas/GetMangasQueryHandler.cs` (cho Tags)](#buoc-22-cap-nhat-getmangasqueryhandler-cho-tags)
4.  [Chức năng 3: Include Artist/Author cho danh sách/chi tiết manga](#chuc-nang-3-include-artistauthor)
    4.1. [Cập nhật `Application/Features/Mangas/Queries/GetMangaById/GetMangaByIdQueryHandler.cs` (cho Author/Artist)](#buoc-31-cap-nhat-getmangabyidqueryhandler-cho-authorartist)
    4.2. [Cập nhật `Application/Features/Mangas/Queries/GetMangas/GetMangasQueryHandler.cs` (cho Author/Artist)](#buoc-32-cap-nhat-getmangasqueryhandler-cho-authorartist)
5.  [Cập nhật tài liệu API `docs/api_conventions.md`](#muc-4-cap-nhat-tai-lieu-api)

---

## Mục 0: Chuẩn bị

Các bước này cập nhật các model cơ bản để hỗ trợ các chức năng mới.

### Bước 0.1: Cập nhật `Application/Common/Models/RelationshipObject.cs`

Mở rộng `RelationshipObject` để có thể chứa `attributes` của entity liên quan, tương tự như cách Mangadex API xử lý.

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
        public string Type { get; set; } = string.Empty; // Loại của mối quan hệ hoặc vai trò

        [JsonPropertyOrder(3)]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public object? Attributes { get; set; } // Thuộc tính chi tiết của thực thể liên quan (nếu được include)
    }
}
```

### Bước 0.2: Cập nhật `Application/Common/DTOs/Mangas/MangaAttributesDto.cs`

Thêm trường `Tags` để chứa danh sách thông tin chi tiết của các tag.

```csharp
// Application/Common/DTOs/Mangas/MangaAttributesDto.cs
using Application.Common.DTOs.Tags;
using Application.Common.Models; // Cần cho ResourceObject
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

        // Thêm trường Tags để chứa thông tin chi tiết của các Tag
        public List<ResourceObject<TagAttributesDto>> Tags { get; set; } = new List<ResourceObject<TagAttributesDto>>();
    }
}
```

### Bước 0.3: Cập nhật `Application/Features/Mangas/Queries/GetMangas/GetMangasQuery.cs`

Thêm tham số `Includes` vào query.

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
        public List<Guid>? TagIdsFilter { get; set; }
        public List<Guid>? AuthorIdsFilter { get; set; }
        
        public List<string>? IncludedTags { get; set; }
        public string? IncludedTagsMode { get; set; } 
        public List<Guid>? ExcludedTags { get; set; }
        public string? ExcludedTagsMode { get; set; }

        public string OrderBy { get; set; } = "UpdatedAt";
        public bool Ascending { get; set; } = false;

        // Thêm tham số Includes
        public List<string>? Includes { get; set; } // Ví dụ: ["cover_art", "author", "artist"]
    }
}
```

### Bước 0.4: Cập nhật `Application/Features/Mangas/Queries/GetMangaById/GetMangaByIdQuery.cs`

Thêm tham số `Includes` vào query.

```csharp
// Application/Features/Mangas/Queries/GetMangaById/GetMangaByIdQuery.cs
using Application.Common.DTOs.Mangas;
using Application.Common.Models;
using MediatR;
using System.Collections.Generic; // Thêm using này

namespace Application.Features.Mangas.Queries.GetMangaById
{
    public class GetMangaByIdQuery : IRequest<ResourceObject<MangaAttributesDto>?>
    {
        public Guid MangaId { get; set; }
        
        // Thêm tham số Includes
        public List<string>? Includes { get; set; } // Ví dụ: ["author", "artist"]
    }
}
```

---

## Chức năng 1: Include Cover cho API danh sách manga

Cho phép client yêu cầu thông tin ảnh bìa (public ID) khi lấy danh sách manga.

### Bước 1.1: Cập nhật `Application/Features/Mangas/Queries/GetMangas/GetMangasQueryHandler.cs`

Xử lý tham số `Includes` để thêm thông tin cover vào `relationships`.

```csharp
// Application/Features/Mangas/Queries/GetMangas/GetMangasQueryHandler.cs
using Application.Common.DTOs;
using Application.Common.DTOs.Mangas;
using Application.Common.DTOs.Authors; // Cần cho AuthorAttributesDto
using Application.Common.DTOs.Tags;    // Cần cho TagAttributesDto
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
            if (request.TagIdsFilter != null && request.TagIdsFilter.Any())
            {
                predicate = predicate.And(m => m.MangaTags.Any(mt => request.TagIdsFilter.Contains(mt.TagId)));
            }
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
            bool includeAuthor = request.Includes?.Contains("author", StringComparer.OrdinalIgnoreCase) ?? false;
            bool includeArtist = request.Includes?.Contains("artist", StringComparer.OrdinalIgnoreCase) ?? false;

            foreach (var manga in pagedMangas.Items)
            {
                var mangaAttributes = _mapper.Map<MangaAttributesDto>(manga);
                
                // Xử lý Tags (Yêu cầu 2 - Luôn đi kèm)
                mangaAttributes.Tags = manga.MangaTags
                    .Select(mt => new ResourceObject<TagAttributesDto>
                    {
                        Id = mt.Tag.TagId.ToString(),
                        Type = "tag",
                        Attributes = _mapper.Map<TagAttributesDto>(mt.Tag),
                        Relationships = new List<RelationshipObject> 
                        {
                            new RelationshipObject { Id = mt.Tag.TagGroupId.ToString(), Type = "tag_group" }
                        }
                    })
                    .ToList();

                var relationships = new List<RelationshipObject>();

                // Xử lý Authors/Artists (Yêu cầu 3 - Include)
                if (manga.MangaAuthors != null)
                {
                    foreach (var mangaAuthor in manga.MangaAuthors)
                    {
                        if (mangaAuthor.Author != null) 
                        {
                            var relationshipType = mangaAuthor.Role == MangaStaffRole.Author ? "author" : "artist";
                            bool shouldIncludeAttributes = (relationshipType == "author" && includeAuthor) || (relationshipType == "artist" && includeArtist);
                            
                            relationships.Add(new RelationshipObject
                            {
                                Id = mangaAuthor.Author.AuthorId.ToString(),
                                Type = relationshipType,
                                Attributes = shouldIncludeAttributes ? _mapper.Map<AuthorAttributesDto>(mangaAuthor.Author) : null
                            });
                        }
                    }
                }
                
                // Xử lý CoverArt (Yêu cầu 1 - Include)
                if (includeCoverArt)
                {
                    var primaryCover = manga.CoverArts?.OrderByDescending(ca => ca.CreatedAt).FirstOrDefault(); 
                    if (primaryCover != null && !string.IsNullOrEmpty(primaryCover.PublicId))
                    {
                        relationships.Add(new RelationshipObject
                        {
                            Id = primaryCover.PublicId, // Sử dụng PublicId
                            Type = "cover_art",
                            Attributes = null // Không yêu cầu attributes cho cover trong danh sách manga
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

---

## Chức năng 2: Thông tin Tag luôn đi kèm trong manga/chi tiết manga

Thông tin chi tiết của các Tag sẽ được nhúng trực tiếp vào `attributes` của Manga, không còn nằm trong `relationships`.

### Bước 2.1: Cập nhật `Application/Features/Mangas/Queries/GetMangaById/GetMangaByIdQueryHandler.cs` (cho Tags)

Đảm bảo thông tin Tags được populate vào `MangaAttributesDto.Tags` và loại bỏ khỏi `relationships`.

```csharp
// Application/Features/Mangas/Queries/GetMangaById/GetMangaByIdQueryHandler.cs
using Application.Common.DTOs.Authors; // Cần cho AuthorAttributesDto
using Application.Common.DTOs.Mangas;
using Application.Common.DTOs.Tags;    // Cần cho TagAttributesDto
using Application.Common.Models;
using Application.Contracts.Persistence;
using AutoMapper;
using Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Collections.Generic; // Thêm using
using System.Linq; // Thêm using

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
            
            // GetMangaWithDetailsAsync đã include MangaTags.Tag.TagGroup và MangaAuthors.Author, CoverArts
            var manga = await _unitOfWork.MangaRepository.GetMangaWithDetailsAsync(request.MangaId);

            if (manga == null)
            {
                _logger.LogWarning("Không tìm thấy manga với ID: {MangaId}", request.MangaId);
                return null;
            }

            var mangaAttributes = _mapper.Map<MangaAttributesDto>(manga);
            
            // Populate Tags vào MangaAttributesDto (Yêu cầu 2)
            mangaAttributes.Tags = manga.MangaTags
                .Select(mt => new ResourceObject<TagAttributesDto>
                {
                    Id = mt.Tag.TagId.ToString(),
                    Type = "tag",
                    Attributes = _mapper.Map<TagAttributesDto>(mt.Tag),
                    Relationships = new List<RelationshipObject> 
                    {
                        new RelationshipObject { Id = mt.Tag.TagGroupId.ToString(), Type = "tag_group" }
                    }
                })
                .ToList();
            
            var relationships = new List<RelationshipObject>();

            bool includeAuthor = request.Includes?.Contains("author", StringComparer.OrdinalIgnoreCase) ?? false;
            bool includeArtist = request.Includes?.Contains("artist", StringComparer.OrdinalIgnoreCase) ?? false;
            // Mangadex API cho phép include "cover_art" cho GET /manga/{id} để lấy toàn bộ danh sách cover art.
            // Hiện tại yêu cầu chỉ nói "Include Cover cho API gọi danh sách manga".
            // Nếu muốn mở rộng cho chi tiết manga thì thêm logic tương tự ở đây.
            // bool includeAllCovers = request.Includes?.Contains("cover_art", StringComparer.OrdinalIgnoreCase) ?? false;

            // Xử lý Authors/Artists (Yêu cầu 3)
            if (manga.MangaAuthors != null)
            {
                foreach (var mangaAuthor in manga.MangaAuthors)
                {
                    if (mangaAuthor.Author != null)
                    {
                        var relationshipType = mangaAuthor.Role == MangaStaffRole.Author ? "author" : "artist";
                        bool shouldIncludeAttributes = (relationshipType == "author" && includeAuthor) || (relationshipType == "artist" && includeArtist);
                        
                        relationships.Add(new RelationshipObject
                        {
                            Id = mangaAuthor.Author.AuthorId.ToString(),
                            Type = relationshipType,
                            Attributes = shouldIncludeAttributes ? _mapper.Map<AuthorAttributesDto>(mangaAuthor.Author) : null
                        });
                    }
                }
            }
            
            // Relationship cho cover_art (chỉ cover chính/mới nhất, không phải toàn bộ danh sách)
            // Nếu client muốn toàn bộ danh sách cover, họ nên gọi endpoint /mangas/{id}/covers
            // Đây là relationship cho "primary cover" như Mangadex hay làm.
            var primaryCover = manga.CoverArts?.OrderByDescending(ca => ca.CreatedAt).FirstOrDefault(); 
            if (primaryCover != null)
            {
                relationships.Add(new RelationshipObject
                {
                    Id = primaryCover.CoverId.ToString(), // ID của CoverArt entity
                    Type = "cover_art",
                    // Nếu client yêu cầu include attributes của cover_art này, thì có thể thêm vào đây
                    // Attributes = includePrimaryCoverDetails ? _mapper.Map<CoverArtAttributesDto>(primaryCover) : null 
                    // Hiện tại, để đơn giản, không include attributes của cover trong relationship này.
                    Attributes = null 
                });
            }
            
            var resourceObject = new ResourceObject<MangaAttributesDto>
            {
                Id = manga.MangaId.ToString(),
                Type = "manga",
                Attributes = mangaAttributes, // Đã bao gồm Tags
                Relationships = relationships.Any() ? relationships : null // Không còn "tag" ở đây
            };
            
            return resourceObject;
        }
    }
}
```

### Bước 2.2: Cập nhật `Application/Features/Mangas/Queries/GetMangas/GetMangasQueryHandler.cs` (cho Tags)

Phần này đã được tích hợp vào Bước 1.1. `MangaAttributesDto.Tags` đã được populate và `relationships` của Manga không còn chứa tag.

---

## Chức năng 3: Cho phép gọi Include Artist/Author cho danh sách/chi tiết manga

Thông tin chi tiết của Artist/Author sẽ được trả về trong `attributes` của `RelationshipObject` tương ứng nếu client yêu cầu.

### Bước 3.1: Cập nhật `Application/Features/Mangas/Queries/GetMangaById/GetMangaByIdQueryHandler.cs` (cho Author/Artist)

Phần này đã được tích hợp vào Bước 2.1. `RelationshipObject` cho author/artist đã có thể chứa `attributes` tùy theo `request.Includes`.

### Bước 3.2: Cập nhật `Application/Features/Mangas/Queries/GetMangas/GetMangasQueryHandler.cs` (cho Author/Artist)

Phần này đã được tích hợp vào Bước 1.1. `RelationshipObject` cho author/artist đã có thể chứa `attributes` tùy theo `request.Includes`.

---

## Mục 4: Cập nhật tài liệu API

Cập nhật file `docs/api_conventions.md` để phản ánh các thay đổi trên.

```markdown
<!-- docs/api_conventions.md -->
# API Conventions

## 1. Base URL

Tất cả các API endpoints đều sử dụng tên của controller làm đường dẫn gốc. Ví dụ:

- `https://api.mangareader.com/mangas` (cho controller `MangasController`)
- `https://api.mangareader.com/authors` (cho controller `AuthorsController`)

Một số endpoint có thể có đường dẫn tùy chỉnh (absolute path) bắt đầu bằng `/` để tạo mối quan hệ rõ ràng hơn giữa các tài nguyên. Ví dụ:

- `https://api.mangareader.com/mangas/{mangaId}/covers`
- `https://api.mangareader.com/translatedmangas/{translatedMangaId}/chapters`

## 2. HTTP Methods

| Method | Mục đích |
|--------|----------|
| GET    | Lấy dữ liệu |
| POST   | Tạo mới dữ liệu |
| PUT    | Cập nhật toàn bộ dữ liệu |
| DELETE | Xóa dữ liệu |

## 3. Status Codes

| Status Code | Ý nghĩa |
|-------------|---------|
| 200 OK | Request thành công |
| 201 Created | Tạo mới thành công |
| 204 No Content | Request thành công, không có dữ liệu trả về |
| 400 Bad Request | Request không hợp lệ (lỗi validation) |
| 404 Not Found | Không tìm thấy tài nguyên |
| 500 Internal Server Error | Lỗi server |

## 4. Pagination

Các endpoints trả về danh sách đều hỗ trợ phân trang với các tham số:

- `offset`: Vị trí bắt đầu (mặc định: 0)
- `limit`: Số lượng tối đa kết quả trả về (mặc định: 20, tối đa: 100)

Ví dụ:

```
GET /mangas?offset=20&limit=10
```

## 5. Filtering, Sorting và Includes

### 5.1. Endpoint `GET /mangas` (Danh sách Manga)

-   **Filtering:**
    -   `titleFilter` (string): Lọc theo tiêu đề.
    -   `statusFilter` (enum `MangaStatus`): Lọc theo trạng thái.
    -   `contentRatingFilter` (enum `ContentRating`): Lọc theo đánh giá nội dung.
    -   `publicationDemographicsFilter[]` (list of enum `PublicationDemographic`): Lọc theo một hoặc nhiều đối tượng độc giả.
    -   `originalLanguageFilter` (string): Lọc theo ngôn ngữ gốc.
    -   `yearFilter` (int): Lọc theo năm xuất bản.
    -   `authorIdsFilter[]` (list of GUID): Lọc manga chứa BẤT KỲ author nào trong danh sách ID.
    -   **Lọc Tag Nâng Cao:**
        *   `includedTags[]` (array of GUIDs): Lọc các manga PHẢI chứa các tag được chỉ định.
        *   `includedTagsMode` (string: "AND" | "OR", mặc định "AND"): Chế độ cho `includedTags[]`.
        *   `excludedTags[]` (array of GUIDs): Lọc các manga KHÔNG ĐƯỢC chứa các tag được chỉ định.
        *   `excludedTagsMode` (string: "AND" | "OR", mặc định "OR"): Chế độ cho `excludedTags[]`.
-   **Sorting:** Sử dụng tham số `orderBy` (ví dụ: `updatedAt`, `title`, `year`, `createdAt`) và `ascending` (boolean, `true` hoặc `false`).
-   **Includes:** Sử dụng tham số `includes[]` để yêu cầu thêm dữ liệu liên quan. Các giá trị hỗ trợ:
    -   `cover_art`: Trả về `PublicId` của ảnh bìa mới nhất trong `relationships` của mỗi Manga.
    -   `author`: Trả về thông tin chi tiết (attributes) của tác giả (role 'Author') trong `relationships` của mỗi Manga.
    -   `artist`: Trả về thông tin chi tiết (attributes) của họa sĩ (role 'Artist') trong `relationships` của mỗi Manga.
    *Ví dụ:* `GET /mangas?includes[]=cover_art&includes[]=author`

### 5.2. Endpoint `GET /mangas/{id}` (Chi tiết Manga)

-   **Includes:** Sử dụng tham số `includes[]` để yêu cầu thêm dữ liệu liên quan. Các giá trị hỗ trợ:
    -   `author`: Trả về thông tin chi tiết (attributes) của tác giả (role 'Author') trong `relationships` của Manga.
    -   `artist`: Trả về thông tin chi tiết (attributes) của họa sĩ (role 'Artist') trong `relationships` của Manga.
    *Ví dụ:* `GET /mangas/{id}?includes[]=author&includes[]=artist`

## 6. Cấu Trúc Response Body (JSON)

Tất cả các response thành công (200 OK, 201 Created) trả về dữ liệu sẽ tuân theo cấu trúc sau:

### 6.1. Response Cho Một Đối Tượng Đơn Lẻ

```json
{
  "result": "ok",
  "response": "entity",
  "data": {
    "id": "string (GUID)",
    "type": "string (loại của resource, ví dụ: 'manga', 'author')",
    "attributes": {
      // Các thuộc tính cụ thể của resource.
      // Đối với Manga, trường "tags" sẽ chứa danh sách các TagObject chi tiết.
      // Ví dụ cho MangaAttributesDto:
      //   "title": "One Piece",
      //   "tags": [
      //     {
      //       "id": "tag-guid-1",
      //       "type": "tag",
      //       "attributes": { "name": "Action", "tagGroupName": "Genre", ... },
      //       "relationships": [{ "id": "tag-group-guid", "type": "tag_group" }]
      //     }
      //   ],
      //   ...
    },
    "relationships": [ // (Tùy chọn)
      {
        "id": "string (ID của entity liên quan, hoặc PublicId cho cover_art)",
        "type": "string (loại của MỐI QUAN HỆ hoặc VAI TRÒ)",
        "attributes": { // (Tùy chọn, chỉ có nếu được include)
          // Thuộc tính chi tiết của entity liên quan (ví dụ: AuthorAttributesDto)
        }
      }
      // ... các relationships khác ...
    ]
  }
}
```

*   **`data.id`**: ID của tài nguyên chính.
*   **`data.type`**: Loại của tài nguyên chính.
*   **`data.attributes`**: Object chứa các thuộc tính của tài nguyên.
    *   **Đối với Manga (`type: "manga"`)**: `data.attributes` sẽ chứa một mảng `tags`. Mỗi phần tử trong mảng `tags` là một `ResourceObject<TagAttributesDto>` đầy đủ, bao gồm `id`, `type: "tag"`, `attributes` (chứa `name`, `tagGroupName`, `createdAt`, `updatedAt`), và `relationships` (chứa thông tin về `tag_group` của tag đó).
*   **`data.relationships`**: Mảng các đối tượng `RelationshipObject`.
    *   **`id`**: ID của thực thể liên quan. **Đặc biệt:** Nếu `type` là `"cover_art"` (cho danh sách manga), `id` ở đây sẽ là `PublicId` của ảnh bìa.
    *   **`type`**: Mô tả vai trò/bản chất của mối quan hệ.
    *   **`attributes`**: (Tùy chọn) Nếu client yêu cầu `includes` (ví dụ: `includes[]=author`), trường này sẽ chứa object attributes của entity liên quan (ví dụ: `AuthorAttributesDto`). Nếu không include, trường này sẽ không có hoặc là `null`.

### 6.2. Response Cho Danh Sách Đối Tượng (Collection)

```json
{
  "result": "ok",
  "response": "collection",
  "data": [
    {
      "id": "string (GUID)",
      "type": "string (loại của resource)",
      "attributes": { /* ... xem mục 6.1 ... */ },
      "relationships": [ /* ... xem mục 6.1 ... */ ]
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

### 6.3. Ví dụ Response Cho Manga (Chi tiết hoặc trong danh sách)

```json
{
  "result": "ok",
  "response": "entity", // Hoặc "collection" nếu là danh sách
  "data": {
    "id": "123e4567-e89b-12d3-a456-426614174000",
    "type": "manga",
    "attributes": {
      "title": "Komi Can't Communicate",
      "originalLanguage": "ja",
      "publicationDemographic": "Shounen",
      "status": "Ongoing",
      "year": 2016,
      "contentRating": "Safe",
      "isLocked": false,
      "createdAt": "2023-01-01T00:00:00Z",
      "updatedAt": "2023-06-01T00:00:00Z",
      "tags": [
        {
          "id": "tag-guid-comedy",
          "type": "tag",
          "attributes": {
            "name": "Comedy",
            "tagGroupId": "tag-group-genre-guid",
            "tagGroupName": "Genre",
            "createdAt": "2023-01-01T00:00:00Z",
            "updatedAt": "2023-01-01T00:00:00Z"
          },
          "relationships": [
            {
              "id": "tag-group-genre-guid",
              "type": "tag_group"
            }
          ]
        },
        {
          "id": "tag-guid-school-life",
          "type": "tag",
          "attributes": {
            "name": "School Life",
            "tagGroupId": "tag-group-theme-guid",
            "tagGroupName": "Theme",
            "createdAt": "2023-01-01T00:00:00Z",
            "updatedAt": "2023-01-01T00:00:00Z"
          },
          "relationships": [
            {
              "id": "tag-group-theme-guid",
              "type": "tag_group"
            }
          ]
        }
      ]
    },
    "relationships": [
      {
        "id": "author-artist-guid-1", // ID của Author/Artist
        "type": "author", // hoặc "artist"
        // "attributes" sẽ có ở đây nếu client yêu cầu includes[]=author (hoặc artist)
        // Ví dụ, nếu includes[]=author:
        // "attributes": {
        //   "name": "Tomohito Oda",
        //   "biography": null,
        //   "createdAt": "2023-01-01T00:00:00Z",
        //   "updatedAt": "2023-01-01T00:00:00Z"
        // }
      },
      {
        "id": "cover-art-public-id-xyz", // Public ID của cover nếu GET /mangas và includes[]=cover_art
                                        // Hoặc GUID của CoverArt entity nếu là GET /mangas/{id}
        "type": "cover_art"
        // "attributes" của cover_art không được include mặc định trong relationship này
      }
    ]
  }
}
```

## 7. Cấu Trúc Error Response

```json
{
  "result": "error",
  "errors": [
    {
      "status": 404, // HTTP status code
      "title": "Not Found", // Tóm tắt lỗi
      "detail": "Manga with ID '123...' was not found." // Chi tiết lỗi (có thể null)
      // "id": "unique-error-code", // (Tùy chọn) Mã lỗi duy nhất
      // "context": { ... } // (Tùy chọn) Thông tin bổ sung
    }
  ]
}
```
*Lưu ý: trường `code` trong ví dụ cũ đã được đổi thành `status` để nhất quán với JSON:API spec và HTTP status codes.*

## 8. Validation Errors

```json
{
  "result": "error",
  "errors": [
    {
      "status": 400,
      "title": "Title", // Tên trường gây lỗi (hoặc "Validation Error" chung)
      "detail": "The Title field is required." 
      // "context": { "field": "Title" } // Có thể dùng context để chỉ rõ trường
    },
    {
      "status": 400,
      "title": "OriginalLanguage",
      "detail": "The OriginalLanguage field is required."
    }
  ]
}
```

## 9. Các Loại Relationship Type

| Giá trị `type` | Mô tả                                       | Nơi xuất hiện                                  | ID trong Relationship |
|----------------|---------------------------------------------|------------------------------------------------|-----------------------|
| `author`       | Tác giả của manga                            | `RelationshipObject` (Manga -> Author)         | GUID của Author        |
| `artist`       | Họa sĩ của manga                             | `RelationshipObject` (Manga -> Author)         | GUID của Author        |
| `tag_group`    | Nhóm chứa tag                               | `RelationshipObject` (Tag -> TagGroup)         | GUID của TagGroup      |
| `cover_art`    | Ảnh bìa của manga                            | `RelationshipObject` (Manga -> CoverArt)       | `PublicId` (nếu từ list + include) hoặc GUID của CoverArt (nếu từ detail manga) |
| `manga`        | Manga gốc                                   | `RelationshipObject` (Chapter/TranslatedManga/CoverArt -> Manga) | GUID của Manga |
| `user`         | Người dùng tải lên                           | `RelationshipObject` (Chapter -> User)         | ID của User (int)     |
| `chapter`      | Chương của manga                             | `RelationshipObject` (ChapterPage -> Chapter)  | GUID của Chapter       |
| `translated_manga` | Bản dịch của manga                       | `RelationshipObject` (Chapter -> TranslatedManga) | GUID của TranslatedManga |
*Lưu ý: `tag` không còn là relationship type của Manga, mà được nhúng vào `attributes`.*

## 10. Các Endpoints Chính (Cập nhật cho Manga)

### Mangas

- `GET /mangas`: Lấy danh sách manga.
    - **Hỗ trợ các tham số lọc `titleFilter`, `statusFilter`, `contentRatingFilter`, `demographicFilter[]`, `originalLanguageFilter`, `yearFilter`, `authorIdsFilter[]`, `includedTags[]`, `includedTagsMode`, `excludedTags[]`, `excludedTagsMode`.**
    - **Hỗ trợ `includes[]` với các giá trị: `cover_art`, `author`, `artist`.**
- `GET /mangas/{id}`: Lấy thông tin chi tiết manga.
    - **Hỗ trợ `includes[]` với các giá trị: `author`, `artist`.**
    - **Thông tin chi tiết Tags luôn được trả về trong `attributes.tags`.**
- `POST /mangas`: Tạo manga mới (bao gồm cả tags và authors)
- `PUT /mangas/{id}`: Cập nhật manga (bao gồm cả tags và authors)
- `DELETE /mangas/{id}`: Xóa manga

(... các endpoints khác giữ nguyên ...)
```