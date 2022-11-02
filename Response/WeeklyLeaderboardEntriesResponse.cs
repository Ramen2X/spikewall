using spikewall.Object;
using System;

namespace spikewall.Response
{
    /// <summary>
    /// Response containing cost list (FIXME: Elaborate)
    /// </summary>
    public class WeeklyLeaderboardEntriesResponse : BaseResponse
    {
        public PlayerEntry? playerEntry { get; set; }
        public long? lastOffset { get; set; }
        public long? startTime { get; set; }
        public long? resetTime { get; set; }
        public long? startIndex { get; set; }
        public long? mode { get; set; }
        public long? totalEntries { get; set; }

        // FIXME: This is an array but shouldn't actually be strings, set up "LeaderboardEntry" object
        public PlayerEntry[]? entriesList { get; set; }

        public WeeklyLeaderboardEntriesResponse()
        {
            this.playerEntry = new PlayerEntry();
            this.lastOffset = 0;
            this.startTime = 0;
            this.resetTime = 0;
            this.startIndex = 0;
            this.mode = 0;
            this.totalEntries = 0;
            this.entriesList = new PlayerEntry[0];
        }
    }
}
