namespace IDosGames
{
    public static class IDosGamesData
    {
        public static TitleConfig Config { get; private set; } = new ();
        public static TitleData Title { get; private set; } = new();
        public static UserData User { get; private set; } = new ();

        internal static void Reset()
        {
            Config = new TitleConfig();
            Title = new TitleData();
            User = new UserData();
        }
    }
}
