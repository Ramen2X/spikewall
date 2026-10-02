namespace spikewall.Object
{
    public class LeaderboardEntry
    {
        public string? FriendID { get; set; }
        public string? Name { get; set; }
        public string? URL { get; set; }
        public ulong? Grade { get; set; }
        public ulong? ExposeOnline { get; set; }
        public ulong? RankingScore { get; set; }
        public ulong? RankChanged { get; set; }
        public ulong? IsSentEnergy { get; set; }
        public ulong? ExpireTime { get; set; }
        public ulong? NumRank { get; set; }
        public ulong? LoginTime { get; set; }
        public string? CharacterID { get; set; }
        public ulong? CharacterLevel { get; set; }
        public string? SubCharacterID { get; set; }
        public ulong? SubCharacterLevel { get; set; }
        public string? MainChaoID { get; set; }
        public ulong? MainChaoLevel { get; set; }
        //public ulong? Language { get; set; }
        public ulong? League { get; set; }
        public ulong? MaxScore { get; set; }
        public ulong? TotalScore { get; set; }

        /// <summary>
        /// This enum contains all the languages that are available in the game.
        /// </summary>
        public enum Language
        {
            Japanese,
            English,
            ChineseZHJ,
            ChineseZH,
            Korean,
            French,
            German,
            Spanish,
            Portuguese,
            Italian,
            Russian
        }
    }
}
