using Application.Common.DTOs.Tags;
using FluentValidation;

namespace Application.Validation.Tags
{
    public class CreateTagDtoValidator : AbstractValidator<CreateTagDto>
    {
        public CreateTagDtoValidator()
        {
            RuleFor(p => p.Name)
                .NotEmpty().WithMessage("Tên tag không được để trống.")
                .MaximumLength(100).WithMessage("Tên tag không được vượt quá 100 ký tự.");

            RuleFor(p => p.TagGroupId)
                .NotEmpty().WithMessage("TagGroupId không được để trống.");
        }
    }
} 