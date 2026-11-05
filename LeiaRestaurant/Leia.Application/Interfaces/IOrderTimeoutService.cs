namespace Leia.Application.Interfaces
{
    public interface IOrderTimeoutService
    {
        Task<int> CancelExpiredAsync();
    }
}