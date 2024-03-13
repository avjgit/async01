using Microsoft.EntityFrameworkCore;

namespace Accounting;

public class EventsDb(DbContextOptions<EventsDb> options) : DbContext(options)
{
    public DbSet<Event> Events => Set<Event>();
}