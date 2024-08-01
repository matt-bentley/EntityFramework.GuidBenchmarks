using System.Text.Json.Serialization;

namespace EntityFramework.GuidBenchmarks.DomainEvents.Abstract
{
    [JsonDerivedType(typeof(ProductCreatedEvent), nameof(ProductCreatedEvent))]
    [JsonDerivedType(typeof(ProductUpdatedEvent), nameof(ProductUpdatedEvent))]
    public abstract class DomainEvent
    {
        public DomainEvent()
        {
            Id = Guid.NewGuid();
            Timestamp = DateTime.UtcNow;
        }

        public Guid Id { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
