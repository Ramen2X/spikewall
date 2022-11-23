using MySql.Data.MySqlClient;
using spikewall.Response;
using System.Text.Json.Serialization;

namespace spikewall.Object
{
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public class PlayerState
    {
        public Item[]? items { get; set; }
        public long[]? equipItemList { get; set; }
        public int mainCharaID { get; set; }
        public int subCharaID { get; set; }
        public int mainChaoID { get; set; }
        public int subChaoID { get; set; }
        public ulong numRings { get; set; }
        public long? numBuyRings { get; set; }
        public ulong numRedRings { get; set; }
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

            this.mainCharaID = reader.GetInt32("main_chara_id");
            this.subCharaID = reader.GetInt32("sub_chara_id");
            this.mainChaoID = reader.GetInt32("main_chao_id");
            this.subChaoID = reader.GetInt32("sub_chao_id");
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
            this.totalHighScore = reader.GetUInt64("story_high_score");
            this.quickTotalHighScore = reader.GetUInt64("quick_high_score");
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

            string equipItemList = reader.GetString("equip_item_list");
            if (string.IsNullOrEmpty(equipItemList)) {
                this.equipItemList = Array.Empty<long>();
            } else {
                this.equipItemList = Db.ConvertDBListToIntArray(equipItemList);
            }

            reader.Close();

            // Populate item list
            List<Item> items = new List<Item>();
            sql = Db.GetCommand(@"SELECT item_id FROM `sw_itemownership` WHERE user_id = '{0}';", uid);
            command = new MySqlCommand(sql, conn);
            reader = command.ExecuteReader();
            while (reader.Read())
            {
                long itemId = reader.GetInt64("item_id");
                bool found = false;

                for (int i = 0; i < items.Count; i++)
                {
                    if (items[i].itemId == itemId)
                    {
                        items[i].numItem++;
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    items.Add(new Item(itemId, 1));
                }
            }
            reader.Close();
            this.items = items.ToArray();

            return SRStatusCode.Ok;
        }

        public SRStatusCode Save(MySqlConnection conn, string uid)
        {
            // FIXME: I strongly suspect some of these can be calculated rather than manual cells
            //        in this table so expect these to change.
            var sql = Db.GetCommand(
                @"UPDATE `sw_players` SET
                    main_chara_id = '{0}',
                    sub_chara_id = '{1}',
                    main_chao_id = '{2}',
                    sub_chao_id = '{3}',
                    num_rings = '{4}',
                    num_buy_rings = '{5}',
                    num_red_rings = '{6}',
                    num_buy_red_rings = '{7}',
                    energy = '{8}',
                    energy_buy = '{9}',
                    energy_renews_at = '{10}',
                    num_messages = '{11}',
                    ranking_league = '{12}',
                    quick_ranking_league = '{13}',
                    num_roulette_ticket = '{14}',
                    num_chao_roulette_ticket = '{15}',
                    chao_eggs = '{16}',
                    story_high_score = '{17}',
                    quick_high_score = '{18}',
                    total_distance = '{19}',
                    maximum_distance = '{20}',
                    daily_mission_id = '{21}',
                    daily_mission_end_time = '{22}',
                    daily_challenge_value = '{23}',
                    daily_challenge_complete = '{24}',
                    num_daily_challenge_cont = '{25}',
                    num_playing = '{26}',
                    num_animals = '{27}',
                    num_rank = '{28}',
                    equip_item_list = '{29}'
                  WHERE id = '{30}';",
                    this.mainCharaID,
                    this.subCharaID,
                    this.mainChaoID,
                    this.subChaoID,
                    this.numRings,
                    this.numBuyRings,
                    this.numRedRings,
                    this.numBuyRedRings,
                    this.energy,
                    this.energyBuy,
                    this.energyRenewsAt,
                    this.mumMessages,
                    this.rankingLeague,
                    this.quickRankingLeague,
                    this.numRouletteTicket,
                    this.numChaoRouletteTicket,
                    this.chaoEggs,
                    this.totalHighScore,
                    this.quickTotalHighScore,
                    this.totalDistance,
                    this.maximumDistance,
                    this.dailyMissionId,
                    this.dailyMissionEndTime,
                    this.dailyChallengeValue,
                    this.dailyChallengeComplete,
                    this.numDailyChalCont,
                    this.numPlaying,
                    this.numAnimals,
                    this.numRank,
                    Db.ConvertIntArrayToDBList(this.equipItemList),
                    uid);
            var command = new MySqlCommand(sql, conn);

            int rowsAffected = command.ExecuteNonQuery();

            if (rowsAffected == 0)
            {
                // Failed to find row with this user ID
                return SRStatusCode.MissingPlayer;
            }

            // Store item ownership into database
            sql = Db.GetCommand(@"DELETE FROM `sw_itemownership` WHERE user_id = '{0}';", uid);
            for (int i = 0; i < this.items.Length; i++)
            {
                Item item = this.items[i];

                for (int j = 0; j < item.numItem; j++)
                {
                    sql += Db.GetCommand(@"INSERT INTO `sw_itemownership` (user_id, item_id) VALUES ('{0}', '{1}');", uid, item.itemId);
                }
            }
            command = new MySqlCommand(sql, conn);
            command.ExecuteNonQuery();

            return SRStatusCode.Ok;
        }
    }
}
