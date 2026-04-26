using System.Text.Json;
using Azure.Messaging.ServiceBus;
using TaskManager.Core.Interfaces;

namespace TaskManager.Infrastructure.Messaging;

public class ServiceBusMessagePublisher : IMessagePublisher, IAsyncDisposable
{
	private readonly ServiceBusClient _client;
	private readonly ServiceBusSender _sender;

	public ServiceBusMessagePublisher(string connectionString, string queueName)
	{
		_client = new ServiceBusClient(connectionString);
		_sender = _client.CreateSender(queueName);
	}

	public async Task PublishAsync<T>(T message, CancellationToken cancellationToken = default)
	{
		var json = JsonSerializer.Serialize(message);

		var serviceBusMessage = new ServiceBusMessage(json)
		{
			ContentType = "application/json"
		};

		await _sender.SendMessageAsync(serviceBusMessage, cancellationToken);
	}

	public async ValueTask DisposeAsync()
	{
		await _sender.DisposeAsync();
		await _client.DisposeAsync();
	}
}