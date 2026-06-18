using Leaderboard.Dtos;
using Leaderboard.Services;
using Microsoft.AspNetCore.Mvc;

namespace Leaderboard.Controllers
{
    [ApiController]
    [Route("leaderboard")]
    public class LeaderboardController : ControllerBase
    {
        private readonly ILogger<LeaderboardController> _logger;
        private readonly ICustomerService _service;

        public LeaderboardController(ILogger<LeaderboardController> logger, ICustomerService customerService)
        {
            _logger = logger;
            _service = customerService;
        }

        [HttpGet]
        public async Task<IEnumerable<CustomerModel>> GetLeaderBoardsAsync([FromQuery] LeaderBoardQuery query)
        {
            var customers = await _service.GetLeaderboards(query);
            return customers;
        }

        [HttpGet("/{customerId}")]
        public async Task<IEnumerable<CustomerModel>> GetNeighborhoods([FromRoute] long customerId, [FromQuery] NeighborhoodQuery query)
        {
            var customers = await _service.GetNeighborhoods(customerId, query);
            return customers;
        }
    }
}
