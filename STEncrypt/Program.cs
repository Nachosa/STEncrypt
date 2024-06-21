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
                var optionAndCloseApp = ChooseOption();
                if (optionAndCloseApp.Item2 is true)
                {
                    Console.WriteLine("Thanks for using STEncrypt.");
                    return;
                }

                Console.CursorVisible = true;
                string input = string.Empty;
                string key = string.Empty;

                switch (optionAndCloseApp.Item1)
                {
                    case "Encrypt text":
                        do
                        {
                            Console.WriteLine("Provide text below:");
                            input = Console.ReadLine();
                            if (input.Length == 0) Console.WriteLine("Input cannot be empty!");
                        }
                        while (input.Length == 0);
                        var encryptedStringAndKey = Encrypt(input);
                        Console.WriteLine($"Encrypted text: {encryptedStringAndKey.Item1}");
                        Console.ReadKey();
                        break;
                    case "Decrypt text":
                        do
                        {
                            Console.Write("Provide text to decrypt: ");
                            input = Console.ReadLine();
                            Console.Write("Provide key: ");
                            key = Console.ReadLine();
                            if (input.Length == 0 || key.Length==0) Console.WriteLine("Text or key cannot be empty!");
                        }
                        while (input.Length == 0);
                        var decryptedString = Decrypt(input, key);
                        if(decryptedString.Length > 0)Console.WriteLine($"Decrypted text: {decryptedString}");
                        Console.ReadKey();
                        break;
                }

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
                Console.WriteLine($"Encryption key: {Convert.ToBase64String(aes.Key)}");
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
            byte[] encStringBytes = new byte[16];

            using (Aes aes = Aes.Create())
            {
                try
                {
                    aes.Key = Convert.FromBase64String(key);
                }
                catch (Exception)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("INVALID KEY!");
                    return "";
                }

                aes.IV = iv;

                try
                {
                    encStringBytes = Convert.FromBase64String(cipherText);
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
                catch (Exception)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("INVALID TEXT!");
                    return "";
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


        private static Tuple<string,bool> ChooseOption()
        {
            string[] options = { "Encrypt text", "Decrypt text"};
            int selectedIndex = 0;
            bool confirmed = false;
            bool closeApp = false;

            ConsoleKey key;
            Console.CursorVisible = false;
            do
            {
                Console.Clear();
                Console.WriteLine("Choose one option or press ESC to exit.");
                for (int i = 0; i < options.Length; i++)
                {
                    if (i == selectedIndex)
                    {
                        Console.ForegroundColor = ConsoleColor.Magenta;
                        Console.WriteLine("> " + options[i]);
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.WriteLine("  " + options[i]);
                    }
                }

                key = Console.ReadKey(true).Key;
                switch (key)
                {
                    case ConsoleKey.UpArrow:
                        selectedIndex = (selectedIndex == 0) ? options.Length - 1 : selectedIndex - 1;
                        break;
                    case ConsoleKey.DownArrow:
                        selectedIndex = (selectedIndex == options.Length - 1) ? 0 : selectedIndex + 1;
                        break;
                    case ConsoleKey.Enter:
                        confirmed = true;
                        break;
                    case ConsoleKey.Escape:
                        closeApp = true;
                        confirmed = true;
                        break;
                }
            } while (!confirmed);

            Console.Clear();
            
            return new Tuple<string,bool>(options[selectedIndex],closeApp);
        }
    }
    
}
