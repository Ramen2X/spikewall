using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
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

            UpgradeCharacterResponse upgradeCharacterResponse = new();

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
            int index = -1;
            for (int i = 0; i < characterState.Length; i++)
            {
                if (characterState[i].characterId == characterID)
                {
                    index = i;
                    break;
                }
            }

            if (index == -1)
            {
                // The character the client wants us to upgrade isn't available to the player, abort
                return new JsonResult(EncryptedResponse.Generate(iv, SRStatusCode.InternalServerError));
            }


            ulong? ringCost = characterState[index].numRings - characterState[index].exp;

            conn.Open();

            if (characterState[index].level < 100)
            {
                // Honestly unsure of the signifiance of 120000 here, but it works
                int abilityIndex = abilityID - 120000;

                characterState[index].level++;
                characterState[index].abilityLevel[abilityIndex]++;
                characterState[index].exp = 0;

                // Recalculate the multiple
                var sql = Db.GetCommand("SELECT multiple FROM `sw_characterupgrades` WHERE character_id = '{0}' AND min_level <= '{1}' AND max_level >= '{1}';", characterID, characterState[index].level);
                var upgrdCmd = new MySqlCommand(sql, conn);

                ulong multiple = Convert.ToUInt64(upgrdCmd.ExecuteScalar());
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
    }
}
