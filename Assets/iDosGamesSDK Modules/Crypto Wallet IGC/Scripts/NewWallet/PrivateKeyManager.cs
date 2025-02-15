using System;
using System.Text;
using UnityEngine;

namespace IDosGames
{
    public static class PrivateKeyManager
    {
        private static string PlayerPrefsPrivateKey = "EncryptedPrivateKey" + AuthService.UserID;
        private static string PlayerPrefsSeedPhraseKey = "EncryptedSeedPhrase" + AuthService.UserID;

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
                    //Debug.Log("Decoded PrivateKey: " + privateKey);
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
                    //Debug.Log("Decoded SeedPhrase: " + seedPhrase);
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
            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            byte[] encryptedBytes = new byte[plainBytes.Length];

            for (int i = 0; i < plainBytes.Length; i++)
            {
                encryptedBytes[i] = (byte)(plainBytes[i] ^ passwordBytes[i % passwordBytes.Length]);
            }

            return Convert.ToBase64String(encryptedBytes);
        }

        private static string Decrypt(string encryptedMessage, string password)
        {
            byte[] encryptedBytes = Convert.FromBase64String(encryptedMessage);
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            byte[] plainBytes = new byte[encryptedBytes.Length];

            for (int i = 0; i < encryptedBytes.Length; i++)
            {
                plainBytes[i] = (byte)(encryptedBytes[i] ^ passwordBytes[i % passwordBytes.Length]);
            }

            return Encoding.UTF8.GetString(plainBytes).TrimEnd('\0');
        }
    }
}
