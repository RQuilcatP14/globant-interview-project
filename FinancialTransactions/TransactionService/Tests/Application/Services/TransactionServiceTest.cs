using Moq;
using TransactionMicroservice.Application.Services;
using TransactionMicroservice.Domain.Models;
using TransactionMicroservice.Infrastructure.Kafka;
using TransactionMicroservice.Infrastructure.Repositories;
using Xunit;

namespace TransactionMicroservice.Tests
{
    public class TransactionServiceTests
    {
        private readonly Mock<ITransactionRepository> _transactionRepositoryMock;
        private readonly Mock<IKafkaProducer> _kafkaProducerMock;
        private readonly TransactionService _transactionService;

        public TransactionServiceTests()
        {
            _transactionRepositoryMock = new Mock<ITransactionRepository>();
            _kafkaProducerMock = new Mock<IKafkaProducer>();
            _transactionService = new TransactionService(_transactionRepositoryMock.Object, _kafkaProducerMock.Object);
        }

        [Fact]
        public async Task CreateTransactionAsync_ShouldCreateTransactionAndPublishEvent()
        {
            // Arrange
            var sourceAccountId = Guid.NewGuid();
            var targetAccountId = Guid.NewGuid();
            int transferTypeId = 1;
            decimal value = 1000m;

            _transactionRepositoryMock.Setup(repo => repo.AddTransactionAsync(It.IsAny<Transaction>()))
                .Returns(Task.CompletedTask);

            _kafkaProducerMock.Setup(producer => producer.ProduceAsync("transaction-topic", It.IsAny<Transaction>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _transactionService.CreateTransactionAsync(sourceAccountId, targetAccountId, transferTypeId, value);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("PENDING", result.Status);
            _transactionRepositoryMock.Verify(repo => repo.AddTransactionAsync(It.IsAny<Transaction>()), Times.Once);
            _kafkaProducerMock.Verify(producer => producer.ProduceAsync("transaction-topic", It.IsAny<Transaction>()), Times.Once);
        }

        [Fact]
        public async Task GetTransactionByIdAsync_ShouldReturnTransaction_WhenTransactionExists()
        {
            // Arrange
            var transactionId = Guid.NewGuid();
            var transaction = new Transaction
            {
                TransactionId = transactionId,
                SourceAccountId = Guid.NewGuid(),
                TargetAccountId = Guid.NewGuid(),
                TransferTypeId = 1,
                Value = 1000m,
                Status = "PENDING"
            };

            _transactionRepositoryMock.Setup(repo => repo.GetTransactionByIdAsync(transactionId))
                .ReturnsAsync(transaction);

            // Act
            var result = await _transactionService.GetTransactionByIdAsync(transactionId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(transactionId, result.TransactionId);
        }

        [Fact]
        public async Task GetTransactionByIdAsync_ShouldReturnNull_WhenTransactionDoesNotExist()
        {
            // Arrange
            var transactionId = Guid.NewGuid();
            _transactionRepositoryMock.Setup(repo => repo.GetTransactionByIdAsync(transactionId))
                .ReturnsAsync((Transaction?)null);

            // Act
            var result = await _transactionService.GetTransactionByIdAsync(transactionId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateTransactionStatusAsync_ShouldUpdateTransaction_WhenTransactionExists()
        {
            // Arrange
            var transactionId = Guid.NewGuid();
            var transaction = new Transaction
            {
                TransactionId = transactionId,
                Status = "PENDING"
            };

            _transactionRepositoryMock.Setup(repo => repo.GetTransactionByIdAsync(transactionId))
                .ReturnsAsync(transaction);

            _transactionRepositoryMock.Setup(repo => repo.UpdateTransactionAsync(It.IsAny<Transaction>()))
                .Returns(Task.CompletedTask);

            // Act
            await _transactionService.UpdateTransactionStatusAsync(transactionId, "APPROVED");

            // Assert
            Assert.Equal("APPROVED", transaction.Status);
            _transactionRepositoryMock.Verify(repo => repo.UpdateTransactionAsync(It.IsAny<Transaction>()), Times.Once);
        }

        [Fact]
        public async Task UpdateTransactionStatusAsync_ShouldDoNothing_WhenTransactionDoesNotExist()
        {
            // Arrange
            var transactionId = Guid.NewGuid();
            _transactionRepositoryMock.Setup(repo => repo.GetTransactionByIdAsync(transactionId))
                .ReturnsAsync((Transaction?)null);

            // Act
            await _transactionService.UpdateTransactionStatusAsync(transactionId, "APPROVED");

            // Assert
            _transactionRepositoryMock.Verify(repo => repo.UpdateTransactionAsync(It.IsAny<Transaction>()), Times.Never);
        }
    }
}
