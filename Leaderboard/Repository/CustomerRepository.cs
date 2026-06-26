using Leaderboard.Dtos;

namespace Leaderboard.Repository
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly ILogger<CustomerRepository> _logger;
        private readonly SortedSet<CustomerModel> _customersSorted;
        private readonly ReaderWriterLockSlim _lock;
        private readonly Dictionary<long, decimal> _customerScores;
        private readonly Dictionary<long, int> _customerIdRank;

        public CustomerRepository(
            ILogger<CustomerRepository> logger)
        {
            this._logger = logger;
            this._lock = new(LockRecursionPolicy.NoRecursion);
            this._customersSorted = new SortedSet<CustomerModel>();
            this._customerScores = new Dictionary<long, decimal>();
            this._customerIdRank = new Dictionary<long, int>();
        }

        public decimal Update(long customerId, decimal score)
        {
            _lock.EnterWriteLock();
            var newScore = 0.0M;
            try
            {
                var newCustomerModel = ModifySorted(customerId, score);
                ModifyScores(customerId, newCustomerModel);

                ModifyRanks(customerId, newCustomerModel);

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

            return customerModel;
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

        private void ModifyRanks(long customerId, CustomerModel newCustomerModel)
        {
            var hadOldRank = _customerIdRank.TryGetValue(customerId, out int oldRank);

            var newRank = FindCustomerRank(customerId, newCustomerModel);

            _customerIdRank[customerId] = newRank;

            if (!hadOldRank)//如果是新用户 后面用户明次全部-1
            {
                foreach (var idRank in _customerIdRank.ToList())
                {
                    if (idRank.Value >= newRank && idRank.Key != customerId)
                        _customerIdRank[idRank.Key] = idRank.Value + 1;
                }
                return;
            }
            if (oldRank == newRank) return; //不更新其他用户rank

            if (newRank > oldRank) //如果新排名大于旧排名(降低分数)，则原来大于旧排名的用户 排名更新
            {
                var needModifyRanks = _customersSorted.Skip(oldRank - 1).Take(newRank - oldRank);
                foreach (var item in needModifyRanks)
                {
                    if (item.CustomerId != customerId)
                    {
                        _customerIdRank[item.CustomerId] = _customerIdRank[item.CustomerId] - 1;
                    }
                }
            }
            else //newRank < oldRank  如果新排名小于旧排名(升高分数)，则原来小于旧排名 && 大于等于新排名 的用户 排名+1
            {
                var needModifyRanks = _customersSorted.Skip(newRank).Take(oldRank - newRank);
                foreach (var item in needModifyRanks)
                {
                    if (item.CustomerId != customerId)
                    {
                        _customerIdRank[item.CustomerId] = _customerIdRank[item.CustomerId] + 1;
                    }
                }
            }
        }

        private int FindCustomerRank(long customerId, CustomerModel newCustomerModel)
        {
            var view = _customersSorted.GetViewBetween(new CustomerModel(long.MinValue, decimal.MaxValue), newCustomerModel);
            return view.Count;
        }

        public IEnumerable<CustomerModel> GetLeaderboards(int start, int end)
        {
            _lock.EnterReadLock();
            try
            {
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
            {
                return new List<CustomerModel>();
            }

            return _customersSorted.Skip(start - 1).Take(end - start + 1).Where(x => x.Score > 0).ToList();
        }

        public int GetCustomerRank(long customerId)
        {
            _lock.EnterReadLock();
            try
            {
                return _customerIdRank.TryGetValue(customerId, out int rank) ? rank : -1;
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetCustomerRank Exception:{ex}");
            }
            finally
            {
                _lock.ExitReadLock();
            }
            return -1;
        }

        public IEnumerable<CustomerModel> GetNeighborhoods(long customerId, NeighborhoodQuery query)
        {
            _lock.EnterReadLock();
            try
            {
                if (!_customerIdRank.TryGetValue(customerId, out int customerRank) || customerRank < 0)
                    return [];
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
