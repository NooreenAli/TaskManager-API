using System.ComponentModel.DataAnnotations;

namespace TaskManager.API.DTOs;

public class CreateTaskDto
{
	[Required]
	[MaxLength(200)]
	public string Title { get; set; } = string.Empty;

	[MaxLength(1000)]
	public string? Description { get; set; }
}