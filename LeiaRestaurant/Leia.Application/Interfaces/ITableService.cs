using Leia.Application.Dtos.TableDtos;

namespace Leia.Application.Interfaces
{
    public interface ITableService
    {
        Task<List<ResponseTableDto>> GetMyAsync(Guid ownerId);
        Task<ResponseTableDto> CreateAsync(Guid ownerId, CreateTableDto dto);
        Task<ResponseTableDto> UpdateAsync(Guid ownerId, Guid id, UpdateTableDto dto);
        Task DeleteAsync(Guid ownerId, Guid id);
    }
}