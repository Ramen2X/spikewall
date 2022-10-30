namespace spikewall.Object
{
    public class PlayerState
    {
        public Item[]? items { get; set; }
        public string[]? equipItemList { get; set; }
        public string? mainCharaID { get; set; }
        public string? subCharaID { get; set; }
        public string? mainChaoID { get; set; }
        public string? subChaoID { get; set; }
        public ulong? numRings { get; set; }
        public long? numBuyRings { get; set; }
        public ulong? numRedRings { get; set; }
        public long? numBuyRedRings { get; set; }
        public int? energy { get; set; }
        public long? energyBuy { get; set; }
        public long? energyRenewsAt { get; set; }
        public long? mumMessages { get; set; }
        public long? rankingLeague { get; set; }
        public long? quickRankingLeague { get; set; }
        public long? numRouletteTicket { get; set; }
        public long? numChaoRouletteTicket { get; set; }
        public long? chaoEggs { get; set; }
        public ulong? totalHighScore { get; set; }
        public ulong? quickTotalHighScore { get; set; }
        public ulong? totalDistance { get; set; }
        public ulong? maximumDistance { get; set; }
        public long? dailyMissionId { get; set; }
        public long? dailyMissionEndTime { get; set; }
        public long? dailyChallengeValue { get; set; }
        public long? dailyChallengeComplete { get; set; }
        public long? numDailyChalCont { get; set; }
        public ulong? numPlaying { get; set; }
        public ulong? numAnimals { get; set; }
        public short? numRank { get; set; }
    }
}
