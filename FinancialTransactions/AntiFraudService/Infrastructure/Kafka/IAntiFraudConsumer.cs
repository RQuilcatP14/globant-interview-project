namespace AntiFraudMicroservice.Infrastructure.Kafka
{
    public interface IAntiFraudConsumer
    {
        Task ConsumeAsync();
    }
}
