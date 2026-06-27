using Leaderboard.Dtos;

namespace Leaderboard.Repository
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly ILogger<CustomerRepository> _logger;
        private readonly ReaderWriterLockSlim _lock;
        private readonly SortedSet<CustomerModel> _customersSorted;     //排序数据
        private readonly Dictionary<long, decimal> _customerScores;     //客户分数数据
        private volatile bool _rebuildSnapshot;
        private int _rebuildingFlag;
        private List<CustomerModel> _customersSortedSnapshoot; //排序数据的快照
        public CustomerRepository(
            ILogger<CustomerRepository> logger)
        {
            this._logger = logger;
            this._lock = new(LockRecursionPolicy.NoRecursion);
            this._customersSorted = new SortedSet<CustomerModel>();
            this._customersSortedSnapshoot = new List<CustomerModel>();
            this._customerScores = new Dictionary<long, decimal>();
            this._rebuildSnapshot = false;
        }

        public decimal Update(long customerId, decimal score)
        {
            _lock.EnterWriteLock();
            var newScore = 0.0M;
            try
            {
                var newCustomerModel = ModifySorted(customerId, score);
                ModifyScores(customerId, newCustomerModel);

                newScore = newCustomerModel.Score;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Update Exception:{ex}");
            }
            finally
            {
                _lock.ExitWriteLock();
            }
            return newScore;
        }

        private CustomerModel ModifySorted(long customerId, decimal score)
        {
            decimal newScore = 0.0M;
            _customerScores.TryGetValue(customerId, out decimal oldScore);

            if (oldScore != default || _customerScores.ContainsKey(customerId))
            {
                _customersSorted.Remove(new CustomerModel(customerId, oldScore));
            }

            newScore = oldScore + score;
            var customerModel = new CustomerModel(customerId, newScore);
            _customersSorted.Add(customerModel);

            _rebuildSnapshot = true;

            return customerModel;
        }
        private void RebuildCustomersSnapshoot()
        {
            if (!_rebuildSnapshot)
                return;

            if (Interlocked.CompareExchange(ref _rebuildingFlag, 1, 0) != 0)
                return;

            try
            {
                _customersSortedSnapshoot = _customersSorted.ToList();
                _rebuildSnapshot = false;
            }
            finally
            {
                _rebuildingFlag = 0;
            }
        }

        private void ModifyScores(long customerId, CustomerModel newCustomerModel)
        {
            if (newCustomerModel.Score <= 0)
            {
                _customerScores.Remove(customerId);
                return;
            }
            _customerScores[customerId] = newCustomerModel.Score;
        }
        public IEnumerable<CustomerModel> GetLeaderboards(int start, int end)
        {
            _lock.EnterReadLock();
            try
            {
                RebuildCustomersSnapshoot();

                return GetLeaderboardRange(start, end);
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetLeaderboards Exception:{ex}");
            }
            finally
            {
                _lock.ExitReadLock();
            }
            return [];
        }

        private IEnumerable<CustomerModel> GetLeaderboardRange(int start, int end)
        {
            if (start <= 0 || end <= 0 || end - start + 1 <= 0)
                return new List<CustomerModel>();

            start = Math.Max(1, start);
            end = Math.Min(_customersSortedSnapshoot.Count, end);

            if (start > end) return new List<CustomerModel>();

            return _customersSortedSnapshoot.GetRange(start - 1, end - start + 1);
        }
        private int GetCustomerRank(long customerId)
        {
            var score = GetCustomerScore(customerId);

            var customerModel = new CustomerModel(customerId, score);
            var view = _customersSorted.GetViewBetween(new CustomerModel(long.MinValue, decimal.MaxValue), customerModel);
            return view.Count;
        }
        private decimal GetCustomerScore(long customerId)
        {
            return _customerScores.TryGetValue(customerId, out decimal score) ? score : -1;
        }

        public IEnumerable<CustomerModel> GetNeighborhoods(long customerId, NeighborhoodQuery query)
        {
            _lock.EnterReadLock();
            try
            {
                RebuildCustomersSnapshoot();

                var customerRank = GetCustomerRank(customerId);

                var beginRank = query.CalcBeginRank(customerRank);
                var endRank = query.CalcEndRank(customerRank);

                var customers = GetLeaderboardRange(beginRank, endRank);
                return customers;

            }
            catch (Exception ex)
            {
                _logger.LogError($"GetCustomerRank Exception:{ex}");
            }
            finally
            {
                _lock.ExitReadLock();
            }
            return [];
        }
    }
}
