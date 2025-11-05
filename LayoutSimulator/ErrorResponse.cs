using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LayoutSimulator;

public class ErrorResponse : Exception
{
    public EErrorCode Code { get; private set; }
    public string ErrorMessage { get; private set; }

    public ErrorResponse(EErrorCode code, string message) : base($"An error occurred: {code}")
    {
        Code = code;
        ErrorMessage = message;
    }
}