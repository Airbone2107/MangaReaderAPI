Đã hiểu rõ yêu cầu của bạn. Chúng ta sẽ cập nhật `Plan.md` để phản ánh việc `type` trong `relationships` sẽ linh hoạt hơn, thể hiện vai trò của thực thể liên quan đối với thực thể gốc, đặc biệt là trường hợp "author" và "artist" cho manga. Kế hoạch này sẽ dựa trên cấu trúc code hiện tại của bạn.

Dưới đây là nội dung cập nhật cho `Plan.md`:

```markdown
<!-- Plan.md -->
# Kế Hoạch Cập Nhật Cấu Trúc Dữ Liệu Trả Về API

Mục tiêu của kế hoạch này là điều chỉnh cấu trúc dữ liệu trả về của các API để tuân theo một định dạng chuẩn, dễ hiểu và nhất quán, tương tự như cấu trúc được sử dụng bởi Mangadex API. Cụ thể, mỗi đối tượng dữ liệu chính trong trường "data" sẽ có cấu trúc bao gồm `id`, `type`, `attributes`, và `relationships`.

## I. Phân Tích và Thiết Kế Cấu Trúc DTO Mới

1.  **Hiểu Rõ Cấu Trúc Mục Tiêu:**
    *   Mỗi thực thể (ví dụ: Manga, Author, Tag) khi được trả về trong trường `data` của response sẽ có dạng:
        ```json
        {
          "id": "string (GUID)",
          "type": "string (e.g., 'manga', 'author', 'tag')",
          "attributes": { /* các thuộc tính riêng của thực thể */ },
          "relationships": [ /* danh sách các thực thể liên quan */ ]
        }
        ```
    *   `id`: Định danh duy nhất của thực thể (dưới dạng chuỗi).
    *   `type`: Chuỗi xác định loại thực thể gốc (ví dụ: "manga", "author", "chapter", "cover_art", "tag"). Tên `type` nên là dạng snake\_case và số ít.
    *   `attributes`: Một object chứa tất cả các thuộc tính (fields) của thực thể đó, trừ `id` và các mối quan hệ.
    *   `relationships`: Một mảng các object, mỗi object đại diện cho một mối quan hệ với thực thể khác. Mỗi relationship object sẽ có:
        *   `id`: Định danh duy nhất của thực thể liên quan (dưới dạng chuỗi).
        *   `type`: **Quan trọng**: Chuỗi này sẽ mô tả *bản chất của mối quan hệ* hoặc *vai trò của thực thể liên quan* đối với thực thể gốc, thay vì luôn là loại của thực thể liên quan.
            *   Ví dụ 1: Đối với một `Manga`:
                *   Nếu một `Author` có vai trò `MangaStaffRole.Author`, `type` trong relationship sẽ là `"author"`.
                *   Nếu một `Author` có vai trò `MangaStaffRole.Artist`, `type` trong relationship sẽ là `"artist"`.
                *   Liên kết đến `CoverArt` sẽ có `type` là `"cover_art"`.
            *   Ví dụ 2: Đối với một `Chapter`:
                *   Liên kết đến `User` (người upload) sẽ có `type` là `"user"` (hoặc có thể là `"uploader"` nếu muốn phân biệt rõ hơn).
                *   Liên kết đến `Manga` (manga gốc của chapter) sẽ có `type` là `"manga"`.
            *   Giá trị của `type` trong `relationships` sẽ được xác định dựa trên ngữ cảnh của mối quan hệ.

2.  **Định Nghĩa Các DTO Mới:**
    *   **DTO Wrapper Cơ Sở (ResourceObject):**
        *   Tạo một DTO cơ sở chung, ví dụ: `Application/Common/Models/ResourceObject.cs`.
            ```csharp
            // Tên file: Application/Common/Models/ResourceObject.cs
            // namespace Application.Common.Models
            // {
            //     public class ResourceObject<TAttributes>
            //     {
            //         public string Id { get; set; } // Guid.ToString()
            //         public string Type { get; set; } // Loại của resource chính, ví dụ "manga", "author"
            //         public TAttributes Attributes { get; set; }
            //         public List<RelationshipObject> Relationships { get; set; } = new List<RelationshipObject>();
            //     }
            // }
            ```
    *   **DTO Cho Mối Quan Hệ (RelationshipObject):**
        *   Tạo DTO `Application/Common/Models/RelationshipObject.cs`. Cấu trúc này không thay đổi, nhưng logic gán giá trị cho `Type` sẽ thay đổi.
            ```csharp
            // Tên file: Application/Common/Models/RelationshipObject.cs
            // namespace Application.Common.Models
            // {
            //     public class RelationshipObject
            //     {
            //         public string Id { get; set; } // Guid.ToString() của thực thể liên quan
            //         public string Type { get; set; } // Loại của mối quan hệ/vai trò, ví dụ "author", "artist", "cover_art"
            //     }
            // }
            ```
    *   **DTOs cho Attributes (Ví dụ: `AuthorAttributesDto`, `MangaAttributesDto`):**
        *   Với mỗi entity (Manga, Author, Tag, Chapter, CoverArt, User, TranslatedManga), tạo một DTO `...AttributesDto.cs` tương ứng trong `Application/Common/DTOs/[EntityName]/`.
        *   Ví dụ:
            *   `Application/Common/DTOs/Authors/AuthorAttributesDto.cs`
            *   `Application/Common/DTOs/Mangas/MangaAttributesDto.cs`
            *   (Các DTO Attributes khác tương tự)
        *   Các DTO `...AttributesDto` này sẽ chứa các trường dữ liệu hiện tại của `AuthorDto`, `MangaDto`,... (trừ Id và các navigation properties).
    *   **Cập nhật DTOs Response Chung:**
        *   `Application/Common/Responses/ApiResponse.cs`:
            *   `ApiResponse<TData>`: `TData` giờ đây sẽ là `ResourceObject<TAttributesDto>`.
        *   `Application/Common/Responses/ApiCollectionResponse.cs`:
            *   `ApiCollectionResponse<TData>`: `TData` giờ đây sẽ là `ResourceObject<TAttributesDto>`. `List<ResourceObject<TAttributesDto>>` sẽ là giá trị của trường `data`.

## II. Cập Nhật Tầng Application

1.  **Tạo/Chỉnh Sửa DTOs:**
    *   Triển khai các DTO `...AttributesDto.cs` như đã thiết kế ở trên.
    *   Triển khai các DTO `ResourceObject.cs` và `RelationshipObject.cs` trong `Application/Common/Models/`.

2.  **Cập Nhật AutoMapper Profiles:**
    *   **Tệp cần cập nhật:** `Application/Common/Mappings/MappingProfile.cs`.
    *   Với mỗi Entity, tạo mapping sang `...AttributesDto` tương ứng.
        *   Ví dụ: `CreateMap<Domain.Entities.Author, AuthorAttributesDto>();`
    *   Việc tạo `ResourceObject<TAttributesDto>` hoàn chỉnh sẽ chủ yếu được thực hiện trong các Query Handlers.

3.  **Cập Nhật Query Handlers:**
    *   **Tệp cần cập nhật:** Tất cả các Query Handlers trong `Application/Features/.../Queries/...QueryHandler.cs`.
    *   **Công việc:**
        *   Các Handler sẽ truy vấn Entity như bình thường, đảm bảo Eager Loading các navigation properties cần thiết cho `relationships`.
        *   Sau khi có Entity, Handler sẽ:
            *   Map Entity sang `...AttributesDto` tương ứng bằng AutoMapper.
            *   Xác định chuỗi `type` cho resource chính (ví dụ: "manga", "author").
            *   **Xây dựng danh sách `List<RelationshipObject>`:**
                *   Duyệt qua các navigation properties của Entity.
                *   Với mỗi entity liên quan, tạo một `RelationshipObject`.
                *   `Id` của `RelationshipObject` sẽ là `Id` của entity liên quan (chuyển thành chuỗi).
                *   `Type` của `RelationshipObject` sẽ được xác định dựa trên vai trò hoặc bản chất của mối quan hệ:
                    *   **Manga -> MangaAuthors:**
                        *   Nếu `MangaAuthor.Role == MangaStaffRole.Author`, thì `RelationshipObject.Type = "author"`.
                        *   Nếu `MangaAuthor.Role == MangaStaffRole.Artist`, thì `RelationshipObject.Type = "artist"`.
                    *   **Manga -> CoverArts:**
                        *   Thông thường sẽ có một "cover art chính". `RelationshipObject.Type = "cover_art"`. Nếu có nhiều, cần logic để xác định cái nào đưa vào relationships (ví dụ: chỉ cái đầu tiên, hoặc cái được đánh dấu là chính).
                    *   **Manga -> MangaTags:**
                        *   `RelationshipObject.Type = "tag"`.
                    *   **Chapter -> User (Uploader):**
                        *   `RelationshipObject.Type = "user"` (hoặc "uploader").
                    *   **Chapter -> TranslatedManga -> Manga:**
                        *   `RelationshipObject.Type = "manga"`.
                    *   **Author -> Mangas (MangaAuthors):**
                        *   `RelationshipObject.Type = "manga"`.
                    *   **Tag -> Mangas (MangaTags):**
                        *   `RelationshipObject.Type = "manga"`.
                *   Tham khảo `api.yaml` của Mangadex để có danh sách các `type` cho relationships của từng entity.
            *   Tạo một instance của `ResourceObject<TAttributesDto>` với `Id` (của entity chính, chuyển thành chuỗi), `type`, `Attributes` (đã map), và `Relationships` (đã xây dựng).
        *   Đối với các Handler trả về danh sách (ví dụ: `GetMangasQueryHandler`):
            *   Thực hiện các bước trên cho mỗi Entity trong danh sách.
            *   Kết quả cuối cùng sẽ là `PagedResult<ResourceObject<MangaAttributesDto>>` (ví dụ cho Manga).

4.  **Cập Nhật Command Handlers (Nếu cần thiết cho response):**
    *   Nếu Command Handlers trả về DTO của entity vừa tạo/cập nhật, DTO đó cũng cần tuân theo cấu trúc `ResourceObject<TAttributesDto>`.
    *   Ví dụ, `CreateMangaCommandHandler` sau khi tạo Manga, có thể cần query lại Manga đó và trả về `ResourceObject<MangaAttributesDto>`.

## III. Cập Nhật Tầng Presentation (API Controllers)

1.  **Tệp cần cập nhật:** Tất cả các API Controllers trong `MangaReaderDB/Controllers/`.
2.  **Công việc:**
    *   **Kiểu trả về của Actions:**
        *   Các action trả về một đối tượng đơn lẻ sẽ có kiểu `ActionResult<ApiResponse<ResourceObject<TAttributesDto>>>`.
        *   Các action trả về danh sách sẽ có kiểu `ActionResult<ApiCollectionResponse<ResourceObject<TAttributesDto>>>`.
    *   **`[ProducesResponseType]`:** Cập nhật các attribute này để phản ánh đúng kiểu DTO mới.
    *   **Logic trong Actions:**
        *   Khi gọi `Mediator.Send(query)`, kết quả nhận được từ Query Handler sẽ là `ResourceObject<TAttributesDto>` (cho một đối tượng) hoặc `PagedResult<ResourceObject<TAttributesDto>>` (cho danh sách).
        *   Sử dụng các phương thức `Ok(data)` hoặc `Created(actionName, routeValues, data)` của `BaseApiController`.

## IV. Cập Nhật DTOs Cho Request Bodies (Create/Update)

*   **Tệp cần cập nhật:**
    *   `Application/Common/DTOs/.../Create...Dto.cs`
    *   `Application/Common/DTOs/.../Update...Dto.cs`
*   **Nguyên tắc:**
    *   Giữ nguyên các DTO này (`CreateAuthorDto`, `UpdateMangaDto`, v.v.) vì chúng đại diện cho payload của request, không phải response.
    *   Cấu trúc bên trong của các DTO này không thay đổi theo cấu trúc `id, type, attributes`. Chúng chỉ chứa các trường cần thiết để tạo hoặc cập nhật entity.

## V. Kiểm Thử (Testing)

1.  **Unit Tests:** Cập nhật unit tests cho các Query Handlers để kiểm tra cấu trúc `ResourceObject` được tạo ra có đúng không, đặc biệt là các `type` trong `relationships`.
2.  **Integration Tests / API Tests:**
    *   Thực hiện gọi API đến tất cả các endpoint GET.
    *   Kiểm tra cấu trúc JSON của response, đảm bảo trường `data` (hoặc các item trong `data` của collection) có đúng các trường `id`, `type`, `attributes`, và `relationships`.
    *   Kiểm tra tính chính xác của giá trị `type` trong `relationships` cho các mối quan hệ khác nhau (ví dụ: author/artist, cover\_art).
    *   Kiểm tra các endpoint POST, PUT, DELETE vẫn hoạt động đúng.

## VI. Cập Nhật Tài Liệu

1.  **Tệp cần cập nhật:**
    *   `docs/api_conventions.md`: Mô tả chi tiết cấu trúc response mới, làm rõ ý nghĩa của `type` trong `relationships`. Cung cấp ví dụ JSON minh họa các `type` linh hoạt này.
    *   Cập nhật các ví dụ response trong tài liệu Swagger/OpenAPI.

## VII. Thứ Tự Thực Hiện Đề Xuất

1.  **Thiết kế và Tạo DTOs:**
    *   Tạo/cập nhật các DTO `...AttributesDto.cs`.
    *   Tạo các DTO `ResourceObject.cs` và `RelationshipObject.cs` trong `Application/Common/Models/`.
2.  **Cập Nhật Mapping:**
    *   Cập nhật `MappingProfile.cs` để map Entities sang các `...AttributesDto` mới.
3.  **Cập Nhật Lớp Response Chung (nếu cần).**
4.  **Cập Nhật Query Handlers:**
    *   **Ưu tiên:** Bắt đầu với `GetMangaByIdQueryHandler` để triển khai logic `relationships` phức tạp (author/artist).
    *   Tiếp tục với các Query Handler khác, triển khai logic xây dựng `ResourceObject` và `relationships` một cách cẩn thận.
5.  **Cập Nhật Controllers:**
    *   Song song với việc cập nhật Query Handler, chỉnh sửa Controller tương ứng.
6.  **Cập Nhật Command Handlers (Nếu Response là DTO).**
7.  **Kiểm thử liên tục.**
8.  **Cập nhật tài liệu API.**

---
Kế hoạch này tập trung vào việc làm cho `type` trong `relationships` trở nên linh hoạt và có ý nghĩa hơn, phản ánh đúng bản chất của mối quan hệ đó.
```