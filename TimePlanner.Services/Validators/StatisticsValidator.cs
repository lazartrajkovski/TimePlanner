using TimePlanner.Domain.Enums;
using TimePlanner.Services.DTOs;

namespace TimePlanner.Services.Validators;

public static class StatisticsValidator
{
    private const int MaxCustomRangeDays = 366;

    public static void Validate(StatisticsRequest request)
    {
        if (!Enum.IsDefined(request.Period))
        {
            throw new ArgumentException("Invalid statistics period");
        }

        if (request.Period == StatisticsPeriod.Custom)
        {
            ValidateCustomRange(request);
        }
    }

    public static void EnsureNotInFuture(DateOnly to, DateOnly userLocalDate)
    {
        if (to > userLocalDate)
        {
            throw new ArgumentException("To cannot be in the future");
        }
    }

    private static void ValidateCustomRange(StatisticsRequest request)
    {
        if (request.From is null || request.To is null)
        {
            throw new ArgumentException("From and To are required for a custom period");
        }

        if (request.From > request.To)
        {
            throw new ArgumentException("From cannot be after To");
        }

        var customRangeDays = request.To.Value.DayNumber - request.From.Value.DayNumber + 1;

        if (customRangeDays > MaxCustomRangeDays)
        {
            throw new ArgumentException($"Custom period cannot be longer than {MaxCustomRangeDays} days");
        }
    }
}