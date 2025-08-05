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
        /// </summary>
        public enum LeagueID
        {
            F_M,
            F,
            F_P,
            E_M,
            E,
            E_P,
            D_M,
            D,
            D_P,
            C_M,
            C,
            C_P,
            B_M,
            B,
            B_P,
            A_M,
            A,
            A_P,
            S_M,
            S,
            S_P
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
