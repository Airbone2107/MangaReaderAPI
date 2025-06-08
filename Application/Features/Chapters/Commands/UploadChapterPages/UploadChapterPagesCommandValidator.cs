using FluentValidation;

namespace Application.Features.Chapters.Commands.UploadChapterPages
{
    public class UploadChapterPagesCommandValidator : AbstractValidator<UploadChapterPagesCommand>
    {
        public UploadChapterPagesCommandValidator()
        {
            RuleFor(x => x.ChapterId)
                .NotEmpty().WithMessage("Chapter ID is required.");

            RuleFor(x => x.Files)
                .NotEmpty().WithMessage("At least one file is required.")
                .Must(files => files.All(f => f.ImageStream != null && f.ImageStream.Length > 0))
                .WithMessage("All files must have content.")
                .Must(files => files.All(f => !string.IsNullOrEmpty(f.OriginalFileName)))
                .WithMessage("All files must have an original file name.");
            
            RuleForEach(x => x.Files).ChildRules(fileRule =>
            {
                fileRule.RuleFor(f => f.DesiredPageNumber)
                    .GreaterThan(0).WithMessage("Desired page number must be greater than 0.");
                // Thêm các rule validate file khác nếu cần (ví dụ: content type)
                // Tuy nhiên, việc validate chi tiết file (như content type thực sự) thường phức tạp hơn và có thể thực hiện ở tầng khác.
            });
        }
    }
} 