namespace TransactionMicroservice.Domain.Models
{
    public class Transaction
    {
        public Guid TransactionId { get; set; }
        public Guid SourceAccountId { get; set; }
        public Guid TargetAccountId { get; set; }
        public int TransferTypeId { get; set; }
        public decimal Value { get; set; }
        public string Status { get; set; } = "PENDING";
    }
}
