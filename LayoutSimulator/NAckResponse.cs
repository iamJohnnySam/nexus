using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LayoutSimulator;

public class NAckResponse : Exception
{
    public ENAckCode Code { get; private set; }
    public string ErrorMessage { get; private set; }

    public NAckResponse(ENAckCode code, string message) : base($"An error occurred: {code}")
    {
        Code = code;
        ErrorMessage = message;
    }
}