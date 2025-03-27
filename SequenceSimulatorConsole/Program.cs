using Google.Protobuf.WellKnownTypes;
using LayoutModels;
using SimulatorSequence;
using SimulatorSequenceConsole;

Simulator simulator;
SimulationDataManager simResults;

Directory.CreateDirectory("logs\\");
Directory.CreateDirectory("layouts\\");
string pathLogs = $"logs\\Logs_{System.Environment.MachineName}_{DateTime.Now.ToString("yyyyMMdd_hhmmss")}.txt";
string pathResults = $"logs\\Results_{System.Environment.MachineName}_{DateTime.Now.ToString("yyyyMMdd_hhmmss")}.txt";

string filePath = @"layouts\";

bool isPaused = false;
bool stepThrough = false;
bool skipSim = false;

List<int> DelayValues = [0, 1, 5, 10, 50, 100, 200, 500, 750, 1000];
int delayToUse = 0;

List<string> simulationsToRun = [];
Dictionary<string, string> results = [];
Dictionary<int, (float, float, float, float)> tPutResults = [];

Console.Clear();
UIPrompts.ProgramInfo();

if (UIPrompts.PromptBool("First time? Do you need help?", false))
{
    UIPrompts.HelpFolderStructure();
    if (UIPrompts.PromptBool("Do you need to see the guide to building an .xml layout file?", false))
    {
        UIPrompts.HelpXMLFile();
    }
}

UIPrompts.ShortcutMenu();

Console.WriteLine("Follow the guide below to set up your simulation run time");
UIPrompts.AddSeparator();

if (File.Exists(filePath) && Path.GetExtension(filePath).Equals(".xml", StringComparison.OrdinalIgnoreCase))
{
    simulationsToRun = [filePath];
}
else if (Directory.Exists(filePath))
{
    var xmlFiles = Directory.GetFiles(filePath, "*.xml");
    simulationsToRun = UIPrompts.PromptFileOptions("Which file do you want to run?", xmlFiles);
}
else
{
    Console.WriteLine("No XML layout files found.");
    Console.WriteLine("This program will now exit.");
    Console.ReadLine();
    Environment.Exit(0);
}

int runTime = (int)UIPrompts.PromptFloat("How long do you want to run each simulation in hours?", 0, 999, 48) * 3600;
Console.WriteLine();
Console.ForegroundColor = ConsoleColor.Yellow;
Console.WriteLine("* The steady state throughput will automatically start calculating when all processable stations are filled up.");
Console.ResetColor();

int startTPutMeasure = (int)UIPrompts.PromptFloat($"After which hour do you want to start recording the SS throughput high and low values?", 0, (float)(runTime / 3600.0), (float)(runTime / 14400.0));

bool ignoreLotIDMatching = UIPrompts.PromptBool("When returning back to pod do you want to ignore lot matching?", false);

bool ignoreLogs = !UIPrompts.PromptBool("Do you need to save logs to a file?", false);
Console.ForegroundColor = ConsoleColor.Yellow;
if (!ignoreLogs)
    Console.WriteLine($"Logs save path: {pathLogs}");
Console.WriteLine($"Results save path: {pathResults}");
Console.ResetColor();

bool onScreenLogs = UIPrompts.PromptBool("Do you need to see live on-screen logs? *updates every sim second.", true);
bool onScreenDetails = UIPrompts.PromptBool("Do you need to see the live details of each station & manipulator on-screen?", true);

bool plotVal = UIPrompts.PromptBool("Do you want to save an output file with throughput figures?", false);
bool plot = UIPrompts.PromptBool("Do you want to save an output graph with throughput trend?", true);

int delayTime = (int)UIPrompts.PromptFloat("How much of a delayed start do you want in seconds?", 0, 100, 0);
UIPrompts.Transition();

foreach (string layout in simulationsToRun)
{
    RunLayout(layout, runTime);
}

Console.Clear();
Console.WriteLine("Simulation Summary:");
foreach (KeyValuePair<string, string> kvp in results)
{
    Console.WriteLine();
    Console.WriteLine($"Layout: {kvp.Key}");
    Console.WriteLine(kvp.Value);
}

// ------------------------------------------------------------------------------------------------------------

void RunLayout(string layoutFile, int time, int stepTime = 1)
{
    tPutResults = [];
    simulator = new();
    simResults = new(simulator, time, startTPutMeasure, onScreenDetails);

    simulator.InitializeSimulator(layoutFile, ignoreLotIDMatching);
    string fileName = layoutFile.Substring(layoutFile.LastIndexOf('\\') + 1).Replace(".xml", "");
    string pathTput = $"logs\\TPUT_{fileName}_{System.Environment.MachineName}_{DateTime.Now.ToString("yyyyMMdd_hhmmss")}.txt";
    string pathTputImg = $"logs\\TPUT_{fileName}_{System.Environment.MachineName}_{DateTime.Now.ToString("yyyyMMdd_hhmmss")}.png";

    Console.Clear();

    int TopSectionHeight = simulator.layout.Stations.Keys.Count + simulator.layout.Manipulators.Keys.Count + 4;

    simulator.OnLogEvent += simResults.Simulator_OnLogEvent;
    // simulator.OnLogEvent += Simulator_PrintLogs;

    // Delayed start for recording
    UIPrompts.DelayForRecording(layoutFile, delayTime);
    if (!ignoreLogs)
    {
        WriteToFile(pathLogs, "---------------------------------------");
        WriteToFile(pathLogs, layoutFile);
    }
    WriteToFile(pathResults, "---------------------------------------");
    WriteToFile(pathResults, layoutFile);

    int lastPayloadWrite = 0;

    skipSim = false;

    Thread updateCon = new(() => updateConsole(layoutFile, TopSectionHeight));
    updateCon.Start();

    while (time > 0 && !skipSim)
    {
        if (!isPaused)
        {
            simulator.RunSimulator(stepTime);
            simResults.CalculateTopLine();

            if (!ignoreLogs)
            {
                WriteToFile(pathLogs, simResults.TopLine);
            }

            if (simulator.completedPayloads % 25 == 0 && simulator.completedPayloads > lastPayloadWrite)
            {
                WriteToFile(pathResults, simResults.TopLine);
                lastPayloadWrite = simulator.completedPayloads;
            }

            if (plotVal)
            {
                WriteToFile(pathTput, $"{simulator.TotalTime},{simulator.steadyStateTime},{simulator.completedPayloads},{simulator.Throughput:0.00},{simulator.CalculateThroughput:0.00},{simulator.SteadyStateThroughput:0.00},{simulator.CalculateSteadyStateThroughput:0.00}");
            }
            if (plot)
            {
                tPutResults.Add(simulator.TotalTime, (simulator.Throughput, simulator.CalculateThroughput, simulator.SteadyStateThroughput, simulator.CalculateSteadyStateThroughput));
            }

            time -= stepTime;

            if (stepThrough)
            {
                isPaused = true;
            }
        }
        Thread.Sleep(DelayValues[delayToUse]);
        simResults.CalculateSimulationSpeed();
        ListenToKey();
    }
    simResults.RunUpdate = false;

    WriteToFile(pathResults, simResults.TopLine);
    results.Add(layoutFile, simResults.TopLine);

    if (plot)
    {
        Thread plotter = new(() => new ThroughputPlotter($"{fileName}-{DateTime.Now.ToString("yyyy MMM dd")}", tPutResults).PlotGraph(pathTputImg));
        plotter.Start();
    }
    UIPrompts.Transition();
}

void updateConsole(string layoutFile, int TopSectionHeight)
{
    simResults.UpdateModuleStatus(layoutFile, TopSectionHeight, onScreenLogs);
}

void WriteToFile(string path, string msg)
{
    // Ensure the file is created if it doesn't exist and allow shared read access
    using (FileStream fs = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.Read))
    using (StreamWriter sw = new StreamWriter(fs))
    {
        sw.WriteLine(msg);
    }
}

void ListenToKey()
{
    if (Console.KeyAvailable)
    {
        var key = Console.ReadKey(intercept: true);
        if (key.Modifiers.HasFlag(ConsoleModifiers.Control) && key.Key == ConsoleKey.P)
        {
            isPaused = !isPaused;
            stepThrough = false;
        }
        if (key.Modifiers.HasFlag(ConsoleModifiers.Control) && key.Key == ConsoleKey.S)
        {
            isPaused = false;
            stepThrough = true;
        }
        if (key.Modifiers.HasFlag(ConsoleModifiers.Control) && key.Key == ConsoleKey.Q)
        {
            skipSim = true;
        }
        if (key.Modifiers.HasFlag(ConsoleModifiers.Control) && key.Key == ConsoleKey.O)
        {
            if (delayToUse < DelayValues.Count() - 1)
            {
                delayToUse++;
            }
        }
        if (key.Modifiers.HasFlag(ConsoleModifiers.Control) && key.Key == ConsoleKey.P)
        {
            if (delayToUse > 0)
            {
                delayToUse--;
            }
        }
    }
}
