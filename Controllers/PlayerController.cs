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

            var populateStatus = playerState.Populate(conn, uid);

            conn.Close();

            if (populateStatus == SRStatusCode.Ok) {
                // Return successful PlayerState response
                return new JsonResult(EncryptedResponse.Generate(iv, new PlayerStateResponse(playerState)));
            } else {
                // Return error code from Populate() to client
                return new JsonResult(EncryptedResponse.Generate(iv, new BaseResponse(populateStatus)));
            }
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
                c.starMax = Convert.ToSByte(charRdr["star_max"]);
                c.lockCondition = Convert.ToSByte(charRdr["lock_condition"]);

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

                    c.status = Convert.ToSByte(stateRdr["status"]);
                    c.level = Convert.ToSByte(stateRdr["level"]);
                    c.exp = Convert.ToInt64(stateRdr["exp"]);
                    c.star = Convert.ToSByte(stateRdr["star"]);

                    c.abilityLevel = ConvertDBListToIntArray(stateRdr.GetString("ability_level"));
                    c.abilityNumRings = ConvertDBListToIntArray(stateRdr.GetString("ability_num_rings"));
                    c.abilityLevelup = ConvertDBListToIntArray(stateRdr.GetString("ability_levelup"));
                    c.abilityLevelupExp = ConvertDBListToIntArray(stateRdr.GetString("ability_levelup_exp"));

                    stateRdr.Close();
                } else {
                    stateRdr.Close();

                    // Insert rows
                    c.status = 1; // FIXME: Hardcoded to "enabled" for now, should calculate this (probably based on lockCondition) later
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

        [HttpPost]
        [Route("/Player/getChaoState/")]
        [Produces("text/json")]
        public JsonResult GetChaoState([FromForm] string param, [FromForm] string secure, [FromForm] string key = "")
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

            // Get list of all visible chao
            command = new MySqlCommand("SELECT * FROM `sw_chao`;", conn);

            List<Chao> chao = new List<Chao>();

            using (var chaoRdr = command.ExecuteReader())
            {
                while (chaoRdr.Read()) {
                    Chao c = new Chao();

                    c.chaoID = chaoRdr.GetString("id");
                    c.rarity = Convert.ToInt64(chaoRdr["rarity"]);
                    c.hidden = Convert.ToInt64(chaoRdr["hidden"]);

                    chao.Add(c);
                }

                chaoRdr.Close();
            }

            for (int i = 0; i < chao.Count; i++) {
                Chao c = chao[i];

                sql = Db.GetCommand("SELECT * FROM `sw_chaostates` WHERE user_id = '{0}' AND chao_id = '{1}';", uid, c.chaoID);
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

                    // Insert rows
                    c.status = 0;
                    c.level = 0;
                    c.setStatus = 0;
                    c.acquired = 0;

                    sql = Db.GetCommand(@"INSERT INTO `sw_chaostates` (chao_id, user_id) VALUES ('{0}', '{1}');", c.chaoID, uid);
                    var insertCmd = new MySqlCommand(sql, conn);
                    insertCmd.ExecuteNonQuery();
                }
            }

            conn.Close();

            return new JsonResult(EncryptedResponse.Generate(iv, new ChaoStateResponse(chao.ToArray())));
        }

        [HttpPost]
        [Route("/Player/setUserName/")]
        [Produces("text/json")]
        public JsonResult SetUserName([FromForm] string param, [FromForm] string secure, [FromForm] string key = "")
        {
            var iv = (string)Config.Get("encryption_iv");
            BaseResponse error = null;
            SetUserNameRequest request = BaseRequest.Retrieve<SetUserNameRequest>(param, secure, key, out error);
            if (error != null) {
                return new JsonResult(EncryptedResponse.Generate(iv, error));
            }

            using var conn = Db.Get();
            conn.Open();

            // Retrieve user ID (FIXME: Should probably roll this into "Retrieve")
            var sql = Db.GetCommand("SELECT uid FROM `sw_sessions` WHERE sid = '{0}';", request.sessionId);
            var command = new MySqlCommand(sql, conn);
            var uid = command.ExecuteScalar().ToString();

            // Set player username as requested
            sql = Db.GetCommand("UPDATE `sw_players` SET username = '{0}' WHERE id = '{1}';", request.userName, uid);
            command = new MySqlCommand(sql, conn);
            command.ExecuteScalar();

            conn.Close();

            return new JsonResult(EncryptedResponse.Generate(iv, new BaseResponse()));
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
