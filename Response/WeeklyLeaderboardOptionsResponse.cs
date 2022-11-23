namespace spikewall.Response
{
    /// <summary>
    /// Response containing cost list (FIXME: Elaborate)
    /// </summary>
    public class WeeklyLeaderboardOptionsResponse : BaseResponse
    {
        public long? mode { get; set; }
        public long? type { get; set; }
        public long? param { get; set; }
        public long? startTime { get; set; }
        public long? resetTime { get; set; }

        public WeeklyLeaderboardOptionsResponse()
        {
            this.mode = 0;
            this.type = 0;
            this.param = 0;
            this.startTime = 0;
            this.resetTime = 0;
        }
    }
}
