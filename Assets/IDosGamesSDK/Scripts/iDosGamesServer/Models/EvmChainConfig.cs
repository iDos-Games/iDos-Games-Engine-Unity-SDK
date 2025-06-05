using System.Collections.Generic;

namespace IDosGames
{
    public class ChainConfig : Dictionary<string, EvmChainConfig> { }

    public class EvmChainConfig
    {
#pragma warning disable IDE1006 // Стили именования
        public string data { get; set; }
        public string chainTicker { get; set; }
        public string nativeTokenTicker { get; set; }
        public string baseTokenAddress { get; set; }
        public string routerAddress { get; set; }
        public string funDeployerContractAddress { get; set; }
        public string funPoolContractAddress { get; set; }
        public string funEventTrackerContractAddress { get; set; }
        public string platformPoolContractAddress { get; set; }
        public string totalSupply { get; set; }
        public string teamFee { get; set; }
        public string liquidityETHAmount { get; set; }
        public string defaultAntiSnipeAmount { get; set; }
        public decimal gasPrice { get; set; }
        public string rpcUrl { get; set; }
        public string wssRpcUrl { get; set; }
        public string openRpcUrl { get; set; }
        public string advancedRpcUrl { get; set; }
        public string blockchainExplorerUrl { get; set; }
#pragma warning restore IDE1006 // Стили именования
    }
}
