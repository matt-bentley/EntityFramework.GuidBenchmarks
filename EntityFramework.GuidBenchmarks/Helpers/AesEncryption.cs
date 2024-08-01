using System.Security.Cryptography;

namespace EntityFramework.GuidBenchmarks.Helpers
{
    public static class AesEncryption
    {
        private static readonly Aes _aes = CreateAes();

        private static Aes CreateAes()
        {
            var base64Key = "CM3hu59Bv66DB9q6l9/A+GOXL83Bn2o9wlO6dauxXHk=";
            var base64IV = "CaIb79joIXP0v9RaKHZRjg==";
            byte[] key = Convert.FromBase64String(base64Key);
            byte[] iv = Convert.FromBase64String(base64IV);

            var aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;
            return aes;
        }

        public static byte[] Encrypt(byte[] data)
        {
            using (ICryptoTransform encryptor = _aes.CreateEncryptor(_aes.Key, _aes.IV))
            using (MemoryStream msEncrypt = new MemoryStream())
            using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
            {
                csEncrypt.Write(data, 0, data.Length);
                csEncrypt.FlushFinalBlock();
                return msEncrypt.ToArray();
            }
        }

        public static byte[] Decrypt(byte[] cipherText)
        {
            using (ICryptoTransform decryptor = _aes.CreateDecryptor(_aes.Key, _aes.IV))
            using (MemoryStream msDecrypt = new MemoryStream(cipherText))
            using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
            using (MemoryStream msOutput = new MemoryStream())
            {
                csDecrypt.CopyTo(msOutput);
                return msOutput.ToArray();
            }
        }
    }
}
