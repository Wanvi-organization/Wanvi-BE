using AutoMapper;
using Wanvi.Contract.Repositories.Entities;
using Wanvi.ModelViews.CategoryViewModels;

namespace Wanvi.Services.MapperProfile
{
    public class CategoryProfile : Profile
    {
        public CategoryProfile()
        {
            CreateMap<Category, ResponseCategoryModel>().ReverseMap();
            CreateMap<Category, CreateCategoryModel>().ReverseMap();
            CreateMap<Category, UpdateCategoryModel>().ReverseMap();
        }
    }
}
