using System.IO.Compression;

namespace EntityFramework.GuidBenchmarks.Helpers
{
    public static class Compressor
    {
        public static byte[] Compress(string data)
        {
            using (var outputStream = new MemoryStream())
            {
                using (var gzipStream = new GZipStream(outputStream, CompressionMode.Compress))
                using (var writer = new StreamWriter(gzipStream))
                {
                    writer.Write(data);
                }
                return outputStream.ToArray();
            }
        }

        public static string Decompress(byte[] data)
        {
            using (var inputStream = new MemoryStream(data))
            using (var gzipStream = new GZipStream(inputStream, CompressionMode.Decompress))
            using (var reader = new StreamReader(gzipStream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
