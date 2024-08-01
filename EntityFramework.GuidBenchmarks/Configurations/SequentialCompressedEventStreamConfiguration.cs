using EntityFramework.GuidBenchmarks.Entities;
using EntityFramework.GuidBenchmarks.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace EntityFramework.GuidBenchmarks.Configurations
{
    public sealed class SequentialCompressedEventStreamConfiguration : IEntityTypeConfiguration<SequentialCompressedEventStream>
    {
        public void Configure(EntityTypeBuilder<SequentialCompressedEventStream> builder)
        {
            builder.Property(e => e.Id)
                   .ValueGeneratedNever();

            builder.Property(e => e.SequentialId)
                   .UseIdentityColumn()
                   .ValueGeneratedOnAdd();
            
            builder.HasKey(e => e.Id).IsClustered(false);
            builder.HasIndex(e => e.Id);
            builder.HasIndex(e => e.SequentialId).IsClustered();

            var compressedConverter = new ValueConverter<string, byte[]>(
                data => Compressor.Compress(data),
                data => Compressor.Decompress(data));

            builder.Property(e => e.Events)
                   .HasColumnType("varbinary(max)")
                   .HasConversion(compressedConverter)
                   .IsRequired();
        }
    }
}
