using DataModels.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataModels.Data;

public class SimulationScenario
{
    [Key]
    public int SimulationScenarioId { get; set; }
    public required string SimulationName { get; set; }
    public int ProjectId { get; set; }
    public required string XMLFile { get; set; }
    public float LastThroughput { get; set; }
    private string SimulationStationIdCSV { get; set; } = "";
    public Dictionary<int, int> SimulationStationIds
    {
        get
        {
            Dictionary<int, int> returnDict = new();
            if(SimulationStationIdCSV != "")
            {
                List<int> idsAndCount = [.. SimulationStationIdCSV.Split(",").Select(int.Parse)];

                foreach (int id in idsAndCount)
                {
                    if (returnDict.ContainsKey(id))
                    {
                        returnDict[id]++;
                    }
                    else
                    {
                        returnDict[id] = 1;
                    }
                }
            }
            return returnDict;
        }
        set
        {
            List<string> idList = new();
            foreach (KeyValuePair<int, int> kvp in value)
            {
                for (int i = 0; i < kvp.Value; i++)
                {
                    idList.Add(kvp.Key.ToString());
                }
            }
            SimulationStationIdCSV = String.Join(",", idList);
        }
    }
    public Dictionary<SimulationStation, int> SimulatorStations { get; set; } = [];
    private string SimulationManipulatorIdCSV { get; set; } = "";
    public Dictionary<int, int> SimulationManipulatorIds
    {
        get
        {
            Dictionary<int, int> returnDict = new();
            if(SimulationManipulatorIdCSV != "")
            {
                List<int> idsAndCount = [.. SimulationManipulatorIdCSV.Split(",").Select(int.Parse)];

                foreach (int id in idsAndCount)
                {
                    if (returnDict.ContainsKey(id))
                    {
                        returnDict[id]++;
                    }
                    else
                    {
                        returnDict[id] = 1;
                    }
                }
            }
            return returnDict;
        }
        set
        {
            List<string> idList = new();
            foreach (KeyValuePair<int, int> kvp in value)
            {
                for (int i = 0; i < kvp.Value; i++)
                {
                    idList.Add(kvp.Key.ToString());
                }
            }
            SimulationManipulatorIdCSV = String.Join(",", idList);
        }
    }
    public Dictionary<SimulationManipulator, int> SimulatorManipulators { get; set; } = [];
    private string SimulationReaderIdCSV { get; set; } = "";
    public Dictionary<int, int> SimulationReaderIds
    {
        get
        {
            Dictionary<int, int> returnDict = new();
            if(SimulationReaderIdCSV != "")
            {
                List<int> idsAndCount = [.. SimulationReaderIdCSV.Split(",").Select(int.Parse)];

                foreach (int id in idsAndCount)
                {
                    if (returnDict.ContainsKey(id))
                    {
                        returnDict[id]++;
                    }
                    else
                    {
                        returnDict[id] = 1;
                    }
                }
            }
            return returnDict;
        }
        set
        {
            List<string> idList = new();
            foreach (KeyValuePair<int, int> kvp in value)
            {
                for (int i = 0; i < kvp.Value; i++)
                {
                    idList.Add(kvp.Key.ToString());
                }
            }
            SimulationReaderIdCSV = String.Join(",", idList);
        }
    }
    public Dictionary<SimulationReader, int> SimulatorReaders { get; set; } = [];                                                                                   

    public static TableMetadata Metadata => new(
        typeof(SimulationScenario).Name,
        new Dictionary<string, EDataType>
        {
                { nameof(SimulationScenarioId), EDataType.Key },
                { nameof(SimulationName), EDataType.Text },
                { nameof(ProjectId), EDataType.Integer },
                { nameof(XMLFile), EDataType.LongText },
                { nameof(LastThroughput), EDataType.Real },
                { nameof(SimulationStationIdCSV), EDataType.Text },
                { nameof(SimulationManipulatorIdCSV), EDataType.Text },
                { nameof(SimulationReaderIdCSV), EDataType.Text }
        },
        nameof(SimulationName)
    );
}
