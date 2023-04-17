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
                                           bool itemOwnership = false,
                                           bool information = false,
                                           bool incentives = false,
                                           bool wheelOptions = false,
                                           bool itemRouletteOptions = false)
        {
            // FIXME: this endpoint needs to be privileged

            try
            {
                Db.ResetDatabase(chao, players, characters, mileageMapStates, config, tickers, dailyChallenge, costs, itemOwnership, information, incentives, wheelOptions, itemRouletteOptions);
                return StatusCode(200, "Database reset successfully");
            }
            catch (MySqlException e)
            {
                return StatusCode(500, e.ToString());
            }
        }

        [HttpPost]
        [Route("setDatabaseDetails")]
        public IActionResult SetDatabaseDetails(string host, string port, string username, string password, string database)
        {
            // FIXME: this endpoint needs to be privileged

            try
            {
                Db.SetDetails(host, port, username, password, database);
                return StatusCode(200, "Database details updated successfully");
            }
            catch (MySqlException e)
            {
                return StatusCode(400, "Failed to connect to MySQL with provided details: " + e);
            }
        }
    }
}
