using System.Text.Json;
using Azure.Messaging.ServiceBus;
using TaskManager.Core.Messages;

namespace TaskManager.Worker;

public class TaskWorker : BackgroundService
{
	private readonly ILogger<TaskWorker> _logger;
	private readonly ServiceBusClient _client;
	private ServiceBusProcessor? _processor;

	public TaskWorker(ILogger<TaskWorker> logger, ServiceBusClient client)
	{
		_logger = logger;
		_client = client;
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		_processor = _client.CreateProcessor("task-created", new ServiceBusProcessorOptions
		{
			MaxConcurrentCalls = 1,
			AutoCompleteMessages = false
		});

		_processor.ProcessMessageAsync += HandleMessageAsync;
		_processor.ProcessErrorAsync += HandleErrorAsync;

		await _processor.StartProcessingAsync(stoppingToken);

		_logger.LogInformation("Worker started, listening on queue: task-created");

		await Task.Delay(Timeout.Infinite, stoppingToken)
			.ContinueWith(_ => Task.CompletedTask);
	}

	private async Task HandleMessageAsync(ProcessMessageEventArgs args)
	{
		try
		{
			var body = args.Message.Body.ToString();

			var message = JsonSerializer.Deserialize<TaskCreatedMessage>(body);

			if (message is null)
			{
				_logger.LogWarning("Received a message that could not be deserialised");
				await args.DeadLetterMessageAsync(args.Message);
				return;
			}

			_logger.LogInformation(
				"Task created - Id: {TaskId}, Title: {Title}, CreatedAt: {CreatedAt}",
				message.TaskId,
				message.Title,
				message.CreatedAt);

			await args.CompleteMessageAsync(args.Message);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error processing message");
			await args.AbandonMessageAsync(args.Message);
		}
	}

	private Task HandleErrorAsync(ProcessErrorEventArgs args)
	{
		_logger.LogError(args.Exception, "Service Bus processor error: {ErrorSource}",
			args.ErrorSource);
		return Task.CompletedTask;
	}

	public override async Task StopAsync(CancellationToken cancellationToken)
	{
		if (_processor is not null)
			await _processor.StopProcessingAsync(cancellationToken);

		await base.StopAsync(cancellationToken);
	}
}