using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using spikewall.Debug;
using spikewall.Encryption;
using spikewall.Object;
using spikewall.Request;
using spikewall.Response;
using System.Reflection.PortableExecutable;
using System.Text;

namespace spikewall.Controllers
{
    [ApiController]
    [Route("Game")]
    public class GameController : ControllerBase
    {
        [HttpPost]
        [Route("getDailyChalData")]
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
        [Route("getCostList")]
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
        [Route("getMileageData")]
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
        [Route("getMileageReward")]
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
        [Route("getCampaignList")]
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
        [Route("getFreeItemList")]
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
        [Route("actStart")]
        [Produces("text/json")]
        public JsonResult ActStart([FromForm] string param, [FromForm] string secure, [FromForm] string key = "")
        {
            var iv = (string)Config.Get("encryption_iv");
            BaseResponse error = null;

            // FIXME: Actually do something with this information
            using var conn = Db.Get();
            conn.Open();

            var clientReq = new ClientRequest<ActStartRequest>(conn, param, secure, key);
            if (clientReq.error != SRStatusCode.Ok)
            {
                return new JsonResult(EncryptedResponse.Generate(iv, clientReq.error));
            }

            // Update equipItemList from """modifire"""
            var sql = Db.GetCommand("UPDATE `sw_players` SET equip_item_list = '{0}' WHERE id = '{1}';",
                                    Db.ConvertIntArrayToDBList(clientReq.request.modifire),
                                    clientReq.userId);
            var command = new MySqlCommand(sql, conn);
            command.ExecuteNonQuery();

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
        [Route("quickActStart")]
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

            // Update equipItemList from """modifire"""
            var sql = Db.GetCommand("UPDATE `sw_players` SET equip_item_list = '{0}' WHERE id = '{1}';",
                                    Db.ConvertIntArrayToDBList(clientReq.request.modifire),
                                    clientReq.userId);
            var command = new MySqlCommand(sql, conn);
            command.ExecuteNonQuery();

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
        [Route("quickPostGameResults")]
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

            QuickPostGameResultsResponse quickPostGameResultsResponse = new();

            // If the run wasn't exited out of
            if (request.closed != 1)
            {
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

                Character[] characterState;
                Character.PopulateCharacterState(conn, clientReq.userId, out characterState);

                bool subCharacterPresent = playerState.subCharaID != -1;

                // FIXME: Unfinished character experience system below

                // Character experience is based on how many rings were collected in the entire run
                var expIncrease = request.numRings + request.numFailureRings;

                // Now we need to find the index of the provided character in the CharacterState
                int mainCharaIndex = Character.FindCharacterInCharacterState(playerState.mainCharaID, characterState);

                if (mainCharaIndex == -1)
                {
                    // The character we want to upgrade isn't available to the player, abort
                    return new JsonResult(EncryptedResponse.Generate(iv, SRStatusCode.InternalServerError));
                }

                sbyte charactersInRun = 1;
                int subCharaIndex = -1;

                if (subCharacterPresent)
                {
                    subCharaIndex = Character.FindCharacterInCharacterState(playerState.mainCharaID, characterState);

                    if (subCharaIndex == -1)
                    {
                        // The character we want us to upgrade isn't available to the player, abort
                        return new JsonResult(EncryptedResponse.Generate(iv, SRStatusCode.InternalServerError));
                    }
                    charactersInRun = 2;

                    // We'll level up the sub character now instead
                    // of later to avoid needing to branch again
                    if (characterState[subCharaIndex].level < 100)
                    {
                        characterState[subCharaIndex].exp += expIncrease;
                        if (characterState[subCharaIndex].exp >= characterState[subCharaIndex].numRings)
                        {
                            var expOverflow = characterState[subCharaIndex].exp - characterState[subCharaIndex].numRings;
                            characterState[subCharaIndex].level++;
                            characterState[subCharaIndex].exp += expOverflow;

                            // FIXME: More has to be done here!!!
                        }
                    }
                }

                if (characterState[mainCharaIndex].level < 100)
                {
                    characterState[mainCharaIndex].exp += expIncrease;
                    if (characterState[mainCharaIndex].exp >= characterState[mainCharaIndex].numRings)
                    {
                        var expOverflow = characterState[mainCharaIndex].exp - characterState[mainCharaIndex].numRings;
                        characterState[mainCharaIndex].level++;
                        characterState[mainCharaIndex].exp += expOverflow;

                        // FIXME: More has to be done here!!!
                    }
                }

                Character[] playCharacterState = new Character[charactersInRun];

                if (charactersInRun > 0)
                {
                    playCharacterState[0] = characterState[mainCharaIndex];
                    for (int i = 1; i < charactersInRun; i++)
                    {
                        playCharacterState[i] = characterState[subCharaIndex];
                    }
                }

                conn.Open();

                var playerSaveStatus = playerState.Save(conn, clientReq.userId);
                if (playerSaveStatus != SRStatusCode.Ok)
                {
                    return new JsonResult(EncryptedResponse.Generate(iv, playerSaveStatus));
                }

                var charSaveStatus = Character.SaveCharacterState(conn, clientReq.userId, characterState);
                if (charSaveStatus != SRStatusCode.Ok)
                {
                    return new JsonResult(EncryptedResponse.Generate(iv, charSaveStatus));
                }

                conn.Close();

                quickPostGameResultsResponse.playerState = playerState;
                quickPostGameResultsResponse.characterState = characterState;
                quickPostGameResultsResponse.playCharacterState = playCharacterState;

                // FIXME: Actually implement this normally lmao

                quickPostGameResultsResponse.dailyChallengeIncentive = new Incentive[0];
                quickPostGameResultsResponse.messageList = new string[0];
                quickPostGameResultsResponse.operatorMessageList = new string[0];
                quickPostGameResultsResponse.totalMessage = 0;
                quickPostGameResultsResponse.totalOperatorMessage = 0;
            }
            return new JsonResult(EncryptedResponse.Generate(iv, quickPostGameResultsResponse));
        }

        /// <summary>
        /// Endpoint hit when finishing a Story Mode run.
        /// </summary>
        [HttpPost]
        [Route("postGameResults")]
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
        [Route("actRetry")]
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
