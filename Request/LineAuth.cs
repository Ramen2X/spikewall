namespace spikewall.Request
{
    /// <summary>
    /// Class to store standard authentication data frequently sent by the client.
    /// </summary>
    public class LineAuth
    {
        public string? userId { get; set; }
        public string? password { get; set; }
        public string? migrationPassword { get; set; }
    }
}
