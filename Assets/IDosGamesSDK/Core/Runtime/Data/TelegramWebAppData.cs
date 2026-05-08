using System;

namespace IDosGames
{
    [Serializable]
    public class InitData
    {
        public string query_id;
        public WebAppUser user;
        public WebAppUser receiver;
        public WebAppChat chat;
        public string chat_type;
        public string chat_instance;
        public string start_param;
        public int can_send_after;
        public int auth_date;
        public string hash;
    }

    [Serializable]
    public class WebAppUser
    {
        public long id;
        public bool is_bot;
        public string first_name;
        public string last_name;
        public string username;
        public string language_code;
        public bool is_premium;
        public bool added_to_attachment_menu;
        public bool allows_write_to_pm;
        public string photo_url;
    }

    [Serializable]
    public class WebAppChat
    {
        public long id;
        public string type;
        public string title;
        public string username;
        public string photo_url;
    }
}
