using BackEnd.Domain.Entities.Basket;

namespace BackEnd.Application.Interfaces.Repositories
{
    public interface IBasketRepository
    {
        Task<DonorBasket?> GetBasketAsync(string donorId);
        Task UpdateBasketAsync(DonorBasket basket);
        Task DeleteBasketAsync(string donorId);
    }
}
