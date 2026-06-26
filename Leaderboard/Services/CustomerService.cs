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
            return await Task.FromResult(_repository.Update(id, score));
        }

        public async Task<IEnumerable<CustomerModel>> GetLeaderboards(LeaderBoardQuery query)
        {
            var customers = await Task.FromResult(_repository.GetLeaderboards(query.Start, query.End));
            return customers;
        }

        public async Task<IEnumerable<CustomerModel>> GetNeighborhoods(long customerId, NeighborhoodQuery query)
        {
            var customers = await Task.FromResult(_repository.GetNeighborhoods(customerId, query));
            return customers;
        }
    }
}
