using FluentValidation;

namespace Application.Features.Chapters.Commands.UpdateChapter
{
    public class UpdateChapterCommandValidator : AbstractValidator<UpdateChapterCommand>
    {
        public UpdateChapterCommandValidator()
        {
            RuleFor(p => p.ChapterId)
                .NotEmpty().WithMessage("ChapterId không được để trống.");

            RuleFor(p => p.Volume)
                .MaximumLength(50).WithMessage("Số tập không được vượt quá 50 ký tự.");

            RuleFor(p => p.ChapterNumber)
                .MaximumLength(50).WithMessage("Số chương không được vượt quá 50 ký tự.");

            RuleFor(p => p.Title)
                .MaximumLength(255).WithMessage("Tiêu đề chương không được vượt quá 255 ký tự.");

            RuleFor(p => p.PublishAt)
                .NotEmpty().WithMessage("Thời điểm xuất bản không được để trống.");
                // .LessThanOrEqualTo(p => p.ReadableAt).WithMessage("Thời điểm xuất bản phải trước hoặc bằng thời điểm có thể đọc.");

            RuleFor(p => p.ReadableAt)
                .NotEmpty().WithMessage("Thời điểm có thể đọc không được để trống.");
        }
    }
} 