using AutoMapper;
using Leia.Application.Dtos.AddressDtos;
using Leia.Application.Exceptions;
using Leia.Application.Interfaces;
using Leia.Domain.Entities;
using Leia.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Leia.Infrastructure.Services
{
    public class AddressService(AppDbContext db, IMapper mapper) : IAddressService
    {
        private const int MaxAddresses = 10;

        public async Task<List<ResponseAddressDto>> GetMyAsync(Guid userId)
        {
            var addresses = await db.UserAddresses.AsNoTracking()
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.IsDefault).ThenByDescending(a => a.CreatedAt)
                .ToListAsync();

            return mapper.Map<List<ResponseAddressDto>>(addresses);
        }

        public async Task<ResponseAddressDto> CreateAsync(Guid userId, CreateAddressDto dto)
        {
            var existing = await db.UserAddresses.Where(a => a.UserId == userId).ToListAsync();

            if (existing.Count >= MaxAddresses)
                throw new BadRequestException($"You can save at most {MaxAddresses} addresses.");

            var address = mapper.Map<UserAddress>(dto);
            address.UserId = userId;

            if (existing.Count == 0)
                address.IsDefault = true;

            if (address.IsDefault)
                foreach (var a in existing)
                    a.IsDefault = false;

            db.UserAddresses.Add(address);
            await db.SaveChangesAsync();
            return mapper.Map<ResponseAddressDto>(address);
        }

        public async Task<ResponseAddressDto> UpdateAsync(Guid userId, Guid id, UpdateAddressDto dto)
        {
            var addresses = await db.UserAddresses.Where(a => a.UserId == userId).ToListAsync();

            var address = addresses.FirstOrDefault(a => a.Id == id)
                ?? throw new NotFoundException("Address not found.");

            var wasDefault = address.IsDefault;
            mapper.Map(dto, address);
            address.UpdatedAt = DateTime.UtcNow;

            if (address.IsDefault)
                foreach (var a in addresses.Where(a => a.Id != id))
                    a.IsDefault = false;
            else if (wasDefault)
                address.IsDefault = true;

            await db.SaveChangesAsync();
            return mapper.Map<ResponseAddressDto>(address);
        }

        public async Task<ResponseAddressDto> SetDefaultAsync(Guid userId, Guid id)
        {
            var addresses = await db.UserAddresses.Where(a => a.UserId == userId).ToListAsync();

            var address = addresses.FirstOrDefault(a => a.Id == id)
                ?? throw new NotFoundException("Address not found.");

            foreach (var a in addresses)
                a.IsDefault = a.Id == id;

            await db.SaveChangesAsync();
            return mapper.Map<ResponseAddressDto>(address);
        }

        public async Task DeleteAsync(Guid userId, Guid id)
        {
            var addresses = await db.UserAddresses.Where(a => a.UserId == userId).ToListAsync();

            var address = addresses.FirstOrDefault(a => a.Id == id)
                ?? throw new NotFoundException("Address not found.");

            db.UserAddresses.Remove(address);

            if (address.IsDefault)
            {
                var next = addresses.Where(a => a.Id != id).OrderByDescending(a => a.CreatedAt).FirstOrDefault();
                if (next != null)
                    next.IsDefault = true;
            }

            await db.SaveChangesAsync();
        }
    }
}