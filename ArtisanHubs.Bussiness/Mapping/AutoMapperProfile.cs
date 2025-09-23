using ArtisanHubs.Data.Entities;
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
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow));
        }
    }
}