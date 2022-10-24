using Microsoft.AspNetCore.Mvc;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using spikewall.Debug;
using spikewall.Encryption;
using spikewall.Response;
using spikewall.Request;

namespace spikewall.Controllers
{
    [ApiController]
    [Route("/login/Login/")]
    [Produces("text/json")]
    public class LoginController : ControllerBase
    {
        [HttpPost]
        public JsonResult Login([FromForm] string param, [FromForm] string secure, [FromForm] string key = "")
        {
            string paramJSON = param;

            // The secure parameter is sent by the client to indicate if its param is encrypted.
            if (secure.Equals("1")) {
                paramJSON = EncryptionHelper.Decrypt(paramJSON, key);
            }

            LoginRequest? loginRequest = JsonSerializer.Deserialize<LoginRequest>(paramJSON);

            using var conn = Db.Get();

            conn.Open();

            // Determine this login time
            var loginTime = DateTimeOffset.Now.ToUnixTimeSeconds();

            var uid = loginRequest.lineAuth.userId;
            string sql;
            MySqlCommand command;

            // Determine whether this is a new user or returning user (userId is always '0' for new user)
            if (uid == "0")
            {
                string pass = GenerateRandomPassword(20);
                string keypass = GenerateRandomPassword(20);

                sql = Db.GetCommand(
                    @"INSERT INTO `sw_players` (
                        password,
                        server_key,
                        last_login,
                        last_login_device,
                        last_login_platform,
                        last_login_version,
                        language
                    )
                    VALUES ('{0}','{1}','{2}','{3}','{4}','{5}','{6}');
                    SELECT LAST_INSERT_ID();",
                    pass,
                    keypass,
                    loginTime,
                    loginRequest.device,
                    loginRequest.platform,
                    loginRequest.version,
                    loginRequest.language
                );

                command = new MySqlCommand(sql, conn);

                // Command will return last inserted ID
                uid = command.ExecuteScalar().ToString();

                // NOTE: US country code hardcoded - in the future we may want to set this
                //       correctly, but at this time I'm not sure what use we would have for it
                //       outside of geoblocking (which is cringe).
                var newUserResponse = new NewUserResponse(uid, pass, keypass, "1", "US");

                return new JsonResult(EncryptedResponse.Generate(key, newUserResponse));
            }

            // Retrieve user info matching the provided ID
            sql = Db.GetCommand(
                @"SELECT username, password, server_key FROM `sw_players` WHERE `id` = '{0}';",
                uid
            );

            // Execute query
            command = new MySqlCommand(sql, conn);
            var reader = command.ExecuteReader();

            // Determine if there's a row to read (there should be 1 if the user exists, or 0 if not)
            if (!reader.HasRows) {
                // No user with this ID exists in the database
                var errResponse = new BaseResponse();
                errResponse.statusCode = BaseResponse.SRStatusCode.MissingPlayer;
                return new JsonResult(EncryptedResponse.Generate(key, errResponse));
            }

            // Read user data from the database
            reader.Read();
            string username = reader.GetString("username");
            string serverKey = reader.GetString("server_key");
            string password = reader.GetString("password");
            reader.Close();

            // Client may check server authenticity before sending the user password by asking for
            // the "key" we sent during initial registration. In this scenario it sends a normal
            // login request but with an empty password field, so we handle that here.
            if (string.IsNullOrEmpty(loginRequest.lineAuth.password)) {
                var keyResponse = new ServerKeyCheckResponse(serverKey);
                return new JsonResult(EncryptedResponse.Generate(key, keyResponse));
            }

            // Hash our password to match the one sent by the client
            byte[] theirHashPass = Convert.FromHexString(loginRequest.lineAuth.password);
            byte[] ourHashedPass;
            using (MD5 md5 = MD5.Create())
            {
                string salted = string.Format("{0}:dho5v5yy7n2uswa5iblb:{1}:{2}", serverKey, uid, password);
                ourHashedPass = md5.ComputeHash(System.Text.Encoding.ASCII.GetBytes(salted));
            }

            if (!theirHashPass.SequenceEqual(ourHashedPass)) {
                // Password is incorrect
                var errResponse = new BaseResponse();
                errResponse.statusCode = BaseResponse.SRStatusCode.PassWordError;
                return new JsonResult(EncryptedResponse.Generate(key, errResponse));
            }

            // Successful login
            // Generate random session ID
            var sid = GenerateRandomPassword(48);

            // Generate SQL to store session
            string insertSessionSql = Db.GetCommand(
                @"INSERT INTO `sw_sessions` (
                    sid,
                    uid,
                    time
                ) VALUES ('{0}', '{1}', '{2}')",
                sid,
                uid,
                loginTime
            );

            // Generate SQL to update player's last login info
            string updatePlayerSql = Db.GetCommand(
                @"UPDATE `sw_players`
                SET
                    last_login = '{0}',
                    last_login_device = '{1}',
                    last_login_platform = '{2}',
                    last_login_version = '{3}',
                    language = '{4}'
                WHERE `id` = '{5}'",
                loginTime,
                loginRequest.device,
                loginRequest.platform,
                loginRequest.version,
                loginRequest.language,
                uid
            );

            // Execute both commands at once
            command = new MySqlCommand(string.Format("{0};{1};", insertSessionSql, updatePlayerSql), conn);
            command.ExecuteNonQuery();

            // Set up response
            var loginResponse = new LoginResponse();
            loginResponse.userName = username;
            loginResponse.sessionId = sid;
            loginResponse.sessionTimeLimit = loginTime + Config.GetInt("session_time");
            loginResponse.energyRecveryTime = 360;             // FIXME: Hardcoded, 6 minutes
            loginResponse.energyRecoveryMax = 17171;           // FIXME: Hardcoded
            loginResponse.inviteBasicIncentiv.itemId = 900000; // FIXME: Hardcoded
            loginResponse.inviteBasicIncentiv.numItem = 5;     // FIXME: Hardcoded

            // Encrypt and containerize
            return new JsonResult(EncryptedResponse.Generate(key, loginResponse));
        }

        private string GenerateRandomPassword(int length)
        {
            var builder = new StringBuilder();
            for (int i = 0; i < length; i++) {
                // Generate a random number that corresponds to a valid character on the ASCII
                // chart, then append that character
                builder.Append((char) RandomNumberGenerator.GetInt32(0x61, 0x7F));
            }
            return builder.ToString();
        }
    }

    [ApiController]
    [Route("/login/getVariousParameter/")]
    [Produces("text/json")]
    public class GetVariousParameterController : ControllerBase
    {
        [HttpPost]
        public JsonResult GetVariousParameter([FromForm] string param, [FromForm] string secure, [FromForm] string key = "")
        {
            string paramJSON = param;

            // The secure parameter is sent by the client to indicate if its param is encrypted.
            if (secure.Equals("1")) {
                paramJSON = EncryptionHelper.Decrypt(paramJSON, key);
            }

            // I don't think we need any information from this request, but
            // we will deserialize anyway just in case we do in the future.
            BaseRequest? baseRequest = JsonSerializer.Deserialize<LoginRequest>(paramJSON);

            var variousParameterResponse = new VariousParameterResponse();
            return new JsonResult(EncryptedResponse.Generate(key, variousParameterResponse));
        }
    }
}
