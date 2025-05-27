**I. Cải thiện về Sắp xếp và Lọc dữ liệu (Sorting & Filtering):**

*   **`Application\Features\Authors\Queries\GetAuthors\GetAuthorsQuery.cs`**:
    *   Đề xuất thêm các tham số sắp xếp động như `OrderBy` (ví dụ: theo `Name`) và `Ascending`.
*   **`Application\Features\Authors\Queries\GetAuthors\GetAuthorsQueryHandler.cs`**:
    *   Cân nhắc cải thiện tìm kiếm `NameFilter` để hỗ trợ tìm kiếm không dấu hoặc chính xác hơn.
    *   Triển khai logic sắp xếp động dựa trên tham số `OrderBy` và `Ascending` từ query.
*   **`Application\Features\Chapters\Queries\GetChapterPages\GetChapterPagesQuery.cs`**:
    *   Xem xét thêm tùy chọn `OrderBy` nếu cần thiết (ngoài `PageNumber`).
*   **`Application\Features\Chapters\Queries\GetChaptersByTranslatedManga\GetChaptersByTranslatedMangaQuery.cs`**:
    *   Đề xuất thêm bộ lọc theo `Volume`, `ChapterNumber`.
*   **`Application\Features\Chapters\Queries\GetChaptersByTranslatedManga\GetChaptersByTranslatedMangaQueryHandler.cs`**:
    *   Triển khai bộ lọc theo `Volume`, `ChapterNumber` nếu được thêm vào query.
    *   Implement sắp xếp tùy chỉnh (custom sorting) cho `ChapterNumber` và `Volume` nếu chúng chứa các ký tự không phải số (ví dụ: "1.5", "2a").
*   **`Application\Features\CoverArts\Queries\GetCoverArtsByManga\GetCoverArtsByMangaQuery.cs`**:
    *   Đề xuất thêm bộ lọc theo `Volume`.
    *   Đề xuất thêm các tùy chọn sắp xếp (ví dụ: theo `Volume`, `CreatedAt`).
*   **`Application\Features\CoverArts\Queries\GetCoverArtsByManga\GetCoverArtsByMangaQueryHandler.cs`**:
    *   Triển khai bộ lọc theo `Volume` và các tùy chọn sắp xếp nếu được thêm vào query.
*   **`Application\Features\Mangas\Queries\GetMangas\GetMangasQuery.cs`**:
    *   Đề xuất thêm bộ lọc cho `TranslatedManga.LanguageKey` (ví dụ: lấy manga có bản dịch tiếng Việt).
*   **`Application\Features\Mangas\Queries\GetMangas\GetMangasQueryHandler.cs`**:
    *   Cân nhắc sử dụng `EF.Functions.Like` hoặc tìm kiếm full-text cho `TitleFilter` để tìm kiếm hiệu quả và linh hoạt hơn.
    *   Khi lọc theo `AuthorIdsFilter`, xem xét việc lọc theo vai trò cụ thể (`Author`/`Artist`) nếu `MangaAuthorInputDto` có thông tin `Role`.
    *   Triển khai bộ lọc cho `TranslatedManga.LanguageKey` nếu được thêm vào query.
*   **`Application\Features\TagGroups\Queries\GetTagGroups\GetTagGroupsQueryHandler.cs`**:
    *   Cân nhắc cải thiện tìm kiếm `NameFilter` để hỗ trợ tìm kiếm không dấu hoặc chính xác hơn.
*   **`Application\Features\Tags\Queries\GetTags\GetTagsQueryHandler.cs`**:
    *   Cân nhắc cải thiện tìm kiếm `NameFilter` để hỗ trợ tìm kiếm không dấu hoặc chính xác hơn.
*   **`Application\Features\TranslatedMangas\Queries\GetTranslatedMangasByManga\GetTranslatedMangasByMangaQuery.cs`**:
    *   Đề xuất thêm bộ lọc theo `LanguageKey`.
*   **`Application\Features\TranslatedMangas\Queries\GetTranslatedMangasByManga\GetTranslatedMangasByMangaQueryHandler.cs`**:
    *   Triển khai bộ lọc theo `LanguageKey` nếu được thêm vào query.

**II. Tối ưu hóa Truy vấn và Phân trang (Query Optimization & Paging):**

*   **`Application\Features\Chapters\Queries\GetChapterPages\GetChapterPagesQueryHandler.cs`**:
    *   Hiện tại, việc lấy danh sách `ChapterPage` đang load tất cả các trang của chapter rồi mới phân trang trong bộ nhớ. Điều này không tối ưu cho chapter có nhiều trang.
    *   Đề xuất: Thêm phương thức `GetPagedPagesByChapterAsync(Guid chapterId, int pageNumber, int pageSize)` vào `IChapterRepository` và triển khai nó trong `ChapterRepository` để thực hiện phân trang ở phía cơ sở dữ liệu.

**III. Cải thiện DTO và Mapping (DTOs & Mapping):**

*   **`Application\Features\TagGroups\Queries\GetTagGroupById\GetTagGroupByIdQuery.cs`**:
    *   Đề xuất thêm tùy chọn `bool IncludeTags` để client có thể quyết định có muốn load danh sách `Tags` con của `TagGroup` hay không.
*   **`Application\Features\TagGroups\Queries\GetTagGroupById\GetTagGroupByIdQueryHandler.cs`**:
    *   Nếu tùy chọn `IncludeTags` được thêm vào query và `TagGroupDto` được cập nhật để chứa `List<TagDto>`, cần đảm bảo `MappingProfile` xử lý việc map `TagGroup.Tags` sang `TagGroupDto.Tags`. Nên sử dụng phương thức `ITagGroupRepository.GetTagGroupWithTagsAsync()` khi `IncludeTags` là `true`.
*   **`Application\Features\TagGroups\Queries\GetTagGroups\GetTagGroupsQuery.cs`**:
    *   Đề xuất thêm tùy chọn `bool IncludeTags` để có thể lấy danh sách `Tags` con trong kết quả phân trang (cần cẩn thận về hiệu năng).
*   **`Application\Features\TagGroups\Queries\GetTagGroups\GetTagGroupsQueryHandler.cs`**:
    *   Nếu query có tùy chọn `IncludeTags`, cần thêm `includeProperties: "Tags"` khi gọi `GetPagedAsync`.
*   **`Application\Features\TranslatedMangas\Queries\GetTranslatedMangaById\GetTranslatedMangaByIdQuery.cs`**:
    *   Đề xuất thêm tùy chọn `IncludeManga` để có thể lấy thông tin `Manga` gốc (ít phổ biến).
    *   Đề xuất thêm tùy chọn `IncludeChapters` để có thể lấy danh sách `Chapters` của `TranslatedManga`.
*   **`Application\Features\TranslatedMangas\Queries\GetTranslatedMangaById\GetTranslatedMangaByIdQueryHandler.cs`**:
    *   Nếu query có tùy chọn `IncludeChapters` và `TranslatedMangaDto` được cập nhật để chứa `List<ChapterDto>`, cần đảm bảo `MappingProfile` xử lý việc map này và sử dụng phương thức repository phù hợp để load `Chapters`.

**IV. Xử lý File và Dịch vụ Bên ngoài (File Handling & External Services):**

*   **`Application\Features\Chapters\Commands\UploadChapterPageImage\UploadChapterPageImageCommandHandler.cs`**:
    *   Logic xử lý `fileExtension` khi tạo `desiredPublicId`: Hiện tại đang mặc định là `.jpg` nếu không có hoặc không hợp lệ. Cần xem xét lại để có giải pháp chặt chẽ hơn, ví dụ: throw `ValidationException` hoặc sử dụng content type để suy ra extension.
*   **`Infrastructure\Photos\PhotoAccessor.cs`**:
    *   `UploadPhotoAsync`: Cần lưu ý cách `folderName` tương tác với đường dẫn trong `desiredPublicId`.
    *   `DeletePhotoAsync`: Kết quả "not found" từ Cloudinary có thể được coi là xóa thành công tùy theo logic nghiệp vụ.

**V. Validation và Logic Nghiệp vụ (Validation & Business Logic):**

*   **`MangaReaderDB\Controllers\ChaptersController.cs`**:
    *   Trong action `CreateChapter`, `UploadedByUserId` hiện đang lấy từ `CreateChapterDto`. Trong thực tế, nên lấy thông tin này từ context của người dùng đã xác thực (ví dụ: `HttpContext.User` hoặc một service như `ICurrentUserService`).
*   **`Application\Features\Mangas\Commands\AddMangaTag\AddMangaTagCommandHandler.cs`**:
    *   Việc kiểm tra xem `MangaTag` đã tồn tại chưa bằng cách load toàn bộ collection `MangaTags` của `Manga` có thể không hiệu quả nếu collection lớn. Cân nhắc kiểm tra trực tiếp trong bảng join `MangaTags` thông qua `DbContext` (nếu có thể) hoặc thêm một phương thức chuyên biệt vào `IMangaRepository`.

**VI. Cấu trúc Controller (Controller Structure):**

*   **`MangaReaderDB\Controllers\ChaptersController.cs`**:
    *   Có chú thích gợi ý tách các endpoint liên quan đến `ChapterPage` ra một `ChapterPagesController` riêng biệt. Hiện tại bạn đã có `ChapterPagesController`, nên có thể xóa bỏ các endpoint `ChapterPage` cũ trong `ChaptersController` nếu chưa làm.