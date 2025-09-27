using ArtisanHubs.Data.Entities;
using ArtisanHubs.DTOs.DTO.Reponse.ArtistProfile;
using ArtisanHubs.DTOs.DTO.Reponse.Categories;
using ArtisanHubs.DTOs.DTO.Reponse.WorkshopPackages;
using ArtisanHubs.DTOs.DTO.Request.ArtistProfile;
using ArtisanHubs.DTOs.DTO.Request.Categories;
using ArtisanHubs.DTOs.DTO.Request.WorkshopPackages;
using ArtisanHubs.DTOs.DTOs.Reponse;
using ArtisanHubs.DTOs.DTOs.Request.Accounts;
using AutoMapper;

namespace ArtisanHubs.Bussiness.Mapping
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // Account -> AccountResponse
            CreateMap<Account, AccountResponse>();

            // AccountRequest -> Account
            CreateMap<AccountRequest, Account>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => "Active")) // default
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore()); ;

            CreateMap<Artistprofile, ArtistProfileResponse>();
            CreateMap<ArtistProfileRequest, Artistprofile>();

            CreateMap<Workshoppackage, WorkshopPackageResponse>();
            CreateMap<WorkshopPackageRequest, Workshoppackage>();

            CreateMap<CreateCategoryRequest, Category>();
            CreateMap<UpdateCategoryRequest, Category>();
            CreateMap<Category, CategoryResponse>();
        }
    }
}   