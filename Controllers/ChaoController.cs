using Microsoft.AspNetCore.Mvc;
using spikewall.Object;
using spikewall.Request;
using spikewall.Response;

namespace spikewall.Controllers
{
    [ApiController]
    [Route("Chao")]
    public class ChaoController : ControllerBase
    {
        [HttpPost]
        [Route("getChaoWheelOptions")]
        [Produces("text/json")]
        public JsonResult GetChaoWheelOptions([FromForm] string param, [FromForm] string secure, [FromForm] string key = "")
        {
            var iv = (string)Config.Get("encryption_iv");

            using var conn = Db.Get();
            conn.Open();

            var clientReq = new ClientRequest<RedstarExchangeListRequest>(conn, param, secure, key);
            if (clientReq.error != SRStatusCode.Ok) {
                return new JsonResult(EncryptedResponse.Generate(iv, clientReq.error));
            }

            PlayerState playerState = new();

            var populateStatus = playerState.Populate(conn, clientReq.userId);
            if (populateStatus != SRStatusCode.Ok)
            {
                return new JsonResult(EncryptedResponse.Generate(iv, populateStatus));
            }

            ChaoWheelOptions chaoWheelOptions = new();
            chaoWheelOptions.PopulateChaoWheel(conn, clientReq.userId);

            ChaoWheelOptionsResponse chaoWheelOptionsResponse = new()
            {
                chaoWheelOptions = chaoWheelOptions
            };

            return new JsonResult(EncryptedResponse.Generate(iv, new ChaoWheelOptionsResponse()));
        }

        [HttpPost]
        [Route("getPrizeChaoWheelSpin")]
        [Produces("text/json")]
        public JsonResult GetPrizeChaoWheelOptions([FromForm] string param, [FromForm] string secure, [FromForm] string key = "")
        {
            var iv = (string)Config.Get("encryption_iv");

            using var conn = Db.Get();
            conn.Open();

            var clientReq = new ClientRequest<RedstarExchangeListRequest>(conn, param, secure, key);
            if (clientReq.error != SRStatusCode.Ok) {
                return new JsonResult(EncryptedResponse.Generate(iv, clientReq.error));
            }

            // FIXME: Stub

            return new JsonResult(EncryptedResponse.Generate(iv, new PrizeChaoWheelSpinResponse()));
        }
    }
}
