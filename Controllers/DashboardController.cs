using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;

namespace spikewall.Controllers
{
    [ApiController]
    [Route("Dashboard")]
    public class DashboardController : ControllerBase
    {
        [HttpGet]
        [Route("resetDatabase")]
        public IActionResult ResetDatabase(bool chao = false,
                                           bool players = false,
                                           bool characters = false,
                                           bool mileageMapStates = false,
                                           bool config = false,
                                           bool tickers = false,
                                           bool dailyChallenge = false,
                                           bool costs = false,
                                           bool information = false,
                                           bool incentives = false)
        {
            try
            {
                Db.ResetDatabase(chao, players, characters, mileageMapStates, config, tickers, dailyChallenge, costs, information, incentives);
                return StatusCode(200, "Database reset successfully");
            }
            catch (MySqlException e)
            {
                return StatusCode(500, e.ToString());
            }
        }
    }
}
