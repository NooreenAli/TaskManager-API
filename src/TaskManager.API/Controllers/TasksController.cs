using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using TaskManager.API.DTOs;
using TaskManager.Core.Interfaces;
using TaskManager.Core.Messages;
using TaskManager.Core.Models;

namespace TaskManager.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
	private readonly ITaskRepository _repository;
	private readonly IMapper _mapper;
	private readonly IMessagePublisher _publisher;

	public TasksController(
		ITaskRepository repository,
		IMapper mapper,
		IMessagePublisher publisher)
	{
		_repository = repository;
		_mapper = mapper;
		_publisher = publisher;
	}

	[HttpGet]
	public async Task<ActionResult<IEnumerable<TaskResponseDto>>> GetAll()
	{
		var tasks = await _repository.GetAllAsync();
		return Ok(_mapper.Map<IEnumerable<TaskResponseDto>>(tasks));
	}

	[HttpGet("{id}")]
	public async Task<ActionResult<TaskResponseDto>> GetById(int id)
	{
		var task = await _repository.GetByIdAsync(id);
		if (task is null) return NotFound();
		return Ok(_mapper.Map<TaskResponseDto>(task));
	}

	[HttpPost]
	public async Task<ActionResult<TaskResponseDto>> Create(CreateTaskDto dto)
	{
		var task = _mapper.Map<TaskItem>(dto);
		var created = await _repository.CreateAsync(task);

		var message = new TaskCreatedMessage
		{
			TaskId = created.Id,
			Title = created.Title,
			Description = created.Description,
			CreatedAt = created.CreatedAt
		};

		await _publisher.PublishAsync(message);

		return CreatedAtAction(
			nameof(GetById),
			new { id = created.Id },
			_mapper.Map<TaskResponseDto>(created));
	}

	[HttpPut("{id}")]
	public async Task<ActionResult<TaskResponseDto>> Update(int id, UpdateTaskDto dto)
	{
		var task = _mapper.Map<TaskItem>(dto);
		var result = await _repository.UpdateAsync(id, task);
		if (result is null) return NotFound();
		return Ok(_mapper.Map<TaskResponseDto>(result));
	}

	[HttpDelete("{id}")]
	public async Task<IActionResult> Delete(int id)
	{
		var deleted = await _repository.DeleteAsync(id);
		if (!deleted) return NotFound();
		return NoContent();
	}
}