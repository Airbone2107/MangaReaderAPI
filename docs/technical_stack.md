# Công Nghệ Sử Dụng (Technical Stack)

Dự án MangaReader API sử dụng các công nghệ và thư viện sau:

## 1. Backend Framework & Runtime

*   **.NET 9**: Nền tảng phát triển chính.
*   **ASP.NET Core 9**: Framework để xây dựng Web API.

## 2. Ngôn Ngữ Lập Trình

*   **C#**: Ngôn ngữ lập trình chính.

## 3. Cơ Sở Dữ Liệu

*   **SQL Server**: Hệ quản trị cơ sở dữ liệu chính.
*   **Entity Framework Core (EF Core)**: Object-Relational Mapper (ORM) để tương tác với SQL Server, sử dụng mô hình Code-First.

## 4. Kiến Trúc và Design Patterns

*   **Domain-Driven Design (DDD)**: Phương pháp tiếp cận thiết kế phần mềm.
*   **Clean Architecture**: Mô hình phân tầng kiến trúc.
*   **CQRS (Command Query Responsibility Segregation)**: Tách biệt logic đọc và ghi dữ liệu.
    *   **MediatR**: Thư viện triển khai CQRS pattern và in-process messaging.
*   **Repository Pattern**: Trừu tượng hóa tầng truy cập dữ liệu.
*   **Unit of Work Pattern**: Quản lý transaction và đảm bảo tính nhất quán dữ liệu.

## 5. Mapping và Validation

*   **AutoMapper**: Thư viện để map đối tượng (ví dụ: từ Entities sang DTOs).
*   **FluentValidation**: Thư viện để định nghĩa và thực thi các quy tắc validation mạnh mẽ và dễ đọc.

## 6. Xử lý Ảnh

*   **Cloudinary**: Dịch vụ lưu trữ và quản lý ảnh trên đám mây.
    *   **CloudinaryDotNet SDK**: Thư viện .NET để tương tác với Cloudinary API.

## 7. API Documentation

*   **Swashbuckle.AspNetCore**: Thư viện để tự động sinh tài liệu OpenAPI (Swagger) cho API.
    *   **Swagger UI**: Giao diện người dùng để tương tác với API documentation.
    *   **ReDoc**: Giao diện người dùng thay thế cho Swagger UI, tập trung vào việc hiển thị tài liệu.

## 8. Development Tools

*   **Visual Studio / Visual Studio Code**: Môi trường phát triển tích hợp (IDE).
*   **Git**: Hệ thống quản lý phiên bản phân tán.
*   **.NET CLI**: Công cụ dòng lệnh cho .NET.
*   **SQL Server Management Studio (SSMS)**: Công cụ quản lý và phát triển cơ sở dữ liệu SQL Server.

## 9. Logging

*   **Microsoft.Extensions.Logging**: Thư viện logging tích hợp sẵn của ASP.NET Core (Sử dụng).

## 10. Configuration Management

*   **appsettings.json**: File cấu hình chính.
*   **User Secrets** (Development): Quản lý các thông tin nhạy cảm trong quá trình phát triển.
*   **Environment Variables** (Staging/Production): Quản lý cấu hình và thông tin nhạy cảm cho môi trường triển khai.