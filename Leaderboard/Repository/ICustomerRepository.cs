using Leaderboard.Dtos;

namespace Leaderboard.Repository
{
    public interface ICustomerRepository
    {
        decimal Update(long customerId, decimal score);
        IEnumerable<CustomerModel> GetLeaderboards(int start, int end);
        int GetCustomerRank(long customerId);
        IEnumerable<CustomerModel> GetNeighborhoods(long customerId, NeighborhoodQuery query);
    }
}
