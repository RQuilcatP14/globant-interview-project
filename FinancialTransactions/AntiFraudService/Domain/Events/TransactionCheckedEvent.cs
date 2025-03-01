namespace AntiFraudMicroservice.Domain.Events
{
    public class TransactionCheckedEvent
    {
        public Guid TransactionId { get; set; }
        public string Status { get; set; }

        public TransactionCheckedEvent(Guid transactionId, string status)
        {
            TransactionId = transactionId;
            Status = status;
        }
    }
}
