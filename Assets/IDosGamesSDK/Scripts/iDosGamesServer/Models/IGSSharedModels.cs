using Newtonsoft.Json;

namespace IDosGames.SharedModels
{
    public class HttpResponseObject
    {
        public int code;
        public string status;
        public object data;
    }

    public class IGSBaseModel
    {
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    public interface IGSInstanceApi { }

    public class IGSRequestCommon : IGSBaseModel
    {
        public IGSAuthenticationContext AuthenticationContext;
    }

    public class IGSResultCommon : IGSBaseModel
    {
        public IGSRequestCommon Request;
        public object CustomData;
    }

    public class IGSLoginResultCommon : IGSResultCommon
    {
        public IGSAuthenticationContext AuthenticationContext;
    }

    public class IGSResult<TResult> where TResult : IGSResultCommon
    {
        public TResult Result;
        public object CustomData;
    }
}
