using ArtisanHubs.Data.Entities;
using ArtisanHubs.DTOs.DTO.Reponse.ArtistProfile;
using ArtisanHubs.DTOs.DTO.Reponse.Carts;
using ArtisanHubs.DTOs.DTO.Reponse.Categories;
using ArtisanHubs.DTOs.DTO.Reponse.Forums;
using ArtisanHubs.DTOs.DTO.Reponse.Order;
using ArtisanHubs.DTOs.DTO.Reponse.Products;
using ArtisanHubs.DTOs.DTO.Reponse.WorkshopPackages;
using ArtisanHubs.DTOs.DTO.Request.ArtistProfile;
using ArtisanHubs.DTOs.DTO.Request.Categories;
using ArtisanHubs.DTOs.DTO.Request.Forums;
using ArtisanHubs.DTOs.DTO.Request.Products;
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




            CreateMap<Achievement, AchievementResponse>();
            CreateMap<Artistprofile, ArtistProfileResponse>();
            CreateMap<ArtistProfileRequest, Artistprofile>()
            .ForMember(dest => dest.Achievements, opt => opt.Ignore()) // Bỏ qua mapping tự động cho Achievements
            .AfterMap((src, dest) => {
                // Xử lý Achievements thủ công
                // Xóa các achievement cũ
                dest.Achievements.Clear();
                // Thêm các achievement mới từ request
                foreach (var achievementDesc in src.Achievements)
                {
                    dest.Achievements.Add(new Achievement { Description = achievementDesc });
                }
            });




            CreateMap<Workshoppackage, WorkshopPackageResponse>();
            CreateMap<WorkshopPackageRequest, Workshoppackage>();

            CreateMap<CreateCategoryRequest, Category>();
            CreateMap<UpdateCategoryRequest, Category>();
            CreateMap<Category, CategoryResponse>();

            //CreateMap<Product, ProductForCustomerResponse>()
            //.ForMember(dest => dest.CategoryName,
            //           opt => opt.MapFrom(src => src.Category != null ? src.Category.Description : null))
            //.ForMember(dest => dest.ArtistName,
            //           opt => opt.MapFrom(src => src.Artist != null ? src.Artist.ArtistName : null));
            CreateMap<CreateProductRequest, Product>();
            // Trong AutoMapper Profile
            CreateMap<Product, ProductSummaryResponse>()
                .ForMember(dest => dest.FavoriteCount, opt => opt.MapFrom(src => src.FavoriteProducts.Count()));
            CreateMap<UpdateProductRequest, Product>();
            CreateMap<Product, ProductResponse>()
               .ForMember(dest => dest.CategoryName,
                          opt => opt.MapFrom(src => src.Category != null ? src.Category.Description : null));

            CreateMap<Product, ProductDetailResponse>()
            .ForMember(dest => dest.CategoryName,
                opt => opt.MapFrom(src => src.Category != null ? src.Category.Description : null))
            .ForMember(dest => dest.ArtistName,
                opt => opt.MapFrom(src => src.Artist != null ? src.Artist.ArtistName : null))
            .ForMember(dest => dest.AverageRating,
                opt => opt.MapFrom(src => src.Feedbacks.Any() ? src.Feedbacks.Average(f => f.Rating) : (double?)null))
            .ForMember(dest => dest.FavoriteCount,
                opt => opt.MapFrom(src => src.FavoriteProducts.Count));

            CreateMap<Cart, CartResponse>()
    .ForMember(dest => dest.CartId, opt => opt.MapFrom(src => src.Id))
    .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.CartItems));

            CreateMap<CartItem, CartItemResponse>()
                .ForMember(dest => dest.CartItemId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Product.Price))
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.Product.Images));


            CreateMap<CreateForumThreadRequest, ForumThread>();
            CreateMap<CreateForumTopicRequest, ForumTopic>();
            CreateMap<Account, AuthorResponse>();
            CreateMap<UpdateForumThreadRequest, ForumThread>();
            CreateMap<ForumPost, ForumPostResponse>()
            .ForMember(dest => dest.Author, opt => opt.MapFrom(src => src.Author));
            CreateMap<ForumThread, ForumThreadResponse>()
           .ForMember(dest => dest.Author, opt => opt.MapFrom(src => src.Author))
           .ForMember(dest => dest.Posts, opt => opt.MapFrom(src => src.Posts));
            CreateMap<ForumTopic, ForumTopicResponse>();
            CreateMap<CreateForumPostRequest, ForumPost>();

            CreateMap<Order, OrderResponse>()
                .ForMember(dest => dest.AccountUsername, opt => opt.MapFrom(src => src.Account != null ? src.Account.Username : null))
                .ForMember(dest => dest.TotalItems, opt => opt.MapFrom(src => src.Orderdetails.Sum(od => od.Quantity)));
        }
    }
}   