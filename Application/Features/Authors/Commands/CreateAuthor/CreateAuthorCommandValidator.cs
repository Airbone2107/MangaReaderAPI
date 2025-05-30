using FluentValidation;

namespace Application.Features.Authors.Commands.CreateAuthor
{
    public class CreateAuthorCommandValidator : AbstractValidator<CreateAuthorCommand>
    {
        public CreateAuthorCommandValidator()
        {
            RuleFor(p => p.Name)
                .NotEmpty().WithMessage("Tên tác giả không được để trống.")
                .MaximumLength(255).WithMessage("Tên tác giả không được vượt quá 255 ký tự.");

            RuleFor(p => p.Biography)
                .MaximumLength(2000).WithMessage("Tiểu sử không được vượt quá 2000 ký tự.");
        }
    }
} 