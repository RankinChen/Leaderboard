using Leaderboard.Dtos;
using Leaderboard.Services;
using Microsoft.AspNetCore.Mvc;
using System.Runtime;

namespace Leaderboard.Controllers
{
    [ApiController]
    [Route("customer")]
    public class CustomerController : ControllerBase
    {
        private readonly ILogger<CustomerController> _logger;
        private readonly ICustomerService _service;
        public CustomerController(ILogger<CustomerController> logger, ICustomerService customerService)
        {
            _logger = logger;
            _service = customerService;
        }

        [HttpPost("{customerid}/score/{score}")]
        public async Task<decimal> Score(long customerid, decimal score)
        {
            if (score < -1000 || score > 1000)
            {
                return 0.0M;
            }
            var currentScore = await _service.Update(customerid, score);

            return currentScore;
        }
    }
}
