using FluentValidation;

namespace Application.Features.Mangas.Queries.GetMangas
{
    public class GetMangasQueryValidator : AbstractValidator<GetMangasQuery>
    {
        public GetMangasQueryValidator()
        {
            RuleFor(query => query.Limit)
                .GreaterThanOrEqualTo(0).WithMessage("Limit phải lớn hơn hoặc bằng 0.")
                .LessThanOrEqualTo(100).WithMessage("Limit không được vượt quá 100.");

            RuleFor(query => query.Offset)
                .GreaterThanOrEqualTo(0).WithMessage("Offset phải lớn hơn hoặc bằng 0.");

            When(query => !string.IsNullOrWhiteSpace(query.IncludedTagsMode), () => {
                RuleFor(query => query.IncludedTagsMode)
                    .Must(mode => mode.Equals("AND", StringComparison.OrdinalIgnoreCase) || mode.Equals("OR", StringComparison.OrdinalIgnoreCase))
                    .WithMessage("IncludedTagsMode phải là 'AND' hoặc 'OR'.");
                
                RuleFor(query => query.IncludedTags)
                    .NotEmpty().WithMessage("IncludedTags không được rỗng khi IncludedTagsMode được chỉ định.")
                    .When(query => !string.IsNullOrWhiteSpace(query.IncludedTagsMode));
            });

            When(query => query.IncludedTags != null && query.IncludedTags.Any() && string.IsNullOrWhiteSpace(query.IncludedTagsMode), () => {
                 RuleFor(query => query.IncludedTagsMode)
                    .NotEmpty().WithMessage("IncludedTagsMode là bắt buộc khi IncludedTags được cung cấp.");
            });


            When(query => !string.IsNullOrWhiteSpace(query.ExcludedTagsMode), () => {
                RuleFor(query => query.ExcludedTagsMode)
                    .Must(mode => mode.Equals("AND", StringComparison.OrdinalIgnoreCase) || mode.Equals("OR", StringComparison.OrdinalIgnoreCase))
                    .WithMessage("ExcludedTagsMode phải là 'AND' hoặc 'OR'.");

                RuleFor(query => query.ExcludedTags)
                    .NotEmpty().WithMessage("ExcludedTags không được rỗng khi ExcludedTagsMode được chỉ định.")
                    .When(query => !string.IsNullOrWhiteSpace(query.ExcludedTagsMode));
            });
            
            When(query => query.ExcludedTags != null && query.ExcludedTags.Any() && string.IsNullOrWhiteSpace(query.ExcludedTagsMode), () => {
                 RuleFor(query => query.ExcludedTagsMode)
                    .NotEmpty().WithMessage("ExcludedTagsMode là bắt buộc khi ExcludedTags được cung cấp.");
            });

            // Các rule validate khác cho TitleFilter, StatusFilter, etc. có thể được thêm ở đây nếu cần.
            // Ví dụ:
            // RuleFor(query => query.TitleFilter)
            //    .MaximumLength(255).WithMessage("TitleFilter không được vượt quá 255 ký tự.");
        }
    }
} 