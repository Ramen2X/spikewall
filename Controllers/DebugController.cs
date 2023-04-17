using Microsoft.AspNetCore.Mvc;
using spikewall.Object;
using spikewall.Request;
using spikewall.Response;
using spikewall.Debug;

namespace spikewall.Controllers
{
    /// <summary>
    /// Controller for the various debug endpoints
    /// that are used by the client's debugging functions.
    /// 
    /// The data sent to these endpoints is not encrypted
    /// unlike the endpoints used in the normal game.
    /// 
    /// Packets were never made available for
    /// these endpoints for obvious reasons so these
    /// implementations are crafted purely through guesswork.
    /// 
    /// You would most likely want to disable these
    /// when running in a production environment.
    /// </summary>
    [ApiController]
    [Route("Debug")]
    public class DebugController : ControllerBase
    {
        [HttpPost]
        [Route("updMileageData")]
        [Produces("text/json")]
        public JsonResult UpdateMileageData([FromForm] string param, [FromForm] string secure, [FromForm] string key = "")
        {
            var iv = (string)Config.Get("encryption_iv");

            if ((sbyte)Config.Get("enable_debug_endpoints") != 1)
                return new JsonResult(EncryptedResponse.Generate(iv, new BaseResponse())); // abort if not enabled

            using var conn = Db.Get();
            conn.Open();

            var clientReq = new ClientRequest<UpdateMileageDataRequest>(conn, param, secure, key);
            if (clientReq.error != SRStatusCode.Ok)
            {
                return new JsonResult(EncryptedResponse.Generate(iv, clientReq.error));
            }

            DebugHelper.Log("user " + clientReq.userId + " updated their MileageMapState through Debug Menu", 2);

            var mileageMapState = clientReq.request.mileageMapState;

            var saveStatus = mileageMapState.Save(conn, clientReq.userId);
            if (saveStatus != SRStatusCode.Ok)
            {
                return new JsonResult(EncryptedResponse.Generate(iv, saveStatus));
            }

            conn.Close();
            return new JsonResult(EncryptedResponse.Generate(iv, new BaseResponse()));
        }

        [HttpPost]
        [Route("updUserData")]
        [Produces("text/json")]
        public JsonResult UpdateUserData([FromForm] string param, [FromForm] string secure, [FromForm] string key = "")
        {
            var iv = (string)Config.Get("encryption_iv");

            if ((sbyte)Config.Get("enable_debug_endpoints") != 1)
                return new JsonResult(EncryptedResponse.Generate(iv, new BaseResponse())); // abort if not enabled

            using var conn = Db.Get();
            conn.Open();

            var clientReq = new ClientRequest<UpdateUserDataRequest>(conn, param, secure, key);
            if (clientReq.error != SRStatusCode.Ok)
            {
                return new JsonResult(EncryptedResponse.Generate(iv, clientReq.error));
            }

            DebugHelper.Log("user " + clientReq.userId + " updated their rank through Debug Menu", 2);

            PlayerState playerState = new();
            playerState.Populate(conn, clientReq.userId);

            playerState.numRank += (short)clientReq.request.addRank;
            if (playerState.numRank > 998)
            {
                playerState.numRank = 998;
            }
            if (playerState.numRank < 0)
            {
                playerState.numRank = 0;
            }

            var saveStatus = playerState.Save(conn, clientReq.userId);
            if (saveStatus != SRStatusCode.Ok)
            {
                return new JsonResult(EncryptedResponse.Generate(iv, saveStatus));
            }

            conn.Close();
            return new JsonResult(EncryptedResponse.Generate(iv, new BaseResponse()));
        }
    }
}
