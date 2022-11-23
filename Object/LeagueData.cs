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
