using System.Security.Cryptography;
using spikewall.Config;
using System.Text;

namespace spikewall.Encryption
{
    /// <summary>
    /// AES decryption and encryption helper class.
    /// </summary>
    public static class EncryptionHelper
    {
        static readonly string m_encryptionKey = "Ec7bLaTdSuXuf5pW";
        static readonly byte[] m_encryptionKeyByte = Encoding.UTF8.GetBytes(m_encryptionKey);
        static readonly string m_encryptionIV = ConfigGetter.GetConfig().IV; // This is the IV sent by the server, not to be confused with the one the client sends. TODO: Make this configurable!
        static readonly byte[] m_encryptionIVByte = Encoding.UTF8.GetBytes(m_encryptionIV);

        /// <summary>
        /// AES decrypts a given string using the encryption key and IV.
        /// </summary>
        /// <param name="input">Base64-encoded string to be decrypted.</param>
        /// <returns>A decrypted string.</returns>
        public static string Decrypt(string input, string iv)
        {
            string decryptedOutput;
            byte[] cleanedInput = Convert.FromBase64String(input);
            byte[] ivByte = Encoding.UTF8.GetBytes(iv);

            Aes aes = Aes.Create();
            aes.BlockSize = 128;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = m_encryptionKeyByte;
            aes.IV = ivByte;

            var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

            using MemoryStream ms = new(cleanedInput);
            using CryptoStream cs = new(ms, decryptor, CryptoStreamMode.Read);
            using StreamReader sr = new(cs);
            decryptedOutput = sr.ReadToEnd();

            return decryptedOutput;
        }

        /// <summary>
        /// AES encrypts and Base64-encodes a given string using the encryption key and IV.
        /// </summary>
        /// <param name="input">String to be encrypted.</param>
        /// <returns>A Base64-encoded and AES encrypted string.</returns>
        public static string Encrypt(string input, string iv)
        {
            string encryptedOutput;
            byte[] encryptedByte;
            byte[] ivByte = Encoding.UTF8.GetBytes(iv);

            Aes aes = Aes.Create();
            aes.BlockSize = 128;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = m_encryptionKeyByte;
            aes.IV = ivByte;

            var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

            using MemoryStream ms = new();
            using CryptoStream cs = new(ms, encryptor, CryptoStreamMode.Write);
            using (StreamWriter sw = new(cs)) {
                sw.Write(input);
            }

            encryptedByte = ms.ToArray();
            encryptedOutput = Convert.ToBase64String(encryptedByte);

            return encryptedOutput;
        }
    }
}
