using Application.Features.Chapters.Commands.UpdateChapterPageDetails;
using FluentValidation;

namespace Application.Features.Chapters.Commands.UpdateChapterPageDetails
{
    public class UpdateChapterPageDetailsCommandValidator : AbstractValidator<UpdateChapterPageDetailsCommand>
    {
        public UpdateChapterPageDetailsCommandValidator()
        {
            RuleFor(p => p.PageId)
                .NotEmpty().WithMessage("PageId không được để trống.");

            RuleFor(p => p.PageNumber)
                .GreaterThan(0).WithMessage("Số trang phải lớn hơn 0.");
        }
    }
} 