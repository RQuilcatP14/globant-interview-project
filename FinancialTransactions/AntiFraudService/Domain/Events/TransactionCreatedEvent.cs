namespace AntiFraudService.Domain.Events
{
    public class TransactionCreatedEvent
    {
        public Guid TransactionId { get; set; }
        public decimal Value { get; set; }

        public TransactionCreatedEvent(Guid transactionId, decimal value)
        {
            TransactionId = transactionId;
            Value = value;
        }
    }
}
