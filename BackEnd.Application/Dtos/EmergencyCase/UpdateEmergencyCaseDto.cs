using BackEnd.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackEnd.Application.Dtos.EmergencyCase
{
    public class UpdateEmergencyCaseDto
    {
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public UrgencyLevel UrgencyLevel { get; set; }

        public decimal? RequiredAmount { get; set; }

        public string? ImageUrl { get; set; }

        public bool IsActive { get; set; }
    }
}
