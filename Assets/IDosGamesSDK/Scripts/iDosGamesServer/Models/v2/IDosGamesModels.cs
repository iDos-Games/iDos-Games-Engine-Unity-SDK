using System;

namespace IDosGames
{
    /// <summary>
    /// A generic wrapper for the API V2 response.
    /// Used in all services (GameLoop, Auth, Store, etc.)
    /// </summary>
    [Serializable]
    public class IDosGamesApiResult<T>
    {
        public bool Success { get; set; }
        public string Error { get; set; }
        public T Data { get; set; }
    }

}
