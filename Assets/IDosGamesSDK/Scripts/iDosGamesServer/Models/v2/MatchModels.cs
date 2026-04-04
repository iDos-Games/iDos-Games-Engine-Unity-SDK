using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace IDosGames.ClientModels
{
    // =================================================================================
    // ENUMS
    // =================================================================================

    [JsonConverter(typeof(StringEnumConverter))]
    public enum MatchStatus
    {
        Open,
        Matched,
        InProgress,
        Finished,
        Cancelled,
        Expired
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum MatchAction
    {
        CreateMatch,
        InstantBattle,
        SaveStrategy,
        GetMyMatches,
        GetAvailableMatches,
        
        JoinMatch,
        GetMatch,
        SubmitAction,
        FinalizeMatch,
        ClaimRewards
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum BattleHitType
    {
        Hit,
        Critical,
        Block,
        Dodge
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum BodyPart
    {
        Head,
        Torso,
        Legs,
    }

    // =================================================================================
    // REQUESTS
    // =================================================================================

    [Serializable]
    public class MatchRequest : IGSRequest
    {
        public string MatchID;
        public string TargetUserID;
        public string CurrencyID;
        public long EntryFeeAmount;
        public List<BattleStepConfig> BattleStrategy;
        public string CharacterID;
        public string RuleID;

        // Paging & Filters
        public int Page = 0;
        public int PageSize = 20;
        public List<string> Statuses;
        public long? MinEntryFeeAmount;
        public long? MaxEntryFeeAmount;
        public bool OnlyPublic = false;
    }

    // =================================================================================
    // RESPONSES
    // =================================================================================

    [Serializable]
    public class CreateMatchResponse
    {
        public MatchStatus Status;
        public string MatchID;
        public string RewardPoolCurrencyID;
        public long RewardPoolAmount;
    }

    [Serializable]
    public class MatchesPageResponse
    {
        public List<PvPMatch> Matches;
        public int Page;
        public int PageSize;
        public bool HasMore;
    }

    [Serializable]
    public class SuccessResponse
    {
        public bool Success = true;
    }

    // =================================================================================
    // DATA MODELS
    // =================================================================================

    [Serializable]
    public class PvPMatch
    {
        public string MatchID;
        public string CreatorID;
        public string CurrencyID;
        public long EntryFeeAmount;
        public MatchStatus Status;
        public long RewardPoolAmount;
        public string CreatorCharacterID;
        public List<BattleStepConfig> CreatorStrategy;
        public DateTime CreatedAt;
        public DateTime? ExpiresAt;
    }

    [Serializable]
    public class BattleResult
    {
        public string WinnerUserID;
        public string LoserUserID;
        public string CurrencyID;
        public long PrizeAmount;
        public bool IsDraw;

        public List<BattleLogEntry> BattleLog = new();

        public PlayerBattleProfile P1BattleProfile { get; set; }
        public PlayerBattleProfile P2BattleProfile { get; set; }
    }

    [Serializable]
    public class PlayerBattleProfile
    {
        public string UserID { get; set; }
        public string SelectedCharacterID { get; set; }
        public Dictionary<string, CharacterModel> Characters { get; set; }
        public List<BattleStepConfig> BattleStrategy { get; set; }
    }

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

    [Serializable]
    public class BattleStepConfig
    {
        public BodyPart AttackTarget;
        public BodyPart DefenseTarget;
    }

    [Serializable]
    public class FighterStats
    {
        public double MaxHp;
        public double CurrentHp;
        public double Damage;
        public double AttackSpeed;
        public double CritChance;
        public double CritMultiplier;
        public double Armor;
        public double DodgeChance;
    }
}
