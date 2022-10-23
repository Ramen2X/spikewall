namespace spikewall.Response
{
    public class BaseInfo
    {
        public string errorMessage { get; set; }
        public Int64 closeTime { get; set; }
        public string seq { get; set; }
        public Int64 server_time { get; set; }
        public int statusCode { get; set; }

        /// <summary>
        /// The constructors for BaseInfo. If no parameters are specified, the default will be used.
        /// </summary>
        /// <param name="em">The error message.</param>
        /// <param name="ct">The close time.</param>
        /// <param name="s">The "seq". (honestly I have no idea what this is)</param>
        /// <param name="sc">The status code.</param>
        public BaseInfo(string em, long ct, string s, int sc)
        {
            errorMessage = em;
            closeTime = ct;
            seq = s;
            server_time = DateTimeOffset.Now.ToUnixTimeSeconds();
            statusCode = sc;
        }

        public BaseInfo()
        {
            errorMessage = "OK";
            closeTime = DateTimeOffset.Now.AddTicks(-1).AddDays(1).ToUnixTimeSeconds();
            seq = "0";
            server_time = DateTimeOffset.Now.ToUnixTimeSeconds();
            statusCode = -19997;
        }
    }
}
