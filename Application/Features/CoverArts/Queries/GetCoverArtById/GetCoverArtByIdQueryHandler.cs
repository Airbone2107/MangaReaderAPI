using Application.Common.DTOs.CoverArts;
using Application.Contracts.Persistence;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.CoverArts.Queries.GetCoverArtById
{
    public class GetCoverArtByIdQueryHandler : IRequestHandler<GetCoverArtByIdQuery, CoverArtDto?>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetCoverArtByIdQueryHandler> _logger;

        public GetCoverArtByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<GetCoverArtByIdQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<CoverArtDto?> Handle(GetCoverArtByIdQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("GetCoverArtByIdQueryHandler.Handle - Lấy cover art với ID: {CoverId}", request.CoverId);
            var coverArt = await _unitOfWork.CoverArtRepository.GetByIdAsync(request.CoverId);

            if (coverArt == null)
            {
                _logger.LogWarning("Không tìm thấy cover art với ID: {CoverId}", request.CoverId);
                return null;
            }
            return _mapper.Map<CoverArtDto>(coverArt);
        }
    }
} 