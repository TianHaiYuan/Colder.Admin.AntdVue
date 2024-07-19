using Coldairarrow.Util;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using NSwag.Annotations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Coldairarrow.Api.Controllers.Base_Manage
{
    [Route("/Base_Manage/[controller]/[action]")]
    [OpenApiTag("上传")]
    public class UploadController : BaseApiController
    {
        readonly IConfiguration _configuration;
        public UploadController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost]
        //[AllowAnonymous]
        public async Task<object> UploadFileByForm(IFormFile formFile)
        {
            var files = Request.Form.Files;
            if (files == null || files.Count == 0)
            {
                return Error("No files were selected for upload.");
            }
            List<object> resp = new List<object>();
            foreach (var file in files)
            {
                string path = $"/Upload/{Guid.NewGuid().ToString("N")}/{file.FileName}";
                string physicPath = GetAbsolutePath($"~{path}");
                string dir = Path.GetDirectoryName(physicPath);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                using (FileStream fs = new FileStream(physicPath, FileMode.Create))
                {
                    await file.CopyToAsync(fs);
                }

                string url = $"{_configuration["WebRootUrl"]}{path}";

                var res = new
                {
                    name = file.FileName,
                    status = "done",
                    url
                };
                resp.Add(res);
            }

            return Success(resp);
        }
    }
}
