namespace TransactionMicroservice.Domain.Events
{
    public class TransactionUpdatedEvent
    {
        public Guid TransactionId { get; set; }
        public string Status { get; set; }

        public TransactionUpdatedEvent(Guid transactionId, string status)
        {
            TransactionId = transactionId;
            Status = status;
        }
    }
}
