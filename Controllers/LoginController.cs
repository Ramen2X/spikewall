using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using spikewall.Debug;
using spikewall.Encryption;
using spikewall.Request;

namespace spikewall.Controllers
{
    [ApiController]
    [Route("/login/Login/")]
    [Produces("text/json")]
    public class LoginController : ControllerBase
    {
        [HttpPost]
        public void Login([FromForm] string param, [FromForm] string secure, [FromForm] string key = "")
        {
            string paramJSON = param;

            // The secure parameter is sent by the client to indicate if its param is encrypted.
            if (secure.Equals("1")) {
                paramJSON = EncryptionHelper.Decrypt(paramJSON, key);
            }

            LoginRequest? loginRequest = JsonSerializer.Deserialize<LoginRequest>(paramJSON);
        }
    }
}
