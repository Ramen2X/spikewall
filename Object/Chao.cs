using MySqlConnector;
using spikewall.Response;
using System.Security.Cryptography;
using static spikewall.Object.ChaoBase;

namespace spikewall.Object
{
    public class ChaoBase
    {
        // The ID of this chao.
        public string? chaoID { get; set; }

        // The rarity of this chao.
        // Rarities range from N to SR.
        public long? rarity { get; set; }

        // Hides the chao in case they're not ready to be showcased to the public
        public long? hidden { get; set; }

        /// <summary>
        /// Enum that contains the names 
        /// and IDs of all buddies.
        /// </summary>
        public enum ChaoID
        {
            HeroChao = 400000,
            GoldChao,
            DarkChao,
            JewelChao,
            NormalChao,
            Omochao,
            RCMonkey,
            RCSpring,
            RCElectromagnet,
            BabyCyanWisp = 400012,
            BabyIndigoWisp,
            BabyYellowWisp,
            RCPinwheel,
            RCPiggyBank,
            RCBalloon,
            EasterChao = 400021,
            PurplePapurisu = 400024,
            MagLv1 = 400025,
            EggChao = 401000,
            PumpkinChao,
            SkullChao,
            Yacker,
            RCGoldenPiggyBank,
            WizardChao,
            RCTurtle = 401009,
            RCUFO,
            RCBomber, 
            EasterBunny = 401015,
            MagicLamp,
            StarShapedMissile,
            Suketoudara,
            Rappy,
            BlowfishTransporter,
            Genesis,
            Cartridge,
            RCFighter,
            RCHelicopter,
            RCHovercraft,
            GreenCrystalMonsterS,
            GreenCrystalMonsterL,
            RCAirship,
            DesertChao,
            RCSatellite,
            MarineChao,
            Nightopian,
            Orca,
            SonicOmochao,
            TailsOmochao,
            KnucklesOmochao,
            Boo,
            HalloweenChao,
            HeavyBomb,
            BlockBomb,
            HunkofMeat,
            Yeti,
            SnowChao,
            Ideya,
            ChristmasNightopian,
            Orbot,
            Cubot,
            LightChaos = 402000,
            HeroChaos,
            DarkChaos,
            Chip,
            Shahra,
            Caliburn,
            KingAuthursGhost,
            RCTornado,
            RCBattleCrusier,
            Merlina,
            ErazorDjinn,
            RCMoonMech,
            Carbuncle,
            Kuna,
            Chaos,
            DeathEgg,
            RedCrystalMonsterS,
            RedCrystalMonsterL,
            GoldenGoose,
            MotherWisp,
            RCPirateSpaceship,
            RCGoldenAngel,
            NiGHTS,
            Reala,
            RCTornado2,
            ChaoWalker,
            DarkQueen,
            KingBoomBoo,
            OPapa,
            OpaOpa,
            RCBlockFace,
            ChristmasYeti,
            ChristmasNiGHTS,
            DFekt,
            DarkChaoWalker,
        }

        /// <summary>
        /// Enum that contains the names 
        /// and IDs of all rarities.
        /// </summary>
        public enum Rarity
        {
            Normal,
            Rare,
            SRare,
            None
        }
    }

    public class Chao : ChaoBase
    {
        // Whether or not the chao is unlocked.
        public sbyte? status { get; set; }

        // The level of this chao.
        public sbyte? level { get; set; }

        // Whether the chao is equipped as a primary or secondary buddy or not at all.
        public long? setStatus { get; set; }

        // Unsure about this. Whether or not
        // the chao is unlocked is already noted
        // with "status", so it cannot be that?
        public long? acquired { get; set; }

        /// <summary>
        /// Enum that contains the status of whether a chao 
        /// is locked, unlocked, or max level
        /// </summary>
        public enum Status
        {
            NotOwned,
            Owned,
            MaxLevel
        }

        /// <summary>
        /// Enum that contains the SetStatus of a chao being 
        /// equipped as a primary or secondary buddy or not at all
        /// </summary>

        public enum SetStatus
        {
            None,
            Primary,
            Secondary
        }

        public Chao()
        {
            status = 0;
            level = 0;
            setStatus = 0;
            acquired = 0;
        }

        public static SRStatusCode PopulateChaoState(MySqlConnection conn, string uid, out Chao[] chaoState)
        {
            List<Chao> chao = new();

            // Get list of all visible chao
            var command = new MySqlCommand("SELECT * FROM `sw_chao`;", conn);

            using (var chaoRdr = command.ExecuteReader())
            {
                while (chaoRdr.Read()) {
                    Chao c = new();

                    c.chaoID = Convert.ToString(chaoRdr["id"]);
                    c.rarity = Convert.ToInt64(chaoRdr["rarity"]);
                    c.hidden = Convert.ToInt64(chaoRdr["hidden"]);

                    chao.Add(c);
                }

                chaoRdr.Close();
            }

            for (int i = 0; i < chao.Count; i++) {
                Chao c = chao[i];

                var sql = Db.GetCommand("SELECT * FROM `sw_chaostates` WHERE user_id = '{0}' AND chao_id = '{1}';", uid, c.chaoID);
                var stateCmd = new MySqlCommand(sql, conn);
                var stateRdr = stateCmd.ExecuteReader();

                if (stateRdr.HasRows) {
                    // Read row
                    stateRdr.Read();

                    c.status = Convert.ToSByte(stateRdr["status"]);
                    c.level = Convert.ToSByte(stateRdr["level"]);
                    c.setStatus = Convert.ToInt64(stateRdr["set_status"]);
                    c.acquired = Convert.ToInt64(stateRdr["acquired"]);

                    stateRdr.Close();
                } else {
                    stateRdr.Close();

                    // Insert a default chaostate
                    sql = Db.GetCommand(@"INSERT INTO `sw_chaostates` (chao_id, user_id) VALUES ('{0}', '{1}');", c.chaoID, uid);
                    var insertCmd = new MySqlCommand(sql, conn);
                    insertCmd.ExecuteNonQuery();
                }
            }

            conn.Close();

            chaoState = chao.ToArray();

            return SRStatusCode.Ok;
        }
        public static SRStatusCode SaveChaoState(MySqlConnection conn, string uid, Chao[] chaoState)
        {
            for (int i = 0; i < chaoState.Length; i++)
            {
                var sql = Db.GetCommand(
                  @"UPDATE `sw_chaostates` SET
                    status = '{0}',
                    level = '{1}',
                    set_status = '{2}',
                    acquired = '{3}'",
                        chaoState[i].status,
                        chaoState[i].level,
                        chaoState[i].setStatus,
                        chaoState[i].acquired);
                var command = new MySqlCommand(sql, conn);

                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected == 0)
                {
                    // Failed to find row with this user ID
                    return SRStatusCode.MissingPlayer;
                }
            }

            return SRStatusCode.Ok;
        }

        public static SRStatusCode AddChaoToChaoState(MySqlConnection conn, int chaoId, ref Chao[] chaoState, string uid, ref int chaoIndex)
        {
            // Get info about provided chao
            var sql = Db.GetCommand("SELECT * FROM `sw_chao` WHERE id = '{0}';", chaoId);
            var chaoCmd = new MySqlCommand(sql, conn);
            var chaoRdr = chaoCmd.ExecuteReader();

            if (chaoRdr.HasRows)
            {
                // Convert CharacterState to list so we can append to it
                List<Chao> chaoStateList = new(chaoState);

                // Read row
                chaoRdr.Read();

                Chao c = new()
                {
                    chaoID = Convert.ToString(chaoId),

                    status = Convert.ToSByte(chaoRdr["status"]),
                    level = Convert.ToSByte(chaoRdr["level"]),
                    setStatus = Convert.ToInt64(chaoRdr["setStatus"]),
                    acquired = Convert.ToInt64(chaoRdr["acquired"]),
                };
                c.status = (sbyte)((c.status == (sbyte)Status.NotOwned) ? 0 : 1);
                if (c.level == 10)
                {
                    c.status = (sbyte)Status.MaxLevel;
                }

                chaoRdr.Close();

                // Insert our newly crafted chao into the ChaoState
                sql = Db.GetCommand(@"INSERT INTO `sw_chaostates` (
                                              user_id, chao_id, status, level, set_status, acquired
                                          ) VALUES (
                                              '{0}', '{1}', '{2}', '{3}'
                                          );", uid, c.chaoID, c.status, c.level, c.setStatus, c.acquired);
                var insertCmd = new MySqlCommand(sql, conn);
                insertCmd.ExecuteNonQuery();

                chaoStateList.Add(c);

                // Convert ChaoState back to array to return it
                chaoState = chaoStateList.ToArray();

                // Return the index of the newly added chao
                chaoIndex = chaoStateList.Count - 1;

                return SRStatusCode.Ok;
            }
            // The chao we're being requested to add does not exist
            else return SRStatusCode.InternalServerError;
        }

        public static int FindChaoInChaoState(int chaoId, Chao[] chaoState)
        {
            int index = -1;
            for (int i = 0; i < chaoState.Length; i++)
            {
                if (Convert.ToInt32(chaoState[i].chaoID) == chaoId)
                {
                    index = i;
                    break;
                }
            }
            return index;
        }
    }

    public class ChaoWheelOptions
    {
        public long[]? rarity { get; set; }
        public long[]? itemWeight { get; set; }
        public Campaign[]? campaignList { get; set; }
        public long? spinCost { get; set; }
        public long? chaoRouletteType { get; set; }
        public long? numSpecialEgg { get; set; }
        public long? rouletteAvailable { get; set; }
        public long? numChaoRouletteToken { get; set; }
        public long? numChaoRoulette { get; set; }
        public long? startTime { get; set; }
        public long? endTime { get; set; }

        /// <summary>
        /// Enum that contains the names 
        /// and IDs of all Chao Roulette Types.
        /// </summary>
        public enum ChaoRouletteType
        {
            Normal,
            Special
        }

        public SRStatusCode PopulateChaoWheel(MySqlConnection conn, string uid)
        {
            PlayerState playerState = new();
            var populateStatus = playerState.Populate(conn, uid);
            if (populateStatus != SRStatusCode.Ok)
            {
                return populateStatus;
            }

            var sql = Db.GetCommand("SELECT * FROM `sw_chaowheeloptions WHERE user_id = '{0}'", uid);
            var command = new MySqlCommand(sql, conn);
            var reader = command.ExecuteReader();

            DateTimeOffset nextDayStart = new DateTime(
                DateTime.Now.Year,
                DateTime.Now.Month,
                DateTime.Now.Day,
                0, 0, 0, 0).AddDays(1);

            if (reader.Read())
            {
                this.chaoRouletteType = reader.GetInt64("chao_roulette_rank");
                this.spinCost = reader.GetInt64("chao_roulette_cost");
                this.numSpecialEgg = reader.GetInt64("num_special_egg");
                this.numChaoRouletteToken = reader.GetInt64("chao_roulette_ticket");
                this.startTime = nextDayStart.ToUnixTimeSeconds();
                this.endTime = nextDayStart.ToUnixTimeSeconds();
                reader.Close();
            }
            else
            {
                // No ChaoWheelOptions for this player, create one
                reader.Close();

                sql = Db.GetCommand(@"INSERT INTO `sw_chaowheeloptions` (
                                            user_id, num_special_egg, chao_roulette_ticket, chao_roulette_cost, chao_roulette_rank
                                        ) VALUES (
                                            '{0}', '{1}', '{2}', '{3}', '{4}'
                                        );", uid, 0, 0, 50, 0);
                var insertCmd = new MySqlCommand(sql, conn);
                insertCmd.ExecuteNonQuery();
            }

            var getChaoWheelOptionsStatus = GetChaoWheelOptions(conn, this.chaoRouletteType, out long[] chaoRarity, out short[] chaoWeight);
            if (getChaoWheelOptionsStatus != SRStatusCode.Ok)
            {
                return getChaoWheelOptionsStatus;
            }

            return SRStatusCode.Ok;
        }

        public SRStatusCode Save(MySqlConnection conn, string uid)
        {
            var sql = Db.GetCommand(
                @"UPDATE `sw_chaowheeloptions` SET
                    num_special_egg = '{0}',
                    chao_roulette_tickets = '{1}',
                    chao_roulette_rank = '{2}'
                  WHERE user_id = '{3}';",
                    this.numSpecialEgg,
                    this.numChaoRouletteToken,
                    this.chaoRouletteType,
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

        public static SRStatusCode GetChaoWheelOptions(MySqlConnection conn, long? chaoRouletteRank, out long[] chaoRarity, out short[] chaoWeight)
        {
            chaoRarity = new long[8];
            chaoWeight = new short[8];

            var sql = Db.GetCommand("SELECT * FROM `sw_chaoroulette` WHERE chao_roulette_rank = '{0}'", chaoRouletteRank);
            var command = new MySqlCommand(sql, conn);
            var reader = command.ExecuteReader();

            if (reader.Read())
            {
                for (sbyte i = 0; i < 8; i++)
                {

                    chaoRarity[i] = reader.GetInt64("chao_rarity");
                    chaoWeight[i] = reader.GetInt16("chao_rate");
                    reader.Read();
                }

                reader.Close();
            }
            else return SRStatusCode.InternalServerError;

            return SRStatusCode.Ok;
        }

        public static SRStatusCode GetPrizeChaoWheelOptions(MySqlConnection conn, string uid, ref int chaoId, ref int characterId, ref int chaoIndex, ref int characterIndex, ref Chao[] chaoState, ref Character[] characterState)
        {

            PlayerState playerState = new();
            var populateStatus = playerState.Populate(conn, uid);
            if (populateStatus != SRStatusCode.Ok)
            {
                return populateStatus;
            }

            var sql = Db.GetCommand("SELECT * FROM `sw_chaostates WHERE user_id = '{0}'", uid);
            var command = new MySqlCommand(sql, conn);
            var reader = command.ExecuteReader();

            var chaosql = Db.GetCommand("SELECT * FROM `sw_chao` WHERE id = '{0}', on_chao_roulette = '{1}'", chaoId, 1);
            var chaoCmd = new MySqlCommand(chaosql, conn);
            var chaoRdr = chaoCmd.ExecuteReader();
            if (chaoRdr.HasRows)
            {
                // Convert ChaoState to list so we can append to it
                List<Chao> prizeList = new(chaoState);

                // Read row
                chaoRdr.Read();

                Chao c = new()
                {
                    chaoID = Convert.ToString(chaoId),
                };


                chaoRdr.Close();

                // Insert our chao into the Prize List
                sql = Db.GetCommand(@"INSERT INTO `sw_rouletteprizelist` (
                                              chao_id
                                          ) VALUES (
                                              '{0}'
                                          );", c.chaoID);
                var insertCmd = new MySqlCommand(sql, conn);
                insertCmd.ExecuteNonQuery();

                prizeList.Add(c);

                // Convert prizeList back to array to return it
                chaoState = prizeList.ToArray();

                return SRStatusCode.Ok;
            }
            

            var charactersql = Db.GetCommand("SELECT * FROM `sw_character` WHERE id = '{0}', on_chao_roulette = '{1}'", characterId, 1);
            var characterCmd = new MySqlCommand(chaosql, conn);
            var characterRdr = characterCmd.ExecuteReader();
            if (characterRdr.HasRows)
            {
                List<Character> prizeList = new(characterState);

                // Read row
                characterRdr.Read();

                Character c = new()
                {
                    characterId = Convert.ToInt32(characterId),
                };
                chaoRdr.Close();

                // Insert our chao into the Prize List
                sql = Db.GetCommand(@"INSERT INTO `sw_rouletteprizelist` (
                                              chao_id
                                          ) VALUES (
                                              '{0}'
                                          );", c.characterId);
                var insertCmd = new MySqlCommand(sql, conn);
                insertCmd.ExecuteNonQuery();

                prizeList.Add(c);

                // Convert prizeList back to array to return it
                characterState = prizeList.ToArray();

                return SRStatusCode.Ok;
            }

            // The chao or character we're being requested to add does not exist
            else return SRStatusCode.InternalServerError;
        }

        //public ChaoWheelOptions()
        //{
        //    // FIXME: Dummy data
        //    rarity = Array.Empty<long>();
        //    itemWeight = Array.Empty<long>();
        //    campaignList = Array.Empty<Campaign>();
        //    spinCost = 0;
        //    chaoRouletteType = 0;
        //    numSpecialEgg = 0;
        //    rouletteAvailable = 0;
        //    numChaoRouletteToken = 0;
        //    numChaoRoulette = 0;
        //    startTime = 0;
        //    endTime = 0;
        //}
    }

    public class ChaoPrize : ChaoBase
    {

    }

    public class ChaoSpinPrize : ChaoBase
    {
        public sbyte? level { get; set; }
    }

    public class ChaoSpinResult
    {
        public ChaoSpinPrize PrizeWon { get; set; }
        public Item[] ItemList { get; set; }
        public int ItemWon { get; set;  }
    }
}
