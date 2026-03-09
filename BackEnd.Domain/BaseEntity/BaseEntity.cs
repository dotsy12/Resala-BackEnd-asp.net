using BackEnd.Domain.Interfaces;

namespace BackEnd.Domain.Common
{
    public abstract class BaseEntity<TId> : IEntity
    {
        private readonly List<IDomainEvent> _domainEvents = new();

        public TId Id { get; set; } = default!;
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedOn { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;

        public IReadOnlyList<IDomainEvent> DomainEvents =>
            _domainEvents.AsReadOnly();

        protected void AddDomainEvent(IDomainEvent domainEvent)
            => _domainEvents.Add(domainEvent);

        public void ClearDomainEvents()
            => _domainEvents.Clear();
    }
}