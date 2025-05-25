using Application.Common.DTOs.Authors;
using Application.Contracts.Persistence;
using Application.Exceptions;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Authors.Queries.GetAuthorById
{
    public class GetAuthorByIdQueryHandler : IRequestHandler<GetAuthorByIdQuery, AuthorDto?>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetAuthorByIdQueryHandler> _logger;

        public GetAuthorByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<GetAuthorByIdQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<AuthorDto?> Handle(GetAuthorByIdQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("GetAuthorByIdQueryHandler.Handle - Lấy tác giả với ID: {AuthorId}", request.AuthorId);
            var author = await _unitOfWork.AuthorRepository.GetByIdAsync(request.AuthorId);

            if (author == null)
            {
                _logger.LogWarning("Không tìm thấy tác giả với ID: {AuthorId}", request.AuthorId);
                // Theo quy ước hiện tại, Query Handler trả về null nếu không tìm thấy.
                // Controller sẽ quyết định trả về 404 Not Found.
                return null; 
            }

            return _mapper.Map<AuthorDto>(author);
        }
    }
} 