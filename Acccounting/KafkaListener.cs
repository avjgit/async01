using Accounting;
using Confluent.Kafka;

namespace Acccounting;

public class KafkaListener : BackgroundService
{
    private readonly ILogger<KafkaListener> _logger;
    private readonly IConsumer<String, AccountingTask> _consumer;
    private const String Topic = "tasksSchemed";

    public KafkaListener(IConsumer<String, AccountingTask> consumer, ILogger<KafkaListener> logger)
    {
        _logger = logger;
        _consumer = consumer;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _consumer.Subscribe(Topic);

        while(!stoppingToken.IsCancellationRequested)
        {
            var result = _consumer.Consume(stoppingToken);

            await HandleMessage(result.Message.Value, stoppingToken);
        }

        _consumer.Close();
    }

    protected virtual async Task HandleMessage(AccountingTask task, CancellationToken stoppingToken)
    {
        _logger.LogInformation($"Message Received: {task.Id}|{task.Assignee}");

        await Task.CompletedTask;
    }
}
