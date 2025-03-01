using TransactionMicroservice.Domain.Models;

namespace TransactionMicroservice.Infrastructure.Repositories
{
    public interface ITransactionRepository
    {
        Task AddTransactionAsync(Transaction transaction);
        Task<Transaction?> GetTransactionByIdAsync(Guid transactionId);
        Task UpdateTransactionAsync(Transaction transaction);
    }
}
