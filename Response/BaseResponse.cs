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
        public Int64 data_version { get; set; }
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
        public BaseResponse(string av, string cdv, Int64 dv, string iv, string v, string swv)
        {
            assets_version = av;
            client_data_version = cdv;
            data_version = dv;
            info_version = iv;
            version = v;
            spikewall_version = swv;
        }

        public BaseResponse()
        {
            assets_version = (string) Config.Get("assets_version");
            client_data_version = (string) Config.Get("client_version");
            data_version = (Int64) Config.Get("data_version");
            info_version = (string) Config.Get("info_version");

            // This and client_data_version are pretty much
            // never different, so this makes sense for now
            version = (string) Config.Get("client_version");

            // This doesn't make much sense to get from the config
            // considering that spikewall's version would
            // only change with a recompilation anyway.
            spikewall_version = "0.0.1";
        }
    }
}