using Leia.Application.Dtos.SliderDtos;
using Leia.Application.Exceptions;
using Leia.Application.Interfaces;
using Leia.Domain.Entities;
using Leia.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Leia.Infrastructure.Services
{
    public class SliderService(AppDbContext db) : ISliderService
    {
        public async Task<List<ResponseSliderDto>> GetActiveAsync()
        {
            var sliders = await db.Sliders.AsNoTracking()
                .Where(s => s.IsActive)
                .OrderBy(s => s.SortOrder)
                .ToListAsync();
            return sliders.Select(Map).ToList();
        }

        public async Task<List<ResponseSliderDto>> GetAllAsync()
        {
            var sliders = await db.Sliders.AsNoTracking().OrderBy(s => s.SortOrder).ToListAsync();
            return sliders.Select(Map).ToList();
        }

        public async Task<ResponseSliderDto> CreateAsync(CreateSliderDto dto)
        {
            var slider = new Slider
            {
                ImageUrl = dto.ImageUrl,
                Title = dto.Title,
                Subtitle = dto.Subtitle,
                Link = dto.Link,
                SortOrder = dto.SortOrder
            };

            db.Sliders.Add(slider);
            await db.SaveChangesAsync();
            return Map(slider);
        }

        public async Task<ResponseSliderDto> UpdateAsync(Guid id, UpdateSliderDto dto)
        {
            var slider = await db.Sliders.FirstOrDefaultAsync(s => s.Id == id)
                ?? throw new NotFoundException("Slider not found.");

            slider.ImageUrl = dto.ImageUrl;
            slider.Title = dto.Title;
            slider.Subtitle = dto.Subtitle;
            slider.Link = dto.Link;
            slider.SortOrder = dto.SortOrder;
            slider.IsActive = dto.IsActive;
            slider.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();
            return Map(slider);
        }

        public async Task DeleteAsync(Guid id)
        {
            var slider = await db.Sliders.FirstOrDefaultAsync(s => s.Id == id)
                ?? throw new NotFoundException("Slider not found.");

            db.Sliders.Remove(slider);
            await db.SaveChangesAsync();
        }

        private static ResponseSliderDto Map(Slider s) => new()
        {
            Id = s.Id,
            ImageUrl = s.ImageUrl,
            Title = s.Title,
            Subtitle = s.Subtitle,
            Link = s.Link,
            SortOrder = s.SortOrder,
            IsActive = s.IsActive
        };
    }
}