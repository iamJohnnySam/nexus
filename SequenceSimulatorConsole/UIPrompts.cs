using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SimulatorSequenceConsole
{
    public static class UIPrompts
    {
        private static readonly char[] chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789@#$%^&*".ToCharArray();
        private static readonly Random rand = new();

        [assembly: AssemblyVersion("1.2.*")]

        private readonly static string programName = "Mindox Techno Systems Engineering: Layout Simulator";
        private readonly static string versionNumber = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0";

        public static void AddSeparator(bool resetColor = true)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("=====================================================================");
            if (resetColor)
                Console.ResetColor();
        }

        public static void ProgramInfo()
        {
            AddSeparator(false);
            Console.WriteLine(programName);
            Console.WriteLine($"Version {versionNumber}");
            Console.WriteLine($"Release Date: {GetBuildDate()}");
            AddSeparator(false);
            Console.WriteLine("Author: John Samarasinghe");
            Console.WriteLine("Company: Mindox Techno Pvt. Ltd.");
            Console.WriteLine("© 2025. All rights reserved.");
            AddSeparator();
        }

        public static void ShortcutMenu()
        {
            Console.WriteLine();
            AddSeparator(false);
            Console.WriteLine("Simulation Shortcuts");
            AddSeparator();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("Ctrl + P");
            Console.ResetColor();
            Console.WriteLine(" : Pause Simulation");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("Ctrl + S");
            Console.ResetColor();
            Console.WriteLine(" : Step through Simulation");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("Ctrl + Q");
            Console.ResetColor();
            Console.WriteLine(" : End current Simulation / Skip to next Simulation");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("Ctrl + P");
            Console.ResetColor();
            Console.WriteLine(" : Speed Up Simulation");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("Ctrl + O");
            Console.ResetColor();
            Console.WriteLine(" : Slow down Simulation");
            Console.WriteLine();
        }

        static string GetBuildDate()
        {
            string filePath = Assembly.GetExecutingAssembly().Location;
            DateTime buildDate = File.GetLastWriteTime(filePath);
            return buildDate.ToString("yyyy-MM-dd");
        }

        public static float PromptFloat(string prompt, float lowLimit, float highLimit, float defaultValue)
        {
            Console.WriteLine();
            Console.Write(prompt + $" ({lowLimit} - ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"[{defaultValue}]");
            Console.ResetColor();
            Console.Write($" - {highLimit}): ");
            while (true)
            {
                string? userInput = Console.ReadLine();
                if (userInput != null && float.TryParse(userInput, out float value) && value >= lowLimit && value <= highLimit)
                {
                    return value;
                }
                else if (userInput == string.Empty)
                {
                    return defaultValue;
                }
                else
                {
                    Console.Write(prompt);
                }
            }
        }

        public static bool PromptBool(string prompt, bool defaultValue)
        {
            Console.WriteLine();
            string? answer;
            while (true)
            {
                Console.Write(prompt + " (");
                if (defaultValue)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write("[Y]");
                    Console.ResetColor();
                    Console.Write("/");
                    Console.Write("N");
                    Console.Write("): ");
                }
                else
                {
                    Console.Write("Y");
                    Console.Write("/");
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write("[N]");
                    Console.ResetColor();
                    Console.Write("): ");
                }

                answer = Console.ReadLine();

                if (answer != null && answer.Equals("Y", StringComparison.CurrentCultureIgnoreCase))
                    return true;
                else if (answer != null && answer.Equals("N", StringComparison.CurrentCultureIgnoreCase))
                    return false;
                else if (answer != null && answer.Equals("Yes", StringComparison.CurrentCultureIgnoreCase))
                    return true;
                else if (answer != null && answer.Equals("No", StringComparison.CurrentCultureIgnoreCase))
                    return false;
                else if (answer == string.Empty)
                    return defaultValue;

            }
        }

        public static List<string> PromptFileOptions(string prompt, string[] xmlFiles)
        {
            if (xmlFiles.Count() == 1)
            {
                Console.WriteLine("There was only 1 valid file.");
                Console.WriteLine($"{xmlFiles[0]} will be picked by default.");
                return xmlFiles.ToList();
            }

            while (true)
            {
                Console.WriteLine();
                Console.Write("Enter a number from ");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("[0]");
                Console.ResetColor();
                Console.WriteLine($"  - {xmlFiles.Count()}");
                int i = 0;

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write(i++);
                Console.ResetColor();
                Console.WriteLine(": All files");


                foreach (var file in xmlFiles)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write(i++);
                    Console.ResetColor();
                    Console.WriteLine($": {file}");
                }

                Console.Write("Enter Number: ");
                string? userInput = Console.ReadLine();
                if (userInput != null && userInput == string.Empty)
                    userInput = "0";
                if (userInput != null && int.TryParse(userInput, out int value) && value >= 0 && value <= xmlFiles.Count())
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    if (value > 0)
                    {
                        Console.WriteLine($"You picked > {xmlFiles[value - 1]}");
                        Console.ResetColor();
                        return [xmlFiles[value - 1]];
                    }
                    else if (value == 0)
                    {
                        Console.WriteLine("You picked all files");
                        Console.ResetColor();
                        return xmlFiles.ToList();
                    }
                }
            }
        }

        public static void Transition()
        {
            Transition_FillScreen();
            Thread.Sleep(1000);
            Transition_DrainScreen();
            Console.Clear();
        }

        static void Transition_FillScreen()
        {
            int i;
            for (i = 0; i < Console.WindowHeight; i++)
            {
                if (i < Console.WindowHeight - 11)
                {
                    for (int j = 0; j < Console.WindowWidth; j++)
                    {
                        Console.SetCursorPosition(j, i);
                        Console.Write(chars[rand.Next(chars.Length)]);
                    }
                    Thread.Sleep(3); // Delay for effect
                }
                else
                {
                    for (int j = 0; j < Console.WindowWidth; j++)
                    {
                        Console.SetCursorPosition(j, i);
                        Console.Write(' ');
                    }
                }
            }
            Console.SetCursorPosition(0, Console.WindowHeight - 10);
            ProgramInfo();
        }

        static void Transition_DrainScreen()
        {
            for (int i = Console.WindowHeight - 1; i >= 0; i--)
            {
                for (int j = 0; j < Console.WindowWidth; j++)
                {
                    Console.SetCursorPosition(j, i);
                    Console.Write(' ');
                }
                Thread.Sleep(3); // Delay for effect
            }
        }
        public static void DelayForRecording(string layoutFile, int timeDelay)
        {
            while (timeDelay > 0)
            {
                Console.WriteLine($"Starting {layoutFile} in {timeDelay} seconds...");
                Thread.Sleep(1000);
                Console.Clear();
                timeDelay--;
            }
        }
        
        public static void HelpFolderStructure()
        {
            Console.WriteLine();
            AddSeparator(false);
            Console.WriteLine("PROGRAM BASICS");
            AddSeparator();
            Console.WriteLine();
            
            Console.WriteLine("Running this program automatically created 2 folders (\"layouts\",\"logs\") in the directory that the program was placed.");
            Console.WriteLine("Your simulations as .xml files must be placed inside the layouts folder.");
            Console.WriteLine("Your simulation results and logs will be added to the logs folder.");
            Console.WriteLine();
            Console.WriteLine("Logs file will contain all transactions occurred in the simulation.");
            Console.WriteLine("Logs file name > \"Logs_{PC name}_{Date and Time}.txt\"");
            Console.WriteLine();
            Console.WriteLine("Results fill will be written at fixed time intervals and will include a summary of the simulation status.");
            Console.WriteLine("Results file name > \"Results_{PC name}_{Date and Time}.txt\"");
            Console.WriteLine();
            Console.WriteLine("Throughput file will contain comma separated values of each simulation second. (Total Time, Steady-state time, Completed Payloads, Throughput at load, Throughput running average, Steady-state Throughput at load, Steady-state Throughput running average)");
            Console.WriteLine("Throughput file name > \"TPUT_{layout name}_{PC name}_{Date and Time}.txt\"");
            Console.WriteLine();
            Console.WriteLine("Throughput image will contain a trend of the throughput over time");
            Console.WriteLine("Throughput image name > \"TPUT_{layout name}_{PC name}_{Date and Time}.png\"");
            Console.WriteLine();
        }

        public static void HelpXMLFile()
        {
            Console.WriteLine();
            AddSeparator(false);
            Console.WriteLine("XML FILE, XML TAG GUIDE");
            AddSeparator();
            Console.WriteLine();
            Console.WriteLine("Below is the breakdown of tags in the XML file.");
            Console.WriteLine("" +
                "<Simulation>\t\t\t\t\t\t\tXML\t\tFile Start\r\n" +
                "-\t<Stations>\t\t\t\t\t\tXML\t\tStations Collection\r\n" +
                "-\t-\t<Station>\t\t\t\t\tXML, Repeated\tSingle Station Configuration\r\n" +
                "-\t-\t-\t<Identifier>\t\t\t\tString\t\tStation Identifier Name\r\n" +
                "-\t-\t-\t<PayloadType>\t\t\t\tString\t\tPayload Type accepted by station\r\n" +
                "-\t-\t-\t<Processes>\t\t\t\tXML\t\tProcesses Collection\r\n" +
                "-\t-\t-\t-\t<Process>\t\t\tXML, Repeated\tSingle Process Configuration\r\n" +
                "-\t-\t-\t-\t-\t<InputState>\t\tString\t\tState of Payload at Input\r\n" +
                "-\t-\t-\t-\t-\t<OutputState>\t\tString\t\tState of Payload at Output (Match with input for buffer)\r\n" +
                "-\t-\t-\t-\t-\t<Location>\t\tString\t\tLocation name to which station should be accessible after process completion.\r\n" +
                "-\t-\t-\t-\t-\t<ProcessTime>\t\tFloat\t\tProcess Time\r\n" +
                "-\t-\t-\t<Capacity>\t\t\t\tInteger\t\tNumber of slots\r\n" +
                "-\t-\t-\t<AccessibleLocationsWithDoor>\t\tString, CSV\tNames of locations to which station has door\r\n" +
                "-\t-\t-\t<AccessibleLocationsWithoutDoor>\tString, CSV\tNames of locations to which station has no door\r\n" +
                "-\t-\t-\t<DoorTransitionTimes>\t\t\tFloat, CSV\tMotion times of doors. Number of values should match with <AccessibleLocationsWithDoor> tag\r\n" +
                "-\t-\t-\t<ConcurrentLocationAccess>\t\tBoolean 1/0\tWhether the station should available to multiple locations at the same time\r\n" +
                "-\t-\t-\t<Processable>\t\t\t\tBoolean 1/0\tIs it a processable station\r\n" +
                "-\t-\t-\t<ProcessTime>\t\t\t\tBoolean 1/0\tProcess time in the event that Process time is not defined in <Processes> tag\r\n" +
                "-\t-\t-\t<PodDockable>\t\t\t\tBoolean 1/0\tIs the station Pod Dockable\r\n" +
                "-\t-\t-\t<AutoLoadPod>\t\t\t\tBoolean 1/0\tShould the station automatically load the Pod when docked\r\n" +
                "-\t-\t-\t<AutoDoorControl>\t\t\tBoolean 1/0\tShould the station automatically control doors when processing. Always Open. Close before processing\r\n" +
                "-\t-\t-\t<LowPriority>\t\t\t\tBoolean 1/0\tManipulator will move the payload to this station only if all other stations are occupied\r\n" +
                "-\t-\t-\t<Count>\t\t\t\t\tInteger\t\tNumber of stations\r\n" +
                "-\t<Manipulators>\t\t\t\t\t\tXML\t\tManipulators Collection\r\n" +
                "-\t-\t<Manipulator>\t\t\t\t\tXML, Repeated\tSingle Manipulator Configuration\r\n" +
                "-\t-\t-\t<Identifier>\t\t\t\tString\t\tManipulator Identifier Name\r\n" +
                "-\t-\t-\t<EndEffectors>\t\t\t\tString, CSV\tPayload Types each end effector can carry\r\n" +
                "-\t-\t-\t<Locations>\t\t\t\tString, CSV\tLocations to which this manipulator has access to\r\n" +
                "-\t-\t-\t<MotionTime>\t\t\t\tFloat\t\tStation to station motion time\r\n" +
                "-\t-\t-\t<ExtendTime>\t\t\t\tFloat\t\tManipulator arm extend time\r\n" +
                "-\t-\t-\t<RetractTime>\t\t\t\tFloat\t\tManipulator arm retract time\r\n" +
                "-\t-\t-\t<Count>\t\t\t\t\tInteger\t\tManipulator count\r\n");
            Console.WriteLine();
            Console.WriteLine("Pod dockable stations can be considered as the start and end of the process. Pods will be automatically loaded at the start and unloaded when completed (All wafers in the station are in process outuput condition).");
            Console.WriteLine();
            Console.WriteLine("In order to define the sequence, pay attention to the tag which defines the process. " +
                "Starting from a pod dockable station, make sure the process output string value is the process input string value of the second station that the payload should be transferred. " +
                "Once a station has been processed, it will convert the state of the payload from the input state to the output state. " +
                "Any station can have multiple processes with combinations of input and output states. However, the simulation will not allow multiple payloads with different states to be added to the same station. It will be first-come-first-serve. " +
                "If 2 or more stations have the same input process string and both stations are accessible by the same robot, then it will look for the LowPriority tag. " +
                "The simulator will give priority to a processable station, then any other station that is not low priority and finally cater to the low priority station.");
            Console.WriteLine();
        }
    }
}
