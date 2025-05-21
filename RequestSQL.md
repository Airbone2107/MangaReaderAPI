Yêu Cầu Phát Triển API và Xử lý Dữ liệu

## I. Tổng Quan và Nguyên Tắc Chung

1.  **Kiến trúc DDD và Phân Tầng Rõ Ràng:**
    *   **Domain Layer (`Domain`):**
        *   Chỉ chứa Entities (POCOs), Enums, Value Objects.
        *   **Không** chứa bất kỳ logic nào liên quan đến cơ sở dữ liệu (EF Core), DTOs, hoặc các thành phần của Application/Infrastructure layer.
        *   Entities nên tự quản lý trạng thái và business rules cốt lõi của chúng (rich domain model nếu phù hợp).
    *   **Application Layer (`Application`):**
        *   Triển khai các use cases của ứng dụng.
        *   Sử dụng **MediatR** để xử lý Commands (thay đổi dữ liệu) và Queries (đọc dữ liệu).
        *   Định nghĩa DTOs (Data Transfer Objects) để truyền dữ liệu giữa Presentation Layer và Application Layer.
        *   Định nghĩa Interfaces cho Repositories (ví dụ: `IMangaRepository`, `IChapterRepository`) và các dịch vụ bên ngoài (ví dụ: `IPhotoAccessor`).
        *   Sử dụng **FluentValidation** để validate DTOs/Commands/Queries.
        *   **Không** chứa logic truy cập trực tiếp vào `DbContext`. Thay vào đó, sử dụng các Repository Interfaces.
    *   **Persistence Layer (`Persistence`):**
        *   Triển khai `ApplicationDbContext`.
        *   Triển khai các Repository Interfaces đã định nghĩa trong Application Layer.
        *   Chứa tất cả logic truy vấn EF Core (LINQ to Entities).
        *   Chứa các Interceptors liên quan đến EF Core (ví dụ: `AuditableEntitySaveChangesInterceptor`).
    *   **Infrastructure Layer (`Infrastructure`):**
        *   Triển khai các Interface từ Application Layer cho các dịch vụ bên ngoài (ví dụ: `PhotoAccessor` triển khai `IPhotoAccessor` cho Cloudinary, dịch vụ email, etc.).
        *   Chứa cấu hình và logic tương tác với các dịch vụ bên thứ ba.
    *   **Presentation Layer (`MangaReaderDB` - API):**
        *   Chứa API Controllers.
        *   Controllers **CHỈ** chịu trách nhiệm nhận HTTP requests, chuẩn bị dữ liệu đầu vào (ví dụ: `IFormFile` thành `Stream` và `fileName` cho `IPhotoAccessor`), gửi Commands/Queries đến MediatR, và trả về HTTP responses (sử dụng DTOs từ Application Layer).
        *   **Không** chứa business logic hoặc logic truy cập dữ liệu trực tiếp.
        *   Thực hiện **validation thủ công** các DTOs đầu vào bằng FluentValidation trước khi gửi đến MediatR.

2.  **Code First:** Database schema được quản lý hoàn toàn thông qua EF Core Migrations. **Không** chỉnh sửa database schema trực tiếp.

3.  **Async Everywhere:** Ưu tiên sử dụng `async/await` cho tất cả các thao tác I/O (đặc biệt là truy cập database và các lời gọi API bên ngoài) để tránh blocking thread.

## II. Sử dụng MediatR (Application Layer)

1.  **Commands và Queries:**
    *   Mọi yêu cầu thay đổi dữ liệu (Create, Update, Delete) **PHẢI** được thực hiện thông qua **Commands**.
        *   Mỗi Command **PHẢI** có một Handler tương ứng (ví dụ: `CreateMangaCommand` -> `CreateMangaCommandHandler`).
        *   Command Handlers chịu trách nhiệm:
            *   Validate command (sử dụng FluentValidation nếu cần cho logic nghiệp vụ phức tạp, nếu không thì DTO đã được validate ở Controller).
            *   Tương tác với Repositories (thông qua `IUnitOfWork` nếu có) và các Services (ví dụ: `IPhotoAccessor`) để thực hiện nghiệp vụ.
            *   Trả về kết quả (ví dụ: ID của entity mới tạo, `Unit` của MediatR cho các lệnh không trả về dữ liệu, hoặc một DTO kết quả).
    *   Mọi yêu cầu đọc dữ liệu **PHẢI** được thực hiện thông qua **Queries**.
        *   Mỗi Query **PHẢI** có một Handler tương ứng (ví dụ: `GetMangaByIdQuery` -> `GetMangaByIdQueryHandler`).
        *   Query Handlers chịu trách nhiệm:
            *   Tương tác với Repositories để lấy dữ liệu.
            *   Sử dụng **AutoMapper** để map từ Entities sang DTOs.
            *   Trả về DTOs.

2.  **Đăng ký MediatR:**
    *   Đăng ký MediatR trong `Program.cs` của project API (`MangaReaderDB`).
        ```csharp
        // MangaReaderDB/Program.cs
        // ...
        builder.Services.AddMediatR(cfg => 
            cfg.RegisterServicesFromAssembly(typeof(Application.AssemblyReference).Assembly)); // Giả sử có class AssemblyReference trong project Application
        // ...
        ```
    *   Bạn cần tạo một class trống `AssemblyReference` trong project `Application` để MediatR có thể quét assembly đó:
        ```csharp
        // Application/AssemblyReference.cs
        namespace Application
        {
            public static class AssemblyReference { }
        }
        ```

## III. Sử dụng AutoMapper (Application Layer)

1.  **Mapping Profiles:**
    *   Tạo các Mapping Profiles (kế thừa từ `Profile` của AutoMapper) trong Application Layer để định nghĩa cách map giữa Entities và DTOs.
    *   Ví dụ: `MangaProfile.cs`, `ChapterProfile.cs`.
    *   Mỗi profile nên chứa các mapping liên quan đến một hoặc một vài entity/DTO.
        ```csharp
        // Application/Mappings/MangaProfile.cs
        using AutoMapper;
        using Domain.Entities;
        using Application.DTOs.Manga; // Giả sử bạn có DTOs này

        namespace Application.Mappings
        {
            public class MangaProfile : Profile
            {
                public MangaProfile()
                {
                    CreateMap<Manga, MangaDto>(); // Entity -> DTO
                    CreateMap<CreateMangaDto, Manga>(); // DTO -> Entity (cho lệnh Create)
                    CreateMap<UpdateMangaDto, Manga>(); // DTO -> Entity (cho lệnh Update)
                    // ... các mapping khác
                }
            }
        }
        ```

2.  **Đăng ký AutoMapper:**
    *   Đăng ký AutoMapper trong `Program.cs` của project API (`MangaReaderDB`).
        ```csharp
        // MangaReaderDB/Program.cs
        // ...
        builder.Services.AddAutoMapper(typeof(Application.AssemblyReference).Assembly); // Giả sử có class AssemblyReference trong project Application
        // ...
        ```

3.  **Sử dụng trong Handlers:**
    *   Inject `IMapper` vào các Command/Query Handlers.
    *   Sử dụng `_mapper.Map<DestinationType>(sourceObject)` để thực hiện mapping.

## IV. Sử dụng FluentValidation (Application Layer và Presentation Layer)

1.  **Validators (Application Layer):**
    *   Tạo các class Validator (kế thừa từ `AbstractValidator<T>`) cho mỗi DTO/Command/Query cần validate trong Application Layer.
    *   Đặt các Validators trong một thư mục như `Validation` trong Application Layer.
    *   Ví dụ:
        ```csharp
        // Application/Validation/Manga/CreateMangaDtoValidator.cs
        using FluentValidation;
        using Application.DTOs.Manga; // Giả sử có DTO này
        using Domain.Enums; // Cần cho Enums

        namespace Application.Validation.Manga
        {
            public class CreateMangaDtoValidator : AbstractValidator<CreateMangaDto>
            {
                public CreateMangaDtoValidator()
                {
                    RuleFor(x => x.Title)
                        .NotEmpty().WithMessage("Tiêu đề không được để trống.")
                        .MaximumLength(255).WithMessage("Tiêu đề không được vượt quá 255 ký tự.");

                    RuleFor(x => x.OriginalLanguage)
                        .NotEmpty().WithMessage("Ngôn ngữ gốc không được để trống.")
                        .Length(2, 10).WithMessage("Mã ngôn ngữ phải từ 2 đến 10 ký tự.");
                    
                    RuleFor(x => x.Status).IsInEnum().WithMessage("Trạng thái không hợp lệ.");
                    RuleFor(x => x.ContentRating).IsInEnum().WithMessage("Đánh giá nội dung không hợp lệ.");

                    RuleFor(x => x.PublicationDemographic)
                        .Must(x => x == null || Enum.IsDefined(typeof(PublicationDemographic), x))
                        .When(x => x.HasValue) // Chỉ validate khi có giá trị
                        .WithMessage("Đối tượng xuất bản không hợp lệ.");

                    RuleFor(x => x.Year)
                        .InclusiveBetween(1900, DateTime.UtcNow.Year + 5).When(x => x.Year.HasValue)
                        .WithMessage($"Năm phải từ 1900 đến {DateTime.UtcNow.Year + 5}.");
                }
            }
        }
        ```

2.  **Đăng ký Validators (Presentation Layer - `MangaReaderDB/Program.cs`):**
    *   Đăng ký các validator với Dependency Injection.
        ```csharp
        // MangaReaderDB/Program.cs
        using FluentValidation; 

        // ...
        // Đăng ký tất cả validators từ Assembly của Application layer
        builder.Services.AddValidatorsFromAssembly(typeof(Application.AssemblyReference).Assembly, ServiceLifetime.Scoped); // ServiceLifetime.Scoped thường phù hợp
        // ...
        ```

3.  **Validation Thủ Công trong Controllers (Presentation Layer):**
    *   Inject `IValidator<T>` vào Controller.
    *   Trước khi gửi Command/Query đến MediatR, gọi phương thức `ValidateAsync` và xử lý kết quả.
    *   Ví dụ:
        ```csharp
        // MangaReaderDB/Controllers/MangaController.cs
        using FluentValidation;
        using Application.DTOs.Manga; // Giả sử có DTO này
        using Application.Features.Manga.Commands; // Giả sử có command
        using MediatR;
        using Microsoft.AspNetCore.Mvc;
        using System.Linq; 
        using System.Threading.Tasks;

        [ApiController]
        [Route("api/[controller]")]
        public class MangasController : ControllerBase // Đổi tên Controller thành số nhiều
        {
            private readonly IMediator _mediator;
            private readonly IValidator<CreateMangaDto> _createMangaDtoValidator; 

            public MangasController(IMediator mediator, IValidator<CreateMangaDto> createMangaDtoValidator)
            {
                _mediator = mediator;
                _createMangaDtoValidator = createMangaDtoValidator;
            }

            [HttpPost]
            public async Task<IActionResult> CreateManga([FromBody] CreateMangaDto createMangaDto)
            {
                var validationResult = await _createMangaDtoValidator.ValidateAsync(createMangaDto);

                if (!validationResult.IsValid)
                {
                    var errors = validationResult.Errors.Select(e => new { e.PropertyName, e.ErrorMessage });
                    return BadRequest(new { Title = "Validation Failed", Errors = errors });
                }

                // Giả sử CreateMangaCommand nhận DTO:
                // var command = new CreateMangaCommand { CreateMangaDto = createMangaDto };
                // Hoặc map thủ công/AutoMapper từ DTO sang Command properties
                var command = new CreateMangaCommand { /* ... map properties ... */ }; 
                var result = await _mediator.Send(command);
                
                // return CreatedAtAction(nameof(GetMangaById), new { id = result.Id }, result); // Ví dụ
                return Ok(result); 
            }

            // ... các actions khác
        }
        ```

## V. Xử lý Logic `CreatedAt`, `UpdatedAt`

1.  **Interceptor:** `AuditableEntitySaveChangesInterceptor` (đã tạo trong `Persistence`) **PHẢI** được đăng ký và sử dụng với `DbContext`.
    *   Interceptor này sẽ tự động cập nhật các trường `CreatedAt`, `UpdatedAt` khi thêm mới (`EntityState.Added`).
    *   Tự động cập nhật `UpdatedAt` khi cập nhật (`EntityState.Modified`).
    *   **Không** cần code xử lý các trường này trong Command Handlers hay Repositories nữa.
    *   Lưu ý: Thuộc tính `Version` đã được loại bỏ khỏi `AuditableEntity` và các entities liên quan.

## VI. Repositories (Persistence Layer)

1.  **Định nghĩa Interfaces (Application Layer):**
    *   Ví dụ: `IMangaRepository`, `IAuthorRepository`, `IUnitOfWork`.
    *   Chứa các phương thức trừu tượng cho các thao tác dữ liệu (ví dụ: `GetByIdAsync`, `GetAllAsync`, `AddAsync`, `UpdateAsync`, `DeleteAsync`).
2.  **Triển khai Repositories (Persistence Layer):**
    *   Các class triển khai sẽ inject `ApplicationDbContext`.
    *   Sử dụng LINQ to Entities để truy vấn và cập nhật dữ liệu.
    *   **Không** gọi `_dbContext.SaveChangesAsync()` trực tiếp trong các phương thức của Repository. Việc này nên được quản lý bởi `UnitOfWork` pattern.
3.  **Unit of Work Pattern (BẮT BUỘC):**
    *   Tạo interface `IUnitOfWork` trong `Application/Common/Interfaces/` (hoặc `Application/Contracts/Persistence/`), chứa phương thức `Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);` và các thuộc tính cho từng Repository.
    *   Triển khai `UnitOfWork` trong `Persistence/Repositories/` (hoặc thư mục riêng cho UnitOfWork), inject `ApplicationDbContext`.
    *   Inject `IUnitOfWork` vào Command Handlers. Gọi `_unitOfWork.SaveChangesAsync()` một lần sau khi tất cả các thao tác trên repositories hoàn tất. Điều này đảm bảo tính toàn vẹn giao dịch.

## VII. Quy Ước Đặt Tên và Tổ Chức Code

1.  **Entities:** Đặt trong `Domain/Entities/`. Tên entity là số ít (ví dụ: `Manga`, `Chapter`).
2.  **Enums:** Đặt trong `Domain/Enums/`.
3.  **DTOs:** Đặt trong `Application/DTOs/` (có thể có thư mục con theo feature/entity). Tên DTO nên rõ ràng mục đích (ví dụ: `MangaDto`, `CreateMangaDto`, `MangaSummaryDto`).
4.  **Commands/Queries/Handlers:** Đặt trong `Application/Features/` (thư mục con theo feature/entity, ví dụ: `Application/Features/Manga/Commands/CreateMangaCommand.cs`).
5.  **Validators:** Đặt trong `Application/Validation/` (thư mục con theo feature/entity).
6.  **Mapping Profiles:** Đặt trong `Application/Mappings/`.
7.  **Common Interfaces (Application):** Các interface chung như `IUnitOfWork`, `IPhotoAccessor` nên đặt trong `Application/Common/Interfaces/`.
8.  **Repository Interfaces:** Đặt trong `Application/Contracts/Persistence/` (hoặc chung trong `Application/Common/Interfaces/` nếu không quá nhiều).
9.  **Repository Implementations:** Đặt trong `Persistence/Repositories/`.
10. **Service Implementations (Infrastructure):** Đặt trong `Infrastructure/Services/` hoặc `Infrastructure/Photos/` (cho `PhotoAccessor`).
11. **Controllers:** Đặt trong `MangaReaderDB/Controllers/`. Tên controller là số nhiều (ví dụ: `MangasController`).

## VIII. Xử lý lỗi và Logging

1.  **Exception Handling:**
    *   Sử dụng try-catch trong Command/Query Handlers để bắt các lỗi cụ thể (ví dụ: `DbUpdateException`, `KeyNotFoundException`).
    *   Tạo các custom exception (trong Domain hoặc Application layer) nếu cần để biểu thị các lỗi nghiệp vụ cụ thể.
    *   Controllers nên bắt các exception chung và trả về HTTP status code phù hợp (ví dụ: 400, 404, 500). Cân nhắc sử dụng middleware xử lý exception toàn cục (khuyến khích).
2.  **Logging:**
    *   Inject `ILogger<T>` vào các services, handlers, repositories, controllers để ghi log các thông tin quan trọng, lỗi, cảnh báo.
    *   Sử dụng logging có cấu trúc (structured logging) với Serilog hoặc NLog nếu cần log nâng cao và tập trung log.

## IX. Xử lý Ảnh với Cloudinary (DataFlow đã thống nhất)

1.  **Cấu hình:**
    *   `CloudinarySettings` class trong `Infrastructure/Photos/` chứa `CloudName`, `ApiKey`, `ApiSecret`.
    *   Thông tin cấu hình (ngoại trừ `ApiSecret`) được lưu trong `appsettings.json`.
    *   `ApiSecret` **PHẢI** được quản lý an toàn thông qua User Secrets (Development) hoặc Environment Variables/Azure Key Vault (Staging/Production).
    *   Đăng ký `CloudinarySettings` và `IPhotoAccessor` trong `MangaReaderDB/Program.cs`.

2.  **Interface và Implementation:**
    *   `IPhotoAccessor` (trong `Application/Common/Interfaces/`) định nghĩa các phương thức `UploadPhotoAsync(Stream stream, string fileName, string? folderName = null)` và `DeletePhotoAsync(string publicId)`.
    *   `PhotoAccessor` (trong `Infrastructure/Photos/`) triển khai `IPhotoAccessor` sử dụng Cloudinary SDK.
        *   Phương thức `UploadPhotoAsync` nhận `Stream` và `fileName` (Controller sẽ lấy từ `IFormFile`), có thể nhận thêm `folderName` để tổ chức ảnh trên Cloudinary (ví dụ: "cover_arts", "chapter_pages/[chapterId]").
        *   Phương thức trả về `PhotoUploadResult` (trong `Application/Common/Models/`) chứa `PublicId` và `Url` của ảnh đã upload.

3.  **Luồng Upload Ảnh:**
    *   **Frontend:** Người dùng chọn ảnh.
    *   **Controller (API):**
        *   Nhận `IFormFile` từ request.
        *   Validate file (kích thước, loại file - nếu cần).
        *   Mở `Stream` từ `IFormFile` và lấy `FileName`.
        *   Gọi Command Handler tương ứng (ví dụ: `UploadCoverArtCommandHandler`, `AddPageToChapterCommandHandler`) với `Stream`, `FileName`, và các thông tin khác.
    *   **Command Handler (Application Layer):**
        *   Inject `IPhotoAccessor` và `IUnitOfWork`.
        *   Gọi `_photoAccessor.UploadPhotoAsync(stream, fileName, "relevant_folder_name")`.
        *   Nếu upload thành công, nhận `PhotoUploadResult` chứa `PublicId`.
        *   Tạo/Cập nhật Entity (ví dụ: `CoverArt`, `ChapterPage`) và **CHỈ LƯU `PublicId`** vào trường tương ứng của Entity.
        *   Sử dụng `_unitOfWork` để thêm Entity vào `DbContext`.
        *   Gọi `_unitOfWork.SaveChangesAsync()` để lưu thay đổi vào database.
        *   Trả về thông tin cần thiết cho Controller (ví dụ: `PublicId` của ảnh mới, hoặc DTO của entity đã cập nhật).

4.  **Luồng Xóa Ảnh (Nếu cần):**
    *   Khi một Entity chứa `PublicId` bị xóa (ví dụ: xóa `ChapterPage`), hoặc khi ảnh được thay thế:
    *   **Command Handler (Application Layer):**
        *   Trước khi xóa Entity khỏi database, hoặc sau khi upload ảnh mới thành công (trong trường hợp thay thế):
            *   Lấy `PublicId` của ảnh cũ.
            *   Gọi `_photoAccessor.DeletePhotoAsync(oldPublicId)`.
            *   Xử lý kết quả trả về từ `DeletePhotoAsync` (log lỗi nếu xóa không thành công, nhưng thường không block nghiệp vụ chính trừ khi có yêu cầu cụ thể).
        *   Tiếp tục xóa/cập nhật Entity trong database và lưu thay đổi.

5.  **Luồng Hiển Thị Ảnh (Frontend):**
    *   **API (Query Handler):**
        *   Khi Frontend yêu cầu dữ liệu (ví dụ: chi tiết Chapter, thông tin Manga), Query Handler sẽ lấy Entity từ database.
        *   Map Entity sang DTO. DTO này sẽ chứa trường `PublicId` (ví dụ: `ChapterPageDto.PublicId`, `CoverArtDto.PublicId`).
        *   API **CHỈ** trả về `PublicId` cho Frontend.
    *   **Frontend:**
        *   Nhận `PublicId` từ API.
        *   Sử dụng thư viện Cloudinary của Frontend (hoặc tự xây dựng URL) để tạo URL hoàn chỉnh của ảnh từ `CloudName` (đã biết ở Frontend) và `PublicId` nhận được.
        *   Có thể áp dụng các transformation (resize, crop, format, quality) ở phía Frontend khi xây dựng URL để tối ưu hiển thị.

6.  **Lưu ý quan trọng về `PublicId`:**
    *   `PublicId` là duy nhất trên Cloudinary account của bạn.
    *   Khi upload, bạn có thể tùy chỉnh `PublicId` hoặc để Cloudinary tự sinh. Để dễ quản lý, có thể cân nhắc tạo `PublicId` có cấu trúc, ví dụ: `manga_slug/coverart/volume_1` hoặc `chapter_xyz/page_1`. Tuy nhiên, việc này cần được xử lý cẩn thận để tránh trùng lặp. Mặc định để Cloudinary tự sinh ID ngẫu nhiên là an toàn nhất.
    *   Thư mục (`Folder` parameter khi upload) giúp tổ chức ảnh trên Cloudinary media library, không ảnh hưởng đến `PublicId` (trừ khi bạn cấu hình Cloudinary để `PublicId` bao gồm cả đường dẫn thư mục).