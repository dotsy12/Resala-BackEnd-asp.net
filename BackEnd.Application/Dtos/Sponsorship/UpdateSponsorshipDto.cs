using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;

namespace BackEnd.Application.Dtos.Sponsorship
{
    public class UpdateSponsorshipDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public IFormFile? IconFile { get; set; }
        public decimal? TargetAmount { get; set; }
        public bool IsActive { get; set; }
        public IFormFile? ImageFile { get; set; }
    }
}
