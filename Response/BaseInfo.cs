namespace spikewall.Response
{
    public class BaseInfo
    {
        public string errorMessage { get; set; }
        public string closeTime { get; set; }
        public string seq { get; set; }
        public string server_time { get; set; }
        public string statusCode { get; set; }

        /// <summary>
        /// The constructors for BaseInfo. If no parameters are specified, the default will be used.
        /// </summary>
        /// <param name="em">The error message.</param>
        /// <param name="ct">The close time.</param>
        /// <param name="s">The "seq". (honestly I have no idea what this is)</param>
        /// <param name="sc">The status code.</param>
        public BaseInfo(string em, string ct, string s, string sc)
        {
            errorMessage = em;
            closeTime = ct;
            seq = s;
            server_time = DateTimeOffset.Now.ToUnixTimeSeconds().ToString();
            statusCode = sc;
        }

        public BaseInfo()
        {
            errorMessage = "OK";
            closeTime = DateTime.Now.AddTicks(-1).AddDays(1).ToString();
            seq = "0";
            server_time = DateTimeOffset.Now.ToUnixTimeSeconds().ToString();
            statusCode = "0";
        }
    }
}
