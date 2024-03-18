using Confluent.Kafka;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using Tasks;
using static Confluent.Kafka.ConfigPropertyNames;


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<TasksDb>(opt => opt.UseInMemoryDatabase("AllThePopugsTasks"));

var kafkaProducerConfig = builder.Services.Configure<ProducerConfig>(
    builder.Configuration.GetSection("Kafka"));

builder.Services.Configure<SchemaRegistryConfig>(
    builder.Configuration.GetSection("SchemaRegistry"));

builder.Services.AddSingleton<ISchemaRegistryClient>(sp =>
{
    var config = sp.GetRequiredService<IOptions<SchemaRegistryConfig>>();

    return new CachedSchemaRegistryClient(config.Value);
});

builder.Services.AddSingleton<IProducer<String, Tasks.Task>>(sp =>
{
    var config = sp.GetRequiredService<IOptions<ProducerConfig>>();
    var schemaRegistry = sp.GetRequiredService<ISchemaRegistryClient>();

    return new ProducerBuilder<String, Tasks.Task>(config.Value)
        .SetValueSerializer(new JsonSerializer<Tasks.Task>(schemaRegistry))
            .Build();
});

var app = builder.Build();
app.UseHttpsRedirection();

app.MapGet("/tasks", async (TasksDb db) =>
    await db.Tasks.ToListAsync());

app.MapGet("/tasks/{id}", async (int id, TasksDb db) =>
    await db.Tasks.FindAsync(id)
        is Tasks.Task task
            ? Results.Ok(task)
            : Results.NotFound());

app.MapPost("/tasks", async (Tasks.Task task, TasksDb db, IProducer<String, Tasks.Task> producer) =>
{
    db.Tasks.Add(task);
    await db.SaveChangesAsync();

    var factory = new ConnectionFactory { HostName = "localhost" };
    using var connection = factory.CreateConnection();
    using var channel = connection.CreateModel();

    channel.QueueDeclare(queue: "tasks",
                     durable: false,
                     exclusive: false,
                     autoDelete: false,
                     arguments: null);

    string message = $"Task {task.Id} created for {task.Assignee}";
    var body = Encoding.UTF8.GetBytes(message);

    channel.BasicPublish(exchange: string.Empty,
                     routingKey: "tasks",
                     basicProperties: null,
    body: body);


    TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);

    producer.InitTransactions(DefaultTimeout);
    producer.BeginTransaction();
    try
    {

        producer.BeginTransaction();

        var result = await producer.ProduceAsync("tasksSchemed", new Message<String, Tasks.Task>
        {
            Value = task
        });
        producer.CommitTransaction();

        producer.Flush();
    }
    catch (Exception ex)
    {
        producer.AbortTransaction();
    }


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