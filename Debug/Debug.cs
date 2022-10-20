namespace spikewall.Debug
{
    public class Debug
    {
        public static void Log(string text, int type = 0)
        {
            switch (type)
            {
                case (1):
                    Console.WriteLine("[Game] " + text);
                    break;
                case (2):
                    Console.WriteLine("![Warn]! " + text);
                    break;
                default:
                    Console.WriteLine("[Server] " + text);
                    break;
            }
        }
    }
}