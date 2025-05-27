using System.Text.Json.Serialization;

namespace Application.Common.Responses
{
    /// <summary>
    /// Response chung cho các API trả về một thực thể đơn lẻ hoặc không có dữ liệu cụ thể.
    /// </summary>
    public class ApiResponse
    {
        [JsonPropertyOrder(1)]
        [JsonPropertyName("result")]
        public string Result { get; set; } = "ok";

        public ApiResponse() { }

        public ApiResponse(string result)
        {
            Result = result;
        }
    }

    /// <summary>
    /// Response chung cho các API trả về một thực thể đơn lẻ.
    /// </summary>
    /// <typeparam name="TData">Kiểu dữ liệu của data payload.</typeparam>
    public class ApiResponse<TData> : ApiResponse
    {
        [JsonPropertyOrder(2)]
        [JsonPropertyName("response")]
        public string ResponseType { get; set; } = "entity";

        [JsonPropertyOrder(3)]
        [JsonPropertyName("data")]
        public TData Data { get; set; }

        public ApiResponse(TData data, string responseType = "entity") : base()
        {
            Data = data;
            ResponseType = responseType;
        }
    }
} 