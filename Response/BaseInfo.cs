namespace spikewall.Response
{
    public class BaseInfo
    {
        public string m_errorMessage;
        public string m_closeTime;
        public string m_seq;
        public string m_serverTime;
        public string m_statusCode;

        /// <summary>
        /// The constructors for BaseInfo. If no parameters are specified, the default will be used.
        /// </summary>
        /// <param name="em">The error message.</param>
        /// <param name="ct">The close time.</param>
        /// <param name="s">The "seq". (honestly I have no idea what this is)</param>
        /// <param name="sc">The status code.</param>
        public BaseInfo(string em, string ct, string s, string sc)
        {
            m_errorMessage = em;
            m_closeTime = ct;
            m_seq = s;
            m_serverTime = DateTimeOffset.Now.ToUnixTimeSeconds().ToString();
            m_statusCode = sc;
        }

        public BaseInfo()
        {
            m_errorMessage = "OK";
            m_closeTime = DateTime.Now.AddTicks(-1).AddDays(1).ToString();
            m_seq = "0";
            m_serverTime = DateTimeOffset.Now.ToUnixTimeSeconds().ToString();
            m_statusCode = "0";
        }
    }
}
