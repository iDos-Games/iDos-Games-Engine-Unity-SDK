using System.Collections.Generic;
using System;

namespace IDosGames.ServerModels
{
    // =================================================================================
    // REQUESTS
    // =================================================================================

    [Serializable]
    public class GameLoopRequest : IGSRequest
    {
        public string LootboxID { get; set; }
        public int Multiplier { get; set; } = 1;
        public string TargetUserID { get; set; }
        public int BuildingIndex { get; set; }
        public int DigIndex { get; set; }
    }

    // =================================================================================
    // RESPONSES
    // =================================================================================

    [Serializable]
    public class RollResponse
    {
        public string Symbol { get; set; }
        public int Multiplier { get; set; }
        public long RewardAmount { get; set; }
        public string RewardType { get; set; }
        public string ActionRequired { get; set; }
        public RollActionResponse ActionData { get; set; }
    }

    [Serializable]
    public class RollActionResponse
    {
        public string TargetUserID { get; set; }
        public string TargetName { get; set; }
        public string TargetAvatar { get; set; }
        public string TargetCountry { get; set; }
        public bool IsBot { get; set; }
    }

    [Serializable]
    public class AttackResponse
    {
        public string Status { get; set; }
        public int Reward { get; set; }
    }

    [Serializable]
    public class RaidResponse
    {
        public string Status { get; set; }
        public long FoundAmount { get; set; }
        public long TotalStolen { get; set; }
        public int OpenedIndex { get; set; }
        public int AttemptsLeft { get; set; }
        public List<long> RaidLayout { get; set; }
    }

    [Serializable]
    public class BuildResponse
    {
        public int BuiltIndex { get; set; }
        public int NewLevel { get; set; }
        public bool MapComplete { get; set; }
        public object BonusReward { get; set; }
    }

    public enum GameLoopAction
    {
        RaidBuildLoopRoll,
        RaidBuildLoopAttack,
        RaidBuildLoopRaid,
        RaidBuildLoopBuild
    }
}
