using Coldairarrow.Util;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using NSwag.Annotations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
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

	        /// <summary>
	        /// 上传图片到 BeeImg 图床（后台代理，前端不直接暴露 Token）
	        /// </summary>
	        /// <param name="file">前端上传的图片文件</param>
	        /// <returns></returns>
	        [HttpPost]
	        public async Task<object> UploadToBeeImg(IFormFile file)
	        {
	            var uploadFile = file ?? (Request.Form.Files.Count > 0 ? Request.Form.Files[0] : null);
	            if (uploadFile == null || uploadFile.Length == 0)
	                return Error("请选择要上传的文件！");

	            var uploadUrl = _configuration["BeeImg:UploadUrl"];
	            var token = _configuration["BeeImg:Token"];
	            var storageId = _configuration["BeeImg:StorageId"];

	            if (uploadUrl.IsNullOrEmpty() || token.IsNullOrEmpty() || storageId.IsNullOrEmpty())
	                return Error("BeeImg 图床配置未正确设置，请联系管理员！");

	            using var httpClient = new HttpClient();
	            using var content = new MultipartFormDataContent();
	            using var fileContent = new StreamContent(uploadFile.OpenReadStream());

	            fileContent.Headers.ContentType = new MediaTypeHeaderValue(uploadFile.ContentType ?? "application/octet-stream");
	            content.Add(fileContent, "file", uploadFile.FileName);
	            content.Add(new StringContent(storageId), "storage_id");

	            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
	            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

	            HttpResponseMessage response;
	            try
	            {
	                response = await httpClient.PostAsync(uploadUrl, content);
	            }
	            catch (Exception ex)
	            {
	                return Error("调用 BeeImg 接口失败：" + ex.Message);
	            }

	            var responseBody = await response.Content.ReadAsStringAsync();
	            if (!response.IsSuccessStatusCode)
	                return Error("调用 BeeImg 接口失败：" + responseBody);

	            try
	            {
	                using var doc = JsonDocument.Parse(responseBody);
	                var root = doc.RootElement;
	                if (root.TryGetProperty("status", out var statusElement)
	                    && statusElement.GetString() == "success"
	                    && root.TryGetProperty("data", out var dataElement)
	                    && dataElement.ValueKind == JsonValueKind.Object
	                    && dataElement.TryGetProperty("public_url", out var urlElement))
	                {
	                    var url = urlElement.GetString();
	                    if (!url.IsNullOrEmpty())
	                    {
	                        return Success(new { url });
	                    }
	                }
	            }
	            catch
	            {
	                // 解析 BeeImg 返回结果失败，下面统一按失败处理
	            }

	            return Error("上传到 BeeImg 图床失败，请稍后重试！");
	        }
    }
}
