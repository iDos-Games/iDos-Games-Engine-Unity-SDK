
namespace IDosGames
{
    public sealed class IGSAuthenticationContext
    {
        public IGSAuthenticationContext()
        {
        }

        public string ClientSessionTicket;
        public string UserID;
        public string EntityToken;
        public string EntityId;
        public string EntityType;
        public string TelemetryKey;
        public bool IsClientLoggedIn()
        {
            return !string.IsNullOrEmpty(ClientSessionTicket);
        }

        public IGSAuthenticationContext(string clientSessionTicket, string entityToken, string userId, string entityId, string entityType, string telemetryKey = null) : this()
        {
            ClientSessionTicket = clientSessionTicket;
            UserID = userId;
            EntityToken = entityToken;
            EntityId = entityId;
            EntityType = entityType;
            TelemetryKey = telemetryKey;
        }

        public void ForgetAllCredentials()
        {
            UserID = null;
            ClientSessionTicket = null;
            EntityToken = null;
            EntityId = null;
            EntityType = null;
            TelemetryKey = null;
        }
    }
}
