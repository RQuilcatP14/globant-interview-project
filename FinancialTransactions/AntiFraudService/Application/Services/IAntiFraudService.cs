namespace AntiFraudMicroservice.Application.Services
{
    public interface IAntiFraudService
    {
        Task CheckTransactionAsync(Guid transactionId, decimal value);
    }
}
