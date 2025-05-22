using Application.Features.TagGroups.Commands.UpdateTagGroup;
using FluentValidation;

namespace Application.Features.TagGroups.Commands.UpdateTagGroup
{
    public class UpdateTagGroupCommandValidator : AbstractValidator<UpdateTagGroupCommand>
    {
        public UpdateTagGroupCommandValidator()
        {
            RuleFor(p => p.TagGroupId)
                .NotEmpty().WithMessage("TagGroupId không được để trống.");

            RuleFor(p => p.Name)
                .NotEmpty().WithMessage("Tên nhóm tag không được để trống.")
                .MaximumLength(100).WithMessage("Tên nhóm tag không được vượt quá 100 ký tự.");
        }
    }
} 