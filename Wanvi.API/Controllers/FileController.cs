using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Wanvi.Contract.Services.Interfaces;
using Wanvi.Core.Bases;

namespace Wanvi.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileController : ControllerBase
    {
        private readonly IS3Service _s3Service;

        public FileController(IS3Service s3Service)
        {
            _s3Service = s3Service;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            string fileName = await _s3Service.UploadFileAsync(file, "images/");
            return Ok(BaseResponse<string>.OkResponse("Upload thành công!"));
        }

        [HttpDelete("delete/{fileName}")]
        public async Task<IActionResult> DeleteFile(string fileName)
        {
            bool deleted = await _s3Service.DeleteFileAsync(fileName);
            if (deleted)
            {
                return Ok(BaseResponse<string>.OkResponse("Xóa file thành công!"));
            }
            else
            {
                return Ok(BaseResponse<string>.OkResponse("File không tồn tại!"));
            }
        }
    }
}
