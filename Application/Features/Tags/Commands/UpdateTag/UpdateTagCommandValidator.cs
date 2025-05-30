using FluentValidation;

namespace Application.Features.Tags.Commands.UpdateTag
{
    public class UpdateTagCommandValidator : AbstractValidator<UpdateTagCommand>
    {
        public UpdateTagCommandValidator()
        {
            RuleFor(p => p.TagId)
                .NotEmpty().WithMessage("TagId không được để trống.");

            RuleFor(p => p.Name)
                .NotEmpty().WithMessage("Tên tag không được để trống.")
                .MaximumLength(100).WithMessage("Tên tag không được vượt quá 100 ký tự.");

            RuleFor(p => p.TagGroupId)
                .NotEmpty().WithMessage("TagGroupId không được để trống.");
        }
    }
} 