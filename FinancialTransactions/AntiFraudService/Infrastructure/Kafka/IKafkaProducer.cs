namespace AntiFraudMicroservice.Infrastructure.Kafka
{
    public interface IKafkaProducer
    {
        Task ProduceAsync<T>(string topic, T message);
    }
}
