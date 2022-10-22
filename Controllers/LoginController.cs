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
        public void Login([FromForm] string param, [FromForm] string secure, [FromForm] string key)
        {
            string decryptedJSON = EncryptionHelper.Decrypt(param, key);

            LoginRequest? loginRequest = JsonSerializer.Deserialize<LoginRequest>(decryptedJSON);
        }
    }
}
