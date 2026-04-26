using TaskManager.Core.Models;

namespace TaskManager.Core.Interfaces;

public interface ITaskRepository
{
	Task<IEnumerable<TaskItem>> GetAllAsync();
	Task<TaskItem?> GetByIdAsync(int id);
	Task<TaskItem> CreateAsync(TaskItem task);
	Task<TaskItem?> UpdateAsync(int id, TaskItem task);
	Task<bool> DeleteAsync(int id);
}