using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MediatR;

namespace BackEnd.Domain.Interfaces
{
    public interface IDomainEvent : INotification
    {
    }
}
