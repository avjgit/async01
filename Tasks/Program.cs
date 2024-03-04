using Microsoft.EntityFrameworkCore;
using Tasks;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<TasksDb>(opt => opt.UseInMemoryDatabase("AllThePopugsTasks"));

var app = builder.Build();
app.UseHttpsRedirection();

app.MapGet("/tasks", async (TasksDb db) =>
    await db.Tasks.ToListAsync());

app.MapGet("/tasks/{id}", async (int id, TasksDb db) =>
    await db.Tasks.FindAsync(id)
        is Tasks.Task task
            ? Results.Ok(task)
            : Results.NotFound());

app.MapPost("/tasks", async (Tasks.Task task, TasksDb db) =>
{
    db.Tasks.Add(task);
    await db.SaveChangesAsync();
    return Results.Created($"/tasks/{task.Id}", task);
});

app.MapPut("/tasks/{id}/{assignee}", async (int id, string assignee, TasksDb db) =>
{
    var task = await db.Tasks.FindAsync(id);
    if (task is null) return Results.NotFound();
    task.Assignee = assignee;
    await db.SaveChangesAsync();
    return Results.NoContent();
});

//app.MapDelete("/tasks/{id}", async (int id, TasksDb db) =>
//{
//    if (await db.Tasks.FindAsync(id) is Tasks.Task task)
//    {
//        db.Tasks.Remove(task);
//        await db.SaveChangesAsync();
//        return Results.NoContent();
//    }

//    return Results.NotFound();
//});

app.Run();