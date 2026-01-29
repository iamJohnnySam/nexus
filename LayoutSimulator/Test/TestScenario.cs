using LayoutSimulator.Creator;
using LayoutSimulator.Models;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace LayoutSimulator.Test;

public static class TestScenario
{
    public static SimulatorLayout GetAutoTestLayout()
    {
        SimulatorLayout layout = new(true);

        // PROCESSES
        layout.AddProcess(new ProcessStruct
        {
            ProcessId = 1,
            ProcessName = "ProcessA",
            InputState = "StateA",
            OutputState = "StateB",
            NextLocation = null,
            ProcessTime = 20
        });
        layout.AddProcess(new ProcessStruct
        {
            ProcessId = 2,
            ProcessName = "Loading",
            InputState = "StateB",
            OutputState = "StateA",
            NextLocation = null,
            ProcessTime = 0
        });

        // STATIONS
        layout.AddStation(new StationStruct
        {
            FriendlyName = "Loading",
            Identifier = "L",
            PayloadType = "A",
            ProcessIdsCSV = "2",
            AccessibleLocationsWithoutDoorCSV = string.Empty,
            AccessibleLocationsWithDoorCSV = "LocationA",
            DoorTransitionTimesCSV = "10",
            AccessiblePayloadsThroughDoorCSV = "25",
            AccessiblePayloadsThroughtGapCSV = string.Empty,
            Capacity = 25,
            Processable = false,
            IsInputAndPodDockable = true,
            IsOutputAndPodDockable = true,
            IsIndexable = false,
            HighPriority = false,
            Count = 2
        });

        layout.AddStation(new StationStruct
        {
            FriendlyName = "Process",
            Identifier = "P",
            PayloadType = "A",
            ProcessIdsCSV = "1",
            AccessibleLocationsWithoutDoorCSV = "LocationA",
            AccessibleLocationsWithDoorCSV = "",
            DoorTransitionTimesCSV = "10",
            AccessiblePayloadsThroughDoorCSV = "",
            AccessiblePayloadsThroughtGapCSV = "2",
            Capacity = 2,
            Processable = true,
            IsInputAndPodDockable = false,
            IsOutputAndPodDockable = false,
            IsIndexable = false,
            HighPriority = true,
            Count = 2
        });

        // MANIPULATORS
        layout.AddManipulator(new ManipulatorStruct
        {
            ManipulatorName = "Manipulator",
            ManipulatorIdentifier = "M",
            EndEffectorsCSV = "A,A",
            EndEffectorSlotsCSV = "1,1",
            LocationsCSV = "LocationA",
            MotionTime = 10,
            ExtendTime = 5,
            RetractTime = 5,
            Count = 1
        });

        return layout;
    }
}
