using Application.Common.DTOs.Chapters;
using FluentValidation;

namespace Application.Validation.Chapters
{
    public class CreateChapterPageDtoValidator : AbstractValidator<CreateChapterPageDto>
    {
        public CreateChapterPageDtoValidator()
        {
            // ChapterId sẽ được lấy từ route, không cần ở đây
            
            RuleFor(p => p.PageNumber)
                .GreaterThan(0).WithMessage("Số trang phải lớn hơn 0.");
        }
    }
} 