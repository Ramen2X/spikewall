namespace spikewall.Request
{
    /// <summary>
    /// Class to store standard authentication data frequently sent by the client.
    /// </summary>
    public class LineAuth
    {
        public string UserId { get; set; }
        public string Password { get; set; }
        public string MigrationPassword { get; set; }
    }
}
