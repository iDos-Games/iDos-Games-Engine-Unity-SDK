using System;

namespace IDosGames
{
    [Serializable]
    public class ClientState
    {
        public TitlePublicConfigurationModel Title { get; set; }
        public UserState User { get; set; }
    }
}
