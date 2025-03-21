﻿using Confluent.Kafka;
using System.Text.Json;

namespace AntiFraudMicroservice.Infrastructure.Kafka
{
    public class KafkaProducer : IKafkaProducer
    {
        private readonly IProducer<string, string> _producer;

        public KafkaProducer(IConfiguration configuration)
        {
            var bootstrapServers = configuration["Kafka:BootstrapServers"] ?? throw new ArgumentNullException("Kafka:BootstrapServers is missing");

            var config = new ProducerConfig
            {
                BootstrapServers = bootstrapServers,
            };
            _producer = new ProducerBuilder<string, string>(config).Build();
        }

        public async Task ProduceAsync<T>(string topic, T message)
        {
            try
            {
                var jsonMessage = JsonSerializer.Serialize(message);
                await _producer.ProduceAsync(topic, new Message<string, string> { Key = Guid.NewGuid().ToString(), Value = jsonMessage });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error enviando mensaje a Kafka: {ex.Message}");
            }
        }
    }
}
