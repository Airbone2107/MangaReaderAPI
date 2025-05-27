# Quy Ước API

Tài liệu này mô tả các quy ước chung được áp dụng cho các endpoint API của MangaReader.

## 1. Định Dạng Endpoint

*   **Base URL**: `/`
*   **Tên Controller**: Sử dụng danh từ số nhiều, tương ứng với tài nguyên (resource) mà nó quản lý.
    *   Ví dụ: `/mangas`, `/authors`, `/chapters`.
*   **Định danh tài nguyên**: Sử dụng GUID cho ID của các tài nguyên chính.
    *   Ví dụ: `/mangas/{mangaId:guid}`.
*   **Hành động trên tài nguyên con**:
    *   Thường được đặt dưới tài nguyên cha.
    *   Ví dụ:
        *   `GET /mangas/{mangaId:guid}/translations` (Lấy các bản dịch của một manga)
        *   `POST /mangas/{mangaId:guid}/covers` (Upload ảnh bìa cho manga)
        *   `GET /chapters/{chapterId:guid}/pages` (Lấy các trang của một chapter)
        *   `POST /chapters/{chapterId:guid}/pages/entry` (Tạo metadata cho một trang của chapter)
        *   `POST /chapterpages/{pageId:guid}/image` (Upload ảnh cho một trang chapter cụ thể - controller `ChapterPagesController` có route `chapterpages`)

## 2. Phương Thức HTTP

Sử dụng các phương thức HTTP một cách chuẩn mực:

*   **GET**: Lấy tài nguyên.
    *   `GET /mangas`: Lấy danh sách manga (có phân trang, lọc).
    *   `GET /mangas/{id}`: Lấy chi tiết một manga.
*   **POST**: Tạo mới tài nguyên.
    *   `POST /mangas`: Tạo một manga mới. Request body chứa DTO tạo mới.
    *   Trả về `201 Created` với `Location` header trỏ đến tài nguyên mới tạo và payload chứa ID (hoặc DTO) của tài nguyên.
*   **PUT**: Cập nhật toàn bộ hoặc một phần tài nguyên đã có.
    *   `PUT /mangas/{id}`: Cập nhật thông tin manga. Request body chứa DTO cập nhật.
    *   Trả về `204 NoContent` nếu thành công.
    *   Trả về `404 NotFound` nếu tài nguyên không tồn tại.
*   **DELETE**: Xóa tài nguyên.
    *   `DELETE /mangas/{id}`: Xóa một manga.
    *   Trả về `204 NoContent` nếu thành công.
    *   Trả về `404 NotFound` nếu tài nguyên không tồn tại.

## 3. Request Body và Response Body

*   Sử dụng định dạng **JSON** cho request và response body.
*   DTOs được định nghĩa trong `Application/Common/DTOs/`.

## 4. Phân Trang (Pagination)

*   Đối với các endpoint trả về danh sách, sử dụng phân trang.
*   Tham số query:
    *   `pageNumber` (mặc định: 1)
    *   `pageSize` (mặc định: 10 hoặc giá trị phù hợp khác)
*   Response Body: Sử dụng `PagedResult<T>` DTO:
    ```json
    {
      "items": [ /* array of T (e.g., MangaDto) */ ],
      "pageNumber": 1,
      "pageSize": 10,
      "totalCount": 100,
      "totalPages": 10,
      "hasPreviousPage": false,
      "hasNextPage": true
    }
    ```

## 5. Lọc (Filtering) và Sắp Xếp (Sorting)

*   Các endpoint lấy danh sách (GET collection) hỗ trợ lọc và sắp xếp qua query parameters.
*   Ví dụ: `GET /mangas?titleFilter=One Piece&statusFilter=Ongoing&orderBy=year&ascending=false`
*   Tên tham số lọc: `[PropertyName]Filter` (ví dụ: `titleFilter`, `statusFilter`).
*   Tham số sắp xếp:
    *   `orderBy`: Tên thuộc tính để sắp xếp (ví dụ: `title`, `createdAt`).
    *   `ascending`: `true` hoặc `false` (mặc định tùy theo endpoint).

## 6. Xử Lý Lỗi và Status Codes

*   **200 OK**: Request thành công (thường cho GET, hoặc POST/PUT nếu có trả về payload).
*   **201 Created**: Tạo tài nguyên thành công (cho POST). Response header `Location` chứa URL của tài nguyên mới.
*   **204 No Content**: Request thành công nhưng không có nội dung trả về (thường cho PUT, DELETE).
*   **400 Bad Request**: Request không hợp lệ.
    *   **Lỗi Validation**: Response body chứa chi tiết lỗi validation.
        ```json
        {
          "title": "Validation Failed",
          "errors": [
            { "propertyName": "Title", "errorMessage": "Tiêu đề không được để trống." },
            { "propertyName": "OriginalLanguage", "errorMessage": "Mã ngôn ngữ phải từ 2 đến 10 ký tự." }
          ]
        }
        ```
    *   **Lỗi Nghiệp Vụ Khác**: Có thể trả về một thông điệp lỗi chung hoặc cấu trúc lỗi tương tự.
*   **401 Unauthorized**: Chưa xác thực (khi implement Authentication).
*   **403 Forbidden**: Đã xác thực nhưng không có quyền truy cập tài nguyên (khi implement Authorization).
*   **404 Not Found**: Tài nguyên không tồn tại.
*   **409 Conflict**: (Ít dùng, nhưng có thể cho trường hợp tạo tài nguyên đã tồn tại và không cho phép ghi đè).
*   **500 Internal Server Error**: Lỗi không mong muốn ở phía server. Response không nên chứa chi tiết lỗi nhạy cảm. Log chi tiết lỗi ở server.

## 7. Versioning (Nếu có)

*   Hiện tại chưa có yêu cầu cụ thể về versioning. Nếu cần, có thể xem xét URL versioning (ví dụ: `/v1/mangas`) hoặc header versioning.

## 8. Idempotency

*   Các request **PUT** và **DELETE** nên được thiết kế để idempotent (gọi nhiều lần với cùng tham số cho kết quả như gọi một lần).
*   Các request **GET** tự bản chất là idempotent.
*   Các request **POST** thường không idempotent.

## 9. Auditing

*   Các trường `CreatedAt` và `UpdatedAt` được tự động quản lý bởi `AuditableEntitySaveChangesInterceptor` cho các entities kế thừa `AuditableEntity`. 