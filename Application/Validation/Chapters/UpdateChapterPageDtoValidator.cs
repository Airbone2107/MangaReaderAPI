using Application.Common.DTOs.Chapters;
using FluentValidation;

namespace Application.Validation.Chapters
{
    public class UpdateChapterPageDtoValidator : AbstractValidator<UpdateChapterPageDto>
    {
        public UpdateChapterPageDtoValidator()
        {
            RuleFor(p => p.PageNumber)
                .GreaterThan(0).WithMessage("Số trang phải lớn hơn 0.");
        }
    }
} 