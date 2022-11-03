using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using spikewall.Debug;
using spikewall.Encryption;
using spikewall.Object;
using spikewall.Request;
using spikewall.Response;

namespace spikewall.Controllers
{
    [ApiController]
    public class GameController : ControllerBase
    {
        [HttpPost]
        [Route("/Game/getDailyChalData/")]
        [Produces("text/json")]
        public JsonResult GetDailyChalData([FromForm] string param, [FromForm] string secure, [FromForm] string key = "")
        {
            var iv = (string)Config.Get("encryption_iv");
            BaseResponse error = null;
            BaseRequest request = BaseRequest.Retrieve<BaseRequest>(param, secure, key, out error);
            if (error != null)
            {
                return new JsonResult(EncryptedResponse.Generate(iv, error));
            }

            // FIXME: Stub

            return new JsonResult(EncryptedResponse.Generate(iv, new DailyChalDataResponse()));
        }

        [HttpPost]
        [Route("/Game/getCostList/")]
        [Produces("text/json")]
        public JsonResult GetCostList([FromForm] string param, [FromForm] string secure, [FromForm] string key = "")
        {
            var iv = (string)Config.Get("encryption_iv");
            BaseResponse error = null;
            BaseRequest request = BaseRequest.Retrieve<BaseRequest>(param, secure, key, out error);
            if (error != null)
            {
                return new JsonResult(EncryptedResponse.Generate(iv, error));
            }

            // FIXME: Stub

            return new JsonResult(EncryptedResponse.Generate(iv, new CostListResponse()));
        }

        [HttpPost]
        [Route("/Game/getMileageData/")]
        [Produces("text/json")]
        public JsonResult GetMileageData([FromForm] string param, [FromForm] string secure, [FromForm] string key = "")
        {
            var iv = (string)Config.Get("encryption_iv");
            BaseResponse error = null;
            BaseRequest request = BaseRequest.Retrieve<BaseRequest>(param, secure, key, out error);
            if (error != null)
            {
                return new JsonResult(EncryptedResponse.Generate(iv, error));
            }

            // FIXME: Stub

            return new JsonResult(EncryptedResponse.Generate(iv, new MileageDataResponse()));
        }

        [HttpPost]
        [Route("/Game/getCampaignList/")]
        [Produces("text/json")]
        public JsonResult GetCampaignList([FromForm] string param, [FromForm] string secure, [FromForm] string key = "")
        {
            var iv = (string)Config.Get("encryption_iv");
            BaseResponse error = null;
            BaseRequest request = BaseRequest.Retrieve<BaseRequest>(param, secure, key, out error);
            if (error != null)
            {
                return new JsonResult(EncryptedResponse.Generate(iv, error));
            }

            // FIXME: Stub

            return new JsonResult(EncryptedResponse.Generate(iv, new CampaignListResponse()));
        }

        [HttpPost]
        [Route("/Game/getFreeItemList/")]
        [Produces("text/json")]
        public JsonResult GetFreeItemList([FromForm] string param, [FromForm] string secure, [FromForm] string key = "")
        {
            var iv = (string)Config.Get("encryption_iv");
            BaseResponse error = null;
            BaseRequest request = BaseRequest.Retrieve<BaseRequest>(param, secure, key, out error);
            if (error != null)
            {
                return new JsonResult(EncryptedResponse.Generate(iv, error));
            }

            // FIXME: Stub

            return new JsonResult(EncryptedResponse.Generate(iv, new FreeItemListResponse()));
        }

        [HttpPost]
        [Route("/Game/quickActStart/")]
        [Produces("text/json")]
        public JsonResult QuickActStart([FromForm] string param, [FromForm] string secure, [FromForm] string key = "")
        {
            var iv = (string)Config.Get("encryption_iv");
            BaseResponse error = null;

            // FIXME: Actually do something with this information
            QuickActStartRequest request = BaseRequest.Retrieve<QuickActStartRequest>(param, secure, key, out error);
            if (error != null)
            {
                return new JsonResult(EncryptedResponse.Generate(iv, error));
            }

            QuickActStartResponse quickActStartBaseResponse = new();

            using var conn = Db.Get();
            conn.Open();

            // Client will have only sent the session ID. We'll need to find the user ID ourselves
            var sql = Db.GetCommand("SELECT uid FROM `sw_sessions` WHERE sid = '{0}';", request.sessionId);
            var command = new MySqlCommand(sql, conn);
            var uid = command.ExecuteScalar().ToString();

            // Now that we have the user ID, we can retrieve the player state
            PlayerState playerState = new PlayerState();

            var populateStatus = playerState.Populate(conn, uid);

            conn.Close();

            if (populateStatus == SRStatusCode.Ok)
            {
                quickActStartBaseResponse.playerState = playerState;
                return new JsonResult(EncryptedResponse.Generate(iv, quickActStartBaseResponse));
            }
            else
            {
                // Return error code from Populate() to client
                return new JsonResult(EncryptedResponse.Generate(iv, new BaseResponse(populateStatus)));
            }
        }

        [HttpPost]
        [Route("/Game/quickPostGameResults/")]
        [Produces("text/json")]
        public JsonResult QuickPostGameResults([FromForm] string param, [FromForm] string secure, [FromForm] string key = "")
        {
            var iv = (string)Config.Get("encryption_iv");
            BaseResponse error = null;

            QuickPostGameResultsRequest request = BaseRequest.Retrieve<QuickPostGameResultsRequest>(param, secure, key, out error);
            if (error != null)
            {
                return new JsonResult(EncryptedResponse.Generate(iv, error));
            }

            using var conn = Db.Get();
            conn.Open();

            // Client will have only sent the session ID. We'll need to find the user ID ourselves
            var sql = Db.GetCommand("SELECT uid FROM `sw_sessions` WHERE sid = '{0}';", request.sessionId);
            var command = new MySqlCommand(sql, conn);
            var uid = command.ExecuteScalar().ToString();

            // Now that we have the user ID, we can retrieve the player state
            PlayerState playerState = new PlayerState();

            var populateStatus = playerState.Populate(conn, uid);

            conn.Close();

            var score = ulong.Parse(request.score);
            var animals = ulong.Parse(request.numAnimals);
            var rings = ulong.Parse(request.numRings);
            var redStarRings = ulong.Parse(request.numRedStarRings);

            // Update the PlayerState with information from the run
            if (playerState.quickTotalHighScore < score)
            {
                playerState.quickTotalHighScore = score;
            }

            playerState.numAnimals += animals;
            playerState.numRings += rings;
            playerState.numRedRings += redStarRings;

            // FIXME: Push this new PlayerState to the one in the database

            QuickPostGameResultsResponse quickPostGameResultsResponse = new();
            quickPostGameResultsResponse.playerState = playerState;

            // FIXME: Actually implement this normally lmao

            quickPostGameResultsResponse.dailyChallengeIncentive = new Incentive[0];
            quickPostGameResultsResponse.messageList = new string[0];
            quickPostGameResultsResponse.operatorMessageList = new string[0];
            quickPostGameResultsResponse.totalMessage = 0;
            quickPostGameResultsResponse.totalOperatorMessage = 0;

            return new JsonResult(EncryptedResponse.Generate(iv, quickPostGameResultsResponse));
        }
    }
}
