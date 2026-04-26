namespace TaskManager.Core.Interfaces;

public interface IMessagePublisher
{
	Task PublishAsync<T>(T message, CancellationToken cancellationToken = default);
}