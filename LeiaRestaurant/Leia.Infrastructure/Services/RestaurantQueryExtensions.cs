using Leia.Application.Dtos.RestaurantDtos;
using Leia.Application.Helpers;
using Leia.Domain.Entities;

namespace Leia.Infrastructure.Services
{
    internal static class RestaurantQueryExtensions
    {
        public static IQueryable<RestaurantListDto> ProjectToList(this IQueryable<Restaurant> query)
        {
            var local = TimeHelper.NowLocal();
            var today = local.Date;
            var day = local.DayOfWeek;
            var time = local.TimeOfDay;

            return query.Select(r => new RestaurantListDto
            {
                Id = r.Id,
                Name = r.Name,
                LogoUrl = r.LogoUrl,
                CoverUrl = r.CoverUrl,
                Address = r.Address,
                AcceptsDelivery = r.AcceptsDelivery,
                AcceptsPickup = r.AcceptsPickup,
                MinOrderAmount = r.MinOrderAmount,
                DeliveryFee = r.DeliveryFee,
                AverageRating = r.Reviews.Average(x => (double?)x.Rating) ?? 0,
                ReviewCount = r.Reviews.Count(),
                IsOpenNow =
                    r.WorkingHours.Any(w => w.Day == day && w.StartTime <= time && w.EndTime > time) &&
                    !r.UnavailableDays.Any(u => u.Date == today),
                Cuisines = r.Cuisines.Select(c => c.Cuisine.Name).ToList()
            });
        }
    }
}