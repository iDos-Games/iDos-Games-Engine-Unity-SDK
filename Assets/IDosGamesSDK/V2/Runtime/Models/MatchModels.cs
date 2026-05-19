using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace IDosGames
{
    /// <summary>
    /// Unified request for all Match actions.
    ///
    /// Field usage by action:
    ///   CreateMatch        — CurrencyID, EntryFeeAmount, RuleID, CharacterID (opt), BattleStrategy (opt), TargetUserID (opt)
    ///   InstantBattle      — MatchID, RuleID, CharacterID (opt), BattleStrategy (opt)
    ///   SaveStrategy       — BattleStrategy
    ///   GetMyMatches       — Page, PageSize, Statuses (opt)
    ///   GetAvailableMatches— Page, PageSize, CurrencyID (opt), MinEntryFeeAmount (opt), MaxEntryFeeAmount (opt), OnlyPublic
    ///   JoinMatch          — MatchID (reserved, not yet implemented server-side)
    ///   GetMatch           — MatchID (reserved, not yet implemented server-side)
    /// </summary>
    [Serializable]
    public class MatchRequest : BaseRequest
    {
        public string MatchID;
        public string TargetUserID;
        public string CurrencyID;
        public long EntryFeeAmount;
        public List<BattleStepConfig> BattleStrategy;
        public string CharacterID;
        public string RuleID;

        public int Page = 0;
        public int PageSize = 20;
        public List<string> Statuses;
        public long? MinEntryFeeAmount;
        public long? MaxEntryFeeAmount;
        public bool OnlyPublic = false;
    }

    // -------------------------------------------------------------------------
    // Response models
    // -------------------------------------------------------------------------

    /// <summary>
    /// Response for CreateMatch. Resources contains the entry fee consume operation.
    /// </summary>
    [Serializable]
    public class CreateMatchResponse
    {
        public MatchStatus Status;
        public string MatchID;
        public string RewardPoolCurrencyID;
        public long RewardPoolAmount;

        /// <summary>Entry fee deducted from creator, as a ResourceOperation.</summary>
        public ResourceOperation Resources;
    }

    /// <summary>
    /// Response for InstantBattle.
    /// Exactly one of Resources (draw) or ResourcesDual (win/loss) will be non-null.
    /// </summary>
    [Serializable]
    public class InstantBattleResponse
    {
        public BattleResult Battle;

        /// <summary>Populated on draw (creator refund). Null on decisive outcome.</summary>
        public ResourceOperation Resources;

        /// <summary>Populated on decisive outcome (winner/loser ops). Null on draw.</summary>
        public ResourceDualPartyResult ResourcesDual;
    }

    /// <summary>
    /// Response for GetMyMatches and GetAvailableMatches.
    /// </summary>
    [Serializable]
    public class MatchesPageResponse
    {
        public List<PvPMatch> Matches = new();
        public int Page;
        public int PageSize;
        public bool HasMore;
    }

    // -------------------------------------------------------------------------
    // Domain models (client mirror of server state)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Client mirror of PvPMatch stored in the matches collection.
    /// CreatorStrategy is excluded from GetAvailableMatches listings (server projection).
    /// </summary>
    [Serializable]
    public class PvPMatch
    {
        public string MatchID;
        public string RuleID;
        public DateTime CreatedAt;
        public string CreatorID;
        /// <summary>CharacterID the creator fights with. Defaults to "Main" server-side.</summary>
        public string CreatorCharacterID;
        public List<BattleStepConfig> CreatorStrategy;
        /// <summary>If null or empty, match is open to all players.</summary>
        public string TargetUserID;
        public string CurrencyID;
        public long EntryFeeAmount;
        public long RewardPoolAmount;
        public string JoinedByUserID;
        public string JoinedByCharacterID;
        public DateTime? JoinedAt;
        public MatchStatus Status;
        public string WinnerUserID;
        public DateTime? CompletedAt;
        public bool IsRewardDistributed;
        public DateTime? RewardDistributedAt;
    }

    /// <summary>
    /// Client mirror of BattleResult — outcome of a simulated fight.
    /// </summary>
    [Serializable]
    public class BattleResult
    {
        public string WinnerUserID;
        public string LoserUserID;
        public string CurrencyID;
        public long EntryFeeAmount;
        /// <summary>Net prize paid to winner (after 5% burn). Gross on draw (no burn).</summary>
        public long PrizeAmount;
        public List<BattleLogEntry> BattleLog = new();
        public bool IsDraw;
        public PlayerBattleProfile P1BattleProfile;
        public PlayerBattleProfile P2BattleProfile;
    }

    /// <summary>One attack/defense exchange in a battle round.</summary>
    [Serializable]
    public class BattleLogEntry
    {
        public int RoundIndex;
        public string AttackerID;
        public string DefenderID;
        public BodyPart AttackZone;
        public BodyPart DefenseZone;
        public BattleHitType HitType;
        public double DamageDealt;
        public double DefenderHpRemaining;
    }

    /// <summary>Player's snapshot used in battle simulation.</summary>
    [Serializable]
    public class PlayerBattleProfile
    {
        public string UserID;
        public string SelectedCharacterID;
        public CharacterModel SelectedCharacter;
        public List<BattleStepConfig> BattleStrategy;
    }

    /// <summary>
    /// One step in a player's battle strategy (up to 10 steps, cycled each round).
    /// </summary>
    [Serializable]
    public class BattleStepConfig
    {
        public BodyPart AttackTarget;
        public BodyPart DefenseTarget;
    }

    /// <summary>Player's saved PvP state stored in UserDataDocument.Match.</summary>
    [Serializable]
    public class UserMatchState
    {
        public List<BattleStepConfig> PvPBattleStrategy = new();
    }

    // -------------------------------------------------------------------------
    // Enums
    // -------------------------------------------------------------------------

    public enum MatchAction
    {
        CreateMatch,
        InstantBattle,
        SaveStrategy,
        GetMyMatches,
        GetAvailableMatches,
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum MatchStatus
    {
        Open,
        InProgress,
        Cancelled,
        Completed,
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum BodyPart
    {
        Head,
        Torso,
        Legs,
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum BattleHitType
    {
        Hit,
        Critical,
        Block,
        Dodge,
    }
}
