namespace IDosGames
{
    public class BaseRequest
    {
        public string TitleID { get; set; }
        public string TitleTemplateID { get; set; }
        public string UserID { get; set; }
        public string Username { get; set; }
        public string Platform { get; set; }
        public string Device { get; set; }
        public string DeviceID { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string ClientSessionTicket { get; set; }
        public string ResetToken { get; set; }
        public string SecretKey { get; set; }
        public string BuildKey { get; set; }
        public bool DevBuild { get; set; }
        public string WebAppLink { get; set; }
        public int UsageTime { get; set; }
        public string TelegramInitData { get; set; }
    }
}
