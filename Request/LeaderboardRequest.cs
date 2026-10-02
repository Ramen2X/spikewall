namespace spikewall.Request
{
    public class LeaderboardRequest : BaseRequest
    {
        public int Mode { get; set; }
    }

    public class LeaderboardEntriesRequest : BaseRequest
    {
        public int Mode { get; set; } //endless or quick mode
        public int First { get; set; } //starting entry index
        public int Count { get; set; } //additional entries starting from first entry
        public int Type { get; set; }
        public string[]? FriendIDList { get; set; }
    }
}
