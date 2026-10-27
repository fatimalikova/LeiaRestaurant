using AutoMapper;
using Leia.Application.Dtos.AppUserDtos;
using Leia.Application.Dtos.MenuCategoryDtos;
using Leia.Application.Dtos.MenuItemDtos;
using Leia.Application.Dtos.NotificationDtos;
using Leia.Application.Dtos.OrderDtos;
using Leia.Application.Dtos.ProfileDtos;
using Leia.Application.Dtos.RestaurantDtos;
using Leia.Application.Dtos.ReviewDtos;
using Leia.Application.Dtos.SliderDtos;
using Leia.Application.Dtos.SystemSettingDtos;
using Leia.Application.Dtos.UnavailableDayDtos;
using Leia.Application.Dtos.UserDtos;
using Leia.Application.Dtos.WorkingHourDtos;
using Leia.Domain.Entities;

namespace Leia.Application.Profiles
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            CreateMap<RegisterDto, ApplicationUser>()
                .ForMember(d => d.UserName, opt => opt.MapFrom(s => s.Email));

            CreateMap<CreateUserDto, ApplicationUser>()
                .ForMember(d => d.UserName, opt => opt.MapFrom(s => s.Email))
                .ForMember(d => d.EmailConfirmed, opt => opt.MapFrom(s => true));

            CreateMap<UpdateUserDto, ApplicationUser>();

            CreateMap<ApplicationUser, ProfileResponseDto>();
            CreateMap<UpdateProfileDto, ApplicationUser>();

            CreateMap<ApplicationUser, ResponseUserDto>()
                .ForMember(d => d.Roles, opt => opt.Ignore());

            CreateMap<Restaurant, ResponseRestaurantDto>()
                .ForMember(d => d.AverageRating, opt => opt.Ignore())
                .ForMember(d => d.ReviewCount, opt => opt.Ignore());

            CreateMap<CreateRestaurantDto, Restaurant>();
            CreateMap<UpdateRestaurantDto, Restaurant>();

            CreateMap<WorkingHour, ResponseWorkingHourDto>();
            CreateMap<CreateWorkingHourDto, WorkingHour>();
            CreateMap<UpdateWorkingHourDto, WorkingHour>();

            CreateMap<UnavailableDay, ResponseUnavailableDayDto>();
            CreateMap<CreateUnavailableDayDto, UnavailableDay>();

            CreateMap<MenuCategory, ResponseMenuCategoryDto>()
                .ForMember(d => d.Items, opt => opt.MapFrom(s => s.MenuItems));
            CreateMap<CreateMenuCategoryDto, MenuCategory>();
            CreateMap<UpdateMenuCategoryDto, MenuCategory>();

            CreateMap<MenuItem, ResponseMenuItemDto>()
                .ForMember(d => d.CategoryName, opt => opt.MapFrom(s => s.Category != null ? s.Category.Name : string.Empty));
            CreateMap<CreateMenuItemDto, MenuItem>();
            CreateMap<UpdateMenuItemDto, MenuItem>();

            CreateMap<Order, ResponseOrderDto>()
                .ForMember(d => d.CustomerName, opt => opt.MapFrom(s => s.Customer.FullName))
                .ForMember(d => d.RestaurantName, opt => opt.MapFrom(s => s.Restaurant.Name))
                .ForMember(d => d.HasReview, opt => opt.MapFrom(s => s.Review != null));

            CreateMap<OrderItem, ResponseOrderItemDto>()
                .ForMember(d => d.Name, opt => opt.MapFrom(s => s.NameAtOrder))
                .ForMember(d => d.Price, opt => opt.MapFrom(s => s.PriceAtOrder))
                .ForMember(d => d.LineTotal, opt => opt.MapFrom(s => s.PriceAtOrder * s.Quantity));

            CreateMap<Review, ResponseReviewDto>()
                .ForMember(d => d.ClientName, opt => opt.MapFrom(s => s.Client != null ? s.Client.FullName : string.Empty))
                .ForMember(d => d.ClientImageUrl, opt => opt.MapFrom(s => s.Client != null ? s.Client.ImageUrl : null));
            CreateMap<CreateReviewDto, Review>();

            CreateMap<Notification, ResponseNotificationDto>();

            CreateMap<Slider, ResponseSliderDto>();
            CreateMap<CreateSliderDto, Slider>();
            CreateMap<UpdateSliderDto, Slider>();

            CreateMap<SystemSetting, ResponseSystemSettingDto>();
            CreateMap<UpdateSystemSettingDto, SystemSetting>();
        }
    }
}