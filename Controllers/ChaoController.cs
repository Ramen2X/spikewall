using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using spikewall.Object;
using spikewall.Request;
using spikewall.Response;
using System.Security.Cryptography;
using static spikewall.Object.Chao;
using static spikewall.Object.ChaoBase;
using static spikewall.Object.Character;

namespace spikewall.Controllers
{
    [ApiController]
    [Route("Chao")]
    public class ChaoController : ControllerBase
    {
        [HttpPost]
        [Route("getChaoWheelOptions")]
        [Produces("text/json")]
        public JsonResult GetChaoWheelOptions([FromForm] string param, [FromForm] string secure, [FromForm] string key = "")
        {
            var iv = (string)Config.Get("encryption_iv");

            using var conn = Db.Get();
            conn.Open();

            var clientReq = new ClientRequest<RedstarExchangeListRequest>(conn, param, secure, key);
            if (clientReq.error != SRStatusCode.Ok) {
                return new JsonResult(EncryptedResponse.Generate(iv, clientReq.error));
            }

            PlayerState playerState = new();

            var populateStatus = playerState.Populate(conn, clientReq.userId);
            if (populateStatus != SRStatusCode.Ok)
            {
                return new JsonResult(EncryptedResponse.Generate(iv, populateStatus));
            }

            var populateChaoState = PopulateChaoState(conn, clientReq.userId, out Chao[] chaoState);
            if (populateChaoState != SRStatusCode.Ok)
            {
                return new JsonResult(EncryptedResponse.Generate(iv, populateChaoState));
            }
            var populateCharacterState = PopulateCharacterState(conn, clientReq.userId, out Character[] characterState);
            if (populateCharacterState != SRStatusCode.Ok)
            {
                return new JsonResult(EncryptedResponse.Generate(iv, populateChaoState));
            }

            ChaoWheelOptions chaoWheelOptions = new();
            chaoWheelOptions.PopulateChaoWheel(conn, clientReq.userId, ref chaoState, ref characterState);

            ChaoWheelOptionsResponse chaoWheelOptionsResponse = new()
            {
                chaoWheelOptions = chaoWheelOptions
            };

            return new JsonResult(EncryptedResponse.Generate(iv, chaoWheelOptionsResponse));
        }

        [HttpPost]
        [Route("getPrizeChaoWheelSpin")]
        [Produces("text/json")]
        public JsonResult GetPrizeChaoWheelOptions([FromForm] string param, [FromForm] string secure, [FromForm] string key = "")
        {
            var iv = (string)Config.Get("encryption_iv");

            using var conn = Db.Get();
            conn.Open();

            var clientReq = new ClientRequest<RedstarExchangeListRequest>(conn, param, secure, key);
            if (clientReq.error != SRStatusCode.Ok) {
                return new JsonResult(EncryptedResponse.Generate(iv, clientReq.error));
            }

            PlayerState playerState = new();

            var populateStatus = playerState.Populate(conn, clientReq.userId);
            if (populateStatus != SRStatusCode.Ok)
            {
                return new JsonResult(EncryptedResponse.Generate(iv, populateStatus));
            }

            ChaoWheelOptions.GetPrizeChaoWheelOptions(conn, out Chao[] rareChaoPrizeList, out Chao[] superRareChaoPrizeList, out Character[] characterPrizeList);

            var prizeList = new Chao[rareChaoPrizeList.Length + superRareChaoPrizeList.Length + characterPrizeList.Length];

            PrizeChaoWheelSpinResponse chaoWheelSpinResponse = new(prizeList)
            {
                prizeList = prizeList
            };

            return new JsonResult(EncryptedResponse.Generate(iv, chaoWheelSpinResponse));
        }

        [HttpPost]
        [Route("commitChaoWheelSpin")]
        [Produces("text/json")]
        public JsonResult CommitChaoWheelSpin([FromForm] string param, [FromForm] string secure, [FromForm] string key = "")
        {
            var iv = (string)Config.Get("encryption_iv");

            using var conn = Db.Get();
            conn.Open();

            var clientReq = new ClientRequest<RedstarExchangeListRequest>(conn, param, secure, key);
            if (clientReq.error != SRStatusCode.Ok)
            {
                return new JsonResult(EncryptedResponse.Generate(iv, clientReq.error));
            }

            PlayerState playerState = new();

            var populateStatus = playerState.Populate(conn, clientReq.userId);
            if (populateStatus != SRStatusCode.Ok)
            {
                return new JsonResult(EncryptedResponse.Generate(iv, populateStatus));
            }

            var populateChaoState = PopulateChaoState(conn, clientReq.userId, out Chao[] chaoState);
            if (populateChaoState != SRStatusCode.Ok)
            {
                return new JsonResult(EncryptedResponse.Generate(iv, populateChaoState));
            }
            var populateCharacterState = PopulateCharacterState(conn, clientReq.userId, out Character[] characterState);
            if (populateCharacterState != SRStatusCode.Ok)
            {
                return new JsonResult(EncryptedResponse.Generate(iv, populateChaoState));
            }

            ChaoWheelOptions chaoWheelOptions = new();
            chaoWheelOptions.PopulateChaoWheel(conn, clientReq.userId, ref chaoState, ref characterState);
            chaoWheelOptions.chaoRouletteType = (long)ChaoWheelOptions.ChaoRouletteType.Normal;

            ChaoSpinResult chaoSpinResult = new();

            CommitChaoWheelSpinRequest commitChaoWheelSpinRequest = new();
            var requestCount = commitChaoWheelSpinRequest.count;

            if (playerState.chaoEggs < 10)
            {
                if (playerState.numChaoRouletteTicket > 0)
                {
                    playerState.numChaoRouletteTicket -= 1 * requestCount;
                }
                else
                {
                    playerState.numRedRings -= 50 * (ulong)requestCount;
                }
            }
            else
            {
                chaoWheelOptions.chaoRouletteType = (long)ChaoWheelOptions.ChaoRouletteType.Special;
                requestCount = 1;
                playerState.chaoEggs -= 10;
            }
            List<ChaoPrize> chaoPrizeList = new List<ChaoPrize>();
            List<Character> characterPrizeList = new List<Character>();
            List<Item> itemList = new List<Item>();

            for (int i = 0; i < requestCount; i++)
            {
                var wonChaoIndex = chaoSpinResult.ItemWon;
                var wonRarityID = (ulong)chaoWheelOptions.rarity[wonChaoIndex];

                var sql = Db.GetCommand(@"SELECT * FROM `sw_chaorouletteprizelist`");
                var command = new MySqlCommand(sql, conn);

                var prizeRdr = command.ExecuteReader();

                while (prizeRdr.Read())
                {   
                    switch (wonRarityID)
                    {
                        case (ulong)Item.ItemID.RareEgg:
                        case (ulong)Item.ItemID.SuperRareEgg:
                            var getChaoPrizeSql = Db.GetCommand(@"SELECT * FROM `sw_chaorouletteprizelist` WHERE rarity = '{0}' ORDER BY (RAND() * chao_weight) DESC LIMIT 1", wonRarityID);
                            var getChaoPrizeCommand = new MySqlCommand(getChaoPrizeSql, conn);
                            var chaoPrizeRdr = getChaoPrizeCommand.ExecuteReader();
                            if (chaoPrizeRdr.HasRows)
                            {
                                ChaoPrize chao = new();
                                chao.chaoID = Convert.ToString(chaoPrizeRdr["chao_id"]);
                                var getChaoIndex = FindChaoInChaoState(Convert.ToInt32(chao.chaoID), chaoState);
                                LevelUpChao(conn, Convert.ToInt32(chao.chaoID), ref chaoState, out getChaoIndex);
                                if (chaoState[getChaoIndex].status == (sbyte)Chao.Status.MaxLevel)
                                {
                                    playerState.chaoEggs += 1;
                                }

                                AddChaoToChaoState(conn, Convert.ToInt32(chao.chaoID), ref chaoState, clientReq.userId, ref getChaoIndex);
                                SaveChaoState(conn, clientReq.userId, chaoState);
                                chaoPrizeList.Add(chao);
                                chaoPrizeRdr.Close();
                            }
                            break;
                        case (ulong)Item.ItemID.CharacterEgg:
                            var characterPrizeSql = Db.GetCommand(@"SELECT * FROM `sw_chaorouletteprizelist` WHERE rarity = '{0}' ORDER BY (RAND() * chao_weight) DESC LIMIT 1", wonRarityID);
                            var characterPrizeCommand = new MySqlCommand(characterPrizeSql, conn);
                            var characterPrizeRdr = characterPrizeCommand.ExecuteReader();
                            if (characterPrizeRdr.HasRows)
                            {
                                Character character = new();
                                character.characterId = Convert.ToInt32(characterPrizeRdr["chao_id"]);
                                var getCharacterIndex = FindCharacterInCharacterState(character.characterId, characterState);
                                if (characterState[getCharacterIndex].status == (sbyte)Character.Status.Locked || characterState[getCharacterIndex].star < 10)
                                {
                                    IncreaseCharacterStarThroughRoulette(conn, character.characterId, ref characterState, out getCharacterIndex);
                                }
                                else
                                {
                                    playerState.numRedRings += 50;
                                    playerState.numRings += 10_000;
                                    playerState.chaoEggs += 1;
                                }
                                AddCharacterToCharacterState(conn, character.characterId, ref characterState, clientReq.userId, ref getCharacterIndex);
                                SaveCharacterState(conn, clientReq.userId, characterState);
                                characterPrizeList.Add(character);
                                characterPrizeRdr.Close();
                            }
                            break;
                    }
                }
                prizeRdr.Close();
            }
            ChaoPrize[] chaoPrizeArray = chaoPrizeList.ToArray();
            Character[] characterPrizeArray = characterPrizeList.ToArray();

            // Regenerate chao roulette and chao weights so the client's chao roulette and weights
            // doesn't become desynced from the current premium roulette rank
            var getChaoWheelOptionsStatus = ChaoWheelOptions.GetChaoWheelOptions(conn, chaoWheelOptions.chaoRouletteType, out long[] chaoRarity, out short[] chaoWeight);
            if (getChaoWheelOptionsStatus != SRStatusCode.Ok)
            {
                return new JsonResult(EncryptedResponse.Generate(iv, clientReq.error));
            }

            var getChaoWeightsStatus = ChaoWheelOptions.AdjustChaoWeights(conn, ref chaoRarity, ref chaoWeight, ref chaoState, ref characterState);

            if (getChaoWeightsStatus != SRStatusCode.Ok)
            {
                return new JsonResult(EncryptedResponse.Generate(iv, clientReq.error));
            }

            chaoWheelOptions.rarity = chaoRarity;
            chaoWheelOptions.itemWeight = chaoWeight;

            var savePlayerStatus = playerState.Save(conn, clientReq.userId);
            if (savePlayerStatus != SRStatusCode.Ok)
            {
                return new JsonResult(EncryptedResponse.Generate(iv, savePlayerStatus));
            }

            var saveChaoWheelStatus = chaoWheelOptions.Save(conn, clientReq.userId);
            if (saveChaoWheelStatus != SRStatusCode.Ok)
            {
                return new JsonResult(EncryptedResponse.Generate(iv, saveChaoWheelStatus));
            }

            ChaoWheelSpinResponse chaoWheelSpinResponse = new()
            {
                playerState = playerState,

                characterState = characterState,

                chaoState = chaoState,

                ChaoWheelOptions = chaoWheelOptions,
            };
            return new JsonResult(EncryptedResponse.Generate(iv, chaoWheelSpinResponse));
        }

        [HttpPost]
        [Route("equipChao")]
        [Produces("text/json")]
        public JsonResult EquipChao([FromForm] string param, [FromForm] string secure, [FromForm] string key = "")
        {
            var iv = (string)Config.Get("encryption_iv");

            using var conn = Db.Get();
            conn.Open();

            var clientReq = new ClientRequest<RedstarExchangeListRequest>(conn, param, secure, key);
            if (clientReq.error != SRStatusCode.Ok)
            {
                return new JsonResult(EncryptedResponse.Generate(iv, clientReq.error));
            }

            PlayerState playerState = new();

            var populateStatus = playerState.Populate(conn, clientReq.userId);
            if (populateStatus != SRStatusCode.Ok)
            {
                return new JsonResult(EncryptedResponse.Generate(iv, populateStatus));
            }

            var populateChaoState = PopulateChaoState(conn, clientReq.userId, out Chao[] chaoState);
            if (populateChaoState != SRStatusCode.Ok)
            {
                return new JsonResult(EncryptedResponse.Generate(iv, populateChaoState));
            }

            EquipChaoRequest equipChaoRequest = new();

            if (equipChaoRequest.mainChaoId == -1 && equipChaoRequest.subChaoId == playerState.mainChaoID)
            {
                playerState.mainChaoID = playerState.subChaoID;
                playerState.subChaoID = equipChaoRequest.subChaoId;
            }

            if (equipChaoRequest.mainChaoId == playerState.subChaoID && equipChaoRequest.subChaoId == -1)
            {
                playerState.subChaoID = playerState.mainChaoID;
                playerState.mainChaoID = equipChaoRequest.mainChaoId;
            }

            var getChaoStatesSql = Db.GetCommand(@"SELECT * FROM `sw_chaostates`");
            var getChaoStatesCmd = new MySqlCommand(getChaoStatesSql, conn);
            var getChaoStatesRdr = getChaoStatesCmd.ExecuteReader();

            if (equipChaoRequest.mainChaoId == -1)
            {
                // The chao we want to equip isn't available to the player, abort
                return new JsonResult(EncryptedResponse.Generate(iv, clientReq.error));
            }

            while (getChaoStatesRdr.Read())
            {
                Chao chao = new();
                chao.chaoID = Convert.ToString(getChaoStatesRdr["chao_id"]);
                var getChaoIndex = FindChaoInChaoState(Convert.ToInt32(chao.chaoID), chaoState);
                if (getChaoIndex == -1)
                {
                    // The chao we want to equip isn't available to the player, abort
                    return new JsonResult(EncryptedResponse.Generate(iv, clientReq.error));
                }
                if (chaoState[getChaoIndex].acquired != 0 && chaoState[getChaoIndex].status != (sbyte)Chao.Status.NotOwned)
                {
                    playerState.mainChaoID = equipChaoRequest.mainChaoId;
                }
                SaveChaoState(conn, clientReq.userId, chaoState);
                getChaoStatesRdr.Close();
            }

            if (equipChaoRequest.subChaoId == -1)
            {
                // The chao we want to equip isn't available to the player, abort
                return new JsonResult(EncryptedResponse.Generate(iv, clientReq.error));
            }
            while (getChaoStatesRdr.Read())
            {
                Chao chao = new();
                chao.chaoID = Convert.ToString(getChaoStatesRdr["chao_id"]);
                var getChaoIndex = FindChaoInChaoState(Convert.ToInt32(chao.chaoID), chaoState);
                if (getChaoIndex == -1)
                {
                    // The chao we want to equip isn't available to the player, abort
                    return new JsonResult(EncryptedResponse.Generate(iv, clientReq.error));
                }
                if (chaoState[getChaoIndex].acquired != 0 && chaoState[getChaoIndex].status != (sbyte)Chao.Status.NotOwned)
                {
                    playerState.subChaoID = equipChaoRequest.subChaoId;
                }
                SaveChaoState(conn, clientReq.userId, chaoState);
                getChaoStatesRdr.Close();
            }

            EquipChaoResponse equipChaoResponse = new()
            {
                playerState = playerState
            };

            return new JsonResult(EncryptedResponse.Generate(iv, equipChaoResponse));
        }
    }
}
