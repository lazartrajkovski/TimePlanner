namespace TimePlanner.Services.Helpers;

public class StatisticsRange
{
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public DateTime StartUtc { get; set; }
    public DateTime EndUtc { get; set; }
}