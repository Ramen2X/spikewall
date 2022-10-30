using Microsoft.AspNetCore.Mvc;
using MySql.Data;
using MySql.Data.MySqlClient;
using spikewall.Debug;
using spikewall.Encryption;
using spikewall.Object;
using spikewall.Request;
using spikewall.Response;

namespace spikewall.Controllers
{
    [ApiController]
    public class PlayerController : ControllerBase
    {
        [HttpPost]
        [Route("/Player/getPlayerState/")]
        [Produces("text/json")]
        public JsonResult GetPlayerState([FromForm] string param, [FromForm] string secure, [FromForm] string key = "")
        {
            var iv = (string)Config.Get("encryption_iv");
            BaseResponse error = null;
            BaseRequest request = BaseRequest.Retrieve<BaseRequest>(param, secure, key, out error);
            if (error != null) {
                return new JsonResult(EncryptedResponse.Generate(iv, error));
            }

            using var conn = Db.Get();
            conn.Open();

            // Client will have only sent the session ID. We'll need to find the user ID ourselves
            var sql = Db.GetCommand("SELECT uid FROM `sw_sessions` WHERE sid = '{0}';", request.sessionId);
            var command = new MySqlCommand(sql, conn);
            var uid = command.ExecuteScalar().ToString();

            // Now that we have the user ID, we can retrieve the player state
            PlayerState playerState = new PlayerState();

            sql = Db.GetCommand(@"SELECT
                    main_chara_id,
                    sub_chara_id,
                    main_chao_id,
                    sub_chao_id,
                    num_rings,
                    num_buy_rings,
                    num_red_rings,
                    num_buy_red_rings,
                    energy,
                    energy_buy,
                    energy_renews_at,
                    num_messages,
                    ranking_league,
                    quick_ranking_league,
                    num_roulette_ticket,
                    num_chao_roulette_ticket,
                    chao_eggs,
                    total_high_score,
                    quick_total_high_score,
                    total_distance,
                    maximum_distance,
                    daily_mission_id,
                    daily_mission_end_time,
                    daily_challenge_value,
                    daily_challenge_complete,
                    num_daily_challenge_cont,
                    num_playing,
                    num_animals,
                    num_rank
                FROM `sw_players` WHERE id = '{0}';", uid);
            command = new MySqlCommand(sql, conn);

            var reader = command.ExecuteReader();
            if (!reader.HasRows) {
                // Somehow failed to find player, return error code
                var errResponse = new BaseResponse(BaseResponse.SRStatusCode.MissingPlayer);
                return new JsonResult(EncryptedResponse.Generate(iv, errResponse));
            }

            // FIXME: I strongly suspect some of these can be calculated rather than manual cells
            //        in this table so expect these to change.
            reader.Read();
            // FIXME: Missing items and equipItemList
            playerState.items = new Item[0];
            playerState.equipItemList = new string[0];
            playerState.mainCharaID = reader.GetString("main_chara_id");
            playerState.subCharaID = reader.GetString("sub_chara_id");
            playerState.mainChaoID = reader.GetString("main_chao_id");
            playerState.subChaoID = reader.GetString("sub_chao_id");
            playerState.numRings = reader.GetInt64("num_rings");
            playerState.numBuyRings = reader.GetInt64("num_buy_rings");
            playerState.numRedRings = reader.GetInt64("num_red_rings");
            playerState.numBuyRedRings = reader.GetInt64("num_buy_red_rings");
            playerState.energy = reader.GetInt64("energy");
            playerState.energyBuy = reader.GetInt64("energy_buy");
            playerState.energyRenewsAt = reader.GetInt64("energy_renews_at");
            playerState.mumMessages = reader.GetInt64("num_messages");
            playerState.rankingLeague = reader.GetInt64("ranking_league");
            playerState.quickRankingLeague = reader.GetInt64("quick_ranking_league");
            playerState.numRouletteTicket = reader.GetInt64("num_roulette_ticket");
            playerState.numChaoRouletteTicket = reader.GetInt64("num_chao_roulette_ticket");
            playerState.chaoEggs = reader.GetInt64("chao_eggs");
            playerState.totalHighScore = reader.GetInt64("total_high_score");
            playerState.quickTotalHighScore = reader.GetInt64("quick_total_high_score");
            playerState.totalDistance = reader.GetInt64("total_distance");
            playerState.maximumDistance = reader.GetInt64("maximum_distance");
            playerState.dailyMissionId = reader.GetInt64("daily_mission_id");
            playerState.dailyMissionEndTime = reader.GetInt64("daily_mission_end_time");
            playerState.dailyChallengeValue = reader.GetInt64("daily_challenge_value");
            playerState.dailyChallengeComplete = reader.GetInt64("daily_challenge_complete");
            playerState.numDailyChalCont = reader.GetInt64("num_daily_challenge_cont");
            playerState.numPlaying = reader.GetInt64("num_playing");
            playerState.numAnimals = reader.GetInt64("num_animals");
            playerState.numRank = reader.GetInt64("num_rank");
            reader.Close();

            conn.Close();

            return new JsonResult(EncryptedResponse.Generate(iv, new PlayerStateResponse(playerState)));
        }

        [HttpPost]
        [Route("/Player/getCharacterState/")]
        [Produces("text/json")]
        public JsonResult GetCharacterState([FromForm] string param, [FromForm] string secure, [FromForm] string key = "")
        {
            var iv = (string)Config.Get("encryption_iv");
            BaseResponse error = null;
            BaseRequest request = BaseRequest.Retrieve<BaseRequest>(param, secure, key, out error);
            if (error != null) {
                return new JsonResult(EncryptedResponse.Generate(iv, error));
            }

            using var conn = Db.Get();
            conn.Open();

            // Client will have only sent the session ID. We'll need to find the user ID ourselves
            var sql = Db.GetCommand("SELECT uid FROM `sw_sessions` WHERE sid = '{0}';", request.sessionId);
            var command = new MySqlCommand(sql, conn);
            var uid = command.ExecuteScalar().ToString();

            // Get list of all visible characters
            command = new MySqlCommand("SELECT * FROM `sw_characters` WHERE visible = '1';", conn);

            List<Character> characters = new List<Character>();

            var charRdr = command.ExecuteReader();
            while (charRdr.Read()) {
                Character c = new Character();

                c.characterId = charRdr.GetString("id");

                // FIXME: Hardcoded empty
                c.campaignList = new Campaign[0];

                c.numRings = Convert.ToInt64(charRdr["num_rings"]);
                c.numRedRings = Convert.ToInt64(charRdr["num_red_rings"]);
                c.priceNumRings = Convert.ToInt64(charRdr["price_num_rings"]);
                c.priceNumRedRings = Convert.ToInt64(charRdr["price_num_red_rings"]);
                c.starMax = Convert.ToInt64(charRdr["star_max"]);
                c.lockCondition = Convert.ToInt64(charRdr["lock_condition"]);

                characters.Add(c);
            }

            charRdr.Close();

            for (int i = 0; i < characters.Count; i++) {
                Character c = characters[i];

                sql = Db.GetCommand("SELECT * FROM `sw_characterstates` WHERE user_id = '{0}' AND character_id = '{1}';", uid, c.characterId);
                var stateCmd = new MySqlCommand(sql, conn);
                var stateRdr = stateCmd.ExecuteReader();

                if (stateRdr.HasRows) {
                    // Read row
                    stateRdr.Read();

                    c.status = Convert.ToInt32(stateRdr["status"]);
                    c.level = Convert.ToInt64(stateRdr["level"]);
                    c.exp = Convert.ToInt64(stateRdr["exp"]);
                    c.star = Convert.ToInt64(stateRdr["star"]);

                    c.abilityLevel = ConvertDBListToIntArray(stateRdr.GetString("ability_level"));
                    c.abilityNumRings = ConvertDBListToIntArray(stateRdr.GetString("ability_num_rings"));
                    c.abilityLevelup = ConvertDBListToIntArray(stateRdr.GetString("ability_levelup"));
                    c.abilityLevelupExp = ConvertDBListToIntArray(stateRdr.GetString("ability_levelup_exp"));

                    stateRdr.Close();
                } else {
                    stateRdr.Close();

                    // Insert rows
                    c.status = 1;
                    c.level = 0;
                    c.exp = 0;
                    c.star = 0;

                    var abilityLevelStr = "0 0 0 0 0 0 0 0 0 0 0";
                    var abilityLevelupStr = "120000";

                    c.abilityLevel = ConvertDBListToIntArray(abilityLevelStr);
                    c.abilityNumRings = ConvertDBListToIntArray(abilityLevelStr);
                    c.abilityLevelup = ConvertDBListToIntArray(abilityLevelupStr);
                    c.abilityLevelupExp = ConvertDBListToIntArray(abilityLevelStr);

                    sql = Db.GetCommand(@"INSERT INTO `sw_characterstates` (
                                              user_id, character_id, status, level, exp, star, ability_level, ability_num_rings, ability_levelup, ability_levelup_exp
                                          ) VALUES (
                                              '{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}'
                                          );", uid, c.characterId, c.status, c.level, c.exp, c.star, abilityLevelStr, abilityLevelStr, abilityLevelupStr, abilityLevelStr);
                    var insertCmd = new MySqlCommand(sql, conn);
                    insertCmd.ExecuteNonQuery();
                }
            }

            conn.Close();

            return new JsonResult(EncryptedResponse.Generate(iv, new CharacterStateResponse(characters.ToArray())));
        }

        static private long[] ConvertDBListToIntArray(string s)
        {
            string[] tokens = s.Split(' ');
            long[] values = new long[tokens.Length];
            for (int i = 0; i < values.Length; i++) {
                values[i] = long.Parse(tokens[i]);
            }
            return values;
        }
    }
}
