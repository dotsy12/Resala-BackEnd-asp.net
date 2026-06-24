namespace BackEnd.Application.Dtos.Donor
{
    public record DonorStatisticsDto(
        int TotalSponsorships,
        int ActiveSponsorships,
        int ExpiredSponsorships,
        decimal TotalMonthlyAmount,
        decimal TotalPaidAmount
    );
}
