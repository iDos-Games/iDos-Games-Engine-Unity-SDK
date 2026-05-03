using System;

namespace IDosGames
{
    public sealed class AuthContext
    {
        public AuthContext() {}

        public string UserID { get; set; }
        public string ClientSessionTicket { get; set; }
        public DateTime ClientSessionTicketExpiration { get; set; }
        
        public bool IsClientLoggedIn()
        {
            return !string.IsNullOrEmpty(ClientSessionTicket);
        }

        public AuthContext(string clientSessionTicket, string userId) : this()
        {
            ClientSessionTicket = clientSessionTicket;
            UserID = userId;
        }
    }
}
