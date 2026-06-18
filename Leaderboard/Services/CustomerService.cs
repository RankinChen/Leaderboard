using Leaderboard.Dtos;
using Leaderboard.Repository;

namespace Leaderboard.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly ILogger<CustomerService> _logger;
        private readonly ICustomerRepository _repository;
        public CustomerService(
            ILogger<CustomerService> logger,
            ICustomerRepository repository)
        {
            _logger = logger;
            _repository = repository;
        }

        public async Task<decimal> Update(long id, decimal score)
        {
            return await _repository.Update(id, score);
        }

        public async Task<IEnumerable<CustomerModel>> GetLeaderboards(LeaderBoardQuery query)
        {
            var customers = await _repository.GetLeaderboards(query.Start, query.End);
            return customers;
        }

        public async Task<IEnumerable<CustomerModel>> GetNeighborhoods(long customerId, NeighborhoodQuery query)
        {
            var customerRank = await _repository.GetCustomerRank(customerId);
            if (customerRank < 0) return new List<CustomerModel>();

            var beginRank = query.CalcBeginRank(customerRank);
            var endRank = query.CalcEndRank(customerRank);

            var customers = await _repository.GetLeaderboards(beginRank, endRank);
            return customers;
        }
    }
}
