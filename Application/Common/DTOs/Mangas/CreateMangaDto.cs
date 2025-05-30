using Domain.Enums;

namespace Application.Common.DTOs.Mangas
{
    public class CreateMangaDto
    {
        public string Title { get; set; } = string.Empty;
        public string OriginalLanguage { get; set; } = string.Empty; // ISO 639-1 code
        public PublicationDemographic? PublicationDemographic { get; set; }
        public MangaStatus Status { get; set; }
        public int? Year { get; set; }
        public ContentRating ContentRating { get; set; }
        
        public List<Guid>? TagIds { get; set; }
        public List<MangaAuthorInputDto>? Authors { get; set; }
        
        // Thông tin Tags và Authors sẽ được xử lý riêng qua các commands AddMangaTag/AddMangaAuthor
        // hoặc nếu muốn thêm ngay khi tạo Manga, thì Command tương ứng sẽ nhận danh sách ID.
        // Để đơn giản, CreateMangaDto ban đầu không chứa list Authors/Tags.
        // Việc thêm Authors/Tags sẽ thực hiện sau khi Manga được tạo.
    }
} 