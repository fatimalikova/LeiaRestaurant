using AutoMapper;
using Leia.Application.Dtos.AddressDtos;
using Leia.Application.Dtos.AllergenDtos;
using Leia.Application.Dtos.AppUserDtos;
using Leia.Application.Dtos.CouponDtos;
using Leia.Application.Dtos.CuisineDtos;
using Leia.Application.Dtos.MenuCategoryDtos;
using Leia.Application.Dtos.MenuItemDtos;
using Leia.Application.Dtos.ModifierDtos;
using Leia.Application.Dtos.NotificationDtos;
using Leia.Application.Dtos.OrderDtos;
using Leia.Application.Dtos.PaymentDtos;
using Leia.Application.Dtos.ProfileDtos;
using Leia.Application.Dtos.ReservationDtos;
using Leia.Application.Dtos.RestaurantDtos;
using Leia.Application.Dtos.RestaurantImageDtos;
using Leia.Application.Dtos.ReviewDtos;
using Leia.Application.Dtos.SliderDtos;
using Leia.Application.Dtos.SystemSettingDtos;
using Leia.Application.Dtos.TableDtos;
using Leia.Application.Dtos.UnavailableDayDtos;
using Leia.Application.Dtos.UserDtos;
using Leia.Application.Dtos.VariantDtos;
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
                .ForMember(d => d.ReviewCount, opt => opt.Ignore())
                .ForMember(d => d.Cuisines, opt => opt.MapFrom(s => s.Cuisines.Select(c => c.Cuisine)))
                .ForMember(d => d.Images, opt => opt.MapFrom(s => s.Images.OrderBy(i => i.SortOrder)));

            CreateMap<CreateRestaurantDto, Restaurant>();
            CreateMap<UpdateRestaurantDto, Restaurant>();

            CreateMap<Cuisine, ResponseCuisineDto>();
            CreateMap<CreateCuisineDto, Cuisine>();
            CreateMap<UpdateCuisineDto, Cuisine>();

            CreateMap<RestaurantImage, ResponseRestaurantImageDto>();
            CreateMap<AddRestaurantImageDto, RestaurantImage>();

            CreateMap<UserAddress, ResponseAddressDto>();
            CreateMap<CreateAddressDto, UserAddress>();
            CreateMap<UpdateAddressDto, UserAddress>();

            CreateMap<WorkingHour, ResponseWorkingHourDto>();
            CreateMap<CreateWorkingHourDto, WorkingHour>();
            CreateMap<UpdateWorkingHourDto, WorkingHour>();

            CreateMap<UnavailableDay, ResponseUnavailableDayDto>();
            CreateMap<CreateUnavailableDayDto, UnavailableDay>();

            CreateMap<RestaurantTable, ResponseTableDto>();
            CreateMap<CreateTableDto, RestaurantTable>();
            CreateMap<UpdateTableDto, RestaurantTable>();

            CreateMap<Reservation, ResponseReservationDto>()
                .ForMember(d => d.RestaurantName, opt => opt.MapFrom(s => s.Restaurant.Name))
                .ForMember(d => d.CustomerName, opt => opt.MapFrom(s => s.Customer.FullName))
                .ForMember(d => d.TableName, opt => opt.MapFrom(s => s.Table.Name));

            CreateMap<MenuCategory, ResponseMenuCategoryDto>()
                .ForMember(d => d.Items, opt => opt.MapFrom(s => s.MenuItems));
            CreateMap<CreateMenuCategoryDto, MenuCategory>();
            CreateMap<UpdateMenuCategoryDto, MenuCategory>();

            CreateMap<MenuItem, ResponseMenuItemDto>()
                .ForMember(d => d.CategoryName, opt => opt.MapFrom(s => s.Category != null ? s.Category.Name : string.Empty))
                .ForMember(d => d.Variants, opt => opt.MapFrom(s => s.Variants.OrderBy(v => v.SortOrder).ThenBy(v => v.Name)))
                .ForMember(d => d.ModifierGroups, opt => opt.MapFrom(s => s.ModifierGroups.OrderBy(g => g.SortOrder)))
                .ForMember(d => d.Allergens, opt => opt.MapFrom(s => s.Allergens.Select(a => a.Allergen)));
            CreateMap<CreateMenuItemDto, MenuItem>();
            CreateMap<UpdateMenuItemDto, MenuItem>();

            CreateMap<MenuItemVariant, ResponseVariantDto>();
            CreateMap<CreateVariantDto, MenuItemVariant>();
            CreateMap<UpdateVariantDto, MenuItemVariant>();

            CreateMap<ModifierGroup, ResponseModifierGroupDto>()
                .ForMember(d => d.IsRequired, opt => opt.MapFrom(s => s.MinSelect > 0))
                .ForMember(d => d.Options, opt => opt.MapFrom(s => s.Options.OrderBy(o => o.SortOrder)));
            CreateMap<CreateModifierGroupDto, ModifierGroup>();
            CreateMap<UpdateModifierGroupDto, ModifierGroup>();

            CreateMap<ModifierOption, ResponseModifierOptionDto>();
            CreateMap<CreateModifierOptionDto, ModifierOption>();
            CreateMap<UpdateModifierOptionDto, ModifierOption>();

            CreateMap<Allergen, ResponseAllergenDto>();
            CreateMap<CreateAllergenDto, Allergen>();
            CreateMap<UpdateAllergenDto, Allergen>();

            CreateMap<Coupon, ResponseCouponDto>();
            CreateMap<CreateCouponDto, Coupon>();
            CreateMap<UpdateCouponDto, Coupon>();

            CreateMap<Order, ResponseOrderDto>()
                .ForMember(d => d.CustomerName, opt => opt.MapFrom(s => s.Customer.FullName))
                .ForMember(d => d.RestaurantName, opt => opt.MapFrom(s => s.Restaurant.Name))
                .ForMember(d => d.HasReview, opt => opt.MapFrom(s => s.Review != null))
                .ForMember(d => d.Timeline, opt => opt.MapFrom(s => s.History.OrderBy(h => h.CreatedAt)));

            CreateMap<OrderItem, ResponseOrderItemDto>()
                .ForMember(d => d.Name, opt => opt.MapFrom(s => s.NameAtOrder))
                .ForMember(d => d.Price, opt => opt.MapFrom(s => s.PriceAtOrder))
                .ForMember(d => d.LineTotal, opt => opt.MapFrom(s => s.PriceAtOrder * s.Quantity));

            CreateMap<OrderItemModifier, ResponseOrderItemModifierDto>();

            CreateMap<OrderStatusHistory, ResponseOrderStatusHistoryDto>()
                .ForMember(d => d.ChangedAt, opt => opt.MapFrom(s => s.CreatedAt));

            CreateMap<Review, ResponseReviewDto>()
                .ForMember(d => d.ClientName, opt => opt.MapFrom(s => s.Client != null ? s.Client.FullName : string.Empty))
                .ForMember(d => d.ClientImageUrl, opt => opt.MapFrom(s => s.Client != null ? s.Client.ImageUrl : null));
            CreateMap<CreateReviewDto, Review>();

            CreateMap<ReviewReply, ResponseReviewReplyDto>();

            CreateMap<Notification, ResponseNotificationDto>();

            CreateMap<Slider, ResponseSliderDto>();
            CreateMap<CreateSliderDto, Slider>();
            CreateMap<UpdateSliderDto, Slider>();

            CreateMap<SystemSetting, ResponseSystemSettingDto>();
            CreateMap<UpdateSystemSettingDto, SystemSetting>();
            CreateMap<Payment, ResponsePaymentDto>();
        }
    }
}