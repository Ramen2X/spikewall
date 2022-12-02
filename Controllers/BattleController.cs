using Microsoft.AspNetCore.Mvc;
using spikewall.Request;
using spikewall.Response;

namespace spikewall.Controllers
{
    /// <summary>
    /// Controller for endpoints relating to the Daily Battle feature.
    /// </summary>
    /// <remarks>
    /// Just like the Debug endpoints, it seems these are all unencrypted too
    /// </remarks>
    [ApiController]
    [Route("Battle")]
    public class BattleController : ControllerBase
    {
        /*[HttpPost]
        [Route("getDailyBattleData")]
        [Produces("text/json")]
        public JsonResult GetDailyBattleData([FromForm] string param, [FromForm] string secure, [FromForm] string key = "")
        {
            var iv = (string)Config.Get("encryption_iv");
            BaseResponse error = null;
            BaseRequest request = BaseRequest.Retrieve<BaseRequest>(param, secure, key, out error);
            if (error != null) {
                return new JsonResult(EncryptedResponse.Generate(iv, error));
            }

            // FIXME: Stub

            return new JsonResult(EncryptedResponse.Generate(iv, new BaseResponse()));
        }

        [HttpPost]
        [Route("updateDailyBattleStatus")]
        [Produces("text/json")]
        public JsonResult UpdateDailyBattleStatus([FromForm] string param, [FromForm] string secure, [FromForm] string key = "")
        {
            var iv = (string)Config.Get("encryption_iv");
            BaseResponse error = null;
            BaseRequest request = BaseRequest.Retrieve<BaseRequest>(param, secure, key, out error);
            if (error != null) {
                return new JsonResult(EncryptedResponse.Generate(iv, error));
            }

            // FIXME: Stub

            return new JsonResult(EncryptedResponse.Generate(iv, new BaseResponse()));
        }*/
    }
}
