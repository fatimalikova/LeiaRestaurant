using Leia.Domain.Common;
using Leia.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Leia.Domain.Entities
{
    public class Restaurant : BaseEntity
    {
        public Guid OwnerId { get; set; }
        public ApplicationUser Owner { get; set; } = null!;

        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? LogoUrl { get; set; }
        public string? CoverUrl { get; set; }

        public string Address { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? ContactEmail { get; set; }
        public string? InstagramUrl { get; set; }
        public string? FacebookUrl { get; set; }

        public RestaurantStatus Status { get; set; } = RestaurantStatus.Pending;
        public bool IsActive { get; set; } = true;

        public bool AcceptsDelivery { get; set; } = true;
        public bool AcceptsPickup { get; set; } = true;
        public decimal MinOrderAmount { get; set; }
        public decimal DeliveryFee { get; set; }

        public ICollection<WorkingHour> WorkingHours { get; set; } = new List<WorkingHour>();
        public ICollection<UnavailableDay> UnavailableDays { get; set; } = new List<UnavailableDay>();
        public ICollection<MenuCategory> MenuCategories { get; set; } = new List<MenuCategory>();
        public ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();
        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<RestaurantCuisine> Cuisines { get; set; } = new List<RestaurantCuisine>();
        public ICollection<RestaurantImage> Images { get; set; } = new List<RestaurantImage>();
    }
}
