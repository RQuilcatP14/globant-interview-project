using Moq;
using AntiFraudMicroservice.Infrastructure.Kafka;
using AntiFraudMicroservice.Infrastructure.Repositories;
using AntiFraudService.Domain.Events;
using Xunit;
using AntiFraudMicroservice.Domain.Models;

namespace AntiFraudMicroservice.Tests
{
    public class AntiFraudServiceTests
    {
        private readonly Mock<ITransactionRepository> _transactionRepositoryMock;
        private readonly Mock<IKafkaProducer> _kafkaProducerMock;
        private readonly IConfiguration _configuration;
        private readonly AntiFraudMicroservice.Application.Services.AntiFraudService _antiFraudService;
        private const string KafkaTopic = "transaction-updated-topic";

        public AntiFraudServiceTests()
        {
            _transactionRepositoryMock = new Mock<ITransactionRepository>();
            _kafkaProducerMock = new Mock<IKafkaProducer>();

            var inMemorySettings = new Dictionary<string, string>
            {
                { "Kafka:TransactionUpdatedTopic", KafkaTopic }
            };

            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            _antiFraudService = new AntiFraudMicroservice.Application.Services.AntiFraudService(
                _transactionRepositoryMock.Object,
                _kafkaProducerMock.Object,
                _configuration
            );
        }

        [Fact]
        public async Task CheckTransactionAsync_ShouldApproveTransaction_WhenBelowThreshold()
        {
            // Arrange
            var transactionId = Guid.NewGuid();
            decimal value = 1500m;

            var transaction = new Transaction
            {
                Id = transactionId,
                Value = value,
                Status = "PENDING"
            };

            var todayTransactions = new List<Transaction>
            {
                new Transaction { Value = 5000m },
                new Transaction { Value = 3000m }
            };

            _transactionRepositoryMock.Setup(repo => repo.GetTransactionByIdAsync(transactionId))
                .ReturnsAsync(transaction);

            _transactionRepositoryMock.Setup(repo => repo.GetTransactionsByDateAsync(It.IsAny<DateTime>()))
                .ReturnsAsync(todayTransactions);

            _transactionRepositoryMock.Setup(repo => repo.UpdateTransactionAsync(transaction))
                .Returns(Task.CompletedTask);

            _kafkaProducerMock.Setup(producer => producer.ProduceAsync(KafkaTopic, It.IsAny<TransactionUpdatedEvent>()))
                .Returns(Task.CompletedTask);

            // Act
            await _antiFraudService.CheckTransactionAsync(transactionId, value);

            // Assert
            Assert.Equal("APPROVED", transaction.Status);
            _transactionRepositoryMock.Verify(repo => repo.UpdateTransactionAsync(transaction), Times.Once);
            _kafkaProducerMock.Verify(producer => producer.ProduceAsync(KafkaTopic, It.IsAny<TransactionUpdatedEvent>()), Times.Once);
        }

        [Fact]
        public async Task CheckTransactionAsync_ShouldRejectTransaction_WhenExceedsThreshold()
        {
            // Arrange
            var transactionId = Guid.NewGuid();
            decimal value = 2500m;

            var transaction = new Transaction
            {
                Id = transactionId,
                Value = value,
                Status = "PENDING"
            };

            var todayTransactions = new List<Transaction>
            {
                new Transaction { Value = 10000m },
                new Transaction { Value = 9000m }
            };

            _transactionRepositoryMock.Setup(repo => repo.GetTransactionByIdAsync(transactionId))
                .ReturnsAsync(transaction);

            _transactionRepositoryMock.Setup(repo => repo.GetTransactionsByDateAsync(It.IsAny<DateTime>()))
                .ReturnsAsync(todayTransactions);

            _transactionRepositoryMock.Setup(repo => repo.UpdateTransactionAsync(transaction))
                .Returns(Task.CompletedTask);

            _kafkaProducerMock.Setup(producer => producer.ProduceAsync(KafkaTopic, It.IsAny<TransactionUpdatedEvent>()))
                .Returns(Task.CompletedTask);

            // Act
            await _antiFraudService.CheckTransactionAsync(transactionId, value);

            // Assert
            Assert.Equal("REJECTED", transaction.Status);
            _transactionRepositoryMock.Verify(repo => repo.UpdateTransactionAsync(transaction), Times.Once);
            _kafkaProducerMock.Verify(producer => producer.ProduceAsync(KafkaTopic, It.IsAny<TransactionUpdatedEvent>()), Times.Once);
        }

        [Fact]
        public async Task CheckTransactionAsync_ShouldDoNothing_WhenTransactionDoesNotExist()
        {
            // Arrange
            var transactionId = Guid.NewGuid();
            decimal value = 1500m;

            _transactionRepositoryMock.Setup(repo => repo.GetTransactionByIdAsync(transactionId))
                .ReturnsAsync((Transaction?)null);

            // Act
            await _antiFraudService.CheckTransactionAsync(transactionId, value);

            // Assert
            _transactionRepositoryMock.Verify(repo => repo.UpdateTransactionAsync(It.IsAny<Transaction>()), Times.Never);
            _kafkaProducerMock.Verify(producer => producer.ProduceAsync(It.IsAny<string>(), It.IsAny<TransactionUpdatedEvent>()), Times.Never);
        }
    }
}
