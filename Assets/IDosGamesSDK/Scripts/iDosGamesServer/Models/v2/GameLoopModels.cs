using System.Collections.Generic;
using System;

namespace IDosGames.ServerModels
{
    // =================================================================================
    // REQUESTS
    // =================================================================================

    [Serializable]
    public class GameLoopRequest
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
    public class SpinResponse
    {
        public string Symbol { get; set; }
        public int Multiplier { get; set; }
        public long RewardAmount { get; set; }
        public string RewardType { get; set; }
        public string ActionRequired { get; set; }
        public SpinActionResponse ActionData { get; set; }
    }

    [Serializable]
    public class SpinActionResponse
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
        Spin,
        Attack,
        Raid,
        Build
    }
}
