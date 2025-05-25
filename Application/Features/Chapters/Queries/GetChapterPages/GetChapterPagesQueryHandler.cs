using Application.Common.DTOs;
using Application.Common.DTOs.Chapters;
using Application.Contracts.Persistence;
using AutoMapper;
using Domain.Entities; // Cần cho ChapterPage
using MediatR;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions; // Cần cho Expression

namespace Application.Features.Chapters.Queries.GetChapterPages
{
    public class GetChapterPagesQueryHandler : IRequestHandler<GetChapterPagesQuery, PagedResult<ChapterPageDto>>
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

        public async Task<PagedResult<ChapterPageDto>> Handle(GetChapterPagesQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("GetChapterPagesQueryHandler.Handle - Lấy các trang cho ChapterId: {ChapterId}, PageNumber: {PageNumber}, PageSize: {PageSize}",
                request.ChapterId, request.PageNumber, request.PageSize);

            // Kiểm tra sự tồn tại của Chapter
            var chapterExists = await _unitOfWork.ChapterRepository.ExistsAsync(request.ChapterId);
            if (!chapterExists)
            {
                _logger.LogWarning("Không tìm thấy Chapter với ID: {ChapterId} khi lấy danh sách trang.", request.ChapterId);
                return new PagedResult<ChapterPageDto>(new List<ChapterPageDto>(), 0, request.PageNumber, request.PageSize);
            }
            
            // TODO: [Improvement] Hiện tại, IChapterRepository không có phương thức GetPagedAsync cho ChapterPage.
            // Phương thức GetPagedAsync trong GenericRepository hoạt động trên DbSet<T>.
            // Để lấy ChapterPage có phân trang hiệu quả từ DB, bạn nên:
            // 1. Thêm phương thức vào IChapterRepository:
            //    Task<PagedResult<ChapterPage>> GetPagedPagesByChapterAsync(Guid chapterId, int pageNumber, int pageSize);
            // 2. Triển khai phương thức đó trong ChapterRepository sử dụng _context.ChapterPages.GetPagedAsync(...) (cần access DbSet ChapterPages)
            //    (Để làm điều này, bạn có thể cần chỉnh sửa GenericRepository hoặc ChapterRepository để truy cập _context hoặc DbSet<ChapterPage>).
            //    Ví dụ triển khai trong ChapterRepository:
            //    public async Task<PagedResult<ChapterPage>> GetPagedPagesByChapterAsync(Guid chapterId, int pageNumber, int pageSize)
            //    {
            //        var query = _context.ChapterPages.Where(cp => cp.ChapterId == chapterId).OrderBy(cp => cp.PageNumber);
            //        var totalCount = await query.CountAsync();
            //        var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).AsNoTracking().ToListAsync();
            //        return new PagedResult<ChapterPage>(items, totalCount, pageNumber, pageSize);
            //    }
            // 3. Sau đó, gọi phương thức mới đó ở đây.

            // Cách làm hiện tại (tạm thời và KHÔNG tối ưu cho DB lớn): Load Chapter với tất cả Pages, rồi phân trang trong bộ nhớ.
            // Điều này chỉ phù hợp với số lượng trang nhỏ.
            var chapterWithPages = await _unitOfWork.ChapterRepository.GetChapterWithPagesAsync(request.ChapterId);
            if (chapterWithPages == null || chapterWithPages.ChapterPages == null)
            {
                 _logger.LogWarning("Chapter with ID {ChapterId} found but has no pages.", request.ChapterId);
                 return new PagedResult<ChapterPageDto>(new List<ChapterPageDto>(), 0, request.PageNumber, request.PageSize);
            }

            var allPages = chapterWithPages.ChapterPages.OrderBy(p => p.PageNumber).ToList(); // Lấy tất cả trang và sắp xếp
            var totalCount = allPages.Count;
            
            // Phân trang trong bộ nhớ
            var items = allPages.Skip((request.PageNumber - 1) * request.PageSize)
                                .Take(request.PageSize)
                                .ToList();

            var pageDtos = _mapper.Map<List<ChapterPageDto>>(items);
            return new PagedResult<ChapterPageDto>(pageDtos, totalCount, request.PageNumber, request.PageSize);
        }
    }
} 