using MySql.Data;
using MySql.Data.MySqlClient;
using spikewall.Encryption;
using spikewall.Response;
using System.Text.Json;

namespace spikewall.Request
{
    /// <summary>
    /// Class from which requests are derived from.
    /// </summary>
    public class BaseRequest
    {
        public string? sessionId { get; set; }
        public string? version { get; set; }
        public string? revivalVerId { get; set; }
        public string? seq { get; set; }

        public static T Retrieve<T>(string param, string secure, string key, out BaseResponse error, bool ignore_session = false) where T : BaseRequest
        {
            // Initialize error as null
            error = null;

            string paramJSON = param;

            // The secure parameter is sent by the client to indicate if its param is encrypted.
            if (secure.Equals("1")) {
                paramJSON = EncryptionHelper.Decrypt(paramJSON, key);
            }

            T deserial = JsonSerializer.Deserialize<T>(paramJSON);

            if (!ignore_session) {
                using (var conn = Db.Get())
                {
                    conn.Open();

                    var sql = Db.GetCommand("SELECT expiry FROM `sw_sessions` WHERE sid = '{0}';", deserial.sessionId);
                    var command = new MySqlCommand(sql, conn);

                    var expiry = Convert.ToInt64(command.ExecuteScalar());

                    if (expiry < DateTimeOffset.Now.ToUnixTimeSeconds()) {
                        // Session has expired, remove it from the table
                        sql = Db.GetCommand("DELETE FROM `sw_sessions` WHERE sid = '{0}';", deserial.sessionId);

                        error = new BaseResponse();
                        error.statusCode = BaseResponse.SRStatusCode.ExpirationSession;
                    }

                    conn.Close();
                }
            }

            return deserial;
        }
    }
}
