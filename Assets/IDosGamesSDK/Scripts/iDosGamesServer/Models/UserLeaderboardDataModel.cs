using IDosGames.TitlePublicConfiguration;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace IDosGames
{
    public class PlayerLeaderboardData
    {
        public int StatValue { get; set; }
        public int Version { get; set; }
        public int PendingRewardVersion { get; set; } = 0;

        [JsonConverter(typeof(StringEnumConverter))]
        public StatisticType StatisticType { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public StatisticResetFrequency StatisticResetFrequency { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public StatisticAggregationMethod StatisticAggregationMethod { get; set; }
        public DateTime? NextReset { get; set; }
    }

    public class UserLeaderboardRewards
    {
        public int Position { get; set; }
        public List<ItemOrCurrency> ItemsToGrant { get; set; }
    }
}
