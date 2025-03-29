using LayoutModels;
using LayoutModels.Manipulators;
using LayoutModels.Stations;
using SequenceSimulator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace SequenceSimulatorConsole
{
    public class SimulationDataManager(Simulator Simulator, int runTime, int startTPutMeasure, bool showStationDetails)
    {
        Queue<string> LogMessages = new();

        private readonly Simulator sim = Simulator;
        private readonly int runTime = runTime;
        private readonly int startTPutMeasure = startTPutMeasure;
        private readonly bool showStationDetails = showStationDetails;

        double lowLimit = double.PositiveInfinity;
        double highLimit = double.NegativeInfinity;

        int simTimeCount = 0;
        DateTime lastSimTime = DateTime.Now;
        double simSpeed = 0;

        public string TopLine { get; set; } = string.Empty;
        public bool RunningUpdate { get; set; } = false;
        public bool RunUpdate { get; set; } = false;

        private bool dataUpdated = false;
        private bool logsUpdated = false;

        public void CalculateTopLine()
        {
            // time
            string output = $"Elapsed Time (s) = {sim.TotalTime,6:N0} / {runTime,6:N0} | ";

            // completed payloads
            output += $"{sim.completedPayloads,3} Completed | ";

            // actual tput
            output += $"Throughput = {sim.Throughput,5: 0.00} ({sim.CalculateThroughput,5: 0.00}) | ";

            if (sim.TotalTime > startTPutMeasure)
            {
                if (sim.CalculateSteadyStateThroughput > highLimit)
                {
                    highLimit = sim.CalculateSteadyStateThroughput;
                }
                if (sim.CalculateSteadyStateThroughput < lowLimit)
                {
                    lowLimit = sim.CalculateSteadyStateThroughput;
                }
            }

            // steady state
            if (sim.stationsNotFilled.Count == 0)
            {
                if (sim.SteadyStateThroughput == 0)
                {
                    output += "Waiting for Payloads to complete | ";
                }
                else if (sim.TotalTime <= startTPutMeasure)
                {
                    output += $"SteadyState {sim.steadyStateTime}s Throughput* = {sim.SteadyStateThroughput,-5: 0.00} ({sim.CalculateSteadyStateThroughput,-5: 0.00}) | ";
                }
                else
                {
                    output += $"SteadyState {sim.steadyStateTime}s Throughput* = {sim.SteadyStateThroughput,-5: 0.00} ({lowLimit,-5: 0.00} - {sim.CalculateSteadyStateThroughput,-5: 0.00} - {highLimit,-5: 0.00}) | ";
                }
            }
            else
            {
                output += $"{string.Join(", ", sim.stationsNotFilled)} Reaching Steady State... | ";
            }

            output += $"Running Threads = {Global.RunningThreads.Count} | {simSpeed:0.0} SimSecs/sec";

            TopLine = output;
            dataUpdated = true;
        }

        public void CalculateSimulationSpeed()
        {
            int limit = 50;
            simTimeCount++;
            if (simTimeCount >= limit)
            {
                simTimeCount = 0;
                simSpeed = limit / DateTime.Now.Subtract(lastSimTime).TotalSeconds;
                lastSimTime = DateTime.Now;
            }
        }

        void WriteConsoleLine(int row, string message)
        {
            Console.SetCursorPosition(0, row);
            Console.Write(message);
            int remainingSpace = Console.WindowWidth - message.Length;
            if (remainingSpace > 0)
                Console.Write(new string(' ', remainingSpace));
        }

        int WriteConsole(string message, ConsoleColor? foregroundColor = null)
        {
            if (foregroundColor != null)
            {
                Console.ForegroundColor = foregroundColor.Value;
            }
            else
            {
                Console.ResetColor();
            }
            Console.Write(message);
            return message.Length;
        }

        public void UpdateModuleStatus(string layoutName, int TopSectionHeight, bool onScreenLogs)
        {
            RunUpdate = true;

            while (RunUpdate)
            {
                while (RunUpdate && !dataUpdated)
                {
                    Thread.Sleep(5);
                }
                RunningUpdate = true;
                Console.ResetColor();
                WriteConsoleLine(0, $"Layout Name: '{layoutName}'");
                WriteConsoleLine(1, TopLine);
                DetailedStationUpdate();

                if (onScreenLogs)
                    PrintLogs(TopSectionHeight);

                RunningUpdate = false;
                dataUpdated = false;
            }
        }
        void DetailedStationUpdate()
        {
            int i = 2;
            foreach (Station station in sim.layout.StationList.Values)
            {
                if (showStationDetails)
                {
                    Console.SetCursorPosition(0, i);
                    StationDetails(station);
                }
                else
                    WriteConsoleLine(i, $"{station.StationID,4} ({station.slots.Count,2}/{station.Capacity,-2}): {station.State,-14}");
                i++;
            }
            i++;
            foreach (Manipulator manipulator in sim.layout.ManipulatorList.Values)
            {
                if (showStationDetails)
                {
                    Console.SetCursorPosition(0, i);
                    ManipulatorDetails(manipulator);
                }
                else
                    WriteConsoleLine(i, $"{manipulator.StationID,4} ({manipulator.EndEffectors.Values.Count,2}/{manipulator.EndEffectorTypes.Count,-2}): {manipulator.State,-14}");
                i++;
            }
        }

        void StationDetails(Station station)
        {
            int stringLen = 0;
            ConsoleColor? color = null;

            if (sim.blockedStations.Contains(station) && station.State == StationState.Idle)
            {
                color = ConsoleColor.Red;
            }
            stringLen += WriteConsole($"{station.StationID,4} ", color);
            stringLen += WriteConsole($"({station.slots.Count,2}/{station.Capacity,-2}): ");

            // Station State
            switch (station.State)
            {
                case StationState.Opening:
                case StationState.Closing:
                case StationState.Mapping:
                    color = ConsoleColor.DarkGray;
                    break;
                case StationState.Processing:
                    if (station.slots.Count == station.Capacity)
                        color = ConsoleColor.Blue;
                    else
                        color = ConsoleColor.Cyan;
                    break;
                case StationState.BeingAccessed:
                    color = ConsoleColor.Red;
                    break;
                default:
                    color = null;
                    break;
            }
            stringLen += WriteConsole($"{station.State,-14} ", color);
            stringLen += WriteConsole("| ");

            // Mapping Data
            int i = 1;
            foreach (MapCodes mapData in station.GetMap())
            {
                switch (mapData)
                {
                    case MapCodes.Empty:
                        if (station.State == StationState.UnDocked)
                            stringLen += WriteConsole("- ", ConsoleColor.DarkGray);
                        else
                            stringLen += WriteConsole("0 ", ConsoleColor.DarkGray);
                        break;
                    case MapCodes.Available:
                        if (station.blockedSlots.Contains(i))
                        {
                            if (station.IsAnOutputState(station.slots[i].PayloadState))
                            {
                                if (!station.PodDockable && !sim.waitingTransfer.Contains((station.slots[i].PayloadID, station.StationID, i, !station.LowPriority)))
                                {
                                    stringLen += WriteConsole("# ", ConsoleColor.Red);
                                }
                                else
                                {
                                    stringLen += WriteConsole("# ", ConsoleColor.DarkGreen);
                                }
                            }
                            else
                                stringLen += WriteConsole("1 ", ConsoleColor.Blue);
                        }
                        else
                        {
                            if (station.slots.ContainsKey(i) && station.IsAnOutputState(station.slots[i].PayloadState))
                            {
                                if (!station.PodDockable && !sim.waitingTransfer.Contains((station.slots[i].PayloadID, station.StationID, i, !station.LowPriority)))
                                {
                                    stringLen += WriteConsole("# ", ConsoleColor.Red);
                                }
                                else
                                {
                                    stringLen += WriteConsole("# ", ConsoleColor.DarkGreen);
                                }
                            }

                            else
                                stringLen += WriteConsole("1 ", null);
                        }
                        break;
                    case MapCodes.Double:
                        stringLen += WriteConsole("D ", ConsoleColor.Red);
                        break;
                    case MapCodes.Cross:
                        stringLen += WriteConsole("X ", ConsoleColor.Red);
                        break;
                    default:
                        break;
                }
                i++;
            }

            stringLen += WriteConsole("| ");

            // Location and Doors
            foreach (KeyValuePair<string, (bool accessLimited, AccessibilityState accessibility, float transitionTime)> kvp in station.Locations)
            {
                string door;
                if (!kvp.Value.accessLimited)
                {
                    door = " ";
                    if (station.ConcurrentLocationAccess)
                        color = null;
                    else if (!station.ConcurrentLocationAccess && kvp.Key == station.CurrentLocation)
                        color = ConsoleColor.Green;
                    else
                        color = ConsoleColor.DarkGray;
                }
                else if (kvp.Value.accessibility == AccessibilityState.Accessible)
                {
                    door = "_";
                    if (station.ConcurrentLocationAccess)
                        color = null;
                    else if (!station.ConcurrentLocationAccess && kvp.Key == station.CurrentLocation)
                        color = ConsoleColor.Green;
                    else
                        color = ConsoleColor.DarkGray;
                }
                else
                {
                    door = "X";
                    color = ConsoleColor.Red;
                }
                stringLen += WriteConsole($"[{door}] {kvp.Key} ", color);
            }

            stringLen += WriteConsole("| ");

            // Process Status
            if (station.Processable && station.State == StationState.Processing)
            {
                WriteConsole($"Processing ({station.showProcessTime - station.showTime} / {station.showProcessTime}) ", ConsoleColor.Blue);
            }
            else
            {
                if (station.PodDockable && (station.State == StationState.Idle || station.State == StationState.BeingAccessed))
                {
                    WriteConsole($"POD ({station.PodID}) ", ConsoleColor.Yellow);
                }
            }

            List<string> mismatchPayloads = [];
            if (station.AllPayloadsSingularState && station.slots.Count != 0)
            {
                foreach (Payload payload in station.slots.Values)
                {
                    if (!mismatchPayloads.Contains(payload.PayloadState))
                        mismatchPayloads.Add(payload.PayloadState);
                }
                stringLen += WriteConsole($"[{string.Join(' ', mismatchPayloads)}]", ConsoleColor.DarkGray);
            }
            else if (!station.AllPayloadsSingularState && station.slots.Count != 0)
            {
                foreach (Payload payload in station.slots.Values)
                {
                    mismatchPayloads.Add(payload.PayloadState);
                }
                stringLen += WriteConsole($"[{string.Join(' ', mismatchPayloads)}]", ConsoleColor.Magenta);
            }

            int remainingSpace = Console.WindowWidth - stringLen;
            if (remainingSpace > 0)
                Console.Write(new string(' ', remainingSpace));
        }

        void ManipulatorDetails(Manipulator manipulator)
        {
            int stringLen = 0;
            ConsoleColor? color = null;

            if (sim.blockedManipulators.Contains(manipulator) && manipulator.State == StationState.Idle)
            {
                color = ConsoleColor.Red;
            }
            stringLen += WriteConsole($"{manipulator.StationID,4} ", color);

            int EEs = 0;
            List<string> EE = new();

            foreach (Dictionary<string, Payload> payload in manipulator.EndEffectors.Values)
            {
                if (payload.Keys.Contains("payload"))
                {
                    EE.Add($"{payload["payload"].PayloadState} ({payload["payload"].PayloadID})");
                    EEs++;
                }
                else
                {
                    EE.Add("-----");
                }
            }
            stringLen += WriteConsole($"({EEs,2}/{manipulator.EndEffectorTypes.Count}): ");


            // Manipulator State
            color = manipulator.State switch
            {
                StationState.Extending => (ConsoleColor?)ConsoleColor.Cyan,
                StationState.Retracting => (ConsoleColor?)ConsoleColor.DarkCyan,
                StationState.Moving => (ConsoleColor?)ConsoleColor.DarkGray,
                _ => null,
            };
            stringLen += WriteConsole($"{manipulator.State,-15} ", color);
            stringLen += WriteConsole("| ");


            // Manipulator Arm State
            color = manipulator.ArmState switch
            {
                ManipulatorArmStates.extended => (ConsoleColor?)ConsoleColor.Cyan,
                ManipulatorArmStates.retracted => (ConsoleColor?)ConsoleColor.DarkCyan,
                _ => null,
            };
            stringLen += WriteConsole($"{manipulator.ArmState,-14}, ", color);
            stringLen += WriteConsole("| Location: ");


            stringLen += WriteConsole($"{manipulator.CurrentLocation,-4} -> ");

            if (EEs == 0)
                color = null;
            else if (EEs == 1)
                color = ConsoleColor.Yellow;
            else if (EEs == 2)
                color = ConsoleColor.Green;
            stringLen += WriteConsole(string.Join(", ", EE), color);

            int remainingSpace = Console.WindowWidth - stringLen;
            if (remainingSpace > 0)
                Console.Write(new string(' ', remainingSpace));
        }

        public void Simulator_OnLogEvent(object? sender, Logger.LogMessage e)
        {
            try
            {
                if (e.message != null)
                {
                    LogMessages.Enqueue($"{(e.transactionID ?? "---").PadLeft(3, '0')}: {e.message}");
                    logsUpdated = true;
                }
            }
            catch { }
        }

        public void PrintLogs(int TopSectionHeight)
        {
            if (!logsUpdated)
                return;

            int logSectionLen = Console.WindowHeight - TopSectionHeight;
            while (LogMessages.Count > logSectionLen)
            {
                LogMessages.Dequeue();
            }
            int line = 0;
            try
            {
                foreach (string? logMessage in LogMessages.ToList())
                {
                    if (logMessage != null)
                    {
                        if (logSectionLen > LogMessages.Count)
                            Console.ForegroundColor = ConsoleColor.Red;
                        else if (logMessage.StartsWith("---"))
                            Console.ForegroundColor = ConsoleColor.DarkGray;
                        else
                            Console.ResetColor();
                        WriteConsoleLine(TopSectionHeight + line, logMessage);
                        line++;
                    }
                }
                logsUpdated = false;
            }
            catch
            {

            }
        }

    }
}
