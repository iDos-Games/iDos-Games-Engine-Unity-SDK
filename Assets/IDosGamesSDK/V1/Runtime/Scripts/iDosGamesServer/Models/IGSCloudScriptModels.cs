using IDosGames.SharedModels;
using System;

namespace IDosGames.CloudScriptModels
{
    [Serializable]
    public class ExecuteFunctionResult : IGSResultCommon
    {
        public FunctionExecutionError Error;
        public int ExecutionTimeMilliseconds;
        public string FunctionName;
        public object FunctionResult;
        public bool? FunctionResultTooLarge;
    }

    [Serializable]
    public class FunctionExecutionError : IGSBaseModel
    {
        public string Error;
        public string Message;
        public string StackTrace;
    }
}
