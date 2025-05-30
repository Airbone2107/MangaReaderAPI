using Application.Common.DTOs; // Cho PagedResult
using Application.Common.Responses; // Cho các Api Response mới
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace MangaReaderDB.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public abstract class BaseApiController : ControllerBase
    {
        private IMediator? _mediator;

        // Sử dụng property injection cho Mediator để không cần khai báo constructor ở các controller con
        // Hoặc có thể inject trực tiếp vào constructor của BaseApiController nếu muốn
        protected IMediator Mediator => _mediator ??= HttpContext.RequestServices.GetService<IMediator>()!;

        /// <summary>
        /// Trả về 200 OK với payload là một đối tượng đơn lẻ.
        /// </summary>
        protected ActionResult Ok<T>(T data)
        {
            if (data == null)
            {
                // Nếu data là null, có thể là NotFound. 
                // Controller nên throw NotFoundException để middleware xử lý,
                // hoặc trả về NotFound() trực tiếp nếu logic nghiệp vụ cho phép.
                // Ở đây, giả sử data không null nếu đến được đây.
                // Nếu logic của bạn cho phép data là null và vẫn là OK, thì cần xem xét lại.
                // Thông thường, nếu query không tìm thấy, handler sẽ trả về null,
                // và controller nên return NotFound(); (hoặc throw NotFoundException).
                // Dòng này chủ yếu dành cho các trường hợp data chắc chắn có.
            }
            return base.Ok(new ApiResponse<T>(data));
        }

        /// <summary>
        /// Trả về 200 OK với payload là một danh sách có phân trang.
        /// </summary>
        protected ActionResult Ok<T>(PagedResult<T> pagedData)
        {
            return base.Ok(new ApiCollectionResponse<T>(pagedData));
        }
        
        /// <summary>
        /// Trả về 200 OK với response không có data payload cụ thể (chỉ có "result": "ok").
        /// Dùng cho các action thành công không cần trả về dữ liệu (ví dụ: một số Update/Delete không trả về 204).
        /// </summary>
        protected ActionResult OkResponseForAction()
        {
            return base.Ok(new ApiResponse());
        }

        /// <summary>
        /// Trả về 201 Created với payload là một đối tượng đơn lẻ và location header.
        /// </summary>
        protected ActionResult Created<T>(string actionName, object? routeValues, T value)
        {
            return base.CreatedAtAction(actionName, routeValues, new ApiResponse<T>(value));
        }
        
        // Phương thức NoContent() đã có sẵn và hoạt động tốt (trả về 204 không có body).
        // Phương thức BadRequest() cũng đã có, nhưng ExceptionMiddleware sẽ xử lý việc tạo body cho BadRequest.
        // Phương thức NotFound() cũng đã có, nhưng ExceptionMiddleware sẽ xử lý việc tạo body cho NotFound.
    }
} 