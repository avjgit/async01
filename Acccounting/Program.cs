using Confluent.Kafka;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using System;
using System.Text;
using Accounting;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using static System.Formats.Asn1.AsnWriter;
using Confluent.Kafka.SyncOverAsync;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<EventsDb>(opt => opt.UseInMemoryDatabase("AccountingDb"));
builder.Services.AddHostedService<RabbitMqListener>();
builder.Services.AddHostedService<KafkaListener>();

builder.Services.Configure<ConsumerConfig>(builder.Configuration.GetSection("Kafka"));

builder.Services.AddSingleton(sp =>
{
    var config = sp.GetRequiredService<IOptions<ConsumerConfig>>();

    return new ConsumerBuilder<String, AccountingTask>(config.Value)
        .SetValueDeserializer(new JsonDeserializer<AccountingTask>().AsSyncOverAsync())
        .Build();
});

var app = builder.Build();

app.UseHttpsRedirection();

app.MapGet("/log", async (EventsDb db) =>
    await db.Events.ToListAsync());

app.Run();
