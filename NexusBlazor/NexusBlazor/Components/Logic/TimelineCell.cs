namespace NexusBlazor.Components.Logic;

public class TimelineCell
{
    public required int Id { get; set; }
    public required string Title { get; set; }
    public required DateTime PlannedStartDate { get; set; }
    public required DateTime ActualStartDate { get; set; }
    public required DateTime PlannedEndDate { get; set; }
    public required DateTime ActualEndDate { get; set; }

}