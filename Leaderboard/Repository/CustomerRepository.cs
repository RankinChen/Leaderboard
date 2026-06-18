using Leaderboard.Dtos;

namespace Leaderboard.Repository
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly ILogger<CustomerRepository> _logger;
        private readonly SortedSet<CustomerModel> _customersSorted;
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
        private readonly Dictionary<long, decimal> _customerScores;

        public CustomerRepository(
            ILogger<CustomerRepository> logger)
        {
            this._logger = logger;
            this._lock = new SemaphoreSlim(1, 1);
            this._customersSorted = new SortedSet<CustomerModel>();
            this._customerScores = new Dictionary<long, decimal>();
        }

        public async Task<decimal> Update(long customerId, decimal score)
        {
            await _lock.WaitAsync();
            decimal currentScore = 0.0M;
            try
            {
                if (_customerScores.TryGetValue(customerId, out currentScore))
                {
                    _customersSorted.Remove(new CustomerModel(customerId, currentScore));
                }
                currentScore += score;

                _customersSorted.Add(new CustomerModel(customerId, currentScore));
                _customerScores[customerId] = currentScore;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Update Exception:{ex}");
            }
            finally
            {
                _lock.Release();
            }
            return currentScore;
        }

        public async Task<IEnumerable<CustomerModel>> GetLeaderboards(int start, int end)
        {

            if (start <= 0 || end <= 0 || end - start + 1 <= 0)
            {
                return _customersSorted.Where(x => x.Score > 0).ToList();
            }

            await _lock.WaitAsync();
            try
            {
                return _customersSorted.Skip(start - 1).Take(end - start + 1).Where(x => x.Score > 0).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetLeaderboards Exception:{ex}");
            }
            finally
            {
                _lock.Release();
            }
            return new List<CustomerModel>();
        }

        public async Task<int> GetCustomerRank(long customerId)
        {
            await _lock.WaitAsync();
            try
            {
                if (!_customerScores.TryGetValue(customerId, out decimal currentScore))
                    return -1;

                var customerModel = new CustomerModel(customerId, currentScore);
                int rank = 1 + _customersSorted.Count(item => item.CompareTo(customerModel) < 0);
                return rank;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Update Exception:{ex}");
            }
            finally
            {
                _lock.Release();
            }
            return -1;
        }
    }
}
