# TODO.md: Các Bước Triển Khai API Quản Lý Dữ Liệu Theo `RequestSQL.md`

Dưới đây là danh sách các công việc chính cần thực hiện để xây dựng và hoàn thiện API quản lý dữ liệu (`MangaReaderDB`), đảm bảo tuân thủ các nguyên tắc và yêu cầu đã đặt ra trong `RequestSQL.md`.

## Bước 1: Định nghĩa Contracts (Interfaces) trong Lớp `Application`

1.  **Tạo Repository Interfaces:**
    *   Trong thư mục `Application/Contracts/Persistence/` (hoặc `Application/Common/Interfaces/` nếu bạn muốn gom chung), tạo các interface cho từng repository cần thiết (ví dụ: `IMangaRepository.cs`, `IAuthorRepository.cs`, `IChapterRepository.cs`, `ITagRepository.cs`, `ITagGroupRepository.cs`, `ICoverArtRepository.cs`, `ITranslatedMangaRepository.cs`).
    *   Các interface này sẽ định nghĩa các phương thức trừu tượng cho các thao tác dữ liệu cụ thể của từng entity.
2.  **Tạo Unit of Work Interface:**
    *   Trong thư mục `Application/Contracts/Persistence/` (hoặc `Application/Common/Interfaces/`), tạo interface `IUnitOfWork.cs`.
    *   Interface này cần có phương thức `Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);` và các thuộc tính để truy cập các Repository Interfaces đã định nghĩa ở trên (ví dụ: `IMangaRepository Mangas { get; }`).
3.  **Xác nhận `IApplicationDbContext`:** Đảm bảo interface `IApplicationDbContext.cs` đã tồn tại trong `Application/Common/Interfaces/` và định nghĩa các `DbSet<T>` cũng như phương thức `SaveChangesAsync`.

## Bước 2: Triển khai Lớp `Persistence` (Data Access Logic)

1.  **Implement Repository Interfaces:**
    *   Trong thư mục `Persistence/Repositories/`, tạo các class triển khai cho từng Repository Interface đã định nghĩa ở Bước 1.1 (ví dụ: `MangaRepository.cs`, `AuthorRepository.cs`).
    *   Các class này sẽ inject `ApplicationDbContext` (hoặc `IApplicationDbContext`) và triển khai các phương thức truy cập dữ liệu bằng EF Core (LINQ to Entities).
    *   **Lưu ý:** Các phương thức trong repository không gọi `_dbContext.SaveChangesAsync()` trực tiếp.
2.  **Implement Unit of Work:**
    *   Trong thư mục `Persistence/Repositories/`, tạo class `UnitOfWork.cs` triển khai interface `IUnitOfWork`.
    *   Class này sẽ inject `ApplicationDbContext`, khởi tạo các instance của các repository implementations, và triển khai phương thức `SaveChangesAsync()` bằng cách gọi `_dbContext.SaveChangesAsync()`.
3.  **Cập nhật `ApplicationDbContext`:** Đảm bảo lớp `ApplicationDbContext.cs` trong `Persistence/Data/` triển khai interface `IApplicationDbContext`.

## Bước 3: Hoàn Thiện Lớp `Application` (Core Business Logic)

1.  **Hoàn thiện Data Transfer Objects (DTOs):**
    *   Rà soát và hoàn thiện tất cả các DTOs cần thiết trong `Application/Common/DTOs/` theo `Project Structure.md` cho tất cả các entities và kịch bản (create, update, display, paged results).
2.  **Cấu hình AutoMapper:**
    *   Trong `Application/Common/Mappings/MappingProfile.cs`, định nghĩa đầy đủ các mapping profiles giữa Entities (Domain) và DTOs (Application).
    *   Đặc biệt chú trọng mapping cho các Query Handlers.
3.  **Triển khai FluentValidation Validators:**
    *   Cho mỗi DTO/Command/Query cần validation, tạo hoặc hoàn thiện các class Validator (kế thừa `AbstractValidator<T>`) trong các thư mục tương ứng trong `Application/Features/` (ví dụ: `Application/Features/Mangas/Commands/CreateManga/CreateMangaCommandValidator.cs`).
4.  **Triển khai Commands và Queries (CQRS với MediatR):**
    *   **Command Handlers:**
        *   Triển khai logic cho tất cả các `CommandHandler` trong `Application/Features/`.
        *   Mỗi `CommandHandler` phải inject `IUnitOfWork` để tương tác với database thông qua repositories.
        *   Sử dụng `_unitOfWork.Repositories.[YourRepository]` để truy cập phương thức của repository.
        *   Gọi `await _unitOfWork.SaveChangesAsync(cancellationToken);` một lần duy nhất sau khi tất cả các thao tác nghiệp vụ (bao gồm cả việc gọi `IPhotoAccessor` nếu có) hoàn tất để đảm bảo tính toàn vẹn giao dịch.
        *   Inject `IPhotoAccessor` nếu Command đó liên quan đến việc upload hoặc xóa file.
        *   Map DTOs đầu vào sang Entities (có thể dùng AutoMapper hoặc map thủ công).
    *   **Query Handlers:**
        *   Triển khai logic cho tất cả các `QueryHandler` trong `Application/Features/`.
        *   Mỗi `QueryHandler` phải inject `IUnitOfWork` (hoặc `IRepository` cụ thể nếu không có nhiều thao tác) để lấy dữ liệu.
        *   Sử dụng `IMapper` (AutoMapper) để map từ Entities sang DTOs trả về.

## Bước 4: Triển khai Lớp `MangaReaderDB` (API Presentation Layer)

1.  **Đăng ký Services trong `Program.cs`:**
    *   Đăng ký `MediatR` và trỏ đến assembly của `Application Layer` nơi chứa các handlers.
    *   Đăng ký `AutoMapper` và trỏ đến assembly của `Application Layer` nơi chứa các mapping profiles.
    *   Đăng ký tất cả các `Validators` từ `Application Layer` với Dependency Injection (ví dụ: `builder.Services.AddValidatorsFromAssembly(...)`).
    *   Đăng ký `IUnitOfWork` và các `IRepository` interfaces với các implementations tương ứng từ `Persistence Layer` (ví dụ: `builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();`, `builder.Services.AddScoped<IMangaRepository, MangaRepository>();`).
2.  **Triển khai `BaseApiController.cs`:**
    *   Đảm bảo `BaseApiController` có một property `protected IMediator Mediator` được khởi tạo thông qua `HttpContext.RequestServices.GetService<IMediator>()` hoặc constructor injection.
3.  **Triển khai API Controllers:**
    *   Hoàn thiện tất cả các API Controllers trong `MangaReaderDB/Controllers/` (ví dụ: `MangasController.cs`, `AuthorsController.cs`).
    *   Mỗi controller action:
        *   Nhận HTTP request và DTO đầu vào.
        *   Inject `IValidator<T>` (ví dụ: `IValidator<CreateMangaDto>`) để thực hiện validation thủ công cho DTO đầu vào. Trả về `BadRequest` nếu validation thất bại.
        *   Đối với các endpoint upload file: xử lý `IFormFile` để lấy `Stream` và `fileName` truyền vào Command.
        *   Tạo instance của Command hoặc Query tương ứng (map từ DTO đầu vào nếu cần).
        *   Gửi Command/Query qua `Mediator.Send()`.
        *   Xử lý kết quả trả về từ MediatR và trả về HTTP response phù hợp (ví dụ: `Ok`, `CreatedAtAction`, `NotFound`, `NoContent`).
        *   **Lưu ý:** Controllers không chứa business logic hoặc logic truy cập dữ liệu trực tiếp.

## Bước 5: Kiểm Thử Toàn Diện và Hoàn Thiện

1.  **Kiểm thử API Endpoints:** Sử dụng Swagger UI hoặc Postman để kiểm thử tất cả các API endpoints với các kịch bản khác nhau (thành công, thất bại, dữ liệu không hợp lệ).
2.  **Xử lý Lỗi Nâng Cao:** Xem xét việc implement một middleware xử lý lỗi toàn cục để chuẩn hóa các response lỗi và ghi log.
3.  **Review và Refactor:** Rà soát lại toàn bộ codebase, đảm bảo tuân thủ các nguyên tắc đã đề ra trong `RequestSQL.md`, Clean Architecture, SOLID và các best practices khác.

Chúc bạn hoàn thành dự án thành công!