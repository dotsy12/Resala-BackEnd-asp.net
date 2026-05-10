using BackEnd.Application.Abstractions.Persistence;
using BackEnd.Application.Interfaces.Services;
using BackEnd.Domain.Common;
using BackEnd.Infrastructure.Persistence.DbContext;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackEnd.Infrastructure.Persistence
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private readonly IMediator _mediator;

        public UnitOfWork(ApplicationDbContext context, IMediator mediator)
        {
            _context = context;
            _mediator = mediator;
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // 1. Dispatch domain events before/after save
            // Better to dispatch AFTER save for notifications so IDs are available
            
            var entitiesWithEvents = _context.ChangeTracker.Entries<BaseEntity<int>>()
                .Select(e => e.Entity)
                .Where(e => e.DomainEvents.Any())
                .ToList();

            var domainEvents = entitiesWithEvents.SelectMany(e => e.DomainEvents).ToList();

            foreach (var entity in entitiesWithEvents)
                entity.ClearDomainEvents();

            var result = await _context.SaveChangesAsync(cancellationToken);

            foreach (var domainEvent in domainEvents)
                await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}
