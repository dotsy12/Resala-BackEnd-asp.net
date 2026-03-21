using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackEnd.Application.Dtos.Sponsorship
{
    public class UpdateSponsorshipDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public decimal? TargetAmount { get; set; }
        public bool IsActive { get; set; }
    }
}
