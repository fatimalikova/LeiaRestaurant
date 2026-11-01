using AutoMapper;
using Leia.Application.Dtos.AllergenDtos;
using Leia.Application.Exceptions;
using Leia.Application.Interfaces;
using Leia.Domain.Entities;
using Leia.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Leia.Infrastructure.Services
{
    public class AllergenService(AppDbContext db, IMapper mapper) : IAllergenService
    {
        public async Task<List<ResponseAllergenDto>> GetActiveAsync()
        {
            var allergens = await db.Allergens.AsNoTracking()
                .Where(a => a.IsActive)
                .OrderBy(a => a.Name)
                .ToListAsync();

            return mapper.Map<List<ResponseAllergenDto>>(allergens);
        }

        public async Task<List<ResponseAllergenDto>> GetAllAsync()
        {
            var allergens = await db.Allergens.AsNoTracking()
                .OrderBy(a => a.Name)
                .ToListAsync();

            return mapper.Map<List<ResponseAllergenDto>>(allergens);
        }

        public async Task<ResponseAllergenDto> CreateAsync(CreateAllergenDto dto)
        {
            var name = dto.Name.Trim();

            if (await db.Allergens.AnyAsync(a => a.Name == name))
                throw new ConflictException("An allergen with this name already exists.");

            var allergen = mapper.Map<Allergen>(dto);
            allergen.Name = name;

            db.Allergens.Add(allergen);
            await db.SaveChangesAsync();
            return mapper.Map<ResponseAllergenDto>(allergen);
        }

        public async Task<ResponseAllergenDto> UpdateAsync(Guid id, UpdateAllergenDto dto)
        {
            var allergen = await db.Allergens.FirstOrDefaultAsync(a => a.Id == id)
                ?? throw new NotFoundException("Allergen not found.");

            var name = dto.Name.Trim();

            if (await db.Allergens.AnyAsync(a => a.Name == name && a.Id != id))
                throw new ConflictException("An allergen with this name already exists.");

            mapper.Map(dto, allergen);
            allergen.Name = name;
            allergen.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();
            return mapper.Map<ResponseAllergenDto>(allergen);
        }

        public async Task DeleteAsync(Guid id)
        {
            var allergen = await db.Allergens.FirstOrDefaultAsync(a => a.Id == id)
                ?? throw new NotFoundException("Allergen not found.");

            db.Allergens.Remove(allergen);
            await db.SaveChangesAsync();
        }
    }
}