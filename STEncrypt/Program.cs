using System;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace STEncrypt
{
    internal class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine("Please enter a string for encryption");
                var str = Console.ReadLine();
                var encryptedStringAndKey = Encrypt(str);
                Console.WriteLine($"encrypted string = {encryptedStringAndKey.Item1}");
                var decryptedString = Decrypt(encryptedStringAndKey.Item1, encryptedStringAndKey.Item2);
                Console.WriteLine($"decrypted string = {decryptedString}");
            }
        }


        private static Tuple<string, string> Encrypt(string plainText)
        {
            byte[] iv = new byte[16];
            byte[] encStringBytes;
            byte[] keyValue;

            using (Aes aes = Aes.Create())
            {

                // var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
                aes.Key = GenerateRandomKey(256);
                keyValue = aes.Key;
                Console.WriteLine($"Key is {Convert.ToBase64String(aes.Key)}");
                aes.IV = iv;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter streamWriter = new StreamWriter((Stream)cryptoStream))
                        {
                            streamWriter.Write(plainText);
                        }

                        encStringBytes = memoryStream.ToArray();
                    }
                }
            }

            return new Tuple<string, string>(Convert.ToBase64String(encStringBytes), Convert.ToBase64String(keyValue));
        }

        private static string Decrypt(string cipherText, string key)
        {
            byte[] iv = new byte[16];
            byte[] encStringBytes = Convert.FromBase64String(cipherText);

            using (Aes aes = Aes.Create())
            {
                aes.Key = Convert.FromBase64String(key);
                aes.IV = iv;
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream(encStringBytes))
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader streamReader = new StreamReader((Stream)cryptoStream))
                        {
                            return streamReader.ReadToEnd();
                        }
                    }
                }
            }
        }

        private static byte[] GenerateRandomKey(int keySizeInBits)
        {
            if (keySizeInBits != 128 && keySizeInBits != 192 && keySizeInBits != 256)
                throw new ArgumentException("Invalid key size. Valid sizes are 128, 192, or 256 bits.");

            int keySizeInBytes = keySizeInBits / 8;
            byte[] key = new byte[keySizeInBytes];

            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(key);
            }

            return key;
        }


    }
}
