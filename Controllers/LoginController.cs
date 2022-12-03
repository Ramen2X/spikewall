using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System.Security.Cryptography;
using System.Text;
using spikewall.Response;
using spikewall.Request;
using spikewall.Object;

namespace spikewall.Controllers
{
    [ApiController]
    [Route("login")]
    public class LoginController : ControllerBase
    {
        [HttpPost]
        [Route("Login")]
        [Produces("text/json")]
        public JsonResult Login([FromForm] string param, [FromForm] string secure, [FromForm] string key = "")
        {
            using var conn = Db.Get();

            conn.Open();

            string iv = (string) Config.Get("encryption_iv");
            var clientReq = new ClientRequest<LoginRequest>(conn, param, secure, key, true);
            if (clientReq.error != SRStatusCode.Ok) {
                return new JsonResult(EncryptedResponse.Generate(iv, clientReq.error));
            }

            // Determine this login time
            var loginTime = DateTimeOffset.Now.ToUnixTimeSeconds();

            var loginRequest = clientReq.request;
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

                return new JsonResult(EncryptedResponse.Generate(iv, newUserResponse));
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
                var errResponse = new BaseResponse(SRStatusCode.MissingPlayer);
                return new JsonResult(EncryptedResponse.Generate(iv, errResponse));
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
                return new JsonResult(EncryptedResponse.Generate(iv, keyResponse));
            }

            // Hash our password to match the one sent by the client
            byte[] theirHashedPass = Convert.FromHexString(loginRequest.lineAuth.password);
            byte[] ourHashedPass;
            using (MD5 md5 = MD5.Create())
            {
                string salted = string.Format("{0}:dho5v5yy7n2uswa5iblb:{1}:{2}", serverKey, uid, password);
                ourHashedPass = md5.ComputeHash(System.Text.Encoding.ASCII.GetBytes(salted));
            }

            if (!theirHashedPass.SequenceEqual(ourHashedPass)) {
                // Password is incorrect
                var errResponse = new BaseResponse(SRStatusCode.PassWordError);
                return new JsonResult(EncryptedResponse.Generate(iv, errResponse));
            }

            // Successful login
            // Generate random session ID
            var sid = GenerateRandomPassword(48);

            var expiryTime = loginTime + Convert.ToInt64(Config.Get("session_time"));

            // Generate SQL to store session
            string insertSessionSql = Db.GetCommand(
                @"INSERT INTO `sw_sessions` (
                    sid,
                    uid,
                    expiry
                ) VALUES ('{0}', '{1}', '{2}')",
                sid,
                uid,
                expiryTime
            );

            string removeStaleSessionsSql = Db.GetCommand(
                @"DELETE FROM `sw_sessions` WHERE expiry < '{0}';",
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
            loginResponse.sessionTimeLimit = expiryTime;
            loginResponse.energyRecveryTime = 360;             // FIXME: Hardcoded, 6 minutes
            loginResponse.energyRecoveryMax = 5;           // FIXME: Hardcoded
            loginResponse.inviteBasicIncentiv.itemId = 900000; // FIXME: Hardcoded
            loginResponse.inviteBasicIncentiv.numItem = 5;     // FIXME: Hardcoded

            // Encrypt and containerize
            return new JsonResult(EncryptedResponse.Generate(iv, loginResponse));
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

        [HttpPost]
        [Route("getVariousParameter")]
        [Produces("text/json")]
        public JsonResult GetVariousParameter([FromForm] string param, [FromForm] string secure, [FromForm] string key = "")
        {
            string iv = (string) Config.Get("encryption_iv");

            using var conn = Db.Get();

            conn.Open();

            // I don't think we need any information from this request, but
            // we will deserialize anyway just in case we do in the future.
            var clientReq = new ClientRequest<BaseRequest>(conn, param, secure, key);
            if (clientReq.error != SRStatusCode.Ok) {
                return new JsonResult(EncryptedResponse.Generate(iv, clientReq.error));
            }

            conn.Close();

            var variousParameterResponse = new VariousParameterResponse();
            return new JsonResult(EncryptedResponse.Generate(iv, variousParameterResponse));
        }

        [HttpPost]
        [Route("getInformation")]
        [Produces("text/json")]
        public JsonResult GetInformation([FromForm] string param, [FromForm] string secure, [FromForm] string key = "")
        {
            string iv = (string) Config.Get("encryption_iv");

            using var conn = Db.Get();

            conn.Open();

            var clientReq = new ClientRequest<BaseRequest>(conn, param, secure, key);
            if (clientReq.error != SRStatusCode.Ok) {
                return new JsonResult(EncryptedResponse.Generate(iv, clientReq.error));
            }

            // Get user's language so we ensure they only get information in their language
            var sql = Db.GetCommand("SELECT language FROM sw_players WHERE id = '{0}'", clientReq.userId);
            var command = new MySqlCommand(sql, conn);
            var language = command.ExecuteScalar();

            // Get appropriate information
            sql = Db.GetCommand("SELECT *, (SELECT COUNT(*) FROM `sw_information`) AS row_count FROM `sw_information` WHERE language = '{0}' OR language = -1", language);
            command = new MySqlCommand(sql, conn);
            var reader = command.ExecuteReader();

            LoginInformationResponse informationResponse = new();

            if (reader.Read())
            {
                var count = reader.GetInt32("row_count");

                Information[] information = new Information[count];

                StringBuilder paramString = new();

                for (int i = 0; i < count; i++)
                {
                    information[i] = new Information();
                    information[i].id = reader.GetInt64("id");
                    information[i].priority = reader.GetSByte("priority");
                    information[i].start = reader.GetInt64("start_time");
                    information[i].end = reader.GetInt64("end_time");

                    paramString.Clear();

                    // StringBuilder.Append is apparently faster than
                    // AppendFormat, so this is what we're going with
                    paramString.Append(reader.GetInt16("display_type"));
                    paramString.Append('_');
                    paramString.Append(reader.GetString("message"));
                    paramString.Append('_');
                    paramString.Append(reader.GetString("image_id"));
                    paramString.Append('_');
                    paramString.Append(reader.GetInt16("info_type"));
                    paramString.Append('_');
                    paramString.Append(reader.GetString("extra"));

                    information[i].param = paramString.ToString();

                    reader.Read();
                }
                informationResponse.informations = information;
            }
            else
            {
                informationResponse.informations = Array.Empty<Information>();
            }

            // No idea what these are for right now, so we'll just return nothing
            informationResponse.operatorEachInfos = Array.Empty<OperatorInformation>();
            informationResponse.numOperatorInfo = 0;

            return new JsonResult(EncryptedResponse.Generate(iv, informationResponse));
        }

        [HttpPost]
        [Route("getTicker")]
        [Produces("text/json")]
        public JsonResult GetTicker([FromForm] string param, [FromForm] string secure, [FromForm] string key = "")
        {
            var iv = (string)Config.Get("encryption_iv");

            using var conn = Db.Get();

            conn.Open();

            var clientReq = new ClientRequest<BaseRequest>(conn, param, secure, key);
            if (clientReq.error != SRStatusCode.Ok) {
                return new JsonResult(EncryptedResponse.Generate(iv, clientReq.error));
            }

            // Get user's language so we ensure they only get tickers in their language
            var sql = Db.GetCommand("SELECT language FROM sw_players WHERE id = '{0}'", clientReq.userId);
            var command = new MySqlCommand(sql, conn);
            var language = command.ExecuteScalar();

            // Get appropriate tickers
            sql = Db.GetCommand("SELECT *, (SELECT COUNT(*) FROM `sw_tickers`) AS row_count FROM `sw_tickers` WHERE language = '{0}'", language);
            command = new MySqlCommand(sql, conn);
            var reader = command.ExecuteReader();

            LoginGetTickerResponse tickerResponse = new();

            if (reader.Read())
            {
                var count = reader.GetInt32("row_count");

                Ticker[] tickers = new Ticker[count];

                for (int i = 0; i < count; i++)
                {
                    tickers[i] = new Ticker
                    {
                        id = reader.GetByte("id"),
                        start = reader.GetInt64("start_time"),
                        end = reader.GetInt64("end_time"),
                        param = reader.GetString("message")
                    };

                    reader.Read();
                }
                tickerResponse.tickerList = tickers;
            }
            else tickerResponse.tickerList = Array.Empty<Ticker>();

            return new JsonResult(EncryptedResponse.Generate(iv, tickerResponse));
        }

        [HttpPost]
        [Route("loginBonus")]
        [Produces("text/json")]
        public JsonResult LoginBonus([FromForm] string param, [FromForm] string secure, [FromForm] string key = "")
        {
            var iv = (string)Config.Get("encryption_iv");

            using var conn = Db.Get();

            conn.Open();

            // I don't think we need any information from this request, but
            // we will deserialize anyway just in case we do in the future.
            var clientReq = new ClientRequest<BaseRequest>(conn, param, secure, key);
            if (clientReq.error != SRStatusCode.Ok)
            {
                return new JsonResult(EncryptedResponse.Generate(iv, clientReq.error));
            }

            // FIXME: Stub

            return new JsonResult(EncryptedResponse.Generate(iv, new LoginBonusResponse()));
        }

        [HttpPost]
        [Route("loginBonusSelect")]
        [Produces("text/json")]
        public JsonResult LoginBonusSelect([FromForm] string param, [FromForm] string secure, [FromForm] string key = "")
        {
            var iv = (string)Config.Get("encryption_iv");

            using var conn = Db.Get();

            conn.Open();

            // I don't think we need any information from this request, but
            // we will deserialize anyway just in case we do in the future.
            var clientReq = new ClientRequest<BaseRequest>(conn, param, secure, key);
            if (clientReq.error != SRStatusCode.Ok)
            {
                return new JsonResult(EncryptedResponse.Generate(iv, clientReq.error));
            }

            // FIXME: Stub

            return new JsonResult(EncryptedResponse.Generate(iv, new LoginBonusSelectResponse()));
        }
    }
}
