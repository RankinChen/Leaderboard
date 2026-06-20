namespace Leaderboard.Dtos
{
    public class CustomerModel : IComparable<CustomerModel>
    {
        public long CustomerId { get; set; }
        public decimal Score { get; set; }

        public CustomerModel(long customerId, decimal score)
        {
            CustomerId = customerId;
            Score = score;
        }

        public int CompareTo(CustomerModel? other)
        {
            if (other == null) return 0;

            int cmp = other.Score.CompareTo(this.Score);
            if (cmp != 0) return cmp;
            return this.CustomerId.CompareTo(other.CustomerId);
        }
    }
}
