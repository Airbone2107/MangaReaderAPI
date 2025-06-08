# API Conventions

## 1. Base URL

Tất cả các API endpoints đều sử dụng tên của controller làm đường dẫn gốc. Ví dụ:

- `https://api.mangareader.com/mangas` (cho controller `MangasController`)
- `https://api.mangareader.com/authors` (cho controller `AuthorsController`)

Một số endpoint có thể có đường dẫn tùy chỉnh (absolute path) bắt đầu bằng `/` để tạo mối quan hệ rõ ràng hơn giữa các tài nguyên. Ví dụ:

- `https://api.mangareader.com/mangas/{mangaId}/covers`
- `https://api.mangareader.com/translatedmangas/{translatedMangaId}/chapters`

## 2. HTTP Methods

| Method | Mục đích |
|--------|----------|
| GET    | Lấy dữ liệu |
| POST   | Tạo mới dữ liệu |
| PUT    | Cập nhật toàn bộ dữ liệu |
| DELETE | Xóa dữ liệu |

## 3. Status Codes

| Status Code | Ý nghĩa |
|-------------|---------|
| 200 OK | Request thành công |
| 201 Created | Tạo mới thành công |
| 204 No Content | Request thành công, không có dữ liệu trả về |
| 400 Bad Request | Request không hợp lệ (lỗi validation) |
| 404 Not Found | Không tìm thấy tài nguyên |
| 500 Internal Server Error | Lỗi server |

## 4. Pagination

Các endpoints trả về danh sách đều hỗ trợ phân trang với các tham số:

- `offset`: Vị trí bắt đầu (mặc định: 0)
- `limit`: Số lượng tối đa kết quả trả về (mặc định và tối đa: 20)

Ví dụ:

```
GET /mangas?offset=20&limit=10
```

## 5. Filtering và Sorting

Các endpoints trả về danh sách hỗ trợ lọc và sắp xếp:

- Filtering: Tùy thuộc vào từng endpoint, ví dụ `titleFilter`, `statusFilter`
- Sorting: Sử dụng tham số `orderBy` và `ascending`

Ví dụ:

```
GET /mangas?statusFilter=ongoing&orderBy=title&ascending=true
```

## 6. Cấu Trúc Response Body (JSON)

Tất cả các response thành công (200 OK, 201 Created) trả về dữ liệu sẽ tuân theo cấu trúc sau:

### 6.1. Response Cho Một Đối Tượng Đơn Lẻ

```json
{
  "result": "ok", // Luôn là "ok" cho response thành công
  "response": "entity", // Loại response, ví dụ "entity" hoặc "collection"
  "data": {
    "id": "string (GUID)",
    "type": "string (loại của resource, ví dụ: 'manga', 'author')",
    "attributes": {
      // Các thuộc tính cụ thể của resource (trừ id và relationships)
      // Ví dụ cho MangaAttributesDto:
      // "title": "One Piece",
      // "originalLanguage": "ja",
      // "status": "Ongoing",
      // ...
    },
    "relationships": [
      {
        "id": "string (GUID của entity liên quan)",
        "type": "string (loại của MỐI QUAN HỆ hoặc VAI TRÒ, ví dụ: 'author', 'artist', 'tag', 'cover_art')"
      }
      // ... các relationships khác ...
    ]
  }
}
```

*   **`data.id`**: ID của tài nguyên chính (luôn là GUID dưới dạng chuỗi).
*   **`data.type`**: Loại của tài nguyên chính (ví dụ: `"manga"`, `"author"`, `"tag"`, `"chapter"`, `"cover_art"`, `"translated_manga"`, `"tag_group"`, `"chapter_page"`). Được viết bằng snake_case, số ít.
*   **`data.attributes`**: Một object chứa tất cả các thuộc tính của tài nguyên (tương ứng với `...AttributesDto`).
*   **`data.relationships`**: (Tùy chọn, có thể không có nếu không có mối quan hệ) Một mảng các đối tượng `RelationshipObject`.
    *   **`id`**: ID của thực thể liên quan.
    *   **`type`**: Mô tả vai trò hoặc bản chất của mối quan hệ đó đối với thực thể gốc.
        *   Ví dụ, đối với một Manga:
            *   Relationship tới Author với vai trò `author`: `{ "id": "author-guid", "type": "author" }`
            *   Relationship tới Author với vai trò `artist`: `{ "id": "artist-guid", "type": "artist" }`
            *   Relationship tới Tag: `{ "id": "tag-guid", "type": "tag" }`
            *   Relationship tới CoverArt chính: `{ "id": "cover_art-guid", "type": "cover_art" }`
        *   Đối với một Chapter:
            *   Relationship tới User (uploader): `{ "id": "user-id", "type": "user" }`
            *   Relationship tới Manga (manga gốc của chapter, thông qua TranslatedManga): `{ "id": "manga-guid", "type": "manga" }`
            *   Relationship tới TranslatedManga (bản dịch chứa chapter này): `{ "id": "translated-manga-guid", "type": "translated_manga" }`

### 6.2. Response Cho Danh Sách Đối Tượng (Collection)

```json
{
  "result": "ok",
  "response": "collection",
  "data": [
    {
      "id": "string (GUID)",
      "type": "string (loại của resource)",
      "attributes": { /* ... */ },
      "relationships": [ /* ... */ ]
    }
    // ... các resource objects khác ...
  ],
  "limit": 10,
  "offset": 0,
  "total": 100
}
```
*   Trường `data` là một mảng các `ResourceObject` như mô tả ở mục 6.1.
*   `limit`, `offset`, `total` là các thông tin phân trang.

### 6.3. Ví dụ Response Cho Manga

```json
{
  "result": "ok",
  "response": "entity",
  "data": {
    "id": "123e4567-e89b-12d3-a456-426614174000",
    "type": "manga",
    "attributes": {
      "title": "One Piece",
      "originalLanguage": "ja",
      "publicationDemographic": "Shounen",
      "status": "Ongoing",
      "year": 1997,
      "contentRating": "Safe",
      "isLocked": false,
      "createdAt": "2023-01-01T00:00:00Z",
      "updatedAt": "2023-06-01T00:00:00Z"
    },
    "relationships": [
      {
        "id": "223e4567-e89b-12d3-a456-426614174001",
        "type": "author"
      },
      {
        "id": "223e4567-e89b-12d3-a456-426614174001",
        "type": "artist"
      },
      {
        "id": "323e4567-e89b-12d3-a456-426614174002",
        "type": "tag"
      },
      {
        "id": "423e4567-e89b-12d3-a456-426614174003",
        "type": "cover_art"
      }
    ]
  }
}
```

### 6.4. Ví dụ Response Cho Danh Sách Tags

```json
{
  "result": "ok",
  "response": "collection",
  "data": [
    {
      "id": "123e4567-e89b-12d3-a456-426614174000",
      "type": "tag",
      "attributes": {
        "name": "Action",
        "tagGroupId": "223e4567-e89b-12d3-a456-426614174001",
        "tagGroupName": "Genres",
        "createdAt": "2023-01-01T00:00:00Z",
        "updatedAt": "2023-01-01T00:00:00Z"
      },
      "relationships": [
        {
          "id": "223e4567-e89b-12d3-a456-426614174001",
          "type": "tag_group"
        }
      ]
    },
    {
      "id": "123e4567-e89b-12d3-a456-426614174002",
      "type": "tag",
      "attributes": {
        "name": "Adventure",
        "tagGroupId": "223e4567-e89b-12d3-a456-426614174001",
        "tagGroupName": "Genres",
        "createdAt": "2023-01-01T00:00:00Z",
        "updatedAt": "2023-01-01T00:00:00Z"
      },
      "relationships": [
        {
          "id": "223e4567-e89b-12d3-a456-426614174001",
          "type": "tag_group"
        }
      ]
    }
  ],
  "limit": 10,
  "offset": 0,
  "total": 50
}
```

## 7. Cấu Trúc Error Response

```json
{
  "result": "error",
  "errors": [
    {
      "code": 404,
      "title": "Not Found",
      "detail": "Manga with ID '123e4567-e89b-12d3-a456-426614174000' was not found."
    }
  ]
}
```

## 8. Validation Errors

```json
{
  "result": "error",
  "errors": [
    {
      "code": 400,
      "title": "Validation Error",
      "detail": "The Title field is required.",
      "source": {
        "field": "Title"
      }
    },
    {
      "code": 400,
      "title": "Validation Error",
      "detail": "The OriginalLanguage field is required.",
      "source": {
        "field": "OriginalLanguage"
      }
    }
  ]
}
```

## 9. Các Loại Relationship Type

Dưới đây là danh sách các giá trị `type` được sử dụng trong các đối tượng `ResourceObject` (cho chính tài nguyên) và `RelationshipObject` (cho mối quan hệ):

| Giá trị `type` | Mô tả                                       | Nơi xuất hiện                                  |
|----------------|---------------------------------------------|------------------------------------------------|
| `author`       | Tác giả của manga                            | `ResourceObject` (cho Author); `RelationshipObject` (Manga -> Author) |
| `artist`       | Họa sĩ của manga                             | `RelationshipObject` (Manga -> Author)         |
| `tag`          | Thẻ gắn với manga                            | `ResourceObject` (cho Tag); `RelationshipObject` (Manga -> Tag) |
| `tag_group`    | Nhóm chứa tag                               | `ResourceObject` (cho TagGroup); `RelationshipObject` (Tag -> TagGroup) |
| `cover_art`    | Ảnh bìa của manga                            | `ResourceObject` (cho CoverArt); `RelationshipObject` (Manga -> CoverArt) |
| `manga`        | Manga gốc                                   | `ResourceObject` (cho Manga); `RelationshipObject` (Chapter/TranslatedManga/CoverArt -> Manga) |
| `user`         | Người dùng tải lên                           | `ResourceObject` (cho User - nếu có API riêng cho User); `RelationshipObject` (Chapter -> User) |
| `chapter`      | Chương của manga                             | `ResourceObject` (cho Chapter); `RelationshipObject` (ChapterPage -> Chapter) |
| `chapter_page` | Trang của chương                             | `ResourceObject` (cho ChapterPage)             |
| `translated_manga` | Bản dịch của manga                       | `ResourceObject` (cho TranslatedManga); `RelationshipObject` (Chapter -> TranslatedManga) |

## 10. Các Endpoints Chính

### Mangas

- `GET /mangas`: Lấy danh sách manga
- `GET /mangas/{id}`: Lấy thông tin chi tiết manga
- `POST /mangas`: Tạo manga mới (bao gồm cả tags và authors)
- `PUT /mangas/{id}`: Cập nhật manga (bao gồm cả tags và authors)
- `DELETE /mangas/{id}`: Xóa manga

### Authors

- `GET /authors`: Lấy danh sách tác giả
- `GET /authors/{id}`: Lấy thông tin chi tiết tác giả
- `POST /authors`: Tạo tác giả mới
- `PUT /authors/{id}`: Cập nhật tác giả
- `DELETE /authors/{id}`: Xóa tác giả

### Tags

- `GET /tags`: Lấy danh sách tag
- `GET /tags/{id}`: Lấy thông tin chi tiết tag
- `POST /tags`: Tạo tag mới
- `PUT /tags/{id}`: Cập nhật tag
- `DELETE /tags/{id}`: Xóa tag

### TagGroups

- `GET /taggroups`: Lấy danh sách nhóm tag
- `GET /taggroups/{id}`: Lấy thông tin chi tiết nhóm tag
- `POST /taggroups`: Tạo nhóm tag mới
- `PUT /taggroups/{id}`: Cập nhật nhóm tag
- `DELETE /taggroups/{id}`: Xóa nhóm tag

### Chapters

- `GET /chapters/{id}`: Lấy thông tin chi tiết chapter
- `GET /translatedmangas/{translatedMangaId}/chapters`: Lấy danh sách chapter của một bản dịch
- `POST /chapters`: Tạo chapter mới
- `PUT /chapters/{id}`: Cập nhật chapter
- `DELETE /chapters/{id}`: Xóa chapter
- `GET /chapters/{chapterId}/pages`: Lấy danh sách trang của chapter
- `POST /chapters/{chapterId}/pages/entry`: Tạo entry cho trang mới

### ChapterPages

- `POST /chapterpages/{pageId}/image`: Upload ảnh cho trang
- `PUT /chapterpages/{pageId}/details`: Cập nhật thông tin trang
- `DELETE /chapterpages/{pageId}`: Xóa trang

### CoverArts

- `GET /coverarts/{id}`: Lấy thông tin chi tiết ảnh bìa
- `GET /mangas/{mangaId}/covers`: Lấy danh sách ảnh bìa của manga
- `POST /mangas/{mangaId}/covers`: Upload ảnh bìa mới
- `DELETE /coverarts/{id}`: Xóa ảnh bìa

### TranslatedMangas

- `GET /translatedmangas/{id}`: Lấy thông tin chi tiết bản dịch
- `GET /mangas/{mangaId}/translations`: Lấy danh sách bản dịch của manga
- `POST /translatedmangas`: Tạo bản dịch mới
- `PUT /translatedmangas/{id}`: Cập nhật bản dịch
- `DELETE /translatedmangas/{id}`: Xóa bản dịch
```