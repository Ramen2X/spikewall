using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using spikewall.Debug;
using spikewall.Encryption;
using spikewall.Object;
using spikewall.Request;
using spikewall.Response;
using System.Reflection.PortableExecutable;
using System.Text;
using static spikewall.Object.Character;
using static spikewall.Object.Item;

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

        private ConsumedItem[] GetCostListData(MySqlConnection conn)
        {
            var sql = Db.GetCommand("SELECT * FROM `sw_costlist`");
            var command = new MySqlCommand(sql, conn);
            var reader = command.ExecuteReader();

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

                return consumedItems;
            }

            reader.Close();

            return Array.Empty<ConsumedItem>();
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

            CostListResponse costListResponse = new();

            costListResponse.consumedCostList = GetCostListData(conn);

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

            // Get player's MileageMapState
            MileageMapState mileageMapState = new();
            var populateStatus = mileageMapState.Populate(conn, clientReq.userId);

            if (populateStatus != SRStatusCode.Ok)
            {
                // Return error code from Populate() to client
                return new JsonResult(EncryptedResponse.Generate(iv, new BaseResponse(populateStatus)));
            }

            MileageRewardResponse mileageRewardResponse = new();

            var sql = Db.GetCommand("SELECT *, (SELECT COUNT(*) FROM `sw_incentives`) AS row_count FROM `sw_incentives` WHERE episode = '{0}' AND chapter = '{1}'", mileageMapState.episode, mileageMapState.chapter);
            var command = new MySqlCommand(sql, conn);
            var reader = command.ExecuteReader();

            if (reader.Read())
            {
                var count = reader.GetInt32("row_count");

                MileageReward[] mileageMapRewardList = new MileageReward[count];

                for (int i = 0; i < count; i++)
                {
                    mileageMapRewardList[i] = new MileageReward();
                    mileageMapRewardList[i].type = reader.GetSByte("type");
                    mileageMapRewardList[i].itemId = reader.GetInt64("item_id");
                    mileageMapRewardList[i].numItem = reader.GetUInt64("num_item");
                    mileageMapRewardList[i].point = reader.GetSByte("point");

                    switch (reader.GetSByte("limit_time"))
                    {
                        // No time limit on this incentive
                        case 0:
                            mileageMapRewardList[i].limitTime = -1;
                            break;
                        // Time limit of 12 hours on this incentive
                        case 1:
                            mileageMapRewardList[i].limitTime = mileageMapState.chapterStartTime + 43200;
                            break;
                        // Time limit of 24 hours on this incentive
                        case 2:
                            mileageMapRewardList[i].limitTime = mileageMapState.chapterStartTime + 86400;
                            break;
                    }

                    reader.Read();
                }
                mileageRewardResponse.mileageMapRewardList = mileageMapRewardList;
            }
            else mileageRewardResponse.mileageMapRewardList = Array.Empty<MileageReward>();

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
        /// Endpoint hit when beginning a Story Mode run (actStart) or Timed Mode run (quickActStart).
        /// </summary>
        [HttpPost]
        [Route("quickActStart")]
        [Route("actStart")]
        [Produces("text/json")]
        public JsonResult ActStart([FromForm] string param, [FromForm] string secure, [FromForm] string key = "")
        {
            var iv = (string)Config.Get("encryption_iv");
            BaseResponse error = null;

            using var conn = Db.Get();
            conn.Open();

            var clientReq = new ClientRequest<ActStartRequest>(conn, param, secure, key);
            if (clientReq.error != SRStatusCode.Ok)
            {
                return new JsonResult(EncryptedResponse.Generate(iv, clientReq.error));
            }

            // NOTE: If this is `actStart` over `quickActStart`, the request will have `distanceFriendList`.
            //       Otherwise it will be null.
            //       Likewise, response will have `distanceFriendList` in `actStart`, otherwise null.

            // Process items (aka "modifires")
            {
                string sql;
                MySqlCommand command;

                for (int i = 0; i < clientReq.request.modifire.Length; i++)
                {
                    long item = clientReq.request.modifire[i];

                    sql = Db.GetCommand("DELETE FROM `sw_itemownership` WHERE user_id = '{0}' AND item_id = '{1}' LIMIT 1;", clientReq.userId, item);
                    command = new MySqlCommand(sql, conn);
                    var rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        // The user had this item and we've just removed one of them. No further action required.
                    }
                    else
                    {
                        // User doesn't have this item, decrement cost from ring count

                        // Determine cost of item
                        ConsumedItem[] costs = GetCostListData(conn);
                        long cost = 0;
                        foreach (ConsumedItem itemData in costs)
                        {
                            if (itemData.consumedItemId == item)
                            {
                                cost = (long)itemData.numItem;
                                break;
                            }
                        }

                        // Decrement cost from ring count
                        sql = Db.GetCommand("UPDATE `sw_players` SET num_rings = num_rings - '{0}' WHERE id = '{1}';", cost, clientReq.userId);
                        command = new MySqlCommand(sql, conn);
                        command.ExecuteNonQuery();
                    }
                }

                // Update equipItemList
                sql = Db.GetCommand("UPDATE `sw_players` SET equip_item_list = '{0}' WHERE id = '{1}';",
                                    Db.ConvertIntArrayToDBList(clientReq.request.modifire),
                                    clientReq.userId);
                command = new MySqlCommand(sql, conn);
                command.ExecuteNonQuery();
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

                bool subCharacterPresent = false;

                for (int i = 0; i < playerState.equipItemList.Length; i++)
                {
                    // Detect if the player equipped a sub character for this
                    // run so we know if we need to level it up after the run
                    if (playerState.equipItemList[i] == (long)Item.ItemID.SubCharacter)
                    {
                        subCharacterPresent = true;
                        break;
                    }
                }

                // Character experience is based on how many rings were collected in the entire run
                var exp = request.numRings + request.numFailureRings;

                sbyte charactersInRun = 1;

                int mainCharaIndex = -1;
                int subCharaIndex = -1;

                if (subCharacterPresent)
                {
                    charactersInRun = 2;
                    var subLevelUpStatus = Character.LevelUpCharacterWithExp(conn, playerState.subCharaID, exp, ref characterState, out subCharaIndex);

                    if (subLevelUpStatus != SRStatusCode.Ok)
                    {
                        return new JsonResult(EncryptedResponse.Generate(iv, subLevelUpStatus));
                    }
                }

                var mainLevelUpStatus = Character.LevelUpCharacterWithExp(conn, playerState.mainCharaID, exp, ref characterState, out mainCharaIndex);

                if (mainLevelUpStatus != SRStatusCode.Ok)
                {
                    return new JsonResult(EncryptedResponse.Generate(iv, mainLevelUpStatus));
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
                quickPostGameResultsResponse.playCharacterState = playCharacterState;
            }

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

            MileageMapState mileageMapState = new();

            var populateMMSStatus = mileageMapState.Populate(conn, clientReq.userId);
            if (populateMMSStatus != SRStatusCode.Ok)
            {
                return new JsonResult(EncryptedResponse.Generate(iv, populateMMSStatus));
            }

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

                Character[] characterState;
                Character.PopulateCharacterState(conn, clientReq.userId, out characterState);

                bool subCharacterPresent = false;

                for (int i = 0; i < playerState.equipItemList.Length; i++)
                {
                    // Detect if the player equipped a sub character for this
                    // run so we know if we need to level it up after the run
                    if (playerState.equipItemList[i] == (long)Item.ItemID.SubCharacter)
                    {
                        subCharacterPresent = true;
                        break;
                    }
                }

                // Character experience is based on how many rings were collected in the entire run
                var exp = request.numRings + request.numFailureRings;

                sbyte charactersInRun = 1;

                int mainCharaIndex = -1;
                int subCharaIndex = -1;

                if (subCharacterPresent)
                {
                    charactersInRun = 2;
                    var subLevelUpStatus = Character.LevelUpCharacterWithExp(conn, playerState.subCharaID, exp, ref characterState, out subCharaIndex);

                    if (subLevelUpStatus != SRStatusCode.Ok)
                    {
                        return new JsonResult(EncryptedResponse.Generate(iv, subLevelUpStatus));
                    }
                }

                var mainLevelUpStatus = Character.LevelUpCharacterWithExp(conn, playerState.mainCharaID, exp, ref characterState, out mainCharaIndex);

                if (mainLevelUpStatus != SRStatusCode.Ok)
                {
                    return new JsonResult(EncryptedResponse.Generate(iv, mainLevelUpStatus));
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

                var previousPoint = mileageMapState.point;
                var previousChapter = mileageMapState.chapter;
                var previousEpisode = mileageMapState.episode;

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

                conn.Open();

                postGameResultsResponse.mileageIncentiveList = Array.Empty<MileageIncentive>();

                // Award the player any incentives that they hit

                string sql;

                if (request.bossDestroyed == 0)
                {
                    if (mileageMapState.point == 5)
                        // Boss point, prevent awarding incentives until boss is defeated
                        sql = Db.GetCommand("SELECT *, (SELECT COUNT(*) FROM `sw_incentives` WHERE episode = '{0}' AND chapter = '{1}' AND point <= 4) AS row_count FROM `sw_incentives` WHERE episode = '{0}' AND chapter = '{1}' AND point <= 4", mileageMapState.episode, mileageMapState.chapter);

                    else if (previousPoint >= mileageMapState.point && (previousChapter != mileageMapState.chapter || previousEpisode != mileageMapState.episode))
                        // Chapter with no boss point completed, get incentives from previous chapter/episode
                        sql = Db.GetCommand("SELECT *, (SELECT COUNT(*) FROM `sw_incentives` WHERE episode = '{0}' AND chapter = '{1}' AND point >= '{2}') AS row_count FROM `sw_incentives` WHERE episode = '{0}' AND chapter = '{1}' AND point >= '{2}'", previousEpisode, previousChapter, previousPoint);

                    else if (previousPoint == mileageMapState.point)
                        // The player hasn't moved; no incentives to award
                        goto endOfIncentiveCode;

                    // Boss has not been defeated, get all possible incentives up to this point
                    else sql = Db.GetCommand("SELECT *, (SELECT COUNT(*) FROM `sw_incentives` WHERE episode = '{0}' AND chapter = '{1}' AND point >= '{2}' AND point <= '{3}') AS row_count FROM `sw_incentives` WHERE episode = '{0}' AND chapter = '{1}' AND point >= '{2}' AND point <= '{3}'", mileageMapState.episode, mileageMapState.chapter, previousPoint, request.reachPoint);
                }
                // Boss was just defeated, get incentives from previous chapter/episode
                else sql = Db.GetCommand("SELECT *, (SELECT COUNT(*) FROM `sw_incentives` WHERE episode = '{0}' AND chapter = '{1}' AND point = 5) AS row_count FROM `sw_incentives` WHERE episode = '{0}' AND chapter = '{1}' AND point = 5", previousEpisode, previousChapter);
                    
                var command = new MySqlCommand(sql, conn);
                var reader = command.ExecuteReader();

                if (reader.Read())
                {
                    var count = reader.GetInt32("row_count");
                    bool incentiveIsValid = false;

                    List<MileageIncentive> mileageIncentiveList = new();

                    for (int i = 0; i < count; i++)
                    {
                        incentiveIsValid = false;

                        switch (reader.GetSByte("limit_time"))
                        {
                            // No time limit on this incentive, always give it to the player
                            case 0:
                                incentiveIsValid = true;
                                break;
                            // Time limit of 12 hours on this incentive, check if still valid
                            case 1:
                                if ((mileageMapState.chapterStartTime + 43200) > DateTimeOffset.Now.ToUnixTimeSeconds())
                                    incentiveIsValid = true;
                                break;
                            // Time limit of 24 hours on this incentive, check if still valid
                            case 2:
                                if ((mileageMapState.chapterStartTime + 86400) > DateTimeOffset.Now.ToUnixTimeSeconds())
                                    incentiveIsValid = true;
                                break;
                        }

                        if (incentiveIsValid)
                        {
                            // Append valid incentives to list so we can return it to
                            // the client and use it to add items to player's account
                            MileageIncentive incentive = new()
                            {
                                itemId = reader.GetInt64("item_id"),
                                numItem = reader.GetUInt64("num_item"),
                                type = reader.GetSByte("type"),
                                pointId = reader.GetSByte("point")
                            };

                            mileageIncentiveList.Add(incentive);
                        }
                        reader.Read();
                    }
                    reader.Close();

                    for (int i = 0; i < mileageIncentiveList.Count; i++)
                    {
                        long itemID = mileageIncentiveList[i].itemId;
                        ulong itemCount = mileageIncentiveList[i].numItem;

                        // Only add valid incentives to the item list (120000 - 120007)
                        if (itemID > (long)ItemID.SubCharacter && itemID < (long)ItemID.RingBonus)
                        {
                            for (ulong i2 = 0; i2 < itemCount; i2++)
                            {
                                var itemSQL = Db.GetCommand(@"INSERT INTO `sw_itemownership` (
                                                                user_id, item_id
                                                            ) VALUES (
                                                                '{0}', '{1}'
                                                            );", clientReq.userId, itemID);
                                var insertCmd = new MySqlCommand(itemSQL, conn);
                                insertCmd.ExecuteNonQuery();
                            }
                        }
                        else if (itemID == (long)ItemID.RedStarRing)
                            playerState.numRedRings += itemCount;

                        else if (itemID == (long)ItemID.Ring)
                            playerState.numRings += itemCount;

                        // This incentive is a character, unlock it
                        // FIXME: Although you never get multiple characters as incentives in
                        // the original game, it probably wouldn't hurt to handle itemCount here
                        else if (itemID >= 300000 && itemID <= 399999)
                        {
                            var incentiveIndex = FindCharacterInCharacterState((int)itemID, characterState);

                            if (incentiveIndex == -1)
                                // The character to be awarded doesn't exist in the CharacterState, abort
                                return new JsonResult(EncryptedResponse.Generate(iv, SRStatusCode.InternalServerError));

                            if (characterState[incentiveIndex].status == 0)
                                characterState[incentiveIndex].status = 1;
                            else
                            {
                                if (characterState[incentiveIndex].star < characterState[incentiveIndex].starMax)
                                    characterState[incentiveIndex].star++;
                            }
                        }
                    }
                    postGameResultsResponse.mileageIncentiveList = mileageIncentiveList.ToArray();
                }
                reader.Close();

                endOfIncentiveCode:

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

                    var charSaveStatus = Character.SaveCharacterState(conn, clientReq.userId, characterState);
                    if (charSaveStatus != SRStatusCode.Ok)
                    {
                        return new JsonResult(EncryptedResponse.Generate(iv, charSaveStatus));
                    }

                    conn.Close();

                    postGameResultsResponse.playerState = playerState;
                    postGameResultsResponse.playCharacterState = playCharacterState;
                }

                // FIXME: Actually implement this normally lmao

                postGameResultsResponse.dailyChallengeIncentive = new Incentive[0];
                postGameResultsResponse.messageList = new string[0];
                postGameResultsResponse.operatorMessageList = new string[0];
                postGameResultsResponse.totalMessage = 0;
                postGameResultsResponse.totalOperatorMessage = 0;
                postGameResultsResponse.mileageMapState = mileageMapState;
                postGameResultsResponse.eventIncentiveList = Array.Empty<Item>();
                postGameResultsResponse.wheelOptions = new WheelOptions();

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
