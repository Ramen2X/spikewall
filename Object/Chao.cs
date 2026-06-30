using MySqlConnector;
using spikewall.Response;
using Swashbuckle.AspNetCore.SwaggerGen;
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

        public static SRStatusCode LevelUpChao(MySqlConnection sql, int chaoId, ref Chao[] chaoState, out int chaoIndex)
        {
            chaoIndex = FindChaoInChaoState(chaoId, chaoState);
            if (chaoIndex == -1)
            {
                // The chao we want to upgrade isn't available to the player, abort
                return SRStatusCode.InternalServerError;
            }
            //We want to make sure that the chao is unlocked first before we level them up
            if (chaoState[chaoIndex].status == (sbyte)Status.NotOwned)
            {
                chaoState[chaoIndex].status = (sbyte)Status.Owned;
            }
            
            else if (chaoState[chaoIndex].level < 10)
            {
                chaoState[chaoIndex].level += 1;
            }
            //If Chao is at Level 10, set them to Max Level
            else chaoState[chaoIndex].status = (sbyte)Status.MaxLevel;
            return SRStatusCode.Ok;
        }
    }

    public class ChaoPrize : ChaoBase
    {

    }

    public class ChaoSpinPrize : Chao
    {

    }

    public class ChaoSpinResult
    {
        public ChaoSpinPrize[] PrizeWon { get; set; }
        public Item[] ItemList { get; set; }
        public long ItemWon { get; set; }
    }

    public class ChaoWheelOptions
    {
        public long[]? rarity { get; set; }
        public short[]? itemWeight { get; set; }
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

            var populateEggWeightsSql = Db.GetCommand("SELECT * FROM `sw_chaoroulette` WHERE chao_roulette_rank = '{0}'", chaoRouletteRank);
            var populateEggWeightsCommand = new MySqlCommand(populateEggWeightsSql, conn);
            var populateEggWeightsReader = populateEggWeightsCommand.ExecuteReader();

            if (populateEggWeightsReader.Read())
            {
                for (sbyte i = 0; i < 8; i++)
                {

                    chaoRarity[i] = populateEggWeightsReader.GetInt64("chao_rarity");
                    chaoWeight[i] = populateEggWeightsReader.GetInt16("chao_rate");
                    populateEggWeightsReader.Read();
                }

                populateEggWeightsReader.Close();
            }
            else return SRStatusCode.InternalServerError;

            return SRStatusCode.Ok;
        }
        public static SRStatusCode GetPrizeChaoWheelOptions(MySqlConnection conn, out Chao[] rareChaoPrizeList, out Chao[] superRareChaoPrizeList, out Character[] characterPrizeList)
        {
            List<Chao> rareChao = new();
            List<Chao> sRareChao = new();
            List<Character> characters = new();

            var populateChaoPrizeListSql = Db.GetCommand("SELECT * FROM `sw_chao` WHERE on_chao_roulette = '{0}'", 1);
            var populateChaoPrizeListCmd = new MySqlCommand(populateChaoPrizeListSql, conn);
            var populateChaoPrizeListRdr = populateChaoPrizeListCmd.ExecuteReader();

            while (populateChaoPrizeListRdr.Read())
            {
                Chao c = new();

                c.chaoID = populateChaoPrizeListRdr.GetString("id");

                if (Convert.ToInt64(populateChaoPrizeListRdr["rarity"]) == (long)Chao.Rarity.Rare)
                {
                    rareChao.Add(c);
                }
                else if (Convert.ToInt64(populateChaoPrizeListRdr["rarity"]) == (long)Chao.Rarity.SRare)
                {
                    sRareChao.Add(c);
                }
            }

            populateChaoPrizeListRdr.Close();

            rareChaoPrizeList = rareChao.ToArray();
            superRareChaoPrizeList = sRareChao.ToArray();

            var populateCharacterPrizeListSql = Db.GetCommand("SELECT * FROM `sw_character` WHERE on_chao_roulette = '{0}'", 1);
            var populateCharacterPrizeListCmd = new MySqlCommand(populateCharacterPrizeListSql, conn);
            var populateCharacterPrizeListRdr = populateCharacterPrizeListCmd.ExecuteReader();

            while (populateCharacterPrizeListRdr.Read())
            {
                Character c = new();

                c.characterId = populateCharacterPrizeListRdr.GetInt32("id");

                characters.Add(c);
            }

            populateCharacterPrizeListRdr.Close();

            characterPrizeList = characters.ToArray();

            return SRStatusCode.Ok;
        }

        public static SRStatusCode AdjustChaoWeights(MySqlConnection conn, ref long[] chaoRarity, ref short[] chaoWeight, ref Chao[] chaoState, ref Character[] characterState)
        {
            //Extracting the weights from the chaoWeight array and calculating the overall odds
            short overallRareOdds = 0;
            short overallSuperRareOdds = 0;
            short overallCharacterOdds = 0;
            for (int i = 0; i < chaoWeight.Length; i++)
            {
                if (chaoRarity[i] == (long)Chao.Rarity.Rare)
                {
                    overallRareOdds += chaoWeight[i];
                }
                else if (chaoRarity[i] == (long)Chao.Rarity.SRare)
                {
                    overallSuperRareOdds += chaoWeight[i];
                }
                else
                {
                    overallCharacterOdds += chaoWeight[i];
                }
            }

            List<Chao> availableChao = new List<Chao>();

            var increasedRareOdds = 0.1 * overallRareOdds;
            var increasedSuperRareOdds = 0.1 * overallRareOdds;

            var rareChaoWithIncreasedOdds = Db.GetCommand(@"SELECT COUNT(id) FROM `sw_chao` WHERE (rarity = '1' AND on_chao_roulette = '1' AND is_odds_increased = '1')");
            var rareChaoIncreasedOddsCount = Convert.ToInt32(rareChaoWithIncreasedOdds);
            var rareChaoWithoutIncreasedOdds = Db.GetCommand(@"SELECT COUNT(id) FROM `sw_chao` WHERE (rarity = '1' AND on_chao_roulette = '1' AND is_odds_increased = '0')");
            var rareChaoNonIncreasedOddsCount = Convert.ToInt32(rareChaoWithoutIncreasedOdds);

            var sRareChaoWithIncreasedOdds = Db.GetCommand(@"SELECT COUNT(id) FROM `sw_chao` WHERE (rarity = '2' AND on_chao_roulette = '1' AND is_odds_increased = '1')");
            var sRareChaoIncreasedOddsCount = Convert.ToInt32(sRareChaoWithIncreasedOdds);
            var sRareChaoWithoutIncreasedOdds = Db.GetCommand(@"SELECT COUNT(id) FROM `sw_chao` WHERE (rarity = '2' AND on_chao_roulette = '1' AND is_odds_increased = '0')");
            var sRareChaoNonIncreasedOddsCount = Convert.ToInt32(sRareChaoWithoutIncreasedOdds);

            //If the product(s) in the parenthesis become(s) 0, odds would be normalized with all Rare and S Rare Chao available in the Premium Roulette respectively
            var adjustedNormalizedRareOdds = (overallRareOdds - (increasedRareOdds * rareChaoIncreasedOddsCount)) / rareChaoNonIncreasedOddsCount;
            var adjustedNormalizedSRareOdds = (overallRareOdds - (increasedRareOdds * sRareChaoIncreasedOddsCount)) / sRareChaoNonIncreasedOddsCount;

            var rareChaoWithIncreasedOddsCmd = new MySqlCommand(rareChaoWithIncreasedOdds, conn);
            var rareChaoWithIncreasedOddsRdr = rareChaoWithIncreasedOddsCmd.ExecuteReader();

            Chao chao = new();

            //This while loop will not run if there are no rare chao with increased odds
            while (rareChaoWithIncreasedOddsRdr.Read())
            {
                chao.chaoID = Convert.ToString(rareChaoWithIncreasedOddsRdr["id"]);
                var increasedOddsSql = Db.GetCommand(@"INSERT INTO `sw_chaorouletteprizelist` (chao_id, chao_rarity, chao_weight) VALUES ('{0}', '{1}', '{2}')", chao.chaoID, (long)Item.ItemID.RareEgg, increasedRareOdds);
                var insertIncreasedOddsCmd = new MySqlCommand(increasedOddsSql, conn);
                insertIncreasedOddsCmd.ExecuteNonQuery();
                availableChao.Add(chao);
                rareChaoWithIncreasedOddsRdr.Read();
            }

            rareChaoWithIncreasedOddsRdr.Close();

            var rareChaoWithoutIncreasedOddsCmd = new MySqlCommand(rareChaoWithoutIncreasedOdds, conn);
            var rareChaoWithoutIncreasedOddsRdr = rareChaoWithoutIncreasedOddsCmd.ExecuteReader();

            //This while loop will run regardless if there are rare chao with increased odds or not
            while (rareChaoWithIncreasedOddsRdr.Read())
            {
                chao.chaoID = Convert.ToString(rareChaoWithoutIncreasedOddsRdr["id"]);
                var sql = Db.GetCommand(@"INSERT INTO `sw_chaorouletteprizelist` (chao_id, chao_rarity, chao_weight) VALUES ('{0}', '{1}', '{2}')", chao.chaoID, (long)Item.ItemID.RareEgg, adjustedNormalizedRareOdds);
                var insertCmd = new MySqlCommand(sql, conn);
                insertCmd.ExecuteNonQuery();
                availableChao.Add(chao);
                rareChaoWithoutIncreasedOddsRdr.Read();
            }

            rareChaoWithoutIncreasedOddsRdr.Close();

            var sRareChaoWithIncreasedOddsCmd = new MySqlCommand(sRareChaoWithIncreasedOdds, conn);
            var sRareChaoWithIncreasedOddsRdr = sRareChaoWithIncreasedOddsCmd.ExecuteReader();

            //This while loop will not run if there are no S Rare chao with increased odds
            while (sRareChaoWithIncreasedOddsRdr.Read())
            {
                chao.chaoID = Convert.ToString(sRareChaoWithIncreasedOddsRdr["id"]);
                var increasedOddsSql = Db.GetCommand(@"INSERT INTO `sw_chaorouletteprizelist` (chao_id, chao_rarity, chao_weight) VALUES ('{0}', '{1}', '{2}')", chao.chaoID, (long)Item.ItemID.SuperRareEgg, increasedSuperRareOdds);
                var insertIncreasedOddsCmd = new MySqlCommand(increasedOddsSql, conn);
                insertIncreasedOddsCmd.ExecuteNonQuery();
                availableChao.Add(chao);
                sRareChaoWithIncreasedOddsRdr.Read();
            }

            sRareChaoWithIncreasedOddsRdr.Close();

            var sRareChaoWithoutIncreasedOddsCmd = new MySqlCommand(sRareChaoWithoutIncreasedOdds, conn);
            var sRareChaoWithoutIncreasedOddsRdr = sRareChaoWithoutIncreasedOddsCmd.ExecuteReader();

            //This while loop will run regardless if there are S Rare chao with increased odds or not
            while (sRareChaoWithoutIncreasedOddsRdr.Read())
            {
                chao.chaoID = Convert.ToString(sRareChaoWithoutIncreasedOddsRdr["id"]);
                var sql = Db.GetCommand(@"INSERT INTO `sw_chaorouletteprizelist` (chao_id, chao_rarity, chao_weight) VALUES ('{0}', '{1}', '{2}')", chao.chaoID, (long)Item.ItemID.SuperRareEgg, adjustedNormalizedSRareOdds);
                var insertCmd = new MySqlCommand(sql, conn);
                insertCmd.ExecuteNonQuery();
                availableChao.Add(chao);
                sRareChaoWithoutIncreasedOddsRdr.Read();
            }

            chaoState = availableChao.ToArray();

            List<Character> availableCharacters = new List<Character>();

            var increasedCharacterOdds = 0.15 * overallCharacterOdds;

            var characterSql = Db.GetCommand("SELECT * FROM `sw_character` WHERE on_chao_roulette = '1'");
            var characterCommand = new MySqlCommand(characterSql, conn);

            var charactersWithIncreasedOdds = Db.GetCommand(@"SELECT COUNT(id) FROM `sw_character` WHERE (on_chao_roulette = '1' AND is_odds_increased = '1')");
            var characterIncreasedOddsCount = Convert.ToInt32(charactersWithIncreasedOdds);

            var charactersWithoutIncreasedOdds = Db.GetCommand(@"SELECT COUNT(id) FROM `sw_character` WHERE (on_chao_roulette = '1' AND is_odds_increased = '0')");
            var characterNonIncreasedOddsCount = Convert.ToInt32(charactersWithoutIncreasedOdds);

            //If the product in the parenthesis is 0, odds would be normalized with all characters in the Premium Roulette
            var adjustedNormalizedCharacterOdds = (overallCharacterOdds - (increasedCharacterOdds * characterIncreasedOddsCount)) / characterNonIncreasedOddsCount;

            var charactersWithIncreasedOddsCmd = new MySqlCommand(charactersWithIncreasedOdds, conn);
            var charactersWithIncreasedOddsRdr = charactersWithIncreasedOddsCmd.ExecuteReader();

            Character character = new();

            //This while loop will not run if there are no characters with increased odds
            while (charactersWithIncreasedOddsRdr.Read())
            {
                character.characterId = Convert.ToInt32(charactersWithIncreasedOddsRdr["id"]);
                var increasedOddsSql = Db.GetCommand(@"INSERT INTO `sw_chaorouletteprizelist` (chao_id, chao_rarity, chao_weight) VALUES ('{0}', '{1}', '{2}')", character.characterId, (long)Item.ItemID.CharacterEgg, increasedCharacterOdds);
                var insertIncreasedOddsCmd = new MySqlCommand(increasedOddsSql, conn);
                insertIncreasedOddsCmd.ExecuteNonQuery();
                availableCharacters.Add(character);
                charactersWithIncreasedOddsRdr.Read();
            }

            charactersWithIncreasedOddsRdr.Close();

            var charactersWithoutIncreasedOddsCmd = new MySqlCommand(charactersWithoutIncreasedOdds, conn);
            var charactersWithoutIncreasedOddsRdr = charactersWithoutIncreasedOddsCmd.ExecuteReader();

            //This while loop will run regardless if there are characters with increased odds or not
            while (charactersWithoutIncreasedOddsRdr.Read())
            {
                character.characterId = Convert.ToInt32(charactersWithoutIncreasedOddsRdr["id"]);
                var sql = Db.GetCommand(@"INSERT INTO `sw_chaorouletteprizelist` (chao_id, chao_rarity, chao_weight) VALUES ('{0}', '{1}', '{2}')", character.characterId, (long)Item.ItemID.CharacterEgg, adjustedNormalizedCharacterOdds);
                var insertCmd = new MySqlCommand(sql, conn);
                insertCmd.ExecuteNonQuery();
                availableCharacters.Add(character);
                charactersWithoutIncreasedOddsRdr.Read();
            }
            charactersWithoutIncreasedOddsRdr.Close();

            characterState = availableCharacters.ToArray();

            return SRStatusCode.Ok;
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
}
