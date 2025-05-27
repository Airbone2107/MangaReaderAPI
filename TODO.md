Mục tiêu: Chuyển đổi cấu trúc DTO trả về của API sang định dạng `ResourceObject` với `id`, `type`, `attributes`, và `relationships` để tăng tính nhất quán và tuân theo các chuẩn API hiện đại.

## I. Tạo Các DTO Cơ Sở và DTO Attributes

1.  **Tạo DTO `ResourceObject<TAttributes>`:**
    *   **File:** `Application/Common/Models/ResourceObject.cs`
    *   **Nội dung:**
        ```csharp
        // Application/Common/Models/ResourceObject.cs
        namespace Application.Common.Models
        {
            public class ResourceObject<TAttributes>
            {
                /// <summary>
                /// Định danh duy nhất của tài nguyên (dưới dạng chuỗi GUID).
                /// </summary>
                public string Id { get; set; } = string.Empty;

                /// <summary>
                /// Loại của tài nguyên chính, ví dụ: "manga", "author", "tag".
                /// Sử dụng snake_case và số ít.
                /// </summary>
                public string Type { get; set; } = string.Empty;

                /// <summary>
                /// Các thuộc tính riêng của tài nguyên.
                /// </summary>
                public TAttributes Attributes { get; set; } = default!;

                /// <summary>
                /// Danh sách các mối quan hệ với các tài nguyên khác.
                /// </summary>
                public List<RelationshipObject> Relationships { get; set; } = new List<RelationshipObject>();
            }
        }
        ```

2.  **Tạo DTO `RelationshipObject`:**
    *   **File:** `Application/Common/Models/RelationshipObject.cs`
    *   **Nội dung:**
        ```csharp
        // Application/Common/Models/RelationshipObject.cs
        namespace Application.Common.Models
        {
            public class RelationshipObject
            {
                /// <summary>
                /// Định danh duy nhất của tài nguyên liên quan (dưới dạng chuỗi GUID).
                /// </summary>
                public string Id { get; set; } = string.Empty;

                /// <summary>
                /// Loại của mối quan hệ hoặc vai trò của tài nguyên liên quan đối với tài nguyên gốc.
                /// Ví dụ: "author", "artist", "cover_art", "tag", "user", "manga".
                /// Sử dụng snake_case và số ít.
                /// </summary>
                public string Type { get; set; } = string.Empty;
            }
        }
        ```

3.  **Tạo các DTO `...AttributesDto.cs` cho mỗi Entity:**
    *   Mục tiêu: Tách các thuộc tính dữ liệu thuần túy (không bao gồm ID và các navigation properties) của các DTO hiện tại (`MangaDto`, `AuthorDto`,...) vào các DTO attributes mới.
    *   **Danh sách các file Attributes DTO cần tạo/kiểm tra:**
        *   `Application/Common/DTOs/Authors/AuthorAttributesDto.cs`:
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
                    // Các thuộc tính khác của Author (trừ Id và relationships) nếu có
                }
            }
            ```
        *   `Application/Common/DTOs/Mangas/MangaAttributesDto.cs`:
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
                    // Các thuộc tính khác của Manga (trừ Id và relationships) nếu có
                }
            }
            ```
        *   `Application/Common/DTOs/Tags/TagAttributesDto.cs`:
            ```csharp
            // Application/Common/DTOs/Tags/TagAttributesDto.cs
            namespace Application.Common.DTOs.Tags
            {
                public class TagAttributesDto
                {
                    public string Name { get; set; } = string.Empty;
                    public DateTime CreatedAt { get; set; }
                    public DateTime UpdatedAt { get; set; }
                    // TagGroupId và TagGroupName sẽ là relationship đến 'tag_group'.
                }
            }
            ```
        *   `Application/Common/DTOs/TagGroups/TagGroupAttributesDto.cs`:
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
        *   `Application/Common/DTOs/Chapters/ChapterAttributesDto.cs`:
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
                    // Uploader (User), TranslatedManga, và ChapterPages sẽ là relationships hoặc endpoint riêng.
                }
            }
            ```
        *   `Application/Common/DTOs/CoverArts/CoverArtAttributesDto.cs`:
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
                    // MangaId sẽ là một relationship.
                }
            }
            ```
        *   `Application/Common/DTOs/Users/UserAttributesDto.cs`:
            ```csharp
            // Application/Common/DTOs/Users/UserAttributesDto.cs
            namespace Application.Common.DTOs.Users
            {
                public class UserAttributesDto
                {
                    public string Username { get; set; } = string.Empty;
                    // Thêm các thuộc tính khác của User nếu có (ví dụ: roles, version,...)
                }
            }
            ```
        *   `Application/Common/DTOs/TranslatedMangas/TranslatedMangaAttributesDto.cs`:
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
                    // MangaId sẽ là một relationship.
                }
            }
            ```
        *   **`ChapterPageAttributesDto`**: Hiện tại không cần thiết nếu `ChapterPage` không phải là một resource độc lập được trả về qua API theo cấu trúc `ResourceObject`. `ChapterAttributesDto` có `PagesCount` và danh sách các trang có thể được lấy qua một endpoint riêng (ví dụ: `/chapters/{id}/pages` hoặc `/at-home/server/{chapterId}` như Mangadex).

## II. Cập Nhật Tầng Application

1.  **Cập Nhật `MappingProfile.cs`:**
    *   **File:** `Application/Common/Mappings/MappingProfile.cs`
    *   **Công việc:**
        *   Với mỗi Entity (ví dụ: `Domain.Entities.Author`), tạo mapping sang `AuthorAttributesDto` tương ứng.
            ```csharp
            // Application/Common/Mappings/MappingProfile.cs
            // ...
            public class MappingProfile : Profile
            {
                public MappingProfile()
                {
                    // User
                    CreateMap<Domain.Entities.User, UserAttributesDto>();
                    // Nếu UserDto cũ vẫn dùng nội bộ, giữ lại: CreateMap<User, UserDto>();

                    // Author
                    CreateMap<Domain.Entities.Author, AuthorAttributesDto>();
                    // Giữ lại các mapping DTO -> Entity và Command -> Entity nếu cần
                    CreateMap<CreateAuthorDto, Domain.Entities.Author>(); 
                    CreateMap<UpdateAuthorDto, Domain.Entities.Author>(); 
                    CreateMap<CreateAuthorCommand, Domain.Entities.Author>();

                    // TagGroup
                    CreateMap<Domain.Entities.TagGroup, TagGroupAttributesDto>();
                    CreateMap<CreateTagGroupDto, Domain.Entities.TagGroup>();
                    CreateMap<UpdateTagGroupDto, Domain.Entities.TagGroup>();
                    CreateMap<CreateTagGroupCommand, Domain.Entities.TagGroup>();

                    // Tag
                    CreateMap<Domain.Entities.Tag, TagAttributesDto>();
                    // Mapping Tag -> TagDto cũ có thể không cần nữa nếu TagDto không còn dùng cho response
                    // CreateMap<Tag, TagDto>()
                    //    .ForMember(dest => dest.TagGroupName, opt => opt.MapFrom(src => src.TagGroup != null ? src.TagGroup.Name : string.Empty));
                    CreateMap<CreateTagDto, Domain.Entities.Tag>();
                    CreateMap<UpdateTagDto, Domain.Entities.Tag>();
                    CreateMap<CreateTagCommand, Domain.Entities.Tag>();

                    // Manga
                    CreateMap<Domain.Entities.Manga, MangaAttributesDto>();
                    // Mapping Manga -> MangaDto cũ có thể không cần nữa
                    CreateMap<CreateMangaDto, Domain.Entities.Manga>(); 
                    CreateMap<UpdateMangaDto, Domain.Entities.Manga>(); 
                    CreateMap<CreateMangaCommand, Domain.Entities.Manga>(); 

                    // TranslatedManga
                    CreateMap<Domain.Entities.TranslatedManga, TranslatedMangaAttributesDto>();
                    CreateMap<CreateTranslatedMangaDto, Domain.Entities.TranslatedManga>();
                    CreateMap<UpdateTranslatedMangaDto, Domain.Entities.TranslatedManga>();
                    CreateMap<CreateTranslatedMangaCommand, Domain.Entities.TranslatedManga>();

                    // CoverArt
                    CreateMap<Domain.Entities.CoverArt, CoverArtAttributesDto>();
                    CreateMap<CreateCoverArtDto, Domain.Entities.CoverArt>(); 

                    // ChapterPage: Không có attributes DTO riêng, ChapterPageDto cũ có thể vẫn dùng
                    CreateMap<Domain.Entities.ChapterPage, ChapterPageDto>();
                    CreateMap<CreateChapterPageDto, Domain.Entities.ChapterPage>(); 
                    CreateMap<UpdateChapterPageDto, Domain.Entities.ChapterPage>();

                    // Chapter
                    CreateMap<Domain.Entities.Chapter, ChapterAttributesDto>()
                        .ForMember(dest => dest.PagesCount, opt => opt.MapFrom(src => src.ChapterPages.Count));
                    // Mapping Chapter -> ChapterDto cũ có thể không cần nữa
                    // CreateMap<Chapter, ChapterDto>()
                    //     .ForMember(dest => dest.Uploader, opt => opt.MapFrom(src => src.User)) // Cần User -> UserDto
                    //     .ForMember(dest => dest.PagesCount, opt => opt.MapFrom(src => src.ChapterPages.Count))
                    //     .ForMember(dest => dest.ChapterPages, opt => opt.MapFrom(src => src.ChapterPages.OrderBy(p => p.PageNumber))); // Cần ChapterPage -> ChapterPageDto
                    CreateMap<CreateChapterDto, Domain.Entities.Chapter>();
                    CreateMap<UpdateChapterDto, Domain.Entities.Chapter>();
                    CreateMap<CreateChapterCommand, Domain.Entities.Chapter>();
                }
            }
            ```
        *   **Lưu ý:** AutoMapper sẽ không tự động map sang `ResourceObject<TAttributes>` hoặc `RelationshipObject`. Việc này sẽ được thực hiện thủ công trong các Query Handlers.

2.  **Cập Nhật Các Query Handlers:**
    *   **Tệp cần cập nhật:** Tất cả các file `*QueryHandler.cs` trong thư mục `Application/Features/`.
    *   **Công việc chung cho mỗi Handler:**
        1.  Truy vấn Entity từ repository, đảm bảo Eager Loading các navigation properties cần thiết để xây dựng `relationships`.
        2.  Nếu Entity không tìm thấy, trả về `null` (Controller sẽ xử lý thành 404).
        3.  Xác định chuỗi `type` cho resource chính (ví dụ: `"manga"`, `"author"`, `Entity.GetType().Name.ToLowerInvariant()` có thể là một khởi đầu, nhưng cần chuẩn hóa thành snake_case nếu cần và theo quy ước của Mangadex API).
        4.  Map Entity sang `...AttributesDto` tương ứng bằng AutoMapper.
        5.  **Xây dựng `List<RelationshipObject>`:**
            *   Duyệt qua các navigation properties của Entity đã được Eager Load.
            *   Với mỗi entity liên quan, tạo một `RelationshipObject`.
            *   `relationship.Id = relatedEntity.Id.ToString();`
            *   `relationship.Type = "...";` (Xác định `type` dựa trên ngữ cảnh và vai trò của mối quan hệ. Xem ví dụ dưới đây và tham khảo `api.yaml` của Mangadex).
        6.  Tạo instance `ResourceObject<TAttributesDto>` và gán các giá trị `Id` (của entity chính, dạng chuỗi), `Type`, `Attributes`, và `Relationships`.
        7.  Trả về `ResourceObject<TAttributesDto>`.
        8.  Đối với Handler trả về danh sách (ví dụ: `GetMangasQueryHandler`):
            *   Lặp qua danh sách Entities, thực hiện các bước trên cho mỗi Entity.
            *   Tạo `PagedResult<ResourceObject<TAttributesDto>>` để trả về.

    *   **Ví dụ cụ thể cho `GetMangaByIdQueryHandler.cs`:**
        ```csharp
        // Application/Features/Mangas/Queries/GetMangaById/GetMangaByIdQueryHandler.cs
        // ...
        using Application.Common.Models; // Cho ResourceObject, RelationshipObject
        using Application.Common.DTOs.Mangas; // Cho MangaAttributesDto
        // ...

        // Sửa đổi kiểu trả về của Handler
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
                _logger.LogInformation("GetMangaByIdQueryHandler: Getting manga with ID: {MangaId}", request.MangaId);
                var manga = await _unitOfWork.MangaRepository.GetMangaWithDetailsAsync(request.MangaId); 
                // GetMangaWithDetailsAsync cần eager load MangaAuthors.Author, MangaTags.Tag, CoverArts, TranslatedMangas

                if (manga == null)
                {
                    _logger.LogWarning("GetMangaByIdQueryHandler: Manga with ID: {MangaId} not found.", request.MangaId);
                    return null;
                }

                var mangaAttributes = _mapper.Map<MangaAttributesDto>(manga);
                var relationships = new List<RelationshipObject>();

                // Authors and Artists
                if (manga.MangaAuthors != null)
                {
                    foreach (var mangaAuthor in manga.MangaAuthors.Where(ma => ma.Author != null))
                    {
                        relationships.Add(new RelationshipObject
                        {
                            Id = mangaAuthor.Author.AuthorId.ToString(),
                            Type = mangaAuthor.Role == Domain.Enums.MangaStaffRole.Author ? "author" : "artist"
                        });
                    }
                }

                // Tags
                if (manga.MangaTags != null)
                {
                    foreach (var mangaTag in manga.MangaTags.Where(mt => mt.Tag != null))
                    {
                        relationships.Add(new RelationshipObject
                        {
                            Id = mangaTag.Tag.TagId.ToString(),
                            Type = "tag" 
                        });
                    }
                }
                
                // Cover Art (ví dụ lấy cover đầu tiên hoặc primary cover)
                var primaryCover = manga.CoverArts?.FirstOrDefault(); // Cần logic rõ ràng hơn để chọn primary cover nếu có nhiều
                if (primaryCover != null)
                {
                    relationships.Add(new RelationshipObject
                    {
                        Id = primaryCover.CoverId.ToString(),
                        Type = "cover_art" // Theo Mangadex API
                    });
                }
                
                // Creator (User) - Nếu Manga có trường CreatorUserId
                // if (manga.CreatorUserId.HasValue) { /* Add relationship type "creator" or "user" */ }


                // Translated Mangas (Bản dịch) - Thường không phải là relationship trực tiếp trong JSON:API
                // Thay vào đó, /manga/{id}/feed hoặc /manga/{id}/aggregate sẽ cung cấp chapter theo ngôn ngữ.
                // Hoặc có thể có endpoint /manga/{id}/translations để list các bản dịch.
                // Nếu vẫn muốn đưa vào relationships của Manga gốc:
                // if (manga.TranslatedMangas != null)
                // {
                //     foreach (var tm in manga.TranslatedMangas)
                //     {
                //         relationships.Add(new RelationshipObject
                //         {
                //             Id = tm.TranslatedMangaId.ToString(),
                //             Type = "manga_translation" // Hoặc một type phù hợp
                //         });
                //     }
                // }


                var resourceObject = new ResourceObject<MangaAttributesDto>
                {
                    Id = manga.MangaId.ToString(),
                    Type = "manga", // Theo Mangadex API
                    Attributes = mangaAttributes,
                    Relationships = relationships.DistinctBy(r => new { r.Id, r.Type }).ToList() // Đảm bảo không trùng lặp relationship
                };

                return resourceObject;
            }
        }
        ```
    *   **Áp dụng tương tự cho các Query Handler khác:**
        *   `GetAuthorsQueryHandler`, `GetTagsQueryHandler`, `GetChaptersByTranslatedMangaQueryHandler`, `GetTagGroupByIdQueryHandler`, `GetCoverArtByIdQueryHandler`, `GetTranslatedMangaByIdQueryHandler`, etc.
        *   **Lưu ý về `type` trong `RelationshipObject`:**
            *   **Chapter -> User (uploader):** `Type = "user"`
            *   **Chapter -> Manga (manga gốc của chapter):** `Type = "manga"`
            *   **Author -> Mangas:** (Trong relationship của Author, mỗi Manga liên quan) `Type = "manga"`
            *   **Tag -> Mangas:** `Type = "manga"`
            *   **Tag -> TagGroup:** `Type = "tag_group"`
            *   **CoverArt -> Manga:** `Type = "manga"`
            *   **TranslatedManga -> Manga (Manga gốc):** `Type = "manga"`

3.  **Cập Nhật Command Handlers (nếu trả về DTO):**
    *   **Tệp cần cập nhật:** Các `*CommandHandler.cs` nếu chúng trả về DTO của entity vừa được tạo/cập nhật.
    *   **Công việc:** Hiện tại, các Command Handlers của bạn chủ yếu trả về `Guid` hoặc `Unit`. Nếu muốn chúng trả về `ResourceObject` hoàn chỉnh, bạn sẽ cần query lại entity sau khi lưu và xây dựng `ResourceObject` tương tự như Query Handlers. Để đơn giản, có thể giữ nguyên kiểu trả về hiện tại của Command Handlers. Client sẽ tự gọi GET endpoint để lấy chi tiết nếu cần.

## III. Cập Nhật Tầng Presentation (API Controllers)

1.  **Cập Nhật Kiểu Trả Về Của Actions và `[ProducesResponseType]`:**
    *   **Tệp cần cập nhật:** Tất cả các API Controllers trong `MangaReaderDB/Controllers/`.
    *   **Công việc:**
        *   Thay đổi kiểu trả về của các action GET:
            *   Từ `Task<ActionResult<MangaDto>>` thành `async Task<ActionResult<ApiResponse<ResourceObject<MangaAttributesDto>>>>`.
            *   Từ `Task<ActionResult<PagedResult<MangaDto>>>` thành `async Task<ActionResult<ApiCollectionResponse<ResourceObject<MangaAttributesDto>>>>`.
        *   Cập nhật `[ProducesResponseType]` tương ứng.
            ```csharp
            // Ví dụ trong MangasController - GetMangaById
            // ...
            using Application.Common.Models;
            using Application.Common.DTOs.Mangas;
            // ...
            [HttpGet("{id:guid}")]
            [ProducesResponseType(typeof(ApiResponse<ResourceObject<MangaAttributesDto>>), StatusCodes.Status200OK)]
            [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
            public async Task<IActionResult> GetMangaById(Guid id)
            {
                var query = new GetMangaByIdQuery { MangaId = id };
                var resource = await Mediator.Send(query); 
                if (resource == null)
                {
                    throw new NotFoundException(nameof(Domain.Entities.Manga), id);
                }
                // BaseApiController.Ok(T data) sẽ tự bọc trong ApiResponse<T>
                return Ok(resource); 
            }

            // Ví dụ trong MangasController - GetMangas
            [HttpGet]
            [ProducesResponseType(typeof(ApiCollectionResponse<ResourceObject<MangaAttributesDto>>), StatusCodes.Status200OK)]
            public async Task<IActionResult> GetMangas([FromQuery] GetMangasQuery query)
            {
                var pagedResources = await Mediator.Send(query); 
                // BaseApiController.Ok(PagedResult<T> data) sẽ tự bọc trong ApiCollectionResponse<T>
                return Ok(pagedResources);
            }
            
            // Ví dụ trong MangasController - CreateManga
            [HttpPost]
            // Sửa kiểu response cho Create
            [ProducesResponseType(typeof(ApiResponse<ResourceObject<MangaAttributesDto>>), StatusCodes.Status201Created)]
            [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
            [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)] // Nếu MangaId trong DTO không tìm thấy (ít xảy ra với Create)
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
                var mangaId = await Mediator.Send(command); // Giả sử command trả về Guid
                
                // Lấy lại thông tin đầy đủ để trả về theo cấu trúc mới
                var query = new GetMangaByIdQuery { MangaId = mangaId };
                var resource = await Mediator.Send(query);

                if (resource == null) 
                {
                    _logger.LogError($"Manga with ID {mangaId} was created but could not be retrieved immediately.");
                    // Trả về lỗi server hoặc một response phù hợp
                    return StatusCode(StatusCodes.Status500InternalServerError, 
                        new ApiErrorResponse(new ApiError(500, "Creation Error", "Manga created but could not be retrieved.")));
                }
                // BaseApiController.Created<T>(...) sẽ tự bọc trong ApiResponse<T>
                return Created(nameof(GetMangaById), new { id = mangaId }, resource);
            }
            ```
    *   Đảm bảo các phương thức `Ok(data)` và `Created(actionName, routeValues, data)` trong `BaseApiController.cs` hoạt động chính xác với các kiểu `ResourceObject<TAttributesDto>` và `PagedResult<ResourceObject<TAttributesDto>>`.

## IV. DTOs Cho Request Bodies (Không thay đổi)

*   Các DTOs như `CreateMangaDto`, `UpdateAuthorDto` dùng cho request body **KHÔNG** thay đổi cấu trúc theo `ResourceObject`. Chúng giữ nguyên các trường cần thiết cho việc tạo/cập nhật.

## V. Kiểm Thử (Testing)

1.  **Unit Tests:**
    *   Viết/cập nhật unit test cho các Query Handlers để kiểm tra:
        *   `ResourceObject` được tạo ra có đúng `Id`, `Type`.
        *   `Attributes` được map chính xác.
        *   `Relationships` được tạo ra đúng số lượng và đúng `Id`, `Type` cho từng mối quan hệ. Đặc biệt kiểm tra logic cho `type` (ví dụ: "author" vs "artist").
2.  **Integration/API Tests:**
    *   Thực hiện gọi API đến tất cả các endpoint GET.
    *   Sử dụng Postman hoặc các công cụ tương tự để kiểm tra cấu trúc JSON của response.
    *   Xác minh các trường `id`, `type`, `attributes`, `relationships` có mặt và đúng định dạng.
    *   Kiểm tra giá trị của `type` trong `relationships`.
    *   Đảm bảo các endpoint POST, PUT, DELETE vẫn hoạt động chính xác.

## VI. Cập Nhật Tài Liệu API

1.  **`docs/api_conventions.md`:**
    *   Cập nhật phần mô tả cấu trúc response.
    *   Thêm ví dụ JSON chi tiết cho từng loại resource (Manga, Author,...) với cấu trúc `ResourceObject` mới, bao gồm cả `relationships` và các `type` đa dạng của nó.
2.  **Swagger/OpenAPI:**
    *   Đảm bảo các `[ProducesResponseType]` trong controllers được cập nhật đúng để Swagger gen ra tài liệu chính xác.
    *   Nếu sử dụng các ví dụ Swagger (Swashbuckle.AspNetCore.Filters), cập nhật các ví dụ đó.

## VII. Thứ Tự Thực Hiện Đề Xuất (Tóm tắt lại)

1.  **Hoàn thiện các DTOs `...AttributesDto.cs`, `ResourceObject.cs`, `RelationshipObject.cs`.**
2.  **Cập nhật `MappingProfile.cs`** để map Entities sang `...AttributesDto`.
3.  **Cập nhật (nếu cần) `ApiResponse.cs` và `ApiCollectionResponse.cs`** (có vẻ đã ổn).
4.  **Refactor Query Handlers:** Bắt đầu với các entity đơn giản, sau đó đến các entity phức tạp.
5.  **Refactor API Controllers** song song với Query Handlers.
6.  **Kiểm tra Command Handlers** (nếu chúng trả về DTO, hiện tại chủ yếu là `Guid` hoặc `Unit`).
7.  **Viết/Cập nhật Unit Tests và API Tests.**
8.  **Cập nhật tài liệu API.**
```