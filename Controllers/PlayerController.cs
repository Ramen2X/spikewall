using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using spikewall.Object;
using spikewall.Request;
using spikewall.Response;

namespace spikewall.Controllers
{
    [ApiController]
    [Route("Player")]
    public class PlayerController : ControllerBase
    {
        [HttpPost]
        [Route("getPlayerState")]
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
            PlayerState playerState = new();

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
        [Route("getCharacterState")]
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
        [Route("getChaoState")]
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

            Chao[] chaoState;
            SRStatusCode chaoStateStatus = Chao.PopulateChaoState(conn, clientReq.userId, out chaoState);
            if (chaoStateStatus != SRStatusCode.Ok)
            {
                return new JsonResult(EncryptedResponse.Generate(iv, chaoStateStatus));
            }

            return new JsonResult(EncryptedResponse.Generate(iv, new ChaoStateResponse(chaoState)));
        }

        [HttpPost]
        [Route("setUserName")]
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
                return new JsonResult(EncryptedResponse.Generate(iv, SRStatusCode.ClientError));
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
