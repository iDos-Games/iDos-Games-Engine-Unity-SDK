using IDosGames.TitlePublicConfiguration;
using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

namespace IDosGames
{
    public static class BlockchainSettings
    {
        public const int DEFAULT_VALUE_IN_NATIVE_TOKEN = 0;
        public static string ChainType { get; private set; }
        public static string RpcUrl { get; private set; }
        public static int ChainID { get; private set; }
        public static string BlockchainExplorerUrl { get; private set; }

        public static string HotWalletAddress { get; private set; }

        public static string SoftTokenTicker { get; private set; }
        public static string SoftTokenContractAddress { get; private set; }
        public static string SoftTokenContractAbi { get; private set; }

        public static string HardTokenTicker { get; private set; }
        public static string HardTokenContractAddress { get; private set; }
        public static string HardTokenContractAbi { get; private set; }

        public static string NftContractAddress { get; private set; }
        public static string NftContractAbi { get; private set; }

        public static float GasPrice { get; private set; }

        [RuntimeInitializeOnLoadMethod]
        private static void Initialize()
        {
            UserDataService.TitleDataUpdated += SetWallet;
        }

        public static void SetWallet()
        {
#if IDOSGAMES_CRYPTO_WALLET
            
            string titleData = UserDataService.GetCachedTitleData(TitleDataKey.CryptoWallet);

            List<CryptoWallet> cryptoWallets = JsonConvert.DeserializeObject<List<CryptoWallet>>(titleData);

            if (cryptoWallets != null && cryptoWallets.Count > 0)
            {
                CryptoWallet firstChain = cryptoWallets[0];

                ChainType = firstChain.ChainType;
                ChainID = firstChain.ChainID;
                RpcUrl = firstChain.RpcUrl;
                GasPrice = firstChain.GasPrice;
                BlockchainExplorerUrl = firstChain.BlockchainExplorerUrl;

                SoftTokenTicker = firstChain.SoftTokenTicker;
                SoftTokenContractAddress = firstChain.SoftTokenContractAddress;
                SoftTokenContractAbi = firstChain.SoftTokenContractAbi;

                HardTokenTicker = firstChain.HardTokenTicker;
                HardTokenContractAddress = firstChain.HardTokenContractAddress;
                HardTokenContractAbi = firstChain.HardTokenContractAbi;

                NftContractAddress = firstChain.NftContractAddress;
                NftContractAbi = firstChain.NftContractAbi;

                HotWalletAddress = firstChain.HotWalletAddress;
            }
            else
            {
                Debug.LogWarning("In the Project Settings you need to set the settings for Crypto Wallet");
            }
#endif

        }

        public static string GetTokenContractABI(VirtualCurrencyID tokenID)
        {
            string contractAddress = string.Empty;

            switch (tokenID)
            {
                case VirtualCurrencyID.IG:
                    contractAddress = HardTokenContractAbi;
                    break;
                case VirtualCurrencyID.CO:
                    contractAddress = SoftTokenContractAbi;
                    break;
            }

            return contractAddress;
        }

        public static string GetTokenContractAddress(VirtualCurrencyID tokenID)
        {
            string contractAddress = string.Empty;

            switch (tokenID)
            {
                case VirtualCurrencyID.IG:
                    contractAddress = HardTokenContractAddress;
                    break;
                case VirtualCurrencyID.CO:
                    contractAddress = SoftTokenContractAddress;
                    break;
            }

            return contractAddress;
        }
    }
}