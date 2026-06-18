using Microsoft.AspNetCore.Mvc;

namespace Leaderboard.Dtos
{
    public class NeighborhoodQuery
    {
        public int? High { get; set; }

        public int? Low { get; set; }

        public int CalcBeginRank(int currentRank)
        {
            if (High.HasValue)
            {
                return currentRank - High.Value;
            }
            return currentRank;
        }
        public int CalcEndRank(int currentRank)
        {
            if (Low.HasValue)
            {
                return currentRank + Low.Value;
            }
            return currentRank;
        }
    }
}
