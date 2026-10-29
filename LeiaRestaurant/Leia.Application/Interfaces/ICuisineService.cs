using Leia.Application.Dtos.CuisineDtos;

namespace Leia.Application.Interfaces
{
    public interface ICuisineService
    {
        Task<List<ResponseCuisineDto>> GetActiveAsync();
        Task<List<ResponseCuisineDto>> GetAllAsync();
        Task<ResponseCuisineDto> CreateAsync(CreateCuisineDto dto);
        Task<ResponseCuisineDto> UpdateAsync(Guid id, UpdateCuisineDto dto);
        Task DeleteAsync(Guid id);
    }
}