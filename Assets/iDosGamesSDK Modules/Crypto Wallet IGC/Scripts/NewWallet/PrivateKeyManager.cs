using System;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace IDosGames
{
    public static class PrivateKeyManager
    {
        private const string PlayerPrefsPrivateKey = "EncryptedPrivateKey";
        private const string PlayerPrefsSeedPhraseKey = "EncryptedSeedPhrase";

        public static void SaveSeedPhrase(string seedPhrase, string privateKey, string password)
        {
            // Encrypt and save the private key  
            string encryptedPrivateKey = Encrypt(privateKey, password);
            PlayerPrefs.SetString(PlayerPrefsPrivateKey, encryptedPrivateKey);

            // Encrypt and save the seed phrase  
            string encryptedSeedPhrase = Encrypt(seedPhrase, password);
            PlayerPrefs.SetString(PlayerPrefsSeedPhraseKey, encryptedSeedPhrase);

            // Save changes to PlayerPrefs  
            PlayerPrefs.Save();

            Debug.Log("PrivateKey and SeedPhrase saved.");
        }

        public static (string seedPhrase, string privateKey) GetSeedPhrase(string password)
        {
            // Retrieve and decrypt the private key  
            string encryptedPrivateKey = PlayerPrefs.GetString(PlayerPrefsPrivateKey, null);
            if (string.IsNullOrEmpty(encryptedPrivateKey))
            {
                Debug.LogWarning("PrivateKey not found.");
                return (null, null);
            }

            string privateKey;
            try
            {
                privateKey = Decrypt(encryptedPrivateKey, password);
                if (IDosGamesSDKSettings.Instance.DebugLogging)
                {
                    Debug.Log("Decoded PrivateKey: " + privateKey);
                }
            }
            catch
            {
                Debug.LogWarning("Incorrect Password for PrivateKey");
                return (null, null);
            }

            // Retrieve and decrypt the seed phrase  
            string encryptedSeedPhrase = PlayerPrefs.GetString(PlayerPrefsSeedPhraseKey, null);
            if (string.IsNullOrEmpty(encryptedSeedPhrase))
            {
                Debug.LogWarning("SeedPhrase not found.");
                return (privateKey, null);
            }

            string seedPhrase;
            try
            {
                seedPhrase = Decrypt(encryptedSeedPhrase, password);
                if (IDosGamesSDKSettings.Instance.DebugLogging)
                {
                    Debug.Log("Decoded SeedPhrase: " + seedPhrase);
                }
            }
            catch
            {
                Debug.LogWarning("Incorrect Password for SeedPhrase");
                return (privateKey, null);
            }

            return (seedPhrase, privateKey);
        }

        private static string Encrypt(string plainText, string password)
        {
            byte[] salt = GenerateRandomBytes(16);
            var key = new Rfc2898DeriveBytes(password, salt, 100000).GetBytes(32);
            byte[] iv = GenerateRandomBytes(16);

            using (var aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                {
                    byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);
                    byte[] encryptedBytes = encryptor.TransformFinalBlock(plainTextBytes, 0, plainTextBytes.Length);

                    byte[] combinedBytes = new byte[salt.Length + iv.Length + encryptedBytes.Length];
                    Buffer.BlockCopy(salt, 0, combinedBytes, 0, salt.Length);
                    Buffer.BlockCopy(iv, 0, combinedBytes, salt.Length, iv.Length);
                    Buffer.BlockCopy(encryptedBytes, 0, combinedBytes, salt.Length + iv.Length, encryptedBytes.Length);

                    return Convert.ToBase64String(combinedBytes);
                }
            }
        }

        private static string Decrypt(string encryptedMessage, string password)
        {
            byte[] combinedBytes = Convert.FromBase64String(encryptedMessage);
            byte[] salt = new byte[16];
            byte[] iv = new byte[16];
            byte[] encryptedBytes = new byte[combinedBytes.Length - salt.Length - iv.Length];

            Buffer.BlockCopy(combinedBytes, 0, salt, 0, salt.Length);
            Buffer.BlockCopy(combinedBytes, salt.Length, iv, 0, iv.Length);
            Buffer.BlockCopy(combinedBytes, salt.Length + iv.Length, encryptedBytes, 0, encryptedBytes.Length);

            var key = new Rfc2898DeriveBytes(password, salt, 100000).GetBytes(32);

            using (var aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                {
                    byte[] decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
                    return Encoding.UTF8.GetString(decryptedBytes).TrimEnd('\0');
                }
            }
        }

        private static byte[] GenerateRandomBytes(int length)
        {
            byte[] randomBytes = new byte[length];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(randomBytes);
            }
            return randomBytes;
        }
    }
}
