# TODO.md

## Bước 3.4.1: Triển khai Commands & Command Handlers

Phần này sẽ bao gồm việc định nghĩa các lớp Command và các lớp Handler tương ứng cho từng nghiệp vụ.

### 1. Features/Authors

#### 1.1. CreateAuthor

```csharp
// Application/Features/Authors/Commands/CreateAuthor/CreateAuthorCommand.cs
using MediatR;

namespace Application.Features.Authors.Commands.CreateAuthor
{
    public class CreateAuthorCommand : IRequest<Guid>
    {
        public string Name { get; set; } = string.Empty;
        public string? Biography { get; set; }
    }
}
```

```csharp
// Application/Features/Authors/Commands/CreateAuthor/CreateAuthorCommandHandler.cs
using Application.Common.Interfaces;
using Application.Contracts.Persistence;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging; // Thêm logging nếu cần

namespace Application.Features.Authors.Commands.CreateAuthor
{
    public class CreateAuthorCommandHandler : IRequestHandler<CreateAuthorCommand, Guid>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<CreateAuthorCommandHandler> _logger; // Ví dụ logging

        public CreateAuthorCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<CreateAuthorCommandHandler> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Guid> Handle(CreateAuthorCommand request, CancellationToken cancellationToken)
        {
            // Kiểm tra xem tác giả đã tồn tại chưa (nếu cần logic nghiệp vụ này)
            var existingAuthor = await _unitOfWork.AuthorRepository.GetAuthorByNameAsync(request.Name);
            if (existingAuthor != null)
            {
                _logger.LogWarning("Author with name {AuthorName} already exists.", request.Name);
                // Có thể throw một custom exception hoặc trả về một kết quả lỗi cụ thể
                // Ví dụ: throw new Exceptions.ValidationException($"Author with name '{request.Name}' already exists.");
                // Hoặc nếu API trả về ID của author đã tồn tại thì: return existingAuthor.AuthorId;
                // Trong ví dụ này, chúng ta sẽ tạo mới và để DB constraint (nếu có) xử lý (hoặc không cho phép trùng tên)
                // Tùy thuộc vào yêu cầu cụ thể của bạn.
                // Hiện tại, cứ tạo mới.
            }

            var author = _mapper.Map<Author>(request);

            await _unitOfWork.AuthorRepository.AddAsync(author);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Author {AuthorId} created successfully.", author.AuthorId);
            return author.AuthorId;
        }
    }
}
```

#### 1.2. UpdateAuthor

```csharp
// Application/Features/Authors/Commands/UpdateAuthor/UpdateAuthorCommand.cs
using MediatR;

namespace Application.Features.Authors.Commands.UpdateAuthor
{
    public class UpdateAuthorCommand : IRequest<Unit> // Hoặc IRequest<AuthorDto> nếu muốn trả về author đã cập nhật
    {
        public Guid AuthorId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Biography { get; set; }
    }
}
```

```csharp
// Application/Features/Authors/Commands/UpdateAuthor/UpdateAuthorCommandHandler.cs
using Application.Contracts.Persistence;
using Application.Exceptions; // Thêm namespace này nếu bạn tạo custom exceptions
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Authors.Commands.UpdateAuthor
{
    public class UpdateAuthorCommandHandler : IRequestHandler<UpdateAuthorCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<UpdateAuthorCommandHandler> _logger;

        public UpdateAuthorCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<UpdateAuthorCommandHandler> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Unit> Handle(UpdateAuthorCommand request, CancellationToken cancellationToken)
        {
            var authorToUpdate = await _unitOfWork.AuthorRepository.GetByIdAsync(request.AuthorId);

            if (authorToUpdate == null)
            {
                _logger.LogWarning("Author with ID {AuthorId} not found for update.", request.AuthorId);
                throw new NotFoundException(nameof(Domain.Entities.Author), request.AuthorId);
            }

            // Kiểm tra xem có tác giả khác trùng tên không (nếu tên thay đổi và không cho phép trùng tên)
            if (!string.Equals(authorToUpdate.Name, request.Name, StringComparison.OrdinalIgnoreCase))
            {
                var existingAuthorWithNewName = await _unitOfWork.AuthorRepository.GetAuthorByNameAsync(request.Name);
                if (existingAuthorWithNewName != null && existingAuthorWithNewName.AuthorId != request.AuthorId)
                {
                    _logger.LogWarning("Another author with name {AuthorName} already exists.", request.Name);
                    // throw new Exceptions.ValidationException($"Another author with name '{request.Name}' already exists.");
                }
            }

            _mapper.Map(request, authorToUpdate); // Map từ command vào entity đã tồn tại

            await _unitOfWork.AuthorRepository.UpdateAsync(authorToUpdate);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Author {AuthorId} updated successfully.", request.AuthorId);
            return Unit.Value;
        }
    }
}
```

#### 1.3. DeleteAuthor

```csharp
// Application/Features/Authors/Commands/DeleteAuthor/DeleteAuthorCommand.cs
using MediatR;

namespace Application.Features.Authors.Commands.DeleteAuthor
{
    public class DeleteAuthorCommand : IRequest<Unit>
    {
        public Guid AuthorId { get; set; }
    }
}
```

```csharp
// Application/Features/Authors/Commands/DeleteAuthor/DeleteAuthorCommandHandler.cs
using Application.Contracts.Persistence;
using Application.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Authors.Commands.DeleteAuthor
{
    public class DeleteAuthorCommandHandler : IRequestHandler<DeleteAuthorCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DeleteAuthorCommandHandler> _logger;

        public DeleteAuthorCommandHandler(IUnitOfWork unitOfWork, ILogger<DeleteAuthorCommandHandler> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Unit> Handle(DeleteAuthorCommand request, CancellationToken cancellationToken)
        {
            var authorToDelete = await _unitOfWork.AuthorRepository.GetByIdAsync(request.AuthorId);

            if (authorToDelete == null)
            {
                _logger.LogWarning("Author with ID {AuthorId} not found for deletion.", request.AuthorId);
                throw new NotFoundException(nameof(Domain.Entities.Author), request.AuthorId);
            }

            // Cân nhắc nghiệp vụ: có cho phép xóa tác giả nếu đang được gán cho manga không?
            // Nếu có MangaAuthors liên quan, bạn có thể muốn ngăn chặn việc xóa hoặc xử lý logic liên quan.
            // Ví dụ:
            // var mangaAuthors = await _unitOfWork.MangaRepository.HasAuthorAssociatedAsync(request.AuthorId);
            // if (mangaAuthors)
            // {
            //     throw new Exceptions.DeleteFailureException(nameof(Domain.Entities.Author), request.AuthorId, "Author is associated with existing mangas.");
            // }

            await _unitOfWork.AuthorRepository.DeleteAsync(authorToDelete);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Author {AuthorId} deleted successfully.", request.AuthorId);
            return Unit.Value;
        }
    }
}
```

### 2. Features/Mangas

#### 2.1. CreateManga

```csharp
// Application/Features/Mangas/Commands/CreateManga/CreateMangaCommand.cs
using Domain.Enums;
using MediatR;

namespace Application.Features.Mangas.Commands.CreateManga
{
    public class CreateMangaCommand : IRequest<Guid>
    {
        public string Title { get; set; } = string.Empty;
        public string OriginalLanguage { get; set; } = string.Empty; // ISO 639-1 code
        public PublicationDemographic? PublicationDemographic { get; set; }
        public MangaStatus Status { get; set; }
        public int? Year { get; set; }
        public ContentRating ContentRating { get; set; }
        
        // Tags và Authors sẽ được thêm qua các command riêng (AddMangaTagCommand, AddMangaAuthorCommand)
        // sau khi Manga được tạo.
    }
}
```

```csharp
// Application/Features/Mangas/Commands/CreateManga/CreateMangaCommandHandler.cs
using Application.Contracts.Persistence;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Mangas.Commands.CreateManga
{
    public class CreateMangaCommandHandler : IRequestHandler<CreateMangaCommand, Guid>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<CreateMangaCommandHandler> _logger;

        public CreateMangaCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<CreateMangaCommandHandler> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Guid> Handle(CreateMangaCommand request, CancellationToken cancellationToken)
        {
            // Kiểm tra xem Manga đã tồn tại chưa (ví dụ theo Title và OriginalLanguage)
            // var existingManga = await _unitOfWork.MangaRepository.FindFirstOrDefaultAsync(
            //     m => m.Title == request.Title && m.OriginalLanguage == request.OriginalLanguage
            // );
            // if (existingManga != null)
            // {
            //     _logger.LogWarning("Manga with title '{MangaTitle}' and language '{OriginalLanguage}' already exists.", request.Title, request.OriginalLanguage);
            //     throw new Exceptions.ValidationException($"Manga with title '{request.Title}' and language '{request.OriginalLanguage}' already exists.");
            // }

            var manga = _mapper.Map<Manga>(request);
            // IsLocked mặc định là false khi tạo mới, không cần gán lại trừ khi có logic khác

            await _unitOfWork.MangaRepository.AddAsync(manga);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Manga {MangaId} created successfully.", manga.MangaId);
            return manga.MangaId;
        }
    }
}
```

#### 2.2. UpdateManga

```csharp
// Application/Features/Mangas/Commands/UpdateManga/UpdateMangaCommand.cs
using Domain.Enums;
using MediatR;

namespace Application.Features.Mangas.Commands.UpdateManga
{
    public class UpdateMangaCommand : IRequest<Unit>
    {
        public Guid MangaId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string OriginalLanguage { get; set; } = string.Empty;
        public PublicationDemographic? PublicationDemographic { get; set; }
        public MangaStatus Status { get; set; }
        public int? Year { get; set; }
        public ContentRating ContentRating { get; set; }
        public bool IsLocked { get; set; }
    }
}
```

```csharp
// Application/Features/Mangas/Commands/UpdateManga/UpdateMangaCommandHandler.cs
using Application.Contracts.Persistence;
using Application.Exceptions;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Mangas.Commands.UpdateManga
{
    public class UpdateMangaCommandHandler : IRequestHandler<UpdateMangaCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<UpdateMangaCommandHandler> _logger;

        public UpdateMangaCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<UpdateMangaCommandHandler> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Unit> Handle(UpdateMangaCommand request, CancellationToken cancellationToken)
        {
            var mangaToUpdate = await _unitOfWork.MangaRepository.GetByIdAsync(request.MangaId);

            if (mangaToUpdate == null)
            {
                _logger.LogWarning("Manga with ID {MangaId} not found for update.", request.MangaId);
                throw new NotFoundException(nameof(Domain.Entities.Manga), request.MangaId);
            }

            // Kiểm tra IsLocked: Nếu Manga bị khóa, có thể không cho phép một số thay đổi nhất định (tùy logic nghiệp vụ)
            // if (mangaToUpdate.IsLocked && (mangaToUpdate.Title != request.Title /* ... các trường khác ... */))
            // {
            //     _logger.LogWarning("Attempted to update a locked manga {MangaId}.", request.MangaId);
            //     throw new Exceptions.ValidationException("Cannot update a locked manga's details.");
            // }

            _mapper.Map(request, mangaToUpdate);

            await _unitOfWork.MangaRepository.UpdateAsync(mangaToUpdate);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Manga {MangaId} updated successfully.", request.MangaId);
            return Unit.Value;
        }
    }
}
```

#### 2.3. DeleteManga

```csharp
// Application/Features/Mangas/Commands/DeleteManga/DeleteMangaCommand.cs
using MediatR;

namespace Application.Features.Mangas.Commands.DeleteManga
{
    public class DeleteMangaCommand : IRequest<Unit>
    {
        public Guid MangaId { get; set; }
    }
}
```

```csharp
// Application/Features/Mangas/Commands/DeleteManga/DeleteMangaCommandHandler.cs
using Application.Contracts.Persistence;
using Application.Exceptions;
using Application.Common.Interfaces; // Cho IPhotoAccessor
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore; // Cho ToListAsync, etc.

namespace Application.Features.Mangas.Commands.DeleteManga
{
    public class DeleteMangaCommandHandler : IRequestHandler<DeleteMangaCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPhotoAccessor _photoAccessor;
        private readonly ILogger<DeleteMangaCommandHandler> _logger;

        public DeleteMangaCommandHandler(IUnitOfWork unitOfWork, IPhotoAccessor photoAccessor, ILogger<DeleteMangaCommandHandler> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _photoAccessor = photoAccessor ?? throw new ArgumentNullException(nameof(photoAccessor));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Unit> Handle(DeleteMangaCommand request, CancellationToken cancellationToken)
        {
            // Lấy manga với các thông tin liên quan cần xóa (CoverArts, Chapters -> ChapterPages)
            var mangaToDelete = await _unitOfWork.MangaRepository.GetMangaWithDetailsAsync(request.MangaId);

            if (mangaToDelete == null)
            {
                _logger.LogWarning("Manga with ID {MangaId} not found for deletion.", request.MangaId);
                throw new NotFoundException(nameof(Domain.Entities.Manga), request.MangaId);
            }

            // 1. Xóa ảnh bìa (CoverArts) khỏi Cloudinary và DB
            if (mangaToDelete.CoverArts != null && mangaToDelete.CoverArts.Any())
            {
                foreach (var coverArt in mangaToDelete.CoverArts.ToList()) // ToList() để tránh lỗi khi modify collection
                {
                    if (!string.IsNullOrEmpty(coverArt.PublicId))
                    {
                        var deletionResult = await _photoAccessor.DeletePhotoAsync(coverArt.PublicId);
                        if (deletionResult != "ok" && deletionResult != "not found") // "not found" có thể chấp nhận được
                        {
                            _logger.LogWarning("Failed to delete cover art {PublicId} from Cloudinary for manga {MangaId}. Result: {DeletionResult}", coverArt.PublicId, request.MangaId, deletionResult);
                        }
                    }
                    // CoverArt entities sẽ được xóa cùng Manga do cấu hình Cascade Delete
                }
            }

            // 2. Xóa các trang của chapter (ChapterPages) khỏi Cloudinary và DB
            if (mangaToDelete.TranslatedMangas != null)
            {
                foreach (var translatedManga in mangaToDelete.TranslatedMangas.ToList())
                {
                    var chapters = await _unitOfWork.ChapterRepository.GetChaptersByTranslatedMangaAsync(translatedManga.TranslatedMangaId);
                    foreach (var chapter in chapters.ToList())
                    {
                        var chapterWithPages = await _unitOfWork.ChapterRepository.GetChapterWithPagesAsync(chapter.ChapterId);
                        if (chapterWithPages?.ChapterPages != null && chapterWithPages.ChapterPages.Any())
                        {
                            foreach (var page in chapterWithPages.ChapterPages.ToList())
                            {
                                if (!string.IsNullOrEmpty(page.PublicId))
                                {
                                    var deletionResult = await _photoAccessor.DeletePhotoAsync(page.PublicId);
                                    if (deletionResult != "ok" && deletionResult != "not found")
                                    {
                                        _logger.LogWarning("Failed to delete chapter page {PublicId} from Cloudinary for chapter {ChapterId}. Result: {DeletionResult}", page.PublicId, chapter.ChapterId, deletionResult);
                                    }
                                }
                                // ChapterPage entities sẽ được xóa cùng Chapter do Cascade Delete
                            }
                        }
                        // Chapter entities sẽ được xóa cùng TranslatedManga do Cascade Delete
                    }
                    // TranslatedManga entities sẽ được xóa cùng Manga do Cascade Delete
                }
            }
            
            // 3. Xóa Manga khỏi DB (các bảng liên quan như MangaTag, MangaAuthor, TranslatedManga, Chapter, ChapterPage, CoverArt sẽ tự động xóa theo cấu hình Cascade)
            await _unitOfWork.MangaRepository.DeleteAsync(mangaToDelete);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Manga {MangaId} and its related data deleted successfully.", request.MangaId);
            return Unit.Value;
        }
    }
}
```

#### 2.4. AddMangaTag

```csharp
// Application/Features/Mangas/Commands/AddMangaTag/AddMangaTagCommand.cs
using MediatR;

namespace Application.Features.Mangas.Commands.AddMangaTag
{
    public class AddMangaTagCommand : IRequest<Unit>
    {
        public Guid MangaId { get; set; }
        public Guid TagId { get; set; }
    }
}
```

```csharp
// Application/Features/Mangas/Commands/AddMangaTag/AddMangaTagCommandHandler.cs
using Application.Contracts.Persistence;
using Application.Exceptions;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore; // For checking existing MangaTag

namespace Application.Features.Mangas.Commands.AddMangaTag
{
    public class AddMangaTagCommandHandler : IRequestHandler<AddMangaTagCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AddMangaTagCommandHandler> _logger;

        public AddMangaTagCommandHandler(IUnitOfWork unitOfWork, ILogger<AddMangaTagCommandHandler> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Unit> Handle(AddMangaTagCommand request, CancellationToken cancellationToken)
        {
            var manga = await _unitOfWork.MangaRepository.GetByIdAsync(request.MangaId);
            if (manga == null)
            {
                throw new NotFoundException(nameof(Manga), request.MangaId);
            }

            var tag = await _unitOfWork.TagRepository.GetByIdAsync(request.TagId);
            if (tag == null)
            {
                throw new NotFoundException(nameof(Tag), request.TagId);
            }

            // Kiểm tra xem MangaTag đã tồn tại chưa
            // EF Core không cung cấp phương thức AddIfNotExists trực tiếp cho collection.
            // Ta cần load collection hoặc kiểm tra trước.
            // Cách 1: Load collection (có thể không hiệu quả nếu collection lớn)
            // var mangaWithTags = await _unitOfWork.MangaRepository.GetMangaWithDetailsAsync(request.MangaId); // Lấy manga với tags
            // if (mangaWithTags.MangaTags.Any(mt => mt.TagId == request.TagId))
            // {
            //    _logger.LogInformation("Tag {TagId} already associated with Manga {MangaId}.", request.TagId, request.MangaId);
            //    return Unit.Value; // Hoặc throw lỗi nếu muốn
            // }

            // Cách 2: Kiểm tra trực tiếp trong bảng join MangaTags (cần ApplicationDbContext để truy cập trực tiếp, hoặc thêm phương thức vào IGenericRepository/IMangaRepository)
            // Vì chúng ta dùng UnitOfWork, và không muốn Repository biết về các Repository khác,
            // cách tốt nhất là thêm một phương thức vào IMangaRepository hoặc sử dụng DbSet trực tiếp nếu có trong UnitOfWork.
            // Tuy nhiên, để đơn giản, chúng ta sẽ cố gắng thêm và để DB xử lý constraint (nếu có Unique Key trên MangaId, TagId)
            // Hoặc, nếu ApplicationDbContext được inject vào IUnitOfWork (không phải chỉ các IRepository) thì có thể truy cập
            // _unitOfWork.Context.MangaTags.AnyAsync(...)
            // Hiện tại, chúng ta sẽ thêm một MangaTag mới.
            
            // Kiểm tra xem record đã tồn tại trong bảng MangaTags chưa
            var existingMangaTag = await _unitOfWork.MangaRepository.FindFirstOrDefaultAsync(
                m => m.MangaId == request.MangaId && m.MangaTags.Any(mt => mt.TagId == request.TagId),
                includeProperties: "MangaTags" // Cần include để kiểm tra collection
            );
            
            // Hoặc nếu bạn có IApplicationDbContext trong UnitOfWork (không khuyến khích)
            // var dbContext = (_unitOfWork as UnitOfWork)?.GetContext(); // Cần ép kiểu và GetContext() public
            // if (dbContext != null)
            // {
            //     bool exists = await dbContext.MangaTags.AnyAsync(mt => mt.MangaId == request.MangaId && mt.TagId == request.TagId, cancellationToken);
            //     if (exists)
            //     {
            //         _logger.LogInformation("Tag {TagId} is already associated with Manga {MangaId}.", request.TagId, request.MangaId);
            //         return Unit.Value;
            //     }
            // }
            // Cách đơn giản nhất là cố gắng thêm và dựa vào primary key constraint của MangaTag
            // Nhưng vì MangaTag không có Id riêng, chúng ta cần thêm một bản ghi vào bảng join.

            // Nếu không có MangaTag entity trong UnitOfWork, chúng ta tạo mới
            var mangaTag = new MangaTag { MangaId = request.MangaId, TagId = request.TagId };
            
            // Vì MangaTags là một ICollection trên Manga, ta có thể thêm vào đó
            // Nhưng chúng ta cần đảm bảo không thêm trùng lặp.
            // Cách tốt nhất là thêm một repository riêng cho MangaTag hoặc thêm phương thức vào MangaRepository.
            // Giả sử MangaRepository có phương thức AddTagAsync
            // await _unitOfWork.MangaRepository.AddTagAsync(manga, tag);

            // Hoặc, ta làm thủ công bằng cách thêm vào bảng MangaTags.
            // Cần có DbSet<MangaTag> trong IApplicationDbContext và một repository cho nó, hoặc truy cập qua context.
            // Để giữ cấu trúc hiện tại, ta sẽ tìm manga, rồi thêm tag vào collection của nó nếu chưa có.
            
            // Lấy Manga bao gồm MangaTags
            var mangaEntity = await _unitOfWork.MangaRepository.FindFirstOrDefaultAsync(
                m => m.MangaId == request.MangaId,
                includeProperties: "MangaTags" // Rất quan trọng: phải include collection này
            );

            if (mangaEntity == null) // Should not happen if previous check passed, but good practice
            {
                throw new NotFoundException(nameof(Manga), request.MangaId);
            }

            if (!mangaEntity.MangaTags.Any(mt => mt.TagId == request.TagId))
            {
                mangaEntity.MangaTags.Add(new MangaTag { MangaId = request.MangaId, TagId = request.TagId });
                // UpdateAsync không cần thiết vì EF Core theo dõi thay đổi của collection trên tracked entity mangaEntity
                // await _unitOfWork.MangaRepository.UpdateAsync(mangaEntity); // Không cần thiết nếu mangaEntity đã được track
            }
            else
            {
                _logger.LogInformation("Tag {TagId} is already associated with Manga {MangaId}.", request.TagId, request.MangaId);
                return Unit.Value; // Tag đã được gán, không làm gì thêm
            }
            
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Tag {TagId} added to Manga {MangaId} successfully.", request.TagId, request.MangaId);
            return Unit.Value;
        }
    }
}
```

#### 2.5. RemoveMangaTag

```csharp
// Application/Features/Mangas/Commands/RemoveMangaTag/RemoveMangaTagCommand.cs
using MediatR;

namespace Application.Features.Mangas.Commands.RemoveMangaTag
{
    public class RemoveMangaTagCommand : IRequest<Unit>
    {
        public Guid MangaId { get; set; }
        public Guid TagId { get; set; }
    }
}
```

```csharp
// Application/Features/Mangas/Commands/RemoveMangaTag/RemoveMangaTagCommandHandler.cs
using Application.Contracts.Persistence;
using Application.Exceptions;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore; // For SingleOrDefaultAsync

namespace Application.Features.Mangas.Commands.RemoveMangaTag
{
    public class RemoveMangaTagCommandHandler : IRequestHandler<RemoveMangaTagCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<RemoveMangaTagCommandHandler> _logger;

        public RemoveMangaTagCommandHandler(IUnitOfWork unitOfWork, ILogger<RemoveMangaTagCommandHandler> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Unit> Handle(RemoveMangaTagCommand request, CancellationToken cancellationToken)
        {
            var manga = await _unitOfWork.MangaRepository.GetByIdAsync(request.MangaId);
            if (manga == null)
            {
                throw new NotFoundException(nameof(Manga), request.MangaId);
            }

            var tag = await _unitOfWork.TagRepository.GetByIdAsync(request.TagId);
            if (tag == null)
            {
                throw new NotFoundException(nameof(Tag), request.TagId);
            }

            // Tìm MangaTag entity để xóa. Cần truy cập DbContext hoặc có Repo cho MangaTag.
            // Cách 1: Load collection MangaTags của Manga
            var mangaEntity = await _unitOfWork.MangaRepository.FindFirstOrDefaultAsync(
                m => m.MangaId == request.MangaId,
                includeProperties: "MangaTags" // Rất quan trọng
            );

            if (mangaEntity == null) // Should not happen
            {
                throw new NotFoundException(nameof(Manga), request.MangaId);
            }

            var mangaTagToRemove = mangaEntity.MangaTags.SingleOrDefault(mt => mt.TagId == request.TagId);

            if (mangaTagToRemove != null)
            {
                mangaEntity.MangaTags.Remove(mangaTagToRemove); 
                // Không cần gọi UpdateAsync trên mangaEntity vì EF Core theo dõi thay đổi collection.
                // _context.Set<MangaTag>().Remove(mangaTagToRemove); // Nếu có repo cho MangaTag thì dùng DeleteAsync của nó
            }
            else
            {
                _logger.LogWarning("Tag {TagId} not found on Manga {MangaId} for removal.", request.TagId, request.MangaId);
                // Có thể throw NotFoundException hoặc trả về thành công nếu "không tìm thấy" cũng là một trạng thái chấp nhận được.
                // Trong trường hợp này, ta coi như không có gì để xóa.
                return Unit.Value;
            }
            
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Tag {TagId} removed from Manga {MangaId} successfully.", request.TagId, request.MangaId);
            return Unit.Value;
        }
    }
}
```

#### 2.6. AddMangaAuthor

```csharp
// Application/Features/Mangas/Commands/AddMangaAuthor/AddMangaAuthorCommand.cs
using Domain.Enums;
using MediatR;

namespace Application.Features.Mangas.Commands.AddMangaAuthor
{
    public class AddMangaAuthorCommand : IRequest<Unit>
    {
        public Guid MangaId { get; set; }
        public Guid AuthorId { get; set; }
        public MangaStaffRole Role { get; set; }
    }
}
```

```csharp
// Application/Features/Mangas/Commands/AddMangaAuthor/AddMangaAuthorCommandHandler.cs
using Application.Contracts.Persistence;
using Application.Exceptions;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore; // For AnyAsync

namespace Application.Features.Mangas.Commands.AddMangaAuthor
{
    public class AddMangaAuthorCommandHandler : IRequestHandler<AddMangaAuthorCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AddMangaAuthorCommandHandler> _logger;

        public AddMangaAuthorCommandHandler(IUnitOfWork unitOfWork, ILogger<AddMangaAuthorCommandHandler> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Unit> Handle(AddMangaAuthorCommand request, CancellationToken cancellationToken)
        {
            var manga = await _unitOfWork.MangaRepository.GetByIdAsync(request.MangaId);
            if (manga == null)
            {
                throw new NotFoundException(nameof(Manga), request.MangaId);
            }

            var author = await _unitOfWork.AuthorRepository.GetByIdAsync(request.AuthorId);
            if (author == null)
            {
                throw new NotFoundException(nameof(Author), request.AuthorId);
            }

            // Kiểm tra xem MangaAuthor đã tồn tại chưa
            var mangaEntity = await _unitOfWork.MangaRepository.FindFirstOrDefaultAsync(
                m => m.MangaId == request.MangaId,
                includeProperties: "MangaAuthors" // Quan trọng
            );

            if (mangaEntity == null) // Should not happen
            {
                throw new NotFoundException(nameof(Manga), request.MangaId);
            }

            if (!mangaEntity.MangaAuthors.Any(ma => ma.AuthorId == request.AuthorId && ma.Role == request.Role))
            {
                mangaEntity.MangaAuthors.Add(new MangaAuthor
                {
                    MangaId = request.MangaId,
                    AuthorId = request.AuthorId,
                    Role = request.Role
                });
            }
            else
            {
                _logger.LogInformation("Author {AuthorId} with role {Role} is already associated with Manga {MangaId}.", request.AuthorId, request.Role, request.MangaId);
                return Unit.Value;
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Author {AuthorId} with role {Role} added to Manga {MangaId} successfully.", request.AuthorId, request.Role, request.MangaId);
            return Unit.Value;
        }
    }
}
```

#### 2.7. RemoveMangaAuthor

```csharp
// Application/Features/Mangas/Commands/RemoveMangaAuthor/RemoveMangaAuthorCommand.cs
using Domain.Enums;
using MediatR;

namespace Application.Features.Mangas.Commands.RemoveMangaAuthor
{
    public class RemoveMangaAuthorCommand : IRequest<Unit>
    {
        public Guid MangaId { get; set; }
        public Guid AuthorId { get; set; }
        public MangaStaffRole Role { get; set; } // Cần Role để xác định chính xác record cần xóa
    }
}
```

```csharp
// Application/Features/Mangas/Commands/RemoveMangaAuthor/RemoveMangaAuthorCommandHandler.cs
using Application.Contracts.Persistence;
using Application.Exceptions;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore; // For SingleOrDefaultAsync

namespace Application.Features.Mangas.Commands.RemoveMangaAuthor
{
    public class RemoveMangaAuthorCommandHandler : IRequestHandler<RemoveMangaAuthorCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<RemoveMangaAuthorCommandHandler> _logger;

        public RemoveMangaAuthorCommandHandler(IUnitOfWork unitOfWork, ILogger<RemoveMangaAuthorCommandHandler> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Unit> Handle(RemoveMangaAuthorCommand request, CancellationToken cancellationToken)
        {
            var manga = await _unitOfWork.MangaRepository.GetByIdAsync(request.MangaId);
            if (manga == null)
            {
                throw new NotFoundException(nameof(Manga), request.MangaId);
            }

            // Tác giả có thể không cần check vì ta chỉ xóa record join
            // var author = await _unitOfWork.AuthorRepository.GetByIdAsync(request.AuthorId);
            // if (author == null)
            // {
            //     throw new NotFoundException(nameof(Author), request.AuthorId);
            // }

            var mangaEntity = await _unitOfWork.MangaRepository.FindFirstOrDefaultAsync(
                m => m.MangaId == request.MangaId,
                includeProperties: "MangaAuthors" // Quan trọng
            );

            if (mangaEntity == null) // Should not happen
            {
                throw new NotFoundException(nameof(Manga), request.MangaId);
            }
            
            var mangaAuthorToRemove = mangaEntity.MangaAuthors.SingleOrDefault(ma => ma.AuthorId == request.AuthorId && ma.Role == request.Role);

            if (mangaAuthorToRemove != null)
            {
                mangaEntity.MangaAuthors.Remove(mangaAuthorToRemove);
            }
            else
            {
                _logger.LogWarning("Author {AuthorId} with role {Role} not found on Manga {MangaId} for removal.", request.AuthorId, request.Role, request.MangaId);
                return Unit.Value;
            }
            
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Author {AuthorId} with role {Role} removed from Manga {MangaId} successfully.", request.AuthorId, request.Role, request.MangaId);
            return Unit.Value;
        }
    }
}
```

### 3. Features/Chapters

#### 3.1. CreateChapter

```csharp
// Application/Features/Chapters/Commands/CreateChapter/CreateChapterCommand.cs
using MediatR;

namespace Application.Features.Chapters.Commands.CreateChapter
{
    public class CreateChapterCommand : IRequest<Guid>
    {
        public Guid TranslatedMangaId { get; set; }
        public int UploadedByUserId { get; set; } // Sẽ lấy từ user context ở Controller
        public string? Volume { get; set; }
        public string? ChapterNumber { get; set; }
        public string? Title { get; set; }
        public DateTime PublishAt { get; set; }
        public DateTime ReadableAt { get; set; }
    }
}
```

```csharp
// Application/Features/Chapters/Commands/CreateChapter/CreateChapterCommandHandler.cs
using Application.Contracts.Persistence;
using Application.Exceptions;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Chapters.Commands.CreateChapter
{
    public class CreateChapterCommandHandler : IRequestHandler<CreateChapterCommand, Guid>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<CreateChapterCommandHandler> _logger;

        public CreateChapterCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<CreateChapterCommandHandler> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Guid> Handle(CreateChapterCommand request, CancellationToken cancellationToken)
        {
            var translatedManga = await _unitOfWork.TranslatedMangaRepository.GetByIdAsync(request.TranslatedMangaId);
            if (translatedManga == null)
            {
                throw new NotFoundException(nameof(TranslatedManga), request.TranslatedMangaId);
            }

            // Kiểm tra User (UploadedByUserId) có tồn tại không.
            // Hiện tại, ta giả định User ID là hợp lệ và được truyền vào.
            // Nếu cần, bạn có thể thêm IUserRepository và kiểm tra.
            // var user = await _unitOfWork.UserRepository.GetByIdAsync(request.UploadedByUserId);
            // if (user == null)
            // {
            //     throw new NotFoundException(nameof(User), request.UploadedByUserId);
            // }

            var chapter = _mapper.Map<Chapter>(request);
            // ChapterId sẽ được tự sinh.
            // ChapterPages sẽ được thêm sau.

            await _unitOfWork.ChapterRepository.AddAsync(chapter);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Chapter {ChapterId} for TranslatedManga {TranslatedMangaId} created successfully by User {UserId}.",
                chapter.ChapterId, request.TranslatedMangaId, request.UploadedByUserId);
            return chapter.ChapterId;
        }
    }
}
```

#### 3.2. UpdateChapter

```csharp
// Application/Features/Chapters/Commands/UpdateChapter/UpdateChapterCommand.cs
using MediatR;

namespace Application.Features.Chapters.Commands.UpdateChapter
{
    public class UpdateChapterCommand : IRequest<Unit>
    {
        public Guid ChapterId { get; set; } // Lấy từ route
        public string? Volume { get; set; }
        public string? ChapterNumber { get; set; }
        public string? Title { get; set; }
        public DateTime PublishAt { get; set; }
        public DateTime ReadableAt { get; set; }
        // UploadedByUserId không nên cho phép cập nhật qua command này
        // TranslatedMangaId cũng không nên thay đổi
    }
}
```

```csharp
// Application/Features/Chapters/Commands/UpdateChapter/UpdateChapterCommandHandler.cs
using Application.Contracts.Persistence;
using Application.Exceptions;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Chapters.Commands.UpdateChapter
{
    public class UpdateChapterCommandHandler : IRequestHandler<UpdateChapterCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<UpdateChapterCommandHandler> _logger;

        public UpdateChapterCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<UpdateChapterCommandHandler> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Unit> Handle(UpdateChapterCommand request, CancellationToken cancellationToken)
        {
            var chapterToUpdate = await _unitOfWork.ChapterRepository.GetByIdAsync(request.ChapterId);

            if (chapterToUpdate == null)
            {
                _logger.LogWarning("Chapter with ID {ChapterId} not found for update.", request.ChapterId);
                throw new NotFoundException(nameof(Domain.Entities.Chapter), request.ChapterId);
            }

            // Chỉ map các trường được phép thay đổi.
            // AutoMapper có thể được cấu hình để bỏ qua các thuộc tính không có trong source
            // hoặc bạn có thể map thủ công từng thuộc tính.
            // Trong ví dụ này, UpdateChapterCommand chỉ chứa các trường có thể cập nhật.
            _mapper.Map(request, chapterToUpdate);

            await _unitOfWork.ChapterRepository.UpdateAsync(chapterToUpdate);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Chapter {ChapterId} updated successfully.", request.ChapterId);
            return Unit.Value;
        }
    }
}
```

#### 3.3. DeleteChapter

```csharp
// Application/Features/Chapters/Commands/DeleteChapter/DeleteChapterCommand.cs
using MediatR;

namespace Application.Features.Chapters.Commands.DeleteChapter
{
    public class DeleteChapterCommand : IRequest<Unit>
    {
        public Guid ChapterId { get; set; }
    }
}
```

```csharp
// Application/Features/Chapters/Commands/DeleteChapter/DeleteChapterCommandHandler.cs
using Application.Contracts.Persistence;
using Application.Common.Interfaces; // Cho IPhotoAccessor
using Application.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore; // Cho ToListAsync

namespace Application.Features.Chapters.Commands.DeleteChapter
{
    public class DeleteChapterCommandHandler : IRequestHandler<DeleteChapterCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPhotoAccessor _photoAccessor;
        private readonly ILogger<DeleteChapterCommandHandler> _logger;

        public DeleteChapterCommandHandler(IUnitOfWork unitOfWork, IPhotoAccessor photoAccessor, ILogger<DeleteChapterCommandHandler> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _photoAccessor = photoAccessor ?? throw new ArgumentNullException(nameof(photoAccessor));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Unit> Handle(DeleteChapterCommand request, CancellationToken cancellationToken)
        {
            var chapterToDelete = await _unitOfWork.ChapterRepository.GetChapterWithPagesAsync(request.ChapterId);

            if (chapterToDelete == null)
            {
                _logger.LogWarning("Chapter with ID {ChapterId} not found for deletion.", request.ChapterId);
                throw new NotFoundException(nameof(Domain.Entities.Chapter), request.ChapterId);
            }

            // 1. Xóa các trang (ChapterPages) khỏi Cloudinary
            if (chapterToDelete.ChapterPages != null && chapterToDelete.ChapterPages.Any())
            {
                foreach (var page in chapterToDelete.ChapterPages.ToList()) // ToList() để tránh lỗi khi modify collection
                {
                    if (!string.IsNullOrEmpty(page.PublicId))
                    {
                        var deletionResult = await _photoAccessor.DeletePhotoAsync(page.PublicId);
                        if (deletionResult != "ok" && deletionResult != "not found")
                        {
                            _logger.LogWarning("Failed to delete chapter page {PublicId} from Cloudinary for chapter {ChapterId}. Result: {DeletionResult}", page.PublicId, request.ChapterId, deletionResult);
                            // Có thể quyết định dừng lại hoặc tiếp tục tùy theo yêu cầu
                        }
                    }
                    // ChapterPage entities sẽ được xóa cùng Chapter do cấu hình Cascade Delete trong DB
                }
            }

            // 2. Xóa Chapter khỏi DB (ChapterPages sẽ tự động xóa theo cấu hình Cascade)
            await _unitOfWork.ChapterRepository.DeleteAsync(chapterToDelete);
            await _unitOFWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Chapter {ChapterId} and its pages deleted successfully.", request.ChapterId);
            return Unit.Value;
        }
    }
}
```

#### 3.4. CreateChapterPageEntry (Tạo metadata cho trang, không upload ảnh)

```csharp
// Application/Features/Chapters/Commands/CreateChapterPageEntry/CreateChapterPageEntryCommand.cs
using MediatR;

namespace Application.Features.Chapters.Commands.CreateChapterPageEntry
{
    public class CreateChapterPageEntryCommand : IRequest<Guid> // Trả về PageId
    {
        public Guid ChapterId { get; set; }
        public int PageNumber { get; set; } 
        // PublicId sẽ được cập nhật sau khi upload ảnh bằng một command khác (UploadChapterPageImageCommand)
    }
}
```

```csharp
// Application/Features/Chapters/Commands/CreateChapterPageEntry/CreateChapterPageEntryCommandHandler.cs
using Application.Contracts.Persistence;
using Application.Exceptions;
using AutoMapper; // Nếu cần map
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Chapters.Commands.CreateChapterPageEntry
{
    public class CreateChapterPageEntryCommandHandler : IRequestHandler<CreateChapterPageEntryCommand, Guid>
    {
        private readonly IUnitOfWork _unitOfWork;
        // private readonly IMapper _mapper; // Không cần mapper nếu command khớp entity
        private readonly ILogger<CreateChapterPageEntryCommandHandler> _logger;

        public CreateChapterPageEntryCommandHandler(IUnitOfWork unitOfWork, ILogger<CreateChapterPageEntryCommandHandler> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Guid> Handle(CreateChapterPageEntryCommand request, CancellationToken cancellationToken)
        {
            var chapter = await _unitOfWork.ChapterRepository.GetByIdAsync(request.ChapterId);
            if (chapter == null)
            {
                throw new NotFoundException(nameof(Chapter), request.ChapterId);
            }

            // Kiểm tra xem PageNumber đã tồn tại trong Chapter này chưa
            var chapterWithPages = await _unitOfWork.ChapterRepository.GetChapterWithPagesAsync(request.ChapterId);
            if (chapterWithPages != null && chapterWithPages.ChapterPages.Any(p => p.PageNumber == request.PageNumber))
            {
                _logger.LogWarning("Page number {PageNumber} already exists in chapter {ChapterId}.", request.PageNumber, request.ChapterId);
                throw new Exceptions.ValidationException($"Page number {request.PageNumber} already exists in chapter {request.ChapterId}.");
            }

            // Nếu PageNumber được cung cấp là 0 hoặc âm, hoặc không được cung cấp, ta có thể tự động gán số trang tiếp theo.
            int pageNumberToSet = request.PageNumber;
            if (pageNumberToSet <= 0)
            {
                pageNumberToSet = await _unitOfWork.ChapterRepository.GetMaxPageNumberAsync(request.ChapterId) + 1;
            }
            
            var chapterPage = new ChapterPage
            {
                ChapterId = request.ChapterId,
                PageNumber = pageNumberToSet,
                PublicId = string.Empty // Sẽ được cập nhật sau khi upload ảnh
            };
            // PageId sẽ tự sinh

            // Thay vì _unitOfWork.ChapterPageRepository.AddAsync(chapterPage);
            // Ta sử dụng phương thức trong IChapterRepository đã định nghĩa
            await _unitOfWork.ChapterRepository.AddPageAsync(chapterPage);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("ChapterPage entry {PageId} (PageNumber: {PageNumber}) created for Chapter {ChapterId}.", 
                chapterPage.PageId, chapterPage.PageNumber, request.ChapterId);
            return chapterPage.PageId;
        }
    }
}
```

#### 3.5. UploadChapterPageImage (Upload ảnh và cập nhật PublicId cho ChapterPage đã tồn tại)

```csharp
// Application/Features/Chapters/Commands/UploadChapterPageImage/UploadChapterPageImageCommand.cs
using MediatR;
using Microsoft.AspNetCore.Http; // Tạm thời để IFormFile, controller sẽ chuyển thành stream

namespace Application.Features.Chapters.Commands.UploadChapterPageImage
{
    public class UploadChapterPageImageCommand : IRequest<string> // Trả về PublicId của ảnh
    {
        public Guid ChapterPageId { get; set; } // ID của ChapterPage entry đã được tạo
        
        // Các thông tin này sẽ được Controller chuẩn bị từ IFormFile
        public Stream ImageStream { get; set; } = null!;
        public string OriginalFileName { get; set; } = string.Empty; 
        public string ContentType { get; set; } = string.Empty; // Có thể cần cho việc đặt tên hoặc kiểm tra

        // Thông tin để tạo desiredPublicId
        public Guid MangaId { get; set; } // Cần để tạo đường dẫn public_id
        public Guid TranslatedMangaId { get; set; } // Cần để tạo đường dẫn public_id
        public Guid ChapterId { get; set; } // Cần để tạo đường dẫn public_id
    }
}
```

```csharp
// Application/Features/Chapters/Commands/UploadChapterPageImage/UploadChapterPageImageCommandHandler.cs
using Application.Common.Interfaces;
using Application.Contracts.Persistence;
using Application.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Chapters.Commands.UploadChapterPageImage
{
    public class UploadChapterPageImageCommandHandler : IRequestHandler<UploadChapterPageImageCommand, string>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPhotoAccessor _photoAccessor;
        private readonly ILogger<UploadChapterPageImageCommandHandler> _logger;

        public UploadChapterPageImageCommandHandler(IUnitOfWork unitOfWork, IPhotoAccessor photoAccessor, ILogger<UploadChapterPageImageCommandHandler> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _photoAccessor = photoAccessor ?? throw new ArgumentNullException(nameof(photoAccessor));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<string> Handle(UploadChapterPageImageCommand request, CancellationToken cancellationToken)
        {
            var chapterPage = await _unitOfWork.ChapterRepository.GetPageByIdAsync(request.ChapterPageId);
            if (chapterPage == null)
            {
                throw new NotFoundException(nameof(Domain.Entities.ChapterPage), request.ChapterPageId);
            }

            // Nếu ChapterPage đã có ảnh (PublicId không rỗng), xóa ảnh cũ trước khi upload ảnh mới
            if (!string.IsNullOrEmpty(chapterPage.PublicId))
            {
                var deletionResult = await _photoAccessor.DeletePhotoAsync(chapterPage.PublicId);
                if (deletionResult != "ok" && deletionResult != "not found")
                {
                    _logger.LogWarning("Failed to delete old chapter page image {OldPublicId} from Cloudinary for ChapterPage {ChapterPageId}. Result: {DeletionResult}",
                        chapterPage.PublicId, request.ChapterPageId, deletionResult);
                    // Quyết định có dừng lại không tùy thuộc vào yêu cầu. Thường thì vẫn tiếp tục upload ảnh mới.
                }
            }
            
            // Tạo desiredPublicId cho Cloudinary (ví dụ)
            // mangas/{MangaId}/translated/{TranslatedMangaId}/chapters/{ChapterId}/pages/{ChapterPageId}_{OriginalFileNameWithoutExtensionAndSpaces}
            // Hoặc đơn giản là mangas/chapters/{ChapterId}/pages/{ChapterPageId}
            // Cần đảm bảo tính duy nhất và dễ quản lý.
            // Ví dụ:
            var fileExtension = Path.GetExtension(request.OriginalFileName); // .jpg, .png
            var desiredPublicId = $"mangas_v2/{request.MangaId}/translated/{request.TranslatedMangaId}/chapters/{request.ChapterId}/pages/{request.ChapterPageId}{fileExtension}"; 
            // Folder có thể được cấu hình trong PhotoAccessor hoặc truyền vào đây.
            // Ví dụ folder: $"mangas_v2/{request.MangaId}/translated/{request.TranslatedMangaId}/chapters/{request.ChapterId}/pages"
            // và desiredPublicId chỉ là $"{request.ChapterPageId}{fileExtension}"

            var uploadResult = await _photoAccessor.UploadPhotoAsync(
                request.ImageStream,
                desiredPublicId,
                request.OriginalFileName // originalFileNameForUpload
                // folderName: có thể truyền nếu desiredPublicId không bao gồm folder
            );

            if (uploadResult == null || string.IsNullOrEmpty(uploadResult.PublicId))
            {
                _logger.LogError("Failed to upload image for ChapterPage {ChapterPageId}.", request.ChapterPageId);
                throw new Exceptions.ApiException("Image upload failed."); // Hoặc một exception cụ thể hơn
            }

            chapterPage.PublicId = uploadResult.PublicId;
            await _unitOfWork.ChapterRepository.UpdatePageAsync(chapterPage);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Image uploaded and PublicId {PublicId} set for ChapterPage {ChapterPageId}.", uploadResult.PublicId, request.ChapterPageId);
            return uploadResult.PublicId;
        }
    }
}
```

#### 3.6. UpdateChapterPageDetails (Chỉ cập nhật metadata của trang, không phải ảnh)

```csharp
// Application/Features/Chapters/Commands/UpdateChapterPageDetails/UpdateChapterPageDetailsCommand.cs
using MediatR;

namespace Application.Features.Chapters.Commands.UpdateChapterPageDetails
{
    public class UpdateChapterPageDetailsCommand : IRequest<Unit>
    {
        public Guid PageId { get; set; } // ID của ChapterPage
        public int PageNumber { get; set; }
        // Các metadata khác của trang nếu có (ví dụ: chú thích,...)
    }
}
```

```csharp
// Application/Features/Chapters/Commands/UpdateChapterPageDetails/UpdateChapterPageDetailsCommandHandler.cs
using Application.Contracts.Persistence;
using Application.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Chapters.Commands.UpdateChapterPageDetails
{
    public class UpdateChapterPageDetailsCommandHandler : IRequestHandler<UpdateChapterPageDetailsCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UpdateChapterPageDetailsCommandHandler> _logger;

        public UpdateChapterPageDetailsCommandHandler(IUnitOfWork unitOfWork, ILogger<UpdateChapterPageDetailsCommandHandler> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Unit> Handle(UpdateChapterPageDetailsCommand request, CancellationToken cancellationToken)
        {
            var pageToUpdate = await _unitOfWork.ChapterRepository.GetPageByIdAsync(request.PageId);

            if (pageToUpdate == null)
            {
                throw new NotFoundException(nameof(Domain.Entities.ChapterPage), request.PageId);
            }

            // Kiểm tra nếu PageNumber thay đổi, có bị trùng với trang khác trong cùng chapter không
            if (pageToUpdate.PageNumber != request.PageNumber)
            {
                var chapter = await _unitOfWork.ChapterRepository.GetChapterWithPagesAsync(pageToUpdate.ChapterId);
                if (chapter != null && chapter.ChapterPages.Any(p => p.PageId != request.PageId && p.PageNumber == request.PageNumber))
                {
                     _logger.LogWarning("Page number {PageNumber} already exists in chapter {ChapterId} for another page.", request.PageNumber, pageToUpdate.ChapterId);
                     throw new Exceptions.ValidationException($"Page number {request.PageNumber} already exists in chapter {pageToUpdate.ChapterId} for another page.");
                }
            }

            pageToUpdate.PageNumber = request.PageNumber;
            // Cập nhật các trường metadata khác nếu có

            await _unitOfWork.ChapterRepository.UpdatePageAsync(pageToUpdate);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("ChapterPage {PageId} details updated successfully.", request.PageId);
            return Unit.Value;
        }
    }
}
```

#### 3.7. DeleteChapterPage

```csharp
// Application/Features/Chapters/Commands/DeleteChapterPage/DeleteChapterPageCommand.cs
using MediatR;

namespace Application.Features.Chapters.Commands.DeleteChapterPage
{
    public class DeleteChapterPageCommand : IRequest<Unit>
    {
        public Guid PageId { get; set; } // ID của ChapterPage cần xóa
    }
}
```

```csharp
// Application/Features/Chapters/Commands/DeleteChapterPage/DeleteChapterPageCommandHandler.cs
using Application.Common.Interfaces;
using Application.Contracts.Persistence;
using Application.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Chapters.Commands.DeleteChapterPage
{
    public class DeleteChapterPageCommandHandler : IRequestHandler<DeleteChapterPageCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPhotoAccessor _photoAccessor;
        private readonly ILogger<DeleteChapterPageCommandHandler> _logger;

        public DeleteChapterPageCommandHandler(IUnitOfWork unitOfWork, IPhotoAccessor photoAccessor, ILogger<DeleteChapterPageCommandHandler> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _photoAccessor = photoAccessor ?? throw new ArgumentNullException(nameof(photoAccessor));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Unit> Handle(DeleteChapterPageCommand request, CancellationToken cancellationToken)
        {
            var pageToDelete = await _unitOfWork.ChapterRepository.GetPageByIdAsync(request.PageId);

            if (pageToDelete == null)
            {
                _logger.LogWarning("ChapterPage with ID {PageId} not found for deletion.", request.PageId);
                throw new NotFoundException(nameof(Domain.Entities.ChapterPage), request.PageId);
            }

            // 1. Xóa ảnh khỏi Cloudinary (nếu có)
            if (!string.IsNullOrEmpty(pageToDelete.PublicId))
            {
                var deletionResult = await _photoAccessor.DeletePhotoAsync(pageToDelete.PublicId);
                if (deletionResult != "ok" && deletionResult != "not found")
                {
                    _logger.LogWarning("Failed to delete chapter page image {PublicId} from Cloudinary for PageId {PageId}. Result: {DeletionResult}", 
                        pageToDelete.PublicId, request.PageId, deletionResult);
                    // Quyết định có dừng lại không tùy thuộc vào yêu cầu. Thường thì vẫn tiếp tục xóa khỏi DB.
                }
            }

            // 2. Xóa ChapterPage khỏi DB
            await _unitOfWork.ChapterRepository.DeletePageAsync(request.PageId);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("ChapterPage {PageId} deleted successfully.", request.PageId);
            return Unit.Value;
        }
    }
}
```

### 4. Features/CoverArts

#### 4.1. UploadCoverArtImage (Tạo CoverArt entry và upload ảnh)

```csharp
// Application/Features/CoverArts/Commands/UploadCoverArtImage/UploadCoverArtImageCommand.cs
using MediatR;
using Microsoft.AspNetCore.Http; // Tạm thời

namespace Application.Features.CoverArts.Commands.UploadCoverArtImage
{
    public class UploadCoverArtImageCommand : IRequest<Guid> // Trả về CoverId
    {
        public Guid MangaId { get; set; }
        public string? Volume { get; set; }
        public string? Description { get; set; }

        // Thông tin file sẽ được Controller chuẩn bị
        public Stream ImageStream { get; set; } = null!;
        public string OriginalFileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
    }
}
```

```csharp
// Application/Features/CoverArts/Commands/UploadCoverArtImage/UploadCoverArtImageCommandHandler.cs
using Application.Common.Interfaces;
using Application.Contracts.Persistence;
using Application.Exceptions;
using AutoMapper; // Nếu cần map từ DTO/Command sang Entity
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.CoverArts.Commands.UploadCoverArtImage
{
    public class UploadCoverArtImageCommandHandler : IRequestHandler<UploadCoverArtImageCommand, Guid>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPhotoAccessor _photoAccessor;
        private readonly IMapper _mapper; // Giả sử có map từ command sang CoverArt
        private readonly ILogger<UploadCoverArtImageCommandHandler> _logger;

        public UploadCoverArtImageCommandHandler(IUnitOfWork unitOfWork, IPhotoAccessor photoAccessor, IMapper mapper, ILogger<UploadCoverArtImageCommandHandler> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _photoAccessor = photoAccessor ?? throw new ArgumentNullException(nameof(photoAccessor));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Guid> Handle(UploadCoverArtImageCommand request, CancellationToken cancellationToken)
        {
            var manga = await _unitOfWork.MangaRepository.GetByIdAsync(request.MangaId);
            if (manga == null)
            {
                throw new NotFoundException(nameof(Manga), request.MangaId);
            }

            // Tạo desiredPublicId cho Cloudinary
            // Ví dụ: mangas_v2/{MangaId}/covers/{NewGuid}_{OriginalFileNameWithoutExtension}
            // Hoặc mangas_v2/{MangaId}/covers/volume_{VolumeNumber}_{NewGuid}
            var fileExtension = Path.GetExtension(request.OriginalFileName);
            var uniqueId = Guid.NewGuid().ToString("N").Substring(0, 8); // Một phần Guid cho duy nhất
            var desiredPublicId = $"mangas_v2/{request.MangaId}/covers/{request.Volume ?? "default"}_{uniqueId}{fileExtension}";
            
            var uploadResult = await _photoAccessor.UploadPhotoAsync(
                request.ImageStream,
                desiredPublicId,
                request.OriginalFileName
            );

            if (uploadResult == null || string.IsNullOrEmpty(uploadResult.PublicId))
            {
                _logger.LogError("Failed to upload cover art image for Manga {MangaId}.", request.MangaId);
                throw new Exceptions.ApiException("Cover art image upload failed.");
            }

            var coverArt = new CoverArt
            {
                MangaId = request.MangaId,
                Volume = request.Volume,
                Description = request.Description,
                PublicId = uploadResult.PublicId 
                // CoverId sẽ tự sinh
            };
            // Hoặc _mapper.Map<CoverArt>(request); nếu command có đủ thông tin và đã cấu hình mapping
            // và sau đó gán coverArt.PublicId = uploadResult.PublicId;

            await _unitOfWork.CoverArtRepository.AddAsync(coverArt);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("CoverArt {CoverId} (PublicId: {PublicId}) created for Manga {MangaId}.", 
                coverArt.CoverId, coverArt.PublicId, request.MangaId);
            return coverArt.CoverId;
        }
    }
}
```

#### 4.2. DeleteCoverArt

```csharp
// Application/Features/CoverArts/Commands/DeleteCoverArt/DeleteCoverArtCommand.cs
using MediatR;

namespace Application.Features.CoverArts.Commands.DeleteCoverArt
{
    public class DeleteCoverArtCommand : IRequest<Unit>
    {
        public Guid CoverId { get; set; }
    }
}
```

```csharp
// Application/Features/CoverArts/Commands/DeleteCoverArt/DeleteCoverArtCommandHandler.cs
using Application.Common.Interfaces;
using Application.Contracts.Persistence;
using Application.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.CoverArts.Commands.DeleteCoverArt
{
    public class DeleteCoverArtCommandHandler : IRequestHandler<DeleteCoverArtCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPhotoAccessor _photoAccessor;
        private readonly ILogger<DeleteCoverArtCommandHandler> _logger;

        public DeleteCoverArtCommandHandler(IUnitOfWork unitOfWork, IPhotoAccessor photoAccessor, ILogger<DeleteCoverArtCommandHandler> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _photoAccessor = photoAccessor ?? throw new ArgumentNullException(nameof(photoAccessor));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Unit> Handle(DeleteCoverArtCommand request, CancellationToken cancellationToken)
        {
            var coverArtToDelete = await _unitOfWork.CoverArtRepository.GetByIdAsync(request.CoverId);

            if (coverArtToDelete == null)
            {
                _logger.LogWarning("CoverArt with ID {CoverId} not found for deletion.", request.CoverId);
                throw new NotFoundException(nameof(Domain.Entities.CoverArt), request.CoverId);
            }

            // 1. Xóa ảnh khỏi Cloudinary
            if (!string.IsNullOrEmpty(coverArtToDelete.PublicId))
            {
                var deletionResult = await _photoAccessor.DeletePhotoAsync(coverArtToDelete.PublicId);
                if (deletionResult != "ok" && deletionResult != "not found")
                {
                    _logger.LogWarning("Failed to delete cover art image {PublicId} from Cloudinary for CoverId {CoverId}. Result: {DeletionResult}", 
                        coverArtToDelete.PublicId, request.CoverId, deletionResult);
                }
            }

            // 2. Xóa CoverArt khỏi DB
            await _unitOfWork.CoverArtRepository.DeleteAsync(coverArtToDelete);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("CoverArt {CoverId} deleted successfully.", request.CoverId);
            return Unit.Value;
        }
    }
}
```

### 5. Features/Tags

#### 5.1. CreateTag

```csharp
// Application/Features/Tags/Commands/CreateTag/CreateTagCommand.cs
using MediatR;

namespace Application.Features.Tags.Commands.CreateTag
{
    public class CreateTagCommand : IRequest<Guid>
    {
        public string Name { get; set; } = string.Empty;
        public Guid TagGroupId { get; set; }
    }
}
```

```csharp
// Application/Features/Tags/Commands/CreateTag/CreateTagCommandHandler.cs
using Application.Contracts.Persistence;
using Application.Exceptions;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Tags.Commands.CreateTag
{
    public class CreateTagCommandHandler : IRequestHandler<CreateTagCommand, Guid>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<CreateTagCommandHandler> _logger;

        public CreateTagCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<CreateTagCommandHandler> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Guid> Handle(CreateTagCommand request, CancellationToken cancellationToken)
        {
            var tagGroup = await _unitOfWork.TagGroupRepository.GetByIdAsync(request.TagGroupId);
            if (tagGroup == null)
            {
                throw new NotFoundException(nameof(TagGroup), request.TagGroupId);
            }

            // Kiểm tra xem Tag đã tồn tại trong TagGroup này chưa
            var existingTag = await _unitOfWork.TagRepository.GetTagByNameAndGroupAsync(request.Name, request.TagGroupId);
            if (existingTag != null)
            {
                _logger.LogWarning("Tag with name '{TagName}' already exists in TagGroup {TagGroupId}.", request.Name, request.TagGroupId);
                throw new Exceptions.ValidationException($"Tag with name '{request.Name}' already exists in TagGroup '{tagGroup.Name}'.");
            }

            var tag = _mapper.Map<Tag>(request);
            // TagId sẽ tự sinh

            await _unitOfWork.TagRepository.AddAsync(tag);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Tag {TagId} created successfully in TagGroup {TagGroupId}.", tag.TagId, request.TagGroupId);
            return tag.TagId;
        }
    }
}
```

#### 5.2. UpdateTag

```csharp
// Application/Features/Tags/Commands/UpdateTag/UpdateTagCommand.cs
using MediatR;

namespace Application.Features.Tags.Commands.UpdateTag
{
    public class UpdateTagCommand : IRequest<Unit>
    {
        public Guid TagId { get; set; }
        public string Name { get; set; } = string.Empty;
        public Guid TagGroupId { get; set; }
    }
}
```

```csharp
// Application/Features/Tags/Commands/UpdateTag/UpdateTagCommandHandler.cs
using Application.Contracts.Persistence;
using Application.Exceptions;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Tags.Commands.UpdateTag
{
    public class UpdateTagCommandHandler : IRequestHandler<UpdateTagCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<UpdateTagCommandHandler> _logger;

        public UpdateTagCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<UpdateTagCommandHandler> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Unit> Handle(UpdateTagCommand request, CancellationToken cancellationToken)
        {
            var tagToUpdate = await _unitOfWork.TagRepository.GetByIdAsync(request.TagId);
            if (tagToUpdate == null)
            {
                throw new NotFoundException(nameof(Tag), request.TagId);
            }

            // Kiểm tra TagGroup mới có tồn tại không
            if (tagToUpdate.TagGroupId != request.TagGroupId)
            {
                var newTagGroup = await _unitOfWork.TagGroupRepository.GetByIdAsync(request.TagGroupId);
                if (newTagGroup == null)
                {
                    throw new NotFoundException(nameof(TagGroup), request.TagGroupId, "New TagGroup for Tag update not found.");
                }
            }

            // Kiểm tra nếu tên hoặc TagGroup thay đổi, có bị trùng với tag khác không
            if (!string.Equals(tagToUpdate.Name, request.Name, StringComparison.OrdinalIgnoreCase) || tagToUpdate.TagGroupId != request.TagGroupId)
            {
                var existingTagWithNewProps = await _unitOfWork.TagRepository.GetTagByNameAndGroupAsync(request.Name, request.TagGroupId);
                if (existingTagWithNewProps != null && existingTagWithNewProps.TagId != request.TagId)
                {
                    var tagGroupName = (await _unitOfWork.TagGroupRepository.GetByIdAsync(request.TagGroupId))?.Name ?? request.TagGroupId.ToString();
                    _logger.LogWarning("Another Tag with name '{TagName}' already exists in TagGroup '{TagGroupName}'.", request.Name, tagGroupName);
                    throw new Exceptions.ValidationException($"Another Tag with name '{request.Name}' already exists in TagGroup '{tagGroupName}'.");
                }
            }

            _mapper.Map(request, tagToUpdate);

            await _unitOfWork.TagRepository.UpdateAsync(tagToUpdate);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Tag {TagId} updated successfully.", request.TagId);
            return Unit.Value;
        }
    }
}
```

#### 5.3. DeleteTag

```csharp
// Application/Features/Tags/Commands/DeleteTag/DeleteTagCommand.cs
using MediatR;

namespace Application.Features.Tags.Commands.DeleteTag
{
    public class DeleteTagCommand : IRequest<Unit>
    {
        public Guid TagId { get; set; }
    }
}
```

```csharp
// Application/Features/Tags/Commands/DeleteTag/DeleteTagCommandHandler.cs
using Application.Contracts.Persistence;
using Application.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Tags.Commands.DeleteTag
{
    public class DeleteTagCommandHandler : IRequestHandler<DeleteTagCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DeleteTagCommandHandler> _logger;

        public DeleteTagCommandHandler(IUnitOfWork unitOfWork, ILogger<DeleteTagCommandHandler> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Unit> Handle(DeleteTagCommand request, CancellationToken cancellationToken)
        {
            var tagToDelete = await _unitOfWork.TagRepository.GetByIdAsync(request.TagId);
            if (tagToDelete == null)
            {
                throw new NotFoundException(nameof(Domain.Entities.Tag), request.TagId);
            }

            // Cân nhắc: nếu Tag đang được sử dụng bởi Manga (MangaTags), có cho phép xóa không?
            // OnDelete behavior trong DB đã được cấu hình là Cascade, nên MangaTags liên quan sẽ bị xóa.
            // Nếu không muốn cascade, bạn cần kiểm tra ở đây.
            // var isTagUsed = await _unitOfWork.MangaRepository.IsTagUsedAsync(request.TagId); // Cần thêm phương thức này vào IMangaRepository
            // if (isTagUsed)
            // {
            //    throw new Exceptions.DeleteFailureException(nameof(Domain.Entities.Tag), request.TagId, "Tag is currently associated with mangas.");
            // }

            await _unitOfWork.TagRepository.DeleteAsync(tagToDelete);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Tag {TagId} deleted successfully.", request.TagId);
            return Unit.Value;
        }
    }
}
```

### 6. Features/TagGroups

#### 6.1. CreateTagGroup

```csharp
// Application/Features/TagGroups/Commands/CreateTagGroup/CreateTagGroupCommand.cs
using MediatR;

namespace Application.Features.TagGroups.Commands.CreateTagGroup
{
    public class CreateTagGroupCommand : IRequest<Guid>
    {
        public string Name { get; set; } = string.Empty;
    }
}
```

```csharp
// Application/Features/TagGroups/Commands/CreateTagGroup/CreateTagGroupCommandHandler.cs
using Application.Contracts.Persistence;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.TagGroups.Commands.CreateTagGroup
{
    public class CreateTagGroupCommandHandler : IRequestHandler<CreateTagGroupCommand, Guid>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<CreateTagGroupCommandHandler> _logger;

        public CreateTagGroupCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<CreateTagGroupCommandHandler> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Guid> Handle(CreateTagGroupCommand request, CancellationToken cancellationToken)
        {
            var existingTagGroup = await _unitOfWork.TagGroupRepository.GetTagGroupByNameAsync(request.Name);
            if (existingTagGroup != null)
            {
                _logger.LogWarning("TagGroup with name '{TagGroupName}' already exists.", request.Name);
                throw new Exceptions.ValidationException($"TagGroup with name '{request.Name}' already exists.");
            }

            var tagGroup = _mapper.Map<TagGroup>(request);
            // TagGroupId sẽ tự sinh

            await _unitOfWork.TagGroupRepository.AddAsync(tagGroup);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("TagGroup {TagGroupId} created successfully.", tagGroup.TagGroupId);
            return tagGroup.TagGroupId;
        }
    }
}
```

#### 6.2. UpdateTagGroup

```csharp
// Application/Features/TagGroups/Commands/UpdateTagGroup/UpdateTagGroupCommand.cs
using MediatR;

namespace Application.Features.TagGroups.Commands.UpdateTagGroup
{
    public class UpdateTagGroupCommand : IRequest<Unit>
    {
        public Guid TagGroupId { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
```

```csharp
// Application/Features/TagGroups/Commands/UpdateTagGroup/UpdateTagGroupCommandHandler.cs
using Application.Contracts.Persistence;
using Application.Exceptions;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.TagGroups.Commands.UpdateTagGroup
{
    public class UpdateTagGroupCommandHandler : IRequestHandler<UpdateTagGroupCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<UpdateTagGroupCommandHandler> _logger;

        public UpdateTagGroupCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<UpdateTagGroupCommandHandler> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Unit> Handle(UpdateTagGroupCommand request, CancellationToken cancellationToken)
        {
            var tagGroupToUpdate = await _unitOfWork.TagGroupRepository.GetByIdAsync(request.TagGroupId);
            if (tagGroupToUpdate == null)
            {
                throw new NotFoundException(nameof(TagGroup), request.TagGroupId);
            }

            if (!string.Equals(tagGroupToUpdate.Name, request.Name, StringComparison.OrdinalIgnoreCase))
            {
                var existingTagGroupWithNewName = await _unitOfWork.TagGroupRepository.GetTagGroupByNameAsync(request.Name);
                if (existingTagGroupWithNewName != null && existingTagGroupWithNewName.TagGroupId != request.TagGroupId)
                {
                    _logger.LogWarning("Another TagGroup with name '{TagGroupName}' already exists.", request.Name);
                    throw new Exceptions.ValidationException($"Another TagGroup with name '{request.Name}' already exists.");
                }
            }

            _mapper.Map(request, tagGroupToUpdate);

            await _unitOfWork.TagGroupRepository.UpdateAsync(tagGroupToUpdate);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("TagGroup {TagGroupId} updated successfully.", request.TagGroupId);
            return Unit.Value;
        }
    }
}
```

#### 6.3. DeleteTagGroup

```csharp
// Application/Features/TagGroups/Commands/DeleteTagGroup/DeleteTagGroupCommand.cs
using MediatR;

namespace Application.Features.TagGroups.Commands.DeleteTagGroup
{
    public class DeleteTagGroupCommand : IRequest<Unit>
    {
        public Guid TagGroupId { get; set; }
    }
}
```

```csharp
// Application/Features/TagGroups/Commands/DeleteTagGroup/DeleteTagGroupCommandHandler.cs
using Application.Contracts.Persistence;
using Application.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.TagGroups.Commands.DeleteTagGroup
{
    public class DeleteTagGroupCommandHandler : IRequestHandler<DeleteTagGroupCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DeleteTagGroupCommandHandler> _logger;

        public DeleteTagGroupCommandHandler(IUnitOfWork unitOfWork, ILogger<DeleteTagGroupCommandHandler> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Unit> Handle(DeleteTagGroupCommand request, CancellationToken cancellationToken)
        {
            var tagGroupToDelete = await _unitOfWork.TagGroupRepository.GetTagGroupWithTagsAsync(request.TagGroupId);
            if (tagGroupToDelete == null)
            {
                throw new NotFoundException(nameof(Domain.Entities.TagGroup), request.TagGroupId);
            }

            // Kiểm tra xem TagGroup có chứa Tags nào không.
            // Theo OnModelCreating, Tags.TagGroupId có OnDelete(DeleteBehavior.Restrict)
            // Điều này có nghĩa là DB sẽ không cho xóa TagGroup nếu nó còn chứa Tags.
            // Chúng ta cần kiểm tra ở đây để trả về lỗi thân thiện hơn.
            if (tagGroupToDelete.Tags != null && tagGroupToDelete.Tags.Any())
            {
                _logger.LogWarning("Attempted to delete TagGroup {TagGroupId} which still contains tags.", request.TagGroupId);
                throw new Exceptions.DeleteFailureException(nameof(Domain.Entities.TagGroup), request.TagGroupId, "Cannot delete TagGroup because it still contains tags. Please delete or reassign tags first.");
            }

            await _unitOfWork.TagGroupRepository.DeleteAsync(tagGroupToDelete);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("TagGroup {TagGroupId} deleted successfully.", request.TagGroupId);
            return Unit.Value;
        }
    }
}
```

### 7. Features/TranslatedMangas

#### 7.1. CreateTranslatedManga

```csharp
// Application/Features/TranslatedMangas/Commands/CreateTranslatedManga/CreateTranslatedMangaCommand.cs
using MediatR;

namespace Application.Features.TranslatedMangas.Commands.CreateTranslatedManga
{
    public class CreateTranslatedMangaCommand : IRequest<Guid>
    {
        public Guid MangaId { get; set; }
        public string LanguageKey { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}
```

```csharp
// Application/Features/TranslatedMangas/Commands/CreateTranslatedManga/CreateTranslatedMangaCommandHandler.cs
using Application.Contracts.Persistence;
using Application.Exceptions;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.TranslatedMangas.Commands.CreateTranslatedManga
{
    public class CreateTranslatedMangaCommandHandler : IRequestHandler<CreateTranslatedMangaCommand, Guid>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<CreateTranslatedMangaCommandHandler> _logger;

        public CreateTranslatedMangaCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<CreateTranslatedMangaCommandHandler> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Guid> Handle(CreateTranslatedMangaCommand request, CancellationToken cancellationToken)
        {
            var manga = await _unitOfWork.MangaRepository.GetByIdAsync(request.MangaId);
            if (manga == null)
            {
                throw new NotFoundException(nameof(Manga), request.MangaId);
            }

            // Kiểm tra xem bản dịch cho ngôn ngữ này đã tồn tại chưa
            var existingTranslation = await _unitOfWork.TranslatedMangaRepository.GetByMangaIdAndLanguageAsync(request.MangaId, request.LanguageKey);
            if (existingTranslation != null)
            {
                _logger.LogWarning("Translation for Manga {MangaId} in language '{LanguageKey}' already exists.", request.MangaId, request.LanguageKey);
                throw new Exceptions.ValidationException($"Translation for Manga ID '{request.MangaId}' in language '{request.LanguageKey}' already exists.");
            }

            var translatedManga = _mapper.Map<TranslatedManga>(request);
            // TranslatedMangaId sẽ tự sinh

            await _unitOfWork.TranslatedMangaRepository.AddAsync(translatedManga);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("TranslatedManga {TranslatedMangaId} created for Manga {MangaId} in language {LanguageKey}.",
                translatedManga.TranslatedMangaId, request.MangaId, request.LanguageKey);
            return translatedManga.TranslatedMangaId;
        }
    }
}
```

#### 7.2. UpdateTranslatedManga

```csharp
// Application/Features/TranslatedMangas/Commands/UpdateTranslatedManga/UpdateTranslatedMangaCommand.cs
using MediatR;

namespace Application.Features.TranslatedMangas.Commands.UpdateTranslatedManga
{
    public class UpdateTranslatedMangaCommand : IRequest<Unit>
    {
        public Guid TranslatedMangaId { get; set; } // Lấy từ route
        public string LanguageKey { get; set; } = string.Empty; // Cân nhắc có cho đổi không
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}
```

```csharp
// Application/Features/TranslatedMangas/Commands/UpdateTranslatedManga/UpdateTranslatedMangaCommandHandler.cs
using Application.Contracts.Persistence;
using Application.Exceptions;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.TranslatedMangas.Commands.UpdateTranslatedManga
{
    public class UpdateTranslatedMangaCommandHandler : IRequestHandler<UpdateTranslatedMangaCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<UpdateTranslatedMangaCommandHandler> _logger;

        public UpdateTranslatedMangaCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<UpdateTranslatedMangaCommandHandler> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Unit> Handle(UpdateTranslatedMangaCommand request, CancellationToken cancellationToken)
        {
            var translatedMangaToUpdate = await _unitOfWork.TranslatedMangaRepository.GetByIdAsync(request.TranslatedMangaId);
            if (translatedMangaToUpdate == null)
            {
                throw new NotFoundException(nameof(TranslatedManga), request.TranslatedMangaId);
            }

            // Nếu LanguageKey thay đổi, kiểm tra xem có bị trùng với bản dịch khác của cùng Manga không
            if (!string.Equals(translatedMangaToUpdate.LanguageKey, request.LanguageKey, StringComparison.OrdinalIgnoreCase))
            {
                var existingTranslationWithNewLang = await _unitOfWork.TranslatedMangaRepository
                    .GetByMangaIdAndLanguageAsync(translatedMangaToUpdate.MangaId, request.LanguageKey);
                
                if (existingTranslationWithNewLang != null && existingTranslationWithNewLang.TranslatedMangaId != request.TranslatedMangaId)
                {
                    _logger.LogWarning("Another translation for Manga {MangaId} in language '{LanguageKey}' already exists.", 
                        translatedMangaToUpdate.MangaId, request.LanguageKey);
                    throw new Exceptions.ValidationException($"Another translation for Manga ID '{translatedMangaToUpdate.MangaId}' in language '{request.LanguageKey}' already exists.");
                }
            }

            _mapper.Map(request, translatedMangaToUpdate);

            await _unitOfWork.TranslatedMangaRepository.UpdateAsync(translatedMangaToUpdate);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("TranslatedManga {TranslatedMangaId} updated successfully.", request.TranslatedMangaId);
            return Unit.Value;
        }
    }
}
```

#### 7.3. DeleteTranslatedManga

```csharp
// Application/Features/TranslatedMangas/Commands/DeleteTranslatedManga/DeleteTranslatedMangaCommand.cs
using MediatR;

namespace Application.Features.TranslatedMangas.Commands.DeleteTranslatedManga
{
    public class DeleteTranslatedMangaCommand : IRequest<Unit>
    {
        public Guid TranslatedMangaId { get; set; }
    }
}
```

```csharp
// Application/Features/TranslatedMangas/Commands/DeleteTranslatedManga/DeleteTranslatedMangaCommandHandler.cs
using Application.Common.Interfaces;
using Application.Contracts.Persistence;
using Application.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Linq; // Cho ToList(), Any()

namespace Application.Features.TranslatedMangas.Commands.DeleteTranslatedManga
{
    public class DeleteTranslatedMangaCommandHandler : IRequestHandler<DeleteTranslatedMangaCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPhotoAccessor _photoAccessor; // Cần để xóa ảnh của các chapter pages
        private readonly ILogger<DeleteTranslatedMangaCommandHandler> _logger;

        public DeleteTranslatedMangaCommandHandler(IUnitOfWork unitOfWork, IPhotoAccessor photoAccessor, ILogger<DeleteTranslatedMangaCommandHandler> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _photoAccessor = photoAccessor ?? throw new ArgumentNullException(nameof(photoAccessor));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Unit> Handle(DeleteTranslatedMangaCommand request, CancellationToken cancellationToken)
        {
            var translatedMangaToDelete = await _unitOfWork.TranslatedMangaRepository.GetByIdAsync(request.TranslatedMangaId);
            if (translatedMangaToDelete == null)
            {
                throw new NotFoundException(nameof(Domain.Entities.TranslatedManga), request.TranslatedMangaId);
            }

            // Lấy tất cả các chapter của TranslatedManga này để xóa ảnh
            var chapters = await _unitOfWork.ChapterRepository.GetChaptersByTranslatedMangaAsync(request.TranslatedMangaId);
            if (chapters != null && chapters.Any())
            {
                foreach (var chapter in chapters.ToList())
                {
                    var chapterWithPages = await _unitOfWork.ChapterRepository.GetChapterWithPagesAsync(chapter.ChapterId);
                    if (chapterWithPages?.ChapterPages != null && chapterWithPages.ChapterPages.Any())
                    {
                        foreach (var page in chapterWithPages.ChapterPages.ToList())
                        {
                            if (!string.IsNullOrEmpty(page.PublicId))
                            {
                                var deletionResult = await _photoAccessor.DeletePhotoAsync(page.PublicId);
                                if (deletionResult != "ok" && deletionResult != "not found")
                                {
                                    _logger.LogWarning("Failed to delete chapter page image {PublicId} from Cloudinary for chapter {ChapterId} (during TranslatedManga deletion). Result: {DeletionResult}", 
                                        page.PublicId, chapter.ChapterId, deletionResult);
                                }
                            }
                        }
                    }
                    // Chapter entities sẽ được xóa cùng TranslatedManga do Cascade Delete
                }
            }

            // Xóa TranslatedManga (DB sẽ tự động xóa Chapters và ChapterPages liên quan do cấu hình Cascade Delete)
            await _unitOfWork.TranslatedMangaRepository.DeleteAsync(translatedMangaToDelete);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("TranslatedManga {TranslatedMangaId} and its related chapters/pages deleted successfully.", request.TranslatedMangaId);
            return Unit.Value;
        }
    }
}
```