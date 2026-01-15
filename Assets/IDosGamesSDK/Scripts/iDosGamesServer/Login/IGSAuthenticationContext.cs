using System;

namespace IDosGames
{
    public sealed class IGSAuthenticationContext
    {
        public IGSAuthenticationContext() {}

        public string ClientSessionTicket { get; set; }
        public DateTime ClientSessionTicketExpiration { get; set; }
        public string UserID { get; set; }
        public string EntityToken { get; set; }
        public string EntityId { get; set; }
        public string EntityType { get; set; }
        public string TelemetryKey { get; set; }

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
    }
}
