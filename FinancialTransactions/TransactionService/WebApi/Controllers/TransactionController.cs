using Microsoft.AspNetCore.Mvc;
using TransactionMicroservice.Application.Services;
using TransactionMicroservice.Domain.Models;

namespace TransactionMicroservice.WebApi.Controllers
{
    [ApiController]
    [Route("api/transactions")]
    public class TransactionController : ControllerBase
    {
        private readonly ITransactionService _transactionService;

        public TransactionController(ITransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateTransaction(Transaction transaction)
        {
            var result = await _transactionService.CreateTransactionAsync(
                transaction.SourceAccountId, transaction.TargetAccountId,
                transaction.TransferTypeId, transaction.Value);
            return Ok(result);
        }
    }
}
