using Newtonsoft.Json;
using System;

namespace IDosGames
{
    public class JsonRpcRequest<T>
    {
        [JsonProperty("jsonrpc")]
        public string JsonRpc { get; set; } = "2.0";

        [JsonProperty("method")]
        public string Method { get; set; }

        [JsonProperty("params")]
        public T Params { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; } = 1;
    }

    public class EthCallParams
    {
        [JsonProperty("to")]
        public string To { get; set; }

        [JsonProperty("data")]
        public string Data { get; set; }
    }

    public class JsonRpcResponse<T>
    {
        [JsonProperty("jsonrpc")]
        public string JsonRpc { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("result")]
        public T Result { get; set; }

        [JsonProperty("error")]
        public JsonRpcError Error { get; set; }
    }

    public class JsonRpcError
    {
        [JsonProperty("code")]
        public int Code { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }
    }

    [Serializable]
    public class JsonResponse
    {
        public string jsonrpc;
        public int id;
        public string result;
        public JsonError error;
    }

    [Serializable]
    public class JsonError
    {
        public int code;
        public string message;
    }
}
