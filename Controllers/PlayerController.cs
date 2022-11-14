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

            using var conn = Db.Get();
            conn.Open();

            var clientReq = new ClientRequest<BaseRequest>(conn, param, secure, key);
            if (clientReq.error != SRStatusCode.Ok) {
                return new JsonResult(EncryptedResponse.Generate(iv, clientReq.error));
            }

            // Now that we have the user ID, we can retrieve the player state
            PlayerState playerState = new PlayerState();

            var populateStatus = playerState.Populate(conn, clientReq.userId);

            conn.Close();

            if (populateStatus == SRStatusCode.Ok)
            {
                // Return successful PlayerState response
                return new JsonResult(EncryptedResponse.Generate(iv, new PlayerStateResponse(playerState)));
            }
            else
            {
                // Return error code from Populate() to client
                return new JsonResult(EncryptedResponse.Generate(iv, populateStatus));
            }
        }

        [HttpPost]
        [Route("/Player/getCharacterState/")]
        [Produces("text/json")]
        public JsonResult GetCharacterState([FromForm] string param, [FromForm] string secure, [FromForm] string key = "")
        {
            var iv = (string)Config.Get("encryption_iv");

            using var conn = Db.Get();
            conn.Open();

            var clientReq = new ClientRequest<BaseRequest>(conn, param, secure, key);
            if (clientReq.error != SRStatusCode.Ok) {
                return new JsonResult(EncryptedResponse.Generate(iv, clientReq.error));
            }

            // Get list of all visible characters
            var command = new MySqlCommand("SELECT * FROM `sw_characters` WHERE visible = '1';", conn);

            List<Character> characters = new List<Character>();

            var charRdr = command.ExecuteReader();
            while (charRdr.Read()) {
                Character c = new Character();

                c.characterId = charRdr.GetString("id");

                // FIXME: Hardcoded empty
                c.campaignList = new Campaign[0];

                c.numRings = Convert.ToUInt64(charRdr["num_rings"]);
                c.numRedRings = Convert.ToUInt64(charRdr["num_red_rings"]);
                c.priceNumRings = Convert.ToUInt64(charRdr["price_num_rings"]);
                c.priceNumRedRings = Convert.ToUInt64(charRdr["price_num_red_rings"]);
                c.starMax = Convert.ToSByte(charRdr["star_max"]);
                c.lockCondition = Convert.ToSByte(charRdr["lock_condition"]);

                characters.Add(c);
            }

            charRdr.Close();

            for (int i = 0; i < characters.Count; i++) {
                Character c = characters[i];

                var sql = Db.GetCommand("SELECT * FROM `sw_characterstates` WHERE user_id = '{0}' AND character_id = '{1}';", clientReq.userId, c.characterId);
                var stateCmd = new MySqlCommand(sql, conn);
                var stateRdr = stateCmd.ExecuteReader();

                if (stateRdr.HasRows) {
                    // Read row
                    stateRdr.Read();

                    c.status = Convert.ToSByte(stateRdr["status"]);
                    c.level = Convert.ToSByte(stateRdr["level"]);
                    c.exp = Convert.ToUInt64(stateRdr["exp"]);
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
                                          );", clientReq.userId, c.characterId, c.status, c.level, c.exp, c.star, abilityLevelStr, abilityLevelStr, abilityLevelupStr, abilityLevelStr);
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

            using var conn = Db.Get();
            conn.Open();

            var clientReq = new ClientRequest<BaseRequest>(conn, param, secure, key);
            if (clientReq.error != SRStatusCode.Ok) {
                return new JsonResult(EncryptedResponse.Generate(iv, clientReq.error));
            }

            // Get list of all visible chao
            var command = new MySqlCommand("SELECT * FROM `sw_chao`;", conn);

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

                var sql = Db.GetCommand("SELECT * FROM `sw_chaostates` WHERE user_id = '{0}' AND chao_id = '{1}';", clientReq.userId, c.chaoID);
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

                    sql = Db.GetCommand(@"INSERT INTO `sw_chaostates` (chao_id, user_id) VALUES ('{0}', '{1}');", c.chaoID, clientReq.userId);
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

            using var conn = Db.Get();
            conn.Open();

            var clientReq = new ClientRequest<SetUserNameRequest>(conn, param, secure, key);
            if (clientReq.error != SRStatusCode.Ok) {
                return new JsonResult(EncryptedResponse.Generate(iv, clientReq.error));
            }

            // This should never happen on a vanilla client but we should catch it anyway
            if (clientReq.request.userName.Length > 12)
            {
                // TODO: Test this on a 2.0.3 client
                return new JsonResult(EncryptedResponse.Generate(iv, SRStatusCode.UsernameTooLong));
            }

            // Set player username as requested
            var sql = Db.GetCommand("UPDATE `sw_players` SET username = '{0}' WHERE id = '{1}';", clientReq.request.userName, clientReq.userId);
            var command = new MySqlCommand(sql, conn);
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
