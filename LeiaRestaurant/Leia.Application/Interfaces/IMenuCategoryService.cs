using Leia.Application.Dtos.MenuCategoryDtos;
using Leia.Application.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Leia.Application.Interfaces
{
    public interface IMenuCategoryService
    {
        Task<ResponseModel<List<ResponseMenuCategoryDto>>> GetByRestaurantAsync(Guid restaurantId);
        Task<ResponseModel<ResponseMenuCategoryDto>> CreateAsync(Guid ownerId, CreateMenuCategoryDto dto);
        Task<ResponseModel<ResponseMenuCategoryDto>> UpdateAsync(Guid ownerId, Guid id, UpdateMenuCategoryDto dto);
        Task<ResponseModel<bool>> DeleteAsync(Guid ownerId, Guid id);
    }
}
