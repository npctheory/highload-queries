namespace Infrastructure.Providers;

using Application.Abstractions;

public class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}