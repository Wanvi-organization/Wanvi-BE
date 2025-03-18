using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Wanvi.Contract.Repositories.Entities;
using Wanvi.Contract.Repositories.IUOW;
using Wanvi.Contract.Services.Interfaces;
using Wanvi.Core.Bases;
using Wanvi.Core.Constants;
using Wanvi.ModelViews.RoleModelViews;

namespace Wanvi.Services.Services
{
    public class RoleService : IRoleService
    {
        private readonly IUnitOfWork _unitOfWork;

        public RoleService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task CreateAsync(CreateRoleModel model)
        {
            var existingRole = await _unitOfWork.GetRepository<ApplicationRole>()
                .Entities.FirstOrDefaultAsync(r => r.Name == model.Name);

            if (existingRole != null)
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ResponseCodeConstants.BADREQUEST, "Vai trò đã tồn tại!");
            }

            var newRole = new ApplicationRole
            {
                Id = Guid.NewGuid(),
                Name = model.Name
            };

            await _unitOfWork.GetRepository<ApplicationRole>().InsertAsync(newRole);
            await _unitOfWork.SaveAsync();
        }

        public async Task<List<ResponseRoleModel>> GetAllAsync()
        {
            var roles = await _unitOfWork.GetRepository<ApplicationRole>().Entities
                .Select(r => new ResponseRoleModel
                {
                    Id = r.Id,
                    Name = r.Name
                })
                .ToListAsync();

            return roles;
        }

        public async Task UpdateAsync(Guid roleId, UpdateRoleModel model)
        {
            var roleRepo = _unitOfWork.GetRepository<ApplicationRole>();

            var existingRole = await roleRepo.Entities.FirstOrDefaultAsync(r => r.Id == roleId)
                ?? throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "Vai trò không tồn tại!");

            var roleWithSameName = await roleRepo.Entities.FirstOrDefaultAsync(r => r.Name == model.Name && r.Id != roleId);
            if (roleWithSameName != null)
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ResponseCodeConstants.BADREQUEST, "Tên vai trò đã tồn tại!");
            }

            existingRole.Name = model.Name;

            roleRepo.Update(existingRole);
            await _unitOfWork.SaveAsync();
        }

        public async Task DeleteAsync(Guid roleId)
        {
            var roleRepo = _unitOfWork.GetRepository<ApplicationRole>();

            var role = await roleRepo.Entities.FirstOrDefaultAsync(r => r.Id == roleId)
                ?? throw new ErrorException(StatusCodes.Status404NotFound, ResponseCodeConstants.NOT_FOUND, "Vai trò không tồn tại!");

            roleRepo.Delete(role);
            await _unitOfWork.SaveAsync();
        }
    }
}
