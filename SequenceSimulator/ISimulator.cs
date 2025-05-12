
namespace SequenceSimulator
{
    public interface ISimulator
    {
        float CalculateSteadyStateThroughput { get; }
        float CalculateThroughput { get; }
        bool IgnoreLotIDMatching { get; set; }
        float SteadyStateThroughput { get; }
        float Throughput { get; }
        int TotalTime { get; set; }

        event EventHandler<(string? tID, string message)>? OnLogEvent;

        string CreateFilledPod(string tID, int payloadQuantity, int podSize, string payloadType);
        void CreatePodAndDock(string tID, string stationID);
        void InitializeSimulator(string xmlPath, bool ignoreLotIDMatching);
        void RunSimulator(int timeInUnits);
    }
}