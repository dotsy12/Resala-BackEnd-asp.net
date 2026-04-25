using BackEnd.Domain.Entities.EmergencyCase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BackEnd.Application.Interfaces.Repositories
{
    public interface IEmergencyCaseRepository 
    {
        Task<EmergencyCase> CreateAsync(EmergencyCase entity, CancellationToken cancellationToken);

        Task<EmergencyCase?> GetByIdAsync(int id, CancellationToken cancellationToken);

        // ✅ للكتابة (Update/Delete) — بدون AsNoTracking
        Task<EmergencyCase?> GetByIdTrackedAsync(int id, CancellationToken cancellationToken);

        Task<IEnumerable<EmergencyCase>> GetAllAsync(CancellationToken cancellationToken);

        Task UpdateAsync(EmergencyCase entity, CancellationToken cancellationToken);

        Task DeleteAsync(EmergencyCase entity, CancellationToken cancellationToken);

        Task<bool> ExistsAsync(Expression<Func<EmergencyCase, bool>> predicate, CancellationToken cancellationToken);
    }
}

