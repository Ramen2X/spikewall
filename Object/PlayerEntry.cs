namespace spikewall.Object
{
    public class PlayerEntry
    {
        public string? friendId { get; set; }
        public string? name { get; set; }
        public string? url { get; set; }
        public long? grade { get; set; }
        public long? exposeOnline { get; set; }
        public long? rankingScore { get; set; }
        public long? rankChanged { get; set; }
        public long? energyFlg { get; set; }
        public long? expireTime { get; set; }
        public long? numRank { get; set; }
        public long? loginTime { get; set; }
        public string? charaId { get; set; }
        public long? characterLevel { get; set; }
        public string? subCharaId { get; set; }
        public long? subCharaLevel { get; set; }
        public string? mainChaoId { get; set; }
        public long? mainChaoLevel { get; set; }
        public string? subChaoId { get; set; }
        public long? subChaoLevel { get; set; }
        public long? language { get; set; }
        public long? league { get; set; }
        public long? maxScore { get; set; }

        public PlayerEntry()
        {
        }
    }
}
