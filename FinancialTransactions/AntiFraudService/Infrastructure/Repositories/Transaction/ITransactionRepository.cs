using AntiFraudMicroservice.Domain.Models;

namespace AntiFraudMicroservice.Infrastructure.Repositories
{
    public interface ITransactionRepository
    {
        Task AddTransactionAsync(Transaction transaction);
        Task<Transaction?> GetTransactionByIdAsync(Guid transactionId);
        Task UpdateTransactionAsync(Transaction transaction);
    }
}
