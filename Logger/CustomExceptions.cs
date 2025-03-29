using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logger
{
    public class ErrorResponse : Exception
    {
        public ErrorCodes Code { get; private set; }
        public string ErrorMessage { get; private set; }

        public ErrorResponse(ErrorCodes code, string message) : base($"An error occurred: {code}")
        {
            Code = code;
            ErrorMessage = message;
        }
    }

    public class NAckResponse : Exception
    {
        public NAckCodes Code { get; private set; }
        public string ErrorMessage { get; private set; }

        public NAckResponse(NAckCodes code, string message) : base($"An error occurred: {code}")
        {
            Code = code;
            ErrorMessage = message;
        }
    }

    public class ConnectionFailedException : Exception
    {
        public ConnectionFailedException(string message, Exception innerException) : base(message, innerException) { }
        public ConnectionFailedException(string message) : base(message) { }
    }

    public class MissingArgumentsException : Exception
    {
        public MissingArgumentsException(string message, Exception innerException) : base(message, innerException) { }
        public MissingArgumentsException(string message) : base(message) { }
    }
    public class BotException : Exception
    {
        public BotException(string message, Exception innerException) : base(message, innerException) { }
        public BotException(string message) : base(message) { }
    }
    public class ExitProgramException : Exception
    {
        public ExitProgramException(string message, Exception innerException) : base(message, innerException) { }
        public ExitProgramException(string message) : base(message) { }
    }
}
