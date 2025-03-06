using AntiFraudMicroservice.Application.Services;
using AntiFraudService.Domain.Events;
using Confluent.Kafka;
using System.Text.Json;

namespace AntiFraudMicroservice.Infrastructure.Kafka
{
    public class AntiFraudConsumer : IAntiFraudConsumer
    {
        private readonly IAntiFraudService _antiFraudService;
        private readonly string _bootstrapServers;
        private readonly string _groupId;
        private readonly string _transactionTopic;

        public AntiFraudConsumer(IAntiFraudService antiFraudService, IConfiguration configuration)
        {
            _antiFraudService = antiFraudService;
            _bootstrapServers = configuration["Kafka:BootstrapServers"] ?? throw new ArgumentNullException("Kafka:BootstrapServers is missing");
            _groupId = configuration["Kafka:GroupId"] ?? throw new ArgumentNullException("Kafka:GroupId is missing");
            _transactionTopic = configuration["Kafka:TransactionTopic"] ?? throw new ArgumentNullException("Kafka:TransactionTopic is missing");
        }

        public async Task ConsumeAsync()
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = _bootstrapServers,
                GroupId = _groupId,
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            using var consumer = new ConsumerBuilder<string, string>(config).Build();
            consumer.Subscribe(_transactionTopic);

            while (true)
            {
                try
                {
                    var consumeResult = consumer.Consume();
                    var transactionCreated = JsonSerializer.Deserialize<TransactionCreatedEvent>(consumeResult.Value);

                    if (transactionCreated != null)
                    {
                        await _antiFraudService.CheckTransactionAsync(transactionCreated.TransactionId, transactionCreated.Value);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error procesando el mensaje: {ex.Message}");
                }
            }
        }
    }
}
