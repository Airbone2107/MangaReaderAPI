using Application.Common.Interfaces;
using Application.Contracts.Persistence;
using Application.Exceptions;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using System.IO;

namespace Application.Features.CoverArts.Commands.UploadCoverArtImage
{
    public class UploadCoverArtImageCommandHandler : IRequestHandler<UploadCoverArtImageCommand, Guid>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPhotoAccessor _photoAccessor;
        private readonly IMapper _mapper; // Giả sử có map từ command sang CoverArt
        private readonly ILogger<UploadCoverArtImageCommandHandler> _logger;

        public UploadCoverArtImageCommandHandler(IUnitOfWork unitOfWork, IPhotoAccessor photoAccessor, IMapper mapper, ILogger<UploadCoverArtImageCommandHandler> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _photoAccessor = photoAccessor ?? throw new ArgumentNullException(nameof(photoAccessor));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Guid> Handle(UploadCoverArtImageCommand request, CancellationToken cancellationToken)
        {
            var manga = await _unitOfWork.MangaRepository.GetByIdAsync(request.MangaId);
            if (manga == null)
            {
                throw new NotFoundException(nameof(Manga), request.MangaId);
            }

            // Tạo desiredPublicId cho Cloudinary
            // KHÔNG BAO GỒM ĐUÔI FILE
            var uniqueId = Guid.NewGuid().ToString("N").Substring(0, 8); // Một phần Guid cho duy nhất
            var desiredPublicId = $"mangas_v2/{request.MangaId}/covers/{request.Volume ?? "default"}_{uniqueId}";
            
            var uploadResult = await _photoAccessor.UploadPhotoAsync(
                request.ImageStream,
                desiredPublicId,
                request.OriginalFileName // originalFileNameForUpload vẫn cần chứa đuôi file cho Cloudinary biết định dạng gốc.
            );

            if (uploadResult == null || string.IsNullOrEmpty(uploadResult.PublicId))
            {
                _logger.LogError("Failed to upload cover art image for Manga {MangaId}.", request.MangaId);
                throw new Exceptions.ApiException("Cover art image upload failed.");
            }

            var coverArt = new CoverArt
            {
                MangaId = request.MangaId,
                Volume = request.Volume,
                Description = request.Description,
                PublicId = uploadResult.PublicId 
                // CoverId sẽ tự sinh
            };
            // Hoặc _mapper.Map<CoverArt>(request); nếu command có đủ thông tin và đã cấu hình mapping
            // và sau đó gán coverArt.PublicId = uploadResult.PublicId;

            await _unitOfWork.CoverArtRepository.AddAsync(coverArt);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("CoverArt {CoverId} (PublicId: {PublicId}) created for Manga {MangaId}.", 
                coverArt.CoverId, coverArt.PublicId, request.MangaId);
            return coverArt.CoverId;
        }
    }
} 