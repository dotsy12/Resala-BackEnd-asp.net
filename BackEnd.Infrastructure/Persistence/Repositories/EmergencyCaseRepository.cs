using BackEnd.Application.Interfaces.Repositories;
using BackEnd.Domain.Entities.EmergencyCase;
using BackEnd.Infrastructure.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BackEnd.Infrastructure.Persistence.Repositories
{
        public class EmergencyCaseRepository : IEmergencyCaseRepository
    {
            private readonly ApplicationDbContext _context;

            public EmergencyCaseRepository(ApplicationDbContext context)
            {
                _context = context;
            }

            public async Task<EmergencyCase> CreateAsync(
                EmergencyCase entity,
                CancellationToken cancellationToken)
            {
                await _context.EmergencyCases.AddAsync(entity, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);

                return entity;
            }

            public async Task<EmergencyCase?> GetByIdAsync(
                int id,
                CancellationToken cancellationToken)
            {
                return await _context.EmergencyCases
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            }

            public async Task<EmergencyCase?> GetByIdTrackedAsync(
                int id,
                CancellationToken cancellationToken)
            {
                return await _context.EmergencyCases
                    .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            }

            public async Task<IEnumerable<EmergencyCase>> GetAllAsync(
                CancellationToken cancellationToken)
            {
                return await _context.EmergencyCases
                    .OrderByDescending(x => x.CreatedOn)
                    .ToListAsync(cancellationToken);
            }

            public async Task UpdateAsync(
                EmergencyCase entity,
                CancellationToken cancellationToken)
            {
                _context.EmergencyCases.Update(entity);
                await _context.SaveChangesAsync(cancellationToken);
            }

            public async Task DeleteAsync(
                EmergencyCase entity,
                CancellationToken cancellationToken)
            {
                _context.EmergencyCases.Remove(entity);
                await _context.SaveChangesAsync(cancellationToken);
            }

            public async Task<bool> ExistsAsync(
                Expression<Func<EmergencyCase, bool>> predicate,
                CancellationToken cancellationToken)
            {
                return await _context.EmergencyCases
                    .AnyAsync(predicate, cancellationToken);
            }
        }
  }

