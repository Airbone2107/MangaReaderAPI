using FluentValidation;

namespace Application.Features.Chapters.Commands.UploadChapterPageImage
{
    public class UploadChapterPageImageCommandValidator : AbstractValidator<UploadChapterPageImageCommand>
    {
        public UploadChapterPageImageCommandValidator()
        {
            RuleFor(p => p.ChapterPageId)
                .NotEmpty().WithMessage("ChapterPageId không được để trống.");

            // File (IFormFile) sẽ được validate ở Controller (ví dụ: kích thước, loại file)
            // hoặc trong Handler. Validator này tập trung vào ID.
            // RuleFor(p => p.File)
            //     .NotNull().WithMessage("File ảnh không được để trống.");
        }
    }
} 