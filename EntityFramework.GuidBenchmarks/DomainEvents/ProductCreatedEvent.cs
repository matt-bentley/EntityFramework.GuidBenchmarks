using EntityFramework.GuidBenchmarks.DomainEvents.Abstract;

namespace EntityFramework.GuidBenchmarks.DomainEvents
{
    public sealed class ProductCreatedEvent : DomainEvent
    {
        public Guid ProductId { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
    }
}
