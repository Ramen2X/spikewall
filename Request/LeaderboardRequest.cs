namespace spikewall.Request
{
    public class LeaderboardRequest : BaseRequest
    {
        public int Mode { get; set; }
    }

    public class LeaderboardEntriesRequest : BaseRequest
    {
        public int Mode { get; set; }
        public int First { get; set; }
        public int Count { get; set; }
        public int Type { get; set; }
        public string[]? FriendIDList { get; set; }
    }
}
