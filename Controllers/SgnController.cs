using Microsoft.AspNetCore.Mvc;
using spikewall.Debug;
using spikewall.Encryption;
using spikewall.Response;

namespace spikewall.Controllers
{
    [ApiController]
    [Route("/sgn/sendApollo/")]
    [Produces("text/json")]
    public class SgnController : ControllerBase
    {
        [HttpPost]
        public JsonResult sendApollo([FromForm] string param, [FromForm] string secure, [FromForm] string key = "")
        {
            string paramJSON = param;

            // The secure parameter is sent by the client to indicate if its param is encrypted.
            if (secure.Equals("1")) {
                paramJSON = EncryptionHelper.Decrypt(paramJSON, key);
            }

            // Client appears to send the following information:
            //
            //   {"sessionId":<session-id>,"value":["1"],"type":"8","seq":"1","version":"2.2.3"}
            //
            // It is not clear if this info needs to be verified or processed in any way yet
            // (e.g. should we be checking if the session ID has expired?) but the game only
            // needs a simple base response, so that's all we do for now.

            return new JsonResult(EncryptedResponse.Generate(key, new BaseResponse()));
        }
    }
}
