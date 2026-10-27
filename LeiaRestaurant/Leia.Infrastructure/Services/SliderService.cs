using AutoMapper;
using Leia.Application.Dtos.SliderDtos;
using Leia.Application.Exceptions;
using Leia.Application.Interfaces;
using Leia.Domain.Entities;
using Leia.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Leia.Infrastructure.Services
{
    public class SliderService(AppDbContext db, IMapper mapper) : ISliderService
    {
        public async Task<List<ResponseSliderDto>> GetActiveAsync()
        {
            var sliders = await db.Sliders.AsNoTracking()
                .Where(s => s.IsActive)
                .OrderBy(s => s.SortOrder)
                .ToListAsync();

            return mapper.Map<List<ResponseSliderDto>>(sliders);
        }

        public async Task<List<ResponseSliderDto>> GetAllAsync()
        {
            var sliders = await db.Sliders.AsNoTracking()
                .OrderBy(s => s.SortOrder)
                .ToListAsync();

            return mapper.Map<List<ResponseSliderDto>>(sliders);
        }

        public async Task<ResponseSliderDto> CreateAsync(CreateSliderDto dto)
        {
            var slider = mapper.Map<Slider>(dto);

            db.Sliders.Add(slider);
            await db.SaveChangesAsync();
            return mapper.Map<ResponseSliderDto>(slider);
        }

        public async Task<ResponseSliderDto> UpdateAsync(Guid id, UpdateSliderDto dto)
        {
            var slider = await db.Sliders.FirstOrDefaultAsync(s => s.Id == id)
                ?? throw new NotFoundException("Slider not found.");

            mapper.Map(dto, slider);
            slider.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();
            return mapper.Map<ResponseSliderDto>(slider);
        }

        public async Task DeleteAsync(Guid id)
        {
            var slider = await db.Sliders.FirstOrDefaultAsync(s => s.Id == id)
                ?? throw new NotFoundException("Slider not found.");

            db.Sliders.Remove(slider);
            await db.SaveChangesAsync();
        }
    }
}