namespace spikewall.Debug
{
    public static class DebugHelper
    {
        public static void Log(string text, int type = 0)
        {
            switch (type) {
                case (1):
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.WriteLine("[Game] " + text);
                    Console.ResetColor();
                    break;
                case (2):
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("![Warn]! " + text);
                    Console.ResetColor();
                    break;
                default:
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("[Server] " + text);
                    Console.ResetColor();
                    break;
            }
        }
    }
}