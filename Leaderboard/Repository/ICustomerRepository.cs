using Leaderboard.Dtos;

namespace Leaderboard.Repository
{
    public interface ICustomerRepository
    {
        Task<decimal> Update(long customerId, decimal score);
        Task<IEnumerable<CustomerModel>> GetLeaderboards(int start, int end);
        Task<int> GetCustomerRank(long customerId);
    }
}
