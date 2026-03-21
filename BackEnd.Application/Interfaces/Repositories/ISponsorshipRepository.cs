using BackEnd.Domain.Entities.Sponsorship;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackEnd.Application.Interfaces.Repositories
{

    public interface ISponsorshipRepository
    {
        Task<Sponsorship?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Sponsorship>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Sponsorship> CreateAsync(Sponsorship sponsorship, CancellationToken cancellationToken = default);
        Task UpdateAsync(Sponsorship sponsorship, CancellationToken cancellationToken = default);
        Task DeleteAsync(Sponsorship sponsorship, CancellationToken cancellationToken = default);
    }
}
