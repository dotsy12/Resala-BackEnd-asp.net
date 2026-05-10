namespace BackEnd.Domain.Entities.Basket;

public class DonorBasket
{
    public string Id { get; set; } = string.Empty;
    public List<BasketItem> Items { get; set; } = new();
}
