namespace TaskManager.Infrastructure.Settings;

public class ServiceBusSettings
{
	public string ConnectionString { get; set; } = string.Empty;
	public string QueueName { get; set; } = string.Empty;
}