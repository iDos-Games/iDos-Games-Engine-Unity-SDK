using System;

namespace IDosGames
{
    [Serializable]
    public class AuthenticationRequest : IGSRequest 
    {
        public string PlatformAuthToken { get; set; }
        public string PlatformRefreshToken { get; set; }
    }

    [Serializable]
    public class OperationResult<T>
    {
        public bool Success { get; set; }
        public string Error { get; set; }
        public T Data { get; set; }
        public bool IsThrottled { get; set; }

        public static OperationResult<T> Ok(T data)
            => new OperationResult<T> { Success = true, Data = data, Error = null };

        public static OperationResult<T> Fail(string error)
            => new OperationResult<T> { Success = false, Data = default, Error = error };

        public static OperationResult<T> Throttled()
            => new() { Success = false, Error = "Throttled", IsThrottled = true };
    }

    [Serializable]
    public class SuccessResponse
    {
        public bool IsCompleted { get; set; }
        public DateTime ServerTime { get; set; }
    }

    [Serializable]
    public class PlatformLoginResponse
    {
        public string PlatformUserID { get; set; }
        public string PlatformAuthToken { get; set; }
        public DateTime? PlatformAuthTokenExpiration { get; set; }

        public string TitleUserID { get; set; }
        public string TitleClientSessionTicket { get; set; }
        public DateTime TitleClientSessionTicketExpiration { get; set; }
    }

    [Serializable]
    public class PlatformUser
    {
        public string PlatformUserID { get; set; }
        public string PublisherID { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public string AvatarUrl { get; set; }
        public string AuthToken { get; set; }
        public string AuthTokenExpiration { get; set; }
    }

    [Serializable]
    public class PlatformAuthResponse
    {
        public string type;
        public string requestId;
        public bool ok;
        public string error;
        public PlatformUser user;
    }

    public enum AuthenticationAction
    {
        // FULL (tokens + all data)
        LoginWithDeviceID,
        LoginWithTelegram,
        LoginWithEmail,
        LoginWithGoogle,
        LoginWithPlatformToken,
        RegisterWithEmail,

        // TOKENS ONLY
        LoginTokensWithDeviceID,
        LoginTokensWithTelegram,
        LoginTokensWithEmail,
        LoginTokensWithGoogle,
        LoginTokensWithPlatformToken,
        RegisterTokensWithEmail,

        ForgotPassword,
        ResetPassword,
        RefreshPlatformToken,
    }
}
