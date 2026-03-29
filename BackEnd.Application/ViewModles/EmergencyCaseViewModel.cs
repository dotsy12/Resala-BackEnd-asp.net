using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackEnd.Application.ViewModles
{
   
 
        public class EmergencyCaseViewModel
        {
            public int Id { get; set; }

            public string Title { get; set; } = string.Empty;

            public string Description { get; set; } = string.Empty;

            public string ImageUrl { get; set; } = string.Empty;

            public string UrgencyLevel { get; set; } = string.Empty;

            public decimal RequiredAmount { get; set; }

            public decimal CollectedAmount { get; set; }

            public bool IsActive { get; set; }

            public bool IsCompleted { get; set; }

            public DateTime CreatedAt { get; set; }
        }
    }
