using Microsoft.EntityFrameworkCore;
using TaskManager.Core.Models;

namespace TaskManager.Infrastructure.Data;

public class AppDbContext : DbContext
{
	public DbSet<TaskItem> Tasks { get; set; }

	public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
	{
	}

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<TaskItem>(entity =>
		{
			entity.ToTable("Tasks");

			entity.HasKey(e => e.Id);

			entity.Property(e => e.Title)
				  .IsRequired()
				  .HasMaxLength(200);

			entity.Property(e => e.Description)
				  .HasMaxLength(1000);

			entity.Property(e => e.CreatedAt)
				  .ValueGeneratedOnAdd();
		});
	}
}