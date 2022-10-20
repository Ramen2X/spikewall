using System.Security.Cryptography;
using System.Text;

namespace spikewall.Encryption
{
    /// <summary>
    /// AES decryption and encryption helper class.
    /// </summary>
    public static class Encryption
    {
        static readonly string m_encryptionKey = "Ec7bLaTdSuXuf5pW";
        static readonly byte[] m_encryptionKeyByte = System.Text.Encoding.UTF8.GetBytes(m_encryptionKey);
        static readonly string m_encryptionIV = "burgersMetKortin"; // Temporary, the client is the one who gives us the decryption IV
        static readonly byte[] m_encryptionIVByte = System.Text.Encoding.UTF8.GetBytes(m_encryptionIV);

        /// <summary>
        /// AES decrypts a given string using the encryption key and IV.
        /// </summary>
        /// <param name="input">Base64-encoded string to be decrypted.</param>
        /// <returns>A decrypted string.</returns>
        public static string Decrypt(string input)
        {
            string decryptedOutput;
            byte[] cleanedInput = Convert.FromBase64String(input);

            Aes aes = Aes.Create();
            aes.BlockSize = 128;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = m_encryptionKeyByte;
            aes.IV = m_encryptionIVByte;

            var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

            using MemoryStream ms = new(cleanedInput);
            using CryptoStream cs = new(ms, decryptor, CryptoStreamMode.Read);
            using StreamReader sr = new(cs);
            decryptedOutput = sr.ReadToEnd();

            return decryptedOutput;
        }

        /// <summary>
        /// AES encrypts a given string using the encryption key and IV.
        /// </summary>
        /// <param name="input">String to be encrypted.</param>
        /// <returns>A Base64-encoded and AES encrypted string.</returns>
        public static string Encrypt(string input)
        {
            string encryptedOutput;
            byte[] encryptedByte;

            Aes aes = Aes.Create();
            aes.BlockSize = 128;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = m_encryptionKeyByte;
            aes.IV = m_encryptionIVByte;

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
