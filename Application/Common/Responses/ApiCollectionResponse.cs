using Application.Common.DTOs; // Cần cho PagedResult
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Application.Common.Responses
{
    /// <summary>
    /// Response cho các API trả về một danh sách (collection) dữ liệu, thường có phân trang.
    /// </summary>
    /// <typeparam name="TData">Kiểu dữ liệu của các item trong danh sách.</typeparam>
    public class ApiCollectionResponse<TData> : ApiResponse // Kế thừa từ ApiResponse (chỉ có Result)
    {
        // Thuộc tính "Result" (order 1) được kế thừa từ lớp cha ApiResponse.

        [JsonPropertyOrder(2)] // "response" xuất hiện thứ hai
        [JsonPropertyName("response")]
        public string ResponseType { get; set; } = "collection";

        [JsonPropertyOrder(3)] // "data" xuất hiện thứ ba
        [JsonPropertyName("data")]
        public List<TData> Data { get; set; }

        [JsonPropertyOrder(4)] // "limit" xuất hiện thứ tư
        [JsonPropertyName("limit")]
        public int Limit { get; set; }

        [JsonPropertyOrder(5)] // "offset" xuất hiện thứ năm
        [JsonPropertyName("offset")]
        public int Offset { get; set; }

        [JsonPropertyOrder(6)] // "total" xuất hiện thứ sáu
        [JsonPropertyName("total")]
        public int Total { get; set; }

        public ApiCollectionResponse(List<TData> data, int total, int offset, int limit)
            : base("ok") // Gọi constructor của ApiResponse để thiết lập Result = "ok"
        {
            // ResponseType đã có giá trị mặc định là "collection"
            Data = data;
            Total = total;
            Offset = offset;
            Limit = limit;
        }

        public ApiCollectionResponse(PagedResult<TData> pagedResult)
            : this(pagedResult.Items, pagedResult.Total, pagedResult.Offset, pagedResult.Limit)
        {
        }
    }
} 