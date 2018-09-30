using System;

namespace Portal2Boards.Extensions
{
    public static class Portal2BoardsExtensions
    {
        public static float? AsTime(this uint? time)
            => (time != default) ? (float)Math.Round((float)time / 100, 2) : default;
        public static float AsTime(this uint time)
            => (time != default) ? (float)Math.Round((float)time / 100, 2) : default;

        public static string AsTimeToString(this uint? time)
        {
            if (time == default)
                return default;

            var centi = time % 100;
            var totalsec = Math.Floor((decimal)time / 100);
            var sec = totalsec % 60;
            var min = Math.Floor(totalsec / 60);
            return (min > 0)
                ? $"{min}:{((sec < 10) ? $"0{sec}" : $"{sec}")}.{((centi < 10) ? $"0{centi}" : $"{centi}")}"
                : $"{sec}.{((centi < 10) ? $"0{centi}" : $"{centi}")}";
        }
        public static string AsTimeToString(this uint time)
        {
            if (time == default)
                return default;

            var centi = time % 100;
            var totalsec = Math.Floor((decimal)time / 100);
            var sec = totalsec % 60;
            var min = Math.Floor(totalsec / 60);
            return (min > 0)
                ? $"{min}:{((sec < 10) ? $"0{sec}" : $"{sec}")}.{((centi < 10) ? $"0{centi}" : $"{centi}")}"
                : $"{sec}.{((centi < 10) ? $"0{centi}" : $"{centi}")}";
        }

        public static string DateTimeToString(this DateTime? date)
            => (date != default) ? date?.ToString("yyyy-MM-dd HH:mm:ss") : "Unknown";
        public static string DateTimeToString(this DateTime date)
            => (date != default) ? date.ToString("yyyy-MM-dd HH:mm:ss") : "Unknown";
    }
}
