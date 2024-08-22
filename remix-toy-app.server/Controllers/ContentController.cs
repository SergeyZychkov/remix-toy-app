using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace ReactPWA.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class ContentController : ControllerBase
    {
        private readonly IWebHostEnvironment _hostingEnvironment;

        public ContentController(IWebHostEnvironment environment) {
            _hostingEnvironment = environment;
        }
        
        [HttpPost]
        public async Task Upload([FromForm] IFormFile file)
        {
            if (file.Length > 0) 
            {
                string uploads = Path.Combine(_hostingEnvironment.ContentRootPath, "uploads");

                if (!Directory.Exists(uploads))
                {
                    Directory.CreateDirectory(uploads);
                }

                string filePath = Path.Combine(uploads, file.FileName);

                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }

                using (Stream fileStream = new FileStream(filePath, FileMode.Create)) {
                    await file.CopyToAsync(fileStream);
                }
            }
        }

        [HttpGet]
        public List<FileModel> GetFilesFromContentFolder()
        {
            string uploads = Path.Combine(_hostingEnvironment.ContentRootPath, "uploads");

            if (!Directory.Exists(uploads))
            {
                return new List<FileModel>();
            }

            var result = Directory.GetFiles(uploads).Select(x => new FileModel
            {
                Link = x,
                Name = new FileInfo(x).Name
            }).ToList();

            return result;
        }

        [HttpGet]
        public async Task<IActionResult> DownloadFile(string fileName)
        {
            string uploads = Path.Combine(_hostingEnvironment.ContentRootPath, "uploads");
            string filePath = Path.Combine(uploads, fileName);

            new FileExtensionContentTypeProvider().TryGetContentType(fileName, out string? contentType);

            var content = await System.IO.File.ReadAllBytesAsync(filePath);

            return File(content, contentType ?? "application/octet-stream", fileName); 
        }

        public class FileModel
        {
            public string? Name { get; set; }
            public string? Link { get; set; }
        }
    }
}