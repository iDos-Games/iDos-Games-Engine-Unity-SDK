using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace IDosGames
{
    /// <summary>
    /// Request for all GameLoop actions.
    /// Fields used per action:
    ///   BoardLoopRoll        — RollMultiplier, RelatedEntityID
    ///   BoardLoopAttack      — BuildingIndex, RelatedEntityID
    ///   BoardLoopRaid        — DigIndex, RelatedEntityID
    ///   BoardLoopRaidFast    — DigIndices, RelatedEntityID
    ///   BoardLoopBuild       — BuildingIndex, RelatedEntityID
    ///   GetBoardDefinitionForLevel — StageLevel
    ///   GetGameLoops / GetBoardDefinition / GetUserBoardState — no extra fields
    /// </summary>
    [Serializable]
    public class GameLoopRequest : BaseRequest
    {
        /// <summary>
        /// Requested roll multiplier (x1, x5, x10...).
        /// Server normalizes to the nearest allowed value not exceeding dice balance.
        /// </summary>
        public int RollMultiplier { get; set; } = 1;

        /// <summary>
        /// Target building slot index (0..N-1) for Attack or Build actions.
        /// -1 = server picks randomly.
        /// </summary>
        public int BuildingIndex { get; set; } = -1;

        /// <summary>
        /// Cell index to open (0..11) in Sequential raid mode (BoardLoopRaid).
        /// </summary>
        public int DigIndex { get; set; } = 0;

        /// <summary>
        /// Stage level for GetBoardDefinitionForLevel.
        /// </summary>
        public int StageLevel { get; set; } = 1;

        /// <summary>
        /// All cell indices opened locally by the client in Fast raid mode (BoardLoopRaidFast).
        /// </summary>
        public List<int> DigIndices { get; set; }
    }

    /// <summary>Response for BoardLoopRoll.</summary>
    [Serializable]
    public class BoardRollResponse
    {
        /// <summary>Actual multiplier used after normalization.</summary>
        public int UsedMultiplier { get; set; }
        /// <summary>Number of steps rolled on two dice (2..12).</summary>
        public int Steps { get; set; }
        /// <summary>Board position before the roll.</summary>
        public int OldPosition { get; set; }
        /// <summary>Board position after the roll.</summary>
        public int NewPosition { get; set; }
        /// <summary>Number of full laps completed this roll (usually 0 or 1).</summary>
        public long CyclesCompletedDelta { get; set; }
        /// <summary>String representation of the tile type landed on, or RollResultTypes.ShieldRefund.</summary>
        public string LandedTileType { get; set; }
        /// <summary>Full resource operation result (dice consumed, rewards granted).</summary>
        public ResourceOperation Operation { get; set; }
        /// <summary>"ATTACK" or "RAID" if a pending interaction was created; null otherwise.</summary>
        public string ActionRequired { get; set; }
        /// <summary>Target data for the pending interaction. Populated when ActionRequired != null.</summary>
        public RollActionData ActionData { get; set; }
    }

    /// <summary>Response for BoardLoopAttack.</summary>
    [Serializable]
    public class AttackResponse
    {
        /// <summary>Hit or Blocked.</summary>
        public AttackOutcome Outcome { get; set; }
        /// <summary>True if the target was a bot (Operation is populated); false for a real player (DualResult is populated).</summary>
        public bool IsBotTarget { get; set; }
        /// <summary>Index of the building hit (-1 if bot target or no buildings available).</summary>
        public int BuildingIndexHit { get; set; } = -1;
        /// <summary>Resource operation result when target is a bot. Null for real player — use DualResult.</summary>
        public ResourceOperation Operation { get; set; }
        /// <summary>Dual-party result when target is a real player. Null for bot — use Operation.</summary>
        public ResourceDualPartyResult DualResult { get; set; }
    }

    /// <summary>Response for BoardLoopRaid and BoardLoopRaidFast.</summary>
    [Serializable]
    public class RaidResponse
    {
        /// <summary>"CONTINUE", "FINISHED_SMALL", "FINISHED_MEDIUM", "FINISHED_BIG", or "FINISHED_JACKPOT".</summary>
        public string Status { get; set; }
        /// <summary>Final raid outcome tier. Meaningful only when Status starts with "FINISHED_".</summary>
        public RaidOutcome Outcome { get; set; }
        /// <summary>Symbol found in the just-opened cell. HeistSymbol.None in Fast mode.</summary>
        public HeistSymbol FoundSymbol { get; set; }
        /// <summary>Bonus grant from the just-opened cell. Null if no bonus or Fast mode.</summary>
        public ResourceGrant FoundBonus { get; set; }
        /// <summary>Index of the just-opened cell (0..11). -1 in Fast mode.</summary>
        public int OpenedIndex { get; set; }
        /// <summary>Remaining cells available to open. 0 when raid is finished.</summary>
        public int AttemptsLeft { get; set; }
        /// <summary>
        /// Grid layout (12 cells).
        /// Sequential: only opened cells revealed; unopened are masked as HeistSymbol.None.
        /// Fast: null on CONTINUE; full layout on FINISH.
        /// </summary>
        public List<HeistCell> RaidLayout { get; set; }
        /// <summary>Variant tag for UI banners ("classic", "small_bonus", "big_bonus", "jackpot"). Null if no HeistGridBonusConfig.</summary>
        public string HeistVariantTag { get; set; }
        /// <summary>True if this is a jackpot raid.</summary>
        public bool IsJackpot { get; set; }
        /// <summary>Single-user resource operation (bot target or empty victim bank). Null for real P2P steal — use DualResult.</summary>
        public ResourceOperation Operation { get; set; }
        /// <summary>Dual-party result for real P2P steal. Null for bot or empty bank — use Operation.</summary>
        public ResourceDualPartyResult DualResult { get; set; }
    }

    /// <summary>Response for BoardLoopBuild.</summary>
    [Serializable]
    public class BuildResponse
    {
        /// <summary>Index of the building that was upgraded or repaired (0..N-1).</summary>
        public int BuiltIndex { get; set; }
        /// <summary>Building level after the operation.</summary>
        public int NewLevel { get; set; }
        /// <summary>True if this build completed the stage (all buildings maxed and undamaged).</summary>
        public bool StageComplete { get; set; }
        /// <summary>True if the max-level one-time reward was claimed in this operation.</summary>
        public bool MaxLevelRewardClaimed { get; set; }
        /// <summary>Full resource operation (cost consumed, OnBuild/OnStageComplete/MaxLevelReward granted).</summary>
        public ResourceOperation Operation { get; set; }
    }

    /// <summary>Snapshot of an interaction target returned with the roll response.</summary>
    [Serializable]
    public class RollActionData
    {
        /// <summary>UserID of the target (real player or bot ID).</summary>
        public string TargetUserID { get; set; }
        /// <summary>True if the target is a bot.</summary>
        public bool IsBot { get; set; }
        /// <summary>Public profile of the target for UI display.</summary>
        public UserPublicDataModel PublicData { get; set; }
        /// <summary>Snapshot of the target's building states at the time of the roll.</summary>
        public List<BuildingState> TargetBuildingStates { get; set; }
        /// <summary>True if the target has an active shield (attack will result in Blocked).</summary>
        public bool TargetHasShield { get; set; }
    }

    // =====================================================================
    // STATE MODELS
    // =====================================================================

    /// <summary>Root container for all game loop state in UserDataDocument.</summary>
    [Serializable]
    public class UserGameLoopsState
    {
        /// <summary>Board Core Loop state (dice rolls, buildings, attacks, raids).</summary>
        public BoardLoopState Board { get; set; } = new BoardLoopState();
    }

    /// <summary>Persistent board state for a player.</summary>
    [Serializable]
    public class BoardLoopState
    {
        /// <summary>Current stage number, starting from 1.</summary>
        public int StageLevel { get; set; } = 1;
        /// <summary>Current tile index (0..N-1).</summary>
        public int Position { get; set; } = 0;
        /// <summary>Player-specific override for allowed roll multipliers. Empty = use global config.</summary>
        public List<int> AvailableRollMultipliers { get; set; } = new List<int>();
        /// <summary>Building states for the current stage (SlotIndex invariant: BuildingStates[i].SlotIndex == i).</summary>
        public List<BuildingState> BuildingStates { get; set; } = new List<BuildingState>();
        /// <summary>Active pending interaction (attack or raid). Null = free to roll.</summary>
        public BoardPendingInteraction Pending { get; set; }
        /// <summary>Total laps completed (passes through start tile).</summary>
        public long CyclesCompleted { get; set; } = 0;
        /// <summary>UTC time of the last successful roll. Used in OCC filter.</summary>
        public DateTime LastRollAtUtc { get; set; } = DateTime.MinValue;
    }

    /// <summary>State of one building slot on the board.</summary>
    [Serializable]
    public class BuildingState
    {
        /// <summary>Slot index (0..N-1). Matches BuildingDefinition.SlotIndex.</summary>
        public int SlotIndex { get; set; }
        /// <summary>Current level: 0 = not built, MaxLevel = maxed.</summary>
        public int Level { get; set; } = 0;
        /// <summary>True if the building was damaged by an attack. Next build call is a repair.</summary>
        public bool IsDamaged { get; set; } = false;
        /// <summary>True if the max-level one-time reward has already been claimed.</summary>
        public bool MaxLevelRewardClaimed { get; set; } = false;
    }

    /// <summary>Active pending interaction blocking the next roll.</summary>
    [Serializable]
    public class BoardPendingInteraction
    {
        /// <summary>"ATTACK" or "RAID".</summary>
        public string Type { get; set; }
        /// <summary>Target UserID (real player or bot ID).</summary>
        public string TargetUserID { get; set; }
        /// <summary>Public profile snapshot of the target.</summary>
        public UserPublicDataModel TargetPublicData { get; set; } = new UserPublicDataModel();
        /// <summary>Building state snapshot of the target (Attack only).</summary>
        public List<BuildingState> TargetBuildingStates { get; set; } = new List<BuildingState>();
        /// <summary>True if the target had a shield at roll time.</summary>
        public bool TargetHasShield { get; set; }
        /// <summary>Roll multiplier locked at the time of landing on the Railroad tile.</summary>
        public int RollMultiplier { get; set; } = 1;
        /// <summary>Expiry time (UTC). Expired pending is ignored and cleared on next board init.</summary>
        public DateTime ExpiresAtUtc { get; set; }
        /// <summary>Server-side raid grid (12 cells). Masked for Sequential mode before sending to client.</summary>
        public List<HeistCell> RaidLayout { get; set; } = new List<HeistCell>();
        /// <summary>Indices of cells already opened.</summary>
        public List<int> OpenedIndices { get; set; } = new List<int>();
        /// <summary>Variant tag for UI banners. Null if no HeistGridBonusConfig on the stage.</summary>
        public string HeistVariantTag { get; set; }
        /// <summary>True if this is a jackpot raid.</summary>
        public bool IsJackpotRaid { get; set; } = false;
        /// <summary>Jackpot multiplier applied to the stolen bank on finalization. 0 or negative = 1.0.</summary>
        public double JackpotFinalMultiplier { get; set; } = 1.0;
        /// <summary>Pre-rolled guaranteed bonus for this raid variant. Given once on any raid outcome.</summary>
        public ResourceGrant GuaranteedBonus { get; set; }
    }

    /// <summary>One cell in the raid grid.</summary>
    [Serializable]
    public class HeistCell
    {
        /// <summary>Primary symbol used for 3-of-a-kind finish detection.</summary>
        public HeistSymbol Symbol { get; set; }
        /// <summary>Instant bonus grant on opening this cell. Null = plain cell.</summary>
        public ResourceGrant OnOpenBonus { get; set; }
        /// <summary>Bonus tag for analytics and UI.</summary>
        public string BonusTag { get; set; }
    }

    // =====================================================================
    // CONFIG MODELS
    // =====================================================================

    /// <summary>Root config for the GameLoop system in TitlePublicConfigurationModel.</summary>
    [Serializable]
    public class GameLoopDefinitions
    {
        /// <summary>Board Core Loop configuration.</summary>
        public BoardLoopDefinition Board { get; set; } = new BoardLoopDefinition();
    }

    /// <summary>Global board configuration (currencies, multipliers, templates, stages).</summary>
    [Serializable]
    public class BoardLoopDefinition
    {
        /// <summary>Virtual currency used as dice energy. Default "DICE".</summary>
        public string RollCurrencyID { get; set; } = "DICE";
        /// <summary>Virtual currency used as shields. Default "SH".</summary>
        public string ShieldCurrencyID { get; set; } = "SH";
        /// <summary>Raid mini-game mode: Fast or Sequential.</summary>
        public RaidMode RaidMode { get; set; } = RaidMode.Fast;
        /// <summary>Ordered list of allowed roll multiplier values (e.g. [1, 5, 10, 20, 50, 100]).</summary>
        public List<int> AllowedRollMultipliers { get; set; } = new List<int> { 1, 5, 10, 20, 50, 100 };
        /// <summary>Tile ring templates keyed by template ID.</summary>
        public Dictionary<string, BoardTemplateDefinition> BoardTemplatesByID { get; set; } = new Dictionary<string, BoardTemplateDefinition>();
        /// <summary>Stage configs keyed by level as string ("1", "2", ...). Fallback: nearest lower, then minimum.</summary>
        public Dictionary<string, BoardStageDefinition> StagesByLevel { get; set; } = new Dictionary<string, BoardStageDefinition>();
    }

    /// <summary>A fixed ring of tiles shared across stages.</summary>
    [Serializable]
    public class BoardTemplateDefinition
    {
        /// <summary>Tile ring keyed by index as string.</summary>
        public Dictionary<string, BoardTileDefinition> Tiles { get; set; } = new Dictionary<string, BoardTileDefinition>();
    }

    /// <summary>Configuration for one stage (location) on the board.</summary>
    [Serializable]
    public class BoardStageDefinition
    {
        /// <summary>Display name of the stage.</summary>
        public string Name { get; set; }
        /// <summary>Asset paths keyed by asset type.</summary>
        public Dictionary<string, string> AssetPaths { get; set; }
        /// <summary>ID of the tile template from BoardLoopDefinition.BoardTemplatesByID.</summary>
        public string BoardTemplateID { get; set; }
        /// <summary>Building definitions for this stage (usually 5 slots).</summary>
        public List<BuildingDefinition> Buildings { get; set; } = new List<BuildingDefinition>();
        /// <summary>Exponential cost growth factor per building level. Default 1.25.</summary>
        public double CostGrowthFactor { get; set; } = 1.25;
        /// <summary>Base resource grant for a successful attack (before multipliers).</summary>
        public ResourceGrant BaseAttackReward { get; set; }
        /// <summary>Base "victim bank" for a raid (before multipliers, clipped by victim balance).</summary>
        public ResourceGrant BaseRaidReward { get; set; }
        /// <summary>Max allowed roll multiplier on this stage. 0 or negative = 100.</summary>
        public int MaxRollMultiplier { get; set; } = 100;
        /// <summary>Max shields a player can hold on this stage. Overflow converts to dice. 0 or negative = 3.</summary>
        public int MaxShields { get; set; } = 3;
        /// <summary>Raid grid variant/bonus config. Null = all raids are classic (no bonuses or jackpot).</summary>
        public HeistGridBonusConfig HeistGridBonusConfig { get; set; }
        /// <summary>Resource operations for all board events on this stage.</summary>
        public StageOperations StageOperations { get; set; }
    }

    /// <summary>Configuration of raid grid variant selection for a stage.</summary>
    [Serializable]
    public class HeistGridBonusConfig
    {
        /// <summary>Weighted list of raid variants. One is chosen randomly per raid creation.</summary>
        public List<HeistRaidVariant> Variants { get; set; } = new List<HeistRaidVariant>();
    }

    /// <summary>One raid layout variant with bonus density, bonus pool, guaranteed bonus, and jackpot params.</summary>
    [Serializable]
    public class HeistRaidVariant
    {
        /// <summary>Weight for variant selection. Must be > 0 to participate.</summary>
        public int Weight { get; set; } = 1;
        /// <summary>Variant tag for analytics and UI banners ("classic", "small_bonus", "big_bonus", "jackpot").</summary>
        public string Tag { get; set; }
        /// <summary>Minimum number of bonus cells in this variant (0..12).</summary>
        public int MinBonusCells { get; set; } = 0;
        /// <summary>Maximum number of bonus cells (0..12). Must be >= MinBonusCells.</summary>
        public int MaxBonusCells { get; set; } = 0;
        /// <summary>Weighted bonus pool. One entry is chosen per bonus cell independently.</summary>
        public List<WeightedHeistCellBonus> BonusPool { get; set; } = new List<WeightedHeistCellBonus>();
        /// <summary>Pre-rolled guaranteed bonus given once on any raid outcome finalization. Null = none.</summary>
        public ScaledResourceOperation GuaranteedBonus { get; set; }
        /// <summary>True = jackpot variant. Outcome is always RaidOutcome.Jackpot; JackpotFinalMultiplier is applied to stolen bank.</summary>
        public bool IsJackpot { get; set; } = false;
        /// <summary>Multiplier applied to the entire stolen bank in jackpot mode. 0 or negative = 1.0.</summary>
        public double JackpotFinalMultiplier { get; set; } = 1.0;
        /// <summary>Symbol distribution override for jackpot layout. Must sum to 12; otherwise falls back to 4/4/4. Null = standard 4/4/4.</summary>
        public Dictionary<HeistSymbol, int> JackpotSymbolDistribution { get; set; }
    }

    /// <summary>One bonus entry with weight for selection from HeistRaidVariant.BonusPool.</summary>
    [Serializable]
    public class WeightedHeistCellBonus
    {
        /// <summary>Selection weight. Must be > 0.</summary>
        public int Weight { get; set; } = 1;
        /// <summary>Bonus operation. Only Grant is used; Consume is ignored in this context.</summary>
        public ScaledResourceOperation Bonus { get; set; }
        /// <summary>Tag for analytics and UI ("small_token_bag", "big_token_bag", "shield_drop").</summary>
        public string Tag { get; set; }
    }

    /// <summary>All resource operations for board events on a stage.</summary>
    [Serializable]
    public class StageOperations
    {
        /// <summary>Operation on passing the Start tile. Applied lapsDelta times per roll.</summary>
        public ScaledResourceOperation OnPassStart { get; set; }
        /// <summary>Operations per tile type on landing. Missing key = no resources for that tile.</summary>
        public Dictionary<BoardTileType, ScaledResourceOperation> OnTileLanding { get; set; }
        /// <summary>Operations per attack outcome (Hit, Blocked). Granted in addition to BaseAttackReward.</summary>
        public Dictionary<AttackOutcome, ScaledResourceOperation> OnAttack { get; set; }
        /// <summary>Operations per raid outcome (Small, Medium, Big, Jackpot). Granted in addition to stolen bank.</summary>
        public Dictionary<RaidOutcome, ScaledResourceOperation> OnRaid { get; set; }
        /// <summary>Operation on any building upgrade or repair.</summary>
        public ScaledResourceOperation OnBuild { get; set; }
        /// <summary>Operation on stage completion (all buildings maxed and undamaged). Granted once.</summary>
        public ScaledResourceOperation OnStageComplete { get; set; }
        /// <summary>Independent probabilistic bonus drops on attack. Each entry is rolled separately.</summary>
        public List<AttackBonusDrop> OnAttackBonusDrops { get; set; }
    }

    /// <summary>One independent probabilistic bonus drop on attack.</summary>
    [Serializable]
    public class AttackBonusDrop
    {
        /// <summary>Trigger probability (0.0..1.0). Each drop is rolled independently.</summary>
        public double Chance { get; set; }
        /// <summary>Optional outcome filter. Null = triggers on any outcome.</summary>
        public AttackOutcome? RequiredOutcome { get; set; }
        /// <summary>Bonus content (usually Grant.Standard.EventTokens).</summary>
        public ScaledResourceOperation Reward { get; set; }
        /// <summary>Tag for tracing/analytics.</summary>
        public string Tag { get; set; }
    }

    /// <summary>One tile on the board ring.</summary>
    [Serializable]
    public class BoardTileDefinition
    {
        /// <summary>Tile index in the ring (0..N-1).</summary>
        public int Index { get; set; }
        /// <summary>Tile type — determines the mechanic triggered on landing.</summary>
        public BoardTileType Type { get; set; } = BoardTileType.Empty;
        /// <summary>Weight of the Attack option for RandomAction tiles.</summary>
        public int RandomActionAttackWeight { get; set; } = 50;
        /// <summary>Weight of the Raid option for RandomAction tiles.</summary>
        public int RandomActionRaidWeight { get; set; } = 50;
        /// <summary>Arbitrary string params for A/B testing and event extensions.</summary>
        public Dictionary<string, string> Params { get; set; } = new Dictionary<string, string>();
    }

    /// <summary>Building definition for one slot in a stage.</summary>
    [Serializable]
    public class BuildingDefinition
    {
        /// <summary>Slot index (0..N-1). Matches BuildingState.SlotIndex.</summary>
        public int SlotIndex { get; set; }
        /// <summary>Display name.</summary>
        public string Name { get; set; }
        /// <summary>Asset paths keyed by asset type.</summary>
        public Dictionary<string, string> AssetPaths { get; set; }
        /// <summary>Base upgrade cost (level 0 → 1). Higher levels scale via CostGrowthFactor.</summary>
        public ResourceConsume BaseBuildCost { get; set; }
        /// <summary>Maximum building level (inclusive). 0 or negative = 5.</summary>
        public int MaxLevel { get; set; } = 5;
        /// <summary>One-time reward on first reaching MaxLevel.</summary>
        public ResourceGrant MaxLevelReward { get; set; }
    }

    /// <summary>ResourceOperation wrapper with a roll-multiplier scaling flag.</summary>
    [Serializable]
    public class ScaledResourceOperation
    {
        /// <summary>The resource operation (grant/consume) before scaling.</summary>
        public ResourceOperation Operation { get; set; } = new ResourceOperation();
        /// <summary>If true, all amounts are multiplied by RollMultiplier before applying.</summary>
        public bool ScaleWithRollMultiplier { get; set; } = true;
    }

    // =====================================================================
    // ENUMS
    // =====================================================================

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
    public enum RaidMode
    {
        Fast,
        Sequential,
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
        EventToken,
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum AttackOutcome
    {
        Hit,
        Blocked,
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum RaidOutcome
    {
        Small,
        Medium,
        Big,
        Jackpot,
    }

    public enum HeistSymbol
    {
        None = 0,
        Small = 1,
        Medium = 2,
        Big = 3,
    }
}
