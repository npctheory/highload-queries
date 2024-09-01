namespace Core.Application.Abstractions;

public interface IEventBus
{
    Task PublishAsync<T>(T message, CancellationToken cancellationtoken = default);
}