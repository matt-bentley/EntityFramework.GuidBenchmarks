
namespace EntityFramework.GuidBenchmarks.Entities
{
    public sealed class BaselineEventStream
    {
        public BaselineEventStream(string events)
        {
            Events = events;
            CreatedDate = DateTime.UtcNow;
            Version = 1;
        }

        public int Id { get; private set; }
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
