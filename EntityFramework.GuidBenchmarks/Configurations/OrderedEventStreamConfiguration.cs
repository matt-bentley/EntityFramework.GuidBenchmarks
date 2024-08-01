using EntityFramework.GuidBenchmarks.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EntityFramework.GuidBenchmarks.Configurations
{
    public sealed class OrderedEventStreamConfiguration : IEntityTypeConfiguration<OrderedEventStream>
    {
        public void Configure(EntityTypeBuilder<OrderedEventStream> builder)
        {
            builder.Property(e => e.Id)
                   .ValueGeneratedNever();

            builder.Property(e => e.Events)
                   .HasColumnType("varchar(max)")
                   .IsRequired();
        }
    }
}
