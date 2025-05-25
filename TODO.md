### Bước 2: Cập nhật các API Controllers để thực hiện validation thủ công

Bây giờ, bạn cần cập nhật các action trong Controller (thường là `POST` và `PUT` nhận DTO từ request body) để gọi validator một cách tường minh.

Các validator cho DTOs (ví dụ: `CreateAuthorDtoValidator`) đã được bạn định nghĩa trong thư mục `Application/Validation/` và đã được inject vào các controllers tương ứng. Chúng ta chỉ cần thêm logic gọi validate.

#### 2.1. Cập nhật `AuthorsController.cs`

```csharp
// MangaReaderDB/Controllers/AuthorsController.cs
using Application.Common.DTOs;
using Application.Common.DTOs.Authors;
using Application.Features.Authors.Commands.CreateAuthor;
using Application.Features.Authors.Commands.DeleteAuthor;
using Application.Features.Authors.Commands.UpdateAuthor;
using Application.Features.Authors.Queries.GetAuthorById;
using Application.Features.Authors.Queries.GetAuthors;
using FluentValidation;
using MediatR; // For Unit
using Microsoft.AspNetCore.Mvc;
using System.Linq; // Required for .Select on validationResult.Errors

namespace MangaReaderDB.Controllers
{
    public class AuthorsController : BaseApiController
    {
        private readonly IValidator<CreateAuthorDto> _createAuthorDtoValidator;
        private readonly IValidator<UpdateAuthorDto> _updateAuthorDtoValidator;

        public AuthorsController(
            IValidator<CreateAuthorDto> createAuthorDtoValidator,
            IValidator<UpdateAuthorDto> updateAuthorDtoValidator)
        {
            _createAuthorDtoValidator = createAuthorDtoValidator;
            _updateAuthorDtoValidator = updateAuthorDtoValidator;
        }

        [HttpPost]
        [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateAuthor([FromBody] CreateAuthorDto createAuthorDto)
        {
            var validationResult = await _createAuthorDtoValidator.ValidateAsync(createAuthorDto);
            if (!validationResult.IsValid)
            {
                // Trả về lỗi validation dưới dạng một đối tượng dễ xử lý ở client
                var errors = validationResult.Errors.Select(e => new { e.PropertyName, e.ErrorMessage });
                return BadRequest(new { Title = "Validation Failed", Errors = errors });
            }

            var command = new CreateAuthorCommand 
            { 
                Name = createAuthorDto.Name, 
                Biography = createAuthorDto.Biography 
            };
            var authorId = await Mediator.Send(command);
            return CreatedAtAction(nameof(GetAuthorById), new { id = authorId }, new { AuthorId = authorId });
        }

        // ... GetAuthorById và GetAuthors không thay đổi ...

        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateAuthor(Guid id, [FromBody] UpdateAuthorDto updateAuthorDto)
        {
            var validationResult = await _updateAuthorDtoValidator.ValidateAsync(updateAuthorDto);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => new { e.PropertyName, e.ErrorMessage });
                return BadRequest(new { Title = "Validation Failed", Errors = errors });
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

        // ... DeleteAuthor không thay đổi ...
    }
} 
```

#### 2.2. Cập nhật `MangasController.cs`

```csharp
// MangaReaderDB/Controllers/MangasController.cs
using Application.Common.DTOs;
using Application.Common.DTOs.Mangas;
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
using System.Linq; // Required for .Select on validationResult.Errors

namespace MangaReaderDB.Controllers
{
    public class MangasController : BaseApiController
    {
        private readonly IValidator<CreateMangaDto> _createMangaDtoValidator;
        private readonly IValidator<UpdateMangaDto> _updateMangaDtoValidator;
        // Nếu MangaTagInputDto và MangaAuthorInputDto có validator riêng, bạn cũng cần inject chúng.
        // Hiện tại, chúng khá đơn giản và có thể validate trực tiếp trong action nếu cần.

        public MangasController(
            IValidator<CreateMangaDto> createMangaDtoValidator,
            IValidator<UpdateMangaDto> updateMangaDtoValidator)
        {
            _createMangaDtoValidator = createMangaDtoValidator;
            _updateMangaDtoValidator = updateMangaDtoValidator;
        }

        [HttpPost]
        [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateManga([FromBody] CreateMangaDto createDto)
        {
            var validationResult = await _createMangaDtoValidator.ValidateAsync(createDto);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => new { e.PropertyName, e.ErrorMessage });
                return BadRequest(new { Title = "Validation Failed", Errors = errors });
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
            var id = await Mediator.Send(command);
            return CreatedAtAction(nameof(GetMangaById), new { id }, new { MangaId = id });
        }

        // ... GetMangaById và GetMangas không thay đổi ...

        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateManga(Guid id, [FromBody] UpdateMangaDto updateDto)
        {
            var validationResult = await _updateMangaDtoValidator.ValidateAsync(updateDto);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => new { e.PropertyName, e.ErrorMessage });
                return BadRequest(new { Title = "Validation Failed", Errors = errors });
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

        // ... DeleteManga không thay đổi ...
        // ... AddMangaTag, RemoveMangaTag, AddMangaAuthor, RemoveMangaAuthor:
        // Các DTO input (MangaTagInputDto, MangaAuthorInputDto) hiện tại rất đơn giản.
        // Nếu chúng có validator riêng (ví dụ: Application/Validation/Mangas/MangaTagInputDtoValidator.cs),
        // bạn cũng cần inject và validate tương tự.
        // Nếu không, bạn có thể thêm các kiểm tra đơn giản trực tiếp trong action.
        // Ví dụ cho AddMangaTag:
        [HttpPost("{mangaId:guid}/tags")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddMangaTag(Guid mangaId, [FromBody] MangaTagInputDto input)
        {
            if (input.TagId == Guid.Empty) 
            {
                return BadRequest(new { Title = "Validation Failed", Errors = new[] { new { PropertyName = nameof(input.TagId), ErrorMessage = "TagId is required." } } });
            }
            var command = new AddMangaTagCommand { MangaId = mangaId, TagId = input.TagId };
            await Mediator.Send(command);
            return NoContent();
        }
        
        [HttpPost("{mangaId:guid}/authors")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddMangaAuthor(Guid mangaId, [FromBody] MangaAuthorInputDto input)
        {
            if (input.AuthorId == Guid.Empty)
            {
                 return BadRequest(new { Title = "Validation Failed", Errors = new[] { new { PropertyName = nameof(input.AuthorId), ErrorMessage = "AuthorId is required." } } });
            }
            // Bạn có thể thêm validation cho Enum MangaStaffRole nếu cần
            if (!Enum.IsDefined(typeof(MangaStaffRole), input.Role))
            {
                return BadRequest(new { Title = "Validation Failed", Errors = new[] { new { PropertyName = nameof(input.Role), ErrorMessage = "Invalid Role." } } });
            }
            var command = new AddMangaAuthorCommand { MangaId = mangaId, AuthorId = input.AuthorId, Role = input.Role };
            await Mediator.Send(command);
            return NoContent();
        }
        // ... các action khác tương tự ...
    }
} 
```

#### 2.3. Cập nhật `ChaptersController.cs` và `ChapterPagesController`

```csharp
// MangaReaderDB/Controllers/ChaptersController.cs
using Application.Common.DTOs;
using Application.Common.DTOs.Chapters;
// ... (các using khác)
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using System.Linq; // Required

namespace MangaReaderDB.Controllers
{
    public class ChaptersController : BaseApiController
    {
        private readonly IValidator<CreateChapterDto> _createChapterDtoValidator;
        private readonly IValidator<UpdateChapterDto> _updateChapterDtoValidator;
        private readonly IValidator<CreateChapterPageDto> _createChapterPageDtoValidator; 
        // UpdateChapterPageDtoValidator sẽ được inject vào ChapterPagesController

        public ChaptersController(
            IValidator<CreateChapterDto> createChapterDtoValidator,
            IValidator<UpdateChapterDto> updateChapterDtoValidator,
            IValidator<CreateChapterPageDto> createChapterPageDtoValidator)
        {
            _createChapterDtoValidator = createChapterDtoValidator;
            _updateChapterDtoValidator = updateChapterDtoValidator;
            _createChapterPageDtoValidator = createChapterPageDtoValidator;
        }

        [HttpPost]
        [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateChapter([FromBody] CreateChapterDto createDto)
        {
            var validationResult = await _createChapterDtoValidator.ValidateAsync(createDto);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => new { e.PropertyName, e.ErrorMessage });
                return BadRequest(new { Title = "Validation Failed", Errors = errors });
            }
            // ... (phần còn lại của action)
            var command = new CreateChapterCommand
            {
                TranslatedMangaId = createDto.TranslatedMangaId,
                UploadedByUserId = createDto.UploadedByUserId, // Tạm thời lấy từ DTO
                Volume = createDto.Volume,
                ChapterNumber = createDto.ChapterNumber,
                Title = createDto.Title,
                PublishAt = createDto.PublishAt,
                ReadableAt = createDto.ReadableAt
            };
            var id = await Mediator.Send(command);
            return CreatedAtAction(nameof(GetChapterById), new { id }, new { ChapterId = id });
        }

        // ... GetChapterById, GetChaptersByTranslatedManga không thay đổi ...

        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateChapter(Guid id, [FromBody] UpdateChapterDto updateDto)
        {
            var validationResult = await _updateChapterDtoValidator.ValidateAsync(updateDto);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => new { e.PropertyName, e.ErrorMessage });
                return BadRequest(new { Title = "Validation Failed", Errors = errors });
            }
            // ... (phần còn lại của action)
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

        // ... DeleteChapter không thay đổi ...

        [HttpPost("{chapterId:guid}/pages/entry")]
        [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreateChapterPageEntry(Guid chapterId, [FromBody] CreateChapterPageDto createPageDto)
        {
            var validationResult = await _createChapterPageDtoValidator.ValidateAsync(createPageDto);
            if (!validationResult.IsValid)
            {
                 var errors = validationResult.Errors.Select(e => new { e.PropertyName, e.ErrorMessage });
                return BadRequest(new { Title = "Validation Failed", Errors = errors });
            }
            // ... (phần còn lại của action)
            var command = new CreateChapterPageEntryCommand 
            { 
                ChapterId = chapterId, 
                PageNumber = createPageDto.PageNumber
            };
            var pageId = await Mediator.Send(command);
            return CreatedAtAction("UploadChapterPageImage", "ChapterPages", new { pageId = pageId }, new { PageId = pageId });
        }
        
        // ... GetChapterPages không thay đổi ...
    }

    [Route("api/chapterpages")]
    public class ChapterPagesController : BaseApiController
    {
        private readonly IValidator<UpdateChapterPageDto> _updateChapterPageDtoValidator;

        public ChapterPagesController(IValidator<UpdateChapterPageDto> updateChapterPageDtoValidator)
        {
            _updateChapterPageDtoValidator = updateChapterPageDtoValidator;
        }

        // ... UploadChapterPageImage không thay đổi (validation IFormFile làm trực tiếp) ...

        [HttpPut("{pageId:guid}/details")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateChapterPageDetails(Guid pageId, [FromBody] UpdateChapterPageDto updateDto)
        {
            var validationResult = await _updateChapterPageDtoValidator.ValidateAsync(updateDto);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => new { e.PropertyName, e.ErrorMessage });
                return BadRequest(new { Title = "Validation Failed", Errors = errors });
            }
            // ... (phần còn lại của action)
            var command = new UpdateChapterPageDetailsCommand
            {
                PageId = pageId,
                PageNumber = updateDto.PageNumber
            };
            await Mediator.Send(command);
            return NoContent();
        }

        // ... DeleteChapterPage không thay đổi ...
    }
} 
```

#### 2.4. Cập nhật `CoverArtsController.cs`

Action `UploadCoverArtImage` nhận `IFormFile` và các tham số riêng lẻ, không phải một DTO phức tạp từ body. `CreateCoverArtDtoValidator` được inject nhưng chưa được sử dụng. Nếu bạn muốn validate `volume` và `description`, bạn có thể làm như sau:

```csharp
// MangaReaderDB/Controllers/CoverArtsController.cs
using Application.Common.DTOs.CoverArts;
// ... (các using khác)
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using System.Linq; // Required

namespace MangaReaderDB.Controllers
{
    public class CoverArtsController : BaseApiController
    {
        private readonly IValidator<CreateCoverArtDto> _createCoverArtDtoValidator;

        public CoverArtsController(IValidator<CreateCoverArtDto> createCoverArtDtoValidator)
        {
            _createCoverArtDtoValidator = createCoverArtDtoValidator;
        }

        [HttpPost("/api/mangas/{mangaId:guid}/covers")]
        [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UploadCoverArtImage(Guid mangaId, IFormFile file, [FromForm] string? volume, [FromForm] string? description) // Sử dụng FromForm cho metadata
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { Title = "Validation Failed", Errors = new[] { new { PropertyName = "file", ErrorMessage = "File is required." } } });
            }
             if (file.Length > 5 * 1024 * 1024) // Giới hạn 5MB
            {
                return BadRequest(new { Title = "Validation Failed", Errors = new[] { new { PropertyName = "file", ErrorMessage = "File size cannot exceed 5MB." } } });
            }
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(ext) || !allowedExtensions.Contains(ext))
            {
                 return BadRequest(new { Title = "Validation Failed", Errors = new[] { new { PropertyName = "file", ErrorMessage = "Invalid file type. Allowed types are: " + string.Join(", ", allowedExtensions) } } });
            }


            // Validate metadata (volume, description) bằng CreateCoverArtDtoValidator
            var metadataDto = new CreateCoverArtDto { MangaId = mangaId, Volume = volume, Description = description };
            var validationResult = await _createCoverArtDtoValidator.ValidateAsync(metadataDto);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => new { e.PropertyName, e.ErrorMessage });
                return BadRequest(new { Title = "Validation Failed", Errors = errors });
            }

            using var stream = file.OpenReadStream();
            var command = new UploadCoverArtImageCommand
            {
                MangaId = mangaId,
                Volume = volume, // metadataDto.Volume
                Description = description, // metadataDto.Description
                ImageStream = stream,
                OriginalFileName = file.FileName,
                ContentType = file.ContentType
            };

            var coverId = await Mediator.Send(command);
            return CreatedAtAction(nameof(GetCoverArtById), new { id = coverId }, new { CoverId = coverId });
        }

        // ... các actions khác không thay đổi ...
    }
}
```
**Lưu ý:** Bạn cần đảm bảo `CreateCoverArtDtoValidator` (`Application/Validation/CoverArts/CreateCoverArtDtoValidator.cs`) được định nghĩa đúng cách để validate `Volume` và `Description`. File này đã được cung cấp trong context.

#### 2.5. Cập nhật `TagGroupsController.cs`

```csharp
// MangaReaderDB/Controllers/TagGroupsController.cs
using Application.Common.DTOs.TagGroups;
// ... (các using khác)
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using System.Linq; // Required

namespace MangaReaderDB.Controllers
{
    public class TagGroupsController : BaseApiController
    {
        private readonly IValidator<CreateTagGroupDto> _createTagGroupDtoValidator;
        private readonly IValidator<UpdateTagGroupDto> _updateTagGroupDtoValidator;

        public TagGroupsController(
            IValidator<CreateTagGroupDto> createTagGroupDtoValidator,
            IValidator<UpdateTagGroupDto> updateTagGroupDtoValidator)
        {
            _createTagGroupDtoValidator = createTagGroupDtoValidator;
            _updateTagGroupDtoValidator = updateTagGroupDtoValidator;
        }

        [HttpPost]
        [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateTagGroup([FromBody] CreateTagGroupDto createDto)
        {
            var validationResult = await _createTagGroupDtoValidator.ValidateAsync(createDto);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => new { e.PropertyName, e.ErrorMessage });
                return BadRequest(new { Title = "Validation Failed", Errors = errors });
            }
            // ... (phần còn lại của action)
            var command = new CreateTagGroupCommand { Name = createDto.Name };
            var id = await Mediator.Send(command);
            return CreatedAtAction(nameof(GetTagGroupById), new { id }, new { TagGroupId = id });
        }

        // ... GetTagGroupById, GetTagGroups không thay đổi ...

        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateTagGroup(Guid id, [FromBody] UpdateTagGroupDto updateDto)
        {
            var validationResult = await _updateTagGroupDtoValidator.ValidateAsync(updateDto);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => new { e.PropertyName, e.ErrorMessage });
                return BadRequest(new { Title = "Validation Failed", Errors = errors });
            }
            // ... (phần còn lại của action)
            var command = new UpdateTagGroupCommand { TagGroupId = id, Name = updateDto.Name };
            await Mediator.Send(command);
            return NoContent();
        }
        
        // ... DeleteTagGroup không thay đổi (vì không nhận DTO từ body) ...
    }
}
```

#### 2.6. Cập nhật `TagsController.cs`

```csharp
// MangaReaderDB/Controllers/TagsController.cs
using Application.Common.DTOs.Tags;
// ... (các using khác)
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using System.Linq; // Required

namespace MangaReaderDB.Controllers
{
    public class TagsController : BaseApiController
    {
        private readonly IValidator<CreateTagDto> _createTagDtoValidator;
        private readonly IValidator<UpdateTagDto> _updateTagDtoValidator;

        public TagsController(
            IValidator<CreateTagDto> createTagDtoValidator,
            IValidator<UpdateTagDto> updateTagDtoValidator)
        {
            _createTagDtoValidator = createTagDtoValidator;
            _updateTagDtoValidator = updateTagDtoValidator;
        }

        [HttpPost]
        [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateTag([FromBody] CreateTagDto createDto)
        {
            var validationResult = await _createTagDtoValidator.ValidateAsync(createDto);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => new { e.PropertyName, e.ErrorMessage });
                return BadRequest(new { Title = "Validation Failed", Errors = errors });
            }
            // ... (phần còn lại của action)
            var command = new CreateTagCommand { Name = createDto.Name, TagGroupId = createDto.TagGroupId };
            var id = await Mediator.Send(command);
            return CreatedAtAction(nameof(GetTagById), new { id }, new { TagId = id });
        }

        // ... GetTagById, GetTags không thay đổi ...

        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateTag(Guid id, [FromBody] UpdateTagDto updateDto)
        {
            var validationResult = await _updateTagDtoValidator.ValidateAsync(updateDto);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => new { e.PropertyName, e.ErrorMessage });
                return BadRequest(new { Title = "Validation Failed", Errors = errors });
            }
            // ... (phần còn lại của action)
            var command = new UpdateTagCommand { TagId = id, Name = updateDto.Name, TagGroupId = updateDto.TagGroupId };
            await Mediator.Send(command);
            return NoContent();
        }

        // ... DeleteTag không thay đổi ...
    }
}
```

#### 2.7. Cập nhật `TranslatedMangasController.cs`

```csharp
// MangaReaderDB/Controllers/TranslatedMangasController.cs
using Application.Common.DTOs.TranslatedMangas;
// ... (các using khác)
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using System.Linq; // Required

namespace MangaReaderDB.Controllers
{
    public class TranslatedMangasController : BaseApiController
    {
        private readonly IValidator<CreateTranslatedMangaDto> _createDtoValidator;
        private readonly IValidator<UpdateTranslatedMangaDto> _updateDtoValidator;

        public TranslatedMangasController(
            IValidator<CreateTranslatedMangaDto> createDtoValidator,
            IValidator<UpdateTranslatedMangaDto> updateDtoValidator)
        {
            _createDtoValidator = createDtoValidator;
            _updateDtoValidator = updateDtoValidator;
        }

        [HttpPost]
        [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateTranslatedManga([FromBody] CreateTranslatedMangaDto createDto)
        {
            var validationResult = await _createDtoValidator.ValidateAsync(createDto);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => new { e.PropertyName, e.ErrorMessage });
                return BadRequest(new { Title = "Validation Failed", Errors = errors });
            }
            // ... (phần còn lại của action)
            var command = new CreateTranslatedMangaCommand
            {
                MangaId = createDto.MangaId,
                LanguageKey = createDto.LanguageKey,
                Title = createDto.Title,
                Description = createDto.Description
            };
            var id = await Mediator.Send(command);
            return CreatedAtAction(nameof(GetTranslatedMangaById), new { id }, new { TranslatedMangaId = id });
        }

        // ... GetTranslatedMangaById, GetTranslatedMangasByManga không thay đổi ...

        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateTranslatedManga(Guid id, [FromBody] UpdateTranslatedMangaDto updateDto)
        {
            var validationResult = await _updateDtoValidator.ValidateAsync(updateDto);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => new { e.PropertyName, e.ErrorMessage });
                return BadRequest(new { Title = "Validation Failed", Errors = errors });
            }
            // ... (phần còn lại của action)
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
        
        // ... DeleteTranslatedManga không thay đổi ...
    }
}
```

### Bước 3: Kiểm tra và Hoàn tất

Sau khi thực hiện các thay đổi trên:
1.  Build lại project.
2.  Chạy API và kiểm tra các endpoint đã được cập nhật.
    *   Thử gửi request với dữ liệu hợp lệ.
    *   Thử gửi request với dữ liệu không hợp lệ để xem API có trả về `400 Bad Request` với danh sách lỗi validation như mong đợi không.

Với những thay đổi này, bạn đã chuyển sang sử dụng FluentValidation theo cách thủ công, cho phép bạn kiểm soát hoàn toàn quá trình validation trong các controllers.
```