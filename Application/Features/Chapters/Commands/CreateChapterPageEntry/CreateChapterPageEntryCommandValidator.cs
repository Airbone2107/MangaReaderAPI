using Application.Features.Chapters.Commands.CreateChapterPageEntry;
using FluentValidation;

namespace Application.Features.Chapters.Commands.CreateChapterPageEntry
{
    public class CreateChapterPageEntryCommandValidator : AbstractValidator<CreateChapterPageEntryCommand>
    {
        public CreateChapterPageEntryCommandValidator()
        {
            RuleFor(p => p.ChapterId)
                .NotEmpty().WithMessage("ChapterId không được để trống.");

            RuleFor(p => p.PageNumber)
                .GreaterThan(0).WithMessage("Số trang phải lớn hơn 0.");
            
            // PublicId sẽ được gán sau, không validate ở đây.
            // File (Stream/IFormFile) sẽ được xử lý/validate ở Controller hoặc Handler.
        }
    }
} 