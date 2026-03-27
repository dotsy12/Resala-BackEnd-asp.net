using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackEnd.Application.Dtos.InKindDonation
{
    public record DonorLookupDto(
         int Id,
         string FullName,
         string PhoneNumber,
         string Email
     );
}
