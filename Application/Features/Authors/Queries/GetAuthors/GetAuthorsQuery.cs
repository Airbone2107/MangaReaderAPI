using Application.Common.DTOs;
using Application.Common.DTOs.Authors;
using MediatR;

namespace Application.Features.Authors.Queries.GetAuthors
{
    public class GetAuthorsQuery : IRequest<PagedResult<AuthorDto>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? NameFilter { get; set; }
        // TODO: [Improvement] Thêm các tham số sắp xếp nếu cần, ví dụ:
        // public string OrderBy { get; set; } = "Name"; // Mặc định sắp xếp theo tên
        // public bool Ascending { get; set; } = true;
    }
} 