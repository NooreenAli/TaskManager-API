using Azure.Messaging.ServiceBus;
using TaskManager.Infrastructure.Settings;
using TaskManager.Worker;

var builder = Host.CreateApplicationBuilder(args);

var serviceBusSettings = builder.Configuration
	.GetSection("ServiceBus")
	.Get<ServiceBusSettings>()!;

builder.Services.AddSingleton(
	new ServiceBusClient(serviceBusSettings.ConnectionString));

builder.Services.AddHostedService<TaskWorker>();

var host = builder.Build();
host.Run();