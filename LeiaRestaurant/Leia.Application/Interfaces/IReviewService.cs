using Leia.Application.Dtos.Common;
using Leia.Application.Dtos.ReviewDtos;

namespace Leia.Application.Interfaces
{
    public interface IReviewService
    {
        Task<ResponseReviewDto> CreateAsync(Guid clientId, CreateReviewDto dto);
        Task<PagedResultDto<ResponseReviewDto>> GetByRestaurantAsync(Guid restaurantId, PaginationDto dto);
        Task<ResponseReviewDto> ReplyAsync(Guid ownerId, Guid reviewId, CreateReviewReplyDto dto);
        Task DeleteReplyAsync(Guid ownerId, Guid reviewId);
    }
}