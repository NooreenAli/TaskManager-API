using AutoMapper;
using Microsoft.Extensions.Logging.Abstractions;
using TaskManager.API.Mappings;

namespace TaskManager.Tests.Helpers;

public abstract class TestBase
{
	protected readonly IMapper Mapper;

	protected TestBase()
	{
		Mapper = new MapperConfiguration(cfg =>
		{
			cfg.AddProfile<TaskMappingProfile>();
		}, NullLoggerFactory.Instance).CreateMapper();
	}
}