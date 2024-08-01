using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace EntityFramework.GuidBenchmarks.Entities
{
    public sealed class OrderedEventStream
    {
        private static readonly SequentialGuidValueGenerator _idFactory = new();

        public OrderedEventStream(Guid id, string events)
        {
            Id = id;
            Events = events;
            CreatedDate = DateTime.UtcNow;
            Version = 1;
        }

        public Guid Id { get; private set; }
        public string Events { get; private set; }
        public DateTime CreatedDate { get; private set; }
        public int Version { get; private set; }

        public static Guid GenerateId() => _idFactory.Next(null!);

        public void Update(string events)
        {
            Events = events;
            Version++;
        }
    }
}
