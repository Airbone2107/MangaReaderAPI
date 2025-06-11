Chắc chắn rồi, đây là file `TODO.md` chi tiết để cập nhật API lấy danh sách manga, cho phép truyền nhiều tham số `publicationDemographic`:

```markdown
<!-- TODO.md -->
# TODO: Cập nhật API lấy danh sách Manga để hỗ trợ nhiều PublicationDemographic

Hướng dẫn này mô tả các bước cần thiết để cập nhật API lấy danh sách manga (`GET /mangas`) cho phép client truyền vào nhiều giá trị cho tham số `publicationDemographic` để lọc kết quả. Ví dụ: `/mangas?publicationDemographics=Shounen&publicationDemographics=Seinen`.

## Các bước thực hiện

### Bước 1: Cập nhật `GetMangasQuery` DTO

Thay đổi thuộc tính `DemographicFilter` từ một `PublicationDemographic?` đơn lẻ thành một danh sách `List<PublicationDemographic>?`.

**File cần thay đổi:** `Application\Features\Mangas\Queries\GetMangas\GetMangasQuery.cs`

**Nội dung file đầy đủ sau khi thay đổi:**

```csharp
// Application\Features\Mangas\Queries\GetMangas\GetMangasQuery.cs
using Application.Common.DTOs;
using Application.Common.DTOs.Mangas;
using Application.Common.Models;
using Domain.Enums;
using MediatR;
using System.Collections.Generic; // Thêm using này

namespace Application.Features.Mangas.Queries.GetMangas
{
    public class GetMangasQuery : IRequest<PagedResult<ResourceObject<MangaAttributesDto>>>
    {
        public int Offset { get; set; } = 0;
        public int Limit { get; set; } = 20;
        public string? TitleFilter { get; set; }
        public MangaStatus? StatusFilter { get; set; }
        public ContentRating? ContentRatingFilter { get; set; }
        // Thay đổi từ PublicationDemographic? DemographicFilter thành List<PublicationDemographic>?
        public List<PublicationDemographic>? PublicationDemographicsFilter { get; set; }
        public string? OriginalLanguageFilter { get; set; }
        public int? YearFilter { get; set; }
        public List<Guid>? TagIdsFilter { get; set; } // Lọc manga chứa BẤT KỲ tag nào trong danh sách này
        public List<Guid>? AuthorIdsFilter { get; set; } // Lọc manga chứa BẤT KỲ author nào trong danh sách này
        
        // TODO: [Improvement] Thêm bộ lọc cho TranslatedManga.LanguageKey? (Ví dụ: lấy manga có bản dịch tiếng Việt)

        public string OrderBy { get; set; } = "UpdatedAt"; // title, year, createdAt, updatedAt
        public bool Ascending { get; set; } = false; // Mặc định giảm dần cho UpdatedAt
    }
}
```

### Bước 2: Cập nhật `GetMangasQueryHandler`

Điều chỉnh logic lọc trong `Handle` method để xử lý danh sách `PublicationDemographicsFilter`.

**File cần thay đổi:** `Application\Features\Mangas\Queries\GetMangas\GetMangasQueryHandler.cs`

**Nội dung file đầy đủ sau khi thay đổi:**

```csharp
// Application\Features\Mangas\Queries\GetMangas\GetMangasQueryHandler.cs
using Application.Common.DTOs;
using Application.Common.DTOs.Mangas;
using Application.Common.Extensions; // Thêm using này
using Application.Common.Models;
using Application.Contracts.Persistence;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions; // Cần cho Expression
using System.Linq; // Thêm using này cho .Any()

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

### Bước 3: Kiểm tra `MangasController`

Thông thường, `MangasController` sẽ không cần thay đổi vì ASP.NET Core Model Binding sẽ tự động xử lý việc binding nhiều tham số query string có cùng tên vào một `List<T>`.

**File (không thay đổi, chỉ để tham khảo):** `MangaReaderDB\Controllers\MangasController.cs`

```csharp
// MangaReaderDB\Controllers\MangasController.cs
using Application.Common.DTOs.Mangas;
using Application.Common.Models; // For ResourceObject
using Application.Common.Responses;
using Application.Exceptions;
using Application.Features.Mangas.Commands.CreateManga;
using Application.Features.Mangas.Commands.DeleteManga;
using Application.Features.Mangas.Commands.UpdateManga;
using Application.Features.Mangas.Queries.GetMangaById;
using Application.Features.Mangas.Queries.GetMangas;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace MangaReaderDB.Controllers
{
    public class MangasController : BaseApiController
    {
        private readonly IValidator<CreateMangaDto> _createMangaDtoValidator;
        private readonly IValidator<UpdateMangaDto> _updateMangaDtoValidator;
        private readonly ILogger<MangasController> _logger; // Thêm logger

        public MangasController(
            IValidator<CreateMangaDto> createMangaDtoValidator,
            IValidator<UpdateMangaDto> updateMangaDtoValidator,
            ILogger<MangasController> logger) // Inject logger
        {
            _createMangaDtoValidator = createMangaDtoValidator;
            _updateMangaDtoValidator = updateMangaDtoValidator;
            _logger = logger; // Gán logger
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
                ContentRating = createDto.ContentRating,
                TagIds = createDto.TagIds,
                Authors = createDto.Authors
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
        public async Task<IActionResult> GetMangas([FromQuery] GetMangasQuery query) // Không cần thay đổi ở đây
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
                IsLocked = updateDto.IsLocked,
                TagIds = updateDto.TagIds,
                Authors = updateDto.Authors
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
    }
}
```

### Bước 4: Cập nhật Tài liệu API

Cập nhật file tài liệu API (ví dụ: `docs/api_conventions.md` hoặc Swagger comments) để phản ánh sự thay đổi của tham số `publicationDemographic`.

**File cần cập nhật (ví dụ):** `docs/api_conventions.md`

**Đoạn cần cập nhật trong `docs/api_conventions.md` (phần mô tả endpoint GET /mangas):**

Trước đó có thể là:
```
- `publicationDemographic`: Lọc theo đối tượng độc giả (ví dụ: `Shounen`, `Shoujo`).
```

Sau khi cập nhật sẽ là:
```
- `publicationDemographicsFilter`: Lọc theo một hoặc nhiều đối tượng độc giả. Truyền nhiều giá trị bằng cách lặp lại tham số (ví dụ: `publicationDemographicsFilter=Shounen&publicationDemographicsFilter=Seinen`).
```

**Nội dung đầy đủ của file `docs/api_conventions.md` sau khi cập nhật (chỉ thay đổi phần liên quan đến filter của `GET /mangas`):**
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
- `limit`: Số lượng tối đa kết quả trả về (mặc định và tối đa: 20)

Ví dụ:

```
GET /mangas?offset=20&limit=10
```

## 5. Filtering và Sorting

Các endpoints trả về danh sách hỗ trợ lọc và sắp xếp. Ví dụ cho endpoint `GET /mangas`:

-   **Filtering:**
    -   `titleFilter` (string): Lọc theo tiêu đề.
    -   `statusFilter` (enum `MangaStatus`): Lọc theo trạng thái.
    -   `contentRatingFilter` (enum `ContentRating`): Lọc theo đánh giá nội dung.
    -   `publicationDemographicsFilter` (list of enum `PublicationDemographic`): Lọc theo một hoặc nhiều đối tượng độc giả. Truyền nhiều giá trị bằng cách lặp lại tham số (ví dụ: `publicationDemographicsFilter=Shounen&publicationDemographicsFilter=Seinen`).
    -   `originalLanguageFilter` (string): Lọc theo ngôn ngữ gốc.
    -   `yearFilter` (int): Lọc theo năm xuất bản.
    -   `tagIdsFilter` (list of GUID): Lọc manga chứa BẤT KỲ tag nào trong danh sách ID.
    -   `authorIdsFilter` (list of GUID): Lọc manga chứa BẤT KỲ author nào trong danh sách ID.
-   **Sorting:** Sử dụng tham số `orderBy` (ví dụ: `updatedAt`, `title`, `year`, `createdAt`) và `ascending` (boolean, `true` hoặc `false`).

Ví dụ:

```
GET /mangas?statusFilter=Ongoing&publicationDemographicsFilter=Shounen&publicationDemographicsFilter=Seinen&orderBy=title&ascending=true
```

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
*   **`data.type`**: Loại của tài nguyên chính (ví dụ: `"manga"`, `"author"`, `"tag"`, `"chapter"`, `"cover_art"`, `"translated_manga"`, `"tag_group"`, `"chapter_page"`). Được viết bằng snake_case, số ít.
*   **`data.attributes`**: Một object chứa tất cả các thuộc tính của tài nguyên (tương ứng với `...AttributesDto`).
*   **`data.relationships`**: (Tùy chọn, có thể không có nếu không có mối quan hệ) Một mảng các đối tượng `RelationshipObject`.
    *   **`id`**: ID của thực thể liên quan.
    *   **`type`**: Mô tả vai trò hoặc bản chất của mối quan hệ đó đối với thực thể gốc.
        *   Ví dụ, đối với một Manga:
            *   Relationship tới Author với vai trò `author`: `{ "id": "author-guid", "type": "author" }`
            *   Relationship tới Author với vai trò `artist`: `{ "id": "artist-guid", "type": "artist" }`
            *   Relationship tới Tag: `{ "id": "tag-guid", "type": "tag" }`
            *   Relationship tới CoverArt chính: `{ "id": "cover_art-guid", "type": "cover_art" }`
        *   Đối với một Chapter:
            *   Relationship tới User (uploader): `{ "id": "user-id", "type": "user" }`
            *   Relationship tới Manga (manga gốc của chapter, thông qua TranslatedManga): `{ "id": "manga-guid", "type": "manga" }`
            *   Relationship tới TranslatedManga (bản dịch chứa chapter này): `{ "id": "translated-manga-guid", "type": "translated_manga" }`

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

### 6.3. Ví dụ Response Cho Manga

```json
{
  "result": "ok",
  "response": "entity",
  "data": {
    "id": "123e4567-e89b-12d3-a456-426614174000",
    "type": "manga",
    "attributes": {
      "title": "One Piece",
      "originalLanguage": "ja",
      "publicationDemographic": "Shounen",
      "status": "Ongoing",
      "year": 1997,
      "contentRating": "Safe",
      "isLocked": false,
      "createdAt": "2023-01-01T00:00:00Z",
      "updatedAt": "2023-06-01T00:00:00Z"
    },
    "relationships": [
      {
        "id": "223e4567-e89b-12d3-a456-426614174001",
        "type": "author"
      },
      {
        "id": "223e4567-e89b-12d3-a456-426614174001",
        "type": "artist"
      },
      {
        "id": "323e4567-e89b-12d3-a456-426614174002",
        "type": "tag"
      },
      {
        "id": "423e4567-e89b-12d3-a456-426614174003",
        "type": "cover_art"
      }
    ]
  }
}
```

### 6.4. Ví dụ Response Cho Danh Sách Tags

```json
{
  "result": "ok",
  "response": "collection",
  "data": [
    {
      "id": "123e4567-e89b-12d3-a456-426614174000",
      "type": "tag",
      "attributes": {
        "name": "Action",
        "tagGroupId": "223e4567-e89b-12d3-a456-426614174001",
        "tagGroupName": "Genres",
        "createdAt": "2023-01-01T00:00:00Z",
        "updatedAt": "2023-01-01T00:00:00Z"
      },
      "relationships": [
        {
          "id": "223e4567-e89b-12d3-a456-426614174001",
          "type": "tag_group"
        }
      ]
    },
    {
      "id": "123e4567-e89b-12d3-a456-426614174002",
      "type": "tag",
      "attributes": {
        "name": "Adventure",
        "tagGroupId": "223e4567-e89b-12d3-a456-426614174001",
        "tagGroupName": "Genres",
        "createdAt": "2023-01-01T00:00:00Z",
        "updatedAt": "2023-01-01T00:00:00Z"
      },
      "relationships": [
        {
          "id": "223e4567-e89b-12d3-a456-426614174001",
          "type": "tag_group"
        }
      ]
    }
  ],
  "limit": 10,
  "offset": 0,
  "total": 50
}
```

## 7. Cấu Trúc Error Response

```json
{
  "result": "error",
  "errors": [
    {
      "code": 404,
      "title": "Not Found",
      "detail": "Manga with ID '123e4567-e89b-12d3-a456-426614174000' was not found."
    }
  ]
}
```

## 8. Validation Errors

```json
{
  "result": "error",
  "errors": [
    {
      "code": 400,
      "title": "Validation Error",
      "detail": "The Title field is required.",
      "source": {
        "field": "Title"
      }
    },
    {
      "code": 400,
      "title": "Validation Error",
      "detail": "The OriginalLanguage field is required.",
      "source": {
        "field": "OriginalLanguage"
      }
    }
  ]
}
```

## 9. Các Loại Relationship Type

Dưới đây là danh sách các giá trị `type` được sử dụng trong các đối tượng `ResourceObject` (cho chính tài nguyên) và `RelationshipObject` (cho mối quan hệ):

| Giá trị `type` | Mô tả                                       | Nơi xuất hiện                                  |
|----------------|---------------------------------------------|------------------------------------------------|
| `author`       | Tác giả của manga                            | `ResourceObject` (cho Author); `RelationshipObject` (Manga -> Author) |
| `artist`       | Họa sĩ của manga                             | `RelationshipObject` (Manga -> Author)         |
| `tag`          | Thẻ gắn với manga                            | `ResourceObject` (cho Tag); `RelationshipObject` (Manga -> Tag) |
| `tag_group`    | Nhóm chứa tag                               | `ResourceObject` (cho TagGroup); `RelationshipObject` (Tag -> TagGroup) |
| `cover_art`    | Ảnh bìa của manga                            | `ResourceObject` (cho CoverArt); `RelationshipObject` (Manga -> CoverArt) |
| `manga`        | Manga gốc                                   | `ResourceObject` (cho Manga); `RelationshipObject` (Chapter/TranslatedManga/CoverArt -> Manga) |
| `user`         | Người dùng tải lên                           | `ResourceObject` (cho User - nếu có API riêng cho User); `RelationshipObject` (Chapter -> User) |
| `chapter`      | Chương của manga                             | `ResourceObject` (cho Chapter); `RelationshipObject` (ChapterPage -> Chapter) |
| `chapter_page` | Trang của chương                             | `ResourceObject` (cho ChapterPage)             |
| `translated_manga` | Bản dịch của manga                       | `ResourceObject` (cho TranslatedManga); `RelationshipObject` (Chapter -> TranslatedManga) |

## 10. Các Endpoints Chính

### Mangas

- `GET /mangas`: Lấy danh sách manga
- `GET /mangas/{id}`: Lấy thông tin chi tiết manga
- `POST /mangas`: Tạo manga mới (bao gồm cả tags và authors)
- `PUT /mangas/{id}`: Cập nhật manga (bao gồm cả tags và authors)
- `DELETE /mangas/{id}`: Xóa manga

### Authors

- `GET /authors`: Lấy danh sách tác giả
- `GET /authors/{id}`: Lấy thông tin chi tiết tác giả
- `POST /authors`: Tạo tác giả mới
- `PUT /authors/{id}`: Cập nhật tác giả
- `DELETE /authors/{id}`: Xóa tác giả

### Tags

- `GET /tags`: Lấy danh sách tag
- `GET /tags/{id}`: Lấy thông tin chi tiết tag
- `POST /tags`: Tạo tag mới
- `PUT /tags/{id}`: Cập nhật tag
- `DELETE /tags/{id}`: Xóa tag

### TagGroups

- `GET /taggroups`: Lấy danh sách nhóm tag
- `GET /taggroups/{id}`: Lấy thông tin chi tiết nhóm tag
- `POST /taggroups`: Tạo nhóm tag mới
- `PUT /taggroups/{id}`: Cập nhật nhóm tag
- `DELETE /taggroups/{id}`: Xóa nhóm tag

### Chapters

- `GET /chapters/{id}`: Lấy thông tin chi tiết chapter
- `GET /translatedmangas/{translatedMangaId}/chapters`: Lấy danh sách chapter của một bản dịch
- `POST /chapters`: Tạo chapter mới
- `PUT /chapters/{id}`: Cập nhật chapter
- `DELETE /chapters/{id}`: Xóa chapter
- `GET /chapters/{chapterId}/pages`: Lấy danh sách trang của chapter
- `POST /chapters/{chapterId}/pages/entry`: Tạo entry cho trang mới

### ChapterPages

- `POST /chapterpages/{pageId}/image`: Upload ảnh cho trang
- `PUT /chapterpages/{pageId}/details`: Cập nhật thông tin trang
- `DELETE /chapterpages/{pageId}`: Xóa trang

### CoverArts

- `GET /coverarts/{id}`: Lấy thông tin chi tiết ảnh bìa
- `GET /mangas/{mangaId}/covers`: Lấy danh sách ảnh bìa của manga
- `POST /mangas/{mangaId}/covers`: Upload ảnh bìa mới
- `DELETE /coverarts/{id}`: Xóa ảnh bìa

### TranslatedMangas

- `GET /translatedmangas/{id}`: Lấy thông tin chi tiết bản dịch
- `GET /mangas/{mangaId}/translations`: Lấy danh sách bản dịch của manga
- `POST /translatedmangas`: Tạo bản dịch mới
- `PUT /translatedmangas/{id}`: Cập nhật bản dịch
- `DELETE /translatedmangas/{id}`: Xóa bản dịch
```

### Bước 5: Kiểm thử

Sau khi hoàn tất các thay đổi, cần kiểm thử kỹ lưỡng API với các trường hợp sau:

1.  Gọi `GET /mangas` không có tham số `publicationDemographicsFilter`.
2.  Gọi `GET /mangas` với một giá trị `publicationDemographicsFilter` (ví dụ: `?publicationDemographicsFilter=Shounen`).
3.  Gọi `GET /mangas` với nhiều giá trị `publicationDemographicsFilter` (ví dụ: `?publicationDemographicsFilter=Shounen&publicationDemographicsFilter=Seinen`).
4.  Gọi `GET /mangas` với một giá trị `publicationDemographicsFilter` không hợp lệ hoặc không tồn tại.
5.  Gọi `GET /mangas` kết hợp `publicationDemographicsFilter` với các tham số lọc khác (ví dụ: `statusFilter`, `titleFilter`).
6.  Kiểm tra logic phân trang và sắp xếp khi có `publicationDemographicsFilter`.

---
Hoàn tất các bước trên sẽ cho phép API hỗ trợ lọc danh sách manga theo nhiều `publicationDemographic` một cách linh hoạt.
```