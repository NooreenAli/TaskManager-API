using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TaskManager.API.Controllers;
using TaskManager.API.DTOs;
using TaskManager.Core.Interfaces;
using TaskManager.Core.Models;
using TaskManager.Tests.Helpers;

namespace TaskManager.Tests.Controllers;

public class TasksControllerTests : TestBase
{
	private readonly Mock<ITaskRepository> _repositoryMock;
	private readonly Mock<IMessagePublisher> _publisherMock;
	private readonly TasksController _controller;

	public TasksControllerTests()
	{
		_repositoryMock = new Mock<ITaskRepository>();
		_publisherMock = new Mock<IMessagePublisher>();

		_controller = new TasksController(
			_repositoryMock.Object,
			Mapper,
			_publisherMock.Object);
	}

	[Fact]
	public async Task GetAll_WhenTasksExist_ReturnsOkWithAllTasks()
	{
		var tasks = new List<TaskItem>
		{
			new() { Id = 1, Title = "Task 1", IsCompleted = false, CreatedAt = DateTime.UtcNow },
			new() { Id = 2, Title = "Task 2", IsCompleted = true, CreatedAt = DateTime.UtcNow }
		};

		_repositoryMock
			.Setup(r => r.GetAllAsync())
			.ReturnsAsync(tasks);

		var result = await _controller.GetAll();

		var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
		var returnedTasks = okResult.Value.Should().BeAssignableTo<IEnumerable<TaskResponseDto>>().Subject;
		returnedTasks.Should().HaveCount(2);
		returnedTasks.First().Title.Should().Be("Task 1");
	}

	[Fact]
	public async Task GetAll_WhenNoTasksExist_ReturnsOkWithEmptyList()
	{
		_repositoryMock
			.Setup(r => r.GetAllAsync())
			.ReturnsAsync(new List<TaskItem>());

		var result = await _controller.GetAll();

		var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
		var returnedTasks = okResult.Value.Should().BeAssignableTo<IEnumerable<TaskResponseDto>>().Subject;
		returnedTasks.Should().BeEmpty();
	}

	[Fact]
	public async Task GetById_WhenTaskExists_ReturnsOkWithTask()
	{
		var task = new TaskItem
		{
			Id = 1,
			Title = "Test Task",
			Description = "Test Description",
			IsCompleted = false,
			CreatedAt = DateTime.UtcNow
		};

		_repositoryMock
			.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
			.ReturnsAsync(task);

		var result = await _controller.GetById(1);

		var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
		var returnedTask = okResult.Value.Should().BeOfType<TaskResponseDto>().Subject;
		returnedTask.Id.Should().Be(1);
		returnedTask.Title.Should().Be("Test Task");
		returnedTask.Description.Should().Be("Test Description");
	}

	[Fact]
	public async Task GetById_WhenTaskDoesNotExist_ReturnsNotFound()
	{
		_repositoryMock
			.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
			.ReturnsAsync((TaskItem?)null);

		var result = await _controller.GetById(999);

		result.Result.Should().BeOfType<NotFoundResult>();
	}


	[Fact]
	public async Task Create_WithValidData_ReturnsCreatedAtActionWithTask()
	{
		var dto = new CreateTaskDto
		{
			Title = "New Task",
			Description = "New Description"
		};

		var createdTask = new TaskItem
		{
			Id = 1,
			Title = dto.Title,
			Description = dto.Description,
			IsCompleted = false,
			CreatedAt = DateTime.UtcNow
		};

		_repositoryMock
			.Setup(r => r.CreateAsync(It.IsAny<TaskItem>()))
			.ReturnsAsync(createdTask);

		_publisherMock
			.Setup(p => p.PublishAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
			.Returns(Task.CompletedTask);

		var result = await _controller.Create(dto);

		var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
		createdResult.ActionName.Should().Be(nameof(TasksController.GetById));

		var returnedTask = createdResult.Value.Should().BeOfType<TaskResponseDto>().Subject;
		returnedTask.Id.Should().Be(1);
		returnedTask.Title.Should().Be("New Task");

		_publisherMock.Verify(
			p => p.PublishAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()),
			Times.Once);
	}

	[Fact]
	public async Task Create_VerifiesRepositoryIsCalledOnce()
	{
		var dto = new CreateTaskDto { Title = "Test" };
		var createdTask = new TaskItem { Id = 1, Title = "Test", CreatedAt = DateTime.UtcNow };

		_repositoryMock
			.Setup(r => r.CreateAsync(It.IsAny<TaskItem>()))
			.ReturnsAsync(createdTask);

		await _controller.Create(dto);

		_repositoryMock.Verify(
			r => r.CreateAsync(It.IsAny<TaskItem>()),
			Times.Once);
	}


	[Fact]
	public async Task Update_WhenTaskExists_ReturnsOkWithUpdatedTask()
	{
		var dto = new UpdateTaskDto
		{
			Title = "Updated Title",
			Description = "Updated Description",
			IsCompleted = true
		};

		var updatedTask = new TaskItem
		{
			Id = 1,
			Title = dto.Title,
			Description = dto.Description,
			IsCompleted = dto.IsCompleted,
			CreatedAt = DateTime.UtcNow
		};

		_repositoryMock
			.Setup(r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<TaskItem>()))
			.ReturnsAsync(updatedTask);

		var result = await _controller.Update(1, dto);

		var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
		var returnedTask = okResult.Value.Should().BeOfType<TaskResponseDto>().Subject;
		returnedTask.Title.Should().Be("Updated Title");
		returnedTask.IsCompleted.Should().BeTrue();
	}

	[Fact]
	public async Task Update_WhenTaskDoesNotExist_ReturnsNotFound()
	{
		_repositoryMock
			.Setup(r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<TaskItem>()))
			.ReturnsAsync((TaskItem?)null);

		var result = await _controller.Update(999, new UpdateTaskDto { Title = "Test" });

		result.Result.Should().BeOfType<NotFoundResult>();
	}

	[Fact]
	public async Task Delete_WhenTaskExists_ReturnsNoContent()
	{
		_repositoryMock
			.Setup(r => r.DeleteAsync(It.IsAny<int>()))
			.ReturnsAsync(true);

		var result = await _controller.Delete(1);

		result.Should().BeOfType<NoContentResult>();
	}

	[Fact]
	public async Task Delete_WhenTaskDoesNotExist_ReturnsNotFound()
	{
		_repositoryMock
			.Setup(r => r.DeleteAsync(It.IsAny<int>()))
			.ReturnsAsync(false);

		var result = await _controller.Delete(999);

		result.Should().BeOfType<NotFoundResult>();
	}
}