using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using IDosGames.TitlePublicConfiguration;

namespace IDosGames.ClientModels
{
    // =================================================================================
    // ENUMS
    // =================================================================================

    public enum GameLoopAction
    {
        GetGameLoops,
        GetBoardDefinition,
        GetBoardDefinitionForLevel,
        GetUserBoardState,
        BoardLoopRoll,
        BoardLoopAttack,
        BoardLoopRaid,
        BoardLoopRaidFast,
        BoardLoopBuild,
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum BoardTileType
    {
        Empty,
        Reward,
        Chance,
        RandomAction,
        Attack,
        Raid,
        Shield,
        EventToken
    }

    public enum HeistSymbol
    {
        None = 0,
        Small = 1,
        Medium = 2,
        Big = 3
    }

    // =================================================================================
    // REQUESTS
    // =================================================================================

    [Serializable]
    public class GameLoopRequest : IGSRequest
    {
        // ✅ Board
        public int RollMultiplier { get; set; }    // x1/x5/x10...
        public int BuildingIndex { get; set; }    // attack/build
        public int DigIndex { get; set; }          // RAID (0..8)
        public int StageLevel { get; set; }        // for GetBoardDefinitionForLevel
        public List<int> DigIndices { get; set; }
    }

    // =================================================================================
    // USER STATE (RESPONSES)
    // =================================================================================

    [Serializable]
    public class BoardLoopState
    {
        public int StageLevel { get; set; }
        public int Position { get; set; }

        public List<BuildingState> BuildingStates { get; set; }
        public BoardPendingInteraction Pending { get; set; }

        public long CyclesCompleted { get; set; }
        public DateTime LastRollAtUtc { get; set; }
    }

    [Serializable]
    public class BuildingState
    {
        public int SlotIndex { get; set; }
        public int Level { get; set; }
        public bool IsDamaged { get; set; }
        public bool MaxLevelRewardClaimed { get; set; }
    }

    [Serializable]
    public class BoardPendingInteraction
    {
        /// <summary>"ATTACK" or "RAID" (or "RAID_FINISHING" server-side)</summary>
        public string Type { get; set; }

        public string TargetUserID { get; set; }
        public UserPublicDataModel TargetPublicData { get; set; }
        public List<BuildingState> TargetBuildingStates { get; set; }
        public bool TargetHasShield { get; set; }

        public int RollMultiplier { get; set; }
        public DateTime ExpiresAtUtc { get; set; }

        // RAID mini-game (server sanitizes layout in GetUserBoardState; can still be present in Raid response)
        public List<HeistSymbol> RaidLayout { get; set; }
        public List<int> OpenedIndices { get; set; }
    }

    [Serializable]
    public class RollActionData
    {
        public string TargetUserID { get; set; }
        public bool IsBot { get; set; }
        public UserPublicDataModel PublicData { get; set; }
        public List<BuildingState> TargetBuildingStates { get; set; }
        public bool TargetHasShield { get; set; }
    }

    [Serializable]
    public class BoardRollResponse
    {
        public int UsedMultiplier { get; set; }
        public int Steps { get; set; }

        public int OldPosition { get; set; }
        public int NewPosition { get; set; }
        public long CyclesCompletedDelta { get; set; }

        public string LandedTileType { get; set; }

        public List<ItemOrCurrency> GrantedRewards { get; set; }
        public List<ItemOrCurrency> ConsumedResources { get; set; }

        public string ActionRequired { get; set; } // "ATTACK" / "RAID" / null
        public RollActionData ActionData { get; set; }
    }

    [Serializable]
    public class AttackResponse
    {
        public string Status { get; set; } // "HIT", "BLOCKED"
        public ItemOrCurrency RewardResource { get; set; }
    }

    [Serializable]
    public class RaidResponse
    {
        public string Status { get; set; } // "CONTINUE", "FINISHED_SMALL", ...
        public HeistSymbol FoundSymbol { get; set; }
        public ItemOrCurrency StolenResource { get; set; }
        public int OpenedIndex { get; set; }
        public int AttemptsLeft { get; set; }

        public List<HeistSymbol> RaidLayout { get; set; }
    }

    [Serializable]
    public class BuildResponse
    {
        public int BuiltIndex { get; set; }
        public int NewLevel { get; set; }
        public bool StageComplete { get; set; }

        public ItemOrCurrency ConsumedResource { get; set; }
        public List<ItemOrCurrency> CompletionReward { get; set; }
        public List<ItemOrCurrency> MaxLevelReward { get; set; }
        public bool MaxLevelRewardClaimed { get; set; }
    }
}
