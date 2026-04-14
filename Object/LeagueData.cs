namespace spikewall.Object
{
    public class LeagueData
    {
        public string? leagueId { get; set; }
        public string? groupId { get; set; }
        public string? numUp { get; set; }
        public string? numDown { get; set; }
        public string? numGroupMember { get; set; }

        // FIXME: This is an array but shouldn't actually be strings, set up "Cost" object
        public string[]? highScoreOpe { get; set; }
        public string[]? totalScoreOpe { get; set; }

        /// <summary>
        /// Enum that contains the names 
        /// and IDs of all Runners League Ranks.
        /// The number at the end signifies the number of stars in each League Rank.
        /// </summary>
        public enum LeagueID
        {
            F1,
            F2,
            F3,
            E1,
            E2,
            E3,
            D1,
            D2,
            D3,
            C1,
            C2,
            C3,
            B1,
            B2,
            B3,
            A1,
            A2,
            A3,
            S1,
            S2,
            S3
        }

        /// <summary>
        /// Enum that contains the the mode types
        /// and IDs of the Runners League Ranks.
        /// </summary>
        public enum RankingMode
        {
            Endless,
            Quick
        }

        public LeagueData()
        {
            leagueId = "0";
            groupId = "0";
            numUp = "40";
            numDown = "0";
            numGroupMember = "0";
            highScoreOpe = Array.Empty<string>();
            totalScoreOpe = Array.Empty<string>();
        }
    }
}
