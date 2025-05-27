using Application.Common.DTOs;
using Application.Common.DTOs.Chapters;
using Application.Common.Models;
using Application.Contracts.Persistence;
using AutoMapper;
using Domain.Entities; // Cần cho ChapterPage
using MediatR;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions; // Cần cho Expression

namespace Application.Features.Chapters.Queries.GetChapterPages
{
    public class GetChapterPagesQueryHandler : IRequestHandler<GetChapterPagesQuery, PagedResult<ResourceObject<ChapterPageAttributesDto>>>
    {
        private readonly IUnitOfWork _unitOfWork; // Sẽ dùng ChapterRepository
        private readonly IMapper _mapper;
        private readonly ILogger<GetChapterPagesQueryHandler> _logger;

        public GetChapterPagesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<GetChapterPagesQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<PagedResult<ResourceObject<ChapterPageAttributesDto>>> Handle(GetChapterPagesQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("GetChapterPagesQueryHandler.Handle - Lấy các trang cho ChapterId: {ChapterId}, Offset: {Offset}, Limit: {Limit}",
                request.ChapterId, request.Offset, request.Limit);

            // Kiểm tra sự tồn tại của Chapter
            var chapterExists = await _unitOfWork.ChapterRepository.ExistsAsync(request.ChapterId);
            if (!chapterExists)
            {
                _logger.LogWarning("Không tìm thấy Chapter với ID: {ChapterId} khi lấy danh sách trang.", request.ChapterId);
                return new PagedResult<ResourceObject<ChapterPageAttributesDto>>(new List<ResourceObject<ChapterPageAttributesDto>>(), 0, request.Offset, request.Limit);
            }
            
            // TODO: [Improvement] Hiện tại, IChapterRepository không có phương thức GetPagedAsync cho ChapterPage.
            // Phương thức GetPagedAsync trong GenericRepository hoạt động trên DbSet<T>.
            // Để lấy ChapterPage có phân trang hiệu quả từ DB, bạn nên:
            // 1. Thêm phương thức vào IChapterRepository:
            //    Task<PagedResult<ChapterPage>> GetPagedPagesByChapterAsync(Guid chapterId, int offset, int limit);
            // 2. Triển khai phương thức đó trong ChapterRepository sử dụng _context.ChapterPages.GetPagedAsync(...) (cần access DbSet ChapterPages)
            //    (Để làm điều này, bạn có thể cần chỉnh sửa GenericRepository hoặc ChapterRepository để truy cập _context hoặc DbSet<ChapterPage>).
            //    Ví dụ triển khai trong ChapterRepository:
            //    public async Task<PagedResult<ChapterPage>> GetPagedPagesByChapterAsync(Guid chapterId, int offset, int limit)
            //    {
            //        var query = _context.ChapterPages.Where(cp => cp.ChapterId == chapterId).OrderBy(cp => cp.PageNumber);
            //        var totalCount = await query.CountAsync();
            //        var items = await query.Skip(offset).Take(limit).AsNoTracking().ToListAsync();
            //        return new PagedResult<ChapterPage>(items, totalCount, offset, limit);
            //    }
            // 3. Sau đó, gọi phương thức mới đó ở đây.

            // Cách làm hiện tại (tạm thời và KHÔNG tối ưu cho DB lớn): Load Chapter với tất cả Pages, rồi phân trang trong bộ nhớ.
            // Điều này chỉ phù hợp với số lượng trang nhỏ.
            var chapterWithPages = await _unitOfWork.ChapterRepository.GetChapterWithPagesAsync(request.ChapterId);
            if (chapterWithPages == null || chapterWithPages.ChapterPages == null)
            {
                 _logger.LogWarning("Chapter with ID {ChapterId} found but has no pages.", request.ChapterId);
                 return new PagedResult<ResourceObject<ChapterPageAttributesDto>>(new List<ResourceObject<ChapterPageAttributesDto>>(), 0, request.Offset, request.Limit);
            }

            var allPages = chapterWithPages.ChapterPages.OrderBy(p => p.PageNumber).ToList(); // Lấy tất cả trang và sắp xếp
            var totalCount = allPages.Count;
            
            // Phân trang trong bộ nhớ
            var items = allPages.Skip(request.Offset)
                                .Take(request.Limit)
                                .ToList();

            var resourceObjects = items.Select(page => {
                var attributes = _mapper.Map<ChapterPageAttributesDto>(page);
                var relationships = new List<RelationshipObject>
                {
                    new RelationshipObject { Id = page.ChapterId.ToString(), Type = "chapter" }
                };
                return new ResourceObject<ChapterPageAttributesDto>
                {
                    Id = page.PageId.ToString(),
                    Type = "chapter_page", 
                    Attributes = attributes,
                    Relationships = relationships // ChapterPage always has a relationship to chapter
                };
            }).ToList();
            
            return new PagedResult<ResourceObject<ChapterPageAttributesDto>>(resourceObjects, totalCount, request.Offset, request.Limit);
        }
    }
} 