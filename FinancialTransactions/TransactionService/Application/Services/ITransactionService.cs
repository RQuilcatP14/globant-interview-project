using TransactionMicroservice.Domain.Models;

namespace TransactionMicroservice.Application.Services
{
    public interface ITransactionService
    {
        Task<Transaction> CreateTransactionAsync(Guid sourceAccountId, Guid targetAccountId, int transferTypeId, decimal value);
        Task<Transaction?> GetTransactionByIdAsync(Guid transactionId);
        Task UpdateTransactionStatusAsync(Guid transactionId, string status);
    }
}
