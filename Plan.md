// Plan.md
# Kế Hoạch Triển Khai API Quản Lý Dữ Liệu (Cập Nhật)

Dưới đây là danh sách các công việc chính cần thực hiện để xây dựng và hoàn thiện API quản lý dữ liệu (`MangaReaderDB`), đảm bảo tuân thủ các nguyên tắc và yêu cầu đã đặt ra trong `RequestSQL.md`.

**Chú thích trạng thái:**
*   ✅: Hoàn thành
*   🔄: Đã tạo cấu trúc file, cần hoàn thiện nội dung / Đang thực hiện
*   ❌: Chưa bắt đầu / Chưa hoàn thành
*   ⏳: Chờ (phụ thuộc vào task khác)

## Bước 1: Định nghĩa Contracts (Interfaces) trong Lớp `Application`

1.  **Tạo Repository Interfaces:** (`Application/Contracts/Persistence/`)
    *   ✅ `IMangaRepository.cs` (File đã tạo, đã định nghĩa các phương thức cần thiết)
    *   ✅ `IAuthorRepository.cs` (File đã tạo, đã định nghĩa các phương thức cần thiết)
    *   ✅ `IChapterRepository.cs` (File đã tạo, đã định nghĩa các phương thức cần thiết, bao gồm cho Chapter và ChapterPage)
    *   ✅ `ITagRepository.cs` (File đã tạo, đã định nghĩa các phương thức cần thiết)
    *   ✅ `ITagGroupRepository.cs` (File đã tạo, đã định nghĩa các phương thức cần thiết)
    *   ✅ `ICoverArtRepository.cs` (File đã tạo, đã định nghĩa các phương thức cần thiết)
    *   ✅ `ITranslatedMangaRepository.cs` (File đã tạo, đã định nghĩa các phương thức cần thiết)
    *   ✅ `IGenericRepository.cs` (File đã tạo, đã định nghĩa các phương thức cần thiết)
2.  **Tạo Unit of Work Interface:** (`Application/Contracts/Persistence/`)
    *   ✅ `IUnitOfWork.cs` (File đã tạo, đã định nghĩa phương thức `SaveChangesAsync` và các thuộc tính Repository)
3.  **Hoàn thiện `IApplicationDbContext`:** (`Application/Common/Interfaces/`)
    *   ✅ `IApplicationDbContext.cs` (File đã tạo, đã định nghĩa các `DbSet<T>` và phương thức `SaveChangesAsync`, `Set<TEntity>`)
4.  **Hoàn thiện `IPhotoAccessor`:** (`Application/Common/Interfaces/`)
    *   ✅ `IPhotoAccessor.cs` (Đã có và có nội dung hoàn chỉnh)

## Bước 2: Triển khai Lớp `Persistence` (Data Access Logic)

1.  **Implement Repository Interfaces:** (`Persistence/Repositories/`)
    *   ✅ `MangaRepository.cs` (Hoàn thành nội dung triển khai interface)
    *   ✅ `AuthorRepository.cs` (Hoàn thành nội dung triển khai interface)
    *   ✅ `ChapterRepository.cs` (Hoàn thành nội dung triển khai interface)
    *   ✅ `TagRepository.cs` (Hoàn thành nội dung triển khai interface)
    *   ✅ `TagGroupRepository.cs` (Hoàn thành nội dung triển khai interface)
    *   ✅ `CoverArtRepository.cs` (Hoàn thành nội dung triển khai interface)
    *   ✅ `TranslatedMangaRepository.cs` (Hoàn thành nội dung triển khai interface)
    *   ✅ `GenericRepository.cs` (Hoàn thành nội dung triển khai interface)
2.  **Implement Unit of Work:** (`Persistence/Repositories/`)
    *   ✅ `UnitOfWork.cs` (Hoàn thành nội dung triển khai interface)
3.  **Cập nhật `ApplicationDbContext`:** (`Persistence/Data/`)
    *   ✅ `ApplicationDbContext.cs` (Đã có, đã định nghĩa DbSets và cấu hình OnModelCreating)
4.  **Interceptors:**
    *   ✅ `AuditableEntitySaveChangesInterceptor.cs` (Đã có và có nội dung hoàn chỉnh)
5.  **Migrations:**
    *   ✅ `InitialCreate` migration (Đã tạo và áp dụng)

## Bước 3: Hoàn Thiện Lớp `Application` (Core Business Logic)

1.  **Hoàn thiện Data Transfer Objects (DTOs):** (`Application/Common/DTOs/`)
    *   ✅ `PagedResult.cs`
    *   ✅ `Authors/AuthorDto.cs`, `CreateAuthorDto.cs`, `UpdateAuthorDto.cs`
    *   ✅ `Mangas/MangaDto.cs`, `CreateMangaDto.cs`, `UpdateMangaDto.cs`, `MangaAuthorInputDto.cs`, `MangaTagInputDto.cs`
    *   ✅ `Chapters/ChapterDto.cs`, `CreateChapterDto.cs`, `UpdateChapterDto.cs`, `ChapterPageDto.cs`, `CreateChapterPageDto.cs`, `UpdateChapterPageDto.cs`
    *   ✅ `Tags/TagDto.cs`, `CreateTagDto.cs`, `UpdateTagDto.cs`
    *   ✅ `TagGroups/TagGroupDto.cs`, `CreateTagGroupDto.cs`, `UpdateTagGroupDto.cs`
    *   ✅ `CoverArts/CoverArtDto.cs`, `CreateCoverArtDto.cs`
    *   ✅ `TranslatedMangas/TranslatedMangaDto.cs`, `CreateTranslatedMangaDto.cs`, `UpdateTranslatedMangaDto.cs`
    *   ✅ `Users/UserDto.cs`
2.  **Cấu hình AutoMapper:** (`Application/Common/Mappings/`)
    *   ✅ `MappingProfile.cs`
3.  **Triển khai FluentValidation Validators:** (`Application/Features/.../...Validator.cs`)
    *   ✅ Cấu trúc thư mục và files đã được tạo cho Validators (ví dụ: `CreateMangaCommandValidator.cs`), cần định nghĩa nội dung.
4.  **Triển khai Commands và Queries (CQRS với MediatR):** (`Application/Features/`)
    *   **Commands & Command Handlers:**
        *   🔄 Cấu trúc thư mục và files đã được tạo cho Commands và Handlers (ví dụ: `CreateMangaCommand.cs`, `CreateMangaCommandHandler.cs`), cần định nghĩa nội dung.
    *   **Queries & Query Handlers:**
        *   🔄 Cấu trúc thư mục và files đã được tạo cho Queries và Handlers (ví dụ: `GetMangaByIdQuery.cs`, `GetMangaByIdQueryHandler.cs`), cần định nghĩa nội dung.
5.  **Models:**
    *   ✅ `PhotoUploadResult.cs` (Đã có và có nội dung hoàn chỉnh)

## Bước 4: Triển khai Lớp `MangaReaderDB` (API Presentation Layer)

1.  **Đăng ký Services trong `Program.cs`:**
    *   🔄 Đã đăng ký `ApplicationDbContext`, `AuditableEntitySaveChangesInterceptor`, `CloudinarySettings`, `IPhotoAccessor`, và các services cơ bản của ASP.NET Core.
    *   ❌ Cần đăng ký `MediatR`, `AutoMapper`, `FluentValidation Validators` (từ `Application` layer), `IUnitOfWork` và các `IRepository` interfaces với implementations tương ứng.
2.  **Triển khai `BaseApiController.cs`:** (`MangaReaderDB/Controllers/`)
    *   ❌ Chưa có.
3.  **Triển khai API Controllers:** (`MangaReaderDB/Controllers/`)
    *   ❌ Thư mục `Controllers` đã được tạo nhưng chưa có file controller nào.
    *   Các controllers cần được tạo: `MangasController.cs`, `AuthorsController.cs`, `ChaptersController.cs`, `CoverArtsController.cs`, `TagsController.cs`, `TagGroupsController.cs`, `TranslatedMangasController.cs`.

## Bước 5: Kiểm Thử Toàn Diện và Hoàn Thiện

1.  **Kiểm thử API Endpoints:**
    *   ❌ Chưa bắt đầu.
2.  **Xử lý Lỗi Nâng Cao:**
    *   ❌ Chưa bắt đầu.
3.  **Review và Refactor:**
    *   ❌ Chưa bắt đầu.

Chúc bạn hoàn thành dự án thành công!