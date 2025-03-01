using TransactionMicroservice.Domain.Models;
using TransactionMicroservice.Infrastructure.Kafka;
using TransactionMicroservice.Infrastructure.Repositories;

namespace TransactionMicroservice.Application.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IKafkaProducer _kafkaProducer;

        public TransactionService(ITransactionRepository transactionRepository, IKafkaProducer kafkaProducer)
        {
            _transactionRepository = transactionRepository;
            _kafkaProducer = kafkaProducer;
        }

        public async Task<Transaction> CreateTransactionAsync(Guid sourceAccountId, Guid targetAccountId, int transferTypeId, decimal value)
        {
            var transaction = new Transaction
            {
                TransactionId = Guid.NewGuid(),
                SourceAccountId = sourceAccountId,
                TargetAccountId = targetAccountId,
                TransferTypeId = transferTypeId,
                Value = value,
                Status = "PENDING"
            };

            await _transactionRepository.AddTransactionAsync(transaction);

            await _kafkaProducer.ProduceAsync("transaction-topic", transaction);

            return transaction;
        }

        public async Task<Transaction?> GetTransactionByIdAsync(Guid transactionId)
        {
            return await _transactionRepository.GetTransactionByIdAsync(transactionId);
        }

        public async Task UpdateTransactionStatusAsync(Guid transactionId, string status)
        {
            var transaction = await _transactionRepository.GetTransactionByIdAsync(transactionId);
            if (transaction != null)
            {
                transaction.Status = status;
                await _transactionRepository.UpdateTransactionAsync(transaction);
            }
        }
    }
}
