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
        public Int64? numRings { get; set; }
        public Int64? numBuyRings { get; set; }
        public Int64? numRedRings { get; set; }
        public Int64? numBuyRedRings { get; set; }
        public Int64? energy { get; set; }
        public Int64? energyBuy { get; set; }
        public Int64? energyRenewsAt { get; set; }
        public Int64? mumMessages { get; set; }
        public Int64? rankingLeague { get; set; }
        public Int64? quickRankingLeague { get; set; }
        public Int64? numRouletteTicket { get; set; }
        public Int64? numChaoRouletteTicket { get; set; }
        public Int64? chaoEggs { get; set; }
        public Int64? totalHighScore { get; set; }
        public Int64? quickTotalHighScore { get; set; }
        public Int64? totalDistance { get; set; }
        public Int64? maximumDistance { get; set; }
        public Int64? dailyMissionId { get; set; }
        public Int64? dailyMissionEndTime { get; set; }
        public Int64? dailyChallengeValue { get; set; }
        public Int64? dailyChallengeComplete { get; set; }
        public Int64? numDailyChalCont { get; set; }
        public Int64? numPlaying { get; set; }
        public Int64? numAnimals { get; set; }
        public Int64? numRank { get; set; }
    }
}
