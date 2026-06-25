using BackEnd.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace BackEnd.Application.Dtos.Cart
{
    public class CheckoutCartDto
    {
        public PaymentMethod PaymentMethod { get; set; }

        // VodafoneCash / InstaPay
        public IFormFile? ReceiptImage { get; set; }
        public string? SenderPhoneNumber { get; set; }

        // Representative
        public int? DeliveryAreaId { get; set; }
        public string? ContactName { get; set; }
        public string? ContactPhone { get; set; }
        public string? Address { get; set; }
        public string? RepresentativeNotes { get; set; }

        // Branch
        public int? SlotId { get; set; }
        public string? DonorName { get; set; }
        public string? BranchContactPhone { get; set; }
    }
}
