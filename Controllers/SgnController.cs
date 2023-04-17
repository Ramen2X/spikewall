using Microsoft.AspNetCore.Mvc;
using spikewall.Request;
using spikewall.Response;

namespace spikewall.Controllers
{
    /// <summary>
    /// Controller for SEGA Networks endpoints.
    ///
    /// Most of these endpoints are not used by Sonic
    /// Runners Revival, but they'll be implemented
    /// here for completeness.
    /// </summary>

    [ApiController]
    [Route("sgn")]
    [Produces("text/json")]
    public class SgnController : ControllerBase
    {
        /// <summary>
        /// An endpoint originally used to send
        /// data to an internal telemetry system.
        /// </summary>
        /// <remarks>
        /// In Spikewall, this is implemented as a stub.
        /// </remarks>
        [HttpPost]
        [Route("sendApollo")]
        public JsonResult sendApollo([FromForm] string param, [FromForm] string secure, [FromForm] string key = "")
        {
            var iv = (string)Config.Get("encryption_iv");

            using var conn = Db.Get();
            conn.Open();

            var clientReq = new ClientRequest<BaseRequest>(conn, param, secure, key);
            if (clientReq.error != SRStatusCode.Ok) {
                return new JsonResult(EncryptedResponse.Generate(iv, clientReq.error));
            }

            // Client appears to send the following information:
            //
            //   {"sessionId":<session-id>,"value":["1"],"type":"8","seq":"1","version":"2.2.3"}
            //
            // It is not clear if this info needs to be verified or processed in any way yet
            // (e.g. should we be checking if the session ID has expired?) but the game only
            // needs a simple base response, so that's all we do for now.

            return new JsonResult(EncryptedResponse.Generate(iv, new BaseResponse()));
        }
    }

    // TODO: implement stub setAtomSerial and setNoahId endpoints
}
