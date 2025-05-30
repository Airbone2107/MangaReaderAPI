using Application.Contracts.Persistence;
using Application.Exceptions;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Authors.Commands.UpdateAuthor
{
    public class UpdateAuthorCommandHandler : IRequestHandler<UpdateAuthorCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<UpdateAuthorCommandHandler> _logger;

        public UpdateAuthorCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<UpdateAuthorCommandHandler> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Unit> Handle(UpdateAuthorCommand request, CancellationToken cancellationToken)
        {
            var authorToUpdate = await _unitOfWork.AuthorRepository.GetByIdAsync(request.AuthorId);

            if (authorToUpdate == null)
            {
                _logger.LogWarning("Author with ID {AuthorId} not found for update.", request.AuthorId);
                throw new NotFoundException(nameof(Domain.Entities.Author), request.AuthorId);
            }

            // Kiểm tra xem có tác giả khác trùng tên không (nếu tên thay đổi và không cho phép trùng tên)
            if (!string.Equals(authorToUpdate.Name, request.Name, StringComparison.OrdinalIgnoreCase))
            {
                var existingAuthorWithNewName = await _unitOfWork.AuthorRepository.GetAuthorByNameAsync(request.Name);
                if (existingAuthorWithNewName != null && existingAuthorWithNewName.AuthorId != request.AuthorId)
                {
                    _logger.LogWarning("Another author with name {AuthorName} already exists.", request.Name);
                    // throw new Exceptions.ValidationException($"Another author with name '{request.Name}' already exists.");
                }
            }

            _mapper.Map(request, authorToUpdate); // Map từ command vào entity đã tồn tại

            await _unitOfWork.AuthorRepository.UpdateAsync(authorToUpdate);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Author {AuthorId} updated successfully.", request.AuthorId);
            return Unit.Value;
        }
    }
} 