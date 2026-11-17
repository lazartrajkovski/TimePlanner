using System.Text.Json.Serialization;

namespace TimePlanner.Services.DTOs.Statistics;

public class TrackedTimeIntervalResponse
{
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Hour { get; set; }
    public long TrackedSeconds { get; set; }
}