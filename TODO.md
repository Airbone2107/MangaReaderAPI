// TODO.md
# TODO: Xây dựng API quản lý dữ liệu cho MangaReaderAPI

## I. Chuẩn bị cấu trúc và các thành phần cơ bản (Core Components)

1.  **Tạo thư mục (Nếu chưa có):**
    *   `MangaReaderAPI/Application/DTOs/`
    *   `MangaReaderAPI/Application/Features/`
    *   `MangaReaderAPI/Application/Mappings/`
    *   `MangaReaderAPI/Application/Validation/`
    *   `MangaReaderAPI/Application/Contracts/Persistence/`
    *   `MangaReaderAPI/Persistence/Repositories/`
    *   `MangaReaderAPI/MangaReaderDB/Controllers/` (API Project)

2.  **Tạo file `AssemblyReference.cs` trong project `Application`:**
    *   File này giúp MediatR và AutoMapper quét các assembly.
    *   **Path:** `MangaReaderAPI/Application/AssemblyReference.cs`
    *   **Code:**
        ```csharp
        // MangaReaderAPI/Application/AssemblyReference.cs
        namespace Application
        {
            public static class AssemblyReference { }
        }
        ```

3.  **Tạo `IUnitOfWork` interface (Application Layer):**
    *   Quản lý các repository và giao dịch.
    *   **Path:** `MangaReaderAPI/Application/Contracts/Persistence/IUnitOfWork.cs`
    *   **Code:** (Sẽ được cung cấp bên dưới)

4.  **Triển khai `UnitOfWork` (Persistence Layer):**
    *   **Path:** `MangaReaderAPI/Persistence/Repositories/UnitOfWork.cs`
    *   **Code:** (Sẽ được cung cấp bên dưới)

5.  **Tạo Generic `PagedList<T>` (Nếu cần cho phân trang):**
    *   Có thể đặt trong `Application/Common/Models/` hoặc `Application/DTOs/Common/`.
    *   Hiện tại, chúng ta sẽ tập trung vào CRUD cơ bản trước.

## II. Xây dựng CRUD cho từng Entity

Chúng ta sẽ thực hiện các bước sau cho mỗi entity (Author, User, TagGroup, Tag, Manga, TranslatedManga, CoverArt, Chapter, ChapterPage). Bảng `MangaAuthor` và `MangaTag` sẽ được xử lý thông qua entity `Manga`.

**Ví dụ chi tiết cho Entity `Author`:**

### 1. `Author` Entity

    *   **DTOs (Application/DTOs/Author):**
        *   `AuthorDto.cs`: Dữ liệu hiển thị Author.
        *   `CreateAuthorDto.cs`: Dữ liệu để tạo mới Author.
        *   `UpdateAuthorDto.cs`: Dữ liệu để cập nhật Author.
    *   **Mappings (Application/Mappings):**
        *   `AuthorProfile.cs`: Cấu hình AutoMapper cho Author.
    *   **Repository Interface (Application/Contracts/Persistence):**
        *   `IAuthorRepository.cs`: Interface cho Author Repository.
    *   **Repository Implementation (Persistence/Repositories):**
        *   `AuthorRepository.cs`: Triển khai `IAuthorRepository`.
    *   **Commands & Handlers (Application/Features/Author/Commands):**
        *   `CreateAuthorCommand.cs` và `CreateAuthorCommandHandler.cs`
        *   `UpdateAuthorCommand.cs` và `UpdateAuthorCommandHandler.cs`
        *   `DeleteAuthorCommand.cs` và `DeleteAuthorCommandHandler.cs`
    *   **Queries & Handlers (Application/Features/Author/Queries):**
        *   `GetAuthorByIdQuery.cs` và `GetAuthorByIdQueryHandler.cs`
        *   `GetAllAuthorsQuery.cs` và `GetAllAuthorsQueryHandler.cs`
    *   **Validators (Application/Validation/Author):**
        *   `CreateAuthorDtoValidator.cs`
        *   `UpdateAuthorDtoValidator.cs`
    *   **Controller (MangaReaderDB/Controllers):**
        *   `AuthorsController.cs`

(Lặp lại các bước trên cho các entities khác)

### 2. `User` Entity

    *   **DTOs (Application/DTOs/User):**
        *   `UserDto.cs`
        *   `CreateUserDto.cs` (Lưu ý: `UserId` là identity, không cần truyền khi tạo)
        *   `UpdateUserDto.cs`
    *   **Mappings (Application/Mappings):**
        *   `UserProfile.cs`
    *   **Repository Interface (Application/Contracts/Persistence):**
        *   `IUserRepository.cs`
    *   **Repository Implementation (Persistence/Repositories):**
        *   `UserRepository.cs`
    *   **Commands & Handlers (Application/Features/User/Commands):**
        *   `CreateUserCommand.cs`, `UpdateUserCommand.cs`, `DeleteUserCommand.cs`
    *   **Queries & Handlers (Application/Features/User/Queries):**
        *   `GetUserByIdQuery.cs`, `GetAllUsersQuery.cs`
    *   **Validators (Application/Validation/User):**
        *   `CreateUserDtoValidator.cs`, `UpdateUserDtoValidator.cs`
    *   **Controller (MangaReaderDB/Controllers):**
        *   `UsersController.cs`

### 3. `TagGroup` Entity

    *   Tương tự như `Author`.

### 4. `Tag` Entity

    *   Tương tự như `Author`.
    *   Lưu ý khóa ngoại `TagGroupId` trong DTOs tạo/cập nhật.
    *   `CreateTagDto` cần `TagGroupId`.
    *   `UpdateTagDto` cần `TagGroupId`.

### 5. `Manga` Entity

    *   Đây là entity phức tạp, DTOs sẽ bao gồm thông tin về Authors và Tags.
    *   **DTOs (Application/DTOs/Manga):**
        *   `MangaDto.cs` (có thể chứa danh sách `AuthorDto`, `TagDto`, `CoverArtDto`, `TranslatedMangaDto`)
        *   `CreateMangaDto.cs`:
            *   Các thuộc tính của Manga.
            *   `List<Guid> AuthorIds` (hoặc `List<MangaAuthorInputDto> if role is needed for each author`).
            *   `List<Guid> TagIds`.
        *   `UpdateMangaDto.cs`: Tương tự `CreateMangaDto`.
    *   **Mappings (Application/Mappings):**
        *   `MangaProfile.cs` (cần map `MangaAuthors` và `MangaTags` cẩn thận).
    *   **Repository Interface (Application/Contracts/Persistence):**
        *   `IMangaRepository.cs` (cần các phương thức để load kèm Authors, Tags).
    *   **Repository Implementation (Persistence/Repositories):**
        *   `MangaRepository.cs`.
    *   **Commands & Handlers (Application/Features/Manga/Commands):**
        *   `CreateMangaCommand.cs`: Xử lý tạo Manga, và các bản ghi trong `MangaAuthors`, `MangaTags`.
        *   `UpdateMangaCommand.cs`: Xử lý cập nhật Manga, và các bản ghi trong `MangaAuthors`, `MangaTags` (xóa cũ, thêm mới).
        *   `DeleteMangaCommand.cs`.
    *   **Queries & Handlers (Application/Features/Manga/Queries):**
        *   `GetMangaByIdQuery.cs` (load kèm details).
        *   `GetAllMangasQuery.cs` (có thể có version basic và version load details).
    *   **Validators (Application/Validation/Manga):**
        *   `CreateMangaDtoValidator.cs`.
        *   `UpdateMangaDtoValidator.cs`.
    *   **Controller (MangaReaderDB/Controllers):**
        *   `MangasController.cs`.

### 6. `TranslatedManga` Entity

    *   Tương tự như `Author`.
    *   Lưu ý khóa ngoại `MangaId`.
    *   `CreateTranslatedMangaDto` cần `MangaId`, `LanguageKey`, `Title`, `Description`.
    *   `UpdateTranslatedMangaDto` tương tự.

### 7. `CoverArt` Entity

    *   Tương tự như `Author`, nhưng có xử lý upload ảnh.
    *   **DTOs (Application/DTOs/CoverArt):**
        *   `CoverArtDto.cs` (chứa `PublicId`, `Url` có thể được xây dựng ở Frontend).
        *   `CreateCoverArtDto.cs`:
            *   `Guid MangaId`.
            *   `IFormFile File` (Controller sẽ truyền `Stream` và `fileName` cho Command).
            *   `string? Volume`, `string? Description`.
        *   `UpdateCoverArtDto.cs` (có thể cho phép cập nhật file hoặc không, hoặc chỉ metadata).
    *   **Commands & Handlers (Application/Features/CoverArt/Commands):**
        *   `CreateCoverArtCommand.cs`:
            *   Nhận `Stream fileStream`, `string fileName`, `Guid mangaId`, etc.
            *   Gọi `_photoAccessor.UploadPhotoAsync()`.
            *   Lưu `PublicId` vào `CoverArt` entity.
        *   `DeleteCoverArtCommand.cs`:
            *   Lấy `PublicId` từ `CoverArt`.
            *   Gọi `_photoAccessor.DeletePhotoAsync()`.
            *   Xóa entity `CoverArt`.
    *   **Controller (MangaReaderDB/Controllers):**
        *   `CoverArtsController.cs`. Controller sẽ nhận `IFormFile` từ request và chuẩn bị `Stream`, `fileName` cho command.

### 8. `Chapter` Entity

    *   Tương tự như `Author`.
    *   Lưu ý khóa ngoại `TranslatedMangaId`, `UploadedByUserId`.
    *   `CreateChapterDto` cần các thông tin này.
    *   `UpdateChapterDto` tương tự.

### 9. `ChapterPage` Entity

    *   Tương tự `CoverArt` về xử lý ảnh.
    *   **DTOs (Application/DTOs/ChapterPage):**
        *   `ChapterPageDto.cs` (chứa `PublicId`, `PageNumber`).
        *   `CreateChapterPageDto.cs`:
            *   `Guid ChapterId`.
            *   `IFormFile File`.
            *   `int PageNumber`.
        *   `BulkCreateChapterPagesDto.cs` (Cho phép upload nhiều trang cho 1 chapter):
            *   `Guid ChapterId`.
            *   `List<IFormFile> Files` (Controller sẽ xử lý từng file).
            *   `int StartPageNumber` (tùy chọn, hoặc để handler tự xác định).
    *   **Commands & Handlers (Application/Features/ChapterPage/Commands):**
        *   `CreateChapterPageCommand.cs`:
            *   Nhận `Stream fileStream`, `string fileName`, `Guid chapterId`, `int pageNumber`.
            *   Gọi `_photoAccessor.UploadPhotoAsync()`.
            *   Lưu `PublicId` vào `ChapterPage` entity.
        *   `DeleteChapterPageCommand.cs`:
            *   Lấy `PublicId` từ `ChapterPage`.
            *   Gọi `_photoAccessor.DeletePhotoAsync()`.
            *   Xóa entity `ChapterPage`.
    *   **Controller (MangaReaderDB/Controllers):**
        *   `ChapterPagesController.cs`.

## III. Cập nhật `Program.cs` (trong `MangaReaderDB`)

1.  **Đăng ký MediatR:**
    ```csharp
    builder.Services.AddMediatR(cfg => 
        cfg.RegisterServicesFromAssembly(typeof(Application.AssemblyReference).Assembly));
    ```

2.  **Đăng ký AutoMapper:**
    ```csharp
    builder.Services.AddAutoMapper(typeof(Application.AssemblyReference).Assembly);
    ```

3.  **Đăng ký FluentValidation Validators:**
    ```csharp
    using FluentValidation;
    // ...
    builder.Services.AddValidatorsFromAssembly(typeof(Application.AssemblyReference).Assembly, ServiceLifetime.Scoped);
    ```

4.  **Đăng ký Repositories và UnitOfWork:**
    ```csharp
    // Ví dụ cho Author
    // builder.Services.AddScoped<IAuthorRepository, AuthorRepository>(); 
    // Nên đăng ký UnitOfWork, các repositories sẽ được khởi tạo trong UnitOfWork
    builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
    ```
    Trong `IUnitOfWork` sẽ có các thuộc tính cho từng repository, ví dụ `IAuthorRepository Authors { get; }`. `UnitOfWork.cs` sẽ khởi tạo chúng.

## IV. Middleware Xử lý Lỗi Toàn Cục (Khuyến Nghị)

*   Tạo một middleware để bắt các exception và trả về JSON response chuẩn hóa.
*   **Path:** `MangaReaderAPI/MangaReaderDB/Middleware/ErrorHandlingMiddleware.cs` (ví dụ)
*   Đăng ký middleware trong `Program.cs`: `app.UseMiddleware<ErrorHandlingMiddleware>();`

---

Đây là kế hoạch tổng thể. Bây giờ tôi sẽ cung cấp code cho các phần cơ bản và entity `Author`.
```

Dưới đây là mã nguồn cho các file cơ bản và entity `Author`:

**1. File `AssemblyReference.cs` (Đã có trong TODO.md)**

**2. `IUnitOfWork.cs` và `UnitOfWork.cs`**

```csharp
// MangaReaderAPI/Application/Contracts/Persistence/IUnitOfWork.cs
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Contracts.Persistence
{
    public interface IUnitOfWork : IDisposable
    {
        IAuthorRepository AuthorRepository { get; }
        IUserRepository UserRepository { get; }
        ITagGroupRepository TagGroupRepository { get; }
        ITagRepository TagRepository { get; }
        IMangaRepository MangaRepository { get; }
        ITranslatedMangaRepository TranslatedMangaRepository { get; }
        IChapterRepository ChapterRepository { get; }
        IChapterPageRepository ChapterPageRepository { get; }
        ICoverArtRepository CoverArtRepository { get; }
        // Thêm các repository khác ở đây

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
```

```csharp
// MangaReaderAPI/Persistence/Repositories/UnitOfWork.cs
using Application.Contracts.Persistence;
using Persistence.Data;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Persistence.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IAuthorRepository? _authorRepository;
        private IUserRepository? _userRepository;
        private ITagGroupRepository? _tagGroupRepository;
        private ITagRepository? _tagRepository;
        private IMangaRepository? _mangaRepository;
        private ITranslatedMangaRepository? _translatedMangaRepository;
        private IChapterRepository? _chapterRepository;
        private IChapterPageRepository? _chapterPageRepository;
        private ICoverArtRepository? _coverArtRepository;
        // ... Khai báo các repository khác

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public IAuthorRepository AuthorRepository => _authorRepository ??= new AuthorRepository(_context);
        public IUserRepository UserRepository => _userRepository ??= new UserRepository(_context);
        public ITagGroupRepository TagGroupRepository => _tagGroupRepository ??= new TagGroupRepository(_context);
        public ITagRepository TagRepository => _tagRepository ??= new TagRepository(_context);
        public IMangaRepository MangaRepository => _mangaRepository ??= new MangaRepository(_context);
        public ITranslatedMangaRepository TranslatedMangaRepository => _translatedMangaRepository ??= new TranslatedMangaRepository(_context);
        public IChapterRepository ChapterRepository => _chapterRepository ??= new ChapterRepository(_context);
        public IChapterPageRepository ChapterPageRepository => _chapterPageRepository ??= new ChapterPageRepository(_context);
        public ICoverArtRepository CoverArtRepository => _coverArtRepository ??= new CoverArtRepository(_context);
        // ... Triển khai các thuộc tính repository khác

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        public void Dispose()
        {
            _context.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
```

**3. Entity `Author`**

**DTOs:**

```csharp
// MangaReaderAPI/Application/DTOs/Author/AuthorDto.cs
namespace Application.DTOs.Author
{
    public class AuthorDto
    {
        public Guid AuthorId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Biography { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
```

```csharp
// MangaReaderAPI/Application/DTOs/Author/CreateAuthorDto.cs
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Author
{
    public class CreateAuthorDto
    {
        [Required(ErrorMessage = "Tên tác giả không được để trống.")]
        [MaxLength(255, ErrorMessage = "Tên tác giả không được vượt quá 255 ký tự.")]
        public string Name { get; set; } = string.Empty;

        public string? Biography { get; set; }
    }
}
```

```csharp
// MangaReaderAPI/Application/DTOs/Author/UpdateAuthorDto.cs
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Author
{
    public class UpdateAuthorDto
    {
        [Required(ErrorMessage = "Tên tác giả không được để trống.")]
        [MaxLength(255, ErrorMessage = "Tên tác giả không được vượt quá 255 ký tự.")]
        public string Name { get; set; } = string.Empty;

        public string? Biography { get; set; }
    }
}
```

**AutoMapper Profile:**

```csharp
// MangaReaderAPI/Application/Mappings/AuthorProfile.cs
using AutoMapper;
using Domain.Entities;
using Application.DTOs.Author;

namespace Application.Mappings
{
    public class AuthorProfile : Profile
    {
        public AuthorProfile()
        {
            CreateMap<Author, AuthorDto>();
            CreateMap<CreateAuthorDto, Author>();
            CreateMap<UpdateAuthorDto, Author>();
        }
    }
}
```

**Repository Interface:**
(Bạn cần tạo các base repository interface và implementation để tránh lặp code, nhưng để đơn giản, tôi sẽ tạo trực tiếp)

```csharp
// MangaReaderAPI/Application/Contracts/Persistence/IAuthorRepository.cs
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Contracts.Persistence
{
    public interface IAuthorRepository
    {
        Task<Author?> GetByIdAsync(Guid id);
        Task<IReadOnlyList<Author>> GetAllAsync();
        Task<Author> AddAsync(Author entity);
        Task UpdateAsync(Author entity); // EF Core theo dõi thay đổi, nên không cần trả về entity
        Task DeleteAsync(Author entity);
        Task<bool> ExistsAsync(Guid id);
    }
}
```

**Repository Implementation:**

```csharp
// MangaReaderAPI/Persistence/Repositories/AuthorRepository.cs
using Application.Contracts.Persistence;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Persistence.Repositories
{
    public class AuthorRepository : IAuthorRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public AuthorRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<Author?> GetByIdAsync(Guid id)
        {
            return await _dbContext.Authors.FindAsync(id);
        }

        public async Task<IReadOnlyList<Author>> GetAllAsync()
        {
            return await _dbContext.Authors.ToListAsync();
        }

        public async Task<Author> AddAsync(Author entity)
        {
            await _dbContext.Authors.AddAsync(entity);
            // SaveChangesAsync sẽ được gọi bởi UnitOfWork
            return entity;
        }

        public Task UpdateAsync(Author entity)
        {
            // EF Core theo dõi entity này, không cần làm gì đặc biệt ở đây
            // _dbContext.Entry(entity).State = EntityState.Modified; // Nếu entity không được theo dõi
            // SaveChangesAsync sẽ được gọi bởi UnitOfWork
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Author entity)
        {
            _dbContext.Authors.Remove(entity);
            // SaveChangesAsync sẽ được gọi bởi UnitOfWork
            return Task.CompletedTask;
        }
        
        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _dbContext.Authors.AnyAsync(e => e.AuthorId == id);
        }
    }
}
```

**Commands & Handlers:**

```csharp
// MangaReaderAPI/Application/Features/Author/Commands/CreateAuthorCommand.cs
using MediatR;
using Application.DTOs.Author;
using System;

namespace Application.Features.Author.Commands
{
    public class CreateAuthorCommand : IRequest<Guid> // Trả về ID của Author mới tạo
    {
        public CreateAuthorDto CreateAuthorDto { get; set; } = null!;
    }

    public class CreateAuthorCommandHandler : IRequestHandler<CreateAuthorCommand, Guid>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CreateAuthorCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Guid> Handle(CreateAuthorCommand request, CancellationToken cancellationToken)
        {
            var author = _mapper.Map<Domain.Entities.Author>(request.CreateAuthorDto);
            
            await _unitOfWork.AuthorRepository.AddAsync(author);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return author.AuthorId;
        }
    }
}
```

```csharp
// MangaReaderAPI/Application/Features/Author/Commands/UpdateAuthorCommand.cs
using MediatR;
using Application.DTOs.Author;
using System;

namespace Application.Features.Author.Commands
{
    public class UpdateAuthorCommand : IRequest<Unit> // Hoặc IRequest<AuthorDto> nếu muốn trả về DTO
    {
        public Guid AuthorId { get; set; }
        public UpdateAuthorDto UpdateAuthorDto { get; set; } = null!;
    }

    public class UpdateAuthorCommandHandler : IRequestHandler<UpdateAuthorCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UpdateAuthorCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Unit> Handle(UpdateAuthorCommand request, CancellationToken cancellationToken)
        {
            var authorToUpdate = await _unitOfWork.AuthorRepository.GetByIdAsync(request.AuthorId);

            if (authorToUpdate == null)
            {
                // Xử lý lỗi không tìm thấy, ví dụ throw một NotFoundException
                // Hoặc trả về một Result object để controller xử lý
                throw new Exceptions.NotFoundException(nameof(Domain.Entities.Author), request.AuthorId);
            }

            _mapper.Map(request.UpdateAuthorDto, authorToUpdate);
            // AuthorRepository.UpdateAsync không làm gì nhiều vì EF Core tracking
            // await _unitOfWork.AuthorRepository.UpdateAsync(authorToUpdate); // Không thực sự cần thiết nếu entity được track

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
```

```csharp
// MangaReaderAPI/Application/Features/Author/Commands/DeleteAuthorCommand.cs
using MediatR;
using System;

namespace Application.Features.Author.Commands
{
    public class DeleteAuthorCommand : IRequest<Unit>
    {
        public Guid AuthorId { get; set; }
    }

    public class DeleteAuthorCommandHandler : IRequestHandler<DeleteAuthorCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;

        public DeleteAuthorCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Unit> Handle(DeleteAuthorCommand request, CancellationToken cancellationToken)
        {
            var authorToDelete = await _unitOfWork.AuthorRepository.GetByIdAsync(request.AuthorId);

            if (authorToDelete == null)
            {
                throw new Exceptions.NotFoundException(nameof(Domain.Entities.Author), request.AuthorId);
            }

            await _unitOfWork.AuthorRepository.DeleteAsync(authorToDelete);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
```
*Lưu ý:* Bạn cần tạo `NotFoundException` trong `Application/Exceptions/NotFoundException.cs`

```csharp
// MangaReaderAPI/Application/Exceptions/NotFoundException.cs
using System;

namespace Application.Exceptions
{
    public class NotFoundException : ApplicationException
    {
        public NotFoundException(string name, object key)
            : base($"Entity \"{name}\" ({key}) was not found.")
        {
        }
    }
}
```

**Queries & Handlers:**

```csharp
// MangaReaderAPI/Application/Features/Author/Queries/GetAuthorByIdQuery.cs
using MediatR;
using Application.DTOs.Author;
using System;

namespace Application.Features.Author.Queries
{
    public class GetAuthorByIdQuery : IRequest<AuthorDto?>
    {
        public Guid AuthorId { get; set; }
    }

    public class GetAuthorByIdQueryHandler : IRequestHandler<GetAuthorByIdQuery, AuthorDto?>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetAuthorByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<AuthorDto?> Handle(GetAuthorByIdQuery request, CancellationToken cancellationToken)
        {
            var author = await _unitOfWork.AuthorRepository.GetByIdAsync(request.AuthorId);
            return _mapper.Map<AuthorDto>(author);
        }
    }
}
```

```csharp
// MangaReaderAPI/Application/Features/Author/Queries/GetAllAuthorsQuery.cs
using MediatR;
using Application.DTOs.Author;
using System.Collections.Generic;

namespace Application.Features.Author.Queries
{
    public class GetAllAuthorsQuery : IRequest<IReadOnlyList<AuthorDto>>
    {
    }

    public class GetAllAuthorsQueryHandler : IRequestHandler<GetAllAuthorsQuery, IReadOnlyList<AuthorDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetAllAuthorsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IReadOnlyList<AuthorDto>> Handle(GetAllAuthorsQuery request, CancellationToken cancellationToken)
        {
            var authors = await _unitOfWork.AuthorRepository.GetAllAsync();
            return _mapper.Map<IReadOnlyList<AuthorDto>>(authors);
        }
    }
}
```

**Validators:**

```csharp
// MangaReaderAPI/Application/Validation/Author/CreateAuthorDtoValidator.cs
using FluentValidation;
using Application.DTOs.Author;

namespace Application.Validation.Author
{
    public class CreateAuthorDtoValidator : AbstractValidator<CreateAuthorDto>
    {
        public CreateAuthorDtoValidator()
        {
            RuleFor(p => p.Name)
                .NotEmpty().WithMessage("{PropertyName} không được để trống.")
                .MaximumLength(255).WithMessage("{PropertyName} không được vượt quá 255 ký tự.");

            // Thêm các rule khác cho Biography nếu cần
        }
    }
}
```

```csharp
// MangaReaderAPI/Application/Validation/Author/UpdateAuthorDtoValidator.cs
using FluentValidation;
using Application.DTOs.Author;

namespace Application.Validation.Author
{
    public class UpdateAuthorDtoValidator : AbstractValidator<UpdateAuthorDto>
    {
        public UpdateAuthorDtoValidator()
        {
            RuleFor(p => p.Name)
                .NotEmpty().WithMessage("{PropertyName} không được để trống.")
                .MaximumLength(255).WithMessage("{PropertyName} không được vượt quá 255 ký tự.");
            // Thêm các rule khác cho Biography nếu cần
        }
    }
}
```

**Controller:**

```csharp
// MangaReaderAPI/MangaReaderDB/Controllers/AuthorsController.cs
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.DTOs.Author;
using Application.Features.Author.Commands;
using Application.Features.Author.Queries;
using FluentValidation;
using System.Linq;
using Application.Exceptions; // Cần cho NotFoundException

namespace MangaReaderDB.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthorsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IValidator<CreateAuthorDto> _createAuthorDtoValidator;
        private readonly IValidator<UpdateAuthorDto> _updateAuthorDtoValidator;

        public AuthorsController(IMediator mediator, 
                                 IValidator<CreateAuthorDto> createAuthorDtoValidator,
                                 IValidator<UpdateAuthorDto> updateAuthorDtoValidator)
        {
            _mediator = mediator;
            _createAuthorDtoValidator = createAuthorDtoValidator;
            _updateAuthorDtoValidator = updateAuthorDtoValidator;
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<AuthorDto>>> GetAllAuthors()
        {
            var query = new GetAllAuthorsQuery();
            var authors = await _mediator.Send(query);
            return Ok(authors);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<AuthorDto>> GetAuthorById(Guid id)
        {
            var query = new GetAuthorByIdQuery { AuthorId = id };
            var author = await _mediator.Send(query);
            
            if (author == null)
            {
                return NotFound(new { Message = $"Author with id {id} not found." });
            }
            return Ok(author);
        }

        [HttpPost]
        public async Task<ActionResult<Guid>> CreateAuthor([FromBody] CreateAuthorDto createAuthorDto)
        {
            var validationResult = await _createAuthorDtoValidator.ValidateAsync(createAuthorDto);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => new { e.PropertyName, e.ErrorMessage });
                return BadRequest(new { Title = "Validation Failed", Errors = errors });
            }

            var command = new CreateAuthorCommand { CreateAuthorDto = createAuthorDto };
            var authorId = await _mediator.Send(command);
            
            // Trả về 201 Created với URI của resource mới và ID
            return CreatedAtAction(nameof(GetAuthorById), new { id = authorId }, new { Id = authorId });
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateAuthor(Guid id, [FromBody] UpdateAuthorDto updateAuthorDto)
        {
            var validationResult = await _updateAuthorDtoValidator.ValidateAsync(updateAuthorDto);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => new { e.PropertyName, e.ErrorMessage });
                return BadRequest(new { Title = "Validation Failed", Errors = errors });
            }

            var command = new UpdateAuthorCommand { AuthorId = id, UpdateAuthorDto = updateAuthorDto };
            try
            {
                await _mediator.Send(command);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            // Có thể bắt các exceptions khác ở đây hoặc dùng middleware
            
            return NoContent(); // 204 No Content là response chuẩn cho PUT thành công không trả về body
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteAuthor(Guid id)
        {
            var command = new DeleteAuthorCommand { AuthorId = id };
            try
            {
                await _mediator.Send(command);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            // Có thể bắt các exceptions khác ở đây hoặc dùng middleware

            return NoContent(); // 204 No Content
        }
    }
}
```

**4. Cập nhật `Program.cs`**

Bạn cần thêm các đăng ký dịch vụ vào file `MangaReaderAPI/MangaReaderDB/Program.cs`:

```csharp
// MangaReaderDB/Program.cs
using Microsoft.EntityFrameworkCore;
using Persistence.Data; 
using Persistence.Data.Interceptors;
using Infrastructure.Photos;
using Application.Common.Interfaces; // Cần cho IPhotoAccessor
using Application.Contracts.Persistence; // Cần cho IUnitOfWork
using Persistence.Repositories; // Cần cho UnitOfWork
using MediatR; // Cần cho AddMediatR
using FluentValidation; // Cần cho AddValidatorsFromAssembly
// ...

var builder = WebApplication.CreateBuilder(args);

// Đăng ký Interceptor như một Singleton
builder.Services.AddSingleton<AuditableEntitySaveChangesInterceptor>();

// Đăng ký ApplicationDbContext và thêm Interceptor
builder.Services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
{
    var interceptor = serviceProvider.GetRequiredService<AuditableEntitySaveChangesInterceptor>();
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
           .AddInterceptors(interceptor); 
});

// Cấu hình CloudinarySettings
builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("CloudinarySettings"));

// Đăng ký PhotoAccessor
builder.Services.AddScoped<IPhotoAccessor, PhotoAccessor>();

// Đăng ký MediatR
builder.Services.AddMediatR(cfg => 
    cfg.RegisterServicesFromAssembly(typeof(Application.AssemblyReference).Assembly));

// Đăng ký AutoMapper
builder.Services.AddAutoMapper(typeof(Application.AssemblyReference).Assembly);

// Đăng ký FluentValidation Validators
builder.Services.AddValidatorsFromAssembly(typeof(Application.AssemblyReference).Assembly, ServiceLifetime.Scoped);

// Đăng ký UnitOfWork (bao gồm các repositories)
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
// Không cần đăng ký từng repository riêng lẻ nếu chúng được cung cấp qua IUnitOfWork

// Các services khác của ASP.NET Core
builder.Services.AddControllers();
// ... phần còn lại của Program.cs
```

Bạn hãy áp dụng cấu trúc tương tự cho các entities còn lại theo hướng dẫn trong `TODO.md`. Đặc biệt lưu ý các entity có xử lý file như `CoverArt`, `ChapterPage` và các entity phức tạp như `Manga`.

Nếu bạn có bất kỳ câu hỏi nào hoặc cần trợ giúp thêm với một entity cụ thể, đừng ngần ngại hỏi nhé!