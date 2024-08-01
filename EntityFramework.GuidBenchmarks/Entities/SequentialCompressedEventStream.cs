
namespace EntityFramework.GuidBenchmarks.Entities
{
    public sealed class SequentialCompressedEventStream
    {
        public SequentialCompressedEventStream(Guid id, string events)
        {
            Id = id;
            Events = events;
            CreatedDate = DateTime.UtcNow;
            Version = 1;
        }

        public Guid Id { get; private set; }
        public long SequentialId { get; private set; }
        public string Events { get; private set; }
        public DateTime CreatedDate { get; private set; }
        public int Version { get; private set; }

        public void Update(string events)
        {
            Events = events;
            Version++;
        }
    }
}
