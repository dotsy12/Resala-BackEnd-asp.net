namespace BackEnd.Application.Dtos.Cart
{
    public class CartItemDto
    {
        public int Id { get; set; }
        public int? SponsorshipId { get; set; }
        public int? EmergencyCaseId { get; set; }
        public string Title { get; set; } = null!;
        public string? ImagePath { get; set; }
    }
}
