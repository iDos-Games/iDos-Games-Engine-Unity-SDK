// File: Assets/IDosGamesSDK/Runtime/UI/Shared/TimeFormatUtil.cs
using System;

namespace IDosGames.UI.Shared
{
    public static class TimeFormatUtil
    {
        public static string FormatRelativeTime(DateTime utcTime)
        {
            var span = DateTime.UtcNow - utcTime;

            if (span.TotalSeconds < 60) return "just now";
            if (span.TotalMinutes < 60) return $"{(int)span.TotalMinutes}m ago";
            if (span.TotalHours < 24) return $"{(int)span.TotalHours}h ago";
            if (span.TotalDays < 7) return $"{(int)span.TotalDays}d ago";
            return utcTime.ToString("MMM dd, yyyy");
        }

        public static string FormatNumber(long value)
        {
            return value.ToString("N0");
        }
    }
}
