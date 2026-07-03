using MySqlConnector;
using spikewall.Response;
using System.Security.Cryptography;

namespace spikewall.Object
{
    public class WheelOptions
    {
        public long[] items { get; set; }
        public long[] item { get; set; }
        public short[] itemWeight { get; set; }
        public int itemWon { get; set; }
        public long nextFreeSpin { get; set; }
        public int spinCost { get; set; }
        public sbyte rouletteRank { get; set; }
        public long numRouletteToken { get; set; }
        public long numJackpotRing { get; set; }
        public long numRemainingRoulette { get; set; }
        public Item[]? itemList { get; set; }

        /// <summary>
        /// Enum that contains the names 
        /// and IDs of all Item Roulette Ranks.
        /// </summary>
        public enum RouletteRank
        {
            Normal,
            Big,
            Super
        }

        public SRStatusCode Populate(MySqlConnection conn, string uid, ref Chao[] chaoState)
        {
            PlayerState playerState = new();

            var populateStatus = playerState.Populate(conn, uid);
            if (populateStatus != SRStatusCode.Ok)
            {
                return populateStatus;
            }

            var sql = Db.GetCommand("SELECT * FROM `sw_wheeloptions` WHERE user_id = '{0}'", uid);
            var command = new MySqlCommand(sql, conn);
            var reader = command.ExecuteReader();

            DateTimeOffset nextDayStart = new DateTime(
                DateTime.Now.Year,
                DateTime.Now.Month,
                DateTime.Now.Day,
                0, 0, 0, 0).AddDays(1);

            if (reader.Read())
            {
                this.itemWon = reader.GetInt32("item_won");
                this.rouletteRank = reader.GetSByte("roulette_rank");
                this.numRouletteToken = playerState.numRouletteTicket;
                this.numRemainingRoulette = playerState.numRouletteTicket + reader.GetSByte("num_free_spins");
                this.nextFreeSpin = nextDayStart.ToUnixTimeSeconds();

                // Append free spins if applicable
                if (reader.GetInt64("next_free_spin") != this.nextFreeSpin)
                {
                    this.numRemainingRoulette += 3;
                }

                reader.Close();
            }
            else
            {
                // No WheelOptions for this player, create one
                reader.Close();

                sql = Db.GetCommand(@"INSERT INTO `sw_wheeloptions` (
                                            user_id, next_free_spin, num_free_spins, item_won, roulette_rank
                                        ) VALUES (
                                            '{0}', '{1}', '{2}', '{3}', '{4}'
                                        );", uid, 0, 0, RandomNumberGenerator.GetInt32(8), 0);
                var insertCmd = new MySqlCommand(sql, conn);
                insertCmd.ExecuteNonQuery();
            }

            var getWheelOptionsStatus = GetItemWheelOptions(conn, this.rouletteRank, out long[] items, out long[] itemNum, out short[] itemWeight);
            if (getWheelOptionsStatus != SRStatusCode.Ok)
            {
                return getWheelOptionsStatus;
            }

            var getAdjustedNormalChaoStatus = AdjustChaoItemRouletteWeights(conn, ref items, ref itemWeight, ref chaoState);
            if (getAdjustedNormalChaoStatus != SRStatusCode.Ok)
            {
                return getAdjustedNormalChaoStatus;
            }

            this.items = items;
            this.item = itemNum;
            this.itemWeight = itemWeight;
            this.numJackpotRing = 50000; // FIXME: Hardcoded!!

            return SRStatusCode.Ok;
        }

        public SRStatusCode Save(MySqlConnection conn, string uid)
        {
            var sql = Db.GetCommand(
                @"UPDATE `sw_wheeloptions` SET
                    next_free_spin = '{0}',
                    num_free_spins = '{1}',
                    item_won = '{2}',
                    roulette_rank = '{3}'
                  WHERE user_id = '{4}';",
                    this.nextFreeSpin,
                    this.numRemainingRoulette - this.numRouletteToken,

                    // FIXME: While the item roulette in the original always had
                    // equal rates for everything, we should still handle rates here anyway
                    // since there would be no point to itemWeight if we didn't,
                    // and it just makes sense from a configuration standpoint.

                    this.itemWon = RandomNumberGenerator.GetInt32(8),

                    this.rouletteRank,
                    uid);
            var command = new MySqlCommand(sql, conn);

            int rowsAffected = command.ExecuteNonQuery();

            if (rowsAffected == 0)
            {
                // Failed to find row with this user ID
                return SRStatusCode.MissingPlayer;
            }

            return SRStatusCode.Ok;
        }

        public static SRStatusCode GetItemWheelOptions(MySqlConnection conn, sbyte rouletteRank, 
            out long[] items, out long[] itemNum, out short[] itemWeight)
        {
            items = new long[8];
            itemNum = new long[8];
            itemWeight = new short[8];

            var sql = Db.GetCommand("SELECT * FROM `sw_itemroulette` WHERE roulette_rank = '{0}'", rouletteRank);
            var command = new MySqlCommand(sql, conn);
            var reader = command.ExecuteReader();

            if (reader.Read())
            {
                for (sbyte i = 0; i < 8; i++)
                {
                    items[i] = reader.GetInt64("item_id");

                    itemNum[i] = reader.GetInt64("item_num");
                    itemWeight[i] = reader.GetInt16("item_rate");
                    reader.Read();
                }

                reader.Close();
            }
            else return SRStatusCode.InternalServerError;

            return SRStatusCode.Ok;
        }

        public static SRStatusCode AdjustChaoItemRouletteWeights(MySqlConnection conn, ref long[] items, ref short[] itemweight, ref Chao[] chaoState)
        {
            //Extracting the weights from the itemWeight array and calculating the overall odds of normal buddies depending on the item roulette level
            //Overall Normal Chao Odds will stay at zero if no Normal Egg is in the Item Roulette
            var overallNormalOdds = 0;
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i] == (sbyte)Item.ItemID.NormalEgg)
                {
                    overallNormalOdds += itemweight[i];
                }
            }

            List<Chao> availableChao = new List<Chao>();

            var increasedNormalOdds = 0.1 * overallNormalOdds;

            var normalChaoWithIncreasedOdds = Db.GetCommand(@"SELECT COUNT(id) FROM `sw_chao` WHERE (on_item_roulette = '1' AND is_odds_increased = '1')");
            var normalChaoIncreasedOddsCount = Convert.ToInt32(normalChaoWithIncreasedOdds);
            var normalChaoWithoutIncreasedOdds = Db.GetCommand(@"SELECT COUNT(id) FROM `sw_chao` WHERE (on_item_roulette = '1' AND is_odds_increased = '0')");
            var normalChaoNonIncreasedOddsCount = Convert.ToInt32(normalChaoWithoutIncreasedOdds);

            //If the product in the parenthesis becomes 0, odds would be normalized with all Normal Chao available in the corresponding Item Roulette respectively
            var adjustedNormalizedRareOdds = (overallNormalOdds - (increasedNormalOdds * normalChaoIncreasedOddsCount)) / normalChaoNonIncreasedOddsCount;

            var normalChaoWithIncreasedOddsCmd = new MySqlCommand(normalChaoWithIncreasedOdds, conn);
            var normalChaoWithIncreasedOddsRdr = normalChaoWithIncreasedOddsCmd.ExecuteReader();

            Chao chao = new();

            //This while loop will not run if there are no normal chao with increased odds
            while (normalChaoWithIncreasedOddsRdr.Read())
            {
                chao.chaoID = Convert.ToString(normalChaoWithIncreasedOddsRdr["id"]);
                var increasedOddsSql = Db.GetCommand(@"INSERT INTO `sw_chaoitemrouletteprizelist` (chao_id, chao_weight) VALUES ('{0}', '{1}')", chao.chaoID, increasedNormalOdds);
                var insertIncreasedOddsCmd = new MySqlCommand(increasedOddsSql, conn);
                insertIncreasedOddsCmd.ExecuteNonQuery();
                availableChao.Add(chao);
                normalChaoWithIncreasedOddsRdr.Read();
            }

            normalChaoWithIncreasedOddsRdr.Close();

            var normalChaoWithoutIncreasedOddsCmd = new MySqlCommand(normalChaoWithoutIncreasedOdds, conn);
            var normalChaoWithoutIncreasedOddsRdr = normalChaoWithoutIncreasedOddsCmd.ExecuteReader();

            //This while loop will run regardless if there are normal chao with increased odds or not
            while (normalChaoWithIncreasedOddsRdr.Read())
            {
                chao.chaoID = Convert.ToString(normalChaoWithoutIncreasedOddsRdr["id"]);
                var sql = Db.GetCommand(@"INSERT INTO `sw_chaoitemrouletteprizelist` (chao_id, chao_weight) VALUES ('{0}', '{1}')", chao.chaoID, adjustedNormalizedRareOdds);
                var insertCmd = new MySqlCommand(sql, conn);
                insertCmd.ExecuteNonQuery();
                availableChao.Add(chao);
                normalChaoWithoutIncreasedOddsRdr.Read();
            }

            normalChaoWithoutIncreasedOddsRdr.Close();

            chaoState = availableChao.ToArray();

            return SRStatusCode.Ok;
        }
    }
}
