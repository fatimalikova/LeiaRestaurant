using AutoMapper;
using Leia.Application.Dtos.CuisineDtos;
using Leia.Application.Exceptions;
using Leia.Application.Interfaces;
using Leia.Domain.Entities;
using Leia.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Leia.Infrastructure.Services
{
    public class CuisineService(AppDbContext db, IMapper mapper) : ICuisineService
    {
        public async Task<List<ResponseCuisineDto>> GetActiveAsync()
        {
            var cuisines = await db.Cuisines.AsNoTracking()
                .Where(c => c.IsActive)
                .OrderBy(c => c.SortOrder).ThenBy(c => c.Name)
                .ToListAsync();

            return mapper.Map<List<ResponseCuisineDto>>(cuisines);
        }

        public async Task<List<ResponseCuisineDto>> GetAllAsync()
        {
            var cuisines = await db.Cuisines.AsNoTracking()
                .OrderBy(c => c.SortOrder).ThenBy(c => c.Name)
                .ToListAsync();

            return mapper.Map<List<ResponseCuisineDto>>(cuisines);
        }

        public async Task<ResponseCuisineDto> CreateAsync(CreateCuisineDto dto)
        {
            var name = dto.Name.Trim();

            if (await db.Cuisines.AnyAsync(c => c.Name == name))
                throw new ConflictException("A cuisine with this name already exists.");

            var cuisine = mapper.Map<Cuisine>(dto);
            cuisine.Name = name;

            db.Cuisines.Add(cuisine);
            await db.SaveChangesAsync();
            return mapper.Map<ResponseCuisineDto>(cuisine);
        }

        public async Task<ResponseCuisineDto> UpdateAsync(Guid id, UpdateCuisineDto dto)
        {
            var cuisine = await db.Cuisines.FirstOrDefaultAsync(c => c.Id == id)
                ?? throw new NotFoundException("Cuisine not found.");

            var name = dto.Name.Trim();

            if (await db.Cuisines.AnyAsync(c => c.Name == name && c.Id != id))
                throw new ConflictException("A cuisine with this name already exists.");

            mapper.Map(dto, cuisine);
            cuisine.Name = name;
            cuisine.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();
            return mapper.Map<ResponseCuisineDto>(cuisine);
        }

        public async Task DeleteAsync(Guid id)
        {
            var cuisine = await db.Cuisines.FirstOrDefaultAsync(c => c.Id == id)
                ?? throw new NotFoundException("Cuisine not found.");

            db.Cuisines.Remove(cuisine);
            await db.SaveChangesAsync();
        }
    }
}   