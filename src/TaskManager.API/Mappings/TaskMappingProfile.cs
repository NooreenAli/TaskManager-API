using AutoMapper;
using TaskManager.API.DTOs;
using TaskManager.Core.Models;

namespace TaskManager.API.Mappings;

public class TaskMappingProfile : Profile
{
	public TaskMappingProfile()
	{
		CreateMap<CreateTaskDto, TaskItem>();

		CreateMap<UpdateTaskDto, TaskItem>();

		CreateMap<TaskItem, TaskResponseDto>();
	}
}