using BackEnd.Application.Interfaces.Repositories;
using BackEnd.Domain.Entities;
using BackEnd.Infrastructure.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BackEnd.Infrastructure.Persistence.Repositories
{
    public class CartRepository : ICartRepository
    {
        private readonly ApplicationDbContext _context;

        public CartRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(CartItem item, CancellationToken ct = default)
        {
            await _context.Set<CartItem>().AddAsync(item, ct);
        }

        public async Task<CartItem?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _context.Set<CartItem>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
        }

        public async Task<CartItem?> GetByDonorAndTargetAsync(int donorId, int? sponsorshipId, int? emergencyCaseId, CancellationToken ct = default)
        {
            return await _context.Set<CartItem>()
                .FirstOrDefaultAsync(x => x.DonorId == donorId 
                                          && x.SponsorshipId == sponsorshipId 
                                          && x.EmergencyCaseId == emergencyCaseId 
                                          && !x.IsDeleted, ct);
        }

        public async Task<IReadOnlyList<CartItem>> GetByDonorIdAsync(int donorId, CancellationToken ct = default)
        {
            return await _context.Set<CartItem>()
                .Include(c => c.Sponsorship)
                .Include(c => c.EmergencyCase)
                .Where(c => c.DonorId == donorId && !c.IsDeleted)
                .ToListAsync(ct);
        }

        public void Update(CartItem item)
        {
            _context.Set<CartItem>().Update(item);
        }

        public void Remove(CartItem item)
        {
            _context.Set<CartItem>().Remove(item);
        }

        public void RemoveRange(IEnumerable<CartItem> items)
        {
            _context.Set<CartItem>().RemoveRange(items);
        }

        public async Task SaveChangesAsync(CancellationToken ct = default)
        {
            await _context.SaveChangesAsync(ct);
        }
    }
}
