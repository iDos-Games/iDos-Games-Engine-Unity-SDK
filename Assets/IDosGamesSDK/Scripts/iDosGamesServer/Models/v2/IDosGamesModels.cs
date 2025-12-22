using System;

namespace IDosGames
{
    [Serializable]
    public class AuthenticationRequest
    {
        public string TitleID { get; set; }

        public string DeviceID { get; set; }
        public string Device { get; set; }
        public string Platform { get; set; }

        public string Email { get; set; }
        public string Password { get; set; }

        public string TelegramInitData { get; set; }

        public string UserID { get; set; }
        public string ClientSessionTicket { get; set; }
        public string ResetToken { get; set; }
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

    public enum AuthenticationAction
    {
        LoginWithDeviceID,
        LoginWithTelegram,
        LoginWithEmail,
        RegisterUserByEmail,
        AddEmailAndPassword,
        ForgotPassword,
        ResetPassword,
    }
}
