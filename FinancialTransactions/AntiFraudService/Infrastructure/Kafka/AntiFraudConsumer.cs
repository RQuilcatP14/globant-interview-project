using AntiFraudMicroservice.Application.Services;
using AntiFraudService.Domain.Events;
using Confluent.Kafka;
using System.Text.Json;

namespace AntiFraudMicroservice.Infrastructure.Kafka
{
    public class AntiFraudConsumer : IAntiFraudConsumer
    {
        private readonly IAntiFraudService _antiFraudService;

        public AntiFraudConsumer(IAntiFraudService antiFraudService)
        {
            _antiFraudService = antiFraudService;
        }

        public async Task ConsumeAsync()
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = "localhost:9092",
                GroupId = "antifraud-group",
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            using var consumer = new ConsumerBuilder<string, string>(config).Build();
            consumer.Subscribe("transaction-topic");

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
