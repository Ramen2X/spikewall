using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using spikewall.Debug;
using spikewall.Object;
using spikewall.Request;
using spikewall.Response;

namespace spikewall.Controllers
{
    [ApiController]
    [Route("Character")]
    public class CharacterController : ControllerBase
    {
        [HttpPost]
        [Route("upgradeCharacter")]
        [Produces("text/json")]
        public JsonResult UpgradeCharacter([FromForm] string param, [FromForm] string secure, [FromForm] string key = "")
        {
            var iv = (string)Config.Get("encryption_iv");

            using var conn = Db.Get();
            conn.Open();

            var clientReq = new ClientRequest<UpgradeCharacterRequest>(conn, param, secure, key);
            if (clientReq.error != SRStatusCode.Ok)
            {
                return new JsonResult(EncryptedResponse.Generate(iv, clientReq.error));
            }

            CharacterResponse upgradeCharacterResponse = new();

            var characterID = clientReq.request.characterId;
            var abilityID = clientReq.request.abilityId;

            Character[] characterState;

            // Get the player's CharacterState from the db
            var populateStatus = Character.PopulateCharacterState(conn, clientReq.userId, out characterState);

            if (populateStatus != SRStatusCode.Ok)
            {
                // Return error code from PopulateCharacterState() to client
                return new JsonResult(EncryptedResponse.Generate(iv, new BaseResponse(populateStatus)));
            }

            // Now we need to find the index of the provided character in the CharacterState
            var index = Character.FindCharacterInCharacterState(characterID, characterState);

            if (index == -1)
            {
                // The character the client wants us to upgrade isn't available to the player, abort
                return new JsonResult(EncryptedResponse.Generate(iv, SRStatusCode.InternalServerError));
            }


            ulong ringCost = characterState[index].numRings - characterState[index].exp;

            conn.Open();

            if (characterState[index].level < 100)
            {
                // Honestly unsure of the significance of 120000 here, but it works
                var abilityIndex = abilityID - 120000;

                characterState[index].level++;

                // If we just reached level 100, set character status to max level
                if (characterState[index].level == 100)
                {
                    characterState[index].status = (sbyte)Character.Status.MaxLevel;
                }

                characterState[index].abilityLevel[abilityIndex]++;
                characterState[index].exp = 0;

                // Recalculate the multiple
                var sql = Db.GetCommand("SELECT multiple FROM `sw_characterupgrades` WHERE character_id = '{0}' AND min_level <= '{1}' AND max_level >= '{1}';", characterID, characterState[index].level);
                var upgrdCmd = new MySqlCommand(sql, conn);

                var multiple = Convert.ToUInt64(upgrdCmd.ExecuteScalar());
                characterState[index].numRings += multiple;

                PlayerState playerState = new();
                playerState.Populate(conn, clientReq.userId);

                if (playerState.numRings < ringCost)
                {
                    return new JsonResult(EncryptedResponse.Generate(iv, SRStatusCode.NotEnoughRings));
                }
                else playerState.numRings -= ringCost;

                upgradeCharacterResponse.playerState = playerState;
                upgradeCharacterResponse.characterState = characterState;

                playerState.Save(conn, clientReq.userId);
                Character.SaveCharacterState(conn, clientReq.userId, characterState);
            }
            // Character cannot be leveled up higher
            else return new JsonResult(EncryptedResponse.Generate(iv, SRStatusCode.CharacterLevelLimit));

            return new JsonResult(EncryptedResponse.Generate(iv, upgradeCharacterResponse));
        }

        [HttpPost]
        [Route("unlockedCharacter")]
        [Produces("text/json")]
        public JsonResult UnlockedCharacter([FromForm] string param, [FromForm] string secure, [FromForm] string key = "")
        {
            var iv = (string)Config.Get("encryption_iv");

            using var conn = Db.Get();
            conn.Open();

            var clientReq = new ClientRequest<UnlockedCharacterRequest>(conn, param, secure, key);
            if (clientReq.error != SRStatusCode.Ok)
            {
                return new JsonResult(EncryptedResponse.Generate(iv, clientReq.error));
            }

            var characterID = clientReq.request.characterId;
            var currency = clientReq.request.itemId;

            CharacterResponse unlockedCharacterResponse = new();

            PlayerState playerState = new();
            Character[] characterState;

            // Get the player's CharacterState from the db
            var populateCharacterStatus = Character.PopulateCharacterState(conn, clientReq.userId, out characterState);

            if (populateCharacterStatus != SRStatusCode.Ok)
            {
                // Return error code from PopulateCharacterState() to client
                return new JsonResult(EncryptedResponse.Generate(iv, new BaseResponse(populateCharacterStatus)));
            }

            conn.Open();

            // Get the player's PlayerState from the db
            var populatePlayerStatus = playerState.Populate(conn, clientReq.userId);

            if (populatePlayerStatus != SRStatusCode.Ok)
            {
                // Return error code from PopulateCharacterState() to client
                return new JsonResult(EncryptedResponse.Generate(iv, new BaseResponse(populatePlayerStatus)));
            }

            // Now we need to find the index of the provided character in the CharacterState
            var index = -1;
            for (var i = 0; i < characterState.Length; i++)
            {
                if (characterState[i].characterId != characterID) continue;
                index = i;
                break;
            }

            if (index == -1)
            {
                // The character the client wants to purchase isn't available to the player, abort
                return new JsonResult(EncryptedResponse.Generate(iv, SRStatusCode.InternalServerError));
            }

            if (currency == (int)Item.ItemID.RedStarRing)
            {
                if (characterState[index].priceNumRedRings > playerState.numRedRings)
                {
                    return new JsonResult(EncryptedResponse.Generate(iv, SRStatusCode.NotEnoughRedStarRings));
                }
                playerState.numRedRings -= characterState[index].priceNumRedRings;

                characterState[index].priceNumRedRings += 10;
                characterState[index].priceNumRings += 100000;

                switch (characterState[index].status)
                {
                    // Character not unlocked yet, unlock it
                    case (sbyte)Character.Status.Locked:
                        characterState[index].status = (sbyte)Character.Status.Unlocked;
                        break;
                    // Character already unlocked, limit smash
                    case (sbyte)Character.Status.Unlocked:
                        if (characterState[index].star < characterState[index].starMax)
                        {
                            characterState[index].star++;
                            break;
                        }
                        // Character is already fully limit smashed, this should never happen
                        DebugHelper.Log("user " + clientReq.userId + " tried to limit smash character ID " + characterState[index].characterId + " even though it's at max stars! this could indicate bad CharacterState data or a desync!", 3);
                        return new JsonResult(EncryptedResponse.Generate(iv, SRStatusCode.CharacterLevelLimit));
                }
            }
            else if (currency == (int)Item.ItemID.Ring)
            {
                if (characterState[index].priceNumRings > playerState.numRings)
                {
                    return new JsonResult(EncryptedResponse.Generate(iv, SRStatusCode.NotEnoughRings));
                }
                playerState.numRings -= characterState[index].priceNumRings;

                characterState[index].priceNumRings += 100000;
                characterState[index].priceNumRedRings += 10;

                switch (characterState[index].status)
                {
                    // Character not unlocked yet, unlock it
                    case (sbyte)Character.Status.Locked:
                        characterState[index].status = (sbyte)Character.Status.Unlocked;
                        break;
                    // Character already unlocked, limit smash
                    case (sbyte)Character.Status.Unlocked:
                        if (characterState[index].star < characterState[index].starMax)
                        {
                            characterState[index].star++;
                            break;
                        }
                        // Character is already fully limit smashed, this should never happen
                        DebugHelper.Log("user " + clientReq.userId + " tried to limit smash character ID " + characterState[index].characterId + " even though it's at max stars! this could indicate bad CharacterState data or a desync!", 3);
                        return new JsonResult(EncryptedResponse.Generate(iv, SRStatusCode.CharacterLevelLimit));
                }
            }

            Character.SaveCharacterState(conn, clientReq.userId, characterState);
            playerState.Save(conn, clientReq.userId);

            unlockedCharacterResponse.playerState = playerState;
            unlockedCharacterResponse.characterState = characterState;

            return new JsonResult(EncryptedResponse.Generate(iv, unlockedCharacterResponse));
        }

        [HttpPost]
        [Route("changeCharacter")]
        [Produces("text/json")]
        public JsonResult ChangeCharacter([FromForm] string param, [FromForm] string secure, [FromForm] string key = "")
        {
            var iv = (string)Config.Get("encryption_iv");

            using var conn = Db.Get();
            conn.Open();

            var clientReq = new ClientRequest<ChangeCharacterRequest>(conn, param, secure, key);
            if (clientReq.error != SRStatusCode.Ok)
            {
                return new JsonResult(EncryptedResponse.Generate(iv, clientReq.error));
            }

            var mainCharacterID = clientReq.request.mainCharacterId;
            var subCharacterID = clientReq.request.subCharacterId;

            ChangeCharacterResponse changeCharacterResponse = new();

            PlayerState playerState = new();

            // Get the player's PlayerState from the db
            var populateStatus = playerState.Populate(conn, clientReq.userId);

            if (populateStatus != SRStatusCode.Ok)
            {
                // Return error code from Populate() to client
                return new JsonResult(EncryptedResponse.Generate(iv, new BaseResponse(populateStatus)));
            }

            playerState.mainCharaID = mainCharacterID;
            playerState.subCharaID = subCharacterID;

            var saveStatus = playerState.Save(conn, clientReq.userId);

            if (saveStatus != SRStatusCode.Ok)
            {
                // Return error code from Save() to client
                return new JsonResult(EncryptedResponse.Generate(iv, new BaseResponse(saveStatus)));
            }

            changeCharacterResponse.playerState = playerState;

            return new JsonResult(EncryptedResponse.Generate(iv, changeCharacterResponse));
        }
    }
}
