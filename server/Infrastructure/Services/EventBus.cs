using Application.Abstractions;

namespace Infrastructure.Services;

public class EventBus : IEventBus
{
    public Task PublishAsync<T>(T message, CancellationToken cancellationtoken = default)
    {
        throw new NotImplementedException();
    }
}
