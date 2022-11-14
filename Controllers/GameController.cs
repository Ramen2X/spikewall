using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using spikewall.Debug;
using spikewall.Encryption;
using spikewall.Object;
using spikewall.Request;
using spikewall.Response;
using System.Text;

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

            using var conn = Db.Get();
            conn.Open();

            var clientReq = new ClientRequest<BaseRequest>(conn, param, secure, key);
            if (clientReq.error != SRStatusCode.Ok)
            {
                return new JsonResult(EncryptedResponse.Generate(iv, clientReq.error));
            }

            var sql = Db.GetCommand("SELECT * FROM `sw_dailychallenge`");
            var command = new MySqlCommand(sql, conn);
            var reader = command.ExecuteReader();

            DailyChalDataResponse dailyChalDataResponse = new();

            if (reader.Read())
            {
                Incentive[] incentives = new Incentive[7];

                // We use StringBuilder here rather than concatenation
                // to prevent mass creation of unnecessary objects.
                StringBuilder str = new(11);
                for (int i = 0; i < 7; i++)
                {
                    str.Clear();
                    str.Append("item");

                    incentives[i] = new Incentive();

                    str.Append(i + 1);
                    incentives[i].itemId = reader.GetInt64(str.ToString());

                    str.Append("_count");
                    incentives[i].numItem = reader.GetInt64(str.ToString());

                    incentives[i].numIncentiveCont = i + 1;
                }

                reader.Close();

                PlayerState playerState = new();
                playerState.Populate(conn, clientReq.userId);

                dailyChalDataResponse.incentiveList = incentives;
                dailyChalDataResponse.incentiveListCont = 7;
                dailyChalDataResponse.numDilayChalCont = playerState.numDailyChalCont;
                dailyChalDataResponse.maxDailyChalDay = 7;
                dailyChalDataResponse.numDailyChalDay = 1; // FIXME: Hardcoded
                dailyChalDataResponse.chalEndTime = DateTimeOffset.Now.AddDays(1).AddTicks(-1).ToUnixTimeSeconds(); // FIXME: This should be the end of the day
            }
            else dailyChalDataResponse.incentiveList = Array.Empty<Incentive>();

            return new JsonResult(EncryptedResponse.Generate(iv, dailyChalDataResponse));
        }

        [HttpPost]
        [Route("/Game/getCostList/")]
        [Produces("text/json")]
        public JsonResult GetCostList([FromForm] string param, [FromForm] string secure, [FromForm] string key = "")
        {
            var iv = (string)Config.Get("encryption_iv");

            using var conn = Db.Get();
            conn.Open();

            var clientReq = new ClientRequest<BaseRequest>(conn, param, secure, key);
            if (clientReq.error != SRStatusCode.Ok)
            {
                return new JsonResult(EncryptedResponse.Generate(iv, clientReq.error));
            }

            var sql = Db.GetCommand("SELECT * FROM `sw_costlist`");
            var command = new MySqlCommand(sql, conn);
            var reader = command.ExecuteReader();

            CostListResponse costListResponse = new();

            if (reader.Read())
            {
                ConsumedItem[] consumedItems = new ConsumedItem[15];

                // We use StringBuilder here rather than concatenation
                // to prevent mass creation of unnecessary objects.
                StringBuilder str = new(11);

                for (int i = 0; i < 15; i++)
                {
                    str.Clear();
                    str.Append("item");

                    consumedItems[i] = new ConsumedItem();

                    str.Append(i + 1);
                    consumedItems[i].itemId = reader.GetInt64(str.ToString());

                    str.Append("_cost");
                    consumedItems[i].numItem = reader.GetInt64(str.ToString());

                    str.Replace("_cost", "_id");
                    consumedItems[i].consumedItemId = reader.GetInt64(str.ToString());
                }

                reader.Close();

                costListResponse.consumedCostList = consumedItems;
            }
            else costListResponse.consumedCostList = Array.Empty<ConsumedItem>();

            return new JsonResult(EncryptedResponse.Generate(iv, costListResponse));
        }

        /// <summary>
        /// Endpoint that returns the player's MapMileageState
        /// hit when the opening the main menu.
        /// </summary>
        [HttpPost]
        [Route("/Game/getMileageData/")]
        [Produces("text/json")]
        public JsonResult GetMileageData([FromForm] string param, [FromForm] string secure, [FromForm] string key = "")
        {
            var iv = (string)Config.Get("encryption_iv");

            using var conn = Db.Get();
            conn.Open();

            var clientReq = new ClientRequest<BaseRequest>(conn, param, secure, key);
            if (clientReq.error != SRStatusCode.Ok)
            {
                return new JsonResult(EncryptedResponse.Generate(iv, clientReq.error));
            }

            MileageDataResponse mileageDataResponse = new();

            MileageMapState mileageMapState = new();

            var sql = Db.GetCommand("SELECT * FROM `sw_mileagemapstates` WHERE user_id = '{0}';", clientReq.userId);
            var stateCmd = new MySqlCommand(sql, conn);
            var stateRdr = stateCmd.ExecuteReader();

            if (stateRdr.HasRows)
            {
                stateRdr.Close();
                var populateStatus = mileageMapState.Populate(conn, clientReq.userId);

                if (populateStatus != SRStatusCode.Ok)
                {
                    // Return error code from Populate() to client
                    return new JsonResult(EncryptedResponse.Generate(iv, new BaseResponse(populateStatus)));
                }
            }
            else
            {
                // No MileageMapState for this player, create one
                stateRdr.Close();

                sql = Db.GetCommand(@"INSERT INTO `sw_mileagemapstates` (
                                            user_id, episode, chapter, point, stage_total_score, chapter_start_time, map_distance, num_boss_attack, stage_distance, stage_max_score
                                        ) VALUES (
                                            '{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}'
                                        );", clientReq.userId, mileageMapState.episode, mileageMapState.chapter, mileageMapState.point, mileageMapState.stageTotalScore, mileageMapState.chapterStartTime, mileageMapState.mapDistance, mileageMapState.numBossAttack, mileageMapState.stageDistance, mileageMapState.stageMaxScore);
                var insertCmd = new MySqlCommand(sql, conn);
                insertCmd.ExecuteNonQuery();
            }
            mileageDataResponse.mileageMapState = mileageMapState;
            return new JsonResult(EncryptedResponse.Generate(iv, mileageDataResponse));
        }

        /// <summary>
        /// Endpoint hit when the Story Mode
        /// mileage screen is opened.
        /// </summary>
        [HttpPost]
        [Route("/Game/getMileageReward/")]
        [Produces("text/json")]
        public JsonResult GetMileageReward([FromForm] string param, [FromForm] string secure, [FromForm] string key = "")
        {
            var iv = (string)Config.Get("encryption_iv");

            using var conn = Db.Get();
            conn.Open();

            var clientReq = new ClientRequest<BaseRequest>(conn, param, secure, key);
            if (clientReq.error != SRStatusCode.Ok)
            {
                return new JsonResult(EncryptedResponse.Generate(iv, clientReq.error));
            }

            MileageRewardResponse mileageRewardResponse = new();
            mileageRewardResponse.mileageMapRewardList = Array.Empty<MileageReward>();

            // FIXME: Stub

            return new JsonResult(EncryptedResponse.Generate(iv, mileageRewardResponse));
        }

        [HttpPost]
        [Route("/Game/getCampaignList/")]
        [Produces("text/json")]
        public JsonResult GetCampaignList([FromForm] string param, [FromForm] string secure, [FromForm] string key = "")
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

            return new JsonResult(EncryptedResponse.Generate(iv, new CampaignListResponse()));
        }

        [HttpPost]
        [Route("/Game/getFreeItemList/")]
        [Produces("text/json")]
        public JsonResult GetFreeItemList([FromForm] string param, [FromForm] string secure, [FromForm] string key = "")
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

            return new JsonResult(EncryptedResponse.Generate(iv, new FreeItemListResponse()));
        }

        /// <summary>
        /// Endpoint hit when beginning a Story Mode run.
        /// </summary>
        [HttpPost]
        [Route("/Game/actStart/")]
        [Produces("text/json")]
        public JsonResult ActStart([FromForm] string param, [FromForm] string secure, [FromForm] string key = "")
        {
            var iv = (string)Config.Get("encryption_iv");
            BaseResponse error = null;

            // FIXME: Actually do something with this information
            using var conn = Db.Get();
            conn.Open();

            var clientReq = new ClientRequest<QuickActStartRequest>(conn, param, secure, key);
            if (clientReq.error != SRStatusCode.Ok)
            {
                return new JsonResult(EncryptedResponse.Generate(iv, clientReq.error));
            }

            ActStartResponse actStartResponse = new();

            // Now that we have the user ID, we can retrieve the player state
            PlayerState playerState = new PlayerState();

            var populateStatus = playerState.Populate(conn, clientReq.userId);

            conn.Close();

            if (populateStatus == SRStatusCode.Ok)
            {
                actStartResponse.playerState = playerState;
                return new JsonResult(EncryptedResponse.Generate(iv, actStartResponse));
            }
            else
            {
                // Return error code from Populate() to client
                return new JsonResult(EncryptedResponse.Generate(iv, new BaseResponse(populateStatus)));
            }
        }

        /// <summary>
        /// Endpoint hit when beginning a Timed Mode run.
        /// </summary>
        [HttpPost]
        [Route("/Game/quickActStart/")]
        [Produces("text/json")]
        public JsonResult QuickActStart([FromForm] string param, [FromForm] string secure, [FromForm] string key = "")
        {
            var iv = (string)Config.Get("encryption_iv");
            BaseResponse error = null;

            // FIXME: Actually do something with this information
            using var conn = Db.Get();
            conn.Open();

            var clientReq = new ClientRequest<QuickActStartRequest>(conn, param, secure, key);
            if (clientReq.error != SRStatusCode.Ok)
            {
                return new JsonResult(EncryptedResponse.Generate(iv, clientReq.error));
            }

            QuickActStartResponse quickActStartResponse = new();

            // Now that we have the user ID, we can retrieve the player state
            PlayerState playerState = new PlayerState();

            var populateStatus = playerState.Populate(conn, clientReq.userId);

            conn.Close();

            if (populateStatus == SRStatusCode.Ok)
            {
                quickActStartResponse.playerState = playerState;
                return new JsonResult(EncryptedResponse.Generate(iv, quickActStartResponse));
            }
            else
            {
                // Return error code from Populate() to client
                return new JsonResult(EncryptedResponse.Generate(iv, new BaseResponse(populateStatus)));
            }
        }

        /// <summary>
        /// Endpoint hit when finishing a Timed Mode run.
        /// </summary>
        [HttpPost]
        [Route("/Game/quickPostGameResults/")]
        [Produces("text/json")]
        public JsonResult QuickPostGameResults([FromForm] string param, [FromForm] string secure, [FromForm] string key = "")
        {
            var iv = (string)Config.Get("encryption_iv");
            BaseResponse error = null;

            using var conn = Db.Get();
            conn.Open();

            var clientReq = new ClientRequest<QuickPostGameResultsRequest>(conn, param, secure, key);
            if (clientReq.error != SRStatusCode.Ok)
            {
                return new JsonResult(EncryptedResponse.Generate(iv, clientReq.error));
            }

            var request = clientReq.request;

            // Now that we have the user ID, we can retrieve the player state
            PlayerState playerState = new PlayerState();

            var populateStatus = playerState.Populate(conn, clientReq.userId);
            if (populateStatus != SRStatusCode.Ok)
            {
                return new JsonResult(EncryptedResponse.Generate(iv, populateStatus));
            }

            if (playerState.quickTotalHighScore < request.score)
            {
                playerState.quickTotalHighScore = request.score;
            }

            playerState.numAnimals += request.numAnimals;
            playerState.numRings += request.numRings;
            playerState.numRedRings += request.numRedStarRings;
            playerState.totalDistance += request.distance;

            var saveStatus = playerState.Save(conn, clientReq.userId);
            if (saveStatus != SRStatusCode.Ok)
            {
                return new JsonResult(EncryptedResponse.Generate(iv, saveStatus));
            }

            conn.Close();

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

        /// <summary>
        /// Endpoint hit when finishing a Story Mode run.
        /// </summary>
        [HttpPost]
        [Route("/Game/postGameResults/")]
        [Produces("text/json")]
        public JsonResult PostGameResults([FromForm] string param, [FromForm] string secure, [FromForm] string key = "")
        {
            var iv = (string)Config.Get("encryption_iv");
            BaseResponse error = null;

            using var conn = Db.Get();
            conn.Open();

            var clientReq = new ClientRequest<PostGameResultsRequest>(conn, param, secure, key);
            if (clientReq.error != SRStatusCode.Ok)
            {
                return new JsonResult(EncryptedResponse.Generate(iv, clientReq.error));
            }

            var request = clientReq.request;

            PostGameResultsResponse postGameResultsResponse = new();

            // If the run wasn't exited out of
            if (request.closed != 1)
            {
                // Now that we have the user ID, we can retrieve the player state
                PlayerState playerState = new();

                var populateStatus = playerState.Populate(conn, clientReq.userId);
                if (populateStatus != SRStatusCode.Ok)
                {
                    return new JsonResult(EncryptedResponse.Generate(iv, populateStatus));
                }

                if (playerState.totalHighScore < request.score)
                {
                    playerState.totalHighScore = request.score;
                }

                playerState.numAnimals += request.numAnimals;
                playerState.numRings += request.numRings;
                playerState.numRedRings += request.numRedStarRings;
                playerState.totalDistance += request.distance;

                MileageMapState mileageMapState = new();

                var populateMMSStatus = mileageMapState.Populate(conn, clientReq.userId);
                if (populateMMSStatus != SRStatusCode.Ok)
                {
                    return new JsonResult(EncryptedResponse.Generate(iv, populateMMSStatus));
                }

                mileageMapState.stageTotalScore += request.score;

                // Despite its misleading name, this is set to 1 whether a chapter OR an episode is cleared.
                if (request.chapterClear == 1)
                {
                    // Chapter or episode cleared, go to the next one
                    mileageMapState.Advance();

                    // Prevent player rank from going over 999
                    if (playerState.numRank < 998)
                    {
                        playerState.numRank++;
                    }
                }
                else mileageMapState.point = request.reachPoint;

                var saveStatus = playerState.Save(conn, clientReq.userId);
                if (saveStatus != SRStatusCode.Ok)
                {
                    return new JsonResult(EncryptedResponse.Generate(iv, saveStatus));
                }

                var saveMMSStatus = mileageMapState.Save(conn, clientReq.userId);
                if (saveMMSStatus != SRStatusCode.Ok)
                {
                    return new JsonResult(EncryptedResponse.Generate(iv, saveMMSStatus));
                }

                conn.Close();

                postGameResultsResponse.playerState = playerState;

                // FIXME: Actually implement this normally lmao

                postGameResultsResponse.dailyChallengeIncentive = new Incentive[0];
                postGameResultsResponse.messageList = new string[0];
                postGameResultsResponse.operatorMessageList = new string[0];
                postGameResultsResponse.totalMessage = 0;
                postGameResultsResponse.totalOperatorMessage = 0;
                postGameResultsResponse.mileageMapState = mileageMapState;
                postGameResultsResponse.mileageIncentiveList = Array.Empty<MileageIncentive>();
                postGameResultsResponse.eventIncentiveList = Array.Empty<Item>();
                postGameResultsResponse.wheelOptions = new WheelOptions();
            }

            return new JsonResult(EncryptedResponse.Generate(iv, postGameResultsResponse));
        }

        /// <summary>
        /// Endpoint hit when a player revives using Red Star Rings.
        /// </summary>
        [HttpPost]
        [Route("/Game/actRetry/")]
        [Produces("text/json")]
        public JsonResult ActRetry([FromForm] string param, [FromForm] string secure, [FromForm] string key = "")
        {
            var iv = (string)Config.Get("encryption_iv");
            BaseResponse error = null;

            using var conn = Db.Get();
            conn.Open();

            var clientReq = new ClientRequest<BaseRequest>(conn, param, secure, key);
            if (clientReq.error != SRStatusCode.Ok)
            {
                return new JsonResult(EncryptedResponse.Generate(iv, clientReq.error));
            }

            // Now that we have the user ID, we can retrieve the player state
            PlayerState playerState = new PlayerState();

            var populateStatus = playerState.Populate(conn, clientReq.userId);
            if (populateStatus != SRStatusCode.Ok)
            {
                return new JsonResult(EncryptedResponse.Generate(iv, populateStatus));
            }

            ulong reviveCost = (ulong)Config.Get("revive_rsr_cost");

            if (playerState.numRedRings >= reviveCost)
            {
                playerState.numRedRings -= reviveCost;

                var saveStatus = playerState.Save(conn, clientReq.userId);
                if (saveStatus != SRStatusCode.Ok)
                {
                    return new JsonResult(EncryptedResponse.Generate(iv, saveStatus));
                }
                conn.Close();
                return new JsonResult(EncryptedResponse.Generate(iv, new BaseResponse()));
            }
            else
            {
                conn.Close();
                return new JsonResult(EncryptedResponse.Generate(iv, new BaseResponse(SRStatusCode.NotEnoughRedStarRings)));
            }
        }
    }
}
