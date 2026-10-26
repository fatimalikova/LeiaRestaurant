using Leia.Application.Dtos.AppUserDtos;
using Leia.Application.Dtos.Common;

namespace Leia.Application.Interfaces
{
    public interface IAppUserService
    {
        Task<PagedResultDto<ResponseUserDto>> GetAllAsync(PaginationDto dto);
        Task<ResponseUserDto> GetByIdAsync(Guid id);
        Task<ResponseUserDto> CreateAsync(CreateUserDto dto);
        Task<ResponseUserDto> UpdateAsync(Guid id, UpdateUserDto dto);
    }
}