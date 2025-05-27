using Application.Common.DTOs;
using Application.Common.DTOs.Authors;
using Application.Contracts.Persistence;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace Application.Features.Authors.Queries.GetAuthors
{
    public class GetAuthorsQueryHandler : IRequestHandler<GetAuthorsQuery, PagedResult<AuthorDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetAuthorsQueryHandler> _logger;

        public GetAuthorsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<GetAuthorsQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<PagedResult<AuthorDto>> Handle(GetAuthorsQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("GetAuthorsQueryHandler.Handle - Lấy danh sách tác giả với Offset: {Offset}, Limit: {Limit}, NameFilter: {NameFilter}",
                request.Offset, request.Limit, request.NameFilter);

            // Xây dựng bộ lọc
            Expression<Func<Author, bool>>? filter = null;
            if (!string.IsNullOrWhiteSpace(request.NameFilter))
            {
                filter = author => author.Name.Contains(request.NameFilter); // TODO: [Improvement] Cân nhắc tìm kiếm không dấu/chính xác hơn nếu cần
            }

            // Xây dựng sắp xếp
            // TODO: [Improvement] Xử lý logic sắp xếp động dựa trên request.OrderBy và request.Ascending nếu có
            Func<IQueryable<Author>, IOrderedQueryable<Author>> orderBy = q => q.OrderBy(a => a.Name); // Sắp xếp theo tên mặc định

            var pagedAuthors = await _unitOfWork.AuthorRepository.GetPagedAsync(
                request.Offset,
                request.Limit,
                filter,
                orderBy
            );

            var authorDtos = _mapper.Map<List<AuthorDto>>(pagedAuthors.Items);
            
            return new PagedResult<AuthorDto>(authorDtos, pagedAuthors.Total, request.Offset, request.Limit);
        }
    }
} 