namespace spikewall.Request
{
    /// <summary>
    /// Class to store data from a login request.
    /// </summary>
    public class LoginRequest : BaseRequest
    {
        public string Device { get; set; }
        public string Platform { get; set; }
        public string Language { get; set; }
        public string SalesLocate { get; set; }
        public string StoreId { get; set; }
        public string Platform_sns { get; set; }
    }
}
