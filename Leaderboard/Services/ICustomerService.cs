using Leaderboard.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Leaderboard.Services
{
    public interface ICustomerService
    {
        Task<decimal> Update(long id, decimal score);
        Task<IEnumerable<CustomerModel>> GetLeaderboards(LeaderBoardQuery query);
        Task<IEnumerable<CustomerModel>> GetNeighborhoods(long customerId, NeighborhoodQuery query);
    }
}
