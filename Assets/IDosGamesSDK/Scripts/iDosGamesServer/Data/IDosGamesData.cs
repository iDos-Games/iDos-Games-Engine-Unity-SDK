namespace IDosGames
{
    public static class IDosGamesData
    {
        public static TitleConfig Config { get; private set; } = new ();
        public static UserData User { get; private set; } = new ();

        public static bool IsUserLoggedIn => User.IsLoggedIn;

        internal static void OnUserLoggedIn()
        {
            User.IsLoggedIn = true;
        }

        internal static void OnUserLoggedOut()
        {
            User.Clear();
        }

        internal static void Reset()
        {
            Config = new TitleConfig();
            User = new UserData();
        }
    }
}
