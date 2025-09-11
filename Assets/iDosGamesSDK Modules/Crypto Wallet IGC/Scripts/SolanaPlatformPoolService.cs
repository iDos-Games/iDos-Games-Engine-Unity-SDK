using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;
using Solana.Unity.Programs;
using Solana.Unity.Rpc;
using Solana.Unity.Rpc.Core.Http;
using Solana.Unity.Rpc.Models;
using Solana.Unity.Rpc.Types;
using Solana.Unity.Wallet;

namespace IDosGames
{
    public class SolanaPlatformPoolService
    {
        private readonly IRpcClient _rpc;
        public PublicKey ProgramId { get; }

        // Programs
        private static readonly PublicKey TOKEN_PROGRAM = TokenProgram.ProgramIdKey; // SPL Token (legacy)
        private static readonly PublicKey ASSOCIATED_TOKEN_PROGRAM = new PublicKey("ATokenGPvbdGVxr1b2hvZbsiqW5xWH25efTNsLJA8knL");
        private static readonly PublicKey SYSTEM_PROGRAM = SystemProgram.ProgramIdKey;
        private static readonly PublicKey SYSVAR_INSTRUCTIONS = new PublicKey("Sysvar1nstructions1111111111111111111111111");
        private static readonly PublicKey ED25519_PROGRAM_ID = new PublicKey("Ed25519SigVerify111111111111111111111111111");

        public SolanaPlatformPoolService(IRpcClient rpcClient, string programIdBase58)
        {
            _rpc = rpcClient ?? throw new ArgumentNullException(nameof(rpcClient));
            ProgramId = new PublicKey(programIdBase58);
        }

        // ---------------------- PUBLIC METHODS ----------------------

        /// <summary>
        /// SPL deposit: user -> vault_ata (Anchor: deposit_spl(amount, user_id))
        /// amount Ч in minimum units (u64)
        /// </summary>
        public async Task<RequestResult<string>> DepositSplAsync(
            Account payer,
            PublicKey mint,
            ulong amount,
            string userId,
            PublicKey configPda = default,
            PublicKey vaultPda = default,
            Commitment commitment = Commitment.Confirmed)
        {
            if (payer == null) throw new ArgumentNullException(nameof(payer));

            // PDA (you can transfer them from the server so as not to derive them on the client)
            if (configPda == default) configPda = ResolvePda(new[] { "config" }, ProgramId);
            if (vaultPda == default) vaultPda = ResolvePda(new[] { "vault" }, ProgramId);

            // ATAs
            var userAta = AssociatedTokenAccountProgram.DeriveAssociatedTokenAccount(payer.PublicKey, mint);
            var vaultAta = AssociatedTokenAccountProgram.DeriveAssociatedTokenAccount(vaultPda, mint);

            // Anchor ix: deposit_spl(amount, user_id)
            var ix = BuildAnchorInstruction(
                "deposit_spl",
                BorshCat(EncodeU64(amount), EncodeString(userId)),
                new List<AccountMeta>
                {
                AccountMeta.ReadOnly(configPda, false),
                AccountMeta.Writable(vaultPda, false),
                AccountMeta.ReadOnly(mint, false),
                AccountMeta.ReadOnly(payer.PublicKey, true),   // user signer
                AccountMeta.Writable(userAta, false),
                AccountMeta.Writable(vaultAta, false),
                AccountMeta.ReadOnly(TOKEN_PROGRAM, false),
                AccountMeta.ReadOnly(ASSOCIATED_TOKEN_PROGRAM, false),
                AccountMeta.ReadOnly(SYSTEM_PROGRAM, false),
                }
            );

            var bh = (await _rpc.GetLatestBlockHashAsync(commitment)).Result?.Value?.Blockhash;
            var tx = new Transaction
            {
                FeePayer = payer.PublicKey,
                RecentBlockHash = bh,
                Instructions = new List<TransactionInstruction> { ix },
                Signatures = new List<SignaturePubKeyPair>()
            };

            tx.Sign(payer);
            return await _rpc.SendTransactionAsync(Convert.ToBase64String(tx.Serialize()),
                skipPreflight: false, preFlightCommitment: commitment);
        }

        /// <summary>
        /// SPL output for server off-chain signature (Anchor: withdraw_spl(amount, nonce, user_id, sig_ix_index)).
        /// All ed25519 fields (pubkey, message, signature) come from the backend Ч unchanged.
        /// </summary>
        public async Task<RequestResult<string>> WithdrawSplAsync(
            Account payer,
            WithdrawRequest req,
            PublicKey configPda = default,
            PublicKey vaultPda = default,
            PublicKey nonceMarkerPda = default,
            Commitment commitment = Commitment.Confirmed)
        {
            if (payer == null) throw new ArgumentNullException(nameof(payer));
            if (req == null) throw new ArgumentNullException(nameof(req));

            if (configPda == default) configPda = ResolvePda(new[] { "config" }, ProgramId);
            if (vaultPda == default) vaultPda = ResolvePda(new[] { "vault" }, ProgramId);

            if (nonceMarkerPda == default)
            {
                var nonceLe = EncodeU64(req.Nonce); // LE
                nonceMarkerPda = ResolvePda(new byte[][]
                {
                Encoding.ASCII.GetBytes("nonce"),
                nonceLe
                }, ProgramId);
            }

            var vaultAta = AssociatedTokenAccountProgram.DeriveAssociatedTokenAccount(vaultPda, req.Mint);
            var toAta = AssociatedTokenAccountProgram.DeriveAssociatedTokenAccount(req.To, req.Mint);

            // PRE #0: ed25519 verify
            var edIx = CreateEd25519InstructionWithPublicKey(
                HexToBytes(req.Ed25519PublicKeyHex),
                HexToBytes(req.Ed25519MessageHex),
                HexToBytes(req.Ed25519SignatureHex)
            );

            // Anchor ix: withdraw_spl(...)
            var ixWithdraw = BuildAnchorInstruction(
                "withdraw_spl",
                BorshCat(
                    EncodeU64(req.Amount),
                    EncodeU64(req.Nonce),
                    EncodeString(req.UserId),
                    new byte[] { req.SigIxIndex } // u8
                ),
                new List<AccountMeta>
                {
                AccountMeta.ReadOnly(configPda, false),
                AccountMeta.ReadOnly(payer.PublicKey, true),   // payer
                AccountMeta.ReadOnly(vaultPda, false),
                AccountMeta.Writable(nonceMarkerPda, false),
                AccountMeta.ReadOnly(req.Mint, false),
                AccountMeta.ReadOnly(req.To, false),           // unchecked
                AccountMeta.Writable(vaultAta, false),
                AccountMeta.Writable(toAta, false),
                AccountMeta.ReadOnly(SYSVAR_INSTRUCTIONS, false),
                AccountMeta.ReadOnly(TOKEN_PROGRAM, false),
                AccountMeta.ReadOnly(ASSOCIATED_TOKEN_PROGRAM, false),
                AccountMeta.ReadOnly(SYSTEM_PROGRAM, false),
                }
            );

            var bh = (await _rpc.GetLatestBlockHashAsync(commitment)).Result?.Value?.Blockhash;
            var tx = new Transaction
            {
                FeePayer = payer.PublicKey,
                RecentBlockHash = bh,
                Instructions = new List<TransactionInstruction> { edIx, ixWithdraw },
                Signatures = new List<SignaturePubKeyPair>()
            };

            tx.Sign(payer);
            return await _rpc.SendTransactionAsync(Convert.ToBase64String(tx.Serialize()),
                skipPreflight: false, preFlightCommitment: commitment);
        }

        public Task<RequestResult<string>> WithdrawSplAsync(
        Account payer,
        ServerWithdrawPayload srv,
        Commitment commitment = Commitment.Confirmed)
        {
            if (srv == null) throw new ArgumentNullException(nameof(srv));

            var req = new WithdrawRequest
            {
                Mint = new PublicKey(srv.mint),
                To = new PublicKey(srv.walletAddress),
                Amount = ulong.Parse(srv.amount),
                Nonce = ulong.Parse(srv.nonce),
                UserId = srv.userID,
                Ed25519PublicKeyHex = srv.ed25519PublicKey,
                Ed25519MessageHex = srv.ed25519Message,
                Ed25519SignatureHex = srv.signatureHex,
                SigIxIndex = (byte)srv.sigIxIndex
            };

            return WithdrawSplAsync(payer, req, commitment: commitment);
        }

        // ---------------------- HELPERS ----------------------

        private TransactionInstruction BuildAnchorInstruction(string methodName, byte[] args, List<AccountMeta> accounts)
        {
            var data = BorshCat(AnchorDiscriminator(methodName), args);
            return new TransactionInstruction
            {
                ProgramId = ProgramId,
                Keys = accounts,
                Data = data
            };
        }

        private static byte[] AnchorDiscriminator(string methodName)
        {
            using var sha = SHA256.Create();
            var pre = Encoding.UTF8.GetBytes($"global:{methodName}");
            var hash = sha.ComputeHash(pre);
            return hash.Take(8).ToArray();
        }

        private static byte[] EncodeU64(ulong v)
        {
            var b = BitConverter.GetBytes(v);
            if (!BitConverter.IsLittleEndian) Array.Reverse(b);
            return b;
        }

        private static byte[] EncodeString(string s)
        {
            var bytes = Encoding.UTF8.GetBytes(s ?? string.Empty);
            var len = BitConverter.GetBytes((uint)bytes.Length);
            if (!BitConverter.IsLittleEndian) Array.Reverse(len);
            return BorshCat(len, bytes);
        }

        private static byte[] BorshCat(params byte[][] arr)
        {
            var total = arr.Sum(a => a?.Length ?? 0);
            var res = new byte[total];
            int off = 0;
            foreach (var a in arr)
            {
                if (a == null) continue;
                Buffer.BlockCopy(a, 0, res, off, a.Length);
                off += a.Length;
            }
            return res;
        }

        private static byte[] HexToBytes(string hex)
        {
            if (string.IsNullOrWhiteSpace(hex)) return Array.Empty<byte>();
            var h = hex.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ? hex[2..] : hex;
            int len = h.Length / 2;
            var data = new byte[len];
            for (int i = 0; i < len; i++)
                data[i] = Convert.ToByte(h.Substring(i * 2, 2), 16);
            return data;
        }

        private static TransactionInstruction CreateEd25519InstructionWithPublicKey(byte[] publicKey32, byte[] message, byte[] signature64)
        {
            if (publicKey32 == null || publicKey32.Length != 32) throw new ArgumentException("ed25519 public key must be 32 bytes");
            if (signature64 == null || signature64.Length != 64) throw new ArgumentException("ed25519 signature must be 64 bytes");
            message ??= Array.Empty<byte>();

            // layout Ч as in web3.js Ed25519Program.createInstructionWithPublicKey
            const int headerLen = 1 + 1 + 7 * 2; // 16 байт
            ushort sigOffset = (ushort)headerLen;
            ushort sigIxIdx = 0;
            ushort pkOffset = (ushort)(sigOffset + 64);
            ushort pkIxIdx = 0;
            ushort msgOffset = (ushort)(pkOffset + 32);
            ushort msgSize = (ushort)message.Length;
            ushort msgIxIdx = 0;

            var data = new List<byte>(headerLen + 64 + 32 + message.Length)
            {
                1, // num signatures
                0  // padding
            };
            void PushU16(ushort v) { data.Add((byte)(v & 0xff)); data.Add((byte)(v >> 8)); }
            PushU16(sigOffset);
            PushU16(sigIxIdx);
            PushU16(pkOffset);
            PushU16(pkIxIdx);
            PushU16(msgOffset);
            PushU16(msgSize);
            PushU16(msgIxIdx);

            data.AddRange(signature64);
            data.AddRange(publicKey32);
            data.AddRange(message);

            return new TransactionInstruction
            {
                ProgramId = ED25519_PROGRAM_ID,
                Keys = new List<AccountMeta>(),
                Data = data.ToArray()
            };
        }

        // PDA resolver via reflection (if AddressExtensions is in the assembly)
        private static PublicKey ResolvePda(IEnumerable<string> asciiSeeds, PublicKey programId)
            => ResolvePda(asciiSeeds.Select(s => Encoding.ASCII.GetBytes(s)).ToArray(), programId);

        private static PublicKey ResolvePda(byte[][] seeds, PublicKey programId)
        {
            // 1) Solana.Unity.Wallet.Utilities.AddressExtensions.FindProgramAddress
            var t1 = Type.GetType("Solana.Unity.Wallet.Utilities.AddressExtensions, Solana.Unity.Wallet");
            if (t1 != null)
            {
                var m = t1.GetMethod("FindProgramAddress", BindingFlags.Public | BindingFlags.Static);
                if (m != null)
                {
                    var pdaObj = m.Invoke(null, new object[] { seeds, programId });
                    var pdaPubKeyProp = pdaObj.GetType().GetProperty("PublicKey");
                    return (PublicKey)pdaPubKeyProp.GetValue(pdaObj);
                }
            }

            // 2) Solnet.Wallet.Utilities.AddressExtensions.FindProgramAddress
            var t2 = Type.GetType("Solnet.Wallet.Utilities.AddressExtensions, Solnet.Wallet");
            if (t2 != null)
            {
                var m = t2.GetMethod("FindProgramAddress", BindingFlags.Public | BindingFlags.Static);
                if (m != null)
                {
                    var pdaObj = m.Invoke(null, new object[] { seeds, programId });
                    var pdaPubKeyProp = pdaObj.GetType().GetProperty("PublicKey");
                    return (PublicKey)pdaPubKeyProp.GetValue(pdaObj);
                }
            }

            throw new InvalidOperationException(
                "Could not find AddressExtensions.FindProgramAddress. " +
                "Pass the PDA from the server to the method parameters or connect the package with this method.");
        }
    }

    // DTO without init (to avoid catching IsExternalInit)
    public class WithdrawRequest
    {
        public PublicKey Mint { get; set; }
        public PublicKey To { get; set; }
        public ulong Amount { get; set; }
        public ulong Nonce { get; set; }
        public string UserId { get; set; }
        public string Ed25519PublicKeyHex { get; set; }  // 0x...
        public string Ed25519MessageHex { get; set; }    // 0x... (BORSH { program, mint, to, amount, nonce, user_id })
        public string Ed25519SignatureHex { get; set; }  // 0x...
        public byte SigIxIndex { get; set; } = 0;        // PRE instruction index (usually 0)
    }

    public class ServerWithdrawPayload
    {
        public string mint { get; set; }
        public string walletAddress { get; set; }
        public string amount { get; set; }        // "5000000"
        public string nonce { get; set; }         // "1757111418234"
        public string programID { get; set; }     // "FWvDZ..."
        public string signatureHex { get; set; }  // 0x...
        public int sigIxIndex { get; set; }    // 0
        public string ed25519PublicKey { get; set; } // 0x...
        public string ed25519Message { get; set; }   // 0x...
        public string userID { get; set; }
    }
}
