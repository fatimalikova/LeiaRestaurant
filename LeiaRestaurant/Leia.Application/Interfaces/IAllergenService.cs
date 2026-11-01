using Leia.Application.Dtos.AllergenDtos;

namespace Leia.Application.Interfaces
{
    public interface IAllergenService
    {
        Task<List<ResponseAllergenDto>> GetActiveAsync();
        Task<List<ResponseAllergenDto>> GetAllAsync();
        Task<ResponseAllergenDto> CreateAsync(CreateAllergenDto dto);
        Task<ResponseAllergenDto> UpdateAsync(Guid id, UpdateAllergenDto dto);
        Task DeleteAsync(Guid id);
    }
}