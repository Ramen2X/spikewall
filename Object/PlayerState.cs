using MySql.Data;
using MySql.Data.MySqlClient;
using spikewall.Response;

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

        public SRStatusCode Populate(MySqlConnection conn, string uid)
        {
            var sql = Db.GetCommand(@"SELECT * FROM `sw_players` WHERE id = '{0}';", uid);
            var command = new MySqlCommand(sql, conn);

            var reader = command.ExecuteReader();
            if (!reader.HasRows) {
                // Somehow failed to find player, return error code
                return SRStatusCode.MissingPlayer;
            }

            // FIXME: I strongly suspect some of these can be calculated rather than manual cells
            //        in this table so expect these to change.
            reader.Read();
            // FIXME: Missing items and equipItemList
            this.items = new Item[0];
            this.equipItemList = new string[0];
            this.mainCharaID = reader.GetString("main_chara_id");
            this.subCharaID = reader.GetString("sub_chara_id");
            this.mainChaoID = reader.GetString("main_chao_id");
            this.subChaoID = reader.GetString("sub_chao_id");
            this.numRings = reader.GetUInt64("num_rings");
            this.numBuyRings = reader.GetInt64("num_buy_rings");
            this.numRedRings = reader.GetUInt64("num_red_rings");
            this.numBuyRedRings = reader.GetInt64("num_buy_red_rings");
            this.energy = reader.GetInt16("energy");
            this.energyBuy = reader.GetInt64("energy_buy");
            this.energyRenewsAt = reader.GetInt64("energy_renews_at");
            this.mumMessages = reader.GetInt64("num_messages");
            this.rankingLeague = reader.GetInt64("ranking_league");
            this.quickRankingLeague = reader.GetInt64("quick_ranking_league");
            this.numRouletteTicket = reader.GetInt64("num_roulette_ticket");
            this.numChaoRouletteTicket = reader.GetInt64("num_chao_roulette_ticket");
            this.chaoEggs = reader.GetInt64("chao_eggs");
            this.totalHighScore = reader.GetUInt64("total_high_score");
            this.quickTotalHighScore = reader.GetUInt64("quick_total_high_score");
            this.totalDistance = reader.GetUInt64("total_distance");
            this.maximumDistance = reader.GetUInt64("maximum_distance");
            this.dailyMissionId = reader.GetInt64("daily_mission_id");
            this.dailyMissionEndTime = reader.GetInt64("daily_mission_end_time");
            this.dailyChallengeValue = reader.GetInt64("daily_challenge_value");
            this.dailyChallengeComplete = reader.GetInt64("daily_challenge_complete");
            this.numDailyChalCont = reader.GetInt64("num_daily_challenge_cont");
            this.numPlaying = reader.GetUInt64("num_playing");
            this.numAnimals = reader.GetUInt64("num_animals");
            this.numRank = reader.GetInt16("num_rank");

            reader.Close();

            return SRStatusCode.Ok;
        }
    }
}
