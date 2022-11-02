using spikewall.Object;
using System;

namespace spikewall.Response
{
    /// <summary>
    /// Response containing roulette information
    /// </summary>
    public class LeagueDataResponse : BaseResponse
    {
        public LeagueData leagueData { get; set; }
        public long mode { get; set; }

        public LeagueDataResponse()
        {
            this.leagueData = new LeagueData();
            this.mode = 0;
        }
    }
}
