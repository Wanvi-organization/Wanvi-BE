using Amazon.S3;
using Amazon.Runtime;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;
using Amazon.S3.Model;
using Wanvi.Core.Bases;
using Wanvi.Core.Constants;
using Wanvi.Contract.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Wanvi.Services.Services
{
    public class S3Service: IS3Service
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName;
        private readonly string _region;
        private readonly IConfiguration _configuration; // Inject IConfiguration
        private readonly ILogger<S3Service> _logger;  // Add the logger field

        public S3Service(IAmazonS3 s3Client, IConfiguration configuration, ILogger<S3Service> logger)
        {
            _s3Client = s3Client;
            _configuration = configuration;
            _bucketName = _configuration.GetSection("AWS")["BucketName"]; // Lấy bucketName từ IConfiguration
            _logger = logger;
        }

        public async Task<string> UploadFileAsync(IFormFile file, string prefix = "")
        {
            if (file == null || file.Length == 0)
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "File không tồn tại hoặc rỗng."); 
            }

            string extension = Path.GetExtension(file.FileName);
            string fileName = $"{Guid.NewGuid()}{extension}"; // Không có "prefix" chứa "/"

            try
            {
                using (var stream = file.OpenReadStream())
                {
                    var request = new PutObjectRequest
                    {
                        BucketName = _bucketName,
                        Key = fileName.Trim('/'),
                        InputStream = stream,
                        ContentType = file.ContentType,
                        AutoCloseStream = true,
                    };
                    file.OpenReadStream().Position = 0;
                    // Tắt Chunked Encoding (Fix lỗi SHA256)
                    request.Headers["x-amz-content-sha256"] = "UNSIGNED-PAYLOAD";
                    request.UseChunkEncoding = false;
                    var response = await _s3Client.PutObjectAsync(request);

                    if (response.HttpStatusCode != System.Net.HttpStatusCode.OK)
                    {
                        throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, $"Lỗi khi upload file: {response.HttpStatusCode}");
                    }

                    return fileName;
                }
            }
            catch (AmazonS3Exception ex)
            {
                _logger.LogError(ex, "Amazon S3 Error: {Message}, Status Code: {StatusCode}, Error Code: {ErrorCode}, Request ID: {RequestId}",
                   ex.Message, ex.StatusCode, ex.ErrorCode, ex.RequestId);
                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, $"Lỗi Amazon S3: {ex.Message}");
            }
            catch (Exception ex)
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, $"Lỗi: {ex.Message}");
            }
        }

        public async Task<bool> DeleteFileAsync(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, "Tên file không được rỗng.");
            }

            try
            {
                var request = new DeleteObjectRequest
                {
                    BucketName = _bucketName,
                    Key = fileName
                };

                var response = await _s3Client.DeleteObjectAsync(request);
                return response.HttpStatusCode == System.Net.HttpStatusCode.NoContent;
            }
            catch (AmazonS3Exception ex)
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, $"Lỗi Amazon S3: {ex.Message}");
            }
            catch (Exception ex)
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BadRequest, $"Lỗi: {ex.Message}");
            }
        }

    }
}