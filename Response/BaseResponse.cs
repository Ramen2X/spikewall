using System;

namespace spikewall.Response
{
    /// <summary>
    /// Base response class from which all other responses are derived from.
    /// </summary>
    public class BaseResponse
    {
        BaseInfo m_bi;
        string m_assetsVersion;
        string m_clientDataVersion;
        string m_dataVersion;
        string m_infoVersion;
        string m_version;
        string m_spikewallVersion;

        /// <summary>
        /// The constructors for a BaseResponse. If no parameters are specified, the default will be used.
        /// </summary>
        /// <param name="bi">The BaseInfo object.</param>
        /// <param name="av">The assets version.</param>
        /// <param name="cdv">The client data version.</param>
        /// <param name="dv">The data version.</param>
        /// <param name="iv">The info version.</param>
        /// <param name="v">The version.</param>
        /// <param name="swv">The spikewall version.</param>
        public BaseResponse(BaseInfo bi, string av, string cdv, string dv, string iv, string v, string swv)
        {
            m_bi = bi;
            m_assetsVersion = av;
            m_clientDataVersion = cdv;
            m_dataVersion = dv;
            m_infoVersion = iv;
            m_version = v;
            m_spikewallVersion = swv;
        }

        public BaseResponse()
        {
            m_bi = new BaseInfo();
            m_assetsVersion = "054";
            m_clientDataVersion = "2.2.2";
            m_dataVersion = "15";
            m_infoVersion = "017";
            m_version = "2.2.2";
            m_spikewallVersion = "0.0.1";
        }
    }
}