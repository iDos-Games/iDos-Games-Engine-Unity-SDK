using System;

namespace IDosGames
{
    public sealed class AuthContext
    {
        public AuthContext() {}

        public string UserID { get; set; }
        public string ClientSessionTicket { get; set; }
        public DateTime ClientSessionTicketExpiration { get; set; }

        public string PlatformUserID { get; set; }
        public string PlatformAuthToken { get; set; }
        public DateTime? PlatformAuthTokenExpiration { get; set; }

        public bool IsClientLoggedIn()
        {
            return !string.IsNullOrEmpty(ClientSessionTicket);
        }

        public AuthContext(string userID, string clientSessionTicket, DateTime clientSessionTicketExpiration, string platformUserID, string platformAuthToken, DateTime? platformAuthTokenExpiration) : this()
        {
            UserID = userID;
            ClientSessionTicket = clientSessionTicket;
            ClientSessionTicketExpiration = clientSessionTicketExpiration;
            PlatformUserID = platformUserID;
            PlatformAuthToken = platformAuthToken;
            PlatformAuthTokenExpiration = platformAuthTokenExpiration;
        }
    }
}
