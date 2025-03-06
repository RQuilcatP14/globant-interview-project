using AntiFraudMicroservice.Infrastructure.Kafka;
using AntiFraudMicroservice.Infrastructure.Repositories;
using AntiFraudService.Domain.Events;

namespace AntiFraudMicroservice.Application.Services
{
    public class AntiFraudService : IAntiFraudService
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IKafkaProducer _kafkaProducer;
        private readonly string _transactionUpdatedTopic;

        public AntiFraudService(ITransactionRepository transactionRepository, IKafkaProducer kafkaProducer, IConfiguration configuration)
        {
            _transactionRepository = transactionRepository;
            _kafkaProducer = kafkaProducer;
            _transactionUpdatedTopic = configuration["Kafka:TransactionUpdatedTopic"]
                ?? throw new ArgumentNullException("Kafka:TransactionUpdatedTopic is missing");
        }

        public async Task CheckTransactionAsync(Guid transactionId, decimal value)
        {
            var transaction = await _transactionRepository.GetTransactionByIdAsync(transactionId);
            if (transaction == null) return;

            // Verificar el total acumulado del día
            var todayTransactions = await _transactionRepository.GetTransactionsByDateAsync(DateTime.UtcNow);
            decimal totalAmountToday = todayTransactions.Sum(t => t.Value);

            // Aplicar reglas antifraude
            string status = (value > 2000 || totalAmountToday + value > 20000) ? "REJECTED" : "APPROVED";

            transaction.Status = status;
            await _transactionRepository.UpdateTransactionAsync(transaction);

            // Publicar evento de actualización en Kafka
            var updatedEvent = new TransactionUpdatedEvent(transactionId, status);
            await _kafkaProducer.ProduceAsync(_transactionUpdatedTopic, updatedEvent);
        }
    }
}
