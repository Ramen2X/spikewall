using Microsoft.AspNetCore.Mvc;
using spikewall.Request;
using spikewall.Response;

namespace spikewall.Controllers
{
    [ApiController]
    [Route("Option")]
    public class OptionController : Controller
    {
        [HttpPost]
        [Route("userResult")]
        [Produces("text/json")]
        public JsonResult GetUserResult([FromForm] string param, [FromForm] string secure, [FromForm] string key = "")
        {
            var iv = (string)Config.Get("encryption_iv");

            using var conn = Db.Get();
            conn.Open();

            var clientReq = new ClientRequest<BaseRequest>(conn, param, secure, key);
            if (clientReq.error != SRStatusCode.Ok)
            {
                return new JsonResult(EncryptedResponse.Generate(iv, clientReq.error));
            }

            // FIXME: Stub

            return new JsonResult(EncryptedResponse.Generate(iv, new UserResultResponse()));
        }
    }
}
