using EntityFramework.GuidBenchmarks.Entities;
using Microsoft.EntityFrameworkCore;

namespace EntityFramework.GuidBenchmarks
{
    public sealed class AppDbContext : DbContext
    {
        public DbSet<BaselineEventStream> BaselineEventStreams { get; set; }
        public DbSet<EventStream> EventStreams { get; set; }
        public DbSet<OrderedEventStream> OrderedEventStreams { get; set; }
        public DbSet<OrderedCompressedEventStream> OrderedCompressedEventStreams { get; set; }
        public DbSet<SequentialEventStream> SequentialEventStreams { get; set; }
        public DbSet<SequentialCompressedEventStream> SequentialCompressedEventStreams { get; set; }

        public DbSet<Statistics> Statistics { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=127.0.0.1, 1433; Database=GuidBenchmarks; Integrated Security=False; User Id = SA; Password=Admin1234!; MultipleActiveResultSets=False;TrustServerCertificate=True");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}
