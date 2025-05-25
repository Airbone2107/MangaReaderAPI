using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection; // Cho GetService

namespace MangaReaderDB.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public abstract class BaseApiController : ControllerBase
    {
        private IMediator? _mediator;

        // Sử dụng property injection cho Mediator để không cần khai báo constructor ở các controller con
        // Hoặc có thể inject trực tiếp vào constructor của BaseApiController nếu muốn
        protected IMediator Mediator => _mediator ??= HttpContext.RequestServices.GetService<IMediator>()!;

        // Helper để xử lý kết quả từ MediatR (ví dụ)
        protected ActionResult HandleResult<T>(T? result)
        {
            if (result == null) return NotFound();
            // if (result.IsSuccess && result.Value != null) return Ok(result.Value);
            // if (result.IsSuccess && result.Value == null) return NotFound();
            // return BadRequest(result.Error); 
            // -> Phần này cần điều chỉnh tùy theo cách bạn định nghĩa kết quả trả về từ Handler.
            // Hiện tại, chúng ta trả về DTO trực tiếp hoặc null.
            return Ok(result);
        }
        
        protected ActionResult HandleUnitResult(MediatR.Unit result) // Dùng cho các command không trả về dữ liệu cụ thể
        {
            // Với Unit, thường thành công sẽ là NoContent hoặc Ok.
            return NoContent(); // Hoặc Ok() nếu muốn trả về status 200.
        }
    }
} 