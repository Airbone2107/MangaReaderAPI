using Application.Features.CoverArts.Commands.UploadCoverArtImage;
using FluentValidation;

namespace Application.Features.CoverArts.Commands.UploadCoverArtImage
{
    public class UploadCoverArtImageCommandValidator : AbstractValidator<UploadCoverArtImageCommand>
    {
        public UploadCoverArtImageCommandValidator()
        {
            RuleFor(p => p.MangaId)
                .NotEmpty().WithMessage("MangaId không được để trống.");

            RuleFor(p => p.Volume)
                .MaximumLength(50).WithMessage("Thông tin tập (volume) không được vượt quá 50 ký tự.");

            RuleFor(p => p.Description)
                .MaximumLength(512).WithMessage("Mô tả không được vượt quá 512 ký tự.");

            // File (IFormFile) sẽ được validate ở Controller (ví dụ: kích thước, loại file)
            // RuleFor(p => p.File)
            //     .NotNull().WithMessage("File ảnh bìa không được để trống.");
        }
    }
} 