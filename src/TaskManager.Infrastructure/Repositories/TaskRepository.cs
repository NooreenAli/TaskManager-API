using Microsoft.EntityFrameworkCore;
using TaskManager.Core.Interfaces;
using TaskManager.Core.Models;
using TaskManager.Infrastructure.Data;

namespace TaskManager.Infrastructure.Repositories;

public class TaskRepository : ITaskRepository
{
	private readonly AppDbContext _context;

	public TaskRepository(AppDbContext context)
	{
		_context = context;
	}

	public async Task<IEnumerable<TaskItem>> GetAllAsync()
	{
		return await _context.Tasks
			.AsNoTracking()
			.ToListAsync();
	}

	public async Task<TaskItem?> GetByIdAsync(int id)
	{
		return await _context.Tasks
			.AsNoTracking()
			.FirstOrDefaultAsync(t => t.Id == id);
	}

	public async Task<TaskItem> CreateAsync(TaskItem task)
	{
		_context.Tasks.Add(task);
		await _context.SaveChangesAsync();

		return task;
	}

	public async Task<TaskItem?> UpdateAsync(int id, TaskItem updated)
	{
		var existing = await _context.Tasks.FindAsync(id);
		if (existing is null) return null;

		existing.Title = updated.Title;
		existing.Description = updated.Description;
		existing.IsCompleted = updated.IsCompleted;

		await _context.SaveChangesAsync();
		return existing;
	}

	public async Task<bool> DeleteAsync(int id)
	{
		var task = await _context.Tasks.FindAsync(id);
		if (task is null) return false;

		_context.Tasks.Remove(task);
		await _context.SaveChangesAsync();
		return true;
	}
}