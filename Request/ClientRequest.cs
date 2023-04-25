using MySqlConnector;
using spikewall.Encryption;
using spikewall.Response;
using System.Text.Json;

namespace spikewall.Request
{
    public class ClientRequest<T> where T : BaseRequest
    {
        public T request { get; set; }
        public string userId { get; set; }
        public SRStatusCode error { get; set; }

        public ClientRequest(MySqlConnection conn, string param, string secure, string key, bool ignore_session = false)
        {
            string paramJSON = param;

            // The secure parameter is sent by the client to indicate if its param is encrypted.
            if (secure.Equals("1")) {
                paramJSON = EncryptionHelper.Decrypt(paramJSON, key);
            }

            T deserial = JsonSerializer.Deserialize<T>(paramJSON);

            this.request = deserial;
            this.error = SRStatusCode.Ok;

            if (!ignore_session) {
                var sql = Db.GetCommand("SELECT expiry FROM `sw_sessions` WHERE sid = '{0}';", deserial.sessionId);
                var command = new MySqlCommand(sql, conn);

                var expiry = Convert.ToInt64(command.ExecuteScalar());

                if (expiry < DateTimeOffset.Now.ToUnixTimeSeconds()) {
                    // Session has expired, remove it from the table
                    sql = Db.GetCommand("DELETE FROM `sw_sessions` WHERE sid = '{0}';", deserial.sessionId);

                    this.error = SRStatusCode.ExpirationSession;
                } else {
                    sql = Db.GetCommand("SELECT uid FROM `sw_sessions` WHERE sid = '{0}';", deserial.sessionId);
                    command = new MySqlCommand(sql, conn);

                    object uidObj = command.ExecuteScalar();
                    if (uidObj == null) {
                        this.error = SRStatusCode.MissingPlayer;
                    } else {
                        this.userId = uidObj.ToString();
                    }
                }
            }
        }
    }
}
