using System;
using System.Security.Cryptography;
using System.Text;

namespace MoviesApi.Services
{
    public class AES
    {
        private readonly byte[] _key;

        public AES(string key)
        {
            if (key.Length != 16 && key.Length != 24 && key.Length != 32)
                throw new ArgumentException("Key length must be 16, 24, or 32 bytes.");

            _key = Encoding.UTF8.GetBytes(key);
        }

        public (byte[] CipherText, byte[] Tag, byte[] Nonce) Encrypt(string plainText)
        {
            using var aesGcm = new AesGcm(_key);
            byte[] nonce = new byte[12]; // 12 bytes is recommended for GCM
            byte[] tag = new byte[16];
            byte[] cipherText = new byte[plainText.Length];

            new Random().NextBytes(nonce); // Generate random nonce
            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);

            aesGcm.Encrypt(nonce, plainBytes, cipherText, tag);
            return (cipherText, tag, nonce);
        }

        public string Decrypt(byte[] cipherText, byte[] tag, byte[] nonce)
        {
            using var aesGcm = new AesGcm(_key);
            byte[] decryptedBytes = new byte[cipherText.Length];

            aesGcm.Decrypt(nonce, cipherText, tag, decryptedBytes);
            return Encoding.UTF8.GetString(decryptedBytes);
        }
    }
}
