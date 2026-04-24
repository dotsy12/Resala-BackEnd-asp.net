using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackEnd.Application.ViewModles
{
   
 
        public class EmergencyCaseViewModel
        {
        public string Image { get; set; } = string.Empty;
        public string? ImagePublicId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public decimal TargetAmount { get; set; }
        public decimal ReceivedAmount { get; set; }

        public string? UrgencyLevel { get; set; }
    }
    }
