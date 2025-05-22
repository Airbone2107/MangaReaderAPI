using Application.Common.Interfaces;
using Application.Contracts.Persistence;
using Application.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.CoverArts.Commands.DeleteCoverArt
{
    public class DeleteCoverArtCommandHandler : IRequestHandler<DeleteCoverArtCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPhotoAccessor _photoAccessor;
        private readonly ILogger<DeleteCoverArtCommandHandler> _logger;

        public DeleteCoverArtCommandHandler(IUnitOfWork unitOfWork, IPhotoAccessor photoAccessor, ILogger<DeleteCoverArtCommandHandler> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _photoAccessor = photoAccessor ?? throw new ArgumentNullException(nameof(photoAccessor));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Unit> Handle(DeleteCoverArtCommand request, CancellationToken cancellationToken)
        {
            var coverArtToDelete = await _unitOfWork.CoverArtRepository.GetByIdAsync(request.CoverId);

            if (coverArtToDelete == null)
            {
                _logger.LogWarning("CoverArt with ID {CoverId} not found for deletion.", request.CoverId);
                throw new NotFoundException(nameof(Domain.Entities.CoverArt), request.CoverId);
            }

            // 1. Xóa ảnh khỏi Cloudinary
            if (!string.IsNullOrEmpty(coverArtToDelete.PublicId))
            {
                var deletionResult = await _photoAccessor.DeletePhotoAsync(coverArtToDelete.PublicId);
                if (deletionResult != "ok" && deletionResult != "not found")
                {
                    _logger.LogWarning("Failed to delete cover art image {PublicId} from Cloudinary for CoverId {CoverId}. Result: {DeletionResult}", 
                        coverArtToDelete.PublicId, request.CoverId, deletionResult);
                }
            }

            // 2. Xóa CoverArt khỏi DB
            await _unitOfWork.CoverArtRepository.DeleteAsync(coverArtToDelete);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("CoverArt {CoverId} deleted successfully.", request.CoverId);
            return Unit.Value;
        }
    }
} 