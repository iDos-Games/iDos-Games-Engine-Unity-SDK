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

        public static OperationResult<T> Ok(T data)
            => new OperationResult<T> { Success = true, Data = data, Error = null };

        public static OperationResult<T> Fail(string error)
            => new OperationResult<T> { Success = false, Data = default, Error = error };
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
        public string PlatformRefreshToken { get; set; }
        public DateTime? PlatformRefreshTokenExpiration { get; set; }

        public string TitleUserID { get; set; }
        public string TitleClientSessionTicket { get; set; }
        public DateTime TitleClientSessionTicketExpiration { get; set; }
    }

    public enum AuthenticationAction
    {
        LoginWithDeviceID,
        LoginWithTelegram,
        LoginWithEmail,
        RegisterUserByEmail,
        ForgotPassword,
        ResetPassword,
        LoginOrRegisterWithPlatformEmail,
        LoginWithPlatformToken,
        RefreshPlatformToken,
    }
}
