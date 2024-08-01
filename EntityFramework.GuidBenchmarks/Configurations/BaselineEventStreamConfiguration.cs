using EntityFramework.GuidBenchmarks.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EntityFramework.GuidBenchmarks.Configurations
{
    public sealed class BaselineEventStreamConfiguration : IEntityTypeConfiguration<BaselineEventStream>
    {
        public void Configure(EntityTypeBuilder<BaselineEventStream> builder)
        {
            builder.Property(e => e.Id)
                   .UseIdentityColumn()
                   .ValueGeneratedOnAdd();

            builder.Property(e => e.Events)
                   .HasColumnType("varchar(max)")
                   .IsRequired();
        }
    }
}
