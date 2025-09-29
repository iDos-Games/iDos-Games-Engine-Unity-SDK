using System.Threading.Tasks;

namespace IDosGames
{
    public static class CheckTransactionHash
    {
        public static async Task<string> CheckHash(string txHash, CryptoTransactionType transactionType, TransactionDirection transactionDirection)
        {
            string message = string.Empty;

#if IDOSGAMES_CRYPTO_WALLET
            var request = new WalletTransactionRequest
            {
                ChainType = BlockchainSettings.ChainType,
                ChainID = BlockchainSettings.ChainID,
                TransactionType = transactionType,
                Direction = transactionDirection,
                TransactionHash = txHash.Trim()
            };

            var result = await IGSService.TryMakeTransaction(request);

            message = Message.MessageResult(result);
#endif

            return message;
        }
    }
}
