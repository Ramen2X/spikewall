using MySql.Data.MySqlClient;
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

        public SRStatusCode Populate(MySqlConnection conn, string uid)
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
                this.rouletteRank = reader.GetSByte("wheel_rank");
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
                                            user_id, next_free_spin, num_free_spins, item_won, wheel_rank
                                        ) VALUES (
                                            '{0}', '{1}', '{2}', '{3}', '{4}'
                                        );", uid, 0, 0, RandomNumberGenerator.GetInt32(8), 0);
                var insertCmd = new MySqlCommand(sql, conn);
                insertCmd.ExecuteNonQuery();
            }

            long[] items = new long[8];
            long[] itemNum = new long[8];
            short[] itemWeight = new short[8];

            sql = Db.GetCommand("SELECT * FROM `sw_itemroulette` WHERE wheel_rank = '{0}'", this.rouletteRank);
            command = new MySqlCommand(sql, conn);
            reader = command.ExecuteReader();

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
                    wheel_rank = '{3}'
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
    }
}
