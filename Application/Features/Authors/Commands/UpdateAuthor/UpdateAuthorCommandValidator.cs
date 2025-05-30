using FluentValidation;

namespace Application.Features.Authors.Commands.UpdateAuthor
{
    public class UpdateAuthorCommandValidator : AbstractValidator<UpdateAuthorCommand>
    {
        public UpdateAuthorCommandValidator()
        {
            RuleFor(p => p.AuthorId)
                .NotEmpty().WithMessage("AuthorId không được để trống.");

            RuleFor(p => p.Name)
                .NotEmpty().WithMessage("Tên tác giả không được để trống.")
                .MaximumLength(255).WithMessage("Tên tác giả không được vượt quá 255 ký tự.");

            RuleFor(p => p.Biography)
                .MaximumLength(2000).WithMessage("Tiểu sử không được vượt quá 2000 ký tự.");
        }
    }
} 