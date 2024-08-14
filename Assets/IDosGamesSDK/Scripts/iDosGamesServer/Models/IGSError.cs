using System;
using System.Collections.Generic;
using System.Text;

namespace IDosGames
{
    public enum IGSErrorCode
    {
        Unknown = 1,
        ConnectionError = 2,
        JsonParseError = 3,
        Success = 0,
        UnkownError = 500,
        InvalidParams = 1000,
        AccountNotFound = 1001,
        AccountBanned = 1002,
        InvalidUsernameOrPassword = 1003,
        InvalidTitleId = 1004,
        InvalidEmailAddress = 1005,
        EmailAddressNotAvailable = 1006,
        InvalidUsername = 1007,
        InvalidPassword = 1008,
        UsernameNotAvailable = 1009,
    }

    public class IGSError
    {
        public string ApiEndpoint;
        public int HttpCode;
        public string HttpStatus;
        public IGSErrorCode Error;
        public string ErrorMessage;
        public Dictionary<string, List<string>> ErrorDetails;
        public object CustomData;
        public uint? RetryAfterSeconds = null;

        public override string ToString()
        {
            return GenerateErrorReport();
        }

        [ThreadStatic]
        private static StringBuilder _tempSb;
        
        public string GenerateErrorReport()
        {
            if (_tempSb == null)
                _tempSb = new StringBuilder();
            _tempSb.Length = 0;
            if (String.IsNullOrEmpty(ErrorMessage))
            {
                _tempSb.Append(ApiEndpoint).Append(": ").Append("Http Code: ").Append(HttpCode.ToString()).Append("\nHttp Status: ").Append(HttpStatus).Append("\nError: ").Append(Error.ToString()).Append("\n");
            }
            else
            {
                _tempSb.Append(ApiEndpoint).Append(": ").Append(ErrorMessage);
            }

            if (ErrorDetails != null)
                foreach (var pair in ErrorDetails)
                    foreach (var msg in pair.Value)
                        _tempSb.Append("\n").Append(pair.Key).Append(": ").Append(msg);
            return _tempSb.ToString();
        }
    }

    public class IGSException : Exception
    {
        public readonly IGSExceptionCode Code;
        public IGSException(IGSExceptionCode code, string message) : base(message)
        {
            Code = code;
        }
    }

    public enum IGSExceptionCode
    {
        AuthContextRequired,
        BuildError,
        DeveloperKeyNotSet,
        EntityTokenNotSet,
        NotLoggedIn,
        TitleNotSet,
    }
}
