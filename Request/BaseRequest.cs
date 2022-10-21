namespace spikewall.Request
{
    /// <summary>
    /// Class from which requests are derived from.
    /// </summary>
    public class BaseRequest
    {
        public string SessionID { get; set; }
        public string VersionID { get; set; }
        public string RevivalVerID { get; set; }
        public string Seq { get; set; }
    }
}
