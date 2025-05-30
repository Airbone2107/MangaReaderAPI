using Application.Contracts.Persistence;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging; // Thêm logging nếu cần

namespace Application.Features.Authors.Commands.CreateAuthor
{
    public class CreateAuthorCommandHandler : IRequestHandler<CreateAuthorCommand, Guid>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<CreateAuthorCommandHandler> _logger; // Ví dụ logging

        public CreateAuthorCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<CreateAuthorCommandHandler> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Guid> Handle(CreateAuthorCommand request, CancellationToken cancellationToken)
        {
            // Kiểm tra xem tác giả đã tồn tại chưa (nếu cần logic nghiệp vụ này)
            var existingAuthor = await _unitOfWork.AuthorRepository.GetAuthorByNameAsync(request.Name);
            if (existingAuthor != null)
            {
                _logger.LogWarning("Author with name {AuthorName} already exists.", request.Name);
                // Có thể throw một custom exception hoặc trả về một kết quả lỗi cụ thể
                // Ví dụ: throw new Exceptions.ValidationException($"Author with name '{request.Name}' already exists.");
                // Hoặc nếu API trả về ID của author đã tồn tại thì: return existingAuthor.AuthorId;
                // Trong ví dụ này, chúng ta sẽ tạo mới và để DB constraint (nếu có) xử lý (hoặc không cho phép trùng tên)
                // Tùy thuộc vào yêu cầu cụ thể của bạn.
                // Hiện tại, cứ tạo mới.
            }

            var author = _mapper.Map<Author>(request);

            await _unitOfWork.AuthorRepository.AddAsync(author);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Author {AuthorId} created successfully.", author.AuthorId);
            return author.AuthorId;
        }
    }
} 