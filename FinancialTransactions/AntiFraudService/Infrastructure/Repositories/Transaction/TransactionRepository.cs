using AntiFraudMicroservice.Infrastructure.Persistence;
using AntiFraudMicroservice.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace AntiFraudMicroservice.Infrastructure.Repositories
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly AppDbContext _context;

        public TransactionRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddTransactionAsync(Transaction transaction)
        {
            await _context.Transactions.AddAsync(transaction);
            await _context.SaveChangesAsync();
        }

        public async Task<Transaction?> GetTransactionByIdAsync(Guid transactionId)
        {
            return await _context.Transactions.FindAsync(transactionId);
        }

        public async Task UpdateTransactionAsync(Transaction transaction)
        {
            _context.Transactions.Update(transaction);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Transaction>> GetTransactionsByDateAsync(DateTime date)
        {
            return await _context.Transactions
                .Where(t => t.CreatedAt.Date == date.Date)
                .ToListAsync();
        }
    }
}
