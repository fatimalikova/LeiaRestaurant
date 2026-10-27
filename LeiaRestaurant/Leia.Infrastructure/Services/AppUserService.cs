using AutoMapper;
using Leia.Application.Dtos.AppUserDtos;
using Leia.Application.Dtos.Common;
using Leia.Application.Exceptions;
using Leia.Application.Interfaces;
using Leia.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Leia.Infrastructure.Services
{
    public class AppUserService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole<Guid>> roleManager,
        IMapper mapper) : IAppUserService
    {
        public async Task<PagedResultDto<ResponseUserDto>> GetAllAsync(PaginationDto dto)
        {
            var query = userManager.Users.AsNoTracking();

            var total = await query.CountAsync();
            var users = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((dto.Page - 1) * dto.PageSize)
                .Take(dto.PageSize)
                .ToListAsync();

            var items = new List<ResponseUserDto>();
            foreach (var u in users)
                items.Add(await ToDtoAsync(u));

            return new PagedResultDto<ResponseUserDto>
            {
                Items = items,
                TotalCount = total,
                Page = dto.Page,
                PageSize = dto.PageSize
            };
        }

        public async Task<ResponseUserDto> GetByIdAsync(Guid id)
        {
            return await ToDtoAsync(await GetUserAsync(id));
        }

        public async Task<ResponseUserDto> CreateAsync(CreateUserDto dto)
        {
            if (await userManager.FindByEmailAsync(dto.Email) != null)
                throw new ConflictException("This email is already registered.");

            var user = mapper.Map<ApplicationUser>(dto);

            var result = await userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                throw new BadRequestException(string.Join(" ", result.Errors.Select(e => e.Description)));

            if (!await roleManager.RoleExistsAsync(dto.Role))
                await roleManager.CreateAsync(new IdentityRole<Guid>(dto.Role));

            await userManager.AddToRoleAsync(user, dto.Role);

            return await ToDtoAsync(user);
        }

        public async Task<ResponseUserDto> UpdateAsync(Guid id, UpdateUserDto dto)
        {
            var user = await GetUserAsync(id);
            mapper.Map(dto, user);

            var result = await userManager.UpdateAsync(user);
            if (!result.Succeeded)
                throw new BadRequestException(string.Join(" ", result.Errors.Select(e => e.Description)));

            return await ToDtoAsync(user);
        }

        private async Task<ApplicationUser> GetUserAsync(Guid id)
        {
            return await userManager.FindByIdAsync(id.ToString())
                ?? throw new NotFoundException("User not found.");
        }

        private async Task<ResponseUserDto> ToDtoAsync(ApplicationUser user)
        {
            var dto = mapper.Map<ResponseUserDto>(user);
            dto.Roles = await userManager.GetRolesAsync(user);
            return dto;
        }
    }
}