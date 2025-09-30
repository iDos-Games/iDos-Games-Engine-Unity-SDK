using System;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Solana.Unity.Wallet;

namespace IDosGames
{
    public static class SolanaSwapClient
    {
#if UNITY_EDITOR
        public static async UniTask<string> BuyWithSolAsync(string tokenMint, ulong amountLamports)
        {
            EnsureWallet();

            string taker = Solana.Unity.SDK.Web3.Account.PublicKey.Key;
            string createJson = await IGSwapAPI.CreateBuyOrderForSolana(taker, tokenMint, amountLamports);
            (string requestId, string unsignedTxBase64) = ParseOrderResponse(createJson);

            string signedTxBase64 = SignUltraBase64(unsignedTxBase64, Solana.Unity.SDK.Web3.Account);

            string execJson = await IGSwapAPI.ExecuteOrderForSolana(requestId, signedTxBase64);
            string signature = ParseExecuteResponse(execJson);

            return signature;
        }

        public static async UniTask<string> SellForSolAsync(string tokenMint, ulong tokenAmountAtomic)
        {
            EnsureWallet();

            string taker = Solana.Unity.SDK.Web3.Account.PublicKey.Key;
            string createJson = await IGSwapAPI.CreateSellOrderForSolana(taker, tokenMint, tokenAmountAtomic);
            (string requestId, string unsignedTxBase64) = ParseOrderResponse(createJson);

            string signedTxBase64 = SignUltraBase64(unsignedTxBase64, Solana.Unity.SDK.Web3.Account);

            string execJson = await IGSwapAPI.ExecuteOrderForSolana(requestId, signedTxBase64);
            string signature = ParseExecuteResponse(execJson);

            return signature;
        }
#endif

        // ================== Helpers ==================

        private static void EnsureWallet()
        {
            if (Solana.Unity.SDK.Web3.Account == null)
                throw new InvalidOperationException("Wallet not connected. Call Web3.Login... first.");
        }

        private static (string requestId, string unsignedTxBase64) ParseOrderResponse(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                throw new Exception("Empty CreateOrder response.");

            var jo = JObject.Parse(json);
            // ожидаем формат { status: "ok", requestId, unsignedTxBase64, ... }
            string status = jo.Value<string>("status");
            string requestId = jo.Value<string>("requestId");
            string unsignedTxBase64 = jo.Value<string>("unsignedTxBase64");

            if (!string.Equals(status, "ok", StringComparison.OrdinalIgnoreCase))
                throw new Exception($"CreateOrder failed: {json}");

            if (string.IsNullOrWhiteSpace(requestId) || string.IsNullOrWhiteSpace(unsignedTxBase64))
                throw new Exception("CreateOrder returned empty requestId or unsignedTxBase64.");

            return (requestId, unsignedTxBase64);
        }

        private static string ParseExecuteResponse(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                throw new Exception("Empty Execute response.");

            var jo = JObject.Parse(json);
            // ожидаем формат { status: "ok", signature }
            string status = jo.Value<string>("status");
            string signature = jo.Value<string>("signature");

            if (!string.Equals(status, "ok", StringComparison.OrdinalIgnoreCase) || string.IsNullOrWhiteSpace(signature))
                throw new Exception($"Execute failed: {json}");

            return signature;
        }

        private static string SignUltraBase64(string txBase64, Account account)
        {
            byte[] bytes = Convert.FromBase64String(txBase64);

            bool isVersioned = (bytes[0] & 0x80) != 0;

            if (isVersioned)
            {
                // -------- v0 (VersionedTransaction wire) --------
                int idx = 0;
                byte version = bytes[idx++];               // 0x80 | 0
                if ((version & 0x7F) != 0) throw new Exception($"Unsupported version: {version & 0x7F}");

                int sigCount = DecodeShortVec(bytes, idx, out int consumed);
                idx += consumed;
                if (sigCount <= 0) throw new Exception("Invalid sigCount");

                int sigSectionStart = idx;
                int sigSectionLen = sigCount * 64;
                int messageStart = sigSectionStart + sigSectionLen;
                if (messageStart >= bytes.Length) throw new Exception("Corrupted v0 wire");

                var message = new byte[bytes.Length - messageStart];
                Buffer.BlockCopy(bytes, messageStart, message, 0, message.Length);

                byte[] sig = account.Sign(message);
                if (sig == null || sig.Length != 64) throw new Exception("Bad signature length");

                Buffer.BlockCopy(sig, 0, bytes, sigSectionStart, 64);
            }
            else
            {
                // -------- legacy wire --------
                int idx = 0;
                int sigCount = DecodeShortVec(bytes, idx, out int consumed);
                idx += consumed;
                if (sigCount <= 0) throw new Exception("Invalid sigCount");

                int sigSectionStart = idx;
                int sigSectionLen = sigCount * 64;
                int messageStart = sigSectionStart + sigSectionLen;
                if (messageStart >= bytes.Length) throw new Exception("Corrupted legacy wire");

                var message = new byte[bytes.Length - messageStart];
                Buffer.BlockCopy(bytes, messageStart, message, 0, message.Length);

                byte[] sig = account.Sign(message);
                if (sig == null || sig.Length != 64) throw new Exception("Bad signature length");

                Buffer.BlockCopy(sig, 0, bytes, sigSectionStart, 64);
            }

            return Convert.ToBase64String(bytes);
        }

        // shortvec (Solana compact-u16)
        private static int DecodeShortVec(byte[] data, int offset, out int read)
        {
            int result = 0, size = 0, shift = 0;
            while (true)
            {
                if (offset + size >= data.Length) throw new Exception("shortvec truncated");
                byte b = data[offset + size];
                result |= (b & 0x7F) << shift;
                size++;
                if ((b & 0x80) == 0) break;
                shift += 7;
                if (size > 5) throw new Exception("shortvec too long");
            }
            read = size;
            return result;
        }
    }
}
