using System;

namespace IDosGames.UI.LimitedTimeEvent
{
    /// <summary>
    /// Utility for formatting DateTime values into human-readable countdown strings.
    /// </summary>
    public static class TimeFormatUtil
    {
        /// <summary>
        /// Returns countdown string like "8D 18H", "2H 35M", "45S".
        /// </summary>
        public static string FormatCountdown(DateTime endUtc)
        {
            var remaining = endUtc - DateTime.UtcNow;

            if (remaining.TotalSeconds <= 0) return "Ended";

            if (remaining.TotalDays >= 1)
                return $"{(int)remaining.TotalDays}D {remaining.Hours}H";

            if (remaining.TotalHours >= 1)
                return $"{(int)remaining.TotalHours}H {remaining.Minutes}M";

            if (remaining.TotalMinutes >= 1)
                return $"{(int)remaining.TotalMinutes}M {remaining.Seconds}S";

            return $"{(int)remaining.TotalSeconds}S";
        }

        /// <summary>
        /// "2 min ago", "3 hours ago", "Yesterday", short date.
        /// </summary>
        public static string FormatRelative(DateTime utc)
        {
            var elapsed = DateTime.UtcNow - utc;

            if (elapsed.TotalSeconds < 60)   return "Just now";
            if (elapsed.TotalMinutes < 60)   return $"{(int)elapsed.TotalMinutes} min ago";
            if (elapsed.TotalHours   < 24)   return $"{(int)elapsed.TotalHours} hr ago";
            if (elapsed.TotalDays    < 2)    return "Yesterday";
            if (elapsed.TotalDays    < 7)    return $"{(int)elapsed.TotalDays} days ago";

            return utc.ToLocalTime().ToString("MMM d");
        }

        /// <summary>
        /// Formats large numbers with separators: 1,234,567.
        /// </summary>
        public static string FormatNumber(long value) => value.ToString("N0");
    }
}
