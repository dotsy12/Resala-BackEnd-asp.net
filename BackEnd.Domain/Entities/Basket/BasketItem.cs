namespace BackEnd.Domain.Entities.Basket;

public class BasketItem
{
    public Guid Id { get; set; }
    public int TargetId { get; set; }
    public string ItemType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
}
