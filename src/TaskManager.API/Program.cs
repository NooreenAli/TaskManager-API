using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using TaskManager.API.Mappings;
using TaskManager.Core.Interfaces;
using TaskManager.Infrastructure.Data;
using TaskManager.Infrastructure.Messaging;
using TaskManager.Infrastructure.Repositories;
using TaskManager.Infrastructure.Settings;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

builder.Services.AddAutoMapper(cfg =>
	cfg.AddMaps(typeof(TaskMappingProfile).Assembly));

builder.Services.AddDbContext<AppDbContext>(options =>
	options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ITaskRepository, TaskRepository>();

var serviceBusSettings = builder.Configuration
	.GetSection("ServiceBus")
	.Get<ServiceBusSettings>()!;

builder.Services.AddSingleton<IMessagePublisher>(
	new ServiceBusMessagePublisher(
		serviceBusSettings.ConnectionString,
		serviceBusSettings.QueueName));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
	app.MapOpenApi();
	app.MapScalarApiReference();
}

if (app.Environment.IsDevelopment())
{
	app.UseHttpsRedirection();
}

app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
	var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
	db.Database.Migrate();
}

app.Run();