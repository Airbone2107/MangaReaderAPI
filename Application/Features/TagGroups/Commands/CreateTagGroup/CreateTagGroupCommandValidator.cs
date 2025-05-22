using Application.Features.TagGroups.Commands.CreateTagGroup;
using FluentValidation;

namespace Application.Features.TagGroups.Commands.CreateTagGroup
{
    public class CreateTagGroupCommandValidator : AbstractValidator<CreateTagGroupCommand>
    {
        public CreateTagGroupCommandValidator()
        {
            RuleFor(p => p.Name)
                .NotEmpty().WithMessage("Tên nhóm tag không được để trống.")
                .MaximumLength(100).WithMessage("Tên nhóm tag không được vượt quá 100 ký tự.");
        }
    }
} 