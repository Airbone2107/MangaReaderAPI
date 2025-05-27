# // Plan.md
# Kế Hoạch Cập Nhật API và DTOs

Dự án này nhằm mục đích cấu trúc lại các API và Data Transfer Objects (DTOs) để phù hợp hơn với tiêu chuẩn của Mangadex API, đồng thời loại bỏ tiền tố "/api" khỏi các đường dẫn và hậu tố "DTO" khỏi tên các class DTO.

## I. Thay Đổi Chung

1.  **Loại bỏ `/api` khỏi Base Route:**
    *   **Tệp cần cập nhật:**
        *   `MangaReaderDB/Controllers/BaseApiController.cs` (Nếu base route được định nghĩa ở đây).
        *   Tất cả các tệp Controller trong `MangaReaderDB/Controllers/` nếu route được định nghĩa riêng lẻ.
    *   **Công việc:** Chỉnh sửa thuộc tính `[Route]` từ `[Route("api/[controller]")]` thành `[Route("[controller]")]` hoặc tương tự.

2.  **Cập nhật tài liệu:**
    *   **Tệp cần cập nhật:**
        *   `docs/api_conventions.md`
        *   `docs/data_flow.md`
        *   Các file `.md` khác trong thư mục `docs` nếu có đề cập đến cấu trúc DTO cũ hoặc đường dẫn API.
    *   **Công việc:**
        *   Phản ánh việc loại bỏ `/api` khỏi các ví dụ đường dẫn.
        *   Sử dụng tên DTO mới (không có hậu tố "DTO").
        *   Mô tả cấu trúc DTO mới nếu có thay đổi đáng kể để giống Mangadex.

## II. Cấu Trúc và Đặt Tên Lại DTOs

**Nguyên tắc:**
*   Loại bỏ hậu tố "DTO" khỏi tên class DTO. Ví dụ: `MangaDto` -> `Manga`.
*   Mô phỏng cấu trúc DTO của Mangadex (thường bao gồm `id`, `type`, `attributes`, `relationships`).
*   Tạo các DTO `...Attributes` để chứa các thuộc tính chính của đối tượng.
*   Tạo các DTO `...Response` (cho một đối tượng) và `...List` (cho danh sách đối tượng, bao gồm phân trang) theo mẫu Mangadex.
*   Các DTO cho việc tạo (Create) và sửa (Edit) sẽ có dạng `EntityNameCreate` và `EntityNameEdit`.

### 1. DTOs Liên Quan Đến `Author`

*   **Đổi tên và Cập nhật cấu trúc (Tạo mới nếu cần):**
    *   `Application/Common/DTOs/Authors/AuthorDto.cs` -> `Application/Common/Models/Author.cs`
    *   Tạo mới: `Application/Common/Models/AuthorAttributes.cs` (Chuyển các thuộc tính từ `AuthorDto` cũ vào đây, trừ `AuthorId`)
    *   `Application/Common/DTOs/Authors/CreateAuthorDto.cs` -> `Application/Common/Models/AuthorCreate.cs`
    *   `Application/Common/DTOs/Authors/UpdateAuthorDto.cs` -> `Application/Common/Models/AuthorEdit.cs`
    *   Tạo mới: `Application/Common/Responses/AuthorResponse.cs` (Bao gồm `result`, `response`, `data: Author`)
    *   Tạo mới: `Application/Common/Responses/AuthorListResponse.cs` (Bao gồm `result`, `response`, `data: List<Author>`, `limit`, `offset`, `total`)

### 2. DTOs Liên Quan Đến `Manga`

*   **Đổi tên và Cập nhật cấu trúc (Tạo mới nếu cần):**
    *   `Application/Common/DTOs/Mangas/MangaDto.cs` -> `Application/Common/Models/Manga.cs`
    *   Tạo mới: `Application/Common/Models/MangaAttributes.cs` (Chuyển các thuộc tính từ `MangaDto` cũ, ví dụ: `Title`, `OriginalLanguage`, `Status`, `Year`, `ContentRating`, `IsLocked`, `CreatedAt`, `UpdatedAt`. Các collection như `Tags`, `Authors`, `CoverArts`, `TranslatedMangas` sẽ nằm trong `relationships` của `Manga.cs`)
    *   `Application/Common/DTOs/Mangas/CreateMangaDto.cs` -> `Application/Common/Models/MangaCreate.cs`
    *   `Application/Common/DTOs/Mangas/UpdateMangaDto.cs` -> `Application/Common/Models/MangaEdit.cs`
    *   `Application/Common/DTOs/Mangas/MangaAuthorInputDto.cs` -> `Application/Common/Models/MangaAuthorRelationship.cs` (hoặc tương tự, dùng cho request tạo/cập nhật quan hệ)
    *   `Application/Common/DTOs/Mangas/MangaTagInputDto.cs` -> `Application/Common/Models/MangaTagRelationship.cs` (hoặc tương tự)
    *   Tạo mới: `Application/Common/Responses/MangaResponse.cs`
    *   Tạo mới: `Application/Common/Responses/MangaListResponse.cs`

### 3. DTOs Liên Quan Đến `Chapter` và `ChapterPage`

*   **Đổi tên và Cập nhật cấu trúc (Tạo mới nếu cần):**
    *   `Application/Common/DTOs/Chapters/ChapterDto.cs` -> `Application/Common/Models/Chapter.cs`
    *   Tạo mới: `Application/Common/Models/ChapterAttributes.cs`
    *   `Application/Common/DTOs/Chapters/CreateChapterDto.cs` -> `Application/Common/Models/ChapterCreate.cs`
    *   `Application/Common/DTOs/Chapters/UpdateChapterDto.cs` -> `Application/Common/Models/ChapterEdit.cs`
    *   `Application/Common/DTOs/Chapters/ChapterPageDto.cs` -> `Application/Common/Models/ChapterPage.cs` (Mangadex không có DTO riêng cho Page, nó thường được trả về như một phần của thông tin Chapter hoặc `AtHomeServer`)
    *   `Application/Common/DTOs/Chapters/CreateChapterPageDto.cs` -> `Application/Common/Models/ChapterPageCreate.cs`
    *   `Application/Common/DTOs/Chapters/UpdateChapterPageDto.cs` -> `Application/Common/Models/ChapterPageEdit.cs`
    *   Tạo mới: `Application/Common/Responses/ChapterResponse.cs`
    *   Tạo mới: `Application/Common/Responses/ChapterListResponse.cs`

### 4. DTOs Liên Quan Đến `Tag` và `TagGroup`

*   **Đổi tên và Cập nhật cấu trúc (Tạo mới nếu cần):**
    *   `Application/Common/DTOs/Tags/TagDto.cs` -> `Application/Common/Models/Tag.cs`
    *   Tạo mới: `Application/Common/Models/TagAttributes.cs`
    *   `Application/Common/DTOs/Tags/CreateTagDto.cs` -> `Application/Common/Models/TagCreate.cs`
    *   `Application/Common/DTOs/Tags/UpdateTagDto.cs` -> `Application/Common/Models/TagEdit.cs`
    *   Tạo mới: `Application/Common/Responses/TagResponse.cs` (Mangadex trả về một collection với `limit`, `offset`, `total` ngay cả khi chỉ có một tag) -> Có thể cần `TagSingleResponse` và `TagListResponse`. Tuy nhiên, `TagResponse` của họ dùng cho cả danh sách.
    *   `Application/Common/DTOs/TagGroups/TagGroupDto.cs` -> `Application/Common/Models/TagGroup.cs`
    *   Tạo mới: `Application/Common/Models/TagGroupAttributes.cs`
    *   `Application/Common/DTOs/TagGroups/CreateTagGroupDto.cs` -> `Application/Common/Models/TagGroupCreate.cs`
    *   `Application/Common/DTOs/TagGroups/UpdateTagGroupDto.cs` -> `Application/Common/Models/TagGroupEdit.cs`
    *   Tạo mới: `Application/Common/Responses/TagGroupResponse.cs`
    *   Tạo mới: `Application/Common/Responses/TagGroupListResponse.cs`

### 5. DTOs Liên Quan Đến `CoverArt`

*   **Đổi tên và Cập nhật cấu trúc (Tạo mới nếu cần):**
    *   `Application/Common/DTOs/CoverArts/CoverArtDto.cs` -> `Application/Common/Models/CoverArt.cs` (Tên này khớp với Mangadex)
    *   Tạo mới: `Application/Common/Models/CoverAttributes.cs` (Mangadex gọi là `CoverAttributes`)
    *   `Application/Common/DTOs/CoverArts/CreateCoverArtDto.cs` -> `Application/Common/Models/CoverCreate.cs` (Hoặc `CoverArtCreate`, Mangadex không có DTO riêng cho tạo cover qua API spec này, mà là request body của `Upload Cover`)
    *   Tạo mới: `Application/Common/Responses/CoverResponse.cs` (Mangadex gọi là `CoverResponse`)
    *   Tạo mới: `Application/Common/Responses/CoverListResponse.cs` (Mangadex gọi là `CoverList`)

### 6. DTOs Liên Quan Đến `TranslatedManga`

*   **Đổi tên và Cập nhật cấu trúc (Tạo mới nếu cần):**
    *   `Application/Common/DTOs/TranslatedMangas/TranslatedMangaDto.cs` -> `Application/Common/Models/TranslatedManga.cs`
    *   Tạo mới: `Application/Common/Models/TranslatedMangaAttributes.cs`
    *   `Application/Common/DTOs/TranslatedMangas/CreateTranslatedMangaDto.cs` -> `Application/Common/Models/TranslatedMangaCreate.cs`
    *   `Application/Common/DTOs/TranslatedMangas/UpdateTranslatedMangaDto.cs` -> `Application/Common/Models/TranslatedMangaEdit.cs`
    *   Tạo mới: `Application/Common/Responses/TranslatedMangaResponse.cs`
    *   Tạo mới: `Application/Common/Responses/TranslatedMangaListResponse.cs`

### 7. DTOs Liên Quan Đến `User` (Thông tin cơ bản)

*   **Đổi tên và Cập nhật cấu trúc (Tạo mới nếu cần):**
    *   `Application/Common/DTOs/Users/UserDto.cs` -> `Application/Common/Models/User.cs`
    *   Tạo mới: `Application/Common/Models/UserAttributes.cs`
    *   Tạo mới: `Application/Common/Responses/UserResponse.cs`
    *   Tạo mới: `Application/Common/Responses/UserListResponse.cs`

### 8. DTOs Chung

*   **Cập nhật (Nếu cần):**
    *   `Application/Common/DTOs/PagedResult.cs`: Đổi tên thành `CollectionResponse` hoặc tương tự để nhất quán với Mangadex `...List` (ví dụ: `MangaList`). Hoặc giữ nguyên và các `...ListResponse` sẽ sử dụng nó.
    *   Tạo mới: `Application/Common/Models/Relationship.cs` (Theo cấu trúc của Mangadex)
    *   Tạo mới: `Application/Common/Models/LocalizedString.cs` (Theo cấu trúc của Mangadex)
    *   Tạo mới: `Application/Common/Responses/ErrorResponse.cs` (Theo cấu trúc của Mangadex)
    *   Tạo mới: `Application/Common/Responses/GenericResponse.cs` (Cho các response chỉ có `result: "ok"`)

## III. Cập Nhật Các Tệp Tham Chiếu Đến DTOs

Các tệp sau sẽ cần được cập nhật để sử dụng tên và cấu trúc DTO mới:

1.  **`Application/Common/Mappings/MappingProfile.cs`**:
    *   Cập nhật tất cả các `CreateMap<Source, Destination>()` để phản ánh tên DTO mới và cấu trúc mới (ví dụ: map tới `EntityAttributes` thay vì trực tiếp `Entity`).

2.  **Validators (Trong `Application/Validation/...` và `Application/Features/.../...Validator.cs`):**
    *   Đổi tên file và class validator (ví dụ: `CreateAuthorDtoValidator.cs` -> `AuthorCreateValidator.cs`).
    *   Cập nhật kiểu generic `AbstractValidator<T>` sang DTO mới.
    *   Chỉnh sửa các rule nếu cấu trúc DTO thay đổi.

3.  **Command Handlers (Trong `Application/Features/.../...CommandHandler.cs`):**
    *   Cập nhật kiểu dữ liệu của Commands nếu chúng chứa DTO.
    *   Cập nhật kiểu trả về của Handlers nếu chúng trả về DTO (bây giờ sẽ là các `...Response` hoặc `...ListResponse`).
    *   Cập nhật logic mapping thủ công nếu có.

4.  **Query Handlers (Trong `Application/Features/.../...QueryHandler.cs`):**
    *   Cập nhật kiểu trả về của Handlers (bây giờ sẽ là các `...Response` hoặc `...ListResponse`).
    *   Cập nhật logic mapping sang DTO mới.

5.  **Controllers (Trong `MangaReaderDB/Controllers/`):**
    *   **Tệp cần cập nhật:**
        *   `MangaReaderDB/Controllers/AuthorsController.cs`
        *   `MangaReaderDB/Controllers/MangasController.cs`
        *   `MangaReaderDB/Controllers/ChaptersController.cs`
        *   `MangaReaderDB/Controllers/ChapterPagesController.cs` (Nếu đã tách)
        *   `MangaReaderDB/Controllers/CoverArtsController.cs`
        *   `MangaReaderDB/Controllers/TagsController.cs`
        *   `MangaReaderDB/Controllers/TagGroupsController.cs`
        *   `MangaReaderDB/Controllers/TranslatedMangasController.cs`
    *   **Công việc:**
        *   Cập nhật kiểu tham số `[FromBody]` của Actions.
        *   Cập nhật kiểu trả về của Actions (`ActionResult<T>`, T bây giờ là `...Response` hoặc `...ListResponse`).
        *   Cập nhật thuộc tính `[ProducesResponseType]` để phản ánh DTO mới.
        *   Đảm bảo các đường dẫn (routes) không còn tiền tố `/api`.

## IV. Thứ Tự Thực Hiện Đề Xuất

1.  Tạo các DTO cơ bản mới (`AuthorAttributes`, `MangaAttributes`, `Relationship`, `LocalizedString`, `ErrorResponse`, `GenericResponse`).
2.  Đổi tên các DTO entity cơ bản (`AuthorDto` -> `Author`) và tích hợp `...Attributes` và `Relationship` vào chúng.
3.  Tạo các DTO `...Create` và `...Edit`.
4.  Tạo các DTO `...Response` và `...ListResponse`.
5.  Cập nhật `Application/Common/Mappings/MappingProfile.cs`.
6.  Cập nhật các file Validators.
7.  Cập nhật các Command/Query Handlers.
8.  Cập nhật các Controllers (bao gồm cả việc loại bỏ `/api` khỏi routes).
9.  Chạy thử và kiểm tra kỹ lưỡng.
10. Cập nhật tài liệu.

---

**Lưu ý:** Đây là một kế hoạch tổng quan. Trong quá trình thực hiện, bạn có thể cần điều chỉnh hoặc phát hiện thêm các tệp cần thay đổi. Việc đổi tên và cấu trúc DTO là một thay đổi lớn, cần thực hiện cẩn thận và kiểm thử kỹ lưỡng.