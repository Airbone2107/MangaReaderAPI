using Application.Common.Responses;
using System.Net;
using System.Text.Json;

namespace MangaReaderDB.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception has occurred.");
                await HandleExceptionAsync(httpContext, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            var response = context.Response;

            var errorResponse = new ApiErrorResponse();
            var statusCode = (int)HttpStatusCode.InternalServerError; // Mặc định là lỗi server
            string title = "Internal Server Error";
            string? detail = _env.IsDevelopment() ? exception.StackTrace : null;

            switch (exception)
            {
                case Application.Exceptions.ValidationException validationEx:
                    statusCode = (int)HttpStatusCode.BadRequest;
                    title = "Validation Failed";
                    response.StatusCode = statusCode;
                    foreach (var err in validationEx.Errors)
                    {
                        errorResponse.Errors.Add(new ApiError(statusCode, err.Key, string.Join(", ", err.Value)));
                    }
                    // Nếu không có lỗi cụ thể từ Errors Dictionary, dùng message của exception
                    if (errorResponse.Errors.Count == 0 && !string.IsNullOrWhiteSpace(validationEx.Message))
                    {
                        errorResponse.Errors.Add(new ApiError(statusCode, title, validationEx.Message));
                    }
                    break;

                case Application.Exceptions.NotFoundException notFoundEx:
                    statusCode = (int)HttpStatusCode.NotFound;
                    title = "Resource Not Found";
                    response.StatusCode = statusCode;
                    errorResponse.Errors.Add(new ApiError(statusCode, title, notFoundEx.Message));
                    break;

                case Application.Exceptions.ApiException apiEx:
                    statusCode = (int)HttpStatusCode.BadRequest; // Hoặc có thể set status code cụ thể hơn nếu ApiException có thuộc tính đó
                    title = "API Error";
                    response.StatusCode = statusCode;
                    errorResponse.Errors.Add(new ApiError(statusCode, title, apiEx.Message));
                    break;

                case Application.Exceptions.DeleteFailureException deleteFailureEx:
                    statusCode = (int)HttpStatusCode.BadRequest; // Hoặc Conflict (409) tùy trường hợp
                    title = "Delete Operation Failed";
                    response.StatusCode = statusCode;
                    errorResponse.Errors.Add(new ApiError(statusCode, title, deleteFailureEx.Message));
                    break;
                
                // Bạn có thể thêm các case cho các loại exception cụ thể khác ở đây
                // Ví dụ: UnauthorizedAccessException, etc.

                default: // Lỗi không xác định
                    response.StatusCode = statusCode; // HttpStatusCode.InternalServerError
                    errorResponse.Errors.Add(new ApiError(statusCode, title, _env.IsDevelopment() ? exception.Message : "An unexpected error occurred."));
                    if (_env.IsDevelopment())
                    {
                         errorResponse.Errors[0].Context = exception.StackTrace;
                    }
                    break;
            }

            var result = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            await context.Response.WriteAsync(result);
        }
    }
} 