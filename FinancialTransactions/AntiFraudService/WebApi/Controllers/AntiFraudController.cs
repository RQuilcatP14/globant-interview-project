using AntiFraudMicroservice.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace AntiFraudMicroservice.WebApi.Controllers
{
    [ApiController]
    [Route("api/antifraud")]
    public class AntiFraudController : ControllerBase
    {
        private readonly IAntiFraudService _antiFraudService;

        public AntiFraudController(IAntiFraudService antiFraudService)
        {
            _antiFraudService = antiFraudService;
        }

        [HttpPost("check/{transactionId}")]
        public async Task<IActionResult> CheckTransaction(Guid transactionId, [FromQuery] decimal value)
        {
            await _antiFraudService.CheckTransactionAsync(transactionId, value);
            return Ok(new { message = "Transaction checked." });
        }
    }
}
