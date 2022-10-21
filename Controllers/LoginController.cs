using Microsoft.AspNetCore.Mvc;
using spikewall.Debug;

namespace spikewall.Controllers
{
    [ApiController]
    [Route("/login/Login/")]
    [Produces("text/json")]
    public class LoginController : ControllerBase
    {
        [HttpPost(Name = "Login")]
        public void Login()
        {

        }
    }
}
