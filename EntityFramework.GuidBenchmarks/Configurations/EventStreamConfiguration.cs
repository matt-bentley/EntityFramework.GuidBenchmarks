using EntityFramework.GuidBenchmarks.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EntityFramework.GuidBenchmarks.Configurations
{
    public sealed class EventStreamConfiguration : IEntityTypeConfiguration<EventStream>
    {
        public void Configure(EntityTypeBuilder<EventStream> builder)
        {
            builder.Property(e => e.Id)
                   .ValueGeneratedNever();

            builder.Property(e => e.Events)
                   .HasColumnType("varchar(max)")
                   .IsRequired();
        }
    }
}
