using spikewall.Encryption;
using System.Text.Json;

namespace spikewall.Response
{
    /// <summary>
    /// Class which takes the final serialized and encrypted param and wraps it
    /// in another JSON object.
    /// A serialized version of this is the final thing the game client receives.
    /// </summary>
    public class EncryptedResponse
    {
        public string key { get; set; }
        public string param { get; set; }
        public string secure { get; set; }

        public EncryptedResponse(string k, string p, string s)
        {
            key = k;
            param = p;
            secure = s;
        }

        public static EncryptedResponse Generate(string key, object input)
        {
            return new EncryptedResponse(key, EncryptionHelper.Encrypt(JsonSerializer.Serialize(input), key), "1");
        }

        public static EncryptedResponse Generate(string key, SRStatusCode statusCode)
        {
            return new EncryptedResponse(key, EncryptionHelper.Encrypt(JsonSerializer.Serialize(new BaseResponse(statusCode)), key), "1");
        }
    }
}
