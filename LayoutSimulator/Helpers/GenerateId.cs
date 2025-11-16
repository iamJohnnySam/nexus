using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LayoutSimulator.Helpers;

public sealed class GenerateId
{
    private static GenerateId? instance = null;

    private static readonly object padlock = new();

    private const string validChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    private int reservationId = 0;
    private int PodId = 0;
    private int PayloadId = 0;

    private GenerateId()
    {
        reservationId = 0;
    }

    public static GenerateId Instance
    {
        get
        {
            lock (padlock)
            {
                instance ??= new GenerateId();
                return instance;
            }
        }
    }

    public int GetReservationId()
    {
        return reservationId++;
    }

    public static string GetRandomId(int length)
    {
        StringBuilder? val = new(length);
        for (int i = 0; i < length; i++)
            val.Append(validChars[new Random().Next(validChars.Length)]);
        return val.ToString();
    }

    public string GetPodId()
    {
        string val = PodId.ToString("X8");
        PodId += 1;
        return val;
    }

    public string GetPayloadId()
    {
        string val = PayloadId.ToString("X8");
        PayloadId += 1;
        return val;
    }
}
