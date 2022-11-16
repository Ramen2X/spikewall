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
            if (clientReq.error != SRStatusCode.Ok)
            {
                return new JsonResult(EncryptedResponse.Generate(iv, clientReq.error));
            }

            Character[] characterState;

            var populateState = Character.PopulateCharacterState(conn, clientReq.userId, out characterState);
            if (populateState != SRStatusCode.Ok)
            {
                return new JsonResult(EncryptedResponse.Generate(iv, new BaseResponse(SRStatusCode.InternalServerError)));
            }

            conn.Close();
            return new JsonResult(EncryptedResponse.Generate(iv, new CharacterStateResponse(characterState)));
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
    }
}
