using Filevoyage.com.Services;
using Microsoft.AspNetCore.Mvc;

namespace Filevoyage.com.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FileController : ControllerBase
    {
        private readonly AzureStorageService _storage;

        public FileController(AzureStorageService storage)
        {
            _storage = storage;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            using var stream = file.OpenReadStream();
            await _storage.UploadFileAsync(file.FileName, stream);
            return Ok($"File '{file.FileName}' uploaded.");
        }

        [HttpGet("download")]
        public async Task<IActionResult> Download([FromQuery] string name)
        {
            var fileStream = await _storage.DownloadFileAsync(name);
            if (fileStream == null)
                return NotFound("File not found.");

            return File(fileStream, "application/octet-stream", name);
        }

        [HttpGet("list")]
        public async Task<IActionResult> List()
        {
            var files = await _storage.ListFilesAsync();
            return Ok(files);
        }

        [HttpDelete("delete")]
        public async Task<IActionResult> Delete([FromQuery] string name)
        {
            await _storage.DeleteFileAsync(name);
            return Ok($"File '{name}' deleted.");
        }
    }
}
