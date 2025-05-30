using FluentValidation.Results;

namespace Application.Exceptions
{
    public class ValidationException : Exception
    {
        public ValidationException()
            : base("One or more validation failures have occurred.")
        {
            Errors = new Dictionary<string, string[]>();
        }

        // Constructor để nhận lỗi từ FluentValidation
        public ValidationException(IEnumerable<ValidationFailure> failures)
            : this()
        {
            Errors = failures
                .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
                .ToDictionary(failureGroup => failureGroup.Key, failureGroup => failureGroup.ToArray());
        }
        
        // Constructor cho các lỗi validation đơn giản không dùng FluentValidation
        public ValidationException(string message) : base(message)
        {
             Errors = new Dictionary<string, string[]>();
        }

        public ValidationException(string propertyName, string errorMessage) : this()
        {
            Errors = new Dictionary<string, string[]> { { propertyName, new[] { errorMessage } } };
        }


        public IDictionary<string, string[]> Errors { get; }
    }
} 