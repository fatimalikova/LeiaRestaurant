using Leia.Application.Dtos.AddressDtos;

namespace Leia.Application.Interfaces
{
    public interface IAddressService
    {
        Task<List<ResponseAddressDto>> GetMyAsync(Guid userId);
        Task<ResponseAddressDto> CreateAsync(Guid userId, CreateAddressDto dto);
        Task<ResponseAddressDto> UpdateAsync(Guid userId, Guid id, UpdateAddressDto dto);
        Task<ResponseAddressDto> SetDefaultAsync(Guid userId, Guid id);
        Task DeleteAsync(Guid userId, Guid id);
    }
}