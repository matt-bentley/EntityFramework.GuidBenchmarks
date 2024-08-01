
namespace EntityFramework.GuidBenchmarks.Entities
{
    public sealed class Statistics
    {
        public Statistics(string entityType, string operation, int count, double durationSeconds)
        {
            Id = Guid.NewGuid();
            EntityType = entityType;
            Operation = operation;
            Count = count;
            DurationSeconds = durationSeconds;   
            CompletedDate = DateTime.Now;
        }

        public Guid Id { get; private set; }
        public string EntityType { get; private set; }
        public string Operation { get; private set; }
        public int Count { get; private set; }
        public double DurationSeconds { get; private set; }
        public DateTime CompletedDate { get; private set; }
    }
}
