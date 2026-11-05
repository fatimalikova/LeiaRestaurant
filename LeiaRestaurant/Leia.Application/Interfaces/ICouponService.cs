using Leia.Application.Dtos.CouponDtos;

namespace Leia.Application.Interfaces
{
    public interface ICouponService
    {
        Task<List<ResponseCouponDto>> GetMyAsync(Guid ownerId);
        Task<List<ResponseCouponDto>> GetAllForAdminAsync();
        Task<ResponseCouponDto> CreateAsync(Guid userId, bool isAdmin, CreateCouponDto dto);
        Task<ResponseCouponDto> UpdateAsync(Guid userId, bool isAdmin, Guid id, UpdateCouponDto dto);
        Task DeleteAsync(Guid userId, bool isAdmin, Guid id);
        Task<AppliedCouponDto> EvaluateAsync(Guid customerId, Guid restaurantId, string code, decimal subtotal);
        Task ReleaseForOrderAsync(Guid orderId);
    }
}