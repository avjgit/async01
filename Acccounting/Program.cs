using System;
using System.Text;
using Acccounting;
using Accounting;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using static System.Formats.Asn1.AsnWriter;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<EventsDb>(opt => opt.UseInMemoryDatabase("AccountingDb"));
builder.Services.AddHostedService<RabbitMqListener>();

var app = builder.Build();

app.UseHttpsRedirection();

app.MapGet("/log", async (EventsDb db) =>
    await db.Events.ToListAsync());

app.Run();
