using Application.Common.DTOs.TagGroups;
using FluentValidation;

namespace Application.Validation.TagGroups
{
    public class UpdateTagGroupDtoValidator : AbstractValidator<UpdateTagGroupDto>
    {
        public UpdateTagGroupDtoValidator()
        {
            RuleFor(p => p.Name)
                .NotEmpty().WithMessage("Tên nhóm tag không được để trống.")
                .MaximumLength(100).WithMessage("Tên nhóm tag không được vượt quá 100 ký tự.");
        }
    }
}