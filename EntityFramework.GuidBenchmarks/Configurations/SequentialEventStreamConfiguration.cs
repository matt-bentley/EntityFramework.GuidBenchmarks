using EntityFramework.GuidBenchmarks.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EntityFramework.GuidBenchmarks.Configurations
{
    public sealed class SequentialEventStreamConfiguration : IEntityTypeConfiguration<SequentialEventStream>
    {
        public void Configure(EntityTypeBuilder<SequentialEventStream> builder)
        {
            builder.Property(e => e.Id)
                   .ValueGeneratedNever();

            builder.Property(e => e.SequentialId)
                   .UseIdentityColumn()
                   .ValueGeneratedOnAdd();
            
            builder.HasKey(e => e.Id).IsClustered(false);
            
            builder.HasIndex(e => e.SequentialId).IsClustered();

            builder.Property(e => e.Events)
                   .HasColumnType("varchar(max)")
                   .IsRequired();
        }
    }
}
