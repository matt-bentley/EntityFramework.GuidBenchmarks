using EntityFramework.GuidBenchmarks.Entities;
using EntityFramework.GuidBenchmarks.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace EntityFramework.GuidBenchmarks.Configurations
{
    public sealed class OrderedCompressedEventStreamConfiguration : IEntityTypeConfiguration<OrderedCompressedEventStream>
    {
        public void Configure(EntityTypeBuilder<OrderedCompressedEventStream> builder)
        {
            builder.Property(e => e.Id)
                   .ValueGeneratedNever();

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
