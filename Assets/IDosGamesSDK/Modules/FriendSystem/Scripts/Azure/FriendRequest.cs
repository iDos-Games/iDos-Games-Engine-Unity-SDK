using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace IDosGames.Friends
{
    public class FriendRequest
    {
        public string TitleID { get; set; }
        public string FriendID { get; set; }
        public string UserID { get; set; }
        public string ClientSessionTicket { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public FriendActionType Action { get; set; }
    }

}