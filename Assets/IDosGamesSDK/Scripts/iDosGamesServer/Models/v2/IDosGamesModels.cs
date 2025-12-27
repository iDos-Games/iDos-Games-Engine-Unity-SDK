using System;

namespace IDosGames
{
    [Serializable]
    public class AuthenticationRequest : IGSRequest {}

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
