# TODO: Cập nhật API Lấy Danh Sách Manga với Tính Năng Lọc Tag Nâng Cao

Hướng dẫn này mô tả các bước cần thiết để cập nhật API lấy danh sách manga (`GET /mangas`) nhằm hỗ trợ các tham số lọc theo tag nâng cao, bao gồm `includedTags[]`, `includedTagsMode`, `excludedTags[]`, và `excludedTagsMode`, tương tự như cách Mangadex API hoạt động.

## Mục lục

1.  [Cập nhật `GetMangasQuery.cs`](#1-cập-nhật-getmangasquerycs)
2.  [Cập nhật `GetMangasQueryHandler.cs`](#2-cập-nhật-getmangasqueryhandlercs)
3.  [Tạo `GetMangasQueryValidator.cs`](#3-tạo-getmangasqueryvalidatorcs)
4.  [Cập nhật `MangasController.cs`](#4-cập-nhật-mangascontrollercs)
5.  [Cập nhật Tài liệu API (`api_conventions.md`)](#5-cập-nhật-tài-liệu-api-api_conventionsmd)

---

## 1. Cập nhật `GetMangasQuery.cs`

Mở file `Application/Features/Mangas/Queries/GetMangas/GetMangasQuery.cs` và thêm các thuộc tính mới để xử lý việc lọc tag.

**Mô tả thay đổi:**
*   Thêm `IncludedTags` (danh sách GUID các tag bắt buộc phải có).
*   Thêm `IncludedTagsMode` (chế độ lọc cho `IncludedTags`: "AND" hoặc "OR", mặc định "AND").
*   Thêm `ExcludedTags` (danh sách GUID các tag không được phép có).
*   Thêm `ExcludedTagsMode` (chế độ lọc cho `ExcludedTags`: "AND" hoặc "OR", mặc định "OR").

**Code đầy đủ cho `GetMangasQuery.cs`:**
```csharp
// Application/Features/Mangas/Queries/GetMangas/GetMangasQuery.cs
using Application.Common.DTOs;
using Application.Common.DTOs.Mangas;
using Application.Common.Models;
using Domain.Enums;
using MediatR;

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
        
        // Các thuộc tính cũ để lọc theo TagIdsFilter và AuthorIdsFilter vẫn giữ nguyên
        // nếu bạn muốn giữ lại chức năng cũ song song hoặc để so sánh.
        // Trong ví dụ này, chúng ta sẽ tập trung vào các tham số mới.
        // public List<Guid>? TagIdsFilter { get; set; } // Lọc manga chứa BẤT KỲ tag nào trong danh sách này (Logic OR đơn giản)
        public List<Guid>? AuthorIdsFilter { get; set; } // Lọc manga chứa BẤT KỲ author nào trong danh sách này

        // --- Các thuộc tính mới cho lọc tag nâng cao ---
        /// <summary>
        /// Danh sách các ID của tag mà manga PHẢI BAO GỒM.
        /// </summary>
        public List<Guid>? IncludedTags { get; set; }

        /// <summary>
        /// Chế độ lọc cho IncludedTags. Có thể là "AND" hoặc "OR".
        /// "AND": Manga phải chứa TẤT CẢ các tag trong IncludedTags.
        /// "OR": Manga phải chứa ÍT NHẤT MỘT tag trong IncludedTags.
        /// Mặc định là "AND".
        /// </summary>
        public string? IncludedTagsMode { get; set; } // Mặc định "AND" trong Handler

        /// <summary>
        /// Danh sách các ID của tag mà manga KHÔNG ĐƯỢC BAO GỒM.
        /// </summary>
        public List<Guid>? ExcludedTags { get; set; }

        /// <summary>
        /// Chế độ lọc cho ExcludedTags. Có thể là "AND" hoặc "OR".
        /// "AND": Manga không được chứa TẤT CẢ các tag trong ExcludedTags.
        /// "OR": Manga không được chứa BẤT KỲ tag nào trong ExcludedTags.
        /// Mặc định là "OR".
        /// </summary>
        public string? ExcludedTagsMode { get; set; } // Mặc định "OR" trong Handler
        // --- Kết thúc các thuộc tính mới ---
        
        public string OrderBy { get; set; } = "UpdatedAt"; // title, year, createdAt, updatedAt
        public bool Ascending { get; set; } = false; // Mặc định giảm dần cho UpdatedAt
    }
}
```

---

## 2. Cập nhật `GetMangasQueryHandler.cs`

Mở file `Application/Features/Mangas/Queries/GetMangas/GetMangasQueryHandler.cs` và cập nhật logic xây dựng `predicate` để hỗ trợ các tham số lọc tag mới.

**Mô tả thay đổi:**
*   Xử lý `IncludedTags` và `IncludedTagsMode`:
    *   Nếu `IncludedTagsMode` là "OR", sử dụng `m.MangaTags.Any(mt => request.IncludedTags.Contains(mt.TagId))`.
    *   Nếu `IncludedTagsMode` là "AND" (mặc định), sử dụng `request.IncludedTags.All(includedTagId => m.MangaTags.Any(mt => mt.TagId == includedTagId))`.
*   Xử lý `ExcludedTags` và `ExcludedTagsMode`:
    *   Nếu `ExcludedTagsMode` là "OR" (mặc định), sử dụng `!m.MangaTags.Any(mt => request.ExcludedTags.Contains(mt.TagId))`.
    *   Nếu `ExcludedTagsMode` là "AND", sử dụng `!request.ExcludedTags.All(excludedTagId => m.MangaTags.Any(mt => mt.TagId == excludedTagId))`.
*   Sử dụng `ExpressionExtensions.And()` để kết hợp các điều kiện lọc.

**Code đầy đủ cho `GetMangasQueryHandler.cs`:**
```csharp
// Application/Features/Mangas/Queries/GetMangas/GetMangasQueryHandler.cs
using Application.Common.DTOs;
using Application.Common.DTOs.Mangas;
using Application.Common.Extensions;
using Application.Common.Models;
using Application.Contracts.Persistence;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;
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
            if (request.AuthorIdsFilter != null && request.AuthorIdsFilter.Any())
            {
                predicate = predicate.And(m => m.MangaAuthors.Any(ma => request.AuthorIdsFilter.Contains(ma.AuthorId)));
            }

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

---

## 3. Tạo `GetMangasQueryValidator.cs`

Tạo một file mới `Application/Features/Mangas/Queries/GetMangas/GetMangasQueryValidator.cs` để validate các tham số mới.

**Mô tả thay đổi:**
*   Validate `IncludedTagsMode` và `ExcludedTagsMode` chỉ chấp nhận giá trị "AND" hoặc "OR" (không phân biệt chữ hoa chữ thường).
*   Đảm bảo rằng nếu `IncludedTagsMode` hoặc `ExcludedTagsMode` được cung cấp, thì danh sách tag tương ứng (`IncludedTags` hoặc `ExcludedTags`) không được rỗng.

**Code đầy đủ cho `GetMangasQueryValidator.cs`:**
```csharp
// Application/Features/Mangas/Queries/GetMangas/GetMangasQueryValidator.cs
using FluentValidation;

namespace Application.Features.Mangas.Queries.GetMangas
{
    public class GetMangasQueryValidator : AbstractValidator<GetMangasQuery>
    {
        public GetMangasQueryValidator()
        {
            RuleFor(query => query.Limit)
                .GreaterThanOrEqualTo(0).WithMessage("Limit must be greater than or equal to 0.")
                .LessThanOrEqualTo(100).WithMessage("Limit cannot exceed 100.");

            RuleFor(query => query.Offset)
                .GreaterThanOrEqualTo(0).WithMessage("Offset must be greater than or equal to 0.");

            When(query => !string.IsNullOrWhiteSpace(query.IncludedTagsMode), () => {
                RuleFor(query => query.IncludedTagsMode)
                    .Must(mode => mode.Equals("AND", StringComparison.OrdinalIgnoreCase) || mode.Equals("OR", StringComparison.OrdinalIgnoreCase))
                    .WithMessage("IncludedTagsMode must be 'AND' or 'OR'.");
                
                RuleFor(query => query.IncludedTags)
                    .NotEmpty().WithMessage("IncludedTags cannot be empty when IncludedTagsMode is specified.")
                    .When(query => !string.IsNullOrWhiteSpace(query.IncludedTagsMode));
            });

            When(query => request => request.IncludedTags != null && request.IncludedTags.Any() && string.IsNullOrWhiteSpace(request.IncludedTagsMode), () => {
                 RuleFor(query => query.IncludedTagsMode)
                    .NotEmpty().WithMessage("IncludedTagsMode is required when IncludedTags are provided.");
            });


            When(query => !string.IsNullOrWhiteSpace(query.ExcludedTagsMode), () => {
                RuleFor(query => query.ExcludedTagsMode)
                    .Must(mode => mode.Equals("AND", StringComparison.OrdinalIgnoreCase) || mode.Equals("OR", StringComparison.OrdinalIgnoreCase))
                    .WithMessage("ExcludedTagsMode must be 'AND' or 'OR'.");

                RuleFor(query => query.ExcludedTags)
                    .NotEmpty().WithMessage("ExcludedTags cannot be empty when ExcludedTagsMode is specified.")
                    .When(query => !string.IsNullOrWhiteSpace(query.ExcludedTagsMode));
            });
            
            When(query => request => request.ExcludedTags != null && request.ExcludedTags.Any() && string.IsNullOrWhiteSpace(request.ExcludedTagsMode), () => {
                 RuleFor(query => query.ExcludedTagsMode)
                    .NotEmpty().WithMessage("ExcludedTagsMode is required when ExcludedTags are provided.");
            });

            // Các rule validate khác cho TitleFilter, StatusFilter, etc. có thể được thêm ở đây nếu cần.
            // Ví dụ:
            // RuleFor(query => query.TitleFilter)
            //    .MaximumLength(255).WithMessage("TitleFilter cannot exceed 255 characters.");
        }
    }
}
```

**Lưu ý:** Bạn cần đăng ký validator này trong `Program.cs` nếu chưa có cơ chế tự động đăng ký tất cả validator trong assembly. Tuy nhiên, với cấu hình `builder.Services.AddValidatorsFromAssembly(typeof(Application.AssemblyReference).Assembly, ServiceLifetime.Scoped);` hiện tại, validator này sẽ được tự động đăng ký.

---

## 4. Cập nhật `MangasController.cs`

File `MangaReaderDB/Controllers/MangasController.cs` không cần thay đổi gì lớn vì ASP.NET Core có khả năng tự động bind các tham số từ query string (ví dụ `includedTags[]=guid1&includedTags[]=guid2`) vào `List<Guid>` trong `GetMangasQuery` khi sử dụng `[FromQuery]`.

Chỉ cần đảm bảo action `GetMangas` nhận `GetMangasQuery` làm tham số với attribute `[FromQuery]`.

**Code đầy đủ cho `MangasController.cs` (phần liên quan đến `GetMangas`):**
```csharp
// MangaReaderDB/Controllers/MangasController.cs
using Application.Common.DTOs.Mangas;
using Application.Common.Models; // For ResourceObject
using Application.Common.Responses;
using Application.Exceptions;
using Application.Features.Mangas.Commands.CreateManga;
using Application.Features.Mangas.Commands.DeleteManga;
using Application.Features.Mangas.Commands.UpdateManga;
using Application.Features.Mangas.Queries.GetMangaById;
using Application.Features.Mangas.Queries.GetMangas; // Đảm bảo using này
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

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
        public async Task<IActionResult> GetMangas([FromQuery] GetMangasQuery query) // Action này không cần thay đổi
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

---

## 5. Cập nhật Tài liệu API (`api_conventions.md`)

Mở file `docs/api_conventions.md` và cập nhật phần mô tả endpoint `GET /mangas` để bao gồm các tham số lọc tag mới.

**Mô tả thay đổi:**
*   Thêm mô tả cho `includedTags[]`, `includedTagsMode`, `excludedTags[]`, và `excludedTagsMode` vào phần "Filtering và Sorting" hoặc trong mô tả của endpoint `GET /mangas`.

**Nội dung cập nhật cho `docs/api_conventions.md` (chỉ phần liên quan):**

```markdown
<!-- docs/api_conventions.md -->
# API Conventions

## 1. Base URL
... (giữ nguyên) ...

## 2. HTTP Methods
... (giữ nguyên) ...

## 3. Status Codes
... (giữ nguyên) ...

## 4. Pagination
... (giữ nguyên) ...

## 5. Filtering và Sorting

Các endpoints trả về danh sách hỗ trợ lọc và sắp xếp.

**Ví dụ cho Manga:**
```
GET /mangas?statusFilter=ongoing&orderBy=title&ascending=true
```

**Tham số lọc nâng cao cho Manga (Tags):**

*   `includedTags[]` (array of GUIDs): Lọc các manga PHẢI chứa các tag được chỉ định.
    *   Ví dụ: `includedTags[]=tagId1&includedTags[]=tagId2`
*   `includedTagsMode` (string: "AND" | "OR"): Chế độ cho `includedTags[]`.
    *   `AND` (mặc định): Manga phải chứa TẤT CẢ các tag trong `includedTags[]`.
    *   `OR`: Manga phải chứa ÍT NHẤT MỘT tag trong `includedTags[]`.
*   `excludedTags[]` (array of GUIDs): Lọc các manga KHÔNG ĐƯỢC chứa các tag được chỉ định.
    *   Ví dụ: `excludedTags[]=tagId3&excludedTags[]=tagId4`
*   `excludedTagsMode` (string: "AND" | "OR"): Chế độ cho `excludedTags[]`.
    *   `OR` (mặc định): Manga không được chứa BẤT KỲ tag nào trong `excludedTags[]`.
    *   `AND`: Manga không được chứa TẤT CẢ các tag trong `excludedTags[]` (nghĩa là, nó được phép chứa một số tag trong danh sách này, miễn là không phải tất cả).

**Ví dụ sử dụng lọc tag nâng cao:**
Lấy các manga chứa tag "Action" (ID: `action-guid`) VÀ tag "Adventure" (ID: `adventure-guid`), đồng thời KHÔNG chứa tag "Romance" (ID: `romance-guid`):
```
GET /mangas?includedTags[]=action-guid&includedTags[]=adventure-guid&includedTagsMode=AND&excludedTags[]=romance-guid&excludedTagsMode=OR
```

## 6. Cấu Trúc Response Body (JSON)
... (giữ nguyên các phần khác) ...

### 6.1. Response Cho Một Đối Tượng Đơn Lẻ
... (giữ nguyên) ...

### 6.2. Response Cho Danh Sách Đối Tượng (Collection)
... (giữ nguyên) ...

### 6.3. Ví dụ Response Cho Manga
... (giữ nguyên) ...

### 6.4. Ví dụ Response Cho Danh Sách Tags
... (giữ nguyên) ...

## 7. Cấu Trúc Error Response
... (giữ nguyên) ...

## 8. Validation Errors
... (giữ nguyên) ...

## 9. Các Loại Relationship Type
... (giữ nguyên) ...

## 10. Các Endpoints Chính

### Mangas

- `GET /mangas`: Lấy danh sách manga. **Hỗ trợ các tham số lọc `titleFilter`, `statusFilter`, `contentRatingFilter`, `demographicFilter`, `originalLanguageFilter`, `yearFilter`, `authorIdsFilter[]`, `includedTags[]`, `includedTagsMode`, `excludedTags[]`, `excludedTagsMode`.**
- `GET /mangas/{id}`: Lấy thông tin chi tiết manga
- `POST /mangas`: Tạo manga mới (bao gồm cả tags và authors)
- `PUT /mangas/{id}`: Cập nhật manga (bao gồm cả tags và authors)
- `DELETE /mangas/{id}`: Xóa manga

... (giữ nguyên các endpoint khác) ...