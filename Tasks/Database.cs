using Microsoft.EntityFrameworkCore;

namespace Tasks;

class TasksDb(DbContextOptions<TasksDb> options) : DbContext(options)
{
    public DbSet<Task> Tasks => Set<Task>();
}