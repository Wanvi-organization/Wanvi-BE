using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Wanvi.Contract.Repositories.Entities;
using Wanvi.Contract.Repositories.IUOW;
using Wanvi.Contract.Services.Interfaces;
using Wanvi.Core.Bases;
using Wanvi.Core.Constants;
using Wanvi.Core.Utils;
using Wanvi.ModelViews.MediaModelViews;
using Wanvi.Services.Configurations;
using Wanvi.Services.Services.Infrastructure;


namespace Wanvi.Services.Services
{
    public class MediaService : IMediaService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly UploadSettings _uploadSettings;
        private readonly IHttpContextAccessor _contextAccessor;

        public MediaService(IUnitOfWork unitOfWork, IMapper mapper, IOptions<UploadSettings> uploadSettings, IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _uploadSettings = uploadSettings.Value;
            _contextAccessor = httpContextAccessor;
        }

        public async Task<IEnumerable<ResponseMediaModel>> UploadAsync(UploadMediaModel model)
        {
            string userId = Authentication.GetUserIdFromHttpContextAccessor(_contextAccessor);
            model.TrimAllStrings();
            var uploadedMedia = new List<ResponseMediaModel>();
            string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), _uploadSettings.UploadPath);

            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            foreach (var file in model.Files)
            {
                if (file.Length > 0)
                {
                    string fileExtension = Path.GetExtension(file.FileName).ToLower();
                    MediaType mediaType = (fileExtension == ".mp4" || fileExtension == ".avi" || fileExtension == ".mov")
                        ? MediaType.Video
                        : MediaType.Image;

                    string fileName = Guid.NewGuid().ToString() + fileExtension;
                    string filePath = Path.Combine(uploadPath, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    string fileUrl = $"/uploads/{fileName}";
                    var media = new Media
                    {
                        Url = fileUrl,
                        Type = mediaType,
                        AltText = file.FileName,
                        CreatedBy = userId,
                        CreatedTime = CoreHelper.SystemTimeNow,
                        LastUpdatedBy = userId,
                        LastUpdatedTime = CoreHelper.SystemTimeNow
                    };

                    await _unitOfWork.GetRepository<Media>().InsertAsync(media);
                    uploadedMedia.Add(_mapper.Map<ResponseMediaModel>(media));
                }
            }
            await _unitOfWork.SaveAsync();
            return uploadedMedia;
        }

        public async Task DeleteAsync(string id)
        {
            string userId = Authentication.GetUserIdFromHttpContextAccessor(_contextAccessor);

            var mediaRepo = _unitOfWork.GetRepository<Media>();
            var media = await mediaRepo.GetByIdAsync(id.Trim())
                ?? throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "Ảnh không tồn tại.");

            if (media.DeletedTime.HasValue)
            {
                throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "Ảnh đã bị xóa.");
            }

            string filePath = Path.Combine(Directory.GetCurrentDirectory(), _uploadSettings.UploadPath, Path.GetFileName(media.Url));

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            media.Url = "Đã xóa";
            media.LastUpdatedTime = CoreHelper.SystemTimeNow;
            media.LastUpdatedBy = userId;
            media.DeletedTime = CoreHelper.SystemTimeNow;
            media.DeletedBy = userId;

            await mediaRepo.UpdateAsync(media);
            await _unitOfWork.SaveAsync();
        }
    }
}
