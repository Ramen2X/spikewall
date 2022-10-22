using System;

namespace spikewall.Response
{
    /// <summary>
    /// Base response class from which all other responses are derived from.
    /// </summary>
    public class BaseResponse : BaseInfo
    {
        public string assets_version { get; set; }
        public string client_data_version { get; set; }
        public string data_version { get; set; }
        public string info_version { get; set; }
        public string version { get; set; }
        public string spikewall_version { get; set; }

        /// <summary>
        /// The constructors for a BaseResponse. If no parameters are specified, the default will be used.
        /// </summary>
        /// <param name="av">The assets version.</param>
        /// <param name="cdv">The client data version.</param>
        /// <param name="dv">The data version.</param>
        /// <param name="iv">The info version.</param>
        /// <param name="v">The version.</param>
        /// <param name="swv">The spikewall version.</param>
        public BaseResponse(string av, string cdv, string dv, string iv, string v, string swv)
        {
            assets_version = av;
            client_data_version = cdv;
            data_version = dv;
            info_version = iv;
            version = v;
            spikewall_version = swv;
        }

        // TODO: Get these defaults from the config
        public BaseResponse()
        {
            assets_version = "054";
            client_data_version = "2.2.2";
            data_version = "15";
            info_version = "017";
            version = "2.2.2";
            spikewall_version = "0.0.1";
        }
    }
}