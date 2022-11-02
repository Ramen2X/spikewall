﻿using Microsoft.AspNetCore.Mvc;
using spikewall.Debug;
using spikewall.Encryption;
using spikewall.Request;
using spikewall.Response;

namespace spikewall.Controllers
{
    [ApiController]
    public class ChaoController : ControllerBase
    {
        [HttpPost]
        [Route("/Chao/getChaoWheelOptions/")]
        [Produces("text/json")]
        public JsonResult GetChaoWheelOptions([FromForm] string param, [FromForm] string secure, [FromForm] string key = "")
        {
            var iv = (string)Config.Get("encryption_iv");
            BaseResponse error = null;
            RedstarExchangeListRequest request = BaseRequest.Retrieve<RedstarExchangeListRequest>(param, secure, key, out error);
            if (error != null) {
                return new JsonResult(EncryptedResponse.Generate(iv, error));
            }

            // FIXME: Stub

            return new JsonResult(EncryptedResponse.Generate(iv, new ChaoWheelOptionsResponse()));
        }

        [HttpPost]
        [Route("/Chao/getPrizeChaoWheelSpin/")]
        [Produces("text/json")]
        public JsonResult GetPrizeChaoWheelOptions([FromForm] string param, [FromForm] string secure, [FromForm] string key = "")
        {
            var iv = (string)Config.Get("encryption_iv");
            BaseResponse error = null;
            RedstarExchangeListRequest request = BaseRequest.Retrieve<RedstarExchangeListRequest>(param, secure, key, out error);
            if (error != null) {
                return new JsonResult(EncryptedResponse.Generate(iv, error));
            }

            // FIXME: Stub

            return new JsonResult(EncryptedResponse.Generate(iv, new PrizeChaoWheelSpinResponse()));
        }
    }
}