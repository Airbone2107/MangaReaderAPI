using System.Text.Json.Serialization;

namespace Application.Common.Responses
{
    public class ApiError
    {
        /// <summary>
        /// Một định danh duy nhất cho loại lỗi cụ thể này (tùy chọn, có thể dùng cho tra cứu tài liệu lỗi).
        /// </summary>
        [JsonPropertyOrder(1)]
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        /// <summary>
        /// HTTP status code áp dụng cho lỗi này.
        /// </summary>
        [JsonPropertyOrder(2)]
        [JsonPropertyName("status")]
        public int Status { get; set; }

        /// <summary>
        /// Tóm tắt ngắn gọn về vấn đề.
        /// </summary>
        [JsonPropertyOrder(3)]
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Giải thích chi tiết về vấn đề.
        /// </summary>
        [JsonPropertyOrder(4)]
        [JsonPropertyName("detail")]
        public string? Detail { get; set; }

        /// <summary>
        /// Thông tin bổ sung về lỗi, có thể là một đối tượng hoặc chuỗi (ví dụ: tên trường gây lỗi validation).
        /// </summary>
        [JsonPropertyOrder(5)]
        [JsonPropertyName("context")]
        public object? Context { get; set; }

        public ApiError(int status, string title, string? detail = null, string? id = null, object? context = null)
        {
            Status = status;
            Title = title;
            Detail = detail;
            Id = id;
            Context = context;
        }
    }
} 