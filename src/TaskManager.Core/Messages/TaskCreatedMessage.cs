namespace TaskManager.Core.Messages;

public class TaskCreatedMessage
{
	public int TaskId { get; set; }
	public string Title { get; set; } = string.Empty;
	public string? Description { get; set; }
	public DateTime CreatedAt { get; set; }
}