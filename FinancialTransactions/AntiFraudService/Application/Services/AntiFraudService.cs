using AntiFraudMicroservice.Infrastructure.Kafka;
using AntiFraudMicroservice.Infrastructure.Repositories;
using AntiFraudService.Domain.Events;

namespace AntiFraudMicroservice.Application.Services
{
    public class AntiFraudService : IAntiFraudService
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IKafkaProducer _kafkaProducer;

        public AntiFraudService(ITransactionRepository transactionRepository, IKafkaProducer kafkaProducer)
        {
            _transactionRepository = transactionRepository;
            _kafkaProducer = kafkaProducer;
        }

        public async Task CheckTransactionAsync(Guid transactionId, decimal value)
        {
            var transaction = await _transactionRepository.GetTransactionByIdAsync(transactionId);
            if (transaction == null) return;

            // Reglas antifraude: Si el valor > 10000, se marca como "REJECTED"
            string status = value > 10000 ? "REJECTED" : "APPROVED";

            transaction.Status = status;
            await _transactionRepository.UpdateTransactionAsync(transaction);

            // Publicar evento de actualización en Kafka
            var updatedEvent = new TransactionUpdatedEvent(transactionId, status);
            await _kafkaProducer.ProduceAsync("transaction-updated-topic", updatedEvent);
        }
    }
}
