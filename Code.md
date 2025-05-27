```markdown
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
```markdown
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
```markdown
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
```markdown
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
```markdown
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
```markdown
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
```markdown
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
```markdown
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
```markdown
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
```markdown
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
```markdown
// Application/Common/Mappings/MappingProfile.cs
namespace Application.Common.Mappings
{
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

    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // User
            CreateMap<Domain.Entities.User, UserAttributesDto>();
            // Giữ lại User -> UserDto nếu vẫn dùng nội bộ
            CreateMap<Domain.Entities.User, UserDto>();


            // Author
            CreateMap<Domain.Entities.Author, AuthorAttributesDto>();
            // Giữ lại các mapping DTO -> Entity và Command -> Entity nếu cần
            CreateMap<CreateAuthorDto, Domain.Entities.Author>(); 
            CreateMap<UpdateAuthorDto, Domain.Entities.Author>(); 
            CreateMap<CreateAuthorCommand, Domain.Entities.Author>();
            // Bỏ: CreateMap<Author, AuthorDto>(); nếu AuthorDto không còn dùng cho response


            // TagGroup
            CreateMap<Domain.Entities.TagGroup, TagGroupAttributesDto>();
            // Bỏ: CreateMap<TagGroup, TagGroupDto>(); nếu TagGroupDto không còn dùng cho response
            CreateMap<CreateTagGroupDto, Domain.Entities.TagGroup>();
            CreateMap<UpdateTagGroupDto, Domain.Entities.TagGroup>();
            CreateMap<CreateTagGroupCommand, Domain.Entities.TagGroup>();

            // Tag
            CreateMap<Domain.Entities.Tag, TagAttributesDto>();
            // Bỏ: CreateMap<Tag, TagDto>(); nếu TagDto không còn dùng cho response
            CreateMap<CreateTagDto, Domain.Entities.Tag>();
            CreateMap<UpdateTagDto, Domain.Entities.Tag>();
            CreateMap<CreateTagCommand, Domain.Entities.Tag>();

            // Manga
            CreateMap<Domain.Entities.Manga, MangaAttributesDto>();
            // Bỏ: CreateMap<Manga, MangaDto>(); nếu MangaDto không còn dùng cho response
            CreateMap<CreateMangaDto, Domain.Entities.Manga>(); 
            CreateMap<UpdateMangaDto, Domain.Entities.Manga>(); 
            CreateMap<CreateMangaCommand, Domain.Entities.Manga>(); 

            // TranslatedManga
            CreateMap<Domain.Entities.TranslatedManga, TranslatedMangaAttributesDto>();
            // Bỏ: CreateMap<TranslatedManga, TranslatedMangaDto>(); nếu TranslatedMangaDto không còn dùng cho response
            CreateMap<CreateTranslatedMangaDto, Domain.Entities.TranslatedManga>();
            CreateMap<UpdateTranslatedMangaDto, Domain.Entities.TranslatedManga>();
            CreateMap<CreateTranslatedMangaCommand, Domain.Entities.TranslatedManga>();

            // CoverArt
            CreateMap<Domain.Entities.CoverArt, CoverArtAttributesDto>();
            // Bỏ: CreateMap<CoverArt, CoverArtDto>(); nếu CoverArtDto không còn dùng cho response
            CreateMap<CreateCoverArtDto, Domain.Entities.CoverArt>(); 

            // ChapterPage: ChapterPageDto có thể vẫn dùng nội bộ hoặc cho các endpoint chuyên biệt không theo ResourceObject
            CreateMap<Domain.Entities.ChapterPage, ChapterPageDto>();
            CreateMap<CreateChapterPageDto, Domain.Entities.ChapterPage>(); 
            CreateMap<UpdateChapterPageDto, Domain.Entities.ChapterPage>();

            // Chapter
            CreateMap<Domain.Entities.Chapter, ChapterAttributesDto>()
                .ForMember(dest => dest.PagesCount, opt => opt.MapFrom(src => src.ChapterPages.Count));
            // Bỏ: CreateMap<Chapter, ChapterDto>(); nếu ChapterDto không còn dùng cho response
            CreateMap<CreateChapterDto, Domain.Entities.Chapter>();
            CreateMap<UpdateChapterDto, Domain.Entities.Chapter>();
            CreateMap<CreateChapterCommand, Domain.Entities.Chapter>();
        }
    }
} 
```
```markdown
// Application/Features/Mangas/Queries/GetMangaById/GetMangaByIdQuery.cs
using Application.Common.DTOs.Mangas; // Sẽ thay thế bằng Models
using Application.Common.Models; // Cho ResourceObject
using MediatR;

namespace Application.Features.Mangas.Queries.GetMangaById
{
    // Sửa đổi kiểu trả về
    public class GetMangaByIdQuery : IRequest<ResourceObject<MangaAttributesDto>?>
    {
        public Guid MangaId { get; set; }
    }
} 
```
```markdown
// Application/Features/Mangas/Queries/GetMangaById/GetMangaByIdQueryHandler.cs
using Application.Common.DTOs.Mangas;
using Application.Common.Models; // Cho ResourceObject, RelationshipObject
using Application.Contracts.Persistence;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Domain.Entities; // Cần thiết để truy cập các Entity gốc
using System.Linq; // Cho FirstOrDefault, Where, Select

namespace Application.Features.Mangas.Queries.GetMangaById
{
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
            
            // GetMangaWithDetailsAsync đã bao gồm các navigation properties cần thiết
            // MangaAuthors.Author, MangaTags.Tag.TagGroup, CoverArts, TranslatedMangas
            var manga = await _unitOfWork.MangaRepository.GetMangaWithDetailsAsync(request.MangaId); 

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
                    // Nếu muốn thêm TagGroup của Tag vào relationships của Manga
                    // if (mangaTag.Tag.TagGroup != null)
                    // {
                    //     relationships.Add(new RelationshipObject
                    //     {
                    //         Id = mangaTag.Tag.TagGroup.TagGroupId.ToString(),
                    //         Type = "tag_group" // Hoặc một type phù hợp cho mối quan hệ này từ Manga
                    //     });
                    // }
                }
            }
            
            // Cover Art (ví dụ lấy cover đầu tiên hoặc primary cover)
            // Logic này cần cải thiện nếu có trường IsPrimaryCover hoặc tương tự
            var primaryCover = manga.CoverArts?.FirstOrDefault(); 
            if (primaryCover != null)
            {
                relationships.Add(new RelationshipObject
                {
                    Id = primaryCover.CoverId.ToString(),
                    Type = "cover_art" // Theo Mangadex API
                });
            }
            
            // Translated Mangas (các bản dịch)
            // Thường không được list trực tiếp trong relationships của manga root,
            // mà sẽ có endpoint riêng hoặc thông qua aggregate/feed.
            // Nếu muốn thêm:
            // if (manga.TranslatedMangas != null)
            // {
            //     foreach (var tm in manga.TranslatedMangas)
            //     {
            //         relationships.Add(new RelationshipObject
            //         {
            //             Id = tm.TranslatedMangaId.ToString(),
            //             Type = "manga_translation" // Hoặc "translated_version", etc.
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
}
```
```markdown
// MangaReaderDB/Controllers/BaseApiController.cs
using Application.Common.DTOs; // Cho PagedResult
using Application.Common.Models; // Cho ResourceObject
using Application.Common.Responses; 
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection; 

namespace MangaReaderDB.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public abstract class BaseApiController : ControllerBase
    {
        private IMediator? _mediator;
        protected IMediator Mediator => _mediator ??= HttpContext.RequestServices.GetService<IMediator>()!;

        /// <summary>
        /// Trả về 200 OK với payload là một ResourceObject.
        /// </summary>
        protected ActionResult Ok<TAttributes>(ResourceObject<TAttributes> resourceObject)
        {
            if (resourceObject == null)
            {
                // Nếu resourceObject là null, Controller nên throw NotFoundException
                // thay vì truyền null vào đây.
                // Tuy nhiên, để phòng trường hợp, có thể trả về NotFound().
                return NotFound(); 
            }
            return base.Ok(new ApiResponse<ResourceObject<TAttributes>>(resourceObject));
        }

        /// <summary>
        /// Trả về 200 OK với payload là một danh sách ResourceObject có phân trang.
        /// </summary>
        protected ActionResult Ok<TAttributes>(PagedResult<ResourceObject<TAttributes>> pagedResources)
        {
            return base.Ok(new ApiCollectionResponse<ResourceObject<TAttributes>>(pagedResources.Items, pagedResources.Total, pagedResources.Offset, pagedResources.Limit));
        }
        
        /// <summary>
        /// Trả về 200 OK với response không có data payload cụ thể (chỉ có "result": "ok").
        /// </summary>
        protected ActionResult OkResponseForAction()
        {
            return base.Ok(new ApiResponse());
        }

        /// <summary>
        /// Trả về 201 Created với payload là một ResourceObject và location header.
        /// </summary>
        protected ActionResult Created<TAttributes>(string actionName, object? routeValues, ResourceObject<TAttributes> resource)
        {
            return base.CreatedAtAction(actionName, routeValues, new ApiResponse<ResourceObject<TAttributes>>(resource));
        }
    }
} 
```
```markdown
// MangaReaderDB/Controllers/MangasController.cs
using Application.Common.DTOs;
using Application.Common.DTOs.Mangas;
using Application.Common.Models; // Cho ResourceObject
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
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Linq; 
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
            
            var query = new GetMangaByIdQuery { MangaId = mangaId };
            var resource = await Mediator.Send(query);

            if (resource == null) 
            {
                _logger.LogError($"Manga with ID {mangaId} was created but could not be retrieved immediately.");
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new ApiErrorResponse(new ApiError(500, "Creation Error", "Manga created but could not be retrieved.")));
            }
            // BaseApiController.Created<TAttributes>(...) sẽ tự bọc trong ApiResponse
            return Created(nameof(GetMangaById), new { id = mangaId }, resource);
        }

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
            // BaseApiController.Ok<TAttributes>(...) sẽ tự bọc trong ApiResponse
            return Ok(resource); 
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiCollectionResponse<ResourceObject<MangaAttributesDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMangas([FromQuery] GetMangasQuery query)
        {
            // GetMangasQueryHandler sẽ trả về PagedResult<ResourceObject<MangaAttributesDto>>
            var pagedResources = await Mediator.Send(query); 
            // BaseApiController.Ok<TAttributes>(PagedResult<ResourceObject<TAttributes>>) sẽ tự bọc trong ApiCollectionResponse
            return Ok(pagedResources);
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

        // ... (Các action DeleteManga, AddMangaTag, RemoveMangaTag, AddMangaAuthor, RemoveMangaAuthor không thay đổi logic trả về nhiều)
        // ... (Giữ nguyên các action này vì chúng trả về NoContent hoặc lỗi)
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

```markdown
// Application/Features/Mangas/Queries/GetMangas/GetMangasQuery.cs
using Application.Common.DTOs;
using Application.Common.DTOs.Mangas; // Sẽ thay thế bằng Models
using Application.Common.Models;    // Cho ResourceObject
using Domain.Enums;
using MediatR;
using System.Collections.Generic; 

namespace Application.Features.Mangas.Queries.GetMangas
{
    // Sửa đổi kiểu trả về
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

```markdown
// Application/Features/Mangas/Queries/GetMangas/GetMangasQueryHandler.cs
using Application.Common.DTOs;
using Application.Common.DTOs.Mangas; // Sẽ dùng MangaAttributesDto
using Application.Common.Models;    // Cho ResourceObject, RelationshipObject
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
    // Sửa đổi kiểu trả về của Handler
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
            // ... (thêm các điều kiện filter khác từ request vào predicate như đã có) ...
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
                // ... (thêm các case sắp xếp khác như đã có) ...
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
                includeProperties: "MangaTags.Tag.TagGroup,MangaAuthors.Author,CoverArts,TranslatedMangas"
            );

            var resourceObjects = new List<ResourceObject<MangaAttributesDto>>();
            foreach (var manga in pagedMangas.Items)
            {
                var mangaAttributes = _mapper.Map<MangaAttributesDto>(manga);
                var relationships = new List<RelationshipObject>();

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
                var primaryCover = manga.CoverArts?.FirstOrDefault();
                if (primaryCover != null)
                {
                    relationships.Add(new RelationshipObject
                    {
                        Id = primaryCover.CoverId.ToString(),
                        Type = "cover_art"
                    });
                }
                // ... (thêm logic relationships khác nếu cần) ...

                resourceObjects.Add(new ResourceObject<MangaAttributesDto>
                {
                    Id = manga.MangaId.ToString(),
                    Type = "manga",
                    Attributes = mangaAttributes,
                    Relationships = relationships.DistinctBy(r => new { r.Id, r.Type }).ToList()
                });
            }

            return new PagedResult<ResourceObject<MangaAttributesDto>>(resourceObjects, pagedMangas.Total, request.Offset, request.Limit);
        }
    }
}
```

```markdown
// Application/Common/Responses/ApiCollectionResponse.cs
using Application.Common.DTOs; // Cần cho PagedResult
using System.Collections.Generic;
using System.Text.Json.Serialization;
// using Application.Common.Models; // Không cần trực tiếp ở đây nếu TData đã là ResourceObject

namespace Application.Common.Responses
{
    /// <summary>
    /// Response cho các API trả về một danh sách (collection) dữ liệu, thường có phân trang.
    /// </summary>
    /// <typeparam name="TData">Kiểu dữ liệu của các item trong danh sách, thường là ResourceObject<TAttributes>.</typeparam>
    public class ApiCollectionResponse<TData> : ApiResponse 
    {
        [JsonPropertyOrder(2)] 
        [JsonPropertyName("response")]
        public string ResponseType { get; set; } = "collection";

        [JsonPropertyOrder(3)] 
        [JsonPropertyName("data")]
        public List<TData> Data { get; set; }

        [JsonPropertyOrder(4)] 
        [JsonPropertyName("limit")]
        public int Limit { get; set; }

        [JsonPropertyOrder(5)] 
        [JsonPropertyName("offset")]
        public int Offset { get; set; }

        [JsonPropertyOrder(6)] 
        [JsonPropertyName("total")]
        public int Total { get; set; }

        public ApiCollectionResponse(List<TData> data, int total, int offset, int limit)
            : base("ok") 
        {
            Data = data;
            Total = total;
            Offset = offset;
            Limit = limit;
        }

        // Constructor để nhận PagedResult<TData> trực tiếp
        // TData ở đây đã là ResourceObject<TAttributesDto> từ PagedResult
        public ApiCollectionResponse(PagedResult<TData> pagedResult) 
            : this(pagedResult.Items, pagedResult.Total, pagedResult.Offset, pagedResult.Limit)
        {
        }
    }
}
```

```markdown
// Application/Common/Responses/ApiResponse.cs
using System.Text.Json.Serialization;
// using Application.Common.Models; // Không cần trực tiếp ở đây nếu TData đã là ResourceObject

namespace Application.Common.Responses
{
    public class ApiResponse
    {
        [JsonPropertyOrder(1)]
        [JsonPropertyName("result")]
        public string Result { get; set; } = "ok";

        public ApiResponse() { }

        public ApiResponse(string result)
        {
            Result = result;
        }
    }

    /// <summary>
    /// Response chung cho các API trả về một thực thể đơn lẻ.
    /// </summary>
    /// <typeparam name="TData">Kiểu dữ liệu của data payload, thường là ResourceObject<TAttributes>.</typeparam>
    public class ApiResponse<TData> : ApiResponse
    {
        [JsonPropertyOrder(2)]
        [JsonPropertyName("response")]
        public string ResponseType { get; set; } = "entity";

        [JsonPropertyOrder(3)]
        [JsonPropertyName("data")]
        public TData Data { get; set; }

        public ApiResponse(TData data, string responseType = "entity") : base()
        {
            Data = data;
            ResponseType = responseType;
        }
    }
}
```

```markdown
// Application/Common/DTOs/PagedResult.cs
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Application.Common.DTOs
{
    public class PagedResult<T>
    {
        public List<T> Items { get; set; }

        [JsonPropertyName("limit")]
        public int Limit { get; set; }

        [JsonPropertyName("offset")]
        public int Offset { get; set; }

        [JsonPropertyName("total")]
        public int Total { get; set; }

        // Thêm constructor mặc định cho các trường hợp cần thiết (ví dụ: deserialization)
        public PagedResult()
        {
            Items = new List<T>();
            Limit = 0;
            Offset = 0;
            Total = 0;
        }
        
        public PagedResult(List<T> items, int total, int offset, int limit)
        {
            Items = items;
            Total = total;
            Offset = offset;
            Limit = limit;
        }
    }
}
```