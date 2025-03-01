using TransactionMicroservice.Infrastructure.Persistence;

namespace TransactionMicroservice.Infrastructure.Repositories
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly AppDbContext _context;

        public TransactionRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddTransactionAsync(Domain.Models.Transaction transaction)
        {
            await _context.Transactions.AddAsync(transaction);
            await _context.SaveChangesAsync();
        }

        public async Task<Domain.Models.Transaction?> GetTransactionByIdAsync(Guid transactionId)
        {
            return await _context.Transactions.FindAsync(transactionId);
        }

        public async Task UpdateTransactionAsync(Domain.Models.Transaction transaction)
        {
            _context.Transactions.Update(transaction);
            await _context.SaveChangesAsync();
        }
    }
}
