using Leia.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Leia.Infrastructure.Services
{
    internal static class MenuQueryExtensions
    {
        public static IQueryable<MenuItem> WithOptions(this IQueryable<MenuItem> query) => query
            .Include(i => i.Category)
            .Include(i => i.Variants)
            .Include(i => i.ModifierGroups).ThenInclude(g => g.Options)
            .Include(i => i.Allergens).ThenInclude(a => a.Allergen)
            .AsSplitQuery();
    }
}