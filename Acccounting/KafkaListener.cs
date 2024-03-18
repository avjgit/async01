using Accounting;
using Confluent.Kafka;
using static Confluent.Kafka.ConfigPropertyNames;

namespace Acccounting;

public class KafkaListener : BackgroundService
{
    private readonly ILogger<KafkaListener> _logger;
    private readonly IConsumer<String, AccountingTask> _consumer;
    private readonly IProducer<String, AccountingTask> _producer;
    private const String Topic = "tasksSchemed";
    private readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);

    public KafkaListener(
        IConsumer<String, AccountingTask> consumer, 
        IProducer<string, AccountingTask> producer,
        ILogger<KafkaListener> logger)
    {
        _logger = logger;
        _consumer = consumer;
        _producer = producer;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _producer.InitTransactions(DefaultTimeout);
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
        try
        {
            _logger.LogInformation($"Message Received: {task.Id}|{task.Assignee}"); // handle all the logic
        }
        catch (Exception ex)
        {
            var message = new Message<String, AccountingTask>
            {
                Value = task
            };
            _producer.ProduceAsync("tasksDeadLetterQueue", message, stoppingToken);
        }

        await Task.CompletedTask;
    }
}
