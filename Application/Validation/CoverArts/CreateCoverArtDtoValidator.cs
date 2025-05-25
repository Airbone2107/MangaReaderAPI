using Application.Common.DTOs.CoverArts;
using FluentValidation;

namespace Application.Validation.CoverArts
{
    public class CreateCoverArtDtoValidator : AbstractValidator<CreateCoverArtDto>
    {
        public CreateCoverArtDtoValidator()
        {
            // MangaId sẽ được lấy từ route, không cần validate ở đây nếu DTO này dùng cho FromForm
            // RuleFor(p => p.MangaId)
            //     .NotEmpty().WithMessage("MangaId không được để trống.");

            RuleFor(p => p.Volume)
                .MaximumLength(50).WithMessage("Thông tin tập (volume) không được vượt quá 50 ký tự.");

            RuleFor(p => p.Description)
                .MaximumLength(512).WithMessage("Mô tả không được vượt quá 512 ký tự.");
        }
    }
} 